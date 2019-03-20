using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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
        /// Wip receipt crew list to use for the labor part of the transaction
        /// </summary>
        public BindingList<CrewMember> CrewList { get; set; }

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
        /// </summary>
        /// <param name="subFName">Current user full name</param>
        /// <param name="subLName">Current user last name</param>
        /// <param name="wcName">Machine name</param>
        /// <param name="rDate">Report or submit date</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="shift">Optional: shift for this report</param>
        public Press_ShiftReport(string subFName, string subLName, string wcName, DateTime rDate, SqlConnection sqlCon, int shift = 0)
        {
            if (shift == 0)
            {
                if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 15)
                {
                    shift = 1;
                }
                else if (DateTime.Now.Hour >= 15 && DateTime.Now.Hour < 23)
                {
                    shift = 2;
                }
                else
                {
                    shift = 3;
                }
            }
            Shift = shift;
            ReportDate = rDate;
            MachineName = wcName;
            ReportStatus = "O";
            ModelBase.ModelSqlCon = sqlCon;
            CrewList = new BindingList<CrewMember>
                {
                    new CrewMember { IdNumber = CrewMember.GetCrewIdNumber(sqlCon, subFName, subLName), Name = $"{subFName} {subLName}" }
                };
            CrewList.AddNew();
            CrewList.ListChanged += CrewList_ListChanged;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                            SELECT TOP (0) [Time]
                                                                                ,[RoundNumber] as 'Round Number'
                                                                                ,[QtyComplete] as 'Qty Done'
                                                                                ,[RoundSlats] as 'Slats per Round'
                                                                                ,[Notes]
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
            CrewList = new BindingList<CrewMember>();
            RoundTable = new DataTable();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT [UserID] FROM [dbo].[PRM-CSTM_Crew] WHERE [ReportID] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reportID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    CrewList.Add(new CrewMember { IdNumber = reader.SafeGetInt32("UserID"), IsDirect = true, Name = CrewMember.GetCrewDisplayName(sqlCon, reader.SafeGetInt32("UserID")) });
                                }
                                CrewList.AddNew();
                                CrewList.ListChanged += CrewList_ListChanged;
                            }
                        }
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT [Time], [RoundNumber], [QtyComplete], [RoundSlats], [Notes] FROM [dbo].[PRM-CSTM]_Round WHERE [ReportID] = @p1", sqlCon))
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
        /// Happens when an item is added or changed in the CrewMember Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_ListChanged(object sender, ListChangedEventArgs e)
        {
            //TODO: need to add in the ability to post labor here or set up a que so that the update know what to post
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "IdNumber")
            {
                ((BindingList<CrewMember>)sender)[e.NewIndex].Name = string.Empty;
                var _dName = CrewMember.GetCrewDisplayName(ModelBase.ModelSqlCon, Convert.ToInt32(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber));
                var _duplicate = ((BindingList<CrewMember>)sender).Any(o => o.Name == _dName);
                if (!string.IsNullOrEmpty(_dName) && !_duplicate)
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Name = _dName;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].IsDirect = CrewMember.GetProductionStatus(Convert.ToInt32(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber), ModelBase.ModelSqlCon);
                    if (((BindingList<CrewMember>)sender).Count == e.NewIndex + 1)
                    {
                        ((BindingList<CrewMember>)sender).AddNew();
                    }
                }
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
                                        CrewList = CrewMember.GetCrewBindingList(reader.SafeGetInt32("ReportID"), sqlCon),
                                        RoundTable = GetRoundTable(reader.SafeGetInt32("ReportID"), sqlCon)
                                    });
                                }
                                _tempList[0].CrewList.AddNew();
                                _tempList[0].CrewList.ListChanged += _tempList[_tempList.Count - 1].CrewList_ListChanged;
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT [Time], [RoundNumber], [QtyComplete], [RoundSlats], [Notes] FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1", sqlCon))
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

        /// <summary>
        /// Submit the Press shift report object to a SQL Database
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <param name="psReport">Press shift report object to submit</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Report unique ID</returns>
        public int Submit(string woNbr, Press_ShiftReport psReport, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    //Validation to see if this is a duplicate entry and close out any open reports from the origin work order
                    var oldID = 0;
                    using (SqlCommand cmd = new SqlCommand(@"DECLARE @dup int;
                                                                SET @dup = (SELECT COUNT([ReportID]) FROM [dbo].[PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 AND [Shift] = @p2 AND [Status] = 'O');
                                                                SELECT 
	                                                                CASE WHEN (SELECT [ReportID] FROM [dbo].[PRM-CSTM_Shift] WHERE EXISTS (SELECT * FROM [dbo].[PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 AND [Status] = 'O' AND [Shift] != @p2)) > 0
		                                                                THEN
			                                                                (SELECT [ReportID] FROM [dbo].[PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 AND [Status] = 'O' AND [Shift] != @p2)
		                                                                ELSE
			                                                                0
		                                                                END as 'Open',
	                                                                @dup as 'Duplicate';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", psReport.Shift);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.SafeGetInt32("Duplicate") == 1)
                                    {
                                        return -1;
                                    }
                                    oldID = reader.SafeGetInt32("Open");
                                }
                            }
                        }
                    }
                    if (oldID > 0)
                    {
                        var prevShift = psReport.Shift == 1 ? 3 : psReport.Shift - 1;
                        using (SqlCommand cmd = new SqlCommand(@"UPDATE [dbo].[PRM-CSTM_Shift] SET [Status] = @p1 WHERE [ReportID] = @p2 AND [WorkOrder] = @p3 AND [Shift] = @p4", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", "C");
                            cmd.Parameters.AddWithValue("p2", oldID);
                            cmd.Parameters.AddWithValue("p3", woNbr);
                            cmd.Parameters.AddWithValue("p4", prevShift);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    //Insertion of the new report sheet data into the Sql database
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO [dbo].[PRM-CSTM_Shift] ([SubmitDate], [WorkOrder], [Shift], [Status]) VALUES(@p1, @p2, @p3, @p4);
                                                                SELECT [ReportID] FROM [PRM-CSTM_Shift] WHERE [ReportID] = @@IDENTITY;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportDate);
                        cmd.Parameters.AddWithValue("p2", woNbr);
                        cmd.Parameters.AddWithValue("p3", psReport.Shift);
                        cmd.Parameters.AddWithValue("p4", psReport.ReportStatus);
                        psReport.ReportID = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    //Writing the Crew to the database in 1 query, requires the parsing of the the crewlist property
                    var cmdString = $@"USE { sqlCon.Database};";
                    var _counter = 1;
                    using (SqlCommand cmd = new SqlCommand(cmdString, sqlCon))
                    {
                        foreach (var s in psReport.CrewList)
                        {
                            if (s.IdNumber != null)
                            {
                                cmd.CommandText += $" INSERT INTO [dbo].[PRM-CSTM_Crew] ([ReportID], [UserID]) VALUES(@p{_counter}, @p{_counter + 1});";
                                cmd.Parameters.AddWithValue($"p{_counter}", psReport.ReportID);
                                cmd.Parameters.AddWithValue($"p{_counter + 1}", s.IdNumber);
                                _counter = _counter + 2;
                            }
                        }
                        cmd.ExecuteNonQuery();
                    }
                    return Convert.ToInt32(psReport.ReportID);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
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
                    var _time = DateTime.Now.TimeOfDay;
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
                                                                SET @rndNbr = (SELECT COUNT(DISTINCT([RoundNumber])) FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1);
                                                                SET @qtyComp = (SELECT [QtyComplete] FROM [dbo].[PRM-CSTM_Round] WHERE [RoundNumber] = @rndNbr AND [ReportID] = @p1);
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
                        var _prevShift = psReport.Shift == 1 ? psReport.Shift + 2 : psReport.Shift - 1;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                    DECLARE @repID int;
                                                                    SET @repID = (SELECT TOP(1) [ReportID] FROM [PRM-CSTM_Shift] WHERE [WorkOrder] = @p1 AND [Shift] = @p2 ORDER BY [ReportID] DESC);
                                                                    SELECT
	                                                                    MAX([RollNbr]) as 'RollNbr',
	                                                                    SUM([RoundSlats]) as 'QtyComp'
                                                                    FROM
	                                                                    [dbo].[PRM-CSTM_Round]
                                                                    WHERE
	                                                                    [ReportID] = @repID AND [RollNbr] = (SELECT MAX([RollNbr]) FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @repID);", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", pReport.ShopOrder.OrderNumber);
                            cmd.Parameters.AddWithValue("p2", _prevShift);
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
                    //There is also a check to see if the current submission quantity is more that what the work order has determined as the max length
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
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNumber], [Time], [QtyComplete], [RoundSlats], [RollNbr])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                        cmd.Parameters.AddWithValue("p2", _rndNbr);
                        cmd.Parameters.AddWithValue("p3", _time);
                        cmd.Parameters.AddWithValue("p4", _tempLen);
                        cmd.Parameters.AddWithValue("p5", _tempSlat);
                        cmd.Parameters.AddWithValue("p6", _rollNbr);
                        cmd.ExecuteNonQuery();
                    }
                    if (_cut)
                    {
                        _rollNbr++;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNumber], [Time], [QtyComplete], [RoundSlats], [RollNbr], [Notes])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                            cmd.Parameters.AddWithValue("p2", _rndNbr);
                            cmd.Parameters.AddWithValue("p3", _time);
                            cmd.Parameters.AddWithValue("p4", (_qty + _slats) - _tempLen);
                            cmd.Parameters.AddWithValue("p5", _slats - _tempSlat);
                            cmd.Parameters.AddWithValue("p6", _rollNbr);
                            cmd.Parameters.AddWithValue("p7", "**New Roll**");
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
