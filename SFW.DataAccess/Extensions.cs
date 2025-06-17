using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace SFW.DataAccess
{
    public static class Extensions
    {
        /// <summary>
        /// Retrieve a DataTable from a SQL Server
        /// </summary>
        /// <param name="conString">Connection string for the SQL Connection</param>
        /// <param name="parameters">Optional: Parameters for the query</param>
        /// <returns>DataTable object</returns>
        public static DataTable GetTable(string conString, Dictionary<string, object> parameters = null)
        {
            var _tempTable = new DataTable();
            if (Connection.Sql != null && Connection.Sql.State != ConnectionState.Closed && Connection.Sql.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {Connection.Sql.Database}; {conString}", Connection.Sql))
                    {
                        if (parameters != null && parameters.Count() > 0)
                        {
                            foreach (var param in parameters)
                            {
                                adapter.SelectCommand.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
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

        /// <summary>
        /// Retrieve a value from a SQL Server
        /// </summary>
        /// <param name="conString">Connection string for the SQL Connection</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Scalar value as an object</returns>
        public static object GetValue(string conString, Dictionary<string, object> parameters)
        {
            if (Connection.Sql != null && Connection.Sql.State != ConnectionState.Closed && Connection.Sql.State != ConnectionState.Broken)
            {
                object _rtnVal = null;
                try
                {
                    using (SqlCommand cmd = new SqlCommand(conString, Connection.Sql))
                    {
                        if (parameters != null && parameters.Count() > 0)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                        _rtnVal = cmd.ExecuteScalar();
                    }
                    return _rtnVal;
                }
                catch (Exception)
                {
                    return _rtnVal;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Retrieve a value from a SQL Server
        /// </summary>
        /// <param name="conString">Connection string for the SQL Connection</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Scalar value as an object</returns>
        public static T GetValue<T>(string conString, Dictionary<string, object> parameters)
        {
            if (Connection.Sql != null && Connection.Sql.State != ConnectionState.Closed && Connection.Sql.State != ConnectionState.Broken)
            {
                object _rtnVal = null;
                try
                {
                    using (SqlCommand cmd = new SqlCommand(conString, Connection.Sql))
                    {
                        if (parameters != null && parameters.Count() > 0)
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                        _rtnVal = cmd.ExecuteScalar();
                    }
                    return _rtnVal.GetType() == typeof(T) ? (T)_rtnVal : default;
                }
                catch (Exception)
                {
                    return default;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
