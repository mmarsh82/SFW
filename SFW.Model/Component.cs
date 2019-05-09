using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Component
    {
        #region Properties

        public string CompNumber { get; set; }
        public string CompDescription { get; set; }
        public int CurrentOnHand { get; set; }
        public int RequiredQty { get; set; }
        public double AssemblyQty { get; set; }
        public int IssuedQty { get; set; }
        public string CompMasterPrint { get; set; }
        public string CompUom { get; set; }
        public List<Lot> LotList { get; set; }
        public List<Lot> NonLotList { get; set; }
        public List<Lot> DedicatedLotList { get; set; }
        public string InventoryType { get; set; }
        public BindingList<CompWipInfo> WipInfo { get; set; }
        public bool IsLotTrace { get; set; }
        public string BackflushLoc { get; set; }
        public static bool WipInfoUpdating { get; set; }

        #endregion

        /// <summary>
        /// Component Default Constructor 
        /// </summary>
        public Component()
        { }

        /// <summary>
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of Component objects</returns>
        public static List<Component> GetComponentList(string woNbr, int balQty, SqlConnection sqlCon)
        {
            var _tempList = new List<Component>();
            WipInfoUpdating = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT
	                                                            SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component', a.[Qty_Per_Assy] as 'Qty Per', a.[Qty_Reqd] as 'Req Qty',
	                                                            b.[Qty_On_Hand] as 'On Hand', b.[Wip_Rec_Loc] as 'Backflush',
	                                                            c.[Description], c.[Drawing_Nbrs], c.[Um], c.[Inventory_Type], c.[Lot_Trace]
                                                            FROM
	                                                            [dbo].[PL-INIT] a
                                                            RIGHT JOIN
	                                                            [dbo].[IPL-INIT] b ON b.[Part_Nbr] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                            RIGHT JOIN
	                                                            [dbo].[IM-INIT] c ON c.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                            WHERE
	                                                            a.[ID] LIKE CONCAT(@p1, '%')
                                                            ORDER BY
                                                                Component;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new Component
                                    {
                                        CompNumber = reader.SafeGetString("Component"),
                                        AssemblyQty = reader.SafeGetDouble("Qty Per"),
                                        RequiredQty = reader.SafeGetInt32("Req Qty"),
                                        CurrentOnHand = reader.SafeGetInt32("On Hand"),
                                        CompDescription = reader.SafeGetString("Description"),
                                        IssuedQty = Convert.ToInt32(Math.Round(reader.SafeGetDouble("Qty Per") * balQty, 0, MidpointRounding.AwayFromZero)),
                                        CompMasterPrint = reader.SafeGetString("Drawing_Nbrs"),
                                        CompUom = reader.SafeGetString("Um"),
                                        InventoryType = reader.SafeGetString("Inventory_Type"),
                                        IsLotTrace = reader.SafeGetString("Lot_Trace") == "T",
                                        BackflushLoc = reader.SafeGetString("Backflush"),
                                        LotList = reader.IsDBNull(0)
                                            ? new List<Lot>()
                                            : Lot.GetOnHandLotList(reader.SafeGetString("Component"), sqlCon),
                                        DedicatedLotList = reader.IsDBNull(0)
                                            ? new List<Lot>()
                                            : Lot.GetDedicatedLotList(reader.SafeGetString("Component"), woNbr, sqlCon),
                                        WipInfo = new BindingList<CompWipInfo>()
                                    });
                                    _tempList[_tempList.Count - 1].WipInfo.Add(new CompWipInfo(!string.IsNullOrEmpty(_tempList[_tempList.Count - 1].BackflushLoc), _tempList[_tempList.Count - 1].CompNumber));
                                    _tempList[_tempList.Count - 1].WipInfo.ListChanged += WipInfo_ListChanged;
                                    _tempList[_tempList.Count - 1].NonLotList = _tempList[_tempList.Count - 1].LotList.Count == 0 && !reader.IsDBNull(0)
                                        ? Lot.GetOnHandNonLotList(reader.SafeGetString("Component"), sqlCon)
                                        : new List<Lot>();
                                }
                            }
                        }
                    }
                    return _tempList;
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
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

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        public static void WipInfo_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "LotNbr")
            {
                WipInfoUpdating = true;
                if (Lot.LotValidation(((BindingList<CompWipInfo>)sender)[e.NewIndex].LotNbr, ((BindingList<CompWipInfo>)sender)[e.NewIndex].PartNbr, ModelBase.ModelSqlCon))
                {
                    if (!((BindingList<CompWipInfo>)sender)[e.NewIndex].IsBackFlush)
                    {
                        ((BindingList<CompWipInfo>)sender)[e.NewIndex].RcptLoc = Lot.GetLotLocation(((BindingList<CompWipInfo>)sender)[e.NewIndex].LotNbr, ModelBase.ModelSqlCon);
                    }
                    var _unlocked = ((BindingList<CompWipInfo>)sender).Count(o => !string.IsNullOrEmpty(o.RcptLoc)) - ((BindingList<CompWipInfo>)sender).Count(c => c.QtyLock && !string.IsNullOrEmpty(c.RcptLoc));
                    var _newBase = ((BindingList<CompWipInfo>)sender)[e.NewIndex].BaseQty - ((BindingList<CompWipInfo>)sender).Where(w => w.QtyLock).Sum(s => s.LotQty);
                    var _counter = 1;
                    if (_unlocked > 0 && _newBase > 0)
                    {
                        foreach (var c in ((BindingList<CompWipInfo>)sender).Where(w => !w.QtyLock && !string.IsNullOrEmpty(w.RcptLoc)))
                        {
                            c.LotQty = Convert.ToInt32(Math.Round(Convert.ToDouble(_newBase) / Convert.ToDouble(_unlocked), 0));
                            if (_counter == _unlocked && ((BindingList<CompWipInfo>)sender).Sum(s => s.LotQty) != ((BindingList<CompWipInfo>)sender)[0].BaseQty)
                            {
                                c.LotQty = 0;
                                c.LotQty = ((BindingList<CompWipInfo>)sender)[0].BaseQty - ((BindingList<CompWipInfo>)sender).Sum(s => s.LotQty);
                            }
                            _counter++;
                        }
                    }
                    if (!string.IsNullOrEmpty(((BindingList<CompWipInfo>)sender)[((BindingList<CompWipInfo>)sender).Count - 1].RcptLoc))
                    {
                        ((BindingList<CompWipInfo>)sender).Add(new CompWipInfo(((BindingList<CompWipInfo>)sender)[0].IsBackFlush, ((BindingList<CompWipInfo>)sender)[0].PartNbr) { BaseQty = ((BindingList<CompWipInfo>)sender)[0].BaseQty });
                    }
                }
                WipInfoUpdating = false;
            }
            else if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "LotQty" && !WipInfoUpdating)
            {
                WipInfoUpdating = true;
                ((BindingList<CompWipInfo>)sender)[e.NewIndex].QtyLock = true;
                var _unlocked = ((BindingList<CompWipInfo>)sender).Count(c => !string.IsNullOrEmpty(c.LotNbr)) - ((BindingList<CompWipInfo>)sender).Count(c => c.QtyLock && !string.IsNullOrEmpty(c.LotNbr));
                var _newBase = ((BindingList<CompWipInfo>)sender)[e.NewIndex].BaseQty - ((BindingList<CompWipInfo>)sender).Where(w => w.QtyLock).Sum(s => s.LotQty);
                var _counter = 1;
                if (_unlocked > 0 && _newBase > 0)
                {
                    foreach (var c in ((BindingList<CompWipInfo>)sender).Where(w => !w.QtyLock && !string.IsNullOrEmpty(w.LotNbr)))
                    {
                        c.LotQty = Convert.ToInt32(Math.Round(Convert.ToDouble(_newBase) / Convert.ToDouble(_unlocked), 0));
                        if (_counter == _unlocked && ((BindingList<CompWipInfo>)sender).Sum(s => s.LotQty) != ((BindingList<CompWipInfo>)sender)[0].BaseQty)
                        {
                            c.LotQty = 0;
                            c.LotQty = ((BindingList<CompWipInfo>)sender)[0].BaseQty - ((BindingList<CompWipInfo>)sender).Sum(s => s.LotQty);
                        }
                        _counter++;
                    }
                }

                WipInfoUpdating = false;
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
        public static void UpdateWipInfo(this Component comp, double wipQty)
        {
            foreach (var c in comp.WipInfo)
            {
                c.BaseQty = Convert.ToInt32(Math.Round(comp.AssemblyQty * wipQty, 0));
            }
        }
    }
}
