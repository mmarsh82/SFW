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

        #endregion

        /// <summary>
        /// UdefSku Object Constructor
        /// </summary>
        public UdefSku(string partNbr, SqlConnection sqlCon)
        {
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            a.[Sequence_Desc] as 'Type',
	                                                            a.[Core],
	                                                            a.[Heating_Cans],
	                                                            a.[Cooling_Cans],
	                                                            a.[Spreader_Bar],
	                                                            a.[Duster],
	                                                            a.[Rollup_In],
	                                                            a.[Tape_Rolls],
	                                                            a.[Sendto] as 'SendTo'
                                                            FROM
	                                                            [dbo].[IM-UDEF-SPEC-INIT] a
                                                            WHERE
	                                                            a.[ID] LIKE CONCAT(@p1,'%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    SpecDesc = reader.SafeGetString("Type");
                                    Core = reader.SafeGetString("Core");
                                    HeatingCans = reader.SafeGetString("Heating_Cans").Replace(".", "").ToUpper(); ;
                                    CoolingCans = reader.SafeGetString("Cooling_Cans").Replace(".", "").ToUpper(); ;
                                    SpreaderBar = reader.SafeGetString("Spreader_Bar").Replace(".", "").ToUpper(); ;
                                    Duster = reader.SafeGetString("Duster").Replace(".", "").ToUpper(); ;
                                    RollUp = reader.SafeGetString("Rollup_In").ToUpper();
                                    Tape = reader.SafeGetString("Tape_Rolls").ToUpper();
                                    SendTo = reader.SafeGetString("SendTo").ToUpper();
                                }
                            }
                        }
                    }
                    SetUpInstructions = GetSetUpInstructions(partNbr, sqlCon);
                    PackageInstructions = GetPackInstructions(partNbr, sqlCon);
                    PassInformation = UdefSkuPass.GetUdefPassList(partNbr, sqlCon);
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
        /// <param name="sqlCon">Sql connection object to use</param>
        /// <returns></returns>
        public static string GetSetUpInstructions(string partNbr, SqlConnection sqlCon)
        {
            var _temp = string.Empty;
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT [Instructions] FROM [dbo].[IM-UDEF-SPEC-INIT_Instructions] WHERE [ID] LIKE CONCAT(@p1, '%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
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
        /// <param name="sqlCon">Sql Connection object to use</param>
        /// <returns></returns>
        public static string GetPackInstructions(string partNbr, SqlConnection sqlCon)
        {
            var _temp = string.Empty;
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT [Package_Instr] FROM [dbo].[IM-UDEF-SPEC-INIT_Package_Instr] WHERE [ID] LIKE CONCAT(@p1, '%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
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
