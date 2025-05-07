using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace SFW.Model
{
    public class CrewMember : ModelBase, IComparable
    {
        #region Properties

        private string _erpId;
        public string ErpId
        {
            get { return _erpId; }
            set { _erpId = value; OnPropertyChanged(nameof(ErpId)); }
        }

        private int _sapId;
        public int SapId
        {
            get { return _sapId; }
            set { _sapId = value; OnPropertyChanged(nameof(SapId)); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private bool _isDirect;
        public bool IsDirect
        {
            get
            { return _isDirect; }
            set
            { _isDirect = value; OnPropertyChanged(nameof(IsDirect)); }
        }

        private int _shift;
        public int Shift
        {
            get
            { return _shift; }
            set
            { _shift = value; OnPropertyChanged(nameof(Shift)); }
        }

        private string _shiftStart;
        public string ShiftStart
        {
            get
            { return _shiftStart; }
            set
            { _shiftStart = value; OnPropertyChanged(nameof(ShiftStart)); }
        }

        private string _shiftEnd;
        public string ShiftEnd
        {
            get
            { return _shiftEnd; }
            set
            { _shiftEnd = value; OnPropertyChanged(nameof(Shift)); }
        }

        private string _fac;
        public string Facility
        {
            get { return _fac; }
            set { _fac = value; OnPropertyChanged(nameof(Facility)); }
        }

        private string _errMsg;
        public string ErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private string _inTime;
        public string InTime
        {
            get
            { return _inTime; }
            set
            {
                if (IsDirect && !_clockLoaded && string.IsNullOrEmpty(_inTime))
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        if (!string.IsNullOrEmpty(Facility) && IsCrewErpIdValid(ErpId))
                        {
                            Facility = GetFacility(ErpId);
                        }
                        var dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                        var _time = GetInTime(ErpId, Facility, dateId);
                        if (_time.Contains("ERR"))
                        {
                            _inTime = string.Empty;
                            _clockLoaded = false;
                            ErrorMessage = _time.Replace("ERR:", "");
                        }
                        else if (Shift == 3)
                        {
                            if (string.IsNullOrEmpty(_time))
                            {
                                dateId = (DateTime.Today.AddDays(-1) - Convert.ToDateTime("1967/12/31")).Days;
                                _time = GetInTime(ErpId, Facility, dateId);
                                if (string.IsNullOrEmpty(_time))
                                {
                                    _time = GetShiftStartTime(ErpId);
                                    _clockLoaded = true;
                                }
                                else
                                {
                                    if(TimeSpan.Parse(_time) < TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00"))
                                    {
                                        _time = GetShiftStartTime(ErpId);
                                        _clockLoaded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (TimeSpan.Parse(_time) < TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00"))
                                {
                                    _time = GetShiftStartTime(ErpId);
                                    _clockLoaded = true;
                                }
                            }
                            _inTime = value = _time;
                        }
                        else if (Shift == 5)
                        {
                            if (string.IsNullOrEmpty(_time))
                            {
                                dateId = (DateTime.Today.AddDays(-1) - Convert.ToDateTime("1967/12/31")).Days;
                                _time = GetInTime(ErpId, Facility, dateId);
                                if (string.IsNullOrEmpty(_time))
                                {
                                    _time = GetShiftStartTime(ErpId);
                                    _clockLoaded = true;
                                }
                                else
                                {
                                    if (TimeSpan.Parse(_time) < TimeSpan.Parse("14:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("14:00"))
                                    {
                                        _time = GetShiftStartTime(ErpId);
                                        _clockLoaded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (TimeSpan.Parse(_time) < TimeSpan.Parse("14:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("14:00"))
                                {
                                    _time = GetShiftStartTime(ErpId);
                                    _clockLoaded = true;
                                }
                            }
                            _inTime = value = _time;
                        }
                        else
                        {
                            try
                            {
                                _time = string.IsNullOrEmpty(_time) ? GetShiftStartTime(ErpId) : _time;
                                if (DateTime.Now.TimeOfDay < TimeSpan.Parse(_time))
                                {
                                    _time = string.Empty;
                                    ErrorMessage = "Must manually enter time.";
                                    _clockLoaded = false;
                                }
                                else
                                {
                                    _clockLoaded = true;
                                }
                                _inTime = value = _time;
                            }
                            catch
                            {
                                _time = string.Empty;
                                ErrorMessage = "Must manually enter time.";
                                _clockLoaded = false;
                            }
                        }
                        OnPropertyChanged(nameof(InTime));
                        OnPropertyChanged(nameof(ErrorMessage));
                    });
                }
                else
                {
                    _inTime = value;
                    _clockLoaded = false;
                    OnPropertyChanged(nameof(InTime));
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }
        private bool _clockLoaded;
        public string InDate
        {
            get
            {
                if(Shift == 3)
                {
                    if (DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00"))
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else if (DateTime.Now.TimeOfDay < TimeSpan.Parse("21:00") && TimeSpan.Parse(InTime) < TimeSpan.Parse("21:00"))
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        return DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy");
                    }
                }
                else if (Shift == 5)
                {
                    if (DateTime.Now.TimeOfDay > TimeSpan.Parse("14:00"))
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else if (DateTime.Now.TimeOfDay < TimeSpan.Parse("14:00") && TimeSpan.Parse(InTime) < TimeSpan.Parse("14:00"))
                    {
                        return DateTime.Today.ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        return DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy");
                    }
                }
                else
                {
                    return DateTime.Today.ToString("MM-dd-yyyy");
                }
            }
        }

        public char ClockTran { get; set; }

        public string OutTime
        {
            get { return DateTime.Now.ToString("HH:mm"); }
        }

        private bool _work;
        public bool IsWorking
        {
            get { return _work; }
            set
            {
                _work = value;
                HoursWorked = value ? 8 : 0;
                WorkCenter = value ? new Machine() : null;
                OnPropertyChanged(nameof(IsWorking));
            }
        }

        private int _workHours;
        public int HoursWorked
        {
            get { return _workHours; }
            set { _workHours = value; OnPropertyChanged(nameof(HoursWorked)); }
        }

        private Machine _workCenter;
        public Machine WorkCenter
        {
            get { return _workCenter; }
            set{ _workCenter = value; OnPropertyChanged(nameof(WorkCenter)); }
        }

        public string LaborId
        {
            get { return $"{ErpId}*{(DateTime.Today - Convert.ToDateTime("1967/12/31")).Days}*0{Facility}"; }
        }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            return Name.CompareTo(obj);
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewMember()
        { }

        /// <summary>
        /// Overridden Constructor
        /// Load a crewmember object based on an employee ID
        /// </summary>
        /// <param name="sapId">Crew member ID Number</param>
        /// <param name="loadLabor">Load the labor fields for the crew member</param>
        public CrewMember(int sapId, bool loadLabor)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[SapEmployeeID] = {sapId}");
            if (_rows.Count() > 0)
            {
                ErpId = _rows.FirstOrDefault().Field<string>("EmployeeId");
                Name = _rows.FirstOrDefault().Field<string>("DisplayName");
                if (loadLabor)
                {
                    IsDirect = _rows.FirstOrDefault().Field<int>("IsDirect") == 1;
                    Shift = _rows.FirstOrDefault().Field<int>("Shift");
                    ShiftStart = _rows.FirstOrDefault().Field<string>("ShiftStart");
                    ShiftEnd = _rows.FirstOrDefault().Field<string>("ShiftEnd");
                    Facility = $"0{_rows.FirstOrDefault().Field<int>("Site")}";
                    SapId = _rows.FirstOrDefault().Field<int>("SapEmployeeID");
                    ErrorMessage = string.Empty;
                    _clockLoaded = false;
                    if (IsDirect)
                    {
                        IsWorking = true;
                        HoursWorked = 8;
                        WorkCenter = new Machine();
                    }
                }
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// Load a crewmember object based on an employee ID
        /// </summary>
        /// <param name="erpId">Crew member ID Number</param>
        /// <param name="loadLabor">Load the labor fields for the crew member</param>
        public CrewMember(string erpId, bool loadLabor)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{erpId}'");
            if (_rows.Count() > 0)
            {
                ErpId = erpId;
                Name = _rows.FirstOrDefault().Field<string>("DisplayName");
                if (loadLabor)
                {
                    IsDirect = _rows.FirstOrDefault().Field<int>("IsDirect") == 1;
                    Shift = _rows.FirstOrDefault().Field<int>("Shift");
                    ShiftStart = _rows.FirstOrDefault().Field<string>("ShiftStart");
                    ShiftEnd = _rows.FirstOrDefault().Field<string>("ShiftEnd");
                    InTime = string.Empty;
                    Facility = $"0{_rows.FirstOrDefault().Field<int>("Site")}";
                    SapId = _rows.FirstOrDefault().Field<int>("SapEmployeeID");
                    ErrorMessage = string.Empty;
                    _clockLoaded = false;
                    if (IsDirect)
                    {
                        IsWorking = true;
                        HoursWorked = 8;
                        WorkCenter = new Machine();
                    }
                }
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// Load a crewmember object based on an employee ID
        /// </summary>
        /// <param name="erpId">Crew member ID Number</param>
        /// <param name="firstName">Crew member first name</param>
        /// <param name="lastName">Crew member last name</param>
        /// <param name="loadLabor">Load the labor fields for the crew member</param>
        public CrewMember(string erpId, string firstName, string lastName, bool loadLabor)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{erpId}'");
            if (_rows.Count() == 0)
            {
                _rows = MasterDataSet.Tables["CREW"].Select($"[FirstName] = '{firstName}' AND [LastName] = '{lastName}'");
            }
            if (_rows.Length > 0)
            {
                ErpId = erpId;
                Name = _rows.FirstOrDefault().Field<string>("DisplayName");
                if (loadLabor)
                {
                    IsDirect = _rows.FirstOrDefault().Field<int>("IsDirect") == 1;
                    Shift = _rows.FirstOrDefault().Field<int>("Shift");
                    ShiftStart = _rows.FirstOrDefault().Field<string>("ShiftStart");
                    ShiftEnd = _rows.FirstOrDefault().Field<string>("ShiftEnd");
                    InTime = string.Empty;
                    Facility = $"0{_rows.FirstOrDefault().Field<int>("Site")}";
                    ErrorMessage = string.Empty;
                    _clockLoaded = false;
                    if (IsDirect)
                    {
                        IsWorking = true;
                        HoursWorked = 8;
                        WorkCenter = new Machine();
                    }
                }
            }
        }

        #region Data Access

        /// <summary>
        /// Get a table of all the staff on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of staff members</returns>
        public static DataTable GetCrewTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Staff] WHERE [Site] = @p1 ORDER BY [DisplayName]", sqlCon))
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

        /// <summary>
        /// Get a table of all the staff labor for crew management
        /// </summary>
        /// <param name="manager">Manager ID</param>
        /// <param name="date">Date to get the data from</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of staff labor</returns>
        public static List<CrewMember> GetCrewLaborList(string manager, DateTime date, SqlConnection sqlCon)
        {
            var _rtnList = new List<CrewMember>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_StaffLabor] WHERE [Manager] = @p1 AND [DateChanged] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", manager);
                        cmd.Parameters.AddWithValue("p2", date.ToString("yyyy-MM-dd"));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while(reader.Read())
                                {
                                    var _tempMach = reader.SafeGetInt32("WorkCenter") == 0
                                        ? null
                                        : new Machine(reader.SafeGetInt32("WorkCenter"));
                                    var _tempCrew = new CrewMember {
                                    ErpId = reader.SafeGetString("UserID")
                                    , Name = reader.SafeGetString("DisplayName")
                                    , Shift = reader.SafeGetInt32("Shift")
                                    , Facility = reader.SafeGetString("FacilityID")
                                    , IsWorking = reader.SafeGetInt32("WorkHours") > 0
                                    , HoursWorked = reader.SafeGetInt32("WorkHours")
                                    , WorkCenter = _tempMach };
                                    if (!string.IsNullOrEmpty(_tempCrew.Name))
                                    {
                                        _rtnList.Add(_tempCrew);
                                    }
                                }
                            }   
                        }
                    }
                    return _rtnList;
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

        /// <summary>
        /// Get a table of all the staff labor for crew management
        /// </summary>
        /// <param name="Shift">Employee shift</param>
        /// <param name="date">Date to get the data from</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of staff labor</returns>
        public static List<CrewMember> GetCrewLaborList(int shift, DateTime date, SqlConnection sqlCon)
        {
            var _rtnList = new List<CrewMember>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_StaffLabor] WHERE [Shift] = @p1 AND [DateChanged] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", shift);
                        cmd.Parameters.AddWithValue("p2", date.ToString("yyyy-MM-dd"));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while(reader.Read())
                                {
                                    var _tempMach = reader.SafeGetInt32("WorkCenter") == 0
                                        ? null
                                        : new Machine(reader.SafeGetInt32("WorkCenter"));
                                    var _tempCrew = new CrewMember {
                                    ErpId = reader.SafeGetString("UserID")
                                    , Name = reader.SafeGetString("DisplayName")
                                    , Shift = reader.SafeGetInt32("Shift")
                                    , Facility = reader.SafeGetString("FacilityID")
                                    , IsWorking = reader.SafeGetInt32("WorkHours") > 0
                                    , HoursWorked = reader.SafeGetInt32("WorkHours")
                                    , WorkCenter = _tempMach };
                                    if (!string.IsNullOrEmpty(_tempCrew.Name))
                                    {
                                        _rtnList.Add(_tempCrew);
                                    }
                                }
                            }   
                        }
                    }
                    return _rtnList;
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

        /// <summary>
        /// Checks to see if any crew labor has been published
        /// </summary>
        /// <param name="shift">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool IsPublished(string manager, DateTime date, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT COUNT(LaborID) FROM [dbo].[SFW_StaffLabor] WHERE [Manager] = @p1 AND [DateChanged] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", manager);
                        cmd.Parameters.AddWithValue("p2", date);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0;
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

        /// <summary>
        /// Gets the ERP ID from the database
        /// </summary>
        /// <param name="sapId">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>ERP ID as string</returns>
        public static string GetCrewErpID(int sapId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [EmployeeID] FROM [dbo].[SFW_Staff] WHERE [SapEmployeeID] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", sapId);
                        return cmd.ExecuteScalar().ToString();
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

        /// <summary>
        /// Insertion and updating of crew expected labor into a custom SQL table
        /// </summary>
        /// <param name="crewMembers">List of crew members objects</param>
        /// <param name="action">Type of SQL action to process</param>
        /// <param name="managerId">Manager employee ID</param>
        /// <param name="publishDate">Date to use in the publishing</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Error or success message</returns>
        public static IReadOnlyDictionary<bool, string> PublishLabor(List<CrewMember> crewMembers, char action, string managerId, DateTime publishDate, SqlConnection sqlCon)
        {
            var _rtnDict = new Dictionary<bool, string>();
            var cmdString = string.Empty;
            var _dateId = (publishDate - Convert.ToDateTime("1967/12/31")).Days;
            switch (action)
            {
                case 'S':
                    cmdString = "INSERT INTO dbo.[EM-CSTM_Working_Data] ([LaborID], [WorkHours], [Shift], [DisplayName], [WorkCenter], [Manager]) VALUES (@p1, @p2, @p3, @p4, @p5, @p6)";
                    break;
                case 'U':
                    cmdString = "UPDATE dbo.[EM-CSTM_Working_Data] SET [WorkHours] = @p1, [WorkCenter] = @p2 WHERE [LaborID] = @p3";
                    break;
            }
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand sqlCommand = new SqlCommand(cmdString, sqlCon))
                    {
                        foreach (var _crewMember in crewMembers)
                        {
                            sqlCommand.Parameters.Clear();
                            var _workCenter = _crewMember.WorkCenter != null && !string.IsNullOrEmpty(_crewMember.WorkCenter.MachineNumber)
                                    ? _crewMember.WorkCenter.MachineNumber
                                    : "";
                            switch (action)
                            {
                                case 'S':
                                    sqlCommand.Parameters.AddWithValue("@p1", $"{_crewMember.ErpId}*{_dateId}*{_crewMember.Facility}");
                                    sqlCommand.Parameters.AddWithValue("@p2", _crewMember.HoursWorked);
                                    sqlCommand.Parameters.AddWithValue("@p3", _crewMember.Shift);
                                    sqlCommand.Parameters.AddWithValue("@p4", _crewMember.Name);
                                    sqlCommand.Parameters.AddWithValue("@p5", _workCenter);
                                    sqlCommand.Parameters.AddWithValue("@p6", managerId);
                                    break;
                                case 'U':
                                    sqlCommand.Parameters.AddWithValue("@p1", _crewMember.HoursWorked);
                                    sqlCommand.Parameters.AddWithValue("@p2", _workCenter);
                                    sqlCommand.Parameters.AddWithValue("@p3", $"{_crewMember.ErpId}*{_dateId}*{_crewMember.Facility}");
                                    break;
                            }
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    _rtnDict.Add(true, "Submission has completed successfully.");
                    return _rtnDict;
                }
                catch (SqlException sqlEx)
                {
                    _rtnDict.Add(false, sqlEx.Message);
                    return _rtnDict;
                }
                catch (Exception ex)
                {
                    _rtnDict.Add(false, ex.Message);
                    return _rtnDict;
                }
            }
            else
            {
                _rtnDict.Add(false, "A connection could not be made to pull accurate data, please contact your administrator");
                return _rtnDict;
            }
        }

        #endregion

        /// <summary>
        /// Checks to see if a crew ID number is valid
        /// </summary>
        /// <param name="erpId">Crew member ID</param>
        /// <returns>Crew member existance in the database</returns>
        public static bool IsCrewErpIdValid(string erpId)
        {
            return MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{erpId}'").Length > 0;
        }

        /// <summary>
        /// Gets the crew ID based on the crew members first and last name
        /// </summary>
        /// <param name="erpId">ERP Id to search</param>
        /// <returns>Crew member existance in the database</returns>
        public static string GetCrewErpID(int sapId)
        {
            return MasterDataSet?.Tables["CREW"].Select($"[SapEmployeeID] = '{sapId}'").FirstOrDefault().SafeGetField<string>("EmployeeID");
        }

        /// <summary>
        /// Gets the crew ID based on the crew members first and last name
        /// </summary>
        /// <param name="firstName">Crew member first name</param>
        /// <param name="lastName">Crew member last name</param>
        /// <returns>Crew member existance in the database</returns>
        public static string GetCrewErpID(string firstName, string lastName)
        {
            return MasterDataSet.Tables["CREW"].Select($"[FirstName] = '{firstName}' AND [LastName] LIKE '{lastName}%'").FirstOrDefault().SafeGetField<string>("EmployeeID");
        }

        /// <summary>
        /// Get a crew member's display name
        /// </summary>
        /// <param name="idNbr">Crew member's ID number</param>
        /// <returns>Crew member's display name</returns>
        public static string GetCrewDisplayName(string idNbr)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'");
            return _rows.Length > 0 ? _rows.FirstOrDefault().Field<string>("DisplayName") : null;
        }

        /// <summary>
        /// Get the shift start time for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <returns>Shift start time as a string</returns>
        public static string GetShiftStartTime(string idNbr)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'");
            return _rows.Length > 0 ? _rows.FirstOrDefault().Field<string>("ShiftStart") : string.Empty;
        }

        /// <summary>
        /// Get the shift end time for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <returns>Shift end time as a string</returns>
        public static string GetShiftEndTime(string idNbr)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'");
            return _rows.Length > 0 ? _rows.FirstOrDefault().Field<string>("ShiftEnd") : string.Empty;
        }

        /// <summary>
        /// Get facility code for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <returns>facility code as a string</returns>
        public static string GetFacility(string idNbr)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'");
            return _rows.Length > 0 ? $"0{_rows.FirstOrDefault().Field<int>("Site")}" : string.Empty;
        }

        /// <summary>
        /// Retreives the last labor clocked in time for the current user
        /// </summary>
        /// <param name="crewId">User ID number</param>
        /// <param name="facCode">Facility code</param>
        /// <param name="dateId">ERP Date ID in unix time to be used for query</param>
        /// <returns>Time as a string</returns>
        public static string GetInTime(string crewId, string facCode, int dateId)
        {
            if (WipReceipt.ErpCon == null)
            {
                return "24:00";
            }
            if (string.IsNullOrEmpty(facCode))
            {
                facCode = GetFacility(crewId);
            }
            try
            {
                //var dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                var uResponse = string.Empty;
                using (UniSession uSession = UniObjects.OpenSession(WipReceipt.ErpCon[0], WipReceipt.ErpCon[1], WipReceipt.ErpCon[2], WipReceipt.ErpCon[3], WipReceipt.ErpCon[4]))
                {
                    try
                    {
                        using (UniCommand uCmd = uSession.CreateUniCommand())
                        {
                            uCmd.Command = $"LIST LBR.DETAIL WITH @ID = \"{crewId}*{dateId}*{facCode}\" Latest_Time_Out";
                            uCmd.Execute();
                            uResponse = uCmd.Response;
                            if (!string.IsNullOrEmpty(uResponse))
                            {
                                //All the code below is to clean the UniCommand.Response
                                //The response returns identical to a M2k TCL response
                                uResponse = uResponse.Replace("\n", "");
                                var uResArray = uResponse.Split('\r');
                                foreach (var s in uResArray.Where(o => o.Contains(crewId)))
                                {
                                    if (!s.Contains(uCmd.Command))
                                    {
                                        if (s.Contains($"{crewId}*{dateId}*{facCode}"))
                                        {
                                            return s.Replace($"{crewId}*{dateId}*{facCode}", "").Trim();
                                        }
                                        else if (s.Contains($"{crewId}*{dateId}"))
                                        {
                                            return s.Replace($"{crewId}*{dateId}*", "").Trim();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return "ERR:Unable to query system.";
                            }
                        }
                        UniObjects.CloseSession(uSession);
                    }
                    catch (Exception ex)
                    {
                        if (uSession != null)
                        {
                            UniObjects.CloseSession(uSession);
                        }
                        return $"ERR:{ex.Message}";
                    }
                }
                return GetShiftStartTime(crewId);
            }
            catch
            {
                return "ERR:Connection Error";
            }
        }

        /// <summary>
        /// Get any crew members full professional name stored in the active directory
        /// </summary>
        /// <param name="domainName">Crew Member Domain Name</param>
        /// <returns>Full crew member name as string</returns>
        public static string GetCrewMemberFullName(string domainName)
        {
            using (PrincipalContext pContext = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal uPrincipal = UserPrincipal.FindByIdentity(pContext, domainName))
                {
                    return uPrincipal != null ? $"{uPrincipal.GivenName} {uPrincipal.Surname}" : domainName;
                }
            }
        }

        /// <summary>
        /// Get observable collection of crew members
        /// </summary>
        /// <param name="site">Crew site filter</param>
        /// <returns>ObservableCollection of crewmember objects</returns>
        public static ObservableCollection<CrewMember> GetCrewCollection(int site)
        {
            var _crewCol = new ObservableCollection<CrewMember>();
            var _crewRows = MasterDataSet.Tables["CREW"].Select($"[Site] = '{site}'");
            foreach (var _row in _crewRows)
            {
               _crewCol.Add(new CrewMember
                    {
                        ErpId = _row.Field<string>("EmployeeID")
                        ,Name = _row.Field<string>("DisplayName")
                        ,Shift = _row.Field<int>("Shift")
                        ,Facility = _row.Field<int>("Site").ToString()
                    });
            }
            return _crewCol;
        }
    }
}
