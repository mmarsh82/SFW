using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public abstract class ModelBase : IDisposable, INotifyPropertyChanged
    {
        #region Properties

        public static SqlConnection ModelSqlCon { get; set; }
        public static DataSet MasterDataSet { get; set; }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void OnDispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Reflects changes from the ViewModel properties to the View
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Model Base Constructor
        /// </summary>
        public ModelBase()
        { }

        /// <summary>
        /// Builds the Application Master Dataset that all part of the application relies on for DataAccess
        /// </summary>
        /// <param name="machOrder"></param>
        /// <param name="site"></param>
        /// <param name="sqlCon"></param>
        public static IReadOnlyDictionary<bool, string> BuildMasterDataSet(IReadOnlyDictionary<string, int> machOrder, string site, SqlConnection sqlCon)
        {
            ModelSqlCon = sqlCon;
            var _rtnDict = new Dictionary<bool, string>();
            try
            {
                using (var _tempDS = new DataSet())
                {
                    _tempDS.DataSetName = $"{site}DataSet";

                    _tempDS.Tables.Add(Machine.GetScheduleData(machOrder, ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "Master";

                    _tempDS.Tables.Add(SalesOrder.GetScheduleData(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "SalesMaster";

                    _tempDS.Tables.Add(Tool.GetTools(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "TL";

                    _tempDS.Tables.Add(Component.GetComponentBomTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "BOM";

                    _tempDS.Tables.Add(Component.GetComponentPickTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "PL";

                    _tempDS.Tables.Add(WorkOrder.GetNotesTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "WoNotes";

                    _tempDS.Tables.Add(Sku.GetInstructions(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "WI";

                    _tempDS.Tables.Add(Lot.GetLotTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "LOT";

                    _tempDS.Tables.Add(SalesOrder.GetNotesTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "SoNotes";

                    _tempDS.Tables.Add(Machine.GetMachineTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "WC";

                    _tempDS.Tables.Add(Sku.GetSkuTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "SKU";

                    _tempDS.Tables.Add(Sku.GetLocationTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "LOC";

                    _tempDS.Tables.Add(CrewMember.GetCrewTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "CREW";

                    _tempDS.Tables.Add(Lot.GetDiamondTable(ModelSqlCon));
                    _tempDS.Tables[_tempDS.Tables.Count - 1].TableName = "Diamond";

                    MasterDataSet = _tempDS;
                }
                _rtnDict.Add(false, string.Empty);
                return _rtnDict;
            }
            catch (Exception ex)
            {
                _rtnDict.Add(true, ex.Message);
                return _rtnDict;
            }
        }

        /// <summary>
        /// Database row counter
        /// </summary>
        /// <param name="fqtn">Fully qualified table name</param>
        /// <param name="colName">Name of the column to count against</param>
        /// <param name="whereParam">Where parameters to check against</param>
        /// <param name="sqlCon">Open SqlConnection to use</param>
        /// <returns>Count of rows, exceptions will default to 0</returns>
        public static int SqlRowCount(string fqtn, string colName, string whereParam, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($"SELECT COUNT({colName}) FROM {fqtn} WHERE {whereParam};", sqlCon))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="table"></param>
        public static void RefreshTable(this DataSet dataSet, Tables table)
        {
            var _dt = new DataTable();
            switch (table)
            {
                case Tables.LOT:
                    _dt = Lot.GetLotTable(ModelBase.ModelSqlCon);
                    break;
            }
            dataSet.Tables[table.ToString()].Clear();
            dataSet.Tables[table.ToString()].Merge(_dt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="machOrder"></param>
        public static void Refresh(this DataSet dataSet, IReadOnlyDictionary<string, int> machOrder)
        {
            foreach (DataTable _table in dataSet.Tables)
            {
                Enum.TryParse(_table.TableName, out Tables _tName);
                var _dt = new DataTable();
                switch (_tName)
                {
                    case Tables.BOM:
                        _dt = Component.GetComponentBomTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.CREW:
                        _dt = CrewMember.GetCrewTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.LOC:
                        _dt = Sku.GetLocationTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.LOT:
                        _dt = Lot.GetLotTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.Master:
                        _dt = Machine.GetScheduleData(machOrder, ModelBase.ModelSqlCon);
                        break;
                    case Tables.PL:
                        _dt = Component.GetComponentPickTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.SalesMaster:
                        _dt = ModelBase.ModelSqlCon.Database.Contains("WCCO") ? SalesOrder.GetScheduleData(ModelBase.ModelSqlCon) : new DataTable();
                        break;
                    case Tables.SKU:
                        _dt = Sku.GetSkuTable(ModelBase.ModelSqlCon);
                        break;
                    case Tables.SoNotes:
                        _dt = SalesOrder.GetNotesTable(ModelBase.ModelSqlCon);
                        break;
                }
                dataSet.Tables[_table.TableName].Clear();
                dataSet.Tables[_table.TableName].Merge(_dt);
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
            if (dRow[colName] == DBNull.Value)
            {
                return default;
            }
            else
            {
                return dRow.Field<T>(colName);
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
    }

    public enum Tables
    {
        Master = 0,
        SalesMaster = 1,
        TL = 2,
        BOM = 3,
        PL = 4,
        WoNotes = 5,
        WI = 6,
        LOT = 7,
        SoNotes = 8,
        WC = 9,
        SKU = 10,
        LOC = 11,
        CREW = 12
    }
}
