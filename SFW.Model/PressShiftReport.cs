using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class PressShiftReport : ModelBase
    {
        #region Properties

        /// <summary>
        /// Unique ID for the shift report
        /// </summary>
        public int? ReportID { get; set; }

        /// <summary>
        /// Report Work Center Name
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Round data for the press report per shift
        /// </summary>
        public BindingList<PressRound> RoundList { get; set; }

        /// <summary>
        /// Production shift allocated to the shift report object
        /// </summary>
        public int Shift { get; set; }

        /// <summary>
        /// Date this report object was created
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Status of the report
        /// </summary>
        public string ReportStatus { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PressShiftReport()
        { }

        /// <summary>
        /// Press shift report object constructor
        /// To be used when creating a new shift report sheet
        /// </summary>
        /// <param name="woNbr">Work Order number</param>
        /// <param name="rDate">Report or submit date</param>
        /// <param name="shift">Shift for this report</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public PressShiftReport(WorkOrder wo, DateTime rDate, int shift, SqlConnection sqlCon)
        {
            Shift = shift;
            ReportDate = rDate;
            ReportStatus = "O";
            MachineName = wo.Machine;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT COUNT([WorkOrder])
                                                                FROM [dbo].[PRM-CSTM_Shift]
                                                                WHERE [WorkOrder] = @p1 AND [Status] = 'O';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                        if(int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0)
                        {
                            using (SqlCommand nCmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                        UPDATE [dbo].[PRM-CSTM_Shift]
                                                                        SET [Status] = 'C'
                                                                        WHERE [WorkOrder] = @p1 AND [Status] = 'O';", sqlCon))
                            {
                                nCmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                                nCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO [dbo].[PRM-CSTM_Shift]
                                                                ([SubmitDate], [WorkOrder], [Shift], [Status])
                                                                VALUES(@p1, @p2, @p3, @p4);
                                                                SELECT [ReportID] FROM [PRM-CSTM_Shift] WHERE [ReportID] = @@IDENTITY;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ReportDate);
                        cmd.Parameters.AddWithValue("p2", wo.OrderNumber);
                        cmd.Parameters.AddWithValue("p3", Shift);
                        cmd.Parameters.AddWithValue("p4", ReportStatus);
                        ReportID = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
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
            RoundList = PressRound.GetRoundList(int.Parse(ReportID.ToString()), sqlCon);
        }

        /// <summary>
        /// Press shift report object constructor
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="wcName">Machine name</param>
        /// <param name="rDate">Report date</param>
        /// <param name="shift">Shift for this report</param>
        /// <param name="status">Report status</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public PressShiftReport(int reportID,  string wcName, DateTime rDate, int shift, string status, SqlConnection sqlCon)
        {
            ReportID = reportID;
            MachineName = wcName;
            ReportDate = rDate;
            Shift = shift;
            ReportStatus = status;
            RoundList = PressRound.GetRoundList(reportID, sqlCon);
        }

        /// <summary>
        /// Get a list of all the press shift reports for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="machineName">Machine Name</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of Press_ShiftReport objects</returns>
        public static List<PressShiftReport> GetPress_ShiftReportList(string woNbr, string machineName, SqlConnection sqlCon)
        {
            var _tempList = new List<PressShiftReport>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 ORDER BY [ReportID] DESC", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new PressShiftReport(reader.SafeGetInt32("ReportID"), machineName, reader.SafeGetDateTime("SubmitDate"), reader.SafeGetInt32("Shift"), reader.SafeGetString("Status"), sqlCon));
                                }
                                ModelSqlCon = sqlCon;
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
