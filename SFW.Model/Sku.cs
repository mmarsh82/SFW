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
        public string InventoryType { get; set; }
        public int CrewSize { get; set; }

        #endregion

        /// <summary>
        /// Sku Default Constructor
        /// </summary>
        public Sku()
        { }

        /// <summary>
        /// Sku Constructor
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
                                                                a.[Part_Number], a.[Description], a.[Um], a.[Bom_Rev_Date], b.[Qty_On_Hand], a.[Drawing_Nbrs], a.[Inventory_Type], c.[Crew_Size]
                                                            FROM
                                                                [dbo].[IM-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = a.[Part_Number]
                                                            RIGHT JOIN
	                                                            [dbo].[RT-INIT] c ON SUBSTRING(c.[ID],0,CHARINDEX('*',c.[ID],0)) = b.[Part_Nbr]
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
                                    SkuNumber = reader.SafeGetString("Part_Number");
                                    SkuDescription = reader.SafeGetString("Description");
                                    Uom = reader.SafeGetString("Um");
                                    BomRevDate = reader.SafeGetDateTime("Bom_Rev_Date");
                                    TotalOnHand = reader.SafeGetInt32("Qty_On_Hand");
                                    MasterPrint = reader.SafeGetString("Drawing_Nbrs");
                                    InventoryType = reader.SafeGetString("Inventory_Type");
                                    CrewSize = reader.SafeGetInt32("Crew_Size");
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

        /// <summary>
        /// Get the a Sku's crew size
        /// </summary>
        /// <param name="partNbr">Part number to search</param>
        /// <param name="seq">Part sequence to search</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>crew size as int</returns>
        public static int GetCrewSize(string partNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT [Crew_Size] FROM [dbo].[RT-INIT] WHERE [ID] = CONCAT(@p1,'*',@p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int result) ? result : 0;
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
