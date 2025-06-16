using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Sales
{
    public class SalesOrderNote : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Get the sales order internal comments table
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_SalesNotes]", sqlCon))
                        {
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
                }
                catch (SqlException sqlEx)
                {
                    return new DataTable();
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

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SalesOrderNote()
        { }

        /// <summary>
        /// Get the sales order internal comments
        /// </summary>
        /// <param name="soNumber">Sales Order to get the internal comments from</param>
        /// <param name="type">Type of note I = Special Instructions, C = Internal Comments</param>
        /// <returns>Internal comments in a string</returns>
        public static string GetNote(string soNumber, char type)
        {
            var _note = string.Empty;
            if (MasterDataSet.Tables.Contains(new SalesOrderNote().GetType().Name))
            {
                var _rows = MasterDataSet.Tables[new SalesOrderNote().GetType().Name].Select($"[SalesID] = '{soNumber}' AND [Type] = '{type}'");
                if (_rows.Length > 0)
                {
                    foreach (var _row in _rows)
                    {
                        _note += $"{_row.Field<string>("Comments")}\n";
                    }
                    return string.IsNullOrEmpty(_note) ? null : _note?.Trim('\n');
                }
                return null;
            }
            return null;
        }
    }
}
