using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class Tool : ModelBase
    {
        #region Properties

        public string ID { get; set; }
        public string SkuID { get; set; }
        public string ToolID { get; set; }
        public string MachineID { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Tool()
        { }

        #region Data Access

        /// <summary>
        /// Get a Table of all tools in the database
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A datatable of all tool's</returns>
        public static DataTable GetTools(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM SFW_Tools", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        return _tempTable;
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
        /// Get a Sku's tool list
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <returns>A list of tool's associated with the Sku</returns>
        public static List<Tool> GetToolList(string skuNbr)
        {
            var _tempList = new List<Tool>();
            var _rows = MasterDataSet.Tables["TL"].Select($"[SkuID] = '{skuNbr}'");
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _tempList.Add(new Tool
                    {
                        ID = _row.Field<string>("ID")
                        ,SkuID = _row.Field<string>("SkuID")
                        ,ToolID = _row.Field<string>("ToolID")
                        ,MachineID = _row.Field<string>("MachineID")
                    });
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Get a Sku's tool list
        /// </summary>
        /// <param name="skuNbr">Sku ID Number</param>
        /// <param name="seq">Sku's sequence</param>
        /// <param name="facility">Current user facility number</param>
        /// <returns>A list of tool's associated with the Sku</returns>
        public static List<Tool> GetToolList(string skuNbr, string seq, int facCode)
        {
            try
            {
                var _tempList = new List<Tool>();
                var _rows = MasterDataSet.Tables["TL"].Select($"[ID] = '{skuNbr}|0{facCode}*{seq}'");
                if (_rows.Length > 0)
                {
                    foreach (var _row in _rows)
                    {
                        _tempList.Add(new Tool
                        {
                            ID = _row.Field<string>("ID")
                            ,
                            SkuID = _row.Field<string>("SkuID")
                            ,
                            ToolID = _row.Field<string>("ToolID")
                            ,
                            MachineID = _row.Field<string>("MachineID")
                        });
                    }
                }
                return _tempList;
            }
            catch
            {
                return null;
            }
        }
    }
}
