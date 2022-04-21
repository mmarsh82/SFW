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
                    return null;
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
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of historical lot transactions</returns>
        public static DataTable GetLotHistoryTable(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable dt = new DataTable())
                    {
                        if (!string.IsNullOrEmpty(partNbr))
                        {

                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT 
                                                                                    *
                                                                                FROM
                                                                                    [dbo].[LotHistory]
                                                                                WHERE
                                                                                    [PartNbr] = @p1 AND (CAST([TranDateTime] as DATE) > DATEADD(YEAR, -3, GETDATE()))
                                                                                ORDER BY
                                                                                    [TranDateTime] DESC;", sqlCon))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("p1", partNbr);
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
                    using (SqlCommand cmd = new SqlCommand(@"USE OMNI; SELECT [QIRNumber] FROM [qir_metrics_view] WHERE LotNumber=@p1;", sqlCon))
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
        /// Get the Sku's Diamond number using a parent lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number used as a search reference</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Diamond number as string</returns>
        public static string GetDiamondNumber(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _found = false;
                var _lot = $"a.[Parent_Lot] = '{lotNbr}|P'";
                var _dmdNbr = string.Empty;
                while (!_found)
                {
                    _lot += ";";
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                            SELECT
                                                                SUBSTRING(a.[Component_Lot],0,LEN(a.[Component_Lot]) - 1) as 'Comp_Lot', b.[Inventory_Type] as 'Type'
                                                            FROM
	                                                            [dbo].[Lot Structure] a
                                                            RIGHT OUTER JOIN
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Comp_Pn]
                                                            WHERE
	                                                            {_lot}", sqlCon))
                    {
                        _lot = string.Empty;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.SafeGetString("Type") == "RR")
                                    {
                                        _dmdNbr = reader.SafeGetString("Comp_Lot");
                                        _found = true;
                                    }
                                    else if (string.IsNullOrEmpty(_lot) && reader.SafeGetString("Type") != "HM")
                                    {
                                        _lot += $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                    }
                                    else if (reader.SafeGetString("Type") != "HM")
                                    {
                                        _lot += $" OR a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                    }
                                }
                            }
                            else
                            {
                                return !string.IsNullOrEmpty(_dmdNbr) ? _dmdNbr : lotNbr;
                            }
                        }
                        if (string.IsNullOrEmpty(_lot))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _lot = $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                }
                            }
                        }
                    }
                }
                return _dmdNbr;
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a list of diamond numbers for quality validation
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static List<Lot> GetDiamondList(SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _temp = new List<Lot>();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
	                                                            SUBSTRING(lot.[Lot_Number], 0, CHARINDEX('|', lot.[Lot_Number], 0)) as 'LotNbr'
                                                                ,lot.[Part_Nbr] as 'PartNbr'
                                                                ,CAST(lot.[Date_Recvd] as date) as 'Received_date'
                                                                ,ISNULL(lot.[Verified_Qty], 0) as 'Verified'
                                                                ,(SELECT COUNT(aa.[ID]) FROM [dbo].[IT-INIT] aa WHERE aa.[Lot_Number] = lot.[Lot_Number] AND aa.[Tran_Code] = '44') as 'Wip'
                                                            FROM
	                                                            [dbo].[LOT-INIT] lot
                                                            LEFT JOIN
	                                                            [dbo].[LOT-INIT_Lot_Loc_Qtys] lotloc ON lotloc.[ID1] = lot.[Lot_Number]
                                                            LEFT JOIN
	                                                            [dbo].[IM-INIT] im ON im.[Part_Number] = lot.[Part_Nbr]
                                                            WHERE
	                                                            lotloc.[Oh_Qtys] != 0 AND im.[Inventory_Type] = 'RR'", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp.Add(new Lot
                                    {
                                        LotNumber = reader.SafeGetString("LotNbr")
                                        ,Location = reader.SafeGetString("PartNbr")
                                        ,ReceivedDate = reader.SafeGetDateTime("Received_date")
                                        ,Validated = reader.SafeGetInt32("Verified") == 1
                                        ,TransactionCode = reader.SafeGetInt32("Wip") > 0 ? "false" : "true"
                                    });
                                }
                            }
                        }
                    }
                    return _temp;
                }
                catch (Exception)
                {
                    return new List<Lot>();
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        #endregion

        /// <summary>
        /// Validate whether a lot number exists and is attached to the correct part number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="woNbr">Optional: Work Order Number</param>
        /// <returns>Validation response</returns>
        public static bool LotValidation(string lotNbr, string partNbr, string woNbr = null)
        {
            return string.IsNullOrEmpty(woNbr)
                ? MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}'").Length > 0
                : MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [SkuID] = '{partNbr}' AND [WorkOrderID] = '{woNbr}'").Length > 0;
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
            var _rows = MasterDataSet.Tables["LOT"].Select($"[LotID] = '{lotNbr}' AND [OnHand] <> 0");
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
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandLotList(string partNbr, bool lotTrace)
        {
            var _tempList = new List<Lot>();
            var _rows = MasterDataSet.Tables["LOT"].Select($"[SkuID] = '{partNbr}' AND [Type] <> 'dLot' AND [OnHand] <> 0");
            foreach (var _row in _rows)
            {
                if (lotTrace)
                {
                    _tempList.Add(new Lot
                    {
                        LotNumber = _row.Field<string>("LotID"),
                        Onhand = _row.Field<int>("OnHand"),
                        Location = _row.Field<string>("Location"),
                    });
                }
                else
                {
                    _tempList.Add(new Lot
                    {
                        Onhand = _row.Field<int>("OnHand"),
                        Location = _row.Field<string>("Location"),
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
        public static List<Lot> GetDedicatedLotList(string partNbr, string woNbr)
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
                });
            }
            return _tempList;
        }
    }
}
