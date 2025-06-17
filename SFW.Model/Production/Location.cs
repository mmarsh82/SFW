using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Production
{
    public class Location : ModelBase, IModuleData
    {
        #region Properties



        #endregion

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to locations
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the location data results</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Locations]", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_dt);
                        }
                        return _dt;
                    }
                    catch (SqlException)
                    {
                        return _dt;
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
        /// Default Contstructor
        /// </summary>
        public Location()
        { }

        /// <summary>
        /// Validates that the location entered is a valid M2k location
        /// </summary>
        /// <param name="location">location to validate</param>
        /// <param name="facCode">Facility code</param>
        /// <returns>valid location as bool</returns>
        public static bool Valid(string location, int facCode)
        {
            return MasterDataSet.Tables[new Location().GetType().Name].Select($"[Location] = '{location}' AND [Site] = {facCode}").Length > 0;
        }
    }
}
