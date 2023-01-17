using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 5-4-18

namespace SFW.Model
{
    public class SalesOrder : ModelBase
    {
        #region Properties

        public string SalesNumber { get; set; }
        public string PartNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPart { get; set; }
        public int LineNumber { get; set; }
        public int LineTotalNumber { get; set; }
        public int LineBaseQuantity { get; set; }
        public int LineBalQuantity { get; set; }
        public string LineDesc { get; set; }
        public string LineNotes { get; set; }
        public bool LoadPattern { get; set; }
        public string InternalComments { get; set; }
        public string SpecialInstructions { get; set; }
        public bool IsExpedited { get; set; }
        public string ShipName { get; set; }
        public string[] ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipState { get; set; }
        public string ShipZip { get; set; }
        public string ShipCountry { get; set; }
        public bool IsBackOrder { get { return LineBalQuantity < LineBaseQuantity; } }
        public DateTime CommitDate { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool IsLate { get { return CommitDate > DateTime.Today; } }
        public IList<SalesOrder> LineList { get; set; }
        public string CreditStatus { get; set; }
        public string CreditApprover { get; set; }
        public DateTime CreditDate { get; set; }
        public int CreditLimit { get; set; }
        public decimal CreditBalance { get; set; }
        public decimal CreditShippedBalance { get; set; }
        public decimal CreditAllocatedBalance { get; set; }
        public decimal CurrentCreditLimit { get; set; }
        public decimal OrderBalance { get; set; }
        public int Facility { get; set; }

        #endregion

        /// <summary>
        /// SalesOrder Object Default Constructor
        /// </summary>
        public SalesOrder()
        { }

        /// <summary>
        /// Sales Order Object constructor
        /// Will create a new SalesOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the sales order</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public SalesOrder(DataRow dRow)
        {
            SalesNumber = dRow.Field<string>("SoNbr");
            PartNumber = dRow.Field<string>("PartNbr");
            CustomerNumber = dRow.Field<string>("CustNbr");
            CustomerName = dRow.Field<string>("CustName");
            CustomerPart = dRow.Field<string>("CustPartNbr");
            LineNumber = dRow.Field<int>("LineNbr");
            LineTotalNumber = dRow.Field<int>("LineCount");
            LineBalQuantity = dRow.Field<int>("BalQty");
            LineBaseQuantity = dRow.Field<int>("BaseQty");
            LineDesc = dRow.Field<string>("Description");
            LoadPattern = dRow.Field<string>("LoadPattern").ToUpper() == "PLASTIC";
            InternalComments = GetNotes(SalesNumber, 'C');
            SpecialInstructions = GetNotes(SalesNumber, 'I');
            IsExpedited = dRow.Field<int>("IsExpedited") > 0;
            ShipName = dRow.Field<string>("ShipName");
            ShipAddress = new string[2];
            ShipAddress[0] = dRow.Field<string>("ShipAddr1");
            ShipAddress[1] = dRow.Field<string>("ShipAddr2");
            ShipCity = dRow.Field<string>("ShipCity");
            ShipState = dRow.Field<string>("ShipState");
            ShipZip = dRow.Field<string>("ShipZip");
            ShipCountry = dRow.Field<string>("ShipCountry");
            CommitDate = dRow.Field<DateTime>("ShipDate");
            RequestDate = dRow.Field<DateTime>("ReqDate");
            DeliveryDate = dRow.Field<DateTime>("DelDate");
            CreditStatus = dRow.Field<string>("CredStatus");
            CreditApprover = dRow.Field<string>("CredApprover");
            CreditDate = dRow.Field<DateTime>("CredDate") == new DateTime(1999, 01, 01) ? DateTime.MinValue : dRow.Field<DateTime>("CredDate");
            CreditLimit = dRow.Field<int>("AR_Limit");
            CreditBalance = dRow.Field<decimal>("AR_Bal");
            CreditShippedBalance = dRow.Field<decimal>("AR_SBal");
            CreditAllocatedBalance = dRow.Field<decimal>("AR_ABal");
            CurrentCreditLimit = dRow.Field<decimal>("AR_Credit");
            OrderBalance = dRow.Field<decimal>("AR_OrdBal");
            Facility = dRow.Field<int>("Site");
        }

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_SalesSchedule] ORDER BY a.[Commit_Ship_Date], b.[ID] ASC", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable.AsEnumerable()
                                .GroupBy(r => r.Field<string>("ID"))
                                .Select(g => g.First())
                                .CopyToDataTable();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        return _tempTable;
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
        /// Get the sales order internal comments table
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static DataTable GetNotesTable(SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_SalesNotes]", sqlCon))
                        {
                            adapter.Fill(dt);
                        }
                        return dt;
                    }
                }
                catch (SqlException sqlEx)
                {
                    return new DataTable();
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

        /// <summary>
        /// Get the sales order internal comments
        /// </summary>
        /// <param name="soNumber">Sales Order to get the internal comments from</param>
        /// <param name="type">Type of note I = Special Instructions, C = Internal Comments</param>
        /// <returns>Internal comments in a string</returns>
        public static string GetNotes(string soNumber, char type)
        {
            var _note = string.Empty;
            var _rows = MasterDataSet.Tables["SoNotes"].Select($"[SalesID] = '{soNumber}' AND [Type] = '{type}'");
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _note += $"{_row.Field<string>("Comments")}\n";
                }
                return string.IsNullOrEmpty(_note) ? null : _note?.Trim('\n');
            }
            return null;
        }

        /// <summary>
        /// Get a list of the differnt types of sales orders
        /// </summary>
        /// <returns>list of sales order types as IList<string></returns>
        public static IList<string> GetOrderTypeList()
        {
            var _rtnList = new List<string>();
            foreach (DataRow _row in MasterDataSet.Tables["SalesMaster"].DefaultView.ToTable(true, "Type").Rows)
            {
                _rtnList.Add(_row.Field<string>("Type"));
            }
            return _rtnList;
        }

        /// <summary>
        /// Get a list of all the line items on a sales order
        /// </summary>
        /// <param name="soNbr">Sales order number to search</param>
        /// <param name="lineNbr">Optional: Any line number not to include in the list</param>
        /// <returns>list of sales order line items as IList<string></returns>
        public static IList<SalesOrder> GetLineList(string soNbr)
        {
            var _rtnList = new List<SalesOrder>();
            var _rows = MasterDataSet.Tables["SalesMaster"].Select($"[SoNbr] = '{soNbr}'");
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _rtnList.Add(new SalesOrder
                                    {
                                        LineBalQuantity = _row.Field<int>("BalQty")
                                        ,LineDesc = _row.Field<string>("Description")
                                        ,LineNumber = _row.Field<int>("LineNbr")
                                        ,PartNumber = _row.Field<string>("PartNbr")
                                        ,LineBaseQuantity = _row.Field<int>("BaseQty")
                                        ,LineNotes = _row.Field<string>("Uom")
                                    });
                }
            }
            return (from c in _rtnList
                   orderby c.LineNumber
                   select c).ToList();
            /*if (!string.IsNullOrEmpty(soNbr))
            {
                var _condString = string.Empty;
                foreach (int i in lineNbr)
                {
                    _condString += i != 0 ? $" AND a.[ID] != CONCAT('{soNbr}', '*', '{i}')" : "";
                }
                var _rtnList = new List<SalesOrder>();
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand _cmd = new SqlCommand(@"SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID],0) + 1, LEN(a.[ID])) as 'LineNbr'
	                                                                ,a.[Part_Wo_Gl] as 'PartNbr'
	                                                                ,(SELECT aa.[Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = a.[Part_Wo_Gl]) as 'PartDesc'
	                                                                ,a.[Ln_Bal_Qty] as 'BalQty'
	                                                                ,a.[Ln_Del_Qty] as 'BaseQty'
	                                                                ,a.[Um_Base] as 'Uom'
                                                                    ,CASE WHEN CAST(ISNULL((SELECT SUM(aa.[Oh_Qty_By_Loc]) FROM [dbo].[IPL-INIT_Location_Data] aa WHERE aa.[ID1] = a.[Part_Wo_Gl] 
		                                                                AND aa.[Loc_Pick_Avail_Flag] = 'Y'), 0) as int) >= CAST(a.[Ln_Bal_Qty] as int)
		                                                                THEN 0
		                                                                ELSE 1
	                                                                END as 'IsBackOrder'
                                                                FROM [dbo].[SOD-INIT] a
                                                                WHERE a.[ID] LIKE CONCAT(@p1, '*%')", sqlCon))
                        {
                            if (!string.IsNullOrEmpty(_condString))
                            {
                                _cmd.CommandText += _condString;
                            }
                            _cmd.CommandText += " ORDER BY a.[ID] ASC";
                            _cmd.Parameters.AddWithValue("p1", soNbr);
                            using (SqlDataReader reader = _cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _rtnList.Add(new SalesOrder
                                    {
                                        LineBalQuantity = reader.SafeGetInt32("BalQty")
                                        ,LineDesc = reader.SafeGetString("PartDesc")
                                        ,LineNumber = reader.SafeGetInt32("LineNbr")
                                        ,PartNumber = reader.SafeGetString("PartNbr")
                                        ,LineBaseQuantity = reader.SafeGetInt32("BaseQty")
                                        ,LineNotes = reader.SafeGetString("Uom")
                                        ,IsStagged = reader.SafeGetInt32("IsBackOrder") == 1
                                    });
                                }
                            }
                        }
                        return _rtnList;
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
            return null;*/
        }
    }
}
