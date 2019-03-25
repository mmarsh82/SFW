using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class CrewMember : INotifyPropertyChanged
    {
        #region Properties

        private int? idNbr;
        public int? IdNumber
        {
            get { return idNbr; }
            set { idNbr = value; OnPropertyChanged(nameof(IdNumber)); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); OnPropertyChanged(nameof(Shift)); OnPropertyChanged(nameof(IsDirect)); LastClock = string.Empty; }
        }

        public bool IsDirect
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    var sqlCon = ModelBase.ModelSqlCon;
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                CASE WHEN [Dept] = '010'
		                                                                THEN
			                                                                1
		                                                                ELSE
			                                                                0 END as [IsDirect]
                                                                FROM
	                                                                [dbo].[EMPLOYEE_MASTER-INIT]
                                                                WHERE
	                                                                [Emp_No] = @p1 AND [Pay_Status] = 'A';", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", IdNumber);
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
                return false;
            }
        }

        public int Shift
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    var sqlCon = ModelBase.ModelSqlCon;
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                               SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", IdNumber);
                                return Convert.ToInt32(cmd.ExecuteScalar());
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
                return 0;
            }
        }

        private string lastClock;
        public string LastClock
        {
            get { return lastClock; }
            set
            {
                if (!string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(value) && IsDirect)
                {
                    var time = GetLastClockTime(Convert.ToInt32(IdNumber), ModelBase.ModelSqlCon);
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
                    lastClock = time;
                }
                else
                {
                    lastClock = value;
                }
                OnPropertyChanged(nameof(LastClock));
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
        /// Get an crew member's Id number
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="firstName">Crew member's first name</param>
        /// <param name="lastName">Crew member's last name</param>
        /// <returns></returns>
        public static int GetCrewIdNumber(SqlConnection sqlCon, string firstName, string lastName)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Emp_No] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [First_Name] = @p1 AND [Last_Name] = @p2 AND [Pay_Status] = 'A';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", firstName);
                        cmd.Parameters.AddWithValue("p2", lastName);
                        return Convert.ToInt32(cmd.ExecuteScalar());
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
        /// <returns></returns>
        public static string GetCrewDisplayName(SqlConnection sqlCon, int idNbr)
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
                                        IdNumber = reader.SafeGetInt32("UserID"),
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
        public static bool IsClockedIn(int idNbr, string woNbr, string seq, SqlConnection sqlCon)
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
        public static string GetLastClockTime(int idNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var test = (DateTime.Now - Convert.ToDateTime("1967/12/31")).Days;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                DECLARE @Shift as int;
                                                                SET @Shift = (SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [Emp_No] = @p1);
                                                                DECLARE @OutTime as varchar
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
            }
        }

        /// <summary>
        /// Get the shift start time for a user
        /// </summary>
        /// <param name="idNbr">User Id number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Shift start time as a string</returns>
        public static string GetShiftStartTime(int idNbr, SqlConnection sqlCon)
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
    }
}
