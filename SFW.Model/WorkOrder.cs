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
        public int Start_Qty { get; set; }
        public int Current_Qty { get; set; }
        public int Scrap_Qty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

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
                                                                a.[ID], b.[Part_Wo_Desc], b.[Mgt_Priority_Code], b.[Qty_To_Start], a.[Qty_Avail], a.[Qty_Scrap], a.[Date_Start], a.[Due_Date]
                                                            FROM
                                                                [dbo].[WPO-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[WP-INIT] b on a.[ID] LIKE CONCAT(b.[Wp_Nbr], '%')
                                                            WHERE
                                                                (b.[Status_Flag] = 'R' or B.[Status_Flag] = 'A') AND a.[Qty_Avail] <> 0 AND a.[Work_Center] = @p1
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
                                        Part = reader.IsDBNull(1) ? null : new Skew(reader.GetString(1), sqlCon),
                                        Priority = reader.IsDBNull(2) ? "D" : reader.GetString(2),
                                        Start_Qty = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                        Current_Qty = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                                        Scrap_Qty = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                        StartDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                                        DueDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7)
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
