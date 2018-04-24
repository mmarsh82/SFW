using System;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public class Skew : ModelBase
    {
        #region Properties

        public string Number { get; set; }
        public string Description { get; set; }
        public string Uom { get; set; }
        public DateTime Bom_Rev_Date { get; set; }
        public int OnHand { get; set; }
        public string MasterPrint { get; set; }

        #endregion

        /// <summary>
        /// Skew Default Constructor
        /// </summary>
        public Skew()
        {

        }

        /// <summary>
        /// Skew Constructor
        /// Load a Skew object based on a number
        /// </summary>
        /// <param name="nbr">Skew number to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Skew(string nbr, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                a.[Part_Number], a.[Description], a.[Um], a.[Bom_Rev_Date], b.[Qty_On_Hand], a.[Drawing_Nbrs]
                                                            FROM
                                                                [dbo].[IM-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = a.[Part_Number]
                                                            WHERE
                                                                a.[Part_Number] = @p1; ", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", nbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Number = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                                    Description = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                    Uom = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                                    Bom_Rev_Date = reader.IsDBNull(3) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(3));
                                    OnHand = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                    MasterPrint = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
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
