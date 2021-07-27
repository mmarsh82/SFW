using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace SFW.Model
{
    public class CrewMember : INotifyPropertyChanged
    {
        #region Properties

        private string idNbr;
        public string IdNumber
        {
            get { return idNbr; }
            set { idNbr = value; OnPropertyChanged(nameof(IdNumber)); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        private bool isDirect;
        public bool IsDirect
        {
            get
            { return isDirect; }
            set
            { isDirect = value; OnPropertyChanged(nameof(IsDirect)); }
        }

        private int shift;
        public int Shift
        {
            get
            { return shift; }
            set
            { shift = value; OnPropertyChanged(nameof(Shift)); }
        }

        private string lastClock;
        public string LastClock
        {
            get
            { return lastClock; }
            set
            {
                if (IsDirect && lastClock == null)
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        var time = GetLastClockTime(IdNumber, Shift, ModelBase.ModelSqlCon);
                        if (string.IsNullOrEmpty(time) || time == "00:00")
                        {
                            switch (Shift)
                            {
                                case 1:
                                    time = "07:00";
                                    break;
                                case 2:
                                    time = "15:00";
                                    break;
                                case 3:
                                    time = "23:00";
                                    break;
                            }
                        }
                        if (DateTime.TryParse(time, out DateTime dt))
                        {
                            if (DateTime.Now < dt && Shift != 3)
                            {
                                time = string.Empty;
                            }
                        }
                        else
                        {
                            time = string.Empty;
                        }
                        lastClock = time;
                        OnPropertyChanged(nameof(LastClock));
                    });
                }
                else
                {
                    lastClock = value;
                    OnPropertyChanged(nameof(LastClock));
                }
            }
        }

        public char ClockTran { get; set; }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
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
        /// <param name="sqlCon">Sql Connection to use</param>
        public CrewMember(string idNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                CONCAT([First_Name], ' ', [Last_Name]) as 'DisplayName',
	                                                                CASE WHEN [Dept] = '010' THEN 1 ELSE 0 END as 'IsDirect',
	                                                                [Shift]
                                                                FROM
	                                                                [dbo].[EMPLOYEE_MASTER-INIT]
                                                                WHERE
	                                                                [Emp_No] = @p1 AND [Pay_Status] = 'A';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    IdNumber = idNbr;
                                    Name = reader.SafeGetString("DisplayName");
                                    IsDirect = reader.SafeGetBoolean("IsDirect");
                                    Shift = reader.SafeGetInt32("Shift");
                                }
                            }
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
        /// Overridden Constructor
        /// Load a crewmember object based on an employee name
        /// WARNING this will not work if the name is spelled incorrectly
        /// Best to just include all crew member ID's in the active directory
        /// </summary>
        /// <param name="firstName">Crew member first name</param>
        /// <param name="lastName">Crew member last name</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public CrewMember(string firstName, string lastName, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                [Emp_No],
	                                                                CASE WHEN [Dept] = '010' THEN 1 ELSE 0 END as 'IsDirect',
	                                                                [Shift]
                                                                FROM
	                                                                [dbo].[EMPLOYEE_MASTER-INIT]
                                                                WHERE
	                                                                [First_Name] = @p1 AND [Last_Name] = @p2 AND [Pay_Status] = 'A';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", firstName);
                        cmd.Parameters.AddWithValue("p2", lastName);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    IdNumber = reader.SafeGetString("Emp_No");
                                    Name = $"{firstName} {lastName}";
                                    IsDirect = reader.SafeGetBoolean("IsDirect");
                                    Shift = reader.SafeGetInt32("Shift");
                                }
                            }
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
        /// Checks to see if a crew ID number is valid
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="idNbr">Crew member ID</param>
        /// <returns>Crew member existance in the database</returns>
        public static bool IsCrewIDValid(SqlConnection sqlCon, string idNbr)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT COUNT([Emp_No]) as 'Count' FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1 AND [Pay_Status] = 'A';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0;
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
        /// Get a crew member's display name
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="idNbr">Crew member's ID number</param>
        /// <returns>Crew member's display name</returns>
        public static string GetCrewDisplayName(SqlConnection sqlCon, string idNbr)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT CONCAT([First_Name], ' ', [Last_Name]) as 'DisplayName' FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1 AND [Pay_Status] = 'A';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        var _id = cmd.ExecuteScalar();
                        return _id == null ? string.Empty : _id.ToString();
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
        /// Get a populated binding list of CrewMember objects
        /// </summary>
        /// <param name="reportID">Report ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of populated CrewMember objects</returns>
        public static BindingList<CrewMember> GetCrewBindingList(int reportID, SqlConnection sqlCon)
        {
            var _tempList = new BindingList<CrewMember>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT 
	                                                                a.[UserID],
	                                                                CONCAT(b.[First_Name], ' ', b.[Last_Name]) AS 'Name',
	                                                                CASE WHEN b.[Dept] = '010' THEN 1 ELSE 0 END AS 'Direct'
                                                                FROM
	                                                                [dbo].[PRM-CSTM_Crew] a
                                                                LEFT JOIN
	                                                                [dbo].[EMPLOYEE_MASTER-INIT] b on b.[Emp_No] = CAST(a.[UserID] AS VARCHAR)
                                                                WHERE
	                                                                a.[ReportID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reportID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new CrewMember
                                    {
                                        IdNumber = reader.SafeGetString("UserID"),
                                        Name = reader.SafeGetString("Name")
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
        /// Check to see if a user is clocked into a work order
        /// </summary>
        /// <param name="idNbr">User ID number</param>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>bool result value as True: is clocked in; False: is not clocked in</returns>
        public static bool IsClockedIn(string idNbr, string woNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT 
	                                                                COUNT([ID1]) as 'ClockedIn'
                                                                FROM
	                                                                [dbo].[LBR_DETAIL-INIT_Daily_Lbr_Det]
                                                                WHERE
	                                                                [ID1] LIKE CONCAT(@p1,'%') AND [Job] = CONCAT(@p2,'*',@p3) AND [Out_Time] = '00:00';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        cmd.Parameters.AddWithValue("p2", woNbr);
                        cmd.Parameters.AddWithValue("p3", seq);
                        return Convert.ToInt32(cmd.ExecuteScalar()) >= 1;
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
        /// Retreives the last labor clocked in time for the current user
        /// </summary>
        /// <param name="idNbr">User ID number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Time as a string</returns>
        public static string GetLastClockTime(string idNbr, int shift, SqlConnection sqlCon)
        {
            /*if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var test = (DateTime.Now - Convert.ToDateTime("1967/12/31")).Days;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                DECLARE @Shift as int;
                                                                SET @Shift = (SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1);
                                                                SELECT
                                                                CASE WHEN @Shift != 3
	                                                                THEN
		                                                                (SELECT TOP(1) [Out_Time] FROM [dbo].[LBR_DETAIL-INIT_Daily_Lbr_Det] WHERE [ID1] = CONCAT(@p1,'*',@p2) ORDER BY [Out_Time] DESC)
	                                                                ELSE
		                                                                (SELECT TOP(1) [Out_Time]
		                                                                FROM [dbo].[LBR_DETAIL-INIT_Daily_Lbr_Det]
		                                                                WHERE [ID1] = CONCAT(@p1,'*',@p2) OR ([ID1] = CONCAT(@p1,'*',@p2 - 1) AND CAST([Out_Time] as TIME) >= CAST('23:00' as TIME)) AND [Out_Time] != '00:00'
		                                                                ORDER BY [Out_Time] ASC)
                                                                END as 'OutTime';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr.ToString());
                        cmd.Parameters.AddWithValue("p2", (DateTime.Now - Convert.ToDateTime("1967/12/31")).Days);
                        return cmd.ExecuteScalar()?.ToString();
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
            }*/
            try
            {
                var id2 = (DateTime.Now - Convert.ToDateTime("1967/12/31")).Days;
                if (shift == 3 && DateTime.Now.TimeOfDay > new TimeSpan(23,00,00))
                {
                    id2++;
                }
                using (UniSession uSession = UniObjects.OpenSession("172.16.0.10", "omniquery", "omniquery", "E:/roi/WCCO.MAIN", "udcs"))
                {
                    try
                    {
                        var uResponse = string.Empty;
                        using (UniCommand uCmd = uSession.CreateUniCommand())
                        {
                            uCmd.Command = $"LIST LBR.DETAIL WITH @ID = \"{idNbr}*{id2}\" Latest_Time_Out";
                            uCmd.Execute();
                            uResponse = uCmd.Response;
                            if (!string.IsNullOrEmpty(uResponse))
                            {
                                //All the code below is to clean the UniCommand.Response
                                //The response returns identical to a M2k TCL response
                                var uResArray = uResponse.Split('\r');
                                var _sortList = new List<DateTime>();
                                var _listAdd = false;
                                foreach (var s in uResArray)
                                {
                                    if (s.Replace("\n", "").Contains($"records") || s.Replace("\n", "").Contains($"record"))
                                    {
                                        break;
                                    }
                                    if (s.Replace("\n", "~").Contains($"~{idNbr}*{id2}") || _listAdd)
                                    {
                                        _sortList.Add(Convert.ToDateTime(s.Substring(s.Length - 5).Trim()));
                                        _listAdd = true;
                                    }
                                }
                                if (_sortList.Count == 0)
                                {
                                    uResponse = GetShiftStartTime(idNbr, sqlCon);
                                }
                                else
                                {
                                    if (shift == 3 && !_sortList.Any(o => o.TimeOfDay < new TimeSpan(23, 00, 00)))
                                    {
                                        _sortList = _sortList.OrderBy(o => o.TimeOfDay).ToList();
                                    }
                                    else
                                    {
                                        _sortList = _sortList.OrderByDescending(o => o.TimeOfDay).ToList();
                                    }
                                    uResponse = _sortList[0].ToString("HH:mm");
                                }
                            }
                            else
                            {
                                uResponse = GetShiftStartTime(idNbr, sqlCon);
                            }
                        }
                        UniObjects.CloseSession(uSession);
                        return uResponse;
                    }
                    catch (Exception ex)
                    {
                        if (uSession != null)
                        {
                            UniObjects.CloseSession(uSession);
                        }
                        return ex.Message;
                    }
                }
            }
            catch
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the shift start time for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Shift start time as a string</returns>
        public static string GetShiftStartTime(string idNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                               SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        if (int.TryParse(cmd.ExecuteScalar()?.ToString(), out int _shift))
                        {
                            switch (_shift)
                            {
                                case 1:
                                    return "07:00";
                                case 2:
                                    return "15:00";
                                case 3:
                                    return "23:00";
                            }
                        }
                        return string.Empty;
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
        /// Get the shift end time for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Shift end time as a string</returns>
        public static string GetShiftEndTime(string idNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                               SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", idNbr);
                        if (int.TryParse(cmd.ExecuteScalar()?.ToString(), out int _shift))
                        {
                            switch (_shift)
                            {
                                case 1:
                                    return "15:00";
                                case 2:
                                    return "23:00";
                                case 3:
                                    return "07:00";
                            }
                        }
                        return string.Empty;
                    }
                }
                catch (SqlException)
                {
                    return string.Empty;
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
    }
}
