using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work order object
    /// </summary>
    public class WorkOrder : Sku
    {
        #region Properties

        public string OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string Seq { get; set; }
        public string OpDesc { get; set; }
        public string Routing { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public string TaskType { get; set; }
        public int StartQty { get; set; }
        public int CurrentQty { get; set; }
        public int ScrapQty { get; set; }
        public DateTime SchedStartDate { get; set; }
        public DateTime ActStartDate { get; set; }
        public DateTime DueDate { get; set; }
        public SalesOrder SalesOrder { get; set; }
        public string Notes { get; set; }
        public string ShopNotes { get; set; }
        public bool IsLate { get { return DueDate < DateTime.Today; } }
        public bool IsStartedLate { get { return SchedStartDate < DateTime.Today && CurrentQty == StartQty; } }
        public List<Component> Picklist { get; set; }
        public bool IsDeviated { get; set; }
        public int Shift { get; set; }
        public int Priority { get; set; }
        public new int Facility { get; set; }

        #endregion

        /// <summary>
        /// WorkOrder object default constructor
        /// </summary>
        public WorkOrder()
        { }

        /// <summary>
        /// Work Order Object constructor
        /// Will create a new WorkOrder Object based on a Work Order Number
        /// </summary>
        /// <param name="woNumber">Work Order Number</param>
        public WorkOrder(string woNumber)
        {
            var _rows = MasterDataSet.Tables["Master"].Select($"[WO_Number] = '{woNumber}'");
            if (_rows.Length > 0)
            {
                var _row = _rows.FirstOrDefault();
                OrderID = _row.Field<string>("WorkOrderID");
                OrderNumber = _row.Field<string>("WorkOrder");
                Seq = _row.Field<string>("Operation");
                Operation = _row.Field<string>("Operation");
                OpDesc = _row.Field<string>("Op_Desc");
                Routing = _row.Field<string>("Routing");
                State = _row.Field<string>("WO_Priority");
                TaskType = _row.Field<string>("WO_Type");
                StartQty = _row.Field<int>("WO_StartQty");
                CurrentQty = Convert.ToInt32(_row.Field<decimal>("WO_CurrentQty"));
                SchedStartDate = _row.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = _row.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? _row.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = _row.Field<DateTime>("WO_DueDate");
                SkuNumber = _row.Field<string>("SkuNumber");
                SkuDescription = _row.Field<string>("SkuDesc");
                Uom = _row.Field<string>("SkuUom");
                MasterPrint = _row.Field<string>("SkuMasterPrint");
                InternalRev = _row.Field<DateTime>("InternalRev") != Convert.ToDateTime("1999-01-01") ? _row.Field<DateTime>("InternalRev").ToString("yyMMdd-1") : string.Empty;
                CustomerRev = _row.Field<string>("CustomerRev");
                if (!string.IsNullOrEmpty(_row.Field<string>("WO_SalesRef")))
                {
                    var _so = _row.Field<string>("WO_SalesRef").Split('*');
                    SalesOrder = new SalesOrder
                    {
                        SalesNumber = _so[0],
                        CustomerName = _row.Field<string>("Cust_Name"),
                        CustomerNumber = _row.Field<string>("Cust_Nbr"),
                        CustomerPart = _row.Field<string>("Cust_Part_Nbr"),
                        LineNumber = Convert.ToInt32(_so[1]),
                        LineBalQuantity = _row.Field<int>("Ln_Bal_Qty"),
                        LoadPattern = _row.Field<string>("LoadPattern").ToUpper() == "PLASTIC"
                    };
                }
                else
                {
                    SalesOrder = new SalesOrder();
                }
                Machine = _row.Field<string>("MachineName");
                MachineGroup = _row.Field<string>("MachineGroup");
                IsDeviated = _row.Field<string>("Deviation") == "Y";
                Inspection = _row.Field<string>("Inspection") == "Y";
                Priority = _row.Field<int>("Sched_Priority");
                Shift = _row.Field<int>("Sched_Shift");
                Facility = _row.Field<int>("Site");
            }
        }

        /// <summary>
        /// Work Order Object constructor
        /// Will create a new WorkOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the work order</param>
        public WorkOrder(DataRow dRow)
        {
            if (dRow != null)
            {
                OrderID = dRow.Field<string>("WorkOrderID");
                OrderNumber = dRow.Field<string>("WorkOrder");
                Seq = dRow.Field<string>("Operation");
                Operation = dRow.Field<string>("Operation");
                OpDesc = dRow.Field<string>("Op_Desc");
                Routing = dRow.Field<string>("Routing");
                State = dRow.Field<string>("WO_Priority");
                TaskType = dRow.Field<string>("WO_Type");
                StartQty = dRow.Field<int>("WO_StartQty");
                CurrentQty = Convert.ToInt32(dRow.Field<decimal>("WO_CurrentQty"));
                SchedStartDate = dRow.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = dRow.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = dRow.Field<DateTime>("WO_DueDate");
                SkuNumber = dRow.Field<string>("SkuNumber");
                SkuDescription = dRow.Field<string>("SkuDesc");
                Uom = dRow.Field<string>("SkuUom");
                MasterPrint = dRow.Field<string>("SkuMasterPrint");
                InternalRev = dRow.Field<DateTime>("InternalRev") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("InternalRev").ToString("yyMMdd-1") : string.Empty;
                CustomerRev = dRow.Field<string>("CustomerRev");
                if (!string.IsNullOrEmpty(dRow.Field<string>("WO_SalesRef")))
                {
                    var _so = dRow.Field<string>("WO_SalesRef").Split('*');
                    SalesOrder = new SalesOrder
                    {
                        SalesNumber = _so[0],
                        CustomerName = dRow.Field<string>("Cust_Name"),
                        CustomerNumber = dRow.Field<string>("Cust_Nbr"),
                        CustomerPart = dRow.Field<string>("Cust_Part_Nbr"),
                        LineNumber = Convert.ToInt32(_so[1]),
                        LineBalQuantity = dRow.Field<int>("Ln_Bal_Qty"),
                        LoadPattern = dRow.Field<string>("LoadPattern").ToUpper() == "PLASTIC"
                     };
                }
                else
                {
                    SalesOrder = new SalesOrder();
                }
                Machine = dRow.Field<string>("MachineName");
                MachineGroup = dRow.Field<string>("MachineGroup");
                IsDeviated = dRow.Field<string>("Deviation") == "Y";
                Inspection = dRow.Field<string>("Inspection") == "Y";
                Priority = dRow.Field<int>("Sched_Priority");
                Shift = dRow.Field<int>("Sched_Shift");
                Facility = dRow.Field<int>("Site");
            }
        }

        #region Data Access

        /// <summary>
        /// Get work order note's table
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>All work order notes in a datatable</returns>
        public static DataTable GetNotesTable(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Notes]", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
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
        }

        #endregion

        /// <summary>
        /// Get a work order's notes
        /// </summary>
        /// <param name="type">Type of notes to return</param>
        /// <param name="includePriority">Add priority in the sorting</param>
        /// <param name="lookupValue">Value to use in searching for notes</param>
        /// <returns>A concatonation of notes into a string</returns>
        public static string GetNotes(string type, bool includePriority, params string[] lookupValue)
        {
            var _notes = string.Empty;
            var _sort = includePriority 
                ? "[Priority], [LineID] ASC" 
                : "[LineID] ASC";

            var _select = string.Empty;
            if (lookupValue.Length == 1)
            {
                _select = $"[NoteID] = '{lookupValue[0]}' AND [NoteType] = '{type}'";
            }
            else if (lookupValue.Length > 1)
            {
                _select = "(";
                foreach (var _lv in lookupValue)
                {
                    _select += _select.Length > 1 ? " OR " : "";
                    _select += $"[NoteID] = '{_lv}'";
                }
                _select += $") AND [NoteType] = '{type}'";
            }
            if (string.IsNullOrEmpty(_select))
            {
                return _notes;
            }
            foreach (DataRow _dr in MasterDataSet.Tables["WoNotes"].Select(_select, _sort))
            {
                if (!_notes.Contains($"{_dr.Field<string>("Note")}\n"))
                {
                    _notes += $"{_dr.Field<string>("Note")}\n";
                }
            }
            return _notes?.Trim('\n');
        }

        /// <summary>
        /// Checks to see if the work order is valid
        /// </summary>
        /// <param name="woNumber">Work Order number to check</param>
        /// <param name="machName">Optional: Machine Name</param>
        /// <param name="pri">Optional: Priority</param>
        /// <returns>Validation as bool; true = valid, false = invalid</returns>
        public static bool Exists(string woNumber, string machName = null, int pri = 0)
        {
            if (string.IsNullOrEmpty(machName) && pri == 0)
            {
                return MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{woNumber}'").Length > 0;
            }
            else if (string.IsNullOrEmpty(machName) && pri != 0)
            {
                return MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{woNumber}' AND [Sched_Priority] = {pri}").Length > 0;
            }
            else if (!string.IsNullOrEmpty(machName) && pri == 0)
            {
                return MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{woNumber}' AND [MachineName] = '{machName}'").Length > 0;
            }
            else
            {
                return MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{woNumber}' AND [MachineName] = '{machName}' AND [Sched_Priority] = {pri}").Length > 0;
            }
        }

        /// <summary>
        /// Get the work order priority list
        /// </summary>
        /// <param name="machineName">Machine name</param>
        /// <returns>Work order priority as a list of work order objects</returns>
        public static List<WorkOrder> GetWorkOrderPriList(string machineName)
        {
            var _tempList = new List<WorkOrder>();
            var _rows = MasterDataSet.Tables["Master"].Select($"[MachineName] = '{machineName}' AND [Sched_Priority] <> 999 AND [Status] <> 'C'");
            if (_rows.Length > 0)
            {
                foreach (var _row in _rows)
                {
                    _tempList.Add(new WorkOrder(_row));
                }
            }
            return _tempList;
        }
    }
}
