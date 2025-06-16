using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.InventoryControl
{
    public class CountReceipt : ModelBase, IModuleData
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

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="site">Not used here</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_CycleCount] ORDER BY [CountLoc]", sqlCon))
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

        #endregion

        public CountReceipt()
        { }

        /// <summary>
        /// Count Object overloaded Constructor
        /// </summary>
        /// <param name="drow">Data Row to parse into a Count Object</param>
        public CountReceipt(DataRow drow)
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
    }
}
