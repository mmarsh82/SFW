using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Product
{
    public class SkuStructure : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Load a datatable with all the structure information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of structure information</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_ProductStructure] WHERE [Site] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_dt);
                        }
                        return _dt;
                    }
                    catch (SqlException sqlEx)
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
        public SkuStructure()
        { }

        /// <summary>
        /// Get a Sku's structure
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, int> GetStructure(string partNbr, string site)
        {
            var _returnList = new Dictionary<Sku, int>
            {
                { new Sku(partNbr, 'S', int.Parse(site), true), 0 }
            };
            _returnList.First().Key.Location = "1";
            var _levelCount = 0;
            var _query = string.Empty;
            var _partList = new List<string> { $"{partNbr}|0{site}" };
            var _search = "[Part]";
            var _reverse = false;
            while (_partList.Count > 0)
            {
                _levelCount = _reverse ? _levelCount - 1 : _levelCount + 1;
                _query = string.Empty;
                foreach (var _part in _partList)
                {
                    _query += string.IsNullOrEmpty(_query) ? $"({_search} = '{_part}'" : $" OR {_search} = '{_part}'";
                }
                _query += ") AND [Status] = 'A'";
                if (_partList.Count > 100)
                {
                    return null;
                }
                var _temp = MasterDataSet.Tables["PS"].Select(_query);
                _partList.Clear();
                foreach (var _sku in _temp)
                {
                    var _parPart = _reverse ? _sku.Field<string>("Part").Split('|') : _sku.Field<string>("Parent").Split('|');
                    var _childPart = _reverse ? _sku.Field<string>("Parent").Split('|') : _sku.Field<string>("Part").Split('|');
                    if (Sku.Exists(_parPart[0], false))
                    {
                        _returnList.Add(new Sku(_parPart[0], 'S', int.Parse(_parPart[1]), true), _levelCount);
                        if (_returnList.Count(o => o.Key.SkuNumber == _childPart[0] && o.Key.Facility == int.Parse(_childPart[1])) > 0 && _childPart[0] != partNbr)
                        {
                            _returnList.Last().Key.Location = $"{_returnList.FirstOrDefault(o => o.Key.SkuNumber == _childPart[0] && o.Key.Facility == int.Parse(_childPart[1])).Key.Location}.{_returnList.Count()}";
                        }
                        else
                        {
                            _returnList.Last().Key.Location = _returnList.Count().ToString();
                        }
                        if (_reverse)
                        {
                            _partList.Add(_sku.Field<string>("Part"));
                        }
                        else
                        {
                            _partList.Add(_sku.Field<string>("Parent"));
                        }
                    }
                }
                if (_partList.Count == 0 && !_reverse)
                {
                    _search = "[Parent]";
                    _partList = new List<string> { $"{partNbr}|0{site}" };
                    _levelCount = 0;
                    _reverse = true;
                }
            }
            return _returnList;
        }
    }
}
