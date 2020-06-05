using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class Press_ShiftReport
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
        public DataTable RoundTable { get; set; }

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
        public Press_ShiftReport()
        { }

        /// <summary>
        /// Press shift report object constructor
        /// To be used when creating a new shift report sheet
        /// </summary>
        /// <param name="woNbr">Work Order number</param>
        /// <param name="rDate">Report or submit date</param>
        /// <param name="shift">Shift for this report</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Press_ShiftReport(WorkOrder wo, DateTime rDate, int shift, SqlConnection sqlCon)
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
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                            SELECT TOP (0) [Time]
                                                                                ,[RoundNumber] as 'Round'
                                                                                ,[QtyComplete] as 'Quantity'
                                                                                ,[Qlty_Flag] as 'Flag'
                                                                                ,[Notes]
                                                                                ,[RollNbr] as 'Roll'
                                                                            FROM [dbo].[PRM-CSTM_Round]", sqlCon))
                    {
                        RoundTable = new DataTable();
                        adapter.Fill(RoundTable);
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
        /// Press shift report object constructor
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="wcName">Machine name</param>
        /// <param name="rDate">Report date</param>
        /// <param name="shift">Shift for this report</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Press_ShiftReport(int reportID,  string wcName, DateTime rDate, int shift, SqlConnection sqlCon)
        {
            MachineName = wcName;
            ReportDate = rDate;
            Shift = shift;
            RoundTable = new DataTable();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT [Time], [RoundNbr], [Quantity], [Notes] FROM [dbo].[PRM-CSTM]_Round WHERE [ReportID] = @p1", sqlCon))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", reportID);
                        adapter.Fill(RoundTable);
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
        /// Get a list of all the press shift reports for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="machineName">Machine Name</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of Press_ShiftReport objects</returns>
        public static List<Press_ShiftReport> GetPress_ShiftReportList(string woNbr, string machineName, SqlConnection sqlCon)
        {
            var _tempList = new List<Press_ShiftReport>();
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
                                    _tempList.Add(new Press_ShiftReport {
                                        ReportID = reader.SafeGetInt32("ReportID"),
                                        ReportDate = reader.SafeGetDateTime("SubmitDate"),
                                        Shift = reader.SafeGetInt32("Shift"),
                                        ReportStatus = reader.SafeGetString("Status"),
                                        MachineName = machineName,
                                        RoundTable = GetRoundTable(reader.SafeGetInt32("ReportID"), sqlCon)
                                    });
                                }
                                ModelBase.ModelSqlCon = sqlCon;
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
        /// Fill a DataTable object with the round values located in the Sql Database
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable object</returns>
        public static DataTable GetRoundTable(int reportID, SqlConnection sqlCon)
        {
            using (DataTable table = new DataTable())
            {
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                            SELECT [Time]
                                                                                ,[RoundNbr] as 'Round'
                                                                                ,[Quantity]
                                                                                ,[QualityFlg] as 'Flag'
                                                                                ,[Notes]
                                                                                ,[RollNbr] as 'Roll'
                                                                            FROM [dbo].[PRM-CSTM_Round]
                                                                            WHERE [ReportID] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", reportID);
                            adapter.Fill(table);
                        }
                        return table;
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

        public void Update(string woNbr, Press_ShiftReport psReport, SqlConnection sqlCon)
        {

        }

        /// <summary>
        /// Submit or post a round to the SQL database
        /// </summary>
        ///<param name="pReport">Press report object</param>
        ///<param name="psReport">Press shift report object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void SubmitRound(PressReport pReport, Press_ShiftReport psReport, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    //Variable initialization
                    var _time = DateTime.Now.ToString("HH:mm");
                    var _slats = Machine.GetPress_Length(sqlCon, psReport.MachineName) - pReport.SlatTransfer;
                    var _rollNbr = 0;
                    var _qty = 0;
                    var _rndNbr = 0;
                    var _cut = false;

                    //SQL Query to grab any information about the previous rounds
                    //All returned data is used in the calculations for round number, quantity complete and roll number
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                DECLARE @rndNbr int;
                                                                DECLARE @qtyComp int;
                                                                SET @rndNbr = (SELECT COUNT(DISTINCT([RoundNbr])) FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1);
                                                                SET @qtyComp = (SELECT [Quantity] FROM [dbo].[PRM-CSTM_Round] WHERE [RoundNbr] = @rndNbr AND [ReportID] = @p1);
                                                                SELECT
	                                                                MAX([RollNbr]) as 'RollNbr',
	                                                                @qtyComp as 'QtyComp',
	                                                                @rndNbr + 1 as 'RoundNbr'
                                                                FROM
	                                                                [dbo].[PRM-CSTM_Round]
                                                                WHERE
	                                                                [ReportID] = @p1 AND [RollNbr] = (SELECT MAX([RollNbr]) FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _rollNbr = reader.SafeGetInt32("RollNbr");
                                    _qty = reader.SafeGetInt32("QtyComp");
                                    _rndNbr = reader.SafeGetInt32("RoundNbr");
                                }
                            }
                        }
                    }

                    //Check to see if this is the first round submitted for a shift
                    //If it is the first round, SQL query to find out the previous shifts max quantity completed and roll number
                    if (_rndNbr == 1)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                    DECLARE @repID int;
                                                                    SET @repID = (SELECT TOP(1) [ReportID] FROM [PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 AND [Shift] = @p2 ORDER BY [ReportID] DESC);
                                                                    SELECT
	                                                                    MAX([RollNbr]) as 'RollNbr',
	                                                                    SUM([Quantity]) as 'QtyComp'
                                                                    FROM
	                                                                    [dbo].[PRM-CSTM_Round]
                                                                    WHERE
	                                                                    [ReportID] = @repID AND [RollNbr] = (SELECT MAX([RollNbr]) FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @repID);", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", pReport.ShopOrder.OrderNumber);
                            cmd.Parameters.AddWithValue("p2", pReport.ShiftReportList.Count == 1 ? pReport.ShiftReportList[0].Shift : pReport.ShiftReportList[1].Shift);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        _rollNbr = reader.SafeGetInt32("RollNbr");
                                        _qty = reader.SafeGetInt32("QtyComp");
                                    }
                                }
                            }
                        }
                    }

                    //Roll length calculation along with the SQL insert command
                    //There is also a check to see if the current submission quantity is more than what the work order has determined as the max length
                    //If there is more than the determined max length then there is a second calculation along with a second SQL insert command
                    var _tempLen = pReport.RollLength;
                    var _tempSlat = pReport.SlatTransfer;
                    if (_qty + _slats < _tempLen)
                    {
                        _tempLen = _qty + _slats;
                        _tempSlat = _slats;
                    }
                    else
                    {
                        _tempSlat = _slats - ((_qty + _slats) - _tempLen);
                        _cut = true;
                    }
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNbr], [Time], [Quantity], [RollNbr])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                        cmd.Parameters.AddWithValue("p2", _rndNbr);
                        cmd.Parameters.AddWithValue("p3", _time);
                        cmd.Parameters.AddWithValue("p4", _tempLen);
                        cmd.Parameters.AddWithValue("p5", _rollNbr);
                        cmd.ExecuteNonQuery();
                    }
                    if (_cut)
                    {
                        _rollNbr++;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNbr], [Time], [Quantity], [RollNbr], [Notes])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                            cmd.Parameters.AddWithValue("p2", _rndNbr);
                            cmd.Parameters.AddWithValue("p3", _time);
                            cmd.Parameters.AddWithValue("p4", (_qty + _slats) - _tempLen);
                            cmd.Parameters.AddWithValue("p5", _rollNbr);
                            cmd.Parameters.AddWithValue("p6", "**New Roll**");
                            cmd.ExecuteNonQuery();
                        }
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

        public static void UpdateRound(SqlConnection sqlCon)
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
