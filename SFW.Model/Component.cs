﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Component : ModelBase
    {
        #region Properties

        public string CompNumber { get; set; }
        public string CompDescription { get; set; }
        public int CurrentOnHand { get; set; }
        public int CurrentPickable { get; set; }
        public int RequiredQty { get; set; }
        public decimal AssemblyQty { get; set; }
        public int IssuedQty { get; set; }
        public string CompMasterPrint { get; set; }
        public string CompUom { get; set; }
        public string InventoryType { get; set; }
        public BindingList<CompWipInfo> WipInfo { get; set; }
        public bool IsLotTrace { get; set; }
        public string BackflushLoc { get; set; }
        public string PullLocation { get; set; }
        public int Facility { get; set; }
        public static bool WipInfoUpdating { get; set; }
        public static bool FromOtherChange { get; set; }
        public int IssueTotal { get; set; }

        #endregion

        /// <summary>
        /// Component Default Constructor 
        /// </summary>
        public Component()
        { }

        /// <summary>
        /// Overloaded constructor
        /// Will return a component object based on a part number
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="invType">Specific inventory type to single out</param>
        public Component(string partNbr, string invType)
        {
            var _rows = MasterDataSet.Tables["BOM"].Select($"[ParentSkuID] = '{partNbr}' AND [Type] = '{invType}'");
            if (_rows.Length > 0)
            {
                CompNumber = _rows.FirstOrDefault().Field<string>("ChildSkuID");
                AssemblyQty = _rows.FirstOrDefault().Field<decimal>("AssemblyQuantity");
            }
        }

        #region Data Access

        /// <summary>
        /// Get a table of all BOM's for every SKU on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of bill of materials</returns>
        public static DataTable GetComponentBomTable(int site, SqlConnection sqlCon)
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
        }

        /// <summary>
        /// Get a table of all pick lists for every SKU on file
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of pick lists</returns>
        public static DataTable GetComponentPickTable(int site, SqlConnection sqlCon)
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
        }

        #endregion

        /// <summary>
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="dataRows">Array of DataRow objects to translate to Component object properties</param>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="woSeq">Work Order sequence</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <param name="machineNbr">Machine name</param>
        /// <returns>List of Component objects related to a picklist</returns>
        public static List<Component> GetComponentPickList(string woNbr, string woSeq, int balQty, string machineName)
        {
            var _tempList = new List<Component>();
            WipInfoUpdating = false;
            var _rows = MasterDataSet.Tables["PL"].Select($"[WorkOrderID] LIKE '{woNbr}' AND [Routing] = '{woSeq}'");
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    var _lotRows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{_row.Field<string>("ChildSkuID")}' AND [Type] = 'Lot' AND [OnHand] <> 0");
                    var _dLotRows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{_row.Field<string>("ChildSkuID")}' AND [WorkOrderID] = '{woNbr}' AND [Type] = 'dLot' AND [OnHand] <> 0");
                    var _nLotRows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{_row.Field<string>("ChildSkuID")}' AND [Type] = 'nLot' AND [OnHand] <> 0");
                    _tempList.Add(new Component
                    {
                        CompNumber = _row.Field<string>("ChildSkuID")
                        ,AssemblyQty = _row.Field<decimal>("AssemblyQuantity")
                        ,RequiredQty = _row.Field<int>("RequiredQuantity")
                        ,CurrentOnHand = _row.Field<int>("Onhand")
                        ,CurrentPickable = _row.Field<int>("Pickable")
                        ,CompDescription = _row.Field<string>("Description")
                        ,IssuedQty = Convert.ToInt32(Math.Round(_row.Field<decimal>("AssemblyQuantity") * balQty, 0, MidpointRounding.AwayFromZero))
                        ,CompMasterPrint = _row.Field<string>("MasterSkuID")
                        ,CompUom = _row.Field<string>("Uom")
                        ,InventoryType = _row.Field<string>("Type")
                        ,IsLotTrace = _row.Field<string>("LotTrace") == "T"
                        ,BackflushLoc = _row.Field<string>("Backflush")
                        ,PullLocation = Machine.GetPullLocation(machineName)
                        ,Facility = _row.Field<int>("Site")
                        ,IssueTotal = _row.Field<int>("IssueTotal")
                        ,WipInfo = _row.Field<string>("LotTrace") == "T" 
                            ? new BindingList<CompWipInfo>() { new CompWipInfo(!string.IsNullOrEmpty(_row.Field<string>("Backflush")), _row.Field<string>("ChildSkuID"), _row.Field<string>("Uom"), _row.Field<int>("Site"), woNbr) }
                            : null
                    });
                    if (_tempList.Last().WipInfo != null)
                    {
                        _tempList.Last().WipInfo.ListChanged += WipInfo_ListChanged;
                    }
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Retrieve a list of components for a Sku
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <returns>List of Component objects related to a Bill of material</returns>
        public static List<Component> GetComponentBomList(string partNbr, string woSeq)
        {
            var _tempList = new List<Component>();
            WipInfoUpdating = false;
            var _rows = MasterDataSet.Tables["BOM"].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
            if (_rows.Length == 0 && woSeq != "10")
            {
                woSeq = "10";
                _rows = MasterDataSet.Tables["BOM"].Select($"[ParentSkuID] = '{partNbr}' AND [Routing] = '{woSeq}'");
            }
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _tempList.Add(new Component
                    {
                        CompNumber = _row.Field<string>("ChildSkuID")
                        ,AssemblyQty = _row.Field<decimal>("AssemblyQuantity")
                        ,CompDescription = _row.Field<string>("Description")
                        ,CompMasterPrint = _row.Field<string>("MasterSkuID")
                        ,CompUom = _row.Field<string>("Uom")
                    });
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        public static void WipInfo_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.ItemDeleted)
            {
                var _tempItem = e.NewIndex != -1 && !WipInfoUpdating ? ((BindingList<CompWipInfo>)sender)[e.NewIndex] : null;

                #region Lot Number Changed

                if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "LotNbr")
                {
                    FromOtherChange = true;
                    var _validLot = Lot.LotValidation(_tempItem.LotNbr, _tempItem.PartNbr);
                    if (_validLot)
                    {
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].IsValidLot = true;
                        var _machName = Machine.GetMachineName(((BindingList<CompWipInfo>)sender)[e.NewIndex].WorkOrderNumber, 'W');
                        var _loc = Machine.GetReceiptLocation(_machName);
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].RcptLoc = !string.IsNullOrEmpty(_loc) ? _loc : Lot.GetLotLocation(_tempItem.LotNbr);
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].BackFlushLoc = _tempItem.IsBackFlush ? Sku.GetBackFlushLoc(_tempItem.PartNbr) : null;
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].LotQty = 0;
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandQty = Lot.GetLotOnHandQuantity(_tempItem.LotNbr);
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandCalc = 0;
                        if (((BindingList<CompWipInfo>)sender).Count() == ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot))
                        {
                            ((BindingList<CompWipInfo>)sender).Add(new CompWipInfo(_tempItem.IsBackFlush, _tempItem.PartNbr, _tempItem.Uom, _tempItem.Facility, _tempItem.WorkOrderNumber) { BaseQty = _tempItem.BaseQty });
                            if (((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot) > 1)
                            {
                                ((BindingList<CompWipInfo>)sender)[0].ScrapList.ResetBindings();
                            }
                        }
                        var _base = Lot.GetDiamondNumber(_tempItem.LotNbr, 1);
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].BaseLot = _base != "error" ? _base : string.Empty;
                    }
                    else if (_tempItem.IsValidLot)
                    {
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].IsValidLot = false;
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].LotQty = 0;
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandCalc = 0;
                        while (((BindingList<CompWipInfo>)sender).Count(o => !o.IsValidLot) > 1)
                        {
                            ((BindingList<CompWipInfo>)sender).Remove(((BindingList<CompWipInfo>)sender).Last());
                        }
                        foreach (var _item in ((BindingList<CompWipInfo>)sender).Where(o => o.IsValidLot))
                        {
                            ((BindingList<CompWipInfo>)sender)[((BindingList<CompWipInfo>)sender).IndexOf(_item)].LotQty = 0;
                            ((BindingList<CompWipInfo>)sender)[((BindingList<CompWipInfo>)sender).IndexOf(_item)].OnHandCalc = 0;
                        }
                    }
                    FromOtherChange = false;
                }

                #endregion

                #region Lot Quantity Changed

                if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "LotQty" && !WipInfoUpdating)
                {
                    WipInfoUpdating = true;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].IsQtyLocked = !FromOtherChange;
                    if (((BindingList<CompWipInfo>)sender)[e.NewIndex].IsValidLot && ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot) == 1 && ((BindingList<CompWipInfo>)sender).Count(o => o.IsQtyLocked) == 0)
                    {
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].LotQty = ((BindingList<CompWipInfo>)sender)[e.NewIndex].BaseQty;
                    }
                    else if (((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot) > 0)
                    {
                        var _calcQtyPer = ((BindingList<CompWipInfo>)sender).Count(o => o.IsQtyLocked) > 0 && ((BindingList<CompWipInfo>)sender).Count(o => o.IsQtyLocked) != ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot)
                            ? (_tempItem.BaseQty - ((BindingList<CompWipInfo>)sender).Where(o => o.IsQtyLocked && o.IsValidLot).Sum(o => o.LotQty)) / ((BindingList<CompWipInfo>)sender).Count(o => !o.IsQtyLocked && o.IsValidLot)
                            : _tempItem.BaseQty / ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot);
                        if (_calcQtyPer != 0)
                        {
                            var _counter = 1;
                            foreach (var item in ((BindingList<CompWipInfo>)sender).Where(o => !o.IsQtyLocked && o.IsValidLot))
                            {
                                var index = ((BindingList<CompWipInfo>)sender).IndexOf(item);
                                ((BindingList<CompWipInfo>)sender)[index].LotQty = _calcQtyPer;
                                if (_counter == ((BindingList<CompWipInfo>)sender).Count(o => !o.IsQtyLocked && o.IsValidLot))
                                {
                                    if (((BindingList<CompWipInfo>)sender)[index].BaseQty > 0)
                                    {
                                        ((BindingList<CompWipInfo>)sender)[index].LotQty += ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty) < ((BindingList<CompWipInfo>)sender)[index].BaseQty
                                        ? ((BindingList<CompWipInfo>)sender)[index].BaseQty - ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty)
                                        : 0;
                                    }
                                    else
                                    {
                                        ((BindingList<CompWipInfo>)sender)[index].LotQty += ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty) > ((BindingList<CompWipInfo>)sender)[index].BaseQty
                                        ? ((BindingList<CompWipInfo>)sender)[index].BaseQty - ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty)
                                        : 0;
                                    }
                                }
                                _counter++;
                            }
                        }
                        else
                        {
                            foreach (var item in ((BindingList<CompWipInfo>)sender).Where(o => !o.IsQtyLocked && o.IsValidLot))
                            {
                                var index = ((BindingList<CompWipInfo>)sender).IndexOf(item);
                                ((BindingList<CompWipInfo>)sender)[index].LotQty = 0;
                            }
                        }
                    }
                    WipInfoUpdating = false;
                    if (!FromOtherChange)
                    {
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandCalc = 0;
                    }
                }

                #endregion

                #region On Hand Quantity Calculation Changed

                if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "OnHandCalc" && !WipInfoUpdating)
                {
                    WipInfoUpdating = true;
                    foreach (var item in ((BindingList<CompWipInfo>)sender).Where(o => o.IsValidLot))
                    {
                        var _scrap = item.ScrapList.Where(o => int.TryParse(o.Quantity, out int a)).Sum(o => Convert.ToInt32(o.Quantity));
                        var index = ((BindingList<CompWipInfo>)sender).IndexOf(item);
                        ((BindingList<CompWipInfo>)sender)[index].OnHandCalc = int.TryParse(((BindingList<CompWipInfo>)sender)[index].LotQty.ToString(), out int i)
                            ? ((BindingList<CompWipInfo>)sender)[index].OnHandQty - (i + _scrap)
                            : ((BindingList<CompWipInfo>)sender)[index].OnHandQty - _scrap;
                    }
                    WipInfoUpdating = false;
                }

                #endregion

                #region List Reset

                if (e.ListChangedType == ListChangedType.Reset && ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot) > 0)
                {
                    FromOtherChange = true;
                    if (((BindingList<CompWipInfo>)sender).Count(o => o.IsQtyLocked) != ((BindingList<CompWipInfo>)sender).Count())
                    {
                        ((BindingList<CompWipInfo>)sender).FirstOrDefault(o => !o.IsQtyLocked).LotQty = 0;
                    }
                    ((BindingList<CompWipInfo>)sender)[0].OnHandCalc = 0;
                    FromOtherChange = false;
                }

                #endregion
            }
        }
    }

    /// <summary>
    /// Component Object Extensions class
    /// </summary>
    public static class CompExtensions
    {
        /// <summary>
        /// Update the Component Wip Quntities based on the main part Wip Quantity
        /// </summary>
        /// <param name="comp">Component object</param>
        /// <param name="wipQty">Main Part Wip Quantity</param>
        public static void UpdateWipInfo(this Component comp, decimal wipQty)
        {
            if (comp.WipInfo != null)
            {
                foreach (var c in comp.WipInfo)
                {
                    c.BaseQty = Convert.ToInt32(Math.Round(comp.AssemblyQty * wipQty, 0));
                }
                if (comp.WipInfo.Count(o => o.IsValidLot) > 0)
                {
                    comp.WipInfo[0].LotNbr = comp.WipInfo[0].LotNbr;
                }
            }
        }
    }
}
