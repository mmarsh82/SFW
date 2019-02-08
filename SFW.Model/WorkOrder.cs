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
        public string SeqDesc { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
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
        /// <param name="drow">DataRow with the item array values for the work order</param>
        public WorkOrder(DataRow drow, SqlConnection sqlCon)
        {
            if (drow != null)
            {
                var _wo = drow.Field<string>("WO_Number").Split('*');
                OrderNumber = _wo[0];
                Seq = _wo[1];
                SeqDesc = GetOperationDescription(drow.Field<string>("WO_Number"), sqlCon);
                Priority = drow.Field<string>("WO_Priority");
                StartQty = drow.Field<int>("WO_StartQty");
                CurrentQty = Convert.ToInt32(drow.Field<decimal>("WO_CurrentQty"));
                SchedStartDate = drow.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = drow.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? drow.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = drow.Field<DateTime>("WO_DueDate");
                SkuNumber = drow.Field<string>("SkuNumber");
                SkuDescription = drow.Field<string>("SkuDesc");
                Uom = drow.Field<string>("SkuUom");
                MasterPrint = drow.Field<string>("SkuMasterPrint");
                TotalOnHand = drow.Field<int>("SkuOnHand");
                BomRevDate = drow.Field<DateTime>("BomRevDate") != Convert.ToDateTime("1999-01-01") ? drow.Field<DateTime>("BomRevDate") : DateTime.MinValue;
                BomRevLevel = drow.Field<string>("BomRevLvl");
                EngStatus = drow.Field<string>("EngStatus");
                EngStatusDesc = drow.Field<string>("EngStatusDesc");
                SalesOrder = new SalesOrder(drow.Field<string>("WO_SalesRef"), sqlCon);
                Bom = Component.GetComponentList(_wo[0], StartQty - CurrentQty, sqlCon);
                Notes = GetNotes(_wo[0],sqlCon);
                ShopNotes = GetShopNotes(_wo[0], sqlCon);
            }
        }

        /// <summary>
        /// Retrieve a list of Work orders based on a work center
        /// </summary>
        /// <param name="workCntNbr">Work Center Number or ID</param>
        /// /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of WorkOrder objects</returns>
        public static List<WorkOrder> GetWorkOrderList(string workCntNbr, SqlConnection sqlCon)
        {
            var _tempList = new List<WorkOrder>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
                                        SeqDesc = "Coming Soon...",
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
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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
        /// Get a work order's operation description
        /// </summary>
        /// <param name="woNbr">Work order number with sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Operation description as string</returns>
        public static string GetOperationDescription(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Remarks] FROM [dbo].[WPO_REMARKS-INIT_Remarks] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Remarks")} ";
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
    }
}
