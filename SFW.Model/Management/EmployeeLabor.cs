using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Management
{
    public class EmployeeLabor : ModelBase, IModuleData
    {
        #region Properties

        public string LaborId { get; set; }
        public int Shift { get; set; }
        public string OutTime { get; set; }

        private int _dateId;
        public int DateId
        { 
            get
            { return _dateId; }
            set
            {
                if (value == -1)
                {
                    //Wahpeton shift 1 and 2 along with Arlington shift 1 logic
                    //No changes to the date are nessesary
                    if (Shift < 3 || Shift == 4)
                    {
                        value = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                    }
                    //Wahpeton shift 3 logic when time of day is greater than 21:00 then next day
                    //Arlington shift 2 logic when time of day is less than 14:00 then previous day
                    if (TimeSpan.TryParse(InTime, out TimeSpan t))
                    {
                        var _addDay = (Shift == 3 && DateTime.Now.Hour > 21) || (Shift == 5 && DateTime.Now.Hour < 14) ? 4 - Shift : 0;
                        var _conDate = _addDay == 0 ? DateTime.Today.AddDays(_addDay) : DateTime.Today;
                        value = (_conDate - Convert.ToDateTime("1967/12/31")).Days;
                    }
                    else
                    {
                        value = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                    }
                }
                _dateId = value;
                OnPropertyChanged(nameof(DateId));
            }
        }

        private string _inTime;
        public string InTime
        {
            get
            { return _inTime; }
            set
            {
                _inTime = value;
                OnPropertyChanged(nameof(InTime));
            }
        }

        public DateTime InDate
        {
            get
            {
                //Wahpeton shift 1 and 2 along with Arlington shift 1 logic
                //No changes to the date are nessesary
                if (Shift < 3 || Shift == 4)
                {
                    return DateTime.Today;
                }
                //Wahpeton shift 3 logic when in time is after 21:00 but time of day is less than 21:00 then previous day
                //Arlington shift 2 logic when in time is after 14:00 but time of day is less than 14:00 then previous day
                if (TimeSpan.TryParse(InTime, out TimeSpan t))
                {
                    var _hour = Shift == 3 ? 21 : 14;
                    return t.Hours > _hour && DateTime.Now.Hour < _hour ? DateTime.Today.AddDays(-1) : DateTime.Today;
                }
                else
                {
                    return DateTime.Today;
                }
            }
        }

        #endregion

        #region Data Access

        /// <summary>
        /// Get a table of all the staff labor on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of staff members labor</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[SFW_StaffErpLabor] WHERE [Site] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
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

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EmployeeLabor()
        { }

        /// <summary>
        /// Retreives a new employee labor object
        /// </summary>
        /// <param name="erpId">User ERP ID number</param>
        /// <returns>employee labor object or null</returns>
        public static EmployeeLabor GetLabor(string erpId, int shift)
        {
            try
            {
                var _dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                if (shift == 3 && DateTime.Now.Hour > 9)
                {
                    _dateId++;
                }
                else if (shift == 5 && DateTime.Now.Hour < 4)
                {
                    _dateId--;
                }
                var _rows = MasterDataSet.Tables[typeof(EmployeeLabor).Name].Select($"[ErpId] = '{erpId}' AND [DateId] = {_dateId}", "[OutTime] DESC");
                if (_rows.Count() > 0)
                {
                    return new EmployeeLabor
                    {
                        LaborId = _rows.FirstOrDefault().Field<string>("LaborId"),
                        Shift = _rows.FirstOrDefault().Field<int>("Shift"),
                        DateId = _rows.FirstOrDefault().Field<int>("DateId"),
                        InTime = _rows.FirstOrDefault().Field<string>("OutTime"),
                        OutTime = DateTime.Now.ToString("HH:mm")
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retreives a new employee labor object
        /// </summary>
        /// <param name="erpId">User ERP ID number</param>
        /// <returns>List of labor entries from an employee</returns>
        public static List<EmployeeLabor> GetLaborList(string erpId)
        {
            var _rtnList = new List<EmployeeLabor>();
            try
            {
                var _rows = MasterDataSet.Tables[typeof(EmployeeLabor).Name].Select($"[ErpId] = '{erpId}'");
                foreach (var _row in _rows)
                {
                    _rtnList.Add(new EmployeeLabor
                    {
                        LaborId = _rows.FirstOrDefault().Field<string>("LaborId"),
                        Shift = _rows.FirstOrDefault().Field<int>("Shift"),
                        DateId = _rows.FirstOrDefault().Field<int>("DateId"),
                        InTime = _rows.FirstOrDefault().Field<string>("InTime"),
                        OutTime = _rows.FirstOrDefault().Field<string>("OutTime")
                    });
                }
                return _rtnList;
            }
            catch
            {
                return _rtnList;
            }
        }
    }
}
