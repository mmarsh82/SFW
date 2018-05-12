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
        public string Priority { get; set; }
        public int StartQty { get; set; }
        public int CurrentQty { get; set; }
        public int ScrapQty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public SalesOrder SalesOrder { get; set; }

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
                Priority = drow.Field<string>("WO_Priority");
                StartQty = drow.Field<int>("WO_StartQty");
                CurrentQty = Convert.ToInt32(drow.Field<decimal>("WO_CurrentQty"));
                StartDate = drow.Field<DateTime>("WO_StartDate");
                DueDate = drow.Field<DateTime>("WO_DueDate");
                SkuNumber = drow.Field<string>("SkuNumber");
                SkuDescription = drow.Field<string>("SkuDesc");
                Uom = drow.Field<string>("SkuUom");
                MasterPrint = drow.Field<string>("SkuMasterPrint");
                TotalOnHand = drow.Field<int>("SkuOnHand");
                BomRevDate = drow.Field<DateTime>("BomRevDate");
                BomRevLevel = drow.Field<string>("BomRevLvl");
                SalesOrder = new SalesOrder(drow.Field<string>("WO_SalesRef"), sqlCon);
                Bom = Component.GetComponentList(_wo[0], StartQty - CurrentQty, sqlCon);
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
                                                                a.[ID], b.[Part_Wo_Desc], b.[Mgt_Priority_Code], b.[Qty_To_Start], a.[Qty_Avail], a.[Qty_Scrap], a.[Date_Start], a.[Due_Date], b.[So_Reference]
                                                            FROM
                                                                [dbo].[WPO-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[WP-INIT] b on a.[ID] LIKE CONCAT(b.[Wp_Nbr], '%')
                                                            WHERE
                                                                (b.[Status_Flag] = 'R' or B.[Status_Flag] = 'A') AND a.[Seq_Complete_Flag] IS NULL AND a.[Work_Center] = @p1
                                                            ORDER BY
                                                                a.[Date_Start], a.[ID] ASC;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workCntNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _id = reader.IsDBNull(0) ? null : reader.GetString(0).Split('*');
                                    _tempList.Add(new WorkOrder
                                    {
                                        OrderNumber = _id == null ? string.Empty : _id[0].Trim(),
                                        Seq = _id == null ? string.Empty : _id[1].Trim(),
                                        Priority = reader.IsDBNull(2) ? "D" : reader.GetString(2),
                                        StartQty = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                        CurrentQty = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                        ScrapQty = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                        StartDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                                        DueDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                                        SalesOrder = reader.IsDBNull(8) ? new SalesOrder() : new SalesOrder(reader.GetString(8), sqlCon)
                                    });
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
    }
}
