using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Production
{
    public class PickComponent : Component, IModuleData
    {
        #region Properties

        public int RequiredQuantity { get; set; }
        public int OnHand { get; set; }
        public int Pickable { get; set; }
        public int Stock { get; set; }
        public int IssuedQuantity { get; set; }
        public int IssuedTotal { get; set; }
        public string InventoryType { get; set; }
        public string BackFlushLocation { get; set; }
        public string DefaultLocation { get; set; }
        public int Facility { get; set; }
        public decimal ScrapFactor { get; set; }
        public IList<string> DefectList { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Get a table of all pick lists for every SKU on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of pick lists</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_PickList] WHERE [Site] = @p1", sqlCon))
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
        public PickComponent()
        { }

        /// <summary>
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="woSeq">Work Order sequence</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <param name="machineName">Machine name</param>
        /// <returns>List of Component objects related to a picklist</returns>
        public static List<PickComponent> GetList(string woNbr, string woSeq, int balQty, string machineName)
        {
            var _tempList = new List<PickComponent>();
            var _rows = MasterDataSet.Tables[new PickComponent().GetType().Name].Select($"[WorkOrderID] LIKE '{woNbr}' AND [Routing] = '{woSeq}'");
            
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _tempList.Add(new PickComponent
                    {
                        ProductNumber = _row.Field<string>("ChildSkuID")
                        ,AssemblyQuantity = _row.Field<decimal>("AssemblyQuantity")
                        ,RequiredQuantity = _row.Field<int>("RequiredQuantity")
                        ,OnHand = _row.Field<int>("Onhand")
                        ,Pickable = _row.Field<int>("Pickable")
                        ,Stock = _row.Field<int>("Onhand")
                        ,ProductDescription = _row.Field<string>("Description")
                        ,IssuedQuantity = Convert.ToInt32(Math.Round(_row.Field<decimal>("AssemblyQuantity") * balQty, 0, MidpointRounding.AwayFromZero))
                        ,ProductMasterPrint = _row.Field<string>("MasterSkuID")
                        ,ProductUom = _row.Field<string>("Uom")
                        ,InventoryType = _row.Field<string>("Type")
                        ,IsLotTrace = _row.Field<string>("LotTrace") == "T"
                        ,BackFlushLocation = _row.Field<string>("Backflush")
                        ,DefaultLocation = Machine.GetPullLocation(machineName)
                        ,Facility = _row.Field<int>("Site")
                        ,IssuedTotal = _row.Field<int>("IssueTotal")
                        ,ScrapFactor = _row.Field<decimal>("ScrapFactor")
                    });
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Retrieve the scrap factor of a picklist component
        /// </summary>
        /// <param name="compNbr">Sku ID Number</param>
        /// <param name="woNbr">Work order number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <returns>List of Component objects related to a Bill of material</returns>
        public static decimal GetScrapFactor(string compNbr, string woNbr, string woSeq)
        {
            return MasterDataSet.Tables[new PickComponent().GetType().Name].Select($"[ChildSkuID] = '{compNbr}' AND [Routing] = '{woSeq}' AND [WorkOrderID] = '{woNbr}'").FirstOrDefault().Field<decimal>("ScrapFactor");
        }
    }
}
