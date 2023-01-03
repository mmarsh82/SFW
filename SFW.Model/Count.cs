using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class Count
    {
        #region Properties

        public string CountID { get; set; }
        public string CountNumber { get; set; }
        public string PartNumber { get; set; }
        public string PartDesc { get; set; }
        public string Uom { get; set; }
        public string CountLoc { get; set; }
        public string LotNumber { get; set; }
        public int CountQty { get; set; }

        #endregion

        public Count()
        { }

        /// <summary>
        /// Count Object overloaded Constructor
        /// </summary>
        /// <param name="drow">Data Row to parse into a Count Object</param>
        public Count(DataRow drow)
        {
            if (drow != null)
            {
                CountID = drow.Field<string>("CountID");
                CountNumber = drow.Field<string>("CountNumber");
                PartNumber = drow.Field<string>("PartNumber");
                PartDesc = drow.Field<string>("PartDesc");
                Uom = drow.Field<string>("Uom");
                CountLoc = drow.Field<string>("CountLoc");
                CountQty = drow.SafeGetField<int>("CountQty");
                LotNumber = drow.Field<string>("LotNumber");
            }
        }

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon)
        {
            var _selectCmd = $@"USE {sqlCon.Database};
                            SELECT
	                            CONCAT(CONCAT(a.[ID], '*'), b.[ID2]) as 'CountID'
                                ,a.[ID] as 'CountNumber'
	                            ,a.[Part_Nbr] as 'PartNumber'
	                            ,c.[Description] as 'PartDesc'
	                            ,a.[Part_Nbr_Uom] as 'Uom'
	                            ,b.[Location] as 'CountLoc'
	                            ,SUBSTRING(b.[Lot_Nbrs], 0, LEN(b.[Lot_Nbrs]) - 1) as 'LotNumber'
	                            ,CAST(b.[Oh] as int) as 'CountQty'
                            FROM
	                            [dbo].[CYCLE_COUNT-INIT] a
                            RIGHT JOIN
	                            [dbo].[CYCLE_COUNT-INIT_Count_Data] b ON a.[ID] = b.[ID1]
                            LEFT JOIN
	                            [dbo].[IM-INIT] c ON c.[Part_Number] = a.[Part_Nbr]
                            WHERE
	                            (b.[Count_Complete] IS NULL OR b.[Count_Complete] = 'N') AND b.[Qty_Counted] IS NOT NULL
                            ORDER BY
                                b.[Location];";

            using (var _tempTable = new DataTable())
            {
                while (sqlCon.State == ConnectionState.Connecting) { }
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(_selectCmd, sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        sqlCon.Close();
                        throw new Exception(sqlEx.Message);
                    }
                    catch (Exception ex)
                    {
                        sqlCon.Close();
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    sqlCon.Close();
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        #endregion
    }
}
