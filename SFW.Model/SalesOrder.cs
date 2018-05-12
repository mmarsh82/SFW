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
        public DateTime PromiseDate { get; set; }

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
                if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
                {
                    if (soNbr.Contains("*"))
                    {
                        var soNbrArray = soNbr.Split('*');
                        soNbr = $"{soNbrArray[0]}*{soNbrArray[1]}";
                    }
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            c.[ID], a.[Cust_Nbr], b.[Name], c.[Cust_Part_Nbr], c.[Ln_Bal_Qty], a.[Promise_Date20] 
                                                            FROM
	                                                            [dbo].[SOH-INIT] a
                                                            RIGHT JOIN
	                                                            [dbo].[CM-INIT] b ON b.[Cust_Nbr] = a.[Cust_Nbr]
                                                            RIGHT JOIN
	                                                            [dbo].[SOD-INIT] c ON c.[So_Nbr] = a.[So_Nbr]
                                                            WHERE
	                                                            c.[ID] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", soNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        var _tempID = reader.IsDBNull(0) ? null : reader.GetString(0).Split('*');
                                        SalesNumber = _tempID == null ? string.Empty : _tempID[0];
                                        CustomerNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                        CustomerName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                                        CustomerPart = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                                        LineNumber = _tempID == null ? 0 : Convert.ToInt32(_tempID[1]);
                                        LineQuantity = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4));

                                        PromiseDate = reader.IsDBNull(5) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(5));
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
