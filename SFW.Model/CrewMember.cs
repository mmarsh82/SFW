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
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        private bool isDir;
        public bool IsDirect
        {
            get { return isDir; }
            set { isDir = value; OnPropertyChanged(nameof(IsDirect)); }
        }

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
    }
}
