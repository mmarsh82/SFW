using System;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 5-4-18

namespace SFW.Model
{
    public class SalesOrder : ModelBase
    {
        #region Properties

        public string SalesNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPart { get; set; }
        public int LineNumber { get; set; }
        public int LineQuantity { get; set; }
        public string LineNotes { get; set; }
        public bool LoadPattern { get; set; }
        public string InternalComments { get; set; }

        #endregion

        /// <summary>
        /// SalesOrder Object Default Constructor
        /// </summary>
        public SalesOrder()
        { }

        /// <summary>
        /// SalesOrder Constructor
        /// Load a SalesOrder object based on a sales and line number
        /// </summary>
        /// <param name="soNbr">SalesOrder number to load, delimiter for line item is '*' i.e. SalesNumber*LineNumber</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public SalesOrder(string soNbr, SqlConnection sqlCon)
        {
            if (!string.IsNullOrEmpty(soNbr))
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    if (soNbr.Contains("*"))
                    {
                        var soNbrArray = soNbr.Split('*');
                        soNbr = $"{soNbrArray[0]}*{soNbrArray[1]}";
                        SalesNumber = soNbrArray[0];
                        LineNumber = Convert.ToInt32(soNbrArray[1]);
                    }
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT
	                                                                a.[Cust_Nbr], a.[Cust_Part_Nbr], a.[Ln_Bal_Qty],
	                                                                b.[Name] as 'Cust_Name'
                                                                FROM
	                                                                [dbo].[SOD-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[CM-INIT] b ON b.[Cust_Nbr] = a.[Cust_Nbr]
                                                                WHERE
	                                                                a.[ID] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", soNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        CustomerNumber = reader.SafeGetString("Cust_Nbr");
                                        CustomerName = reader.SafeGetString("Cust_Name");
                                        CustomerPart = reader.SafeGetString("Cust_Part_Nbr");
                                        LineQuantity = reader.SafeGetInt32("Ln_Bal_Qty");
                                        LineNotes = string.Empty;
                                    }
                                }
                            }
                        }
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

        public SalesOrder(DataRow dRow)
        {
            SalesNumber = dRow.Field<string>("So_Nbr");
            CustomerNumber = dRow.Field<string>("Cust_Nbr");
            CustomerName = dRow.Field<string>("Cust_Name");
            CustomerPart = dRow.Field<string>("Cust_Part_Nbr");
            LineNumber = dRow.Field<int>("LineNumber");
        }

        /// <summary>
        /// Get the sales order internal comments
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        public void GetInternalComments(SqlConnection sqlCon)
        {
            if (!string.IsNullOrEmpty(SalesNumber))
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    var _tempSales = SalesNumber.Contains("*") ? SalesNumber.Split('*')[0] : SalesNumber;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
                                                                    [Internal_Comments] as 'IntComm'
                                                                FROM
                                                                    [dbo].[SOH-INIT_Internal_Comments]
                                                                WHERE
                                                                    [So_Nbr] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", _tempSales);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.SafeGetString("IntComm").Contains("bag"))
                                        {
                                            InternalComments += $" {reader.SafeGetString("IntComm").Replace('"', ' ')}";
                                        }
                                    }
                                }
                            }
                        }
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
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT
                                                                                b.[ID]
	                                                                            ,a.[So_Nbr]
	                                                                            ,CAST(b.[Line_Nbr] as int) as 'LineNumber'
	                                                                            ,b.[Part_Wo_Gl]
                                                                                ,b.[Ln_Bal_Qty]
	                                                                            ,b.[Um_Base]
	                                                                            ,ISNULL(b.[D_esc], (SELECT aa.[Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = b.[Part_Wo_Gl])) as 'Description'
	                                                                            ,b.[Comp]
                                                                                ,a.[Cust_Nbr]
	                                                                            ,(SELECT ab.[Name] FROM [dbo].[CM-INIT] ab WHERE ab.[Cust_Nbr] = a.[Cust_Nbr]) as 'Cust_Name'
	                                                                            ,a.[Cust_Ord]
                                                                                ,b.[Cust_Part_Nbr]
	                                                                            ,a.[Credit_Code]
	                                                                            ,CASE WHEN a.[Ord_Type] = 'FSE' OR a.[Ord_Type] LIKE '%DE' THEN 'EOP' ELSE a.[Ord_Type] END as 'Type'
	                                                                            ,a.[Frght_Type]
	                                                                            ,a.[Jump_Reason]
	                                                                            ,a.[Ship_To_Name]
	                                                                            ,a.[Ship_To_Addr1]
	                                                                            ,a.[Ship_To_Addr2]
	                                                                            ,a.[Ship_To_City]
	                                                                            ,a.[Ship_To_State]
	                                                                            ,a.[Ship_To_Zip]
	                                                                            ,ISNULL(a.[Ship_To_Country], 'US') as 'Ship_To_Country'
	                                                                            ,CAST(a.[Change_Date] as date) as 'Change_Date'
	                                                                            ,CAST(a.[Date_Added] as date) as 'Date_Added'
	                                                                            ,CAST(a.[Delivery_Date] as date) as 'Delivery_Date'
	                                                                            ,CAST(a.[Cust_Promise_Date] as date) as 'Cust_Promise_Date'
	                                                                            ,CAST(a.[Requested_Date] as date) as 'Requested_Date'
	                                                                            ,CAST(a.[Promise_Date20] as date) as 'Promise_Date20'
	                                                                            ,CAST(a.[Commit_Ship_Date] as date) as 'Commit_Ship_Date'
                                                                            FROM
	                                                                            [dbo].[SOH-INIT] a
                                                                            RIGHT JOIN
	                                                                            [dbo].[SOD-INIT] b ON b.[So_Nbr] = a.[So_Nbr]
                                                                            WHERE
	                                                                            a.[Order_Status] IS NULL AND b.[Comp] = 'O' AND (b.[Part_Wo_Gl] IS NOT NULL OR b.[D_esc] IS NOT NULL) AND a.[So_Nbr] IS NOT NULL
                                                                            ORDER BY
	                                                                            a.[Date_Added] ASC", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
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
    }
}
