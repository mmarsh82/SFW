using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Production
{
    public class BillComponent : Component, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Get a table of all BOM's for every SKU on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of bill of materials</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_BOM] WHERE [Site] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.Add("p1", SqlDbType.Int).Value = site;
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
        public BillComponent()
        { }

        /// <summary>
        /// Overloaded constructor
        /// Will return a component object based on a part number
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="invType">Specific inventory type to single out</param>
        public BillComponent(string partNbr, string invType)
        {
            var _rows = MasterDataSet.Tables[new BillComponent().GetType().Name].Select($"[ParentSkuID] = '{partNbr}' AND [Type] = '{invType}'");
            if (_rows.Length > 0)
            {
                ProductNumber = _rows.FirstOrDefault().Field<string>("ChildSkuID");
                AssemblyQuantity = _rows.FirstOrDefault().Field<decimal>("AssemblyQuantity");
            }
        }

        /// <summary>
        /// Retrieve a list of components for a Sku
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <returns>List of Component objects related to a Bill of material</returns>
        public static List<BillComponent> GetList(string partNbr, string woSeq)
        {
            var _tempList = new List<BillComponent>();
            try
            {
                var _rows = MasterDataSet.Tables[typeof(BillComponent).Name].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
                if (_rows.Length == 0 && woSeq != "10")
                {
                    woSeq = "10";
                    _rows = MasterDataSet.Tables[typeof(BillComponent).Name].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
                }
                if (_rows.Length > 0)
                {
                    foreach (var _row in _rows)
                    {
                        _tempList.Add(new BillComponent
                        {
                            ProductNumber = _row.Field<string>("ChildSkuID")
                            ,AssemblyQuantity = _row.Field<decimal>("AssemblyQuantity")
                            ,ProductDescription = _row.Field<string>("Description")
                            ,ProductMasterPrint = _row.Field<string>("MasterSkuID")
                            ,ProductUom = _row.Field<string>("Uom")
                        });
                    }
                }
                return _tempList;
            }
            catch
            {
                return _tempList;
            }
        }

        /// <summary>
        /// Retrieve a collection of components for a Sku
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <returns>Collection component numbers/descriptions related to a Bill of material</returns>
        public static ObservableCollection<string> GetFullNameCollection(string partNbr, string woSeq)
        {
            var _tempCol = new ObservableCollection<string> { partNbr };
            var _rows = MasterDataSet.Tables[new BillComponent().GetType().Name].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
            if (_rows.Length == 0 && woSeq != "10")
            {
                woSeq = "10";
                _rows = MasterDataSet.Tables[new BillComponent().GetType().Name].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
            }
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _tempCol.Add(_row.Field<string>("ChildSkuID"));
                }
            }
            return _tempCol;
        }
    }
}
