using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="machOrder">Dictionary containing the order property for the machines, based on the user config</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(IReadOnlyDictionary<string, int> machOrder, SqlConnection sqlCon)
        {
            var _mOrder = string.Empty;
            if (machOrder?.Count > 0)
            {
                _mOrder = "CASE";
                foreach (var _mach in machOrder)
                {
                    _mOrder += $" WHEN wc.[Wc_Nbr] = '{_mach.Key}' THEN {_mach.Value}";
                }
                _mOrder += " ELSE 0 END";
            }
            else
            {
                _mOrder = "0";
            }
            var _conString = sqlCon.Database.Contains("WCCO") ?
                //WCCO Query
                $@"SELECT
	                DISTINCT(wpo.[ID]) as 'WorkOrderID'
	                ,SUBSTRING(wpo.[ID], 0, CHARINDEX('*', wpo.[ID], 0)) as 'WorkOrder'
	                ,SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID])) as 'Operation'
                    ,(SELECT rt.[Remarks] FROM [dbo].[RT-INIT_Remarks] rt WHERE rt.[ID] = CONCAT(im.[Part_Number], '*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID]))) AND rt.[ID2] = 1) as 'Op_Desc'
	                ,wc.[Wc_Nbr] as 'MachineNumber'
	                ,wc.[Name] as 'MachineName'
	                ,wc.[D_esc] as 'MachineDesc'
	                ,wc.[Work_Ctr_Group] as 'MachineGroup'
	                ,{_mOrder} as 'MachineOrder'
	                ,ISNULL(wpo.[Qty_Avail], wpo.[Qty_Req] - ISNULL(wpo.[Qty_Compl], 0)) as 'WO_CurrentQty'
	                ,ISNULL(wpo.[Date_Start], '1999-01-01') as 'WO_SchedStartDate'
	                ,ISNULL(wpo.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate'
	                ,ISNULL(wpo.[Due_Date], wpo.[Date_Start]) as 'WO_DueDate'
	                ,CAST(ROUND(wpo.[Mach_Load_Hrs_Rem], 1) as float) as 'RunTime'
	                ,ISNULL(CASE 
		                WHEN (SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) = 'DAI' THEN 'A'
		                WHEN wp.[Wo_Type] = 'R' THEN 'B'
		                ELSE wp.[Mgt_Priority_Code] END, 'D') as 'WO_Priority'
	                ,ISNULL(wp.[Wo_Type], 'S') as 'WO_Type'
	                ,wp.[Qty_To_Start] as 'WO_StartQty'
	                ,wp.[So_Reference] as 'WO_SalesRef'
	                ,cm.Cust_Nbr
	                ,CASE WHEN wp.[Time_Wanted] IS NOT NULL THEN DATEPART(HOUR, CAST(wp.[Time_Wanted] as time)) ELSE '999' END as 'PriTime'
	                ,CASE WHEN wp.[Time_Wanted] IS NOT NULL THEN DATEPART(MINUTE, CAST(wp.[Time_Wanted] as time)) ELSE '999' END as 'Sched_Priority'
	                ,im.[Part_Number] as 'SkuNumber'
	                ,im.[Description] as 'SkuDesc'
	                ,im.[Um] as 'SkuUom'
	                ,im.[Drawing_Nbrs] as 'SkuMasterPrint'
	                ,ISNULL(wp.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate'
	                ,ISNULL(wp.[Bom_Rev_Level], '') as 'BomRevLvl'
	                ,ISNULL(ipl.[Qty_On_Hand], 0) as 'SkuOnHand'
	                ,CASE WHEN wpo.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate'
	                ,CASE WHEN wpo.[Date_Start] < GETDATE() AND wp.[Qty_To_Start] = wpo.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate'
	                ,ipl.[Engineering_Status] as 'EngStatus'
	                ,(SELECT ab.[Description] FROM [dbo].[TM-INIT_Eng_Status] ab WHERE ab.[ID] = ipl.Engineering_Status) as 'EngStatusDesc'
	                ,cm.[Name] as 'Cust_Name'
	                ,(SELECT ac.[Cust_Part_Nbr] FROM [dbo].[SOD-INIT] ac WHERE ac.[ID] = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1)) as 'Cust_Part_Nbr'
	                ,CAST((SELECT ad.[Ln_Bal_Qty] FROM [dbo].[SOD-INIT] ad WHERE ad.[ID] = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1)) AS int) as 'Ln_Bal_Qty'
	                ,ISNULL(cm.Load_Pattern, '') as 'LoadPattern'
	                ,(SELECT ISNULL(rt.[Insp_Req], 'N') FROM [dbo].[RT-INIT] rt WHERE rt.[ID] = CONCAT(im.[Part_Number],'*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0)+1, LEN(wpo.[ID])))) as 'Inspection'
                    ,ISNULL(wp.[Fa_Dept], 'N') as 'Deviation'
                FROM
	                dbo.[WC-INIT] wc
                RIGHT OUTER JOIN
	                dbo.[WPO-INIT] wpo ON wpo.[Work_Center] = wc.[Wc_Nbr] AND (wpo.Seq_Complete_Flag IS NULL OR wpo.Seq_Complete_Flag = 'N') AND (wpo.Alt_Seq_Status IS NULL)
                RIGHT OUTER JOIN
	                dbo.[WP-INIT] wp ON wp.[Wp_Nbr] = SUBSTRING(wpo.[ID], 0, CHARINDEX('*', wpo.[ID], 0)) AND (wp.Status_Flag = 'R' OR wp.Status_Flag = 'A')
                RIGHT OUTER JOIN
	                dbo.[IM-INIT] im ON im.[Part_Number] = wp.[Part_Wo_Desc]
                RIGHT OUTER JOIN
	                dbo.[IPL-INIT] ipl ON ipl.[Part_Nbr] = im.[Part_Number]
                LEFT OUTER JOIN
	                dbo.[CM-INIT] cm ON cm.[Cust_Nbr] = CASE WHEN CHARINDEX('*', wp.[Cust_Nbr], 0) > 0 THEN SUBSTRING(wp.[Cust_Nbr], 0, CHARINDEX('*', wp.[Cust_Nbr], 0)) ELSE wp.[Cust_Nbr] END
                WHERE
	                wc.[D_esc] <> 'DO NOT USE'" :
                 //CSI Query
                 $@"SELECT
	                DISTINCT(wpo.[ID]) as 'WorkOrderID'
	                ,SUBSTRING(wpo.[ID], 0, CHARINDEX('*', wpo.[ID], 0)) as 'WorkOrder'
                    ,SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID])) as 'Operation'
                    ,(SELECT rt.[Remarks] FROM [dbo].[RT-INIT_Remarks] rt WHERE rt.[ID] = CONCAT(im.[Part_Number], '*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID]))) AND rt.[ID2] = 1) as 'Op_Desc'
	                ,wc.[Wc_Nbr] as 'MachineNumber'
	                ,wc.[Name] as 'MachineName'
	                ,wc.[D_esc] as 'MachineDesc'
	                ,wc.[Work_Ctr_Group] as 'MachineGroup'
	                ,{_mOrder} as 'MachineOrder'
	                ,ISNULL(wpo.[Qty_Avail], wpo.[Qty_Req] - ISNULL(wpo.[Qty_Compl], 0)) as 'WO_CurrentQty'
	                ,ISNULL(wpo.[Date_Start], '1999-01-01') as 'WO_SchedStartDate'
	                ,ISNULL(wpo.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate'
	                ,ISNULL(wpo.[Due_Date], wpo.[Date_Start]) as 'WO_DueDate'
	                ,CAST(ROUND(wpo.[Mach_Load_Hrs_Rem], 1) as float) as 'RunTime'
	                ,ISNULL(CASE 
		                WHEN (SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) = 'DAI' THEN 'A' 
		                WHEN wp.[Wo_Type] = 'R' THEN 'B'
		                ELSE wp.[Mgt_Priority_Code] END, 'D') as 'WO_Priority'
	                ,ISNULL(wp.Wo_Type, 'S') as 'WO_Type'
	                ,wp.[Qty_To_Start] as 'WO_StartQty'
	                ,wp.[So_Reference] as 'WO_SalesRef'
	                ,cm.[Cust_Nbr]
	                ,CASE WHEN wp.[Time_Wanted] IS NOT NULL THEN DATEPART(HOUR, CAST(wp.[Time_Wanted] as time)) ELSE '999' END as 'PriTime'
	                ,CASE WHEN wp.[Time_Wanted] IS NOT NULL THEN DATEPART(MINUTE, CAST(wp.[Time_Wanted] as time)) ELSE '999' END as 'Sched_Priority'
	                ,im.[Part_Number] as 'SkuNumber'
	                ,im.[Description] as 'SkuDesc'
	                ,im.[Um] as 'SkuUom'
	                ,im.[Drawing_Nbrs] as 'SkuMasterPrint'
	                ,ISNULL(wp.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate'
	                ,ISNULL(wp.[Bom_Rev_Level], '') as 'BomRevLvl'
	                ,ISNULL(ipl.[Qty_On_Hand], 0) as 'SkuOnHand'
	                ,CASE WHEN wpo.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate'
	                ,CASE WHEN wpo.[Date_Start] < GETDATE() AND wp.[Qty_To_Start] = wpo.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate'
	                ,ipl.[Engineering_Status] as 'EngStatus'
	                ,(SELECT ab.[Description] FROM [dbo].[TM-INIT_Eng_Status] ab WHERE (ab.[ID] = ipl.[Engineering_Status])) as 'EngStatusDesc'
	                ,cm.[Name] as 'Cust_Name'
	                ,(SELECT ac.[Cust_Part_Nbr] FROM [dbo].[SOD-INIT] ac WHERE (ac.[ID] = SUBSTRING(wp.[So_Reference], 0, LEN(wp.[So_Reference]) - 1))) as 'Cust_Part_Nbr'
	                ,CAST((SELECT ad.[Ln_Bal_Qty] FROM [dbo].[SOD-INIT] ad WHERE (ad.[ID] = SUBSTRING(wp.[So_Reference], 0, LEN(wp.[So_Reference]) - 1))) as int) as 'Ln_Bal_Qty'
	                ,ISNULL(cm.[Load_Pattern], '') as 'LoadPattern'
	                ,(SELECT ISNULL(rt.[Insp_Req], 'N') FROM [dbo].[RT-INIT] rt WHERE rt.[ID] = CONCAT(im.[Part_Number],'*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0)+1, LEN(wpo.[ID])))) as 'Inspection'
                    ,'N' as 'Deviation'
                FROM
	                [dbo].[WC-INIT] wc
                RIGHT OUTER JOIN
	                [dbo].[WPO-INIT] wpo ON wpo.[Work_Center] = wc.[Wc_Nbr] AND (wpo.[Seq_Complete_Flag] IS NULL OR wpo.[Seq_Complete_Flag] = 'N') AND wpo.[Alt_Seq_Status] IS NULL
                RIGHT OUTER JOIN
	                [dbo].[WP-INIT] wp ON wp.[Wp_Nbr] = SUBSTRING(wpo.[ID], 0, CHARINDEX('*', wpo.[ID], 0)) AND (wp.Status_Flag = 'R' OR wp.Status_Flag = 'A')
                RIGHT OUTER JOIN
	                dbo.[IM-INIT] im ON im.[Part_Number] = wp.[Part_Wo_Desc]
                RIGHT OUTER JOIN
	                dbo.[IPL-INIT] ipl ON ipl.[Part_Nbr] = im.[Part_Number]
                LEFT OUTER JOIN
	                dbo.[CM-INIT] cm ON cm.[Cust_Nbr] = CASE WHEN CHARINDEX('*', wp.[Cust_Nbr], 0) > 0 THEN SUBSTRING(wp.[Cust_Nbr], 0, CHARINDEX('*', wp.[Cust_Nbr], 0)) ELSE wp.[Cust_Nbr] END
                WHERE
	                wc.[D_esc] <> 'DO NOT USE'";
            //For what ever reason a view does not work for remote clients so had to use the above connection strings
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; {_conString} ORDER BY MachineOrder, MachineNumber, WO_Priority, PriTime, Sched_Priority, WO_SchedStartDate, WorkOrderID ASC", sqlCon))
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
        /// Retrieve a DataTable with all the data relevent to a closed schedule
        /// </summary>
        /// /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetClosedScheduleData(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE [{sqlCon.Database}];
                                                                            SELECT
	                                                                            DISTINCT(wpo.[ID]) as 'WO_Number'
                                                                                ,SUBSTRING(wpo.[ID], CHARINDEX('*',wpo.[ID],0) + 1,LEN(wpo.[ID])) as 'Operation'
                                                                                ,(SELECT rt.[Remarks] FROM [dbo].[RT-INIT_Remarks] rt WHERE rt.[ID] = CONCAT(im.[Part_Number], '*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID]))) AND rt.[ID2] = 1) as 'Op_Desc'
                                                                                ,wc.[Wc_Nbr] as 'MachineNumber'
	                                                                            ,wc.[Name] as 'MachineName'
	                                                                            ,wc.[D_esc] as 'MachineDesc'
	                                                                            ,wc.[Work_Ctr_Group] as 'MachineGroup'
                                                                                ,ISNULL(wpo.[Qty_Avail], wpo.[Qty_Req] - ISNULL(wpo.[Qty_Compl], 0)) as 'WO_CurrentQty'
	                                                                            ,ISNULL(wpo.[Date_Start], '1999-01-01') as 'WO_SchedStartDate'
                                                                                ,ISNULL(wpo.[Date_Act_Start], '1999-01-01') as 'WO_ActStartDate'
	                                                                            ,ISNULL(wpo.[Due_Date], wpo.[Date_Start]) as 'WO_DueDate'
                                                                                ,CAST(ROUND(wpo.[Mach_Load_Hrs_Rem], 1) AS FLOAT) as 'RunTime'
	                                                                            ,ISNULL(CASE WHEN
                                                                                        (SELECT
                                                                                            [Ord_Type]
                                                                                        FROM
                                                                                            [dbo].[SOH-INIT]
                                                                                        WHERE
                                                                                            [So_Nbr] = SUBSTRING(wp.[So_Reference],0,CHARINDEX('*',wp.[So_Reference],0))) = 'DAI'
                                                                                    THEN 'A'
                                                                                    WHEN wp.[Wo_Type] = 'R'
                                                                                    THEN 'B'
                                                                                    ELSE wp.[Mgt_Priority_Code] END, 'D') as 'WO_Priority'
	                                                                            ,wp.[Wo_Type] as 'WO_Type'
	                                                                            ,wp.[Qty_To_Start] as 'WO_StartQty'
	                                                                            ,wp.[So_Reference] as 'WO_SalesRef'
                                                                                ,wp.[Cust_Nbr]
                                                                                ,im.[Part_Number]as 'SkuNumber'
	                                                                            ,im.[Description] as 'SkuDesc'
	                                                                            ,im.[Um] as 'SkuUom', im.[Drawing_Nbrs] as 'SkuMasterPrint'
	                                                                            ,ISNULL(im.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate'
	                                                                            ,ISNULL(im.[Bom_Rev_Level], '') as 'BomRevLvl'
                                                                                ,ISNULL(ipl.[Qty_On_Hand], 0) as 'SkuOnHand'
                                                                                ,CASE WHEN wpo.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate'
	                                                                            ,CASE WHEN wpo.[Date_Start] < GETDATE() AND wp.[Qty_To_Start] = wpo.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate'
                                                                                ,ipl.[Engineering_Status] as 'EngStatus'
	                                                                            ,(SELECT [Description] FROM [dbo].[TM-INIT_Eng_Status] WHERE [ID] = ipl.[Engineering_Status]) as 'EngStatusDesc'
                                                                                ,(SELECT [Name] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = wp.[Cust_Nbr]) as 'Cust_Name'
	                                                                            ,(SELECT [Cust_Part_Nbr] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(wp.[So_Reference],0,LEN(wp.[So_Reference])-1)) as 'Cust_Part_Nbr'
	                                                                            ,CAST((SELECT [Ln_Bal_Qty] FROM [dbo].[SOD-INIT] WHERE [ID] = SUBSTRING(wp.[So_Reference],0,LEN(wp.[So_Reference])-1)) as int) as 'Ln_Bal_Qty'
                                                                                ,ISNULL((SELECT [Load_Pattern] FROM [dbo].[CM-INIT] WHERE [Cust_Nbr] = wp.[Cust_Nbr]),'') as 'LoadPattern'
                                                                                ,ISNULL(wp.[Fa_Dept], 'N') as 'Deviation'
                                                                                ,(SELECT ISNULL(rt.[Insp_Req], 'N') FROM [dbo].[RT-INIT] rt WHERE rt.[ID] = CONCAT(im.[Part_Number],'*', SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0)+1, LEN(wpo.[ID])))) as 'Inspection'
                                                                            FROM
                                                                                [dbo].[WC-INIT] wc
                                                                            RIGHT JOIN
                                                                                [dbo].[WPO-INIT] wpo ON wpo.[Work_Center] = wc.[Wc_Nbr]
                                                                            RIGHT JOIN
                                                                                [dbo].[WP-INIT] wp ON wpo.[ID] LIKE CONCAT(wp.[Wp_Nbr], '%')
                                                                            RIGHT JOIN
                                                                                [dbo].[IM-INIT] im ON im.[Part_Number] = wp.[Part_Wo_Desc]
                                                                            RIGHT JOIN
                                                                                [dbo].[IPL-INIT] ipl ON ipl.[Part_Nbr] = im.[Part_Number]
                                                                            WHERE
                                                                                wc.[D_esc] <> 'DO NOT USE' AND wp.[Status_Flag] = 'C' AND wpo.[Alt_Seq_Status] IS NULL
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
        /// Get a table containing all of the Machines
        /// </summary>
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        public static DataTable GetMachineTable(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    var _selectCmd = string.Empty;
                    if (sqlCon.Database.Contains("WCCO"))
                    {
                        _selectCmd = @"SELECT
	                                        [Wc_Nbr] as 'WorkCenterID'
	                                        ,[Name] as 'Name'
	                                        ,[D_esc] as 'Description'
	                                        ,[Work_Ctr_Group] as 'Group'
	                                        ,CAST(ISNULL([Press_Length], 0) as int) as 'Length'
                                        FROM
	                                        [dbo].[WC-INIT]
                                        WHERE
	                                        [D_esc] <> 'DO NOT USE' AND [Name] IS NOT NULL";
                    }
                    else
                    {
                        _selectCmd = @"SELECT
	                                        [Wc_Nbr] as 'WorkCenterID'
	                                        ,[Name] as 'Name'
	                                        ,[D_esc] as 'Description'
	                                        ,[Work_Ctr_Group] as 'Group'
                                        FROM
	                                        [dbo].[WC-INIT]
                                        WHERE
	                                        [D_esc] <> 'DO NOT USE' AND [Name] IS NOT NULL";
                    }
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE [{sqlCon.Database}]; {_selectCmd}", sqlCon))
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

        #endregion

        /// <summary>
        /// Get a list of work centers
        /// </summary>
        /// <param name="incAll">Include all at the top of the list</param>
        /// <param name="incNone">Include None at the top of the list</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(bool incAll, bool incNone)
        {
            var _tempList = new List<Machine>();
            if (incAll)
            {
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "All", IsLoaded = true, MachineGroup = "All" });
            }
            if (incNone)
            {
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "None", IsLoaded = false, MachineGroup = "None" });
            }
            foreach (DataRow _row in MasterDataSet.Tables["WC"].Rows)
            {
                _tempList.Add(new Machine
                {
                    MachineNumber = _row.Field<string>("WorkCenterID")
                    ,MachineName = _row.Field<string>("Name")
                    ,MachineDescription = _row.Field<string>("Description")
                    ,MachineGroup = _row.Field<string>("Group")
                });
            }
            return _tempList;
        }

        /// <summary>
        /// Get a list of work centers names
        /// </summary>
        /// <returns>generic list of workcenter objects</returns>
        public static List<string> GetMachineList(bool incAll)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            foreach (DataRow _row in MasterDataSet.Tables["WC"].Rows)
            {
                _tempList.Add(_row.Field<string>("Name"));
            }
            return _tempList;
        }

        /// <summary>
        /// Retrieve a List of strings of each of the groups assigned to the machines
        /// </summary>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetMachineGroupList(bool incAll)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            _tempList.Add("Custom");
            foreach (DataRow _row in MasterDataSet.Tables["WC"].DefaultView.ToTable(true, "Group").Rows)
            {
                _tempList.Add(_row.Field<string>("Group"));
            }
            return _tempList;
        }

        /// <summary>
        /// Get a machines display name
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <returns>Machine Name as string</returns>
        public static string GetMachineName(string searchValue)
        {
            var _rVal = string.Empty;
            if (searchValue == "0")
            {
                _rVal = "All";
            }
            else
            {
                if (!Exists(searchValue))
                {
                    var _sRows = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                    if (_sRows.Length > 0)
                    {
                        searchValue = _sRows.FirstOrDefault().Field<string>("WorkCenterID");
                    }
                }
                var _rows = MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{searchValue}'");
                if (_rows.Length > 0)
                {
                    _rVal = _rows.FirstOrDefault().Field<string>("Name");
                }
            }
            return _rVal;
        }

        /// <summary>
        /// Checks to see if a machine exists
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <returns>Verification as a bool</returns>
        public static new bool Exists(string searchValue)
        {
            return int.TryParse(searchValue, out int i)
                ? MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{i}'").Length > 0
                : MasterDataSet.Tables["WC"].Select($"[Name] = '{searchValue}'").Length > 0;
        }

        /// <summary>
        /// Get the machine group that a specific machine is a part of
        /// </summary>
        /// <param name="searchValue">Search value when looking for the machine group</param>
        /// <param name="type">Type of search. M = Machine Name, N = Machine Number</param>
        /// <returns>machine group</returns>
        public static string GetMachineGroup(string searchValue, char type)
        {
            switch(type)
            {
                case 'M':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables["WC"].Select($"[Name] = '{searchValue}'")[0].Field<string>("Group");
                case 'N':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{searchValue}'")[0].Field<string>("Group");
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the machine number from a machine name
        /// </summary>
        /// <param name="machineName">Machine name to get the group of</param>
        /// <returns>machine number</returns>
        public static string GetMachineNumber(string machineName)
        {
            return machineName == "All"
                ? machineName
                : MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'")[0].Field<string>("WorkCenterID");
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="machineNbr">Press ID number</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(int machineNbr)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{machineNbr}'");
            return _tempRow.Length > 1 || _tempRow[0].Field<int>("Length") == 0 ? 0 : _tempRow[0].Field<int>("Length") + 1;
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="machineName">Name of the press</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(string machineName)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'");
            return _tempRow.Length > 1 || _tempRow[0].Field<int>("Length") == 0 ? 0 : _tempRow[0].Field<int>("Length") + 1;
        }
    }
}
