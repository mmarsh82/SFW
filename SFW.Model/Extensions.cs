using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public static class Extensions
    {
        /// <summary>
        /// Refresh a table in the master dataset
        /// </summary>
        /// <param name="ds">Master dataset object</param>
        /// <param name="type">Type of object to refresh</param>
        /// <param name="table">Datatable object to merge in the refresh</param>
        public static void RefreshTable(this DataSet ds, Type type, DataTable table)
        {
            try
            {
                if (ds.Tables.Contains(type.Name))
                {
                    ds.Tables[type.Name].Clear();
                    if (table != null)
                    {
                        ds.Tables[type.Name].BeginLoadData();
                        ds.Tables[type.Name].Merge(table);
                        ds.Tables[type.Name].EndLoadData();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Refresh an entire module in the master dataset
        /// </summary>
        /// <param name="dataSet">Master data set to </param>
        /// <param name="mod">Module to refresh</param>
        /// <param name="site">Facility to load the data from</param>
        /// <param name="sqlCon">Open SqlConnection to use</param>
        /// <param name="machOrder">Optional: Machine order dictionary</param>
        public static void LoadTable(this DataSet dataSet, Module mod, int site, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open && mod.TableMethod.ReturnType == typeof(DataTable))
            {
                var _params = new object[2];
                _params[0] = site;
                _params[1] = sqlCon;
                var _instance = Activator.CreateInstance(mod.TableType);
                var _dt = mod.TableMethod.Invoke(_instance, _params);
                dataSet.RefreshTable(mod.TableType, (DataTable)_dt);
            }
        }

        /// <summary>
        /// Safely retrieve the string values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>string value or empty string</returns>
        public static string SafeGetString(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetString(reader.GetOrdinal(colName)) : string.Empty;
        }

        /// <summary>
        /// Safely retrieve the int values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>int value or 0</returns>
        public static int SafeGetInt32(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToInt32(reader.GetValue(reader.GetOrdinal(colName))) : 0;
        }

        /// <summary>
        /// Safely retrieve the double values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>double value or 0.00</returns>
        public static double SafeGetDouble(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToDouble(reader.GetValue(reader.GetOrdinal(colName))) : 0.00;
        }

        /// <summary>
        /// Safely retrieve the DateTime values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>DateTime value or '1999-01-01'</returns>
        public static DateTime SafeGetDateTime(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetDateTime(reader.GetOrdinal(colName)) : DateTime.MinValue;
        }

        /// <summary>
        /// Safely retrieve the boolean values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>bool value</returns>
        public static bool SafeGetBoolean(this SqlDataReader reader, string colName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(colName)))
            {
                if (reader.GetFieldType(reader.GetOrdinal(colName)) == typeof(int))
                {
                    return reader.SafeGetInt32(colName) >= 1;
                }
                else
                {
                    return reader.GetBoolean(reader.GetOrdinal(colName));
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Safely retrieve any value from a DataRow object
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type</typeparam>
        /// <param name="dRow">DataRow object</param>
        /// <param name="colName">Name of the column to get the value of</param>
        public static T SafeGetField<T>(this DataRow dRow, string colName)
        {
            try
            {
                if (dRow == null || dRow[colName] == DBNull.Value)
                {
                    return default;
                }
                else
                {
                    return dRow.Field<T>(colName);
                }
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Safely retrieve any value from a DataRow object
        /// </summary>
        /// <typeparam name="T">A generic parameter that specifies the return type</typeparam>
        /// <param name="dRow">DataRow object</param>
        /// <param name="colIndex">Index number of the column</param>
        public static T SafeGetField<T>(this DataRow dRow, int colIndex)
        {
            if (dRow[colIndex] == DBNull.Value)
            {
                return default;
            }
            else
            {
                return dRow.Field<T>(colIndex);
            }
        }

        /// <summary>
        /// Update the Component Wip Quntities based on the main part Wip Quantity
        /// </summary>
        /// <param name="comp">Component object</param>
        /// <param name="wipQty">Main Part Wip Quantity</param>
        public static void Update(this IList<Production.Wip.Component> compList, decimal wipQty)
        {
            var _balance = 0;
            if (compList != null)
            {
                foreach (var _comp in compList.Where(o => o.LotTraceable))
                {
                    _balance = Convert.ToInt32(Math.Ceiling(_comp.AssemblyQuantity * wipQty));
                    if (_comp.LotList != null)
                    {
                        _balance -= _comp.LotList.Where(o => o.QuantityLocked).Sum(o => int.TryParse(o.Quantity, out int i) ? i : 0);
                        var _divCount = _comp.LotList.Count(o => !o.QuantityLocked && o.Valid);
                        foreach (var _lot in _comp.LotList)
                        {
                            var _reqQty = int.TryParse(Math.Ceiling(wipQty * _comp.AssemblyQuantity).ToString(), out int i) ? i : 0;
                            _lot.RequiredQuantity = _reqQty;
                        }
                        if (_balance > 0 && _divCount > 0)
                        {
                            _comp.LotList.RaiseListChangedEvents = false;
                            foreach (var _lot in _comp.LotList.Where(o => o.Valid && !o.QuantityLocked))
                            {
                                var _qty = Convert.ToInt32(Math.Round(Convert.ToDouble(_balance)/Convert.ToDouble(_divCount), 0));
                                compList.FirstOrDefault(o => o.ProductNumber == _comp.ProductNumber).LotList.FirstOrDefault(o => o.ID == _lot.ID).Quantity = _qty.ToString();
                                _balance -= _qty;
                                _divCount--;
                            }
                            _comp.LotList.RaiseListChangedEvents = true;
                        }
                        else if (_balance < 0)
                        {
                            _comp.LotList.RaiseListChangedEvents = false;
                            foreach (var _lot in _comp.LotList.Where(o => o.Valid && !o.QuantityLocked))
                            {
                                compList.FirstOrDefault(o => o.ProductNumber == _comp.ProductNumber).LotList.FirstOrDefault(o => o.ID == _lot.ID).Quantity = "0";
                            }
                            _comp.LotList.RaiseListChangedEvents = true;
                        }
                    }
                }
            }
        }
    }
}
