using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Sales
{
    public class SalesOrder : ModelBase, IModuleData
    {
        #region Properties

        public string SalesNumber { get; set; }
        public string PartNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string FullCustomerName { get; set; }
        public string CustomerPart { get; set; }
        public int LineNumber { get; set; }
        public int LineTotalNumber { get; set; }
        public int LineBaseQuantity { get; set; }
        public int LineBalQuantity { get; set; }
        public string LineDesc { get; set; }
        public string LineNotes { get; set; }
        public string LoadPattern { get; set; }
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
        public bool CanShip { get; set; }
        public int Facility { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM [dbo].[SFW_SalesSchedule] ORDER BY [ShipDate], [ID] ASC", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable.AsEnumerable()
                                .GroupBy(r => r.Field<string>("ID"))
                                .Select(g => g.First())
                                .CopyToDataTable();
                        }
                    }
                    catch (SqlException)
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

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SalesOrder()
        { }

        /// <summary>
        /// Sales Order Object constructor
        /// Will create a new SalesOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the sales order</param>
        public SalesOrder(DataRow dRow)
        {
            SalesNumber = dRow.Field<string>("SoNbr");
            PartNumber = dRow.Field<string>("PartNbr");
            CustomerNumber = dRow.Field<string>("CustNbr");
            CustomerName = dRow.Field<string>("CustName");
            FullCustomerName = dRow.Field<string>("FullCustName");
            CustomerPart = dRow.Field<string>("CustPartNbr");
            LineNumber = dRow.Field<int>("LineNbr");
            LineTotalNumber = GetLineCount(SalesNumber, LineNumber);
            LineBalQuantity = dRow.Field<int>("BalQty");
            LineBaseQuantity = dRow.Field<int>("BaseQty");
            LineDesc = dRow.Field<string>("Description");
            LoadPattern = dRow.Field<string>("LoadPattern").ToUpper();
            InternalComments = SalesOrderNote.GetNote(SalesNumber, 'C');
            SpecialInstructions = SalesOrderNote.GetNote(SalesNumber, 'I');
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

        /// <summary>
        /// Sales Order Object constructor
        /// Will create a new SalesOrder Object based on sales order ID as string
        /// </summary>
        /// <param name="salesOrder">Sales order ID as string</param>
        public SalesOrder(string soID)
        {
            var _rows = MasterDataSet.Tables[new SalesOrder().GetType().Name].Select($"[ID] = '{soID}'");
            if (_rows.Length > 0)
            {
                var _row = _rows.FirstOrDefault();
                SalesNumber = _row.Field<string>("SoNbr");
                PartNumber = _row.Field<string>("PartNbr");
                CustomerNumber = _row.Field<string>("CustNbr");
                CustomerName = _row.Field<string>("CustName");
                FullCustomerName = _row.Field<string>("FullCustName");
                CustomerPart = _row.Field<string>("CustPartNbr");
                LineNumber = _row.Field<int>("LineNbr");
                LineTotalNumber = GetLineCount(SalesNumber, LineNumber);
                LineBalQuantity = _row.Field<int>("BalQty");
                LineBaseQuantity = _row.Field<int>("BaseQty");
                LineDesc = _row.Field<string>("Description");
                LoadPattern = _row.Field<string>("LoadPattern").ToUpper();
                InternalComments = SalesOrderNote.GetNote(SalesNumber, 'C');
                SpecialInstructions = SalesOrderNote.GetNote(SalesNumber, 'I');
                IsExpedited = _row.Field<int>("IsExpedited") > 0;
                ShipName = _row.Field<string>("ShipName");
                ShipAddress = new string[2];
                ShipAddress[0] = _row.Field<string>("ShipAddr1");
                ShipAddress[1] = _row.Field<string>("ShipAddr2");
                ShipCity = _row.Field<string>("ShipCity");
                ShipState = _row.Field<string>("ShipState");
                ShipZip = _row.Field<string>("ShipZip");
                ShipCountry = _row.Field<string>("ShipCountry");
                CommitDate = _row.Field<DateTime>("ShipDate");
                RequestDate = _row.Field<DateTime>("ReqDate");
                DeliveryDate = _row.Field<DateTime>("DelDate");
                CreditStatus = _row.Field<string>("CredStatus");
                CreditApprover = _row.Field<string>("CredApprover");
                CreditDate = _row.Field<DateTime>("CredDate") == new DateTime(1999, 01, 01) ? DateTime.MinValue : _row.Field<DateTime>("CredDate");
                CreditLimit = _row.Field<int>("AR_Limit");
                CreditBalance = _row.Field<decimal>("AR_Bal");
                CreditShippedBalance = _row.Field<decimal>("AR_SBal");
                CreditAllocatedBalance = _row.Field<decimal>("AR_ABal");
                CurrentCreditLimit = _row.Field<decimal>("AR_Credit");
                OrderBalance = _row.Field<decimal>("AR_OrdBal");
                Facility = _row.Field<int>("Site");
            }
        }

        /// <summary>
        /// Get the sales order line count
        /// </summary>
        /// <param name="soNumber">Sales Order to get the line count</param>
        /// <param name="lineNumber">Line number as a reference</param>
        /// <returns>Line count as a int</returns>
        public static int GetLineCount(string soNumber, int lineNumber)
        {
            var _rtnVal = MasterDataSet.Tables[new SalesOrder().GetType().Name].Select($"[SoNbr] = '{soNumber}'").Count();
            return _rtnVal >= lineNumber ? _rtnVal : lineNumber;
        }

        /// <summary>
        /// Get a list of the differnt types of sales orders
        /// </summary>
        /// <returns>list of sales order types as IList<string></returns>
        public static IList<string> GetOrderTypeList()
        {
            var _rtnList = new List<string>();
            if (MasterDataSet.Tables[new SalesOrder().GetType().Name].Columns.Contains("Type"))
            {
                foreach (DataRow _row in MasterDataSet.Tables[new SalesOrder().GetType().Name].DefaultView.ToTable(true, "Type").Rows)
                {
                    if (!string.IsNullOrEmpty(_row.Field<string>("Type")))
                    {
                        _rtnList.Add(_row.Field<string>("Type"));
                    }
                }
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
            var _rows = MasterDataSet.Tables[new SalesOrder().GetType().Name].Select($"[SoNbr] = '{soNbr}'");
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
                                        ,CanShip = _row.Field<int>("HasStock") == 1
                                    });
                }
            }
            return (from c in _rtnList
                   orderby c.LineNumber
                   select c).ToList();
        }
    }
}
