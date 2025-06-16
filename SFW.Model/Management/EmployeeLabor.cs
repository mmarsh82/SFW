using System;
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
        public int DateId
        {
            get
            { return DateTime.TryParse(OutDate, out DateTime dt) ? (dt - Convert.ToDateTime("1967/12/31")).Days : (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days; }
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

        public string InDate
        {
            get
            {
                if (Shift != 3 && Shift != 5)
                {
                    return DateTime.Today.ToString("MM-dd-yyyy");
                }
                else
                {
                    var _time = Shift == 3 ? TimeSpan.Parse("21:00") : TimeSpan.Parse("14:00");
                    if (DateTime.Now.TimeOfDay > _time)
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        return DateTime.Today.AddDays(-1).ToString("MM-dd-yyyy");
                    }
                }
            }
        }

        public string OutTime
        {
            get 
            { return DateTime.Now.ToString("HH:mm"); }
        }

        public string OutDate
        {
            get
            {
                if (Shift != 3 && Shift != 5)
                {
                    return DateTime.Today.ToString("MM-dd-yyyy");
                }
                else
                {
                    var _time = Shift == 3 ? TimeSpan.Parse("21:00") : TimeSpan.Parse("14:00");
                    if (DateTime.Now.TimeOfDay > _time)
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        return DateTime.Today.AddDays(-1).ToString("MM-dd-yyyy");
                    }
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
                        var _dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days - 2;
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[SFW_StaffErpLabor] WHERE [DateId] >= @p1 AND [Site] = @p2", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", _dateId);
                            adapter.SelectCommand.Parameters.AddWithValue("p2", site);
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
        /// Overridded Constructor
        /// </summary>
        /// <param name="erpId">ERP ID of the employee</param>
        /// <param name="shift">Employee labor shift</param>
        /// <param name="site">Facility ID for the employee</param>
        public EmployeeLabor(string erpId, int shift, int site, string shiftStart)
        {
            Shift = shift;
            LaborId = $"{erpId}*{DateId}*0{site}";
            var _in = GetInTime(LaborId);
            InTime = !string.IsNullOrEmpty(_in) ? _in : shiftStart;
        }

        /// <summary>
        /// Retreives the last labor clocked in time for the current user
        /// </summary>
        /// <param name="crewId">User ID number</param>
        /// <param name="facCode">Facility code</param>
        /// <param name="dateId">ERP Date ID in unix time to be used for query</param>
        /// <returns>Time as a string</returns>
        public static string GetInTime(string laborId)
        {
            var _rows = MasterDataSet.Tables[new EmployeeLabor().GetType().Name].Select($"[LaborID] = '{laborId}'", "[OutTime] DESC");
            return _rows.Length > 0 ? _rows.FirstOrDefault().Field<string>("OutTime") : string.Empty;
        }
    }
}
