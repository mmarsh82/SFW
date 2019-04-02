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
    }
}
