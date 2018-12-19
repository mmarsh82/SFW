using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-25-18

namespace SFW.Model
{
    public class Lot : ModelBase
    {
        #region Properties

        public string LotNumber { get; set; }
        public int Onhand { get; set; }
        public string Location { get; set; }
        public int TransactionKey { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public int TransactionQty { get; set; }
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

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandLotList(string partNbr, SqlConnection sqlCon)
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
                                                                a.[Part_Nbr] = @p1 AND [Stores_Oh] != 0;", sqlCon))
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

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandNonLotList(string partNbr, SqlConnection sqlCon)
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

        /// <summary>
        /// Get a DataTable of historical transactions of lots based on part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable of historical lot transactions</returns>
        public static DataTable GetLotHistoryTable(string partNbr, SqlConnection sqlCon)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    if (!string.IsNullOrEmpty(partNbr))
                    {

                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT 
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

        /// <summary>
        /// Get a list of lots for work order dedication
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <param name="woNbr">Lot Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static List<Lot> GetDedicatedLotList(string partNbr, string woNbr, SqlConnection sqlCon)
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
    }
}
