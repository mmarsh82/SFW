using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Quality
{
    public class CategoryFilter : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Load a datatable with all the QMS form automation information
        /// </summary>
        /// <param name="site">Place holder</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of form automation information</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [dbo].[DEFECT-CSTM_CategoryLinks]", sqlCon))
                        {
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
        /// Default Constructor
        /// </summary>
        public CategoryFilter()
        { }
    }
}
