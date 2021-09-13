using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable, INotifyPropertyChanged
    {
        #region Properties

        public static SqlConnection ModelSqlCon { get; set; }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void OnDispose(bool disposing)
        {
            if (disposing)
            {

            }
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
        /// Model Base Constructor
        /// </summary>
        public ModelBase()
        {
            
        }

        /// <summary>
        /// Database row counter
        /// </summary>
        /// <param name="fqtn">Fully qualified table name</param>
        /// <param name="colName">Name of the column to count against</param>
        /// <param name="whereParam">Where parameters to check against</param>
        /// <param name="sqlCon">Open SqlConnection to use</param>
        /// <returns>Count of rows, exceptions will default to 0</returns>
        public static int SqlRowCount(string fqtn, string colName, string whereParam, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($"SELECT COUNT({colName}) FROM {fqtn} WHERE {whereParam};", sqlCon))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Safely retrieve the string values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>string value or empty string</returns>
        public static string SafeGetString(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetString(reader.GetOrdinal(colName)) : string.Empty;
        }

        /// <summary>
        /// Safely retrieve the int values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>int value or 0</returns>
        public static int SafeGetInt32(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToInt32(reader.GetValue(reader.GetOrdinal(colName))) : 0;
        }

        /// <summary>
        /// Safely retrieve the double values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>double value or 0.00</returns>
        public static double SafeGetDouble(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToDouble(reader.GetValue(reader.GetOrdinal(colName))) : 0.00;
        }

        /// <summary>
        /// Safely retrieve the DateTime values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>DateTime value or '1999-01-01'</returns>
        public static DateTime SafeGetDateTime(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetDateTime(reader.GetOrdinal(colName)) : DateTime.MinValue;
        }

        /// <summary>
        /// Safely retrieve the boolean values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>bool value</returns>
        public static bool SafeGetBoolean(this SqlDataReader reader, string colName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(colName)))
            {
                if (reader.GetFieldType(reader.GetOrdinal(colName)) == typeof(int))
                {
                    return reader.SafeGetInt32(colName) >= 1;
                }
                else
                {
                    return reader.GetBoolean(reader.GetOrdinal(colName));
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Safely retrieve any value from a DataRow object
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type</typeparam>
        /// <param name="dRow">DataRow object</param>
        /// <param name="colName">Name of the column to get the value of</param>
        public static T SafeGetField<T>(this DataRow dRow, string colName)
        {
            if (dRow[colName] == DBNull.Value)
            {
                return default;
            }
            else
            {
                return dRow.Field<T>(colName);
            }
        }

        /// <summary>
        /// Safely retrieve any value from a DataRow object
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type</typeparam>
        /// <param name="dRow">DataRow object</param>
        /// <param name="colIndex">Index number of the column</param>
        public static T SafeGetField<T>(this DataRow dRow, int colIndex)
        {
            if (dRow[colIndex] == DBNull.Value)
            {
                return default;
            }
            else
            {
                return dRow.Field<T>(colIndex);
            }
        }
    }

}
