using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Product
{
    public class Diamond : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Get a DataTable of all sku objects with onhand values
        /// </summary>
        /// <param name="site">Place holder</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of all onhand values</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable _dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Diamond]", sqlCon))
                        {
                            adapter.Fill(_dt);
                            return _dt;
                        }
                    }
                }
                catch (Exception)
                {
                    return new DataTable();
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Diamond()
        { }
    }
}
