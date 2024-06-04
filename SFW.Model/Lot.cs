using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 4-25-18

namespace SFW.Model
{
    public partial class Lot : ModelBase
    {
        #region Properties

        public string LotNumber { get; set; }
        public int Onhand { get; set; }
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
        public bool Validated { get; set; }
        public DateTime ReceivedDate { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Lot()
        {

        }

        /// <summary>
        /// Lot Constructor for dedication population
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        public Lot(string lotNbr)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [WorkOrderID] != ''");
            foreach (var _row in _rows)
            {
                Dedication.Add(_row.Field<string>("WorkOrderID"), _row.Field<int>("OnHand"));
            }
        }

        #region Data Access

        /// <summary>
        /// Get a DataTable of all sku objects with onhand values
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of all onhand values</returns>
        public static DataTable GetLotTable(SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable _dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Lot]", sqlCon))
                        {
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
        /// Get a DataTable of all sku objects with onhand values
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of all onhand values</returns>
        public static DataTable GetDiamondTable(SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable _dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Diamond]", sqlCon))
                        {
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
        public static DataTable GetLotHistoryTable(string partNbr, int site, SqlConnection sqlCon)
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
        /// <returns>QIR Validity</returns>
        public static bool IsValidQIR(string reference, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _db = sqlCon.Database;
                try
                {
                    var _rtnVal = false;
                    using (SqlCommand cmd = new SqlCommand(@"USE OMNI; SELECT COUNT([QIRNumber]) FROM [qir_metrics_view] WHERE [QIRNumber]=@p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", reference);
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

        #endregion

        /// <summary>
        /// Get the Sku's Diamond number using a parent lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number used as a search reference</param>
        /// <param name="site">Facility code</param>
        /// <returns>Diamond number as string, or the error that was encountered</returns>
        public static string GetDiamondNumber(string lotNbr, int site)
        {
            var _item = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}'").FirstOrDefault();
            var _type = Sku.GetType(_item.Field<string>("SkuID"), site);
            if (_type == "RR")
            {
                return lotNbr;
            }
            else if (_type == "FR" || _type == "MT")
            {
                return string.Empty;
            }
            var _search = $"[ParentLot] = '{lotNbr}'";
            var _dList = MasterDataSet.Tables["Diamond"].Select(_search);
            while (!string.IsNullOrEmpty(_search))
            {
                _dList = _dList == null ? MasterDataSet.Tables["Diamond"].Select(_search) : _dList;
                if (_dList.Length > 0)
                {
                    _search = string.Empty;
                    foreach(var _row in _dList)
                    {
                        if (_row.Field<string>("IsDiamond") == "Y")
                        {
                            return _row.Field<string>("ChildLot");
                        }
                        else
                        {
                            _search += string.IsNullOrEmpty(_search)
                                ? $"[ParentLot] = '{_row.Field<string>("ChildLot")}'"
                                : $" OR [ParentLot] = '{_row.Field<string>("ChildLot")}'";
                        }
                    }
                    _dList = null;
                }
                else
                {
                    return "error";
                }
            }
            return "error";
        }

        /// <summary>
        /// Validate whether a lot number exists and is attached to the correct part number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="woNbr">Optional: Work Order Number</param>
        /// <param name="type">Optional: Type of lot to single out</param>
        /// <returns>Validation response</returns>
        public static bool LotValidation(string lotNbr, string partNbr, string woNbr = null, string type = null)
        {
            if (!string.IsNullOrEmpty(woNbr) && !string.IsNullOrEmpty(type))
            {
                return MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}' AND [Type] = '{type}'").Length > 0;
            }
            else if (!string.IsNullOrEmpty(woNbr))
            {
                return MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}'").Length > 0;
            }
            else if (!string.IsNullOrEmpty(type))
            {
                return MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [Type] = '{type}'").Length > 0;
            }
            else
            {
                return MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}'").Length > 0;
            }
        }

        /// <summary>
        /// Validate lot number existance
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr)
        {
            return MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}'").Length > 0;
        }

        /// <summary>
        /// Get the residing location for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Location that the lot number is current in as string</returns>
        public static string GetLotLocation(string lotNbr)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("Location")
                : null;
        }

        /// <summary>
        /// Get the amount of material on hand for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Lot numbers current on hand quantity</returns>
        public static int GetLotOnHandQuantity(string lotNbr)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [OnHand] <> 0 AND [Type] = 'Lot'");
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
        public static int GetLotOnHandQuantity(string lotNbr, string location)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [Location] = '{location}' AND [OnHand] <> 0");
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
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("SkuID")
                : null;
        }

        /// <summary>
        /// Get the Non-conformance reason related to a selected lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <returns>Non-conformance reason as a string</returns>
        public static string GetNCRNote(string lotNbr)
        {
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}'");
            return _rows.Length > 0
                ? _rows.FirstOrDefault().Field<string>("Notes")
                : null;
        }

        /// <summary>
        /// Lot Constructor for DataRow array conversion
        /// </summary>
        /// <param name="dRows">DataRow array</param>
        /// <param name="type">Type of lot object list to return, Lot = Lot list, Dedicate = Dedicated lot list, NotLot = Nonlot list</param>
        public static List<Lot> DataRowToLotList(DataRow[] dRows, string type)
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
                            ,Onhand = _row.Field<int>("OnHand")
                            ,Location = _row.Field<string>("Location")
                        });
                        break;
                    case "Dedicate":
                        _tempList.Add(new Lot
                        {
                            LotNumber = _row.Field<string>("LotID")
                            ,Onhand = _row.Field<int>("OnHand")
                            ,Location = _row.Field<string>("Location")
                        });;
                        break;
                    case "NonLot":
                        _tempList.Add(new Lot
                        {
                            Onhand = _row.Field<int>("OnHand")
                            ,Location = _row.Field<string>("Location")
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
        public static IList<Lot> GetOnHandLotList(string partNbr, bool lotTrace, int facCode)
        {
            var _tempList = new List<Lot>();
            var _rows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{partNbr}' AND [Type] <> 'dLot' AND [OnHand] <> 0 AND [Site] = {facCode}");
            foreach (var _row in _rows)
            {
                if (lotTrace)
                {
                    _tempList.Add(new Lot
                    {
                        LotNumber = _row.Field<string>("LotID"),
                        Onhand = _row.Field<int>("OnHand"),
                        Location = _row.Field<string>("Location"),
                        Uom = _row.Field<string>("Uom")
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
        public static IList<Lot> GetDedicatedLotList(string partNbr, string woNbr)
        {
            var _tempList = new List<Lot>();
            var _rows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}' AND [Type] = 'dLot' AND [OnHand] <> 0");
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
