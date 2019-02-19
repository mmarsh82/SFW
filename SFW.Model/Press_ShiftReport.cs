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

        public int Shift { get; set; }
        public DateTime ReportDate { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Press_ShiftReport()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subFName"></param>
        /// <param name="subLName"></param>
        /// <param name="wcName"></param>
        /// <param name="rDate"></param>
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
            ModelBase.ModelSqlCon = sqlCon;
            CrewList = new BindingList<CrewMember>
                {
                    new CrewMember { IdNumber = CrewMember.GetCrewIdNumber(sqlCon, subFName, subLName), Name = $"{subFName} {subLName}" }
                };
            CrewList.AddNew();
            CrewList.ListChanged += CrewList_ListChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportID"></param>
        /// <param name="wcName"></param>
        /// <param name="rDate"></param>
        /// <param name="shift"></param>
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        public Press_ShiftReport (int reportID, string wcName, DateTime rDate, int shift, SqlConnection sqlCon)
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

        public static int SubmitReportStart(WorkOrder wo, PressReport pReport, int index, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _idNumber = 0;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM] ([WorkOrder], [SlatTransfer], [RollLength], [SubmitDate], [Shift], [MachineName])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)
                                                                SELECT [ID] FROM [dbo].[PRM-CSTM] WHERE [ID] = @@IDENTITY;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                        cmd.Parameters.AddWithValue("p2", pReport.SlatTransfer);
                        cmd.Parameters.AddWithValue("p3", pReport.RollLength);
                        cmd.Parameters.AddWithValue("p4", pReport.ShiftReportList[index].ReportDate);
                        cmd.Parameters.AddWithValue("p5", pReport.ShiftReportList[index].Shift);
                        cmd.Parameters.AddWithValue("p6", pReport.ShiftReportList[index].MachineName);
                        _idNumber = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    var cmdString = $@"USE { sqlCon.Database};";
                    var _counter = 1;
                    using (SqlCommand cmd = new SqlCommand(cmdString, sqlCon))
                    {
                        foreach (var s in pReport.ShiftReportList[index].CrewList)
                        {
                            cmd.CommandText += $" INSERT INTO [dbo].[PRM-CSTM]_Crew ([ReportID], [UserID]) VALUES(@p{_counter}, @p{_counter + 1});";
                            cmd.Parameters.AddWithValue($"p{_counter}", _idNumber);
                            cmd.Parameters.AddWithValue($"p{_counter + 1}", s.IdNumber);
                            _counter = _counter + 2;
                        }
                        cmd.ExecuteNonQuery();
                    }
                    return _idNumber;
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
        public static void SubmitRound(SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM] ([WorkOrder], [SlatTransfer], [RollLength], [SubmitDate], [Shift], [MachineName])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)
                                                                SELECT [ID] FROM [dbo].[PRM-CSTM] WHERE [ID] = @@IDENTITY;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", "WorkOrder#");
                        cmd.Parameters.AddWithValue("p2", "Slat");
                        cmd.Parameters.AddWithValue("p3", "Length");
                        cmd.Parameters.AddWithValue("p4", "");
                        cmd.Parameters.AddWithValue("p5", "");
                        cmd.Parameters.AddWithValue("p6", "");
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
        public static void UpdateReport(SqlConnection sqlCon)
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
