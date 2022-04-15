using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public class Sku : ModelBase
    {
        #region Properties

        public string SkuNumber { get; set; }
        public string SkuDescription { get; set; }
        public string Uom { get; set; }
        public string BomRevLevel { get; set; }
        public DateTime BomRevDate { get; set; }
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
        public List<string> ToolList { get; set; }

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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                im.[Part_Number] as 'SkuID'
	                                                                                ,im.[Description]
	                                                                                ,im.[Um] as 'Uom'
	                                                                                ,im.[Lot_Trace] as 'LotTraceable'
	                                                                                ,im.[Drawing_Nbrs] as 'MasterSkuID'
	                                                                                ,rt.[Wc_Nbr] as 'WorkCenterID'
	                                                                                ,CAST(im.[Bom_Rev_Date] as date) as 'BomRevDate'
	                                                                                ,im.[Accounting_Status] as 'Status'
	                                                                                ,im.[Inventory_Type] as 'Type'
	                                                                                ,CAST(ISNULL(rt.[Crew_Size], 1) as int) as 'Crew'
	                                                                                ,CAST(ipl.[Qty_On_Hand] as int) as 'OnHand'
	                                                                                ,ipl.[Wip_Rec_Loc] as 'WipLocation'
                                                                                FROM
	                                                                                [dbo].[IM-INIT] im
                                                                                LEFT JOIN
	                                                                                [dbo].[RT-INIT] rt ON rt.[ID] = CONCAT(im.[Part_Number], '*10')
                                                                                LEFT JOIN
	                                                                                [dbo].[IPL-INIT] ipl on ipl.[Part_Nbr] = im.[Part_Number]
                                                                                WHERE
	                                                                                im.[Accounting_Status] IS NOT NULL", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT [Part_Number] as 'SkuID', [Work_Instructions] as 'WI' FROM [dbo].[IM-INIT_Work_Instructions]", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
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
        }

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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                [ID1] as 'ID'
	                                                                                ,[Tool_Tape] as 'Tool'
                                                                                FROM
	                                                                                [dbo].[RT-INIT_Tool_Tape];", sqlCon))
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT locM.[Location] ,locM.[D_esc] as 'Description' FROM [dbo].[LOC_MASTER-INIT] locM", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
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
        }

        /// <summary>
        /// Get a Sku's structure
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="siteCrawl">OPTIONAL: tells the query to crawl all site databases for product structure</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, int> GetStructure(string partNbr, SqlConnection sqlCon, bool siteCrawl = false)
        {
            var _returnList = new Dictionary<Sku, int>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _counter = 0;
                    var _parentCount = 1;
                    var _found = false;
                    var _partList = new List<string>
                    {
                        partNbr
                    };

                    #region Parent Part Retrieval

                    while (!_found)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Part'
                                                                    ,SUBSTRING(a.[ID],0, CHARINDEX('*', a.[ID], 0)) as 'Parent'
	                                                                ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) as 'Desc'
	                                                                ,a.[Bom_Uom] as 'Uom'
	                                                                ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                ,b.[Accounting_Status] as 'Status'
                                                                    ,b.[Drawing_Nbrs] as 'Master'
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))
                                                                WHERE
	                                                                a.[Qty_Per_Assy] IS NOT NULL", sqlCon))
                        {
                            foreach (var s in _partList.Distinct())
                            {
                                if (_partList.IndexOf(s) == 0)
                                {
                                    cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                    cmd.Parameters.AddWithValue("p1", s);
                                }
                                else
                                {
                                    cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                    cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                }
                            }
                            cmd.CommandText += ")";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    _partList = new List<string>();
                                    _counter--;
                                    while (reader.Read())
                                    {
                                        var _tempSku = new Sku
                                        {
                                            SkuNumber = reader.SafeGetString("Part")
                                            ,
                                            EngStatusDesc = reader.SafeGetString("Parent")
                                            ,
                                            DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Parent")) > 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Parent")).DiamondNumber
                                                : _parentCount.ToString()
                                            ,
                                            SkuDescription = reader.SafeGetString("Desc")
                                            ,
                                            Uom = reader.SafeGetString("Uom")
                                            ,
                                            Operation = reader.SafeGetDouble("Qpa").ToString()
                                            ,
                                            EngStatus = reader.SafeGetString("Status")
                                            ,
                                            MasterPrint = reader.SafeGetString("Master")
                                        };
                                        _returnList.Add(_tempSku, _counter);
                                        _partList.Add(reader.SafeGetString("Part"));
                                        _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                    }
                                }
                                else
                                {
                                    _found = true;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Child Part Retrieval

                    _counter = 0;
                    var _childCount = 1;
                    _found = false;
                    _partList = new List<string>
                    {
                        partNbr
                    };
                    while (!_found)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID])) as 'Part'
                                                                    ,SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Child'
                                                                    ,(SELECT COUNT(aa.[ID]) FROM [dbo].[PS-INIT] aa WHERE SUBSTRING(aa.[ID], CHARINDEX('*', aa.[ID], 0) + 1, LEN(aa.[ID])) = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID]))) as 'ChildCount'
	                                                                ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID]))) as 'Desc'
	                                                                ,a.[Bom_Uom] as 'Uom'
	                                                                ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                ,b.[Accounting_Status] as 'Status'
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                WHERE
	                                                                a.[Qty_Per_Assy] IS NOT NULL", sqlCon))
                        {
                            foreach (var s in _partList.Distinct())
                            {
                                if (_partList.IndexOf(s) == 0)
                                {
                                    cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                    cmd.Parameters.AddWithValue("p1", s);
                                }
                                else
                                {
                                    cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                    cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                }
                            }
                            cmd.CommandText += ")";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    _partList = new List<string>();
                                    _counter++;
                                    while (reader.Read())
                                    {
                                        var _tempSku = new Sku
                                        {
                                            SkuNumber = reader.SafeGetString("Part")
                                            ,
                                            EngStatusDesc = reader.SafeGetString("Child")
                                            ,
                                            DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Child")) > 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Child")).DiamondNumber
                                                : _parentCount.ToString()
                                            ,
                                            Location = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Child")) > 0 && reader.SafeGetInt32("ChildCount") == 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Child")).Location
                                                : _childCount.ToString()
                                            ,
                                            SkuDescription = reader.SafeGetString("Desc")
                                            ,
                                            Uom = reader.SafeGetString("Uom")
                                            ,
                                            Operation = reader.SafeGetDouble("Qpa").ToString()
                                            ,
                                            EngStatus = reader.SafeGetString("Status")
                                        };
                                        _returnList.Add(_tempSku, _counter);
                                        _partList.Add(reader.SafeGetString("Part"));
                                        _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                        _childCount = _returnList.Keys.Count(o => o.Location == _childCount.ToString()) > 0 ? _childCount + 1 : _childCount;
                                    }
                                }
                                else
                                {
                                    _found = true;
                                }
                            }
                        }
                    }

                    #endregion

                    //Populating the input part
                    var _parent = new Sku(partNbr, 'C');
                    _parent.EngStatusDesc = _parent.SkuNumber;
                    _parent.DiamondNumber = "0";
                    _returnList.Add(_parent, 0);

                    #region Site Crawl logic for engineer privileges

                    if (siteCrawl)
                    {
                        _partList.Clear();
                        foreach (var _part in _returnList.Where(o => o.Value <= 0))
                        {
                            _partList.Add($"{_part.Key.SkuNumber}C");
                        }
                        _found = false;
                        var _first = true;
                        sqlCon.ChangeDatabase("CSI_MAIN");
                        while (!_found)
                        {
                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                        SELECT
	                                                                        SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Part'
                                                                            ,SUBSTRING(a.[ID],0, CHARINDEX('*', a.[ID], 0)) as 'Parent'
	                                                                        ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) as 'Desc'
	                                                                        ,a.[Bom_Uom] as 'Uom'
	                                                                        ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                        ,b.[Accounting_Status] as 'Status'
                                                                            ,b.[Drawing_Nbrs] as 'Master'
                                                                        FROM
	                                                                        [dbo].[PS-INIT] a
                                                                        RIGHT JOIN
	                                                                        [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))
                                                                        WHERE
	                                                                        a.[Qty_Per_Assy] IS NOT NULL
	                                                                        AND (SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) NOT LIKE '%POLY%'", sqlCon))
                            {
                                foreach (var s in _partList.Distinct())
                                {
                                    if (_partList.IndexOf(s) == 0)
                                    {
                                        cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                        cmd.Parameters.AddWithValue("p1", s);
                                    }
                                    else
                                    {
                                        cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                        cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                    }
                                }
                                cmd.CommandText += ")";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        _partList = new List<string>();
                                        while (reader.Read())
                                        {
                                            var _tempSku = new Sku
                                            {
                                                SkuNumber = reader.SafeGetString("Part")
                                                ,EngStatusDesc = _first
                                                    ? reader.SafeGetString("Parent").Replace("C", "")
                                                    : reader.SafeGetString("Parent")
                                                ,DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Parent") || o.SkuNumber == reader.SafeGetString("Parent").Replace("C", "")) > 0
                                                    ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Parent") || o.SkuNumber == reader.SafeGetString("Parent").Replace("C", "")).DiamondNumber
                                                    : _parentCount.ToString()
                                                ,SkuDescription = reader.SafeGetString("Desc")
                                                ,Uom = reader.SafeGetString("Uom")
                                                ,Operation = reader.SafeGetDouble("Qpa").ToString()
                                                ,EngStatus = reader.SafeGetString("Status")
                                                ,MasterPrint = reader.SafeGetString("Master")
                                            };
                                            _counter = _returnList.First(o => o.Key.SkuNumber == _tempSku.EngStatusDesc || o.Key.SkuNumber == _tempSku.EngStatusDesc.Replace("C", "")).Value - 1;
                                            _returnList.Add(_tempSku, _counter);
                                            _partList.Add(reader.SafeGetString("Part"));
                                            _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                        }
                                        _first = false;
                                    }
                                    else
                                    {
                                        _found = true;
                                    }
                                }
                            }
                        }
                        sqlCon.ChangeDatabase("WCCO_MAIN");
                    }

                    #endregion

                    return _returnList;
                }
                catch (SqlException sqlEx)
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

        #endregion

        /// <summary>
        /// Sku Constructor
        /// Load a Skew object based on a part number
        /// </summary>
        /// <param name="searchValue">Value to search when loading the Sku object</param>
        /// <param name="type">Type of object to load S = Standard Sku, L = Lot based Sku, C = Custom Sku object</param>
        /// <param name="partLoad">Optional: Tell the constructor to load a part for tracking</param>
        public Sku(string searchValue, char type, bool partLoad = false)
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
                            }
                        }
                        else
                        {
                            SkuNumber = searchValue;
                            SkuDescription = _row.FirstOrDefault().Field<string>("Description");
                            Uom = _row.FirstOrDefault().Field<string>("Uom");
                            BomRevDate = _row.FirstOrDefault().Field<DateTime>("BomRevDate");
                            TotalOnHand = _row.FirstOrDefault().Field<int>("OnHand");
                            MasterPrint = _row.FirstOrDefault().Field<string>("MasterSkuID");
                            InventoryType = _row.FirstOrDefault().Field<string>("Type");
                            CrewSize = _row.FirstOrDefault().Field<int>("Crew");
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
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<int>("Crew");
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
        /// <returns>valid location as bool</returns>
        public static bool IsValidLocation(string location)
        {
            return MasterDataSet.Tables["LOC"].Select($"[Location] = '{location}'").Length > 0;
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
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static bool Exists(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").Length > 0;
        }

        /// <summary>
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static string GetMasterNumber(string partNbr)
        {
            return MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{partNbr}' AND [Status] = 'A'").FirstOrDefault().Field<string>("MasterSkuID");
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
            if (siteNbr == 0)
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
        /// Get a Sku's tool list
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <returns>A list of tool's associated with the Sku</returns>
        public static List<string> GetTools(string partNbr, string woSeq)
        {
            return MasterDataSet.Tables["TL"].Select($"[ID] = '{partNbr}*{woSeq}'").Select(o => o[1].ToString()).ToList();
        }

        /// <summary>
        /// Search for a part number of a description of a part in the database
        /// </summary>
        /// <param name="searchInput">Search input to find</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, bool> Search(string searchInput)
        {
            var _returnList = new Dictionary<Sku, bool>();
            var _rows = MasterDataSet.Tables["SKU"].Select($"[SkuID] LIKE '%{searchInput}%' OR [Description] LIKE '%{searchInput}%'");
            foreach (var _row in _rows)
            {
                _returnList.Add(new Sku
                {
                    SkuNumber = _row.Field<string>("SkuID")
                    ,SkuDescription = _row.Field<string>("Description")
                    ,MasterPrint = _row.Field<string>("MasterSkuID")
                    ,EngStatus = _row.Field<string>("Status")
                }, _row.Field<string>("Status") == "A");
            }
            return _returnList;
        }
    }
}
