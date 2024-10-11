using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace SFW.Model
{
    public class CrewMember : ModelBase
    {
        #region Properties

        private string _idNbr;
        public string IdNumber
        {
            get { return _idNbr; }
            set { _idNbr = value; OnPropertyChanged(nameof(IdNumber)); }
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
                        if (!string.IsNullOrEmpty(Facility) && IsCrewIDValid(IdNumber))
                        {
                            Facility = GetFacility(IdNumber);
                        }
                        var dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
                        var _time = GetInTime(IdNumber, Facility, dateId);
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
                                _time = GetInTime(IdNumber, Facility, dateId);
                                if (string.IsNullOrEmpty(_time))
                                {
                                    _time = GetShiftStartTime(IdNumber);
                                    _clockLoaded = true;
                                }
                                else
                                {
                                    if(TimeSpan.Parse(_time) < TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00"))
                                    {
                                        _time = GetShiftStartTime(IdNumber);
                                        _clockLoaded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (TimeSpan.Parse(_time) < TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00"))
                                {
                                    _time = GetShiftStartTime(IdNumber);
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
                                _time = GetInTime(IdNumber, Facility, dateId);
                                if (string.IsNullOrEmpty(_time))
                                {
                                    _time = GetShiftStartTime(IdNumber);
                                    _clockLoaded = true;
                                }
                                else
                                {
                                    if (TimeSpan.Parse(_time) < TimeSpan.Parse("14:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("14:00"))
                                    {
                                        _time = GetShiftStartTime(IdNumber);
                                        _clockLoaded = true;
                                    }
                                }
                            }
                            else
                            {
                                if (TimeSpan.Parse(_time) < TimeSpan.Parse("14:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("14:00"))
                                {
                                    _time = GetShiftStartTime(IdNumber);
                                    _clockLoaded = true;
                                }
                            }
                            _inTime = value = _time;
                        }
                        else
                        {
                            _time = string.IsNullOrEmpty(_time) ? GetShiftStartTime(IdNumber) : _time;
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

        private bool _work;
        public bool IsWorking
        {
            get { return _work; }
            set { _work = value; OnPropertyChanged(nameof(IsWorking)); }
        }

        private int _workHours;
        public int HoursWorked
        {
            get { return _workHours; }
            set { _workHours = value; OnPropertyChanged(nameof(HoursWorked)); }
        }

        public string LaborId
        {
            get { return $"{IdNumber}*{(DateTime.Today - Convert.ToDateTime("1967/12/31")).Days}*0{Facility}"; }
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
        /// <param name="idNbr">Crew member ID Number</param>
        public CrewMember(string idNbr)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'");
            if (_rows.Length > 0)
            {
                IdNumber = idNbr;
                Name = _rows.FirstOrDefault().Field<string>("DisplayName");
                IsDirect = _rows.FirstOrDefault().Field<int>("IsDirect") == 1;
                Shift = _rows.FirstOrDefault().Field<int>("Shift");
                ShiftStart = _rows.FirstOrDefault().Field<string>("ShiftStart");
                ShiftEnd = _rows.FirstOrDefault().Field<string>("ShiftEnd");
                Facility = $"0{_rows.FirstOrDefault().Field<int>("Site")}";
                ErrorMessage = string.Empty;
                _clockLoaded = false;
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// Load a crewmember object based on an employee name
        /// WARNING this will not work if the name is spelled incorrectly
        /// Best to just include all crew member ID's in the active directory
        /// </summary>
        /// <param name="firstName">Crew member first name</param>
        /// <param name="lastName">Crew member last name</param>
        public CrewMember(string firstName, string lastName)
        {
            var _rows = MasterDataSet.Tables["CREW"].Select($"[FirstName] = '{firstName}' AND [LastName] = '{lastName}'");
            if (_rows.Length > 0)
            {
                IdNumber = _rows.FirstOrDefault().Field<string>("EmployeeID");
                Name = _rows.FirstOrDefault().Field<string>("DisplayName");
                IsDirect = _rows.FirstOrDefault().Field<int>("IsDirect") == 1;
                Shift = _rows.FirstOrDefault().Field<int>("Shift");
                ShiftStart = _rows.FirstOrDefault().Field<string>("ShiftStart");
                ShiftEnd = _rows.FirstOrDefault().Field<string>("ShiftEnd");
                Facility = $"0{_rows.FirstOrDefault().Field<int>("Site")}";
                InTime = string.Empty;
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Staff] WHERE [Site] = @p1", sqlCon))
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
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of staff labor</returns>
        public static DataTable GetCrewLaborTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_StaffLabor] WHERE [FacilityID] = @p1", sqlCon))
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
        /// Insertion and updating of crew expected labor into a custom SQL table
        /// </summary>
        /// <param name="crewMembers">List of crew members objects</param>
        /// <param name="action">Type of SQL action to process</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Error or success message</returns>
        public static string PublishLabor(List<CrewMember> crewMembers, char action, SqlConnection sqlCon)
        {
            var cmdString = string.Empty;
            var _dateId = (DateTime.Today - Convert.ToDateTime("1967/12/31")).Days;
            switch (action)
            {
                case 'S':
                    cmdString = "INSERT INTO dbo.[EM-CSTM_Working_Data] ([LaborID], [WorkHours], [Shift], [DisplayName]) VALUES (@p1, @p2, @p3, @p4)";
                    break;
                case 'U':
                    cmdString = "UPDATE dbo.[EM-CSTM_Working_Data] SET [WorkHours] = @p1 WHERE [LaborID] = @p2";
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
                            switch (action)
                            {
                                case 'S':
                                    sqlCommand.Parameters.AddWithValue("@p1", $"{_crewMember.IdNumber}*{_dateId}*0{_crewMember.Facility}");
                                    sqlCommand.Parameters.AddWithValue("@p2", _crewMember.HoursWorked);
                                    sqlCommand.Parameters.AddWithValue("@p3", _crewMember.Shift);
                                    sqlCommand.Parameters.AddWithValue("@p4", _crewMember.Name);
                                    break;
                                case 'U':
                                    sqlCommand.Parameters.AddWithValue("@p1", _crewMember.HoursWorked);
                                    sqlCommand.Parameters.AddWithValue("@p2", $"{_crewMember.IdNumber}*{_dateId}*{_crewMember.Facility}");
                                    break;
                            }
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    return "Submission has completed successfully.";
                }
                catch (SqlException sqlEx)
                {
                    return sqlEx.Message;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return "A connection could not be made to pull accurate data, please contact your administrator";
            }
        }

        #endregion

        /// <summary>
        /// Checks to see if a crew ID number is valid
        /// </summary>
        /// <param name="idNbr">Crew member ID</param>
        /// <returns>Crew member existance in the database</returns>
        public static bool IsCrewIDValid(string idNbr)
        {
            return MasterDataSet.Tables["CREW"].Select($"[EmployeeID] = '{idNbr}'").Length > 0;
        }

        /// <summary>
        /// Gets the crew ID based on the crew members first and last name
        /// </summary>
        /// <param name="firstName">Crew member first name</param>
        /// <param name="lastName">Crew member last name</param>
        /// <returns>Crew member existance in the database</returns>
        public static string GetCrewID(string firstName, string lastName)
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
        /// Get facility code for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <returns>facility code as a string</returns>
        public static IReadOnlyDictionary<char, List<CrewMember>> GetCrewList(int shift, int site)
        {
            var _crewList = new List<CrewMember>();
            var _rowList = new List<DataRow>();
            var _rtnDict = new Dictionary<char, List<CrewMember>>();
            if (site == 2 && shift == 4)
            {
                shift = 1;
            }
            else if (site == 2 && shift == 5)
            {
                shift = 2;
            }
            if (MasterDataSet.Tables["CrewLabor"].Select($"[Shift] = '{shift}' AND [FacilityID] = '0{site}' AND [DateChanged] = '{DateTime.Today.ToString("yyyy-MM-dd")}'").Count() > 0)
            {
                _rowList = MasterDataSet.Tables["CrewLabor"].Select($"[Shift] = '{shift}' AND [FacilityID] = '0{site}' AND [DateChanged] = '{DateTime.Today.ToString("yyyy-MM-dd")}'").ToList();
            }
            else
            {
                _rowList = MasterDataSet.Tables["CREW"].Select($"[Shift] = '{shift}' AND [Site] = '{site}' AND [IsDirect] = 1").ToList();
            }
            var _rtnAction = _rowList[0].Table.TableName == "CrewLabor" ? 'U' : 'S';
            foreach (var _row in _rowList)
            {
                if (_rowList[0].Table.TableName == "CrewLabor")
                {
                    _crewList.Add(new CrewMember
                    {
                        IdNumber = _row.Field<string>("UserID")
                        ,Name = _row.Field<string>("DisplayName")
                        ,IsWorking = _row.Field<int>("WorkHours") > 0
                        ,Shift = _row.Field<int>("Shift")
                        ,Facility = _row.Field<string>("FacilityID")
                        ,HoursWorked = _row.Field<int>("WorkHours")
                    });
                }
                else
                {
                    _crewList.Add(new CrewMember
                    {
                        IdNumber = _row.Field<string>("EmployeeID")
                        ,Name = _row.Field<string>("DisplayName")
                        ,IsWorking = true
                        ,Shift = shift
                        ,Facility = site.ToString()
                        ,HoursWorked = site == 1 ? 8 : 10
                    });
                }
            }
            _rtnDict.Add(_rtnAction, _crewList);
            return _rtnDict;
        }
    }
}
