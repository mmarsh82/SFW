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
        /// <param name="sqlCon"></param>
        /// <param name="shift"></param>
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
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
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

        public Press_ShiftReport (int reportID,  string wcName, DateTime rDate, int shift, SqlConnection sqlCon)
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
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "IdNumber")
            {
                ((BindingList<CrewMember>)sender)[e.NewIndex].Name = string.Empty;
                var _dName = CrewMember.GetCrewDisplayName(ModelBase.ModelSqlCon, Convert.ToInt32(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber));
                var _duplicate = ((BindingList<CrewMember>)sender).Any(o => o.Name == _dName);
                if (!string.IsNullOrEmpty(_dName) && !_duplicate)
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Name = _dName;
                    if (((BindingList<CrewMember>)sender).Count == e.NewIndex + 1)
                    {
                        ((BindingList<CrewMember>)sender).AddNew();
                    }
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

        public static void SubmitRound(DataRow dRow, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM]_Round ([ReportID], [RoundNumber], [Time], [QtyComplete], [RoundSlats], [Notes])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", "ReportID");
                        cmd.Parameters.AddWithValue("p2", "RoundNumber");
                        cmd.Parameters.AddWithValue("p3", "Time");
                        cmd.Parameters.AddWithValue("p4", "Qty");
                        cmd.Parameters.AddWithValue("p5", "Slats");
                        cmd.Parameters.AddWithValue("p6", "Notes");
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
