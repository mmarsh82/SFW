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

        public string OrderNumber { get; set; }
        public string Seq { get; set; }
        public string OpDesc { get; set; }
        public string Priority { get; set; }
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
                var _wo = _row.Field<string>("WO_Number").Split('*');
                OrderNumber = _wo[0];
                Seq = _wo[1];
                Operation = _row.Field<string>("Operation");
                OpDesc = _row.Field<string>("Op_Desc");
                Priority = _row.Field<string>("WO_Priority");
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
                TotalOnHand = _row.Field<int>("SkuOnHand");
                BomRevDate = _row.Field<DateTime>("BomRevDate") != Convert.ToDateTime("1999-01-01") ? _row.Field<DateTime>("BomRevDate") : DateTime.MinValue;
                BomRevLevel = _row.Field<string>("BomRevLvl");
                EngStatus = _row.Field<string>("EngStatus");
                EngStatusDesc = _row.Field<string>("EngStatusDesc");
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
                var _wo = dRow.Field<string>("WO_Number").Split('*');
                OrderNumber = _wo[0];
                Seq = _wo[1];
                Operation = dRow.Field<string>("Operation");
                OpDesc = dRow.Field<string>("Op_Desc");
                Priority = dRow.Field<string>("WO_Priority");
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
                TotalOnHand = dRow.Field<int>("SkuOnHand");
                BomRevDate = dRow.Field<DateTime>("BomRevDate") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("BomRevDate") : DateTime.MinValue;
                BomRevLevel = dRow.Field<string>("BomRevLvl");
                EngStatus = dRow.Field<string>("EngStatus");
                EngStatusDesc = dRow.Field<string>("EngStatusDesc");
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
                _notes += $"{_dr.Field<string>("Note")}\n";
            }
            return _notes?.Trim('\n');
        }
    }
}
