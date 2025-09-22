using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace SFW.Model.Product
{
    public class Sku : ModelBase, IModuleData
    {
        #region Properties

        public string SkuNumber { get; set; }
        public string SkuDescription { get; set; }
        public string Uom { get; set; }
        public string CustomerRev { get; set; }
        public string InternalRev { get; set; }
        public int TotalOnHand { get; set; }
        public string MasterPrint { get; set; }
        public List<Production.BillComponent> Bom { get; set; }
        public string EngStatus { get; set; }
        public string EngStatusDesc { get; set; }
        public string InventoryType { get; set; }
        public int CrewSize { get; set; }
        public List<string> InstructionList { get; set; }
        public string DiamondNumber { get; set; }
        public string Location { get; set; }
        public string WorkOrder { get; set; }
        public string Operation { get; set; }
        public string Machine { get; set; }
        public string MachineGroup { get; set; }
        public bool Inspection { get; set; }
        public string NonCon { get; set; }
        public List<Production.Tool> ToolList { get; set; }
        public int Facility { get; set; }
        public bool IsTransfer { get; set; }
        public bool IsLotTrace { get; set; }
        public double Value { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Load a datatable with all the sku information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of Sku information</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Products]", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
                    }
                    catch (SqlException)
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

        /// <summary>
        /// Get the product cost value based on an ID
        /// </summary>
        /// <param name="skuId">Product ID</param>
        /// <returns>Product value as double</returns>
        public static double GetValue(string skuId)
        {
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT SUM(imav.[Inc_Av_Costs]) + SUM(imav.[Ru_Av_Costs]) FROM [dbo].[IM-INIT_Av_Costs] imav WHERE [imav].[ID1] = @p1", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", skuId);
                        return double.TryParse(cmd.ExecuteScalar().ToString(), out double d) ? d : 0.00;
                    }
                }
                catch (SqlException)
                {
                    return 0.00;
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
        /// Get the total onhand by location based on a part number
        /// </summary>
        /// <param name="skuId">Product ID</param>
        /// <returns>Onhand quantity as int</returns>
        public static int GetLocationQuantity(string skuId, string location)
        {
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT SUM([Oh_Qty_By_Loc]) as 'OnHand' FROM [dbo].[IPL-INIT_Location_Data] WHERE [ID1] = @p1 AND [Location_ID] = @p2", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", skuId);
                        cmd.Parameters.AddWithValue("p2", location);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                    }
                }
                catch (SqlException)
                {
                    return 0;
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
        /// Get observable collection of products
        /// </summary>
        /// <param name="workOrder">Work Order filter</param>
        /// <returns>ObservableCollection of crewmember objects</returns>
        public static ObservableCollection<Sku> GetCollection(string workOrder, int op)
        {
            var _skuCol = new ObservableCollection<Sku>();
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT wp.[Part_Wo_Desc] FROM [dbo].[WP-INIT] wp WHERE wp.[Wp_Nbr] = @p1", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workOrder);
                        _skuCol.Add(new Sku(cmd.ExecuteScalar()?.ToString()));
                    }
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT CONCAT([ChildSkuID], '|0', [Site]) 'ProductId' FROM [dbo].[SFW_Picklist] WHERE [WorkOrderID] = @p1 AND [Routing] = @p2", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workOrder);
                        cmd.Parameters.AddWithValue("p2", op);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _skuCol.Add(new Sku(reader.SafeGetString("ProductId")));
                                }
                            }
                        }
                    }
                    return _skuCol;
                }
                catch (SqlException)
                {
                    return null;
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
        /// Load a datatable with all the activity information
        /// </summary>
        /// <param name="skuId">Sku ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of activity information</returns>
        public static DataTable GetActivityTable(string skuId, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Activity] WHERE [ProductID] = @p1 ORDER BY [DueDate]", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", skuId.Contains("|") ? skuId : $"{skuId}|01");
                            adapter.Fill(_dt);
                        }
                        var _bal = 0;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT SUM([Oh_Qty_By_Loc]) FROM [dbo].[IPL-INIT_Location_Data] WHERE [ID1] = @p1 AND [Loc_Pick_Avail_Flag] = 'Y'", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", skuId.Contains("|") ? skuId : $"{skuId}|01");
                            _bal = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                        }
                        _dt.Columns.Add("Balance", typeof(int));
                        foreach (DataRow _row in _dt.Rows)
                        {
                            _row.BeginEdit();
                            _bal += _row.Field<int>("Quantity");
                            _row["Balance"] = _bal;
                            _row.EndEdit();
                        }

                        return _dt;
                    }
                    catch (SqlException)
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
        public Sku()
        { }

        /// <summary>
        /// Sku Overridden Constructor
        /// </summary>
        public Sku(string skuId)
        {
            if (!string.IsNullOrEmpty(skuId))
            {
                var _sku = skuId;
                var _site = ModelFacility;
                if (skuId != null && skuId.Contains("|"))
                {
                    var splitId = skuId.Split('|');
                    _sku = splitId[0];
                    _site = int.TryParse(splitId[1], out int i) ? i : 1;
                }
                var skuRow = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{_sku}' AND [Site] = {_site}").FirstOrDefault();
                SkuNumber = skuId;
                SkuDescription = skuRow.Field<string>("Description");
                Uom = skuRow.Field<string>("Uom");
                Facility = _site;
                IsLotTrace = IsLotTracable(skuId, _site);
                Value = GetValue(skuId);
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// Load a Skew object based on a part number
        /// </summary>
        /// <param name="searchValue">Value to search when loading the Sku object</param>
        /// <param name="type">Type of object to load S = Standard Sku, L = Lot based Sku, C = Custom Sku object</param>
        /// <param name="site">Facility of the product</param>
        /// <param name="partLoad">Optional: Tell the constructor to load a part for tracking</param>
        public Sku(string searchValue, char type, int site, bool partLoad = false)
        {
            try
            {
                switch (type)
                {
                    //Standard Sku Load
                    case 'S':
                        var _row = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                        if (partLoad)
                        {
                            if (_row.Length > 0)
                            {
                                SkuNumber = searchValue;
                                SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                                Uom = _row.FirstOrDefault().Field<string>("Uom");
                                Facility = site;
                                IsLotTrace = IsLotTracable(searchValue, site);
                            }
                        }
                        else
                        {
                            SkuNumber = searchValue;
                            SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                            Uom = _row.FirstOrDefault().Field<string>("Uom");
                            InternalRev = DateTime.TryParse(_row.FirstOrDefault().Field<string>("InternalRev"), out DateTime dt) ? dt.ToString("yyMMdd-1") : string.Empty;
                            InternalRev = _row.FirstOrDefault().Field<DateTime>("InternalRev") != Convert.ToDateTime("1999-01-01") ? _row.FirstOrDefault().Field<DateTime>("InternalRev").ToString("yyMMdd-1") : string.Empty;
                            TotalOnHand = _row.FirstOrDefault().Field<int>("OnHand");
                            MasterPrint = _row.FirstOrDefault().Field<string>("MasterSkuID");
                            InventoryType = _row.FirstOrDefault().Field<string>("Type");
                            CrewSize = _row.FirstOrDefault().Field<int>("Crew");
                            Facility = _row.FirstOrDefault().Field<int>("Site");
                            IsLotTrace = IsLotTracable(searchValue, site);
                        }
                        break;
                    //Lot based Sku Loading
                    case 'L':
                        _row = MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{searchValue}'");
                        if (_row.Length > 0)
                        {
                            SkuNumber = _row.FirstOrDefault().Field<string>("SkuID");
                            SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                            Uom = _row.FirstOrDefault().Field<string>("Uom");
                            if (!string.IsNullOrEmpty(_row.FirstOrDefault().Field<string>("Notes")))
                            {
                                NonCon = _row.FirstOrDefault().Field<string>("Notes").Replace("/", "");
                            }
                            TotalOnHand = _row.FirstOrDefault().Field<int>("OnHand");
                            Location = _row.FirstOrDefault().Field<string>("Location");
                            Facility = _row.FirstOrDefault().Field<int>("Site");
                            IsLotTrace = IsLotTracable(searchValue, site);
                        }
                        break;
                    //Custom Sku Loading
                    case 'C':
                        _row = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                        if (_row.Length > 0)
                        {
                            SkuNumber = searchValue;
                            SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                            Uom = _row.FirstOrDefault().Field<string>("Uom");
                            TotalOnHand = _row.FirstOrDefault().Field<int>("OnHand");
                            EngStatus = _row.FirstOrDefault().Field<string>("Status");
                            Facility = _row.FirstOrDefault().Field<int>("Site");
                            IsLotTrace = IsLotTracable(searchValue, site);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get the Sku's crew size
        /// </summary>
        /// <param name="partNbr">Part number to search</param>
        /// <returns>crew size as int</returns>
        public static int GetCrewSize(string partNbr)
        {
            return MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<int>("Crew");
        }

        /// <summary>
        /// Get a part number backflush flag
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <returns>backflush or default location as string</returns>
        public static string IsBackFlushLoc(string partNbr)
        {
            return MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("WipLocation");
        }

        /// <summary>
        /// Get the default or backflush location for any part number
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <returns>backflush or default location as string</returns>
        public static string GetBackFlushLoc(string partNbr)
        {
            return MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("WipLocation");
        }

        /// <summary>
        /// Check to see if a Sku number is lot tracable
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <param name="site">Site to search against</param>
        /// <returns>lot tracability as bool</returns>
        public static bool IsLotTracable(string partNbr, int site)
        {
            if (partNbr.Contains('|'))
            {
                partNbr = partNbr.Split('|')[0];
            }
            var _rtnVal = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A' AND [Site] = {site}");
            return _rtnVal != null && _rtnVal.Count() >= 1 ? _rtnVal.FirstOrDefault().Field<string>("LotTraceable") == "T" : true;
        }

        /// <summary>
        /// Get a Sku's current on hand value for a specific lot number
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <returns>on hand value as int</returns>
        public static int GetOnhandQuantity(string partNbr)
        {
            return MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<int>("OnHand");
        }

        /// <summary>
        /// Get a Sku's item type
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="site">Facility code</param>
        /// <returns>Sku item class as string</returns>
        public static string GetType(string partNbr, int site)
        {
            var _class = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}");
            if (_class == null || _class.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return _class.FirstOrDefault().Field<string>("Type");
            }
        }

        /// <summary>
        /// Get a Sku's item class
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="site">Facility code</param>
        /// <returns>Sku item class as string</returns>
        public static string GetClass(string partNbr, int site)
        {
            var _class = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}");
            if (_class == null || _class.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return _class.FirstOrDefault().Field<string>("Class");
            }
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="returnAll">Return all or just active status</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static bool Exists(string partNbr, bool returnAll)
        {
            return returnAll
                ? MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}'").Length > 0
                : MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").Length > 0;
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="returnAll">Return all or just active status</param>
        /// <param name="site">Application site number</param>
        /// <param name="masterCheck">Optional: Check to see if the part number is a master print number</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static bool Exists(string partNbr, bool returnAll, int site, bool masterCheck = false)
        {
            if (!masterCheck)
            {
                return returnAll
                ? MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}").Length > 0
                : MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A' AND [Site] = {site}").Length > 0;
            }
            else
            {
                return returnAll
                ? MasterDataSet.Tables[typeof(Sku).Name].Select($"[MasterSkuID] = '{partNbr}' AND [Site] = {site}").Length > 0
                : MasterDataSet.Tables[typeof(Sku).Name].Select($"[MasterSkuID] = '{partNbr}' AND [Status] = 'A' AND [Site] = {site}").Length > 0;
            }
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="returnAll">Return all results</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static string GetMasterNumber(string partNbr, bool returnAll, int site)
        {
            return returnAll
                ? MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}").FirstOrDefault().Field<string>("MasterSkuID")
                : MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A' AND [Site] = {site}").FirstOrDefault().Field<string>("MasterSkuID");
        }

        /// <summary>
        /// Search for a part number of a description of a part in the database
        /// </summary>
        /// <param name="searchInput">Search input to find</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, bool> Search(string searchInput)
        {
            try
            {
                var _returnList = new Dictionary<Sku, bool>();
                if (searchInput.Contains("%"))
                {
                    searchInput = searchInput.Replace("%", ".*");
                    var _rows = MasterDataSet.Tables[typeof(Sku).Name].AsEnumerable()
                    .Where(row =>
                    {
                        string skuVal = row.Field<string>("SkuID"); return Regex.IsMatch(skuVal, $".*{searchInput}.*");
                    });
                    if (_rows.Count() > 0)
                    {
                        foreach (var _row in _rows)
                        {
                            _returnList.Add(new Sku
                            {
                                SkuNumber = _row.Field<string>("SkuID")
                                ,
                                SkuDescription = _row.Field<string>("Description")
                                ,
                                MasterPrint = _row.Field<string>("MasterSkuID")
                                ,
                                EngStatus = _row.Field<string>("Status")
                            }, _row.Field<string>("Status") == "A");
                        }
                    }
                    _rows = MasterDataSet.Tables[typeof(Sku).Name].AsEnumerable()
                    .Where(row =>
                    {
                        string desVal = row.Field<string>("Description"); return Regex.IsMatch(desVal, $".*{searchInput}.*");
                    });
                    if (_rows.Count() > 0)
                    {
                        foreach (var _row in _rows)
                        {
                            _returnList.Add(new Sku
                            {
                                SkuNumber = _row.Field<string>("SkuID")
                                ,
                                SkuDescription = _row.Field<string>("Description")
                                ,
                                MasterPrint = _row.Field<string>("MasterSkuID")
                                ,
                                EngStatus = _row.Field<string>("Status")
                            }, _row.Field<string>("Status") == "A");
                        }
                    }
                }
                else
                {
                    var _rows = MasterDataSet.Tables[typeof(Sku).Name].Select($"[SkuID] LIKE '%{searchInput}%' OR [Description] LIKE '%{searchInput}%'");
                    foreach (var _row in _rows)
                    {
                        _returnList.Add(new Sku
                        {
                            SkuNumber = _row.Field<string>("SkuID")
                            ,
                            SkuDescription = _row.Field<string>("Description")
                            ,
                            MasterPrint = _row.Field<string>("MasterSkuID")
                            ,
                            EngStatus = _row.Field<string>("Status")
                        }, _row.Field<string>("Status") == "A");
                    }
                }
                return _returnList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Check to see if a Sku is a transfer part
        /// </summary>
        /// <returns></returns>
        public static bool GetIsTransfer(string partNbr)
        {
            if (Exists(partNbr, false, 1))
            {
                var _class = GetClass(partNbr, 1);
                return !string.IsNullOrEmpty(_class) || _class == "T";
            }
            return false;
        }
    }
}
