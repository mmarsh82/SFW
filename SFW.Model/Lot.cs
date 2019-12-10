using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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
            { _loc = value.ToUpper(); OnPropertyChanged(nameof(Location)); }
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
        public Lot(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                if (Dedication == null)
                {
                    Dedication = new Dictionary<string, int>();
                }
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                        [Dedicated_Wo] as 'WorkOrder', [Ded_Wo_Qty] as 'Qty'
                                                        FROM
	                                                        [dbo].[LOT-INIT_Dedicated_Wo_Data]
                                                        WHERE
	                                                        [ID1] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Dedication.Add(reader.SafeGetString("WorkOrder"), reader.SafeGetInt32("Qty"));
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Dedication = null;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandLotList(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _tempList = new List<Lot>();
                    if (!string.IsNullOrEmpty(partNbr))
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                    SUBSTRING(a.[Lot_Number], 0, CHARINDEX('|',a.[Lot_Number],0)) as 'LotNumber',
                                                                    b.[Oh_Qtys], b.[Loc] 
                                                                FROM 
                                                                    [dbo].[LOT-INIT] a 
                                                                RIGHT JOIN 
                                                                    [dbo].[LOT-INIT_Lot_Loc_Qtys] b ON b.[ID1] = a.[Lot_Number] 
                                                                WHERE 
                                                                    a.[Part_Nbr] = @p1 AND [Stores_Oh] != 0
                                                                ORDER BY
	                                                                [LotNumber] ASC;;", sqlCon))
                            {
                            cmd.Parameters.AddWithValue("p1", partNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        _tempList.Add(new Lot
                                        {
                                            LotNumber = reader.SafeGetString("LotNumber"),
                                            Onhand = reader.SafeGetInt32("Oh_Qtys"),
                                            Location = reader.SafeGetString("Loc")
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return _tempList;
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

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandNonLotList(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _tempList = new List<Lot>();
                    if (!string.IsNullOrEmpty(partNbr))
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                [Oh_Qty_By_Loc] AS 'Oh_Qtys',
	                                                            [Location] AS 'Loc'
                                                            FROM
                                                                [dbo].[IPL-INIT_Location_Data]
                                                            WHERE
                                                                [ID1] = @p1 AND [Oh_Qty_By_Loc] != 0;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", partNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        _tempList.Add(new Lot
                                        {
                                            Onhand = reader.SafeGetInt32("Oh_Qtys"),
                                            Location = reader.SafeGetString("Loc")
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return _tempList;
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

        /// <summary>
        /// Get a DataTable of historical transactions of lots based on part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
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
        /// Get a list of lots for work order dedication
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="woNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static List<Lot> GetDedicatedLotList(string partNbr, string woNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _temp = new List<Lot>();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                        SUBSTRING(a.[ID1], 0, LEN(a.[ID1]) - 1) as 'LotNbr', a.[Ded_Wo_Qty] as 'Qty',
	                                                        b.[Loc] as 'Location'
                                                        FROM
	                                                        [dbo].[LOT-INIT_Dedicated_Wo_Data] a
                                                        LEFT OUTER JOIN
	                                                        [dbo].[LOT-INIT_Lot_Loc_Qtys] b ON b.[ID1] = a.[ID1]
                                                        LEFT OUTER JOIN
	                                                        [dbo].[LOT-INIT] c ON c.[Lot_Number] = a.[ID1]
                                                        WHERE
	                                                        a.[Dedicated_Wo] = @p1 AND c.[Part_Nbr] = @p2;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp.Add(new Lot
                                    {
                                        LotNumber = reader.SafeGetString("LotNbr"),
                                        Onhand = reader.SafeGetInt32("Qty"),
                                        Location = reader.SafeGetString("Location")
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

        /// <summary>
        /// Validate whether a lot number exists and is attached to the correct part number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Validation response</returns>
        public static bool LotValidation(string lotNbr, string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT COUNT([Lot_Number]) FROM [dbo].[LOT-INIT] WHERE [Part_Nbr] = @p1 AND [Lot_Number] = CONCAT(@p2, '|P');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", lotNbr);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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
        /// Validate lot number existance
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Validation response</returns>
        public static bool IsValid(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT COUNT([Lot_Number]) FROM [dbo].[LOT-INIT] WHERE [Lot_Number] = CONCAT(@p1, '|P') AND [Stores_Oh] > 0;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out int i) && i > 0;
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
        /// Get the residing location for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Location that the lot number is current in as string</returns>
        public static string GetLotLocation(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [ID2] as 'Location' FROM [dbo].[LOT-INIT_Lot_Loc_Qtys] WHERE [ID1] = CONCAT(@p1, '|P');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var counter = 1;
                            var _loc = string.Empty;
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (counter == 1)
                                    {
                                        _loc = reader.SafeGetString("Location");
                                    }
                                    else
                                    {
                                        return string.Empty;
                                    }
                                }
                            }
                            return _loc;
                        }
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the amount of material on hand for a specific lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Lot numbers current on hand quantity</returns>
        public static int GetLotOnHandQuantity(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Stores_Oh] FROM [dbo].[LOT-INIT] WHERE [Lot_Number] = CONCAT(@p1, '|P');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the amount of material on hand for a lot number from a specific location
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="location">Location</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Lot numbers current on hand quantity</returns>
        public static int GetLotOnHandQuantity(string lotNbr, string location, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Oh_Qtys] FROM [dbo].[LOT-INIT_Lot_Loc_Qtys] WHERE [ID1] = CONCAT(@p1, '|P') AND [ID2] = @p2;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        cmd.Parameters.AddWithValue("p2", location);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the Sku number related to the inputed lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Sku Number as a string</returns>
        public static string GetSkuNumber(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT [Part_Nbr] FROM [dbo].[LOT-INIT] WHERE [Lot_Number] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{lotNbr}|P");
                        return cmd.ExecuteScalar()?.ToString();
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
    }
}
