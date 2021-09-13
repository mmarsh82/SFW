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
        public int CurrentPickable { get; set; }
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
        public static bool FromOtherChange { get; set; }

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
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="invType">Optional: Specific inventory type to single out</param>
        public Component(string partNbr, SqlConnection sqlCon, string invType = "")
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*',a.[ID], 0) + 1, LEN(a.[ID])) as 'PartNbr'
                                                                    ,a.Qty_Per_Assy
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                LEFT OUTER JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*',a.[ID], 0) + 1, LEN(a.[ID]))
                                                                WHERE
	                                                                a.[ID] LIKE CONCAT(@p1, '*%') AND b.[Inventory_Type] = @p2;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", invType);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    CompNumber = reader.SafeGetString("PartNbr");
                                    AssemblyQty = reader.SafeGetDouble("Qty_Per_Assy");
                                }
                            }
                        }
                    }
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
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="operation">Operation number</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of Component objects related to a picklist</returns>
        public static List<Component> GetComponentPickList(string woNbr, string operation, int balQty, SqlConnection sqlCon)
        {
            var _tempList = new List<Component>();
            WipInfoUpdating = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _count = 0;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT COUNT(a.[ID]) FROM [dbo].[PL-INIT] a WHERE a.[ID] LIKE CONCAT(@p1, '%') AND (a.[Routing_Seq] = @p2 OR a.[Routing_Seq] IS NULL)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", operation);
                        int.TryParse(cmd.ExecuteScalar().ToString(), out _count);
                    }
                    operation = _count == 0 ? "10" : operation;
                    var _routCmd = operation == "10" ? "(a.[Routing_Seq] = @p2 OR a.[Routing_Seq] IS NULL)" : "a.[Routing_Seq] = @p2";
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component',
	                                                                a.[Qty_Per_Assy] as 'Qty Per',
	                                                                a.[Qty_Reqd] as 'Req Qty',
	                                                                b.[Qty_On_Hand] as 'On Hand',
	                                                                (SELECT SUM(aa.[OH_Qty_By_Loc]) FROM [dbo].[IPL-INIT_Location_Data] aa WHERE aa.[ID1] = b.[Part_Nbr] AND aa.[Loc_Pick_Avail_Flag] = 'Y') as 'Pickable',
	                                                                b.[Wip_Rec_Loc] as 'Backflush',
	                                                                c.[Description],
	                                                                c.[Drawing_Nbrs],
	                                                                c.[Um],
	                                                                c.[Inventory_Type],
	                                                                c.[Lot_Trace],
	                                                                a.[Routing_Seq]
                                                                FROM
	                                                                [dbo].[PL-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] c ON c.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                WHERE
	                                                                a.[ID] LIKE CONCAT(@p1, '%') AND {_routCmd} AND a.[Qty_Reqd] > 0
                                                                ORDER BY
                                                                    Lot_Trace DESC, Component;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", operation);
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
                                        CurrentPickable = reader.SafeGetInt32("Pickable"),
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
                                    _tempList[_tempList.Count - 1].WipInfo.Add
                                        (
                                            new CompWipInfo(!string.IsNullOrEmpty(_tempList[_tempList.Count - 1].BackflushLoc), _tempList[_tempList.Count - 1].CompNumber, _tempList[_tempList.Count - 1].CompUom)
                                        );
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
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="dataRows">Array of DataRow objects to translate to Component object properties</param>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <returns>List of Component objects related to a picklist</returns>
        public static List<Component> GetComponentPickList(DataSet dataSet, DataRow[] dataRows, string woNbr, int balQty)
        {
            var _tempList = new List<Component>();
            foreach (DataRow _row in dataRows)
            {
                _tempList.Add(new Component
                {
                    CompNumber = _row.Field<string>("Component")
                    ,AssemblyQty = _row.Field<double>("Qty Per")
                    ,RequiredQty = _row.Field<int>("Req Qty")
                    ,CurrentOnHand = _row.Field<int>("On Hand")
                    ,CurrentPickable = _row.Field<int>("Pickable")
                    ,CompDescription = _row.Field<string>("Description")
                    ,IssuedQty = Convert.ToInt32(Math.Round(_row.Field<double>(2) * balQty, 0, MidpointRounding.AwayFromZero))
                    ,CompMasterPrint = _row.Field<string>("Drawing_Nbrs")
                    ,CompUom = _row.Field<string>("Um")
                    ,InventoryType = _row.Field<string>("Inventory_Type")
                    ,IsLotTrace = _row.Field<string>("Lot_Trace") == "T"
                    ,BackflushLoc = _row.Field<string>("Backflush")
                    ,LotList = Lot.DataRowToLotList(dataSet.Tables["OH"].Select($"[ID] = '{_row.Field<string>("Component")}'"), "Lot")
                    ,DedicatedLotList = Lot.DataRowToLotList(dataSet.Tables["OH"].Select($"[ID] = '{_row.Field<string>("Component")}' AND [WO] = '{woNbr}'"), "Dedicate")
                    ,WipInfo = new BindingList<CompWipInfo>()
                });
                _tempList[_tempList.Count - 1].WipInfo.Add
                    (
                        new CompWipInfo(!string.IsNullOrEmpty(_tempList[_tempList.Count - 1].BackflushLoc), _tempList[_tempList.Count - 1].CompNumber, _tempList[_tempList.Count - 1].CompUom)
                    );
                _tempList[_tempList.Count - 1].WipInfo.ListChanged += WipInfo_ListChanged;
                _tempList[_tempList.Count - 1].NonLotList = _tempList[_tempList.Count - 1].LotList.Count == 0 && !string.IsNullOrEmpty(_row.SafeGetField<string>("ID"))
                    ? Lot.DataRowToLotList(dataSet.Tables["OH"].Select($"[ID] = '{_row.Field<string>("Component")}'"), "NonLot")
                    : new List<Lot>();
            }
            dataSet.Dispose();
            return _tempList;
        }

        /// <summary>
        /// Retrieve a list of components for a Sku
        /// </summary>
        /// <param name="skuNbr">Part Number</param>
        /// <param name="operation">Operation Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of Component objects related to a Bill of material</returns>
        public static List<Component> GetComponentBomList(string skuNbr, string operation, SqlConnection sqlCon)
        {
            var _tempList = new List<Component>();
            WipInfoUpdating = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _count = 0;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT COUNT(a.[ID]) FROM [dbo].[PS-INIT] a WHERE a.[ID] LIKE CONCAT(@p1, '*%') AND (a.[Routing_Seq] = @p2 OR a.[Routing_Seq] IS NULL)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", skuNbr);
                        cmd.Parameters.AddWithValue("p2", operation);
                        int.TryParse(cmd.ExecuteScalar().ToString(), out _count);
                    }
                    operation = _count == 0 ? "10" : operation;
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component'
	                                                                ,a.[Qty_Per_Assy]
	                                                                ,b.[Description]
                                                                    ,b.[Drawing_Nbrs]
	                                                                ,b.[Um]
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                WHERE
	                                                                a.[ID] LIKE CONCAT(@p1, '*%') AND (a.[Routing_Seq] = @p2 OR a.[Routing_Seq] IS NULL)
                                                                ORDER BY
                                                                    Lot_Trace, Component;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", skuNbr);
                        cmd.Parameters.AddWithValue("p2", operation);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new Component
                                    {
                                        CompNumber = reader.SafeGetString("Component"),
                                        AssemblyQty = reader.SafeGetDouble("Qty_Per_Assy"),
                                        CompDescription = reader.SafeGetString("Description"),
                                        CompMasterPrint = reader.SafeGetString("Drawing_Nbrs"),
                                        CompUom = reader.SafeGetString("Um")
                                    });
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
        /// Retrieve a list of components for a Sku
        /// </summary>
        /// <param name="dataRows">Array of DataRow objects to translate to Component object properties</param>
        /// <returns>List of Component objects related to a Bill of material</returns>
        public static List<Component> GetComponentBomList(DataRow[] dataRows)
        {
            var _tempList = new List<Component>();
            foreach (DataRow _row in dataRows)
            {
                _tempList.Add(new Component
                {
                    CompNumber = _row.Field<string>(2)
                    ,AssemblyQty = _row.Field<double>(3)
                    ,CompDescription = _row.Field<string>(4)
                    ,CompMasterPrint = _row.Field<string>(5)
                    ,CompUom = _row.Field<string>(6)
                });
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
            var _tempItem = e.NewIndex != -1 && !WipInfoUpdating ? ((BindingList<CompWipInfo>)sender)[e.NewIndex] : null;

            #region Lot Number Changed

            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "LotNbr")
            {
                FromOtherChange = true;
                if(Lot.LotValidation(_tempItem.LotNbr, _tempItem.PartNbr, ModelBase.ModelSqlCon))
                {
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].IsValidLot = true;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].RcptLoc = _tempItem.IsBackFlush
                        ? Sku.GetBackFlushLoc(_tempItem.PartNbr, ModelBase.ModelSqlCon)
                        : Lot.GetLotLocation(_tempItem.LotNbr, ModelBase.ModelSqlCon);
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].LotQty = 0;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandQty = Lot.GetLotOnHandQuantity(_tempItem.LotNbr, ModelBase.ModelSqlCon);
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandCalc = 0;
                    if (((BindingList<CompWipInfo>)sender).Count() == ((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot))
                    {
                        ((BindingList<CompWipInfo>)sender).Add(new CompWipInfo(_tempItem.IsBackFlush, _tempItem.PartNbr, _tempItem.Uom) { BaseQty = _tempItem.BaseQty });
                        if (((BindingList<CompWipInfo>)sender).Count(o => o.IsValidLot) > 1)
                        {
                            ((BindingList<CompWipInfo>)sender)[0].ScrapList.ResetBindings();
                        }
                    }
                }
                else if (_tempItem.IsValidLot)
                {
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].IsValidLot = false;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].RcptLoc = string.Empty;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].LotQty = null;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandQty = 0;
                    ((BindingList<CompWipInfo>)sender)[e.NewIndex].OnHandCalc = 0;
                }
                FromOtherChange = false;
            }

            #endregion

            #region Lot Quantity Changed

            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "LotQty" && !WipInfoUpdating)
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
                    if (_calcQtyPer > 0)
                    {
                        var _counter = 1;
                        foreach (var item in ((BindingList<CompWipInfo>)sender).Where(o => !o.IsQtyLocked && o.IsValidLot))
                        {
                            var index = ((BindingList<CompWipInfo>)sender).IndexOf(item);
                            ((BindingList<CompWipInfo>)sender)[index].LotQty = _calcQtyPer;
                            if (_counter == ((BindingList<CompWipInfo>)sender).Count(o => !o.IsQtyLocked && o.IsValidLot))
                            {
                                ((BindingList<CompWipInfo>)sender)[index].LotQty += ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty) < ((BindingList<CompWipInfo>)sender)[index].BaseQty
                                    ? ((BindingList<CompWipInfo>)sender)[index].BaseQty - ((BindingList<CompWipInfo>)sender).Sum(o => o.LotQty)
                                    : 0;
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

            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "OnHandCalc" && !WipInfoUpdating)
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

        /// <summary>
        /// Get a table of all BOM's for every SKU on file
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of bill of materials</returns>
        public static DataTable GetComponentBomTable(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                REPLACE(a.[ID], '*', '-') as 'ID'
	                                                                                ,a.[Routing_Seq]
	                                                                                ,SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component'
	                                                                                ,CAST(a.[Qty_Per_Assy] as float) as 'Qty_Per_Assy'
	                                                                                ,b.[Description]
                                                                                    ,b.[Drawing_Nbrs]
	                                                                                ,b.[Um]
                                                                                FROM
	                                                                                [dbo].[PS-INIT] a
                                                                                RIGHT JOIN
	                                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]));", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
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
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of pick lists</returns>
        public static DataTable GetComponentPickTable(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                a.[ID]
	                                                                                ,SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component'
	                                                                                ,CAST(a.[Qty_Per_Assy] as float) as 'Qty Per'
	                                                                                ,CAST(a.[Qty_Reqd] as int) as 'Req Qty'
	                                                                                ,CAST(b.[Qty_On_Hand] as int) as 'On Hand'
	                                                                                ,ISNULL(CAST((SELECT SUM(aa.[OH_Qty_By_Loc]) FROM [dbo].[IPL-INIT_Location_Data] aa WHERE aa.[ID1] = b.[Part_Nbr] AND aa.[Loc_Pick_Avail_Flag] = 'Y') as int), 0) as 'Pickable'
	                                                                                ,b.[Wip_Rec_Loc] as 'Backflush'
	                                                                                ,c.[Description]
	                                                                                ,c.[Drawing_Nbrs]
	                                                                                ,c.[Um]
	                                                                                ,c.[Inventory_Type]
	                                                                                ,c.[Lot_Trace]
	                                                                                ,ISNULL(a.[Routing_Seq], 10) as 'Routing'
                                                                                FROM
	                                                                                [dbo].[PL-INIT] a
                                                                                RIGHT JOIN
	                                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                                RIGHT JOIN
	                                                                                [dbo].[IM-INIT] c ON c.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                                WHERE
	                                                                                a.[Qty_Reqd] > 0", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
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
            if (comp.WipInfo.Count(o => o.IsValidLot) > 0)
            {
                comp.WipInfo[0].LotNbr = comp.WipInfo[0].LotNbr;
            }
        }
    }
}
