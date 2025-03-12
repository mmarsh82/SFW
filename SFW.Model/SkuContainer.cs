using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    public class SkuContainer : ModelBase
    {
        #region Properties



        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public SkuContainer()
        { }

        #region Data Access

        /// <summary>
        /// Get the container data
        /// </summary>
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable GetContainerData(SqlConnection sqlCon)
        {
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM dbo.[SFW_Containers]", sqlCon))
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

        /// <summary>
        /// Get a containers location
        /// </summary>
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetContainerLocation(int ctnId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT TOP 1 [ContainerLocation] FROM dbo.[SFW_Containers] WHERE [ContainerID] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ctnId);
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

        #endregion
    }
}
