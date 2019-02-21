using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class PressReport : ModelBase
    {
        #region Properties

        /// <summary>
        /// Number of slats transfered per round
        /// Used in the round qty completed calculation along with a direct tie to the roll length for cut points
        /// </summary>
        public int? SlatTransfer { get; set; }

        /// <summary>
        /// Length of the rolls that production is creating on this report
        /// </summary>
        public int? RollLength { get; set; }

        /// <summary>
        /// List of shift reports
        /// There is 1 press report per work order but there are many shift reports per press report
        /// </summary>
        public List<Press_ShiftReport> ShiftReportList { get; set; }

        /// <summary>
        /// Work order object for this report object to be created against
        /// </summary>
        public WorkOrder ShopOrder { get; set; }

        #endregion

        /// <summary>
        /// PressReport Object Default Constructor
        /// </summary>
        /// <param name="wo">Work Order Object to report against</param>
        /// <param name="sqlCon">Sql Connection to use will be null if loading a new object</param>
        public PressReport(WorkOrder wo, SqlConnection sqlCon)
        {
            ShopOrder = wo;
            ShiftReportList = new List<Press_ShiftReport>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {

                                }
                            }
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Submit a press report object to the Sql Database
        /// </summary>
        /// <param name="pReport">Press Report Object</param>
        /// <param name="psReport">Press Shift Report Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Report unique ID</returns>
        public int Submit(PressReport pReport, Press_ShiftReport psReport, SqlConnection sqlCon)
        {   
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _valid = 0;
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT Count([WorkOrder]) FROM [dbo].[PRM-CSTM] WHERE [WorkOrder] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", pReport.ShopOrder.OrderNumber);
                        _valid = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    if (_valid == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM] ([WorkOrder], [SlatTransfer], [RollLength], [Machine])
                                                                VALUES (@p1, @p2, @p3, @p4)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", pReport.ShopOrder.OrderNumber);
                            cmd.Parameters.AddWithValue("p2", pReport.SlatTransfer);
                            cmd.Parameters.AddWithValue("p3", pReport.RollLength);
                            cmd.Parameters.AddWithValue("p4", psReport.MachineName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return psReport.Submit(pReport.ShopOrder.OrderNumber, psReport, sqlCon);
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        public void Update(PressReport pReport, Press_ShiftReport psReport, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    
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
