using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work Center
    /// </summary>
    public class Machine : WorkOrder
    {
        #region Properties

        public string MachineNumber { get; set; }
        public string MachineName { get; set; }
        public string MachineDescription { get; set; }
        new public string MachineGroup { get; set; }
        public bool IsLoaded { get; set; }

        #endregion

        /// <summary>
        /// Work Center Constructor
        /// </summary>
        public Machine()
        { }

        /// <summary>
        /// Get a list of work centers
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(SqlConnection sqlCon, bool incAll)
        {
            var _tempList = new List<Machine>();
            if (incAll)
            {
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "All", IsLoaded = true, MachineGroup = "All" });
            }
            _tempList.Add(new Machine { MachineNumber = "1", MachineName = "WG Order", IsLoaded = true, MachineGroup = "Custom" });
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Wc_Nbr], [Name], [D_esc], [Work_Ctr_Group] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new Machine
                                    {
                                        MachineNumber = reader.SafeGetString("Wc_Nbr"),
                                        MachineName = reader.SafeGetString("Name"),
                                        MachineDescription = reader.SafeGetString("D_esc"),
                                        MachineGroup = reader.SafeGetString("Work_Ctr_Group")
                                    });
                                }
                            }
                        }
                    }
                    return _tempList;
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon)
        {
            //Currently written with the old Q-Task module, will need to be replaced when the new Q-Task module is developed
            var _selectCmd = sqlCon.Database == "CSI_MAIN"
                ? @"SELECT
	                    DISTINCT(b.[ID]) as 'WO_Number',
                        CASE WHEN b.[Next_Seq] IS NULL AND b.[Prev_Seq] IS NULL
		                        THEN '10'
		                        ELSE SUBSTRING(b.[ID], CHARINDEX('*',b.[ID],0) + 1,LEN(b.[ID])) END as 'Operation',
                        a.[Wc_Nbr] as 'MachineNumber',
	                    a.[Name] as 'MachineName',
	                    a.[D_esc] as 'MachineDesc',
	                    a.[Work_Ctr_Group] as 'MachineGroup',
                        ISNULL(b.[Qty_Avail], b.[Qty_Req] - ISNULL(b.[Qty_Compl], 0)) as 'WO_CurrentQty',
	                    ISNULL(b.[Date_Start], '1999-01-01') as 'WO_SchedStartDate',
                        ISNULL(b.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate',
	                    ISNULL(b.[Due_Date], b.[Date_Start]) as 'WO_DueDate',
                        CAST(ROUND(b.[Mach_Load_Hrs_Rem], 1) as FLOAT) as 'RunTime',
	                    ISNULL(CASE WHEN
                                (SELECT
                                    [Ord_Type]
                                FROM
                                    [dbo].[SOH-INIT]
                                WHERE
                                    [So_Nbr] = SUBSTRING(c.[So_Reference],0,CHARINDEX('*',c.[So_Reference],0))) = 'DAI'
                            THEN 'A'
                            WHEN c.[Wo_Type] = 'R'
                            THEN 'B'
                            ELSE c.[Mgt_Priority_Code] END, 'D') as 'WO_Priority',
	                    ISNULL(c.[Wo_Type], 'S') as 'WO_Type',
	                    c.[Qty_To_Start] as 'WO_StartQty',
	                    c.[So_Reference] as 'WO_SalesRef',
                        c.[Cust_Nbr],
                        CASE WHEN c.[Time_Wanted] IS NOT NULL THEN CONVERT(VARCHAR(2), CAST(c.[Time_Wanted] as TIME),108) ELSE '999' END as 'PriTime',
                        CASE WHEN c.[Time_Wanted] IS NOT NULL THEN DATEPART(MINUTE, CAST(c.[Time_Wanted] as TIME)) ELSE '999' END as 'Sched_Priority',
                        d.[Part_Number]as 'SkuNumber',
	                    d.[Description] as 'SkuDesc',
	                    d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                    ISNULL(c.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                    ISNULL(c.[Bom_Rev_Level], '') as 'BomRevLvl',
                        ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                        CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                    CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate',
                        e.[Engineering_Status] as 'EngStatus',
	                    (SELECT [Description] FROM [dbo].[TM-INIT_Eng_Status] WHERE [ID] = e.[Engineering_Status]) as 'EngStatusDesc',
                        (SELECT [Name] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]) as 'Cust_Name',
	                    (SELECT [Cust_Part_Nbr] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as 'Cust_Part_Nbr',
	                    CAST((SELECT [Ln_Bal_Qty] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as int) as 'Ln_Bal_Qty',
                        ISNULL((SELECT [Load_Pattern] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]),'') as 'LoadPattern',
                        0 as 'QTask'
                    FROM
                        [dbo].[WC-INIT] a
                    RIGHT JOIN
                        [dbo].[WPO-INIT] b ON b.[Work_Center] = a.[Wc_Nbr]
                    RIGHT JOIN
                        [dbo].[WP-INIT] c ON b.[ID] LIKE CONCAT(c.[Wp_Nbr], '%')
                    RIGHT JOIN
                        [dbo].[IM-INIT] d ON d.[Part_Number] = c.[Part_Wo_Desc]
                    RIGHT JOIN
                        [dbo].[IPL-INIT] e ON e.[Part_Nbr] = d.[Part_Number]
                    WHERE
                        a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.[Status_Flag] = 'A') AND (b.[Seq_Complete_Flag] IS NULL OR b.[Seq_Complete_Flag] = 'N') AND b.[Alt_Seq_Status] IS NULL
                    ORDER BY
                        MachineNumber, WO_Priority, PriTime, Sched_Priority, WO_SchedStartDate, WO_Number ASC;"
                //WCCO's custom select string
                : @"SELECT
	                    DISTINCT(b.[ID]) as 'WO_Number',
                        CASE WHEN b.[Next_Seq] IS NULL AND b.[Prev_Seq] IS NULL
		                    THEN '10'
		                    ELSE SUBSTRING(b.[ID], CHARINDEX('*',b.[ID],0) + 1,LEN(b.[ID])) END as 'Operation',
                        a.[Wc_Nbr] as 'MachineNumber',
	                    a.[Name] as 'MachineName',
	                    a.[D_esc] as 'MachineDesc',
	                    a.[Work_Ctr_Group] as 'MachineGroup',
                        ISNULL(b.[Qty_Avail], b.[Qty_Req] - ISNULL(b.[Qty_Compl], 0)) as 'WO_CurrentQty',
	                    ISNULL(b.[Date_Start], '1999-01-01') as 'WO_SchedStartDate',
                        ISNULL(b.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate',
	                    ISNULL(b.[Due_Date], b.[Date_Start]) as 'WO_DueDate',
                        CAST(ROUND(b.[Mach_Load_Hrs_Rem], 1) as FLOAT) as 'RunTime',
	                    ISNULL(CASE WHEN
                                (SELECT
                                    [Ord_Type]
                                FROM
                                    [dbo].[SOH-INIT]
                                WHERE
                                    [So_Nbr] = SUBSTRING(c.[So_Reference],0,CHARINDEX('*',c.[So_Reference],0))) = 'DAI'
                            THEN 'A'
                            WHEN c.[Wo_Type] = 'R'
                            THEN 'B'
                            ELSE c.[Mgt_Priority_Code] END, 'D') as 'WO_Priority',
	                    ISNULL(c.[Wo_Type], 'S') as 'WO_Type',
	                    c.[Qty_To_Start] as 'WO_StartQty',
	                    c.[So_Reference] as 'WO_SalesRef',
                        c.[Cust_Nbr],
                        CASE WHEN c.[Time_Wanted] IS NOT NULL THEN CONVERT(VARCHAR(2), CAST(c.[Time_Wanted] as TIME),108) ELSE '999' END as 'PriTime',
                        CASE WHEN c.[Time_Wanted] IS NOT NULL THEN DATEPART(MINUTE, CAST(c.[Time_Wanted] as TIME)) ELSE '999' END as 'Sched_Priority',
                        d.[Part_Number]as 'SkuNumber',
	                    d.[Description] as 'SkuDesc',
	                    d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                    ISNULL(c.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                    ISNULL(c.[Bom_Rev_Level], '') as 'BomRevLvl',
                        ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                        CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                    CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate',
                        e.[Engineering_Status] as 'EngStatus',
	                    (SELECT [Description] FROM [dbo].[TM-INIT_Eng_Status] WHERE [ID] = e.[Engineering_Status]) as 'EngStatusDesc',
                        (SELECT [Name] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]) as 'Cust_Name',
	                    (SELECT [Cust_Part_Nbr] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as 'Cust_Part_Nbr',
	                    CAST((SELECT [Ln_Bal_Qty] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as int) as 'Ln_Bal_Qty',
                        ISNULL((SELECT [Load_Pattern] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]),'') as 'LoadPattern',
                        (SELECT COUNT([Qtask_Type]) FROM [dbo].[IM_UDEF-INIT_Quality_Tasks] WHERE [Qtask_Initiated_By] IS NOT NULL AND [Qtask_Release_Date] IS NULL AND [ID1] = e.[Part_Nbr]) as 'QTask'
                    FROM
                        [dbo].[WC-INIT] a
                    RIGHT JOIN
                        [dbo].[WPO-INIT] b ON b.[Work_Center] = a.[Wc_Nbr]
                    RIGHT JOIN
                        [dbo].[WP-INIT] c ON b.[ID] LIKE CONCAT(c.[Wp_Nbr], '%')
                    RIGHT JOIN
                        [dbo].[IM-INIT] d ON d.[Part_Number] = c.[Part_Wo_Desc]
                    RIGHT JOIN
                        [dbo].[IPL-INIT] e ON e.[Part_Nbr] = d.[Part_Number]
                    WHERE
                        a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.[Status_Flag] = 'A') AND (b.[Seq_Complete_Flag] IS NULL OR b.[Seq_Complete_Flag] = 'N') AND b.[Alt_Seq_Status] IS NULL
                    ORDER BY
                        MachineNumber, WO_Priority, PriTime, Sched_Priority, WO_SchedStartDate, WO_Number ASC;";

            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(_selectCmd, sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule for a single machine
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="machineNumber">Machine Number to load</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon, string machineNumber)
        {
            //TODO: See the note from the previous method
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT
	                                                                            DISTINCT(b.[ID]) as 'WO_Number', 
                                                                                a.[Wc_Nbr] as 'MachineNumber',
	                                                                            a.[Name] as 'MachineName',
	                                                                            a.[D_esc] as 'MachineDesc',
	                                                                            a.[Work_Ctr_Group] as 'MachineGroup',
                                                                                ISNULL(b.[Qty_Avail], b.[Qty_Req] - ISNULL(b.[Qty_Compl], 0)) as 'WO_CurrentQty',
	                                                                            ISNULL(b.[Date_Start], '1999-01-01') as 'WO_SchedStartDate',
                                                                                ISNULL(b.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate',
	                                                                            ISNULL(b.[Due_Date], b.[Date_Start]) as 'WO_DueDate',
                                                                                CAST(ROUND(b.[Mach_Load_Hrs_Rem], 1) AS FLOAT) as 'RunTime',
	                                                                            ISNULL(CASE WHEN
                                                                                        (SELECT
                                                                                            [Ord_Type]
                                                                                        FROM
                                                                                            [dbo].[SOH-INIT]
                                                                                        WHERE
                                                                                            [So_Nbr] = SUBSTRING(c.[So_Reference],0,CHARINDEX('*',c.[So_Reference],0))) = 'DAI'
                                                                                    THEN 'A'
                                                                                    WHEN c.[Wo_Type] = 'R'
                                                                                    THEN 'B'
                                                                                    ELSE c.[Mgt_Priority_Code] END, 'D') as 'WO_Priority',
	                                                                            ISNULL(c.[Wo_Type], 'S') as 'WO_Type',
	                                                                            c.[Qty_To_Start] as 'WO_StartQty',
	                                                                            c.[So_Reference] as 'WO_SalesRef',
	                                                                            c.[Cust_Nbr],
                                                                                CASE WHEN c.[Time_Wanted] IS NOT NULL THEN CONVERT(VARCHAR(5), CAST(c.[Time_Wanted] as TIME),108) ELSE '9' END as 'PriTime',
                                                                                d.[Part_Number]as 'SkuNumber',
	                                                                            d.[Description] as 'SkuDesc',
	                                                                            d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                                                                            ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                                                                            ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                                                                                CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                                                                            CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate',
                                                                                e.[Engineering_Status] as 'EngStatus',
	                                                                            (SELECT [Description] FROM [dbo].[TM-INIT_Eng_Status] WHERE [ID] = e.[Engineering_Status]) as 'EngStatusDesc',
	                                                                            (SELECT [Name] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]) as 'Cust_Name',
	                                                                            (SELECT [Cust_Part_Nbr] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as 'Cust_Part_Nbr',
	                                                                            CAST((SELECT [Ln_Bal_Qty] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as int) as 'Ln_Bal_Qty'
                                                                            FROM
                                                                                [dbo].[WC-INIT] a
                                                                            RIGHT JOIN
                                                                                [dbo].[WPO-INIT] b ON b.[Work_Center] = a.[Wc_Nbr]
                                                                            RIGHT JOIN
                                                                                [dbo].[WP-INIT] c ON b.[ID] LIKE CONCAT(c.[Wp_Nbr], '%')
                                                                            RIGHT JOIN
                                                                                [dbo].[IM-INIT] d ON d.[Part_Number] = c.[Part_Wo_Desc]
                                                                            RIGHT JOIN
                                                                                [dbo].[IPL-INIT] e ON e.[Part_Nbr] = d.[Part_Number]
                                                                            WHERE
                                                                                a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.[Status_Flag] = 'A') AND (b.[Seq_Complete_Flag] IS NULL OR b.[Seq_Complete_Flag] = 'N') AND b.[Alt_Seq_Status] IS NULL AND (b.[Qty_Avail] > 0 OR b.[Qty_Avail] IS NULL) AND a.[Wc_Nbr] = @p1
                                                                            ORDER BY
                                                                                MachineNumber, WO_Priority, WO_SchedStartDate, WO_Number ASC;", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", machineNumber);
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw sqlEx;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a closed schedule
        /// </summary>
        /// /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetClosedScheduleData(SqlConnection sqlCon)
        {
            //TODO: Needs to be rewritten to include a list rather than a datatable so that in the future async loading can be done
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE [{sqlCon.Database}];
                                                                            SELECT
	                                                                            DISTINCT(b.[ID]) as 'WO_Number',
                                                                                CASE WHEN b.[Next_Seq] IS NULL AND b.[Prev_Seq] IS NULL
		                                                                                THEN '10'
		                                                                                ELSE SUBSTRING(b.[ID], CHARINDEX('*',b.[ID],0) + 1,LEN(b.[ID])) END as 'Operation',
                                                                                a.[Wc_Nbr] as 'MachineNumber',
	                                                                            a.[Name] as 'MachineName',
	                                                                            a.[D_esc] as 'MachineDesc',
	                                                                            a.[Work_Ctr_Group] as 'MachineGroup',
                                                                                ISNULL(b.[Qty_Avail], b.[Qty_Req] - ISNULL(b.[Qty_Compl], 0)) as 'WO_CurrentQty',
	                                                                            ISNULL(b.[Date_Start], '1999-01-01') as 'WO_SchedStartDate',
                                                                                ISNULL(b.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate',
	                                                                            ISNULL(b.[Due_Date], b.[Date_Start]) as 'WO_DueDate',
                                                                                CAST(ROUND(b.[Mach_Load_Hrs_Rem], 1) AS FLOAT) as 'RunTime',
	                                                                            ISNULL(CASE WHEN
                                                                                        (SELECT
                                                                                            [Ord_Type]
                                                                                        FROM
                                                                                            [dbo].[SOH-INIT]
                                                                                        WHERE
                                                                                            [So_Nbr] = SUBSTRING(c.[So_Reference],0,CHARINDEX('*',c.[So_Reference],0))) = 'DAI'
                                                                                    THEN 'A'
                                                                                    WHEN c.[Wo_Type] = 'R'
                                                                                    THEN 'B'
                                                                                    ELSE c.[Mgt_Priority_Code] END, 'D') as 'WO_Priority',
	                                                                            c.[Wo_Type] as 'WO_Type',
	                                                                            c.[Qty_To_Start] as 'WO_StartQty',
	                                                                            c.[So_Reference] as 'WO_SalesRef',
                                                                                c.[Cust_Nbr],
                                                                                d.[Part_Number]as 'SkuNumber',
	                                                                            d.[Description] as 'SkuDesc',
	                                                                            d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                                                                            ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                                                                            ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                                                                                CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                                                                            CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate',
                                                                                e.[Engineering_Status] as 'EngStatus',
	                                                                            (SELECT [Description] FROM [dbo].[TM-INIT_Eng_Status] WHERE [ID] = e.[Engineering_Status]) as 'EngStatusDesc',
                                                                                (SELECT [Name] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]) as 'Cust_Name',
	                                                                            (SELECT [Cust_Part_Nbr] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as 'Cust_Part_Nbr',
	                                                                            CAST((SELECT [Ln_Bal_Qty] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(c.[So_Reference],0,LEN(c.[So_Reference])-1)) as int) as 'Ln_Bal_Qty',
                                                                                ISNULL((SELECT [Load_Pattern] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = c.[Cust_Nbr]),'') as 'LoadPattern'
                                                                            FROM
                                                                                [dbo].[WC-INIT] a
                                                                            RIGHT JOIN
                                                                                [dbo].[WPO-INIT] b ON b.[Work_Center] = a.[Wc_Nbr]
                                                                            RIGHT JOIN
                                                                                [dbo].[WP-INIT] c ON b.[ID] LIKE CONCAT(c.[Wp_Nbr], '%')
                                                                            RIGHT JOIN
                                                                                [dbo].[IM-INIT] d ON d.[Part_Number] = c.[Part_Wo_Desc]
                                                                            RIGHT JOIN
                                                                                [dbo].[IPL-INIT] e ON e.[Part_Nbr] = d.[Part_Number]
                                                                            WHERE
                                                                                a.[D_esc] <> 'DO NOT USE' AND c.[Status_Flag] = 'C' AND b.[Seq_Complete_Flag] = 'C' AND b.[Alt_Seq_Status] IS NULL
                                                                            ORDER BY
                                                                                MachineNumber, WO_Priority, WO_SchedStartDate, WO_Number ASC;", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        /// <summary>
        /// Retrieve a List of strings of each of the groups assigned to the machines
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetMachineGroupList(SqlConnection sqlCon, bool incAll)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            _tempList.Add("Custom");
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT DISTINCT([Work_Ctr_Group]) as 'MachineGroup' FROM [dbo].[WC-INIT] WHERE [Name] <> 'DO NOT USE' AND [Work_Ctr_Group] IS NOT NULL;", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(reader.SafeGetString("MachineGroup"));
                                }
                            }
                        }
                    }
                    return _tempList;
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a machines display name
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="workOrder">Work order object</param>
        /// <returns>Machine Name as string</returns>
        public static string GetMachineName(SqlConnection sqlCon, WorkOrder workOrder)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Name] FROM [dbo].[WC-INIT] WHERE [Wc_Nbr] = (SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = CONCAT(@p1,'*',@p2));", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workOrder.OrderNumber);
                        cmd.Parameters.AddWithValue("p2", workOrder.Seq);
                        return cmd.ExecuteScalar().ToString();
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the machine group that a specific machine is a part of
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="machineNbr">Machine number to get the group of</param>
        /// <returns>machine group</returns>
        public static string GetMachineGroup(SqlConnection sqlCon, string machineNbr)
        {
            if(machineNbr == "0")
            {
                return "All";
            }
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Work_Ctr_Group] FROM [dbo].[WC-INIT] WHERE [Wc_Nbr] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", machineNbr);
                        return cmd.ExecuteScalar().ToString();
                    } 
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the machine group that a specific machine from a work order is contained in
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="workOrder">Work order object</param>
        /// <returns>Machine group as string</returns>
        public static string GetMachineGroup(SqlConnection sqlCon, WorkOrder workOrder)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Work_Ctr_Group] FROM [dbo].[WC-INIT] WHERE [Wc_Nbr] = (SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = CONCAT(@p1, '*', @p2));", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workOrder.OrderNumber);
                        cmd.Parameters.AddWithValue("p2", workOrder.Seq);
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the machine group that a specific machine is a part of
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="woNbr">Work Order number</param>
        /// <param name="seq">work order sequence</param>
        /// <returns>Machine group as string</returns>
        public static string GetMachineGroup(SqlConnection sqlCon, string woNbr, string seq)
        {
            if (!string.IsNullOrEmpty(woNbr) || !string.IsNullOrEmpty(seq))
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Work_Ctr_Group] FROM [dbo].[WC-INIT] WHERE [Wc_Nbr] = (SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = @p1);", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", $"{woNbr}*{seq}");
                            return cmd.ExecuteScalar()?.ToString();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw sqlEx;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="machineNbr">Press ID number</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(SqlConnection sqlCon, int machineNbr)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Press_Length] FROM [dbo].[WC-INIT] WHERE [Wc_Nbr] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", machineNbr);
                        var _len = cmd.ExecuteScalar();
                        return int.TryParse(_len.ToString(), out int i) ? i : 0;
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="machineName">Name of the press</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(SqlConnection sqlCon, string machineName)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Press_Length] FROM [dbo].[WC-INIT] WHERE [Name] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", machineName);
                        var _len = cmd.ExecuteScalar();
                        return int.TryParse(_len.ToString(), out int i) ? i : 0;
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
