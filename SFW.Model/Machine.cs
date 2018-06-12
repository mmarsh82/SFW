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
                    using (SqlCommand cmd = new SqlCommand("SELECT [Wc_Nbr], [Name], [D_esc] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
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
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(SqlConnection sqlCon)
        {
            var _tempList = new List<Machine>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT [Wc_Nbr], [Name] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
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
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT
                                                                                a.[Wc_Nbr] as 'MachineNumber', a.[Name] as 'MachineName', a.[D_esc] as 'MachineDesc',
                                                                                b.[ID] as 'WO_Number', ISNULL(b.[Qty_Avail], 0) as 'WO_CurrentQty', ISNULL(b.[Date_Start], '1999-01-01') as 'WO_StartDate', b.[Due_Date] as 'WO_DueDate',
                                                                                ISNULL(c.[Mgt_Priority_Code], 'D') as 'WO_Priority', c.[Qty_To_Start] as 'WO_StartQty', c.[So_Reference] as 'WO_SalesRef',
                                                                                d.[Part_Number]as 'SkuNumber', d.[Description] as 'SkuDesc', d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint', ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate', ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
	                                                                            (SELECT ' ' + f.[Wo_Notes] FROM [dbo].[WP-INIT_Wo_Notes] f WHERE b.[ID] LIKE CONCAT(f.[ID], '%') FOR XML PATH('')) as 'WO_Notes',
	                                                                            (SELECT ' ' + g.[Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] g WHERE b.[ID] LIKE CONCAT(g.[ID], '%') FOR XML PATH('')) as 'WO_ShopNotes'
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
                                                                                a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.Status_Flag = 'A') AND b.[Seq_Complete_Flag] IS NULL AND b.[Qty_Avail] > 0
                                                                            ORDER BY
                                                                                MachineNumber, WO_Priority, WO_StartDate, WO_Number ASC;", sqlCon))
                        {
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
        /// Retrieve a DataTable with all the data relevent to a schedule for a single machine
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="machineNumber">Machine Number to load</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon, string machineNumber)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT
                                                                                a.[Wc_Nbr] as 'MachineNumber', a.[Name] as 'MachineName', a.[D_esc] as 'MachineDesc',
                                                                                b.[ID] as 'WO_Number', ISNULL(b.[Qty_Avail], 0) as 'WO_CurrentQty', ISNULL(b.[Date_Start], '1999-01-01') as 'WO_StartDate', b.[Due_Date] as 'WO_DueDate',
                                                                                ISNULL(c.[Mgt_Priority_Code], 'D') as 'WO_Priority', c.[Qty_To_Start] as 'WO_StartQty', c.[So_Reference] as 'WO_SalesRef',
                                                                                d.[Part_Number]as 'SkuNumber', d.[Description] as 'SkuDesc', d.[Um] as 'SkuUom', d.[Drawing_Nbrs] as 'SkuMasterPrint', ISNULL(d.[Bom_Rev_Date], '1999-01-01') as 'BomRevDate', ISNULL(d.[Bom_Rev_Level], '') as 'BomRevLvl',
                                                                                ISNULL(e.[Qty_On_Hand], 0) as 'SkuOnHand',
	                                                                            (SELECT ' ' + f.[Wo_Notes] FROM [dbo].[WP-INIT_Wo_Notes] f WHERE b.[ID] LIKE CONCAT(f.[ID], '%') FOR XML PATH('')) as 'WO_Notes',
	                                                                            (SELECT ' ' + g.[Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] g WHERE b.[ID] LIKE CONCAT(g.[ID], '%') FOR XML PATH('')) as 'WO_ShopNotes'
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
                                                                                a.[D_esc] <> 'DO NOT USE' AND (c.[Status_Flag] = 'R' OR c.Status_Flag = 'A') AND b.[Seq_Complete_Flag] IS NULL AND b.[Qty_Avail] > 0 AND a.[Wc_Nbr] = @p1
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
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetWorkCenterGroup(SqlConnection sqlCon)
        {
            var _tempList = new List<string>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    //TODO add in the command string
                    using (SqlCommand cmd = new SqlCommand("", sqlCon))
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
    }
}
