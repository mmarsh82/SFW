using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public class Sku : ModelBase
    {
        #region Properties

        public string SkuNumber { get; set; }
        public string SkuDescription { get; set; }
        public string Uom { get; set; }
        public string CustomerRev { get; set; }
        public string InternalRev { get; set; }
        public int TotalOnHand { get; set; }
        public string MasterPrint { get; set; }
        public List<Component> Bom { get; set; }
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
        public List<Tool> ToolList { get; set; }
        public int Facility { get; set; }
        public bool IsTransfer { get; set; }

        #endregion

        /// <summary>
        /// Sku Default Constructor
        /// </summary>
        public Sku()
        { }

        #region Data Access

        /// <summary>
        /// Load a datatable with all the sku information
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of Sku information</returns>
        public static DataTable GetSkuTable (SqlConnection sqlCon)
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

        /// <summary>
        /// Get a Table of all work instructions in the database
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of URL strings to open the work instructions</returns>
        public static DataTable GetInstructions(SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_WorkInstructions]", sqlCon))
                        {
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

        /// <summary>
        /// Load a datatable with all the location information
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of location information</returns>
        public static DataTable GetLocationTable(SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Locations]", sqlCon))
                        {
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

        /// <summary>
        /// Load a datatable with all the structure information
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of structure information</returns>
        public static DataTable GetStructureTable(SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_ProductStructure]", sqlCon))
                        {
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
        /// Sku Constructor
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
                        var _row = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                        if (partLoad)
                        {
                            if (_row.Length > 0)
                            {
                                SkuNumber = searchValue;
                                SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                                Uom = _row.FirstOrDefault().Field<string>("Uom");
                                Facility = site;
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
                        }
                        break;
                    //Lot based Sku Loading
                    case 'L':
                        _row = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{searchValue}'");
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
                        }
                        break;
                    //Custom Sku Loading
                    case 'C':
                        _row = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                        if (_row.Length > 0)
                        {
                            SkuNumber = searchValue;
                            SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                            Uom = _row.FirstOrDefault().Field<string>("Uom");
                            TotalOnHand = _row.FirstOrDefault().Field<int>("OnHand");
                            EngStatus = _row.FirstOrDefault().Field<string>("Status");
                            Facility = _row.FirstOrDefault().Field<int>("Site");
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
                    if (Exists(_parPart[0], false))
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

        /// <summary>
        /// Get the Sku's crew size
        /// </summary>
        /// <param name="partNbr">Part number to search</param>
        /// <returns>crew size as int</returns>
        public static int GetCrewSize(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<int>("Crew");
        }

        /// <summary>
        /// Get a part number backflush flag
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <returns>backflush or default location as string</returns>
        public static string IsBackFlushLoc(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("WipLocation");
        }

        /// <summary>
        /// Get the default or backflush location for any part number
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <returns>backflush or default location as string</returns>
        public static string GetBackFlushLoc(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("WipLocation");
        }

        /// <summary>
        /// Check to see if a Sku number is lot tracable
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <returns>lot tracability as bool</returns>
        public static bool IsLotTracable(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("LotTraceable") == "T";
        }

        /// <summary>
        /// Validates that the location entered is a valid M2k location
        /// </summary>
        /// <param name="location">location to validate</param>
        /// <param name="facCode">Facility code</param>
        /// <returns>valid location as bool</returns>
        public static bool IsValidLocation(string location, int facCode)
        {
            return MasterDataSet.Tables["LOC"].Select($"[Location] = '{location}' AND [Site] = {facCode}").Length > 0;
        }

        /// <summary>
        /// Validates Sku is in a location
        /// </summary>
        /// <param name="partNbr">Sku ID</param>
        /// <param name="location">Location ID</param>
        /// <returns>valid Sku as bool</returns>
        public static bool IsSkuInLocation(string partNbr, string location)
        {
            return MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{partNbr}' AND [Location] = '{location}'").Length > 0;
        }

        /// <summary>
        /// Validates Sku has a minimum quantity in a location
        /// </summary>
        /// <param name="partNbr">Sku ID</param>
        /// <param name="location">Location ID</param>
        /// <param name="qty">Minimum quantity to validate</param>
        /// <returns>valid Sku as bool</returns>
        public static bool IsValidSkuQuantity(string partNbr, string location, int qty)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{partNbr}' AND [Location] = '{location}'");
            return _rows.Length > 0 && _rows.Sum(o => o.Field<int>("OnHand")) > qty;
        }

        /// <summary>
        /// Get a Sku's current on hand value for a specific lot number
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <returns>on hand value as int</returns>
        public static int GetOnhandQuantity(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<int>("OnHand");
        }

        /// <summary>
        /// Get a Sku's item type
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="site">Facility code</param>
        /// <returns>Sku item class as string</returns>
        public static string GetType(string partNbr, int site)
        {
            var _class = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}");
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
            var _class = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}");
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
                ? MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}'").Length > 0
                : MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").Length > 0;
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="returnAll">Return all or just active status</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static bool Exists(string partNbr, bool returnAll, int site)
        {
            return returnAll 
                ? MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Site] = {site}").Length > 0
                : MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A' AND [Site] = {site}").Length > 0;
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static string GetMasterNumber(string partNbr, bool returnAll)
        {
            return returnAll
                ? MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}'").FirstOrDefault().Field<string>("MasterSkuID")
                : MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("MasterSkuID");
        }

        /// <summary>
        /// Get a Sku's work instructions
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="siteNbr">Facility number to run on</param>
        /// <param name="filepath">Path to the file</param>
        /// <returns>A list of URL strings to open the work instructions</returns>
        public static List<string> GetInstructions(string partNbr, int siteNbr, string filepath)
        {
            //TODO: this code will need to be reformated once CSI has moved to the same process model as WCCO
            var _inst = new List<string>();
            if (siteNbr == 2)
            {
                if (File.Exists($"{filepath}{partNbr}.pdf"))
                {
                    _inst.Add(partNbr);
                }
            }
            else
            {
                var _rows = MasterDataSet.Tables["WI"].Select($"[SkuID] = '{partNbr}'");
                foreach (var _row in _rows)
                {
                    var dir = new DirectoryInfo(filepath);
                    var fileList = dir.GetFiles($"*{_row.Field<int>("WI")}*");
                    foreach (var file in fileList)
                    {
                        _inst.Add(file.Name);
                    }
                }
            }
            return _inst;
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
                    var _rows = MasterDataSet.Tables["SKU"].AsEnumerable()
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
                    _rows = MasterDataSet.Tables["SKU"].AsEnumerable()
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
                    var _rows = MasterDataSet.Tables["SKU"].Select($"[SkuID] LIKE '%{searchInput}%' OR [Description] LIKE '%{searchInput}%'");
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
            catch (Exception e)
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
