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
    public class WorkOrder : ModelBase
    {
        #region Properties

        public string OrderNumber { get; set; }
        public string Seq { get; set; }
        public Skew Part { get; set; }
        public string Priority { get; set; }
        public int Req_Qty { get; set; }
        public int Comp_Qty { get; set; }
        public int Scrap_Qty { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public WorkOrder()
        {
            
        }

        /// <summary>
        /// Retrieve a list of Work orders based on a work center
        /// </summary>
        /// <param name="workCntNbr">Work Center Number or ID</param>
        /// <returns>List of WorkOrder objects</returns>
        public static List<WorkOrder> GetWorkOrderList(string workCntNbr, SqlConnection sqlCon)
        {
            var _tempList = new List<WorkOrder>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                a.[ID], b.[Part_Wo_Desc], b.[Mgt_Priority_Code], b.[Qty_To_Start]
                                                            FROM
                                                                [dbo].[WPO-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[WP-INIT] b on a.[ID] LIKE CONCAT(b.[Wp_Nbr], '%')
                                                            WHERE
                                                                b.[Status_Flag] = 'R' AND a.[Work_Center] = @p1;", sqlCon))
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
                                        Part = reader.IsDBNull(1) ? null : new Skew(reader.GetString(1), sqlCon),
                                        Priority = reader.IsDBNull(2) ? "D" : reader.GetString(2),
                                        Req_Qty = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
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
