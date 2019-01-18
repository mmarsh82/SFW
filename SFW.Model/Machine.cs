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
    public sealed class Machine : WorkOrder
    {
        #region Properties

        public string MachineNumber { get; set; }
        public string MachineName { get; set; }
        public string MachineDescription { get; set; }
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
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetWorkCenterList(SqlConnection sqlCon)
        {
            var _tempList = new List<Machine>();
            var conString = string.Empty;
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Wc_Nbr], [Name], [D_esc] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
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
                                        MachineDescription = reader.SafeGetString("D_esc")
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
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "All", IsLoaded = true });
            }
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Wc_Nbr], [Name] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
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
                                        IsLoaded = true
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
            //TODO: Needs to be rewritten to include a list rather than a datatable so that in the future async loading can be done
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
	                                                                            c.[Wo_Type] as 'WO_Type',
	                                                                            c.[Qty_To_Start] as 'WO_StartQty',
	                                                                            c.[So_Reference] as 'WO_SalesRef',
                                                                                d.[Part_Number]as 'SkuNumber',
	                                                                            d.[Description] as 'SkuDesc',
	                                                                            d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                                                                            ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                                                                            ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                                                                                CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                                                                            CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate'
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
                                                                                a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.[Status_Flag] = 'A') AND b.[Seq_Complete_Flag] IS NULL AND b.[Alt_Seq_Status] IS NULL AND (b.[Qty_Avail] > 0 OR b.[Qty_Avail] IS NULL)
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
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
	                                                                            ISNULL(CASE WHEN (SELECT [Ord_Type] FROM [dbo].[SOH-INIT] WHERE [So_Nbr] = SUBSTRING(c.[So_Reference],0,CHARINDEX('*',c.[So_Reference],0))) = 'DAI' THEN 'A' WHEN c.[Wo_Type] = 'R' THEN 'B' ELSE c.[Mgt_Priority_Code] END, 'D') as 'WO_Priority',
	                                                                            c.[Wo_Type] as 'WO_Type',
	                                                                            c.[Qty_To_Start] as 'WO_StartQty',
	                                                                            c.[So_Reference] as 'WO_SalesRef',
                                                                                d.[Part_Number]as 'SkuNumber',
	                                                                            d.[Description] as 'SkuDesc',
	                                                                            d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint',
	                                                                            ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate',
	                                                                            ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
                                                                                CASE WHEN b.[Due_Date] < GETDATE() THEN 1 ELSE 0 END as 'IsLate',
	                                                                            CASE WHEN b.[Date_Start] < GETDATE() AND c.[Qty_To_Start] = b.[Qty_Avail] THEN 1 ELSE 0 END as 'IsStartLate'
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
                                                                                a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.Status_Flag = 'A') AND b.[Seq_Complete_Flag] IS NULL AND b.[Alt_Seq_Status] IS NULL AND (b.[Qty_Avail] > 0 OR b.[Qty_Avail] IS NULL) AND a.[Wc_Nbr]=@p1
                                                                            ORDER BY
                                                                                MachineNumber, WO_Priority, WO_StartDate, WO_Number ASC;", sqlCon))
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
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
        /// 
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
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
    }
}
