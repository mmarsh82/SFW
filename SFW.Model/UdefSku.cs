using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class UdefSku
    {
        #region Properties

        public string SpecDesc { get; set; }
        public string Core { get; set; }
        public string HeatingCans { get; set; }
        public string CoolingCans { get; set; }
        public string SpreaderBar { get; set; }
        public string Duster { get; set; }
        public string SetUpInstructions { get; set; }
        public string RollUp { get; set; }
        public string Tape { get; set; }
        public string SendTo { get; set; }
        public string PackageInstructions { get; set; }
        public List<UdefSkuPass> PassInformation { get; set; }
        public UdefSkuPass SlitInformation { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Volume { get; set; }
        public double RollDiameter { get; set; }
        public double RollWeight { get; set; }
        public double SpecGravity { get; set; }
        public double Gauge { get; set; }

        //TODO: after the retrieval of the properties for part specifications will need to inherit from Inotify to complete the auto calculations
        //determin if this needs to be a half or full intergral

        #endregion

        /// <summary>
        /// UdefSku Object Constructor
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="sqlCon">Sql onnection Object to use</param>
        public UdefSku(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var exists = 0;
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
                                                                COUNT([ID])
                                                            FROM
	                                                            [dbo].[IM-UDEF-SPEC-INIT]
                                                            WHERE
	                                                            [ID] LIKE CONCAT(@p1,'*10');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        int.TryParse(cmd.ExecuteScalar()?.ToString(), out exists);
                    }
                    if (exists > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            [Length],
                                                                [Width],
                                                                [Volume],
                                                                [Roll_Diameter],
                                                                [Total_Roll_Weight],
                                                                [Spec_Gravity],
                                                                [Gauge]
                                                            FROM
	                                                            [dbo].[IM-UDEF-SPEC-INIT]
                                                            WHERE
	                                                            [ID] LIKE CONCAT(@p1,'*10');", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", partNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        Length = reader.SafeGetDouble("Length");
                                        Width = reader.SafeGetDouble("Width");
                                        Volume = reader.SafeGetDouble("Volume");
                                        RollDiameter = reader.SafeGetDouble("Roll_Diameter");
                                        RollWeight = reader.SafeGetDouble("Total_Roll_Weight");
                                        SpecGravity = reader.SafeGetDouble("Spec_Gravtity");
                                        Gauge = reader.SafeGetDouble("Gauge");
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
        /// UdefSku Object Constructor
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="seq">Work Order Sequence</param>
        /// <param name="sqlCon">Sql onnection Object to use</param>
        public UdefSku(string partNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            [Sequence_Desc] as 'Type',
	                                                            [Core],
	                                                            [Heating_Cans],
	                                                            [Cooling_Cans],
	                                                            [Spreader_Bar],
	                                                            [Duster],
	                                                            [Rollup_In],
	                                                            [Tape_Rolls],
	                                                            [Sendto] as 'SendTo'
                                                            FROM
	                                                            [dbo].[IM-UDEF-SPEC-INIT]
                                                            WHERE
	                                                            [ID] LIKE CONCAT(@p1,'*', @p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    SpecDesc = reader.SafeGetString("Type");
                                    Core = reader.SafeGetString("Core");
                                    Core = string.IsNullOrEmpty(Core) || Core == "NA" ? string.Empty : Core;
                                    HeatingCans = reader.SafeGetString("Heating_Cans").Replace(".", "").ToUpper();
                                    HeatingCans = string.IsNullOrEmpty(HeatingCans) || HeatingCans == "NA" ? "NO" : HeatingCans;
                                    CoolingCans = reader.SafeGetString("Cooling_Cans").Replace(".", "").ToUpper();
                                    CoolingCans = string.IsNullOrEmpty(CoolingCans) || CoolingCans == "NA" ? "NO" : CoolingCans;
                                    SpreaderBar = reader.SafeGetString("Spreader_Bar").Replace(".", "").ToUpper();
                                    SpreaderBar = string.IsNullOrEmpty(SpreaderBar) || SpreaderBar == "NA" ? "NO" : SpreaderBar;
                                    Duster = reader.SafeGetString("Duster").Replace(".", "").ToUpper();
                                    Duster = string.IsNullOrEmpty(Duster) || Duster == "NA" ? "NO" : Duster;
                                    RollUp = reader.SafeGetString("Rollup_In").ToUpper();
                                    Tape = reader.SafeGetString("Tape_Rolls").ToUpper();
                                    SendTo = reader.SafeGetString("SendTo").ToUpper();
                                }
                            }
                        }
                    }
                    SetUpInstructions = GetSetUpInstructions(partNbr, seq, sqlCon);
                    PackageInstructions = GetPackInstructions(partNbr, seq, sqlCon);
                    if (SpecDesc.Contains("SLIT"))
                    {
                        SlitInformation = new UdefSkuPass(partNbr, seq, sqlCon);
                        PassInformation = null;
                    }
                    else
                    {
                        PassInformation = UdefSkuPass.GetUdefPassList(partNbr, seq, sqlCon);
                        SlitInformation = null;
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
        /// Get the setup instructions for a work order
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="seq">Work Order Sequence</param>
        /// <param name="sqlCon">Sql connection object to use</param>
        /// <returns></returns>
        public static string GetSetUpInstructions(string partNbr, string seq, SqlConnection sqlCon)
        {
            var _temp = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT [Instructions] FROM [dbo].[IM-UDEF-SPEC-INIT_Instructions] WHERE [ID] LIKE CONCAT(@p1,'*',@p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp += $"{reader.SafeGetString("Instructions").Replace("amp;","")}\n";
                                }
                            }
                        }
                    }
                    return _temp;
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
        /// Get packaging instructions for a work order
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="seq">Work Order Sequence</param>
        /// <param name="sqlCon">Sql Connection object to use</param>
        /// <returns></returns>
        public static string GetPackInstructions(string partNbr, string seq, SqlConnection sqlCon)
        {
            var _temp = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT [Package_Instr] FROM [dbo].[IM-UDEF-SPEC-INIT_Package_Instr] WHERE [ID] LIKE CONCAT(@p1,'*',@p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp += $"{reader.SafeGetString("Package_Instr")}\n";
                                }
                            }
                        }
                    }
                    return _temp;
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
