using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work order object
    /// </summary>
    public class WorkOrder : Sku
    {
        #region Properties

        public string OrderNumber { get; set; }
        public string Seq { get; set; }
        public string OpDesc { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string TaskType { get; set; }
        public int StartQty { get; set; }
        public int CurrentQty { get; set; }
        public int ScrapQty { get; set; }
        public DateTime SchedStartDate { get; set; }
        public DateTime ActStartDate { get; set; }
        public DateTime DueDate { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public string Notes { get; set; }
        public string ShopNotes { get; set; }
        public bool IsLate { get { return DueDate < DateTime.Today; } }
        public bool IsStartedLate { get { return SchedStartDate < DateTime.Today && CurrentQty == StartQty; } }
        public List<Component> Picklist { get; set; }
        public bool IsDeviated { get; set; }

        #endregion

        /// <summary>
        /// WorkOrder object default constructor
        /// </summary>
        public WorkOrder()
        { }

        /// <summary>
        /// Work Order Object constructor
        /// Will create a new WorkOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the work order</param>
        public WorkOrder(DataRow dRow)
        {
            if (dRow != null)
            {
                var _wo = dRow.Field<string>("WO_Number").Split('*');
                OrderNumber = _wo[0];
                Seq = _wo[1];
                Operation = dRow.Field<string>("Operation");
                OpDesc = dRow.Field<string>("Op_Desc");
                Priority = dRow.Field<string>("WO_Priority");
                TaskType = dRow.Field<string>("WO_Type");
                StartQty = dRow.Field<int>("WO_StartQty");
                CurrentQty = Convert.ToInt32(dRow.Field<decimal>("WO_CurrentQty"));
                SchedStartDate = dRow.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = dRow.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = dRow.Field<DateTime>("WO_DueDate");
                SkuNumber = dRow.Field<string>("SkuNumber");
                SkuDescription = dRow.Field<string>("SkuDesc");
                Uom = dRow.Field<string>("SkuUom");
                MasterPrint = dRow.Field<string>("SkuMasterPrint");
                TotalOnHand = dRow.Field<int>("SkuOnHand");
                BomRevDate = dRow.Field<DateTime>("BomRevDate") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("BomRevDate") : DateTime.MinValue;
                BomRevLevel = dRow.Field<string>("BomRevLvl");
                EngStatus = dRow.Field<string>("EngStatus");
                EngStatusDesc = dRow.Field<string>("EngStatusDesc");
                if (!string.IsNullOrEmpty(dRow.Field<string>("WO_SalesRef")))
                {
                    var _so = dRow.Field<string>("WO_SalesRef").Split('*');
                    SalesOrder = new SalesOrder
                    {
                        SalesNumber = _so[0],
                        CustomerName = dRow.Field<string>("Cust_Name"),
                        CustomerNumber = dRow.Field<string>("Cust_Nbr"),
                        CustomerPart = dRow.Field<string>("Cust_Part_Nbr"),
                        LineNumber = Convert.ToInt32(_so[1]),
                        LineBalQuantity = dRow.Field<int>("Ln_Bal_Qty"),
                        LoadPattern = dRow.Field<string>("LoadPattern").ToUpper() == "PLASTIC"
                     };
                }
                else
                {
                    SalesOrder = new SalesOrder();
                }
                Machine = dRow.Field<string>("MachineName");
                MachineGroup = dRow.Field<string>("MachineGroup");
                IsDeviated = dRow.Field<string>("Deviation") == "Y";
                Inspection = dRow.Field<string>("Inspection") == "Y";
            }
        }

        /// <summary>
        /// Retrieve a list of Work orders based on a work center
        /// </summary>
        /// <param name="workCntNbr">Work Center Number or ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of WorkOrder objects</returns>
        public static List<WorkOrder> GetWorkOrderList(string workCntNbr, SqlConnection sqlCon)
        {
            var _tempList = new List<WorkOrder>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                SUBSTRING(a.[ID], 0, CHARINDEX('*',a.[ID],0)) as 'WoNumber',
	                                                            SUBSTRING(a.[ID], CHARINDEX('*',a.[ID],0) + 1, LEN(a.[ID])) as 'Seq',
	                                                            a.[Qty_Avail] as 'CurrentQty', ISNULL(a.[Qty_Scrap], 0) as 'Scrap', a.[Date_Start] as 'SchedStartDate', a.[Date_Act_Start] as 'ActStartDate', a.[Due_Date] as 'DueDate',
	                                                            b.[Part_Wo_Desc] as 'WoDesc', ISNULL(b.[Mgt_Priority_Code], 'D') as 'Priority', b.[Qty_To_Start] as 'StartQty', b.[So_Reference] as 'SalesOrder'
                                                            FROM
                                                                [dbo].[WPO-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[WP-INIT] b on a.[ID] LIKE CONCAT(b.[Wp_Nbr], '%')
                                                            WHERE
                                                                (b.[Status_Flag] = 'R' or b.[Status_Flag] = 'A') AND a.[Seq_Complete_Flag] IS NULL AND a.[Work_Center] = @p1
                                                            ORDER BY
                                                                StartDate, WoNumber ASC;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workCntNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new WorkOrder
                                    {
                                        OrderNumber = reader.SafeGetString("WoNumber"),
                                        Seq = reader.SafeGetString("Seq"),
                                        Priority = reader.SafeGetString("Priority"),
                                        StartQty = reader.SafeGetInt32("StartQty"),
                                        CurrentQty = reader.SafeGetInt32("CurrentQty"),
                                        ScrapQty = reader.SafeGetInt32("Scrap"),
                                        SchedStartDate = reader.SafeGetDateTime("SchedStartDate"),
                                        DueDate = reader.SafeGetDateTime("DueDate"),
                                        SalesOrder = new SalesOrder(reader.SafeGetString("SalesOrder"), sqlCon)
                                    });
                                }
                            }
                        }
                    }
                    foreach(var o in _tempList)
                    {
                        using (SqlCommand nCmd = new SqlCommand("SELECT * FROM [dbo].[WP-INIT_Wo_Notes] WHERE [ID] = @p1;", sqlCon))
                        {
                            nCmd.Parameters.AddWithValue("p1", o.OrderNumber);
                            using (SqlDataReader reader = nCmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        o.Notes += reader.SafeGetString("");
                                    }
                                }
                            }
                        }
                        using (SqlCommand sCmd = new SqlCommand("SELECT [Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] WHERE [ID] = @p1;", sqlCon))
                        {
                            sCmd.Parameters.AddWithValue("p1", o.OrderNumber);
                            using (SqlDataReader reader = sCmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        o.ShopNotes += reader.SafeGetString("");
                                    }
                                }
                            }
                        }
                    }
                    return _tempList.OrderBy(o => o.Priority).ToList();
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
        /// Get a work order's notes
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A concatonation of notes into a string</returns>
        public static string GetNotes(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Wo_Notes] FROM [dbo].[WP-INIT_Wo_Notes] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Wo_Notes")}\n";
                                }
                            }
                        }
                    }
                    return _notes;
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
        /// Get a work order's shop floor notes
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A concatonation of shop floor notes into a string</returns>
        public static string GetShopNotes(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Wo_Sf_Notes")}\n";
                                }
                            }
                        }
                    }
                    return _notes;
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
        /// Get work order note's table
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>All work order notes in a datatable</returns>
        public static DataTable GetNotes(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Notes]", sqlCon))
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
        /// Get the machine ID that is assigned to a specific work order and sequence
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Machine ID as string</returns>
        public static string GetAssignedMachineID(string woNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = CONCAT(@p1, '*', @p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
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
        /// Get the machine name that is assigned to a specific work order and sequence
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Machine name as string</returns>
        public static string GetAssignedMachineName(string woNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT 
	                                                                b.[Name]
                                                                FROM
	                                                                [dbo].[WPO-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[WC-INIT] b ON b.[Wc_Nbr] = a.[Work_Center]
                                                                WHERE
	                                                                a.[ID] = CONCAT(@p1, '*', @p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
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
