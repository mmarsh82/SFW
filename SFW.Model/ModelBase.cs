using System;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable
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
    }

}
