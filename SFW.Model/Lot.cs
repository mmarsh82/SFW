using SFW.Model.Enumerations;
using SFW.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 4-25-18

namespace SFW.Model
{
    public class Lot
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

        #endregion

        public Lot()
        {

        }

        /// <summary>
        /// Get a List of lot numbers associated with a part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <returns>List of lots associated with the part number</returns>
        public static List<Lot> GetOnHandLotList(string partNbr, SqlConnection sqlCon)
        {
            try
            {
                var _tempList = new List<Lot>();
                if (!string.IsNullOrEmpty(partNbr))
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                a.[Lot_Number], b.[Oh_Qtys], b.[Loc] 
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
                                        LotNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                        Onhand = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1)),
                                        Location = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
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
                return null;
            }
        }

        /// <summary>
        /// Get a list of historical transactions of lots based on part number
        /// </summary>
        /// <param name="partNbr">SKU Part Number</param>
        /// <returns>List of historical lot transactions</returns>
        public static List<Lot> GetLotHistoryList(string partNbr, SqlConnection sqlCon)
        {
            try
            {
                var _tempList = new List<Lot>();
                if (!string.IsNullOrEmpty(partNbr))
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            a.[ID], a.[Process_Time_Date], a.[Logon], a.[Tran_Code], a.[Lot_Number], a.[From_Loc], a.[To_Loc], a.[Qty], a.[Prior_On_Hand], a.[Reference], b.[Wp_Nbr], c.[So_Reference]
                                                            FROM
	                                                            [dbo].[IT-INIT] a
                                                            RIGHT JOIN 
	                                                            [dbo].[WP-INIT_Lot_Entered] b ON b.[Lot_Entered] = a.[Lot_Number]
                                                            RIGHT JOIN
	                                                            [dbo].[WP-INIT] c ON c.[Wp_Nbr] = b.[Wp_Nbr]
                                                            WHERE
	                                                            a.[ID] LIKE CONCAT('1005050', '%')
                                                            ORDER BY
	                                                            a.[Tran_Date] DESC;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    #region Define Transaction Parameters

                                    //Defining the transaction type, do not want to display any value that is not cataloged in the enumeration
                                    var _verifyValue = Tran_Code.NA;
                                    var _typeVerify = reader.IsDBNull(3) ? false : Enum.TryParse(reader.GetString(3), out _verifyValue);
                                    if (_typeVerify && Enum.IsDefined(typeof(Tran_Code), _verifyValue))
                                    {
                                        //Defining the transaction reference and code based on the transaction type being adjust
                                        var _tranRef = string.Empty;
                                        var _tranCode = string.Empty;
                                        if ((_verifyValue == Tran_Code.ADJUST || _verifyValue == Tran_Code.ISSUE) && !reader.IsDBNull(9) && reader.GetString(9).Contains("*"))
                                        {
                                            var _tranArray = reader.GetString(9).Split('*');
                                            _tranCode = _tranArray[0];
                                            if(_tranArray.Count() == 2)
                                            {
                                                _tranRef = _tranArray[1];
                                            }
                                            else
                                            {
                                                foreach(string s in _tranArray)
                                                {
                                                    _tranRef = s == _tranCode ? string.Empty : _tranRef + s;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _tranRef = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                                        }

                                        //Defining the transaction quantity based on the transaction type
                                        var _tranQty = 0;
                                        if (_verifyValue == Tran_Code.ISSUE || _verifyValue == Tran_Code.SALE)
                                        {
                                            _tranQty = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7)) * -1;
                                        }
                                        else
                                        {
                                            _tranQty = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7));
                                        }

                                        //Define the transaction date for use with a converter
                                        var _tranDateTime = reader.IsDBNull(1) ? null : reader.GetString(1).Split('*');

                                        //Define the location based off the database's to and from locations
                                        var _tranLoc = string.Empty;
                                        if (!reader.IsDBNull(5) || !reader.IsDBNull(6))
                                        {
                                            _tranLoc = reader.IsDBNull(5) ? reader.GetString(6) : reader.IsDBNull(6) ? reader.GetString(5) : $"{reader.GetString(5)} --> {reader.GetString(6)}";
                                        }

                                        #endregion

                                        _tempList.Add(new Lot
                                        {
                                            TransactionKey = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetString(0).Substring(reader.GetString(0).IndexOf('*') + 1)),
                                            Submitter = reader.IsDBNull(2) ? string.Empty : reader.GetString(2).Substring(0, reader.GetString(2).IndexOf(':')),
                                            TransactionType = _verifyValue.GetEnumDescription(),
                                            LotNumber = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                            Location = _tranLoc,
                                            TransactionDate = _tranDateTime == null ? DateTime.MinValue : M2kConverters.GetTimeStamp(Convert.ToInt32(_tranDateTime[0]), Convert.ToInt32(_tranDateTime[1])),
                                            TransactionQty = _tranQty,
                                            Onhand = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8)),
                                            TransactionReference = _tranRef,
                                            TransactionCode = _tranCode,
                                            TransactionWorkOrder = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                            TransactionSalesOrder = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                _tempList = _tempList.OrderBy(o => o.TransactionKey).ToList();
                _tempList.Reverse();
                return _tempList;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
