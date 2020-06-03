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
        /// Number of slats blanked out per piece
        /// Used in the round qty completed calculation along with a direct tie to the roll length for cut points
        /// </summary>
        public int? SlatBlankout { get; set; }

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

        /// <summary>
        /// Tells the object interface whether or not the report is a new report or an existing report
        /// </summary>
        public bool IsNew { get; set; }

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
            IsNew = true;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[PRM-CSTM] WHERE [WorkOrder] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                IsNew = false;
                                while (reader.Read())
                                {
                                    SlatTransfer = reader.SafeGetInt32("SlatTransfer");
                                    RollLength = reader.SafeGetInt32("RollLength");
                                    SlatBlankout = reader.SafeGetInt32("BlankSlats");
                                }
                            }
                        }
                    }
                    if (SlatTransfer != null)
                    {
                        ShiftReportList = Press_ShiftReport.GetPress_ShiftReportList(wo.OrderNumber, Machine.GetMachineName(sqlCon, wo), sqlCon);
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
        /// Checks to see if the work order exists in the Sql database
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <returns>True or False on existance</returns>
        public static bool Exists(string woNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT COUNT([WorkOrder]) FROM [dbo].[PRM-CSTM] WHERE [WorkOrder] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i > 0 : false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Submit a press report object to the Sql Database
        /// </summary>
        /// <param name="pReport">Press Report Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Submit(PressReport pReport, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                object o = pReport.SlatBlankout ?? System.Data.SqlTypes.SqlInt32.Null;
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM] ([WorkOrder], [SlatTransfer], [RollLength], [BlankSlats])
                                                                VALUES (@p1, @p2, @p3, @p4)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", pReport.ShopOrder.OrderNumber);
                        cmd.Parameters.AddWithValue("p2", pReport.SlatTransfer);
                        cmd.Parameters.AddWithValue("p3", pReport.RollLength);
                        cmd.Parameters.AddWithValue("p4", o);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                { }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Update a press report object in the Sql Database
        /// </summary>
        /// <param name="pReport">Press Report Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Update(PressReport pReport, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                UPDATE
                                                                    [dbo].[PRM-CSTM] 
                                                                SET
                                                                    [SlatTransfer] = @p1, [RollLength] = @p2, [BlankSlats] = @p3
                                                                WHERE
                                                                    [WorkOrder] = @p4", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", pReport.SlatTransfer);
                        cmd.Parameters.AddWithValue("p2", pReport.RollLength);
                        cmd.Parameters.AddWithValue("p3", pReport.SlatBlankout);
                        cmd.Parameters.AddWithValue("p4", pReport.ShopOrder.OrderNumber);
                        cmd.ExecuteNonQuery();
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
