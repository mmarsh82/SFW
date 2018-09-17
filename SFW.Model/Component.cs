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
        public List<Lot> DedicatedLotList { get; set; }

        #endregion

        /// <summary>
        /// Component Default Constructor 
        /// </summary>
        public Component()
        { }

        /// <summary>
        /// Retrieve a list of components for a work order
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="balQty">Balance quantity left on the work order</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns></returns>
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
                                    _tempList.Add(new Component
                                    {
                                        CompNumber = reader.SafeGetString("Component"),
                                        AssemblyQty = reader.SafeGetDouble("Qty Per"),
                                        RequiredQty = reader.SafeGetInt32("Req Qty"),
                                        CurrentOnHand = reader.SafeGetInt32("On Hand"),
                                        CompDescription = reader.SafeGetString("Description"),
                                        IssuedQty = Convert.ToInt32(Math.Round(reader.SafeGetDouble("Qty Per") * balQty,0,MidpointRounding.AwayFromZero)),
                                        CompMasterPrint = reader.SafeGetString("Drawing_Nbrs"),
                                        CompUom = reader.SafeGetString("Um"),
                                        LotList = reader.IsDBNull(0) ? new List<Lot>() : Lot.GetOnHandLotList(reader.SafeGetString("Component"), sqlCon),
                                        DedicatedLotList = reader.IsDBNull(0) ? new List<Lot>() : Lot.GetDedicatedLotList(reader.SafeGetString("Component"), woNbr, sqlCon)
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
