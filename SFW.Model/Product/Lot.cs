using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Product
{
    public class Lot : ModelBase, IModuleData
    {
        #region Properties

        private string lot;
        public string LotNumber
        {
            get
            { return lot; }
            set
            {
                lot = value;
                OnPropertyChanged(nameof(LotNumber));
            }
        }

        private int _onHand;
        public int Onhand
        { 
            get
            { return _onHand; }
            set
            {
                _onHand = value;
                OnPropertyChanged(nameof(Onhand));
            }
        }
        public string Uom { get; set; }
        private string _loc;
        public string Location
        {
            get
            { return _loc; }
            set
            { _loc = string.IsNullOrEmpty(value) ? value : value.ToUpper(); OnPropertyChanged(nameof(Location)); }
        }
        public int TransactionKey { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }

        private int? _tranQty;
        public string TransactionQty
        {
            get
            { return _tranQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _tranQty = i;
                }
                else
                {
                    _tranQty = null;
                }
                OnPropertyChanged(nameof(TransactionQty));
            }
        }
        public string TransactionCode { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionWorkOrder { get; set; }
        public string TransactionSalesOrder { get; set; }
        public string Submitter { get; set; }
        public string TransactionCrew { get; set; }
        public IDictionary<string, int> Dedication { get; set; }

        private bool _valid;
        public bool Validated
        {
            get
            { return _valid; }
            set
            {
                _valid = value;
                OnPropertyChanged(nameof(Validated));
            }
        }

        public bool Imported { get; set; }

        public DateTime ReceivedDate { get; set; }

        public ObservableCollection<string> LotCollection { get; set; }
        public IList<string> DefectList { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Get a DataTable of all sku objects with onhand values
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of all onhand values</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable _dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Lot]", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.Add("p1", SqlDbType.Int).Value = site;
                            adapter.Fill(_dt);
                            return _dt;
                        }
                    }
                }
                catch (Exception)
                {
                    return new DataTable();
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a DataTable of historical transactions of lots based on part number
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="site">Site Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of historical lot transactions</returns>
        public static DataTable GetHistoryTable(string partNbr, int site, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable dt = new DataTable())
                    {
                        if (!string.IsNullOrEmpty(partNbr))
                        {
                            var _where = site == 0
                                ? "WHERE [PartNbr] = @p1"
                                : "WHERE [PartNbr] = @p1 AND [SiteNumber] = @p2";
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT 
                                                                                    *
                                                                                FROM
                                                                                    [dbo].[SFW_LotHistory]
                                                                                {_where}
                                                                                ORDER BY
                                                                                    [TranDateTime] DESC", sqlCon))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("p1", partNbr);
                                if (site > 0)
                                {
                                    adapter.SelectCommand.Parameters.AddWithValue("p2", site);
                                }
                                adapter.Fill(dt);
                            }
                        }
                        return dt;
                    }
                }
                catch (Exception)
                {
                    return new DataTable();
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a DataTable of historical transactions of lots based on part number
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="lotId">Lot number</param>
        /// <param name="site">Site Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of historical lot transactions</returns>
        public static DataTable GetHistoryTable(string partNbr, string lotId, int site, SqlConnection sqlCon)
        {
            if (!string.IsNullOrEmpty(partNbr))
            {
                partNbr = partNbr.Contains("|") ? partNbr.Split('|')[0] : partNbr;
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (DataTable dt = new DataTable())
                        {
                            if (!string.IsNullOrEmpty(partNbr))
                            {
                                var _where = site == 0
                                    ? "WHERE [PartNbr] = @p1 AND [LotNumber] = @p2"
                                    : "WHERE [PartNbr] = @p1 AND [LotNumber] = @p2 AND [SiteNumber] = @p3";
                                using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT 
                                                                                    *
                                                                                FROM
                                                                                    [dbo].[SFW_LotHistory]
                                                                                {_where}
                                                                                ORDER BY
                                                                                    [TranDateTime] DESC", sqlCon))
                                {
                                    adapter.SelectCommand.Parameters.AddWithValue("p1", partNbr);
                                    adapter.SelectCommand.Parameters.AddWithValue("p2", lotId);
                                    if (site > 0)
                                    {
                                        adapter.SelectCommand.Parameters.AddWithValue("p3", site);
                                    }
                                    adapter.Fill(dt);
                                }
                            }
                            return dt;
                        }
                    }
                    catch (Exception)
                    {
                        return new DataTable();
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
            return null;
        }

        /// <summary>
        /// Get any QIR's associated with a given lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>QIR Number as int</returns>
        public static int GetAssociatedQIR(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _db = sqlCon.Database;
                try
                {
                    var _qirNbr = 0;
                    using (SqlCommand cmd = new SqlCommand(@"USE OMNI; SELECT [QIRNumber] FROM [qir_metrics_view] WHERE LotNumber=@p1 AND [Status] <> 'Closed';", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        _qirNbr = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    sqlCon.ChangeDatabase(_db);
                    return _qirNbr;
                }
                catch (Exception)
                {
                    sqlCon.ChangeDatabase(_db);
                    return 0;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Check to see if an entered QIR number is valid
        /// </summary>
        /// <param name="reference">Reference value</param>
        /// <param name="workOrder">Work Order number</param>
        /// <param name="lot">Lot Number, only used for lot traceable parts</param>
        /// <param name="part">Part number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>QIR Validity</returns>
        public static bool IsValidQIR(string reference, string workOrder, string lot, string part, SqlConnection sqlCon)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return false;
            }
            if (!part.Contains('|'))
            {
                part += "|01";
            }
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _conString = string.IsNullOrEmpty(lot)
                    ? "USE OMNI; SELECT COUNT([QIRNumber]) FROM [qir_metrics_view] WHERE [QIRNumber]=@p1 AND [WONumber]=@p2;"
                    : "USE OMNI; SELECT COUNT([QIRNumber]) FROM [qir_metrics_view] WHERE [QIRNumber] = @p1 AND (([WONumber] = @p2 AND [PartNumber] = @p3) OR ([PartNumber] = @p3 AND [LotNumber] = @p4))";

                var _db = sqlCon.Database;
                try
                {
                    var _rtnVal = false;
                    using (SqlCommand cmd = new SqlCommand(_conString, sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reference);
                        cmd.Parameters.AddWithValue("p2", workOrder);
                        if (!string.IsNullOrEmpty(lot))
                        {
                            cmd.Parameters.AddWithValue("p3", part);
                            cmd.Parameters.AddWithValue("p4", lot);
                        }
                        _rtnVal = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                    sqlCon.ChangeDatabase(_db);
                    return _rtnVal;
                }
                catch (Exception)
                {
                    sqlCon.ChangeDatabase(_db);
                    return false;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Validate lot number existance
        /// </summary>
        /// <param name="lotNbr">Database ready Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT COUNT([Lot_Number]) FROM [dbo].[LOT-INIT] WHERE [Lot_Number]=@p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the Sku's Diamond number using a parent lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number used as a search reference</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Diamond number as string, or the error that was encountered</returns>
        public static string GetDiamondNumber(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken && !string.IsNullOrEmpty(lotNbr))
            {
                try
                {
                    var _rtnValue = string.Empty;
                    using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM [dbo].[SFW_Diamond] sd WHERE sd.[ParentType] IS NOT NULL AND sd.[ChildType] IS NOT NULL AND sd.[ParentLot] = @p1", sqlCon))
                    {
                        if (!lotNbr.Contains("|"))
                        {
                            lotNbr += "|P|01";
                        }
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.SafeGetString("ParentType") == "RR")
                                    {
                                        return reader.SafeGetString("ParentLot").Replace("|P|01", "");
                                    }
                                    else if (reader.SafeGetString("ChildType") == "RR")
                                    {
                                        return reader.SafeGetString("ChildLot").Replace("|P|01", "");
                                    }
                                    else
                                    {
                                        _rtnValue = reader.SafeGetString("ChildLot");
                                    }
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(_rtnValue))
                    {
                        _rtnValue = GetDiamondNumber(_rtnValue, sqlCon);
                        return _rtnValue;
                    }
                    return "error";
                }
                catch (Exception)
                {
                    return "error";
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Lot()
        { }

        /// <summary>
        /// Lot Constructor for dedication population
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        public Lot(string lotNbr)
        {
            if (!string.IsNullOrEmpty(lotNbr))
            {
                var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}' AND [WorkOrderID] != ''");
                foreach (var _row in _rows)
                {
                    Dedication.Add(_row.Field<string>("WorkOrderID"), _row.Field<int>("OnHand"));
                }
            }
        }

        /// <summary>
        /// Lot Constructor for NCR population
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="scrap">Amount of scrap to add to the object</param>
        /// <param name="uom">Unit of measure for this lot</param>
        /// <param name="import">Import type for the lot number</param>
        /// <param name="valid">Validated</param>
        public Lot(string lotNbr, int scrap, string uom, bool import, bool valid)
        {
            LotNumber = lotNbr;
            Onhand = scrap;
            Uom = uom;
            Imported = import;
            Validated = valid;
        }

        /// <summary>
        /// Lot Constructor for dedication population
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Part Number</param>
        public Lot(string lotNbr, string partNbr)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}' AND [Sku] = '{partNbr}'");
            foreach (var _row in _rows)
            {
                LotCollection.Add(_row.Field<string>("LotID"));
            }
        }

        /// <summary>
        /// Validate whether a lot number exists and is attached to the correct part number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="woNbr">Optional: Work Order Number</param>
        /// <param name="type">Optional: Type of lot to single out</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr, string partNbr, string woNbr = null, string type = null)
        {
            if (!string.IsNullOrEmpty(woNbr) && !string.IsNullOrEmpty(type))
            {
                return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}' AND [Type] = '{type}'").Length > 0;
            }
            else if (!string.IsNullOrEmpty(woNbr))
            {
                return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}'").Length > 0;
            }
            else if (!string.IsNullOrEmpty(type))
            {
                return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [Type] = '{type}'").Length > 0;
            }
            else
            {
                return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}'").Length > 0;
            }
        }

        /// <summary>
        /// Validate lot number existance
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Product ID</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr, string partNbr)
        {
            return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}'").Length > 0;
        }

        /// <summary>
        /// Validate lot number existance
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr)
        {
            return MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}'").Length > 0;
        }

        /// <summary>
        /// Validates Sku is in a location
        /// </summary>
        /// <param name="partNbr">Sku ID</param>
        /// <param name="location">Location ID</param>
        /// <returns>valid Sku as bool</returns>
        public static bool ValidSkuLocation(string partNbr, string location)
        {
            return MasterDataSet.Tables[new Lot().GetType().Name].Select($"[SkuID] = '{partNbr}' AND [Location] = '{location}'").Length > 0;
        }

        /// <summary>
        /// Validates Sku has a minimum quantity in a location
        /// </summary>
        /// <param name="partNbr">Sku ID</param>
        /// <param name="location">Location ID</param>
        /// <param name="qty">Minimum quantity to validate</param>
        /// <returns>valid Sku as bool</returns>
        public static bool ValidSkuQuantity(string partNbr, string location, int qty)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[SkuID] = '{partNbr}' AND [Location] = '{location}'");
            return _rows.Length > 0 && _rows.Sum(o => o.Field<int>("OnHand")) > qty;
        }

        /// <summary>
        /// Get the residing location for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Location that the lot number is current in as string</returns>
        public static string GetLocation(string lotNbr)
        {
            var _rows = MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("Location")
                : null;
        }

        /// <summary>
        /// Get the amount of material on hand for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Lot numbers current on hand quantity</returns>
        public static int GetOnHandQuantity(string lotNbr)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}' AND [OnHand] <> 0 AND [Type] = 'Lot'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<int>("OnHand")
                : 0;
        }

        /// <summary>
        /// Get the amount of material on hand for a lot number from a specific location
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="location">Location</param>
        /// <returns>Lot numbers current on hand quantity</returns>
        public static int GetOnHandQuantity(string lotNbr, string location)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}' AND [Location] = '{location}' AND [OnHand] <> 0");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<int>("OnHand")
                : 0;
        }

        /// <summary>
        /// Get the Sku number related to the inputed lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Sku Number as a string</returns>
        public static string GetSkuNumber(string lotNbr)
        {
            var _rows = MasterDataSet.Tables[typeof(Lot).Name].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("SkuID")
                : null;
        }

        /// <summary>
        /// Get the Non-conformance reason related to a selected lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Non-conformance reason as a string</returns>
        public static string GetQualityNote(string lotNbr)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("Notes")
                : null;
        }

        /// <summary>
        /// Get the unit of measure this the lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Unit of measure as a string</returns>
        public static string GetUom(string lotNbr)
        {
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("Uom")
                : null;
        }

        /// <summary>
        /// Lot Constructor for DataRow array conversion
        /// </summary>
        /// <param name="dRows">DataRow array</param>
        /// <param name="type">Type of lot object list to return, Lot = Lot list, Dedicate = Dedicated lot list, NotLot = Nonlot list</param>
        public static List<Lot> DataRowToList(DataRow[] dRows, string type)
        {
            var _tempList = new List<Lot>();
            foreach (DataRow _row in dRows)
            {
                switch (type)
                {
                    case "Lot":
                        _tempList.Add(new Lot
                        {
                            LotNumber = _row.Field<string>("LotID")
                            ,
                            Onhand = _row.Field<int>("OnHand")
                            ,
                            Location = _row.Field<string>("Location")
                        });
                        break;
                    case "Dedicate":
                        _tempList.Add(new Lot
                        {
                            LotNumber = _row.Field<string>("LotID")
                            ,
                            Onhand = _row.Field<int>("OnHand")
                            ,
                            Location = _row.Field<string>("Location")
                        }); ;
                        break;
                    case "NonLot":
                        _tempList.Add(new Lot
                        {
                            Onhand = _row.Field<int>("OnHand")
                            ,
                            Location = _row.Field<string>("Location")
                        });
                        break;
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="lotTrace">Is Sku lot tracecable</param>
        /// <param name="facCode">Facility code</param>
        /// <returns>List of lots associated with the part number</returns>
        public static IList<Lot> GetOnHandList(string partNbr, bool lotTrace, int facCode)
        {
            var _tempList = new List<Lot>();
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[SkuID] = '{partNbr}' AND [Type] <> 'dLot' AND [OnHand] <> 0 AND [Site] = {facCode}");
            foreach (var _row in _rows)
            {
                if (lotTrace)
                {
                    _tempList.Add(new Lot
                    {
                        LotNumber = _row.Field<string>("LotID"),
                        Onhand = _row.Field<int>("OnHand"),
                        Location = _row.Field<string>("Location"),
                        Uom = _row.Field<string>("Uom"),
                        DefectList = Quality.QmsForm.GetNcrList(_row.Field<string>("LotID"), ModelSqlCon)
                    });
                }
                else
                {
                    _tempList.Add(new Lot
                    {
                        Onhand = _row.Field<int>("OnHand"),
                        Location = _row.Field<string>("Location"),
                        Uom = _row.Field<string>("Uom")
                    });
                }
            }
            _tempList = _tempList.OrderBy(o => o.LotNumber).ToList();
            return _tempList;
        }

        /// <summary>
        /// Get a list of lots for work order dedication
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="woNbr">Work Order Number</param>
        /// <returns>List of dedicated lots by work order and part number</returns>
        public static IList<Lot> GetDedicatedList(string partNbr, string woNbr)
        {
            var _tempList = new List<Lot>();
            var _rows = MasterDataSet.Tables[new Lot().GetType().Name].Select($"[SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}' AND [Type] = 'dLot' AND [OnHand] <> 0");
            foreach (var _row in _rows)
            {
                _tempList.Add(new Lot
                {
                    LotNumber = _row.Field<string>("LotID"),
                    Onhand = _row.Field<int>("OnHand"),
                    Location = _row.Field<string>("Location"),
                    Uom = _row.Field<string>("Uom")
                });
            }
            return _tempList;
        }
    }
}
