using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    public class Location
    {
        #region Properties



        #endregion



        /// <summary>
        /// Default Contstructor
        /// </summary>
        public Location()
        { }

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to locations
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the location data results</returns>
        public static DataTable GetLocationData(SqlConnection sqlCon)
        {
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database};", sqlCon))
                    {
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

        #endregion

    }
}
