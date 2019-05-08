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
        public string EngStatus { get; set; }
        public string EngStatusDesc { get; set; }
        public string InventoryType { get; set; }
        public int CrewSize { get; set; }
        public List<string> InstructionList { get; set; }
        public List<string> DiamondNumber { get; set; }
        public string Location { get; set; }
        public string WorkOrder { get; set; }
        public string Operation { get; set; }
        public string Machine { get; set; }

        #endregion

        /// <summary>
        /// Sku Default Constructor
        /// </summary>
        public Sku()
        { }

        /// <summary>
        /// Sku Constructor
        /// Load a Skew object based on a part number
        /// </summary>
        /// <param name="partNbr">Part number to load</param>
        /// <param name="partLoad">Tell the constructor to load a part for tracking</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Sku(string partNbr, bool partLoad, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (partLoad)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT 
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
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT 
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
        /// Sku Constructor
        /// Load a Skew object based on a lot number
        /// </summary>
        /// <param name="lotNbr">Part number to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Sku(string lotNbr, SqlConnection sqlCon)
        {
            var _valid = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                           SELECT [Part_Nbr] FROM [dbo].[LOT-INIT] WHERE [Lot_Number] LIKE CONCAT(@p1,'|P');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotNbr);
                        _valid = !string.IsNullOrEmpty(cmd.ExecuteScalar()?.ToString());
                    }
                    if (_valid)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                            SELECT 
	                                                            a.[Part_Nbr], b.[Description], b.[Um], c.[Locations], c.[Oh_Qtys]
                                                            FROM 
	                                                            [dbo].[LOT-INIT] a 
                                                            LEFT JOIN 
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Part_Nbr]
                                                            LEFT JOIN
	                                                            [dbo].[LOT-INIT_Lot_Loc_Qtys] c ON c.[ID1] = a.[Lot_Number]
                                                            WHERE 
	                                                            a.[Lot_Number] LIKE CONCAT(@p1,'|P');", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", lotNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        SkuNumber = reader.SafeGetString("Part_Nbr");
                                        SkuDescription = reader.SafeGetString("Description");
                                        Uom = reader.SafeGetString("Um");
                                        TotalOnHand = reader.SafeGetInt32("Oh_Qtys");
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

        /// <summary>
        /// Get the Sku's crew size
        /// </summary>
        /// <param name="partNbr">Part number to search</param>
        /// <param name="seq">Part sequence to search</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>crew size as int</returns>
        public static int GetCrewSize(string partNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Crew_Size] FROM [dbo].[RT-INIT] WHERE [ID] = CONCAT(@p1,'*',@p2);", sqlCon))
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

        /// <summary>
        /// Get the Sku's Diamond number using a parent lot number
        /// </summary>
        /// <param name="lotNbr">Lot Number used as a search reference</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Diamond number as string</returns>
        public static string GetDiamondNumber(string lotNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _found = false;
                var _lot = $"a.[Parent_Lot] = '{lotNbr}|P'";
                var _dmdNbr = string.Empty;
                while (!_found)
                {
                    _lot += ";";
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                            SELECT
                                                                SUBSTRING(a.[Component_Lot],0,LEN(a.[Component_Lot]) - 1) as 'Comp_Lot', b.[Inventory_Type] as 'Type'
                                                            FROM
	                                                            [dbo].[Lot Structure] a
                                                            RIGHT OUTER JOIN
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Comp_Pn]
                                                            WHERE
	                                                            {_lot}", sqlCon))
                    {
                        _lot = string.Empty;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (reader.SafeGetString("Type") == "RR")
                                    {
                                        _dmdNbr = reader.SafeGetString("Comp_Lot");
                                        _found = true;
                                    }
                                    else
                                    {
                                        _lot += $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                    }
                                }
                            }
                            else
                            {
                                return _dmdNbr;
                            }
                        }
                    }
                }
                return _dmdNbr;
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get the Sku's Diamond number by crawling the parent part numbers BOM
        /// </summary>
        /// <param name="compList">List of components for a Sku</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Diamond number as string</returns>
        public static string GetDiamondNumber(List<Component> compList, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                var _dmdNbr = string.Empty;
                foreach (var c in compList)
                {
                    if (c.IsLotTrace && c.InventoryType != "HM")
                    {
                        foreach (var w in c.WipInfo)
                        {
                            if (!string.IsNullOrEmpty(w.LotNbr))
                            {
                                var _found = false;
                                var _lot = $"a.[Parent_Lot] = '{w.LotNbr}|P'";
                                while (!_found)
                                {
                                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                            SELECT
                                                                SUBSTRING(a.[Component_Lot],0,LEN(a.[Component_Lot]) - 1) as 'Comp_Lot', b.[Inventory_Type] as 'Type'
                                                            FROM
	                                                            [dbo].[Lot Structure] a
                                                            RIGHT OUTER JOIN
	                                                            [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Comp_Pn]
                                                            WHERE
	                                                            {_lot};", sqlCon))
                                    {
                                        _lot = string.Empty;
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    if (reader.SafeGetString("Type") == "RR")
                                                    {
                                                        _dmdNbr = reader.SafeGetString("Comp_Lot");
                                                        _found = true;
                                                    }
                                                    else
                                                    {
                                                        _lot += $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _dmdNbr = w.LotNbr;
                                                _found = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return _dmdNbr;
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Get a work order's work instructions
        /// </summary>
        /// <param name="woNbr">Sku ID Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A list of URL strings to open the work instructions</returns>
        public static List<string> GetInstructions(string partNbr, SqlConnection sqlCon)
        {
            var _inst = new List<string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT RIGHT([Url], CHARINDEX('\', REVERSE([Url]),1) - 1) as 'Url' FROM [dbo].[IM-INIT_Url_Codes] WHERE [ID1] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _inst.Add($"{reader.SafeGetString("Url")}");
                                }
                            }
                        }
                    }
                    return _inst;
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
        /// Check to see if a Sku number is lot tracable
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>lot tracability as bool</returns>
        public static bool IsLotTracable(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Lot_Trace] FROM [dbo].[IM-INIT] WHERE [Part_Number] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return cmd.ExecuteScalar()?.ToString() == "T";
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
