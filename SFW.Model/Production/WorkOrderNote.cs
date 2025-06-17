using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Production
{
    public class WorkOrderNote : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Get work order note's table
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>All work order notes in a datatable</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Notes] WHERE [Site] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException)
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
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public WorkOrderNote()
        { }

        /// <summary>
        /// Get a work order's notes
        /// </summary>
        /// <param name="type">Type of notes to return</param>
        /// <param name="includePriority">Add priority in the sorting</param>
        /// <param name="lookupValue">Value to use in searching for notes</param>
        /// <returns>A concatonation of notes into a string</returns>
        public static string GetNotes(string type, bool includePriority, params string[] lookupValue)
        {
            var _notes = string.Empty;
            var _sort = includePriority
                ? "[Priority], [LineID] ASC"
                : "[LineID] ASC";

            var _select = string.Empty;
            if (lookupValue.Length == 1)
            {
                _select = $"[NoteID] = '{lookupValue[0]}' AND [NoteType] = '{type}'";
            }
            else if (lookupValue.Length > 1)
            {
                _select = "(";
                foreach (var _lv in lookupValue)
                {
                    _select += _select.Length > 1 ? " OR " : "";
                    _select += $"[NoteID] = '{_lv}'";
                }
                _select += $") AND [NoteType] = '{type}'";
            }
            if (string.IsNullOrEmpty(_select))
            {
                return _notes;
            }
            foreach (DataRow _dr in MasterDataSet.Tables[new WorkOrderNote().GetType().Name].Select(_select, _sort))
            {
                if (!_notes.Contains($"{_dr.Field<string>("Note")}\n"))
                {
                    _notes += $"{_dr.Field<string>("Note")}\n";
                }
            }
            return _notes?.Trim('\n');
        }
    }
}
