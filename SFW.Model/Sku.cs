using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

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
        public string DiamondNumber { get; set; }
        public string Location { get; set; }
        public string WorkOrder { get; set; }
        public string Operation { get; set; }
        public string Machine { get; set; }
        public string MachineGroup { get; set; }
        public bool Inspection { get; set; }
        public string NonCon { get; set; }
        public List<string> ToolList { get; set; }

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
                        var _valid = false;
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                           SELECT a.[Part_Number] FROM [dbo].[IM-INIT] a WHERE a.[Part_Number] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", partNbr);
                            _valid = !string.IsNullOrEmpty(cmd.ExecuteScalar()?.ToString());
                        }
                        if (_valid)
                        {
                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT 
	                                                                a.[Description]
	                                                                ,a.[Um]
                                                                FROM
                                                                    [dbo].[IM-INIT] a
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
                                            SkuNumber = partNbr;
                                            SkuDescription = reader.SafeGetString("Description");
                                            Uom = reader.SafeGetString("Um");
                                        }
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
                                        SkuNumber = partNbr;
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
            var _selectCmd = sqlCon.Database == "CSI_MAIN"
                ? @"SELECT 
                        a.[Part_Nbr]
                        ,b.[Description]
	                    ,b.[Um]
                        ,'' as 'Notes'
	                    ,c.[Oh_Qtys] as 'Qty'
	                    ,c.[Locations] as 'Loc'
                    FROM
                        [dbo].[LOT-INIT] a
                    LEFT JOIN
                        [dbo].[IM-INIT] b ON b.[Part_Number] = a.[Part_Nbr]
                    LEFT JOIN
	                    [dbo].[LOT-INIT_Lot_Loc_Qtys] c ON c.[ID1] = a.[Lot_Number]
                    WHERE
                        a.[Lot_Number] = CONCAT(@p1,'|P');"

                : @"SELECT 
	                    a.[Part_Nbr]
	                    ,c.[Description]
	                    ,c.[Um]
	                    ,b.[Notes]
	                    ,d.[Oh_Qtys] as 'Qty'
	                    ,d.[Locations] as 'Loc'
                    FROM 
	                    [dbo].[LOT-INIT] a
                    LEFT JOIN
	                    [dbo].[LOT-SA] b ON b.[Lot_Number] = a.[Lot_Number]
                    LEFT JOIN 
	                    [dbo].[IM-INIT] c ON c.[Part_Number] = a.[Part_Nbr]
                    LEFT JOIN
	                    [dbo].[LOT-INIT_Lot_Loc_Qtys] d ON d.[ID1] = a.[Lot_Number]
                    WHERE 
	                    a.[Lot_Number] = CONCAT(@p1,'|P');";
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
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; {_selectCmd}", sqlCon))
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
                                        NonCon = reader.SafeGetString("Notes").Replace("/", "");
                                        TotalOnHand = reader.SafeGetInt32("Qty");
                                        Location = reader.SafeGetString("Loc");
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
        /// Load a Skew object based on a part number when using part structure
        /// </summary>
        /// <param name="partNbr">Part number to load</param>
        /// <param name="partStructure">Enter value of 1</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public Sku(string partNbr, int partStructure, SqlConnection sqlCon)
        {
            var optional = partStructure;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT 
                                                                a.[Part_Number], a.[Description], a.[Um], a.[Bom_Rev_Date], b.[Qty_On_Hand], a.[Drawing_Nbrs], a.[Inventory_Type], a.[Accounting_Status]
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
                                    SkuNumber = reader.SafeGetString("Part_Number");
                                    SkuDescription = reader.SafeGetString("Description");
                                    Uom = reader.SafeGetString("Um");
                                    TotalOnHand = reader.SafeGetInt32("Qty_On_Hand");
                                    EngStatus = reader.SafeGetString("Accounting_Status");
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
                        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out int result) ? result : 0;
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
                                    else if (string.IsNullOrEmpty(_lot) && reader.SafeGetString("Type") != "HM")
                                    {
                                        _lot += $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                    }
                                    else if (reader.SafeGetString("Type") != "HM")
                                    {
                                        _lot += $" OR a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
                                    }
                                }
                            }
                            else
                            {
                                return !string.IsNullOrEmpty(_dmdNbr) ? _dmdNbr : lotNbr;
                            }
                        }
                        if (string.IsNullOrEmpty(_lot))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _lot = $"a.[Parent_Lot] = '{reader.SafeGetString("Comp_Lot")}|P'";
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
        /// Get a Sku's work instructions
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="siteNbr">Facility number to run on</param>
        /// <param name="filepath">Path to the file</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A list of URL strings to open the work instructions</returns>
        public static List<string> GetInstructions(string partNbr, int siteNbr, string filepath, SqlConnection sqlCon)
        {
            //TODO: this code will need to be reformated once CSI has moved to the same process model as WCCO
            var _inst = new List<string>();
            if (siteNbr == 0)
            {
                if(File.Exists($"{filepath}{partNbr}.pdf"))
                {
                    _inst.Add(partNbr);
                }
                return _inst;
            }
            else
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Work_Instructions] as 'WI' FROM [dbo].[IM-INIT_Work_Instructions] WHERE [Part_Number] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", partNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        var dir = new DirectoryInfo(filepath);
                                        var fileList = dir.GetFiles($"*{reader.SafeGetInt32("WI")}*");
                                        foreach (var file in fileList)
                                        {
                                            _inst.Add(file.Name);
                                        }
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
        }

        /// <summary>
        /// Get a Sku's work instructions
        /// </summary>
        /// <param name="woNbr">Sku ID Number</param>
        /// <param name="siteNbr">Facility number to run on</param>
        /// <param name="filepath">Path to the file</param>
        /// <returns>A table of URL strings to open the work instructions</returns>
        public static DataTable GetInstructions(SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT [Part_Number] as 'ID', [Work_Instructions] as 'WI' FROM [dbo].[IM-INIT_Work_Instructions]", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
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
        /// Get a Sku's tool list
        /// </summary>
        /// <param name="woNbr">Sku ID Number</param>
        /// <param name="woSeq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A list of tool's associated with the Sku</returns>
        public static List<string> GetTools(string partNbr, string woSeq, SqlConnection sqlCon)
        {
            var _tempList = new List<string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Tool_Tape] as 'Tool' FROM [dbo].[RT-INIT_Tool_Tape] WHERE [ID1] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{partNbr}*{woSeq}");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(reader.SafeGetString("Tool"));
                                }
                            }
                        }
                    }
                    return _tempList;
                }
                catch (SqlException sqlEx)
                {
                    return null;
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
        /// Get a Table of all tools in the database
        /// </summary>
        /// <returns>A datatable of all tool's</returns>
        public static DataTable GetTools(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                [ID1] as 'ID'
	                                                                                ,[Tool_Tape] as 'Tool'
                                                                                FROM
	                                                                                [dbo].[RT-INIT_Tool_Tape];", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(sqlEx.Message);
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

        /// <summary>
        /// Validates that the location entered is a valid M2k location
        /// </summary>
        /// <param name="location">location to validate</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>valid location as bool</returns>
        public static bool IsValidLocation(string location, SqlConnection sqlCon)
        {
            location = location ?? string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT COUNT([ID]) FROM [dbo].[LOC_MASTER-INIT] WHERE [ID] = CONCAT('01*', @p1);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", location);
                        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out int i) && i > 0;
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
        /// Validates Sku
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="location">Optional: Verify that the sku is located in a specific location</param>
        /// <param name="woNbr">Optional: Verify that the sku was ran on a specific work order</param>
        /// <param name="qty">Optional: Verify that the sku has a minimum quantity on hand</param>
        /// <returns>valid Sku as bool</returns>
        public static bool IsValid(string partNbr, SqlConnection sqlCon, string location = "", string woNbr = "", int qty = 0)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                DECLARE @validPart as tinyint;
                                                                SET @validPart = CASE WHEN (SELECT SUM([Oh_Qty_By_Loc]) FROM [dbo].[IPL-INIT_Location_Data] WHERE [ID1] = @p1 AND [Location] = @p2) > CAST(@p3 as int) THEN 1 ELSE 0 END;
                                                                DECLARE @validWo as tinyint;
                                                                SET @validWo = (SELECT COUNT([Wp_Nbr]) FROM [dbo].[WP-INIT] WHERE [Wp_Nbr] = @p4 AND [Part_Wo_Desc] = @p1);
                                                                SELECT TOP(1) @validPart + @validWo as 'Valid' FROM [dbo].[IM-INIT];", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", location);
                        cmd.Parameters.AddWithValue("p3", qty);
                        cmd.Parameters.AddWithValue("p4", woNbr);
                        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out int i) && i > 0;
                    }
                }
                catch (SqlException sqlEx)
                {
                    return false;
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
        /// Get the default or backflush location for any part number
        /// </summary>
        /// <param name="partNbr">Sku Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>backflush or default location as string</returns>
        public static string GetBackFlushLoc(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Wip_Rec_Loc] FROM [dbo].[IPL-INIT] WHERE [Part_Nbr] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return cmd.ExecuteScalar()?.ToString();
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
        /// Get a Sku's current on hand value for a specific lot number
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>on hand value as int</returns>
        public static int GetOnhandQuantity(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Qty_On_Hand] FROM [dbo].[IPL-INIT] WHERE [Part_Nbr] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
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
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static bool Exists(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                COUNT([Part_Number])
                                                                FROM
	                                                                [dbo].[IM-INIT]
                                                                WHERE
	                                                                [Part_Number] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return int.TryParse(cmd.ExecuteScalar()?.ToString(), out int i) && i > 0;
                    }
                }
                catch (SqlException sqlEx)
                {
                    return false;
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
        /// Check to see if a Sku exists in the database
        /// </summary>
        /// <param name="partNbr">Part Number to check</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Pass/Fail as boolean</returns>
        public static string GetMasterNumber(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                [Drawing_Nbrs]
                                                                FROM
	                                                                [dbo].[IM-INIT]
                                                                WHERE
	                                                                [Part_Number] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        var _master = cmd.ExecuteScalar();
                        return string.IsNullOrEmpty(_master.ToString()) ? partNbr : _master.ToString();
                    }
                }
                catch (SqlException sqlEx)
                {
                    return null;
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
        /// Search for a part number of a description of a part in the database
        /// </summary>
        /// <param name="searchInput">Search input to find</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, bool> Search(string searchInput, SqlConnection sqlCon)
        {
            var _returnList = new Dictionary<Sku, bool>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                [Part_Number]
                                                                    ,[Description]
                                                                    ,[Accounting_Status]
                                                                    ,[Drawing_Nbrs]
                                                                FROM
	                                                                [dbo].[IM-INIT]
                                                                WHERE
	                                                                [Part_Number] LIKE CONCAT(CONCAT('%', @p1), '%') OR [Description] LIKE CONCAT(CONCAT('%', @p1), '%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", searchInput);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _status = reader.SafeGetString("Accounting_Status") == "A" ? true : false;
                                    var _tSku = new Sku
                                    {
                                        SkuNumber = reader.SafeGetString("Part_Number")
                                        ,SkuDescription = reader.SafeGetString("Description")
                                        ,MasterPrint = reader.SafeGetString("Drawing_Nbrs")
                                        ,EngStatus = reader.SafeGetString("Accounting_Status")
                                    };
                                    _returnList.Add(_tSku, _status);
                                }
                            }
                        }
                        return _returnList;
                    }
                }
                catch (SqlException sqlEx)
                {
                    return null;
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
        /// Get a Sku's structure
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="siteCrawl">OPTIONAL: tells the query to crawl all site databases for product structure</param>
        /// <returns>List of found part numbers</returns>
        public static IDictionary<Sku, int> GetStructure(string partNbr, SqlConnection sqlCon, bool siteCrawl = false)
        {
            var _returnList = new Dictionary<Sku, int>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _counter = 0;
                    var _parentCount = 1;
                    var _found = false;
                    var _partList = new List<string>
                    {
                        partNbr
                    };

                    #region Parent Part Retrieval

                    while (!_found)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Part'
                                                                    ,SUBSTRING(a.[ID],0, CHARINDEX('*', a.[ID], 0)) as 'Parent'
	                                                                ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) as 'Desc'
	                                                                ,a.[Bom_Uom] as 'Uom'
	                                                                ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                ,b.[Accounting_Status] as 'Status'
                                                                    ,b.[Drawing_Nbrs] as 'Master'
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))
                                                                WHERE
	                                                                a.[Qty_Per_Assy] IS NOT NULL", sqlCon))
                        {
                            foreach (var s in _partList.Distinct())
                            {
                                if (_partList.IndexOf(s) == 0)
                                {
                                    cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                    cmd.Parameters.AddWithValue("p1", s);
                                }
                                else
                                {
                                    cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                    cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                }
                            }
                            cmd.CommandText += ")";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    _partList = new List<string>();
                                    _counter--;
                                    while (reader.Read())
                                    {
                                        var _tempSku = new Sku
                                        {
                                            SkuNumber = reader.SafeGetString("Part")
                                            ,
                                            EngStatusDesc = reader.SafeGetString("Parent")
                                            ,
                                            DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Parent")) > 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Parent")).DiamondNumber
                                                : _parentCount.ToString()
                                            ,
                                            SkuDescription = reader.SafeGetString("Desc")
                                            ,
                                            Uom = reader.SafeGetString("Uom")
                                            ,
                                            Operation = reader.SafeGetDouble("Qpa").ToString()
                                            ,
                                            EngStatus = reader.SafeGetString("Status")
                                            ,
                                            MasterPrint = reader.SafeGetString("Master")
                                        };
                                        _returnList.Add(_tempSku, _counter);
                                        _partList.Add(reader.SafeGetString("Part"));
                                        _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                    }
                                }
                                else
                                {
                                    _found = true;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Child Part Retrieval

                    _counter = 0;
                    var _childCount = 1;
                    _found = false;
                    _partList = new List<string>
                    {
                        partNbr
                    };
                    while (!_found)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
	                                                                SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID])) as 'Part'
                                                                    ,SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Child'
                                                                    ,(SELECT COUNT(aa.[ID]) FROM [dbo].[PS-INIT] aa WHERE SUBSTRING(aa.[ID], CHARINDEX('*', aa.[ID], 0) + 1, LEN(aa.[ID])) = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID]))) as 'ChildCount'
	                                                                ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID]))) as 'Desc'
	                                                                ,a.[Bom_Uom] as 'Uom'
	                                                                ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                ,b.[Accounting_Status] as 'Status'
                                                                FROM
	                                                                [dbo].[PS-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                                WHERE
	                                                                a.[Qty_Per_Assy] IS NOT NULL", sqlCon))
                        {
                            foreach (var s in _partList.Distinct())
                            {
                                if (_partList.IndexOf(s) == 0)
                                {
                                    cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                    cmd.Parameters.AddWithValue("p1", s);
                                }
                                else
                                {
                                    cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                    cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                }
                            }
                            cmd.CommandText += ")";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    _partList = new List<string>();
                                    _counter++;
                                    while (reader.Read())
                                    {
                                        var _tempSku = new Sku
                                        {
                                            SkuNumber = reader.SafeGetString("Part")
                                            ,
                                            EngStatusDesc = reader.SafeGetString("Child")
                                            ,
                                            DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Child")) > 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Child")).DiamondNumber
                                                : _parentCount.ToString()
                                            ,
                                            Location = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Child")) > 0 && reader.SafeGetInt32("ChildCount") == 0
                                                ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Child")).Location
                                                : _childCount.ToString()
                                            ,
                                            SkuDescription = reader.SafeGetString("Desc")
                                            ,
                                            Uom = reader.SafeGetString("Uom")
                                            ,
                                            Operation = reader.SafeGetDouble("Qpa").ToString()
                                            ,
                                            EngStatus = reader.SafeGetString("Status")
                                        };
                                        _returnList.Add(_tempSku, _counter);
                                        _partList.Add(reader.SafeGetString("Part"));
                                        _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                        _childCount = _returnList.Keys.Count(o => o.Location == _childCount.ToString()) > 0 ? _childCount + 1 : _childCount;
                                    }
                                }
                                else
                                {
                                    _found = true;
                                }
                            }
                        }
                    }

                    #endregion

                    //Populating the input part
                    var _parent = new Sku(partNbr, 1, sqlCon);
                    _parent.EngStatusDesc = _parent.SkuNumber;
                    _parent.DiamondNumber = "0";
                    _returnList.Add(_parent, 0);

                    #region Site Crawl logic for engineer privileges

                    if (siteCrawl)
                    {
                        _partList.Clear();
                        foreach (var _part in _returnList.Where(o => o.Value <= 0))
                        {
                            _partList.Add($"{_part.Key.SkuNumber}C");
                        }
                        _found = false;
                        var _first = true;
                        sqlCon.ChangeDatabase("CSI_MAIN");
                        while (!_found)
                        {
                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                        SELECT
	                                                                        SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Part'
                                                                            ,SUBSTRING(a.[ID],0, CHARINDEX('*', a.[ID], 0)) as 'Parent'
	                                                                        ,(SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) as 'Desc'
	                                                                        ,a.[Bom_Uom] as 'Uom'
	                                                                        ,a.[Qty_Per_Assy] as 'Qpa'
	                                                                        ,b.[Accounting_Status] as 'Status'
                                                                            ,b.[Drawing_Nbrs] as 'Master'
                                                                        FROM
	                                                                        [dbo].[PS-INIT] a
                                                                        RIGHT JOIN
	                                                                        [dbo].[IM-INIT] b ON b.[Part_Number] = SUBSTRING(a.[ID], 0, CHARINDEX('*', a.[ID], 0))
                                                                        WHERE
	                                                                        a.[Qty_Per_Assy] IS NOT NULL
	                                                                        AND (SELECT [Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))) NOT LIKE '%POLY%'", sqlCon))
                            {
                                foreach (var s in _partList.Distinct())
                                {
                                    if (_partList.IndexOf(s) == 0)
                                    {
                                        cmd.CommandText += $" AND (b.[Part_Number] = @p1";
                                        cmd.Parameters.AddWithValue("p1", s);
                                    }
                                    else
                                    {
                                        cmd.CommandText += $" OR b.[Part_Number] = @p{_partList.IndexOf(s) + 1}";
                                        cmd.Parameters.AddWithValue($"p{_partList.IndexOf(s) + 1}", s);
                                    }
                                }
                                cmd.CommandText += ")";
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        _partList = new List<string>();
                                        while (reader.Read())
                                        {
                                            var _tempSku = new Sku
                                            {
                                                SkuNumber = reader.SafeGetString("Part")
                                                ,EngStatusDesc = _first
                                                    ? reader.SafeGetString("Parent").Replace("C", "")
                                                    : reader.SafeGetString("Parent")
                                                ,DiamondNumber = _returnList.Keys.Count(o => o.SkuNumber == reader.SafeGetString("Parent") || o.SkuNumber == reader.SafeGetString("Parent").Replace("C", "")) > 0
                                                    ? _returnList.Keys.First(o => o.SkuNumber == reader.SafeGetString("Parent") || o.SkuNumber == reader.SafeGetString("Parent").Replace("C", "")).DiamondNumber
                                                    : _parentCount.ToString()
                                                ,SkuDescription = reader.SafeGetString("Desc")
                                                ,Uom = reader.SafeGetString("Uom")
                                                ,Operation = reader.SafeGetDouble("Qpa").ToString()
                                                ,EngStatus = reader.SafeGetString("Status")
                                                ,MasterPrint = reader.SafeGetString("Master")
                                            };
                                            _counter = _returnList.First(o => o.Key.SkuNumber == _tempSku.EngStatusDesc || o.Key.SkuNumber == _tempSku.EngStatusDesc.Replace("C", "")).Value - 1;
                                            _returnList.Add(_tempSku, _counter);
                                            _partList.Add(reader.SafeGetString("Part"));
                                            _parentCount = _returnList.Keys.Count(o => o.DiamondNumber == _parentCount.ToString()) > 0 ? _parentCount + 1 : _parentCount;
                                        }
                                        _first = false;
                                    }
                                    else
                                    {
                                        _found = true;
                                    }
                                }
                            }
                        }
                        sqlCon.ChangeDatabase("WCCO_MAIN");
                    }

                    #endregion

                    return _returnList;
                }
                catch (SqlException sqlEx)
                {
                    return null;
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
