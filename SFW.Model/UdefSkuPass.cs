using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class UdefSkuPass
    {
        #region Properties

        public string Temperature { get; set; }
        public string GumWall { get; set; }
        public string OAG { get; set; }
        public int LineSpeed { get; set; }
        public string AtTable { get; set; }
        public double Volume { get; set; }
        public double PoundPerFoot { get; set; }
        public string Pass { get; set; }
        public string TopTemp { get; set; }
        public string CenterTemp { get; set; }
        public string BottomTemp { get; set; }
        public string Instructions { get; set; }

        #endregion

        public UdefSkuPass()
        { }

        public static List<UdefSkuPass> GetUdefPassList(string partNbr, SqlConnection sqlCon)
        {
            var _temp = new List<UdefSkuPass>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
	                                                            a.[Ps_Setup_Temp] as 'Temperature',
	                                                            a.[Ps_Gum_Wall] as 'GumWall',
	                                                            a.[Ps_Oag] as 'OAG',
	                                                            a.[Ps_Line_Speed] as 'LineSpeed',
	                                                            a.[Ps_At_Table] as 'AtTable',
	                                                            a.[Ps_Volume] as 'Volume',
	                                                            a.[Ps_Lb_SQ_Ft] as 'PoundsToFeet',
	                                                            CONCAT('PASS ',a.[Ps_Pass]) as 'Pass',
	                                                            a.[Ps_Setup_Top] as 'Top',
	                                                            a.[Ps_Setup_Center] as 'Center',
	                                                            a.[Ps_Setup_Bottom] as 'Bottom',
	                                                            a.[Ps_Pass_Instructions] as 'Instructions'
                                                            FROM
	                                                            [dbo].[IM-UDEF-SPEC-INIT_Passes] a
                                                            WHERE
	                                                            a.[ID1] LIKE CONCAT(@p1, '%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _temp.Add(new UdefSkuPass
                                    {
                                        Temperature = reader.SafeGetString("Temperature"),
                                        GumWall = reader.SafeGetString("GumWall"),
                                        OAG = reader.SafeGetString("OAG"),
                                        LineSpeed = reader.SafeGetInt32("LineSpeed"),
                                        AtTable = reader.SafeGetString("AtTable"),
                                        Volume = reader.SafeGetDouble("Volume"),
                                        PoundPerFoot = reader.SafeGetDouble("PoundsToFeet"),
                                        Pass = reader.SafeGetString("Pass"),
                                        TopTemp = reader.SafeGetString("Top"),
                                        CenterTemp = reader.SafeGetString("Center"),
                                        BottomTemp = reader.SafeGetString("Bottom"),
                                        Instructions = reader.SafeGetString("Instructions")
                                    });
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
