using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    public class Sku : ModelBase
    {
        #region Properties

        public string SkuNumber { get; set; }
        public string SkuDescription { get; set; }
        public string Uom { get; set; }
        public string BomRevLevel { get; set; }
        public DateTime BomRevDate { get; set; }
        public int TotalOnHand { get; set; }
        public string MasterPrint { get; set; }
        public List<Component> Bom { get; set; }
        public QualityTask QTask { get; set; }

        #endregion

        /// <summary>
        /// Skew Default Constructor
        /// </summary>
        public Sku()
        { }

        /// <summary>
        /// Skew Constructor
        /// Load a Skew object based on a number
        /// </summary>
        /// <param name="partNbr">Part number to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Sku(string partNbr, SqlConnection sqlCon)
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
                                                                a.[Part_Number] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    SkuNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                                    SkuDescription = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                    Uom = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                                    BomRevDate = reader.IsDBNull(3) ? DateTime.MinValue : DateTime.TryParse(reader.GetValue(3).ToString(), out DateTime _brd) ? _brd : DateTime.MinValue;
                                    TotalOnHand = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
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
