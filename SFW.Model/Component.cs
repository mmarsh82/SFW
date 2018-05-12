using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class Component
    {
        #region Properties

        public string CompNumber { get; set; }
        public string CompDescription { get; set; }
        public int CurrentOnHand { get; set; }
        public int RequiredQty { get; set; }
        public double AssemblyQty { get; set; }
        public int IssuedQty { get; set; }
        public string CompMasterPrint { get; set; }
        public string CompUom { get; set; }
        public List<Lot> LotList { get; set; }

        #endregion

        /// <summary>
        /// Component Default Constructor 
        /// </summary>
        public Component()
        { }

        public static List<Component> GetComponentList(string woNbr, int balQty, SqlConnection sqlCon)
        {
            var _tempList = new List<Component>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT
	                                                            SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID])) as 'Component', a.[Qty_Per_Assy] as 'Qty Per', a.[Qty_Reqd] as 'Req Qty',
	                                                            b.[Qty_On_Hand] as 'On Hand',
	                                                            c.[Description], c.[Drawing_Nbrs], c.[Um]
                                                            FROM
	                                                            [dbo].[PL-INIT] a
                                                            RIGHT JOIN
	                                                            [dbo].[IPL-INIT] b ON b.[Part_Nbr] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                            RIGHT JOIN
	                                                            [dbo].[IM-INIT] c ON c.[Part_Number] = SUBSTRING(a.[ID], CHARINDEX('*', a.[ID], 0) + 1, LEN(a.[ID]))
                                                            WHERE
	                                                            a.[ID] LIKE CONCAT(@p1, '%');", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _id = reader.IsDBNull(0) ? null : reader.GetString(0).Split('*');
                                    _tempList.Add(new Component
                                    {
                                        CompNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                        AssemblyQty = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader.GetDecimal(1)),
                                        RequiredQty = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2)),
                                        CurrentOnHand = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3)),
                                        CompDescription = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                        IssuedQty = reader.IsDBNull(1) ? 0 : balQty * Convert.ToInt32(reader.GetValue(1)),
                                        CompMasterPrint = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                        CompUom = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                        LotList = reader.IsDBNull(0) ? new List<Lot>() : Lot.GetOnHandLotList(reader.GetString(0), sqlCon)
                                    });
                                }
                            }
                        }
                    }
                    return _tempList;
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
