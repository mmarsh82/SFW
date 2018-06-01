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

        #endregion

        public Lot()
        {

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
                                                                                a.[TranDateTime], 
                                                                                CASE WHEN a.[Scan_Station_ID] IS NOT NULL 
		                                                                            THEN CASE WHEN ISNUMERIC(a.[Scan_Station_ID]) = 0
				                                                                            THEN a.[Scan_Station_ID] 
				                                                                            ELSE (SELECT        
						                                                                            CONCAT(b1.[First_Name], ' ', b1.[Last_Name])
					                                                                            FROM
						                                                                            [dbo].[WIP_HIST-INIT] a1 
					                                                                            RIGHT JOIN
						                                                                            [dbo].[EMPLOYEE_MASTER-INIT] b1 ON b1.[Emp_No] = a1.[Crew_Leader]
					                                                                            WHERE        
						                                                                            a1.[ID] = a.[Scan_Station_ID]) END 
		                                                                            ELSE SUBSTRING(a.[Logon], 0, CHARINDEX(':', a.[Logon], 0))
	                                                                            END AS 'Submitter', 
                                                                                d.[Ext_Desc] AS 'TranType', 
                                                                                CASE WHEN a.[Tran_Code] = 44 OR a.[Tran_Code] = 50 
                                                                                    THEN SUBSTRING(a.[Reference], 0, CHARINDEX('*', a.[Reference], 0)) 
                                                                                    ELSE ''
	                                                                            END AS 'TranCode',
                                                                                SUBSTRING(a.[Lot_Number], 0, CHARINDEX('|', a.[Lot_Number], 0)) AS 'LotNumber', 
                                                                                CASE WHEN a.[From_Loc] IS NULL
                                                                                    THEN a.[To_Loc]
                                                                                    ELSE CASE WHEN a.[To_Loc] IS NULL
			                                                                            THEN a.[From_Loc]
			                                                                            ELSE CONCAT(a.[From_Loc], ' --> ', a.[To_Loc]) END
	                                                                            END AS 'TranLoc',
                                                                                CAST (CASE WHEN a.[Tran_Code] = 44 OR a.[Tran_Code] = 49 
                                                                                    THEN a.[Qty] * - 1 
                                                                                    ELSE a.[Qty] END AS int) AS 'TranQty',
                                                                                CAST(a.[Prior_On_Hand] AS int) AS 'OnHand',
                                                                                CASE WHEN a.[Tran_Code] = 44 OR a.[Tran_Code] = 50
                                                                                    THEN SUBSTRING(a.[Reference], CHARINDEX('*', a.[Reference], 0) + 1, LEN(a.[Reference]))
                                                                                    ELSE ''
	                                                                            END AS 'Reference',
                                                                                b.[Wp_Nbr],
                                                                                ISNULL(SUBSTRING(c.[So_Reference], 0, LEN(c.[So_Reference]) - 1), '') AS 'SalesOrder'
                                                                            FROM
                                                                                dbo.[IT-INIT] AS a 
                                                                            RIGHT OUTER JOIN
                                                                                dbo.[WP-INIT_Lot_Entered] AS b ON b.[Lot_Entered] = a.[Lot_Number]
                                                                            RIGHT OUTER JOIN
                                                                                dbo.[WP-INIT] AS c ON c.[Wp_Nbr] = b.[Wp_Nbr]
                                                                            RIGHT OUTER JOIN
                                                                                dbo.[IT-INIT_Tran_Code] AS d ON d.[ID] = a.[Tran_Code]
                                                                            WHERE
                                                                                a.[ID] LIKE CONCAT(@p1,'%') AND (a.[Tran_Date] > DATEADD(YEAR, -3, GETDATE()))
                                                                            ORDER BY
                                                                                a.[TranDateTime] DESC;", sqlCon))
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
    }
}
