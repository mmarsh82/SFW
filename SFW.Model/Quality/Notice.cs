using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Quality
{
    public class Notice : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Load a datatable with all the NCR Notice information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of NCR Notice information</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [dbo].[SFW_NcrNotice] ncr WHERE ncr.[Site] = @p1 ORDER BY ncr.[RevisionDateTime] DESC", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_dt);
                        }
                        using (DataTable _sdt = new DataTable())
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT
	CASE WHEN ISNUMERIC([NcrId]) = 0
		THEN REPLACE(UPPER([NcrId]), 'NCR', '')
		ELSE [NcrId] end as 'NcrId'
	,SUM([Quantity]) as 'Quantity'
	,CAST(ROUND(SUM([ScrapCost]),3) as numeric(12,3)) as 'Cost'
FROM
	[dbo].[SFW_ScrapCost]
WHERE
	[NcrId] IS NOT NULL AND [NcrId] <> ''
GROUP BY
	[NcrId]", sqlCon))
                            {
                                adapter.Fill(_sdt);
                            }
                            _dt.Columns.Add(new DataColumn("ScrapQuantity", typeof(int)));
                            _dt.Columns.Add(new DataColumn("ScrapCost", typeof(double)));
                            foreach (DataRow _row in _sdt.Rows)
                            {
                                var _id = int.TryParse(_row.ItemArray[0].ToString(), out int i) ? i : 0;
                                var _qty = int.TryParse(_row.ItemArray[1].ToString(), out i) ? i : 0;
                                var _cost = double.TryParse(_row.ItemArray[2].ToString(), out double d) ? d : 0.0;
                                if (_id > 0 && _qty > 0 && _cost > 0)
                                {
                                    var _dtRows = _dt.Select($"[NcrId] = {_id}");
                                    foreach (var _dtRow in _dtRows)
                                    {
                                        var _index = _dt.Rows.IndexOf(_dtRow);
                                        _dt.Rows[_index].SetField("ScrapQuantity", _qty);
                                        _dt.Rows[_index].SetField("ScrapCost", _cost);
                                    }
                                }
                            }
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
        public Notice()
        { }

    }
}
