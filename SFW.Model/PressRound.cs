using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace SFW.Model
{
    public class PressRound : ModelBase
    {
        #region Properties

        public int RoundNumber { get; set; }

        public TimeSpan RoundTime { get; set; }

        private int _qty;
        public int Quantity
        {
            get => _qty;
            set
            {
                HasChanges = _qty != value;
                _qty = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public int Pieces { get; set; }

        private string _note;
        public string Notes
        {
            get => _note;
            set
            {
                HasChanges = _note != value;
                _note = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public int RollNumber { get; set; }

        private string _flg;
        public string QualityFlag
        {
            get => _flg;
            set
            {
                HasChanges = _flg != value;
                _flg = value;
                OnPropertyChanged(nameof(QualityFlag));
            }
        }

        private bool _hasChg;
        public bool HasChanges
        {
            get => _hasChg;
            set
            {
                _hasChg = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PressRound()
        { }

        /// <summary>
        /// Get a list of rounds associated with the press shift report
        /// Set up as a binding list to watch for changes
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of press rounds as a binding list</returns>
        public static BindingList<PressRound> GetRoundList(int reportId, SqlConnection sqlCon)
        {
            var _tempList = new BindingList<PressRound>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                    SELECT [Time]
                                                                        ,[RoundNbr] as 'Round'
                                                                        ,[Quantity]
                                                                        ,[QualityFlg] as 'Flag'
                                                                        ,[Notes]
                                                                        ,[RollNbr] as 'Roll'
                                                                        ,[Pieces]
                                                                    FROM [dbo].[PRM-CSTM_Round]
                                                                    WHERE [ReportID] = @p1
                                                                    ORDER BY [RoundNbr]", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reportId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new PressRound
                                    {
                                        RoundTime = reader.GetTimeSpan(0)
                                        ,RoundNumber = reader.SafeGetInt32("Round")
                                        ,Quantity = reader.SafeGetInt32("Quantity")
                                        ,QualityFlag = reader.SafeGetString("Flag")
                                        ,Notes = reader.SafeGetString("Notes")
                                        ,RollNumber = reader.SafeGetInt32("Roll")
                                        ,Pieces = reader.SafeGetInt32("Pieces")
                                        ,HasChanges = false
                                    });
                                }
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
        /// Update any round data with manually enter inputs
        /// </summary>
        /// <param name="reportID">ID of the parent report header</param>
        /// <param name="rndTable">Round table to update</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Update(int reportID, BindingList<PressRound> rndList, SqlConnection sqlCon)
        {
            foreach (var round in rndList.Where(o => o.HasChanges))
            {
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                UPDATE
                                                                    [dbo].[PRM-CSTM_Round]
                                                                SET
                                                                    [Quantity] = @p1, [Notes] = @p2, [QualityFlg] = @p3
                                                                WHERE
                                                                    [ReportID] = @p4 AND [RoundNbr] = @p5", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", round.Quantity);
                            cmd.Parameters.AddWithValue("p2", round.Notes);
                            cmd.Parameters.AddWithValue("p3", round.QualityFlag);
                            cmd.Parameters.AddWithValue("p4", reportID);
                            cmd.Parameters.AddWithValue("p5", round.RoundNumber);
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

        /// <summary>
        /// Submit or post a round to the SQL database
        /// </summary>
        ///<param name="pReport">Press report object</param>
        ///<param name="psReport">Press shift report object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Submit(PressReport pReport, PressShiftReport psReport, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    //Variable initialization
                    var _time = DateTime.Now.ToString("HH:mm:ss");
                    var _slats = Machine.GetPress_Length(sqlCon, psReport.MachineName) - int.Parse(pReport.SlatTransfer.ToString()) + 1;
                    var _blankOut = pReport.DoubleBlankout ? int.Parse(pReport.SlatBlankout.ToString()) * 2 + 1 : int.Parse(pReport.SlatBlankout.ToString());
                    var _rollLen = int.Parse(pReport.RollLength.ToString());
                    var _rollNbr = 1;
                    var _qty = 0;
                    var _pcs = 0;
                    var _rndNbr = 1;
                    var _cut = false;
                    var _isNew = false;
                    var _isEA = pReport.ShopOrder.Uom == "EA";

                    //SQL Query to grab any information about the previous rounds
                    //All returned data is used in the calculations for round number, quantity complete and roll number
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT TOP(1) * FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1 ORDER BY [RollNbr] DESC, [RoundNbr] DESC;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _rollNbr = reader.SafeGetInt32("RollNbr");
                                    _qty = reader.SafeGetInt32("Quantity");
                                    _pcs = reader.SafeGetInt32("Pieces");
                                    _rndNbr = reader.SafeGetInt32("RoundNbr") + 1;
                                }
                            }
                            else
                            {
                                _isNew = true;
                            }
                        }
                    }

                    //Check to see if this is the first round submitted for a shift
                    //If it is the first round, SQL query to find out the previous shifts max quantity completed and roll number
                    if (_isNew)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                    SELECT TOP(1)
	                                                                    [RollNbr]
	                                                                    ,[Quantity]
                                                                        ,[Pieces]
                                                                    FROM
	                                                                    [dbo].[PRM-CSTM_Round]
                                                                    WHERE
	                                                                    [ReportID] = @p1
                                                                    ORDER BY
	                                                                    [RollNbr] DESC, [RoundNbr] DESC;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", pReport.ShiftReportList.Count == 1 ? pReport.ShiftReportList[0].ReportID : pReport.ShiftReportList[1].ReportID);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        _rollNbr = reader.SafeGetInt32("RollNbr");
                                        _qty = reader.SafeGetInt32("Quantity");
                                        _pcs = reader.SafeGetInt32("Pieces");
                                    }
                                }
                            }
                        }
                    }

                    //Roll length calculation along with the SQL insert command
                    //There is also a check to see if the current submission quantity is more than what the work order has determined as the max length
                    //If there is more than the determined max length then there is a second calculation along with a second SQL insert command
                    var _tempLen = _qty + _slats;
                    var _cutLen = 0;
                    var _cutPcs = 0;

                    if (_isEA)
                    {
                        var _lenStr = new Regex("([0-9]+)X([0-9]+)").Match(pReport.ShopOrder.SkuDescription.Replace(" ", "")).Value;
                        var _tempSkuLen = !string.IsNullOrEmpty(_lenStr) && int.TryParse(_lenStr.Substring(_lenStr.IndexOf("X") + 1), out int i) ? i / 12 + _blankOut : 0;
                        if (_tempLen > _tempSkuLen && _tempSkuLen > 0)
                        {
                            while (_tempLen > _tempSkuLen)
                            {
                                _pcs++;
                                _cut = _pcs >= _rollLen;
                                if (_cut)
                                {
                                    _cutLen = _tempLen - _tempSkuLen;
                                    _tempLen = _cutLen;
                                    _cutPcs = _pcs - _rollLen;
                                }
                                else
                                {
                                    _tempLen -= _tempSkuLen;
                                }
                            }
                        }
                        _pcs = _cut ? _rollLen : _pcs;
                    }
                    else
                    {
                        _cut = _tempLen > _rollLen;
                        _cutLen = _cut ? _tempLen - _rollLen : 0;
                    }

                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNbr], [Time], [Quantity], [Pieces], [RollNbr])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                        cmd.Parameters.AddWithValue("p2", _rndNbr);
                        cmd.Parameters.AddWithValue("p3", _time);
                        cmd.Parameters.AddWithValue("p4", _tempLen - _cutLen);
                        cmd.Parameters.AddWithValue("p5", _pcs);
                        cmd.Parameters.AddWithValue("p6", _rollNbr);
                        cmd.ExecuteNonQuery();
                    }
                    if (_cut)
                    {
                        _rollNbr++;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                INSERT INTO
                                                                    [dbo].[PRM-CSTM_Round] ([ReportID], [RoundNbr], [Time], [Quantity], [Pieces], [RollNbr], [Notes])
                                                                VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", psReport.ReportID);
                            cmd.Parameters.AddWithValue("p2", _rndNbr);
                            cmd.Parameters.AddWithValue("p3", _time);
                            cmd.Parameters.AddWithValue("p4", _cutLen);
                            cmd.Parameters.AddWithValue("p5", _cutPcs);
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

        /// <summary>
        /// Submit or post a round to the SQL database
        /// </summary>
        ///<param name="reportID">Report ID</param>
        ///<param name="roundNbr">Round Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Delete(int reportID, int roundNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    //SQL Query to delete the row that matches the report id and round number
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                DELETE FROM [dbo].[PRM-CSTM_Round] WHERE [ReportID] = @p1 AND [RoundNbr] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reportID);
                        cmd.Parameters.AddWithValue("p2", roundNbr);
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
