using SFW.Model.Product;
using SFW.Model.SupplyChain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Production
{
    public class WorkOrder : ModelBase, IModuleData
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
        public Sales.SalesOrder SalesOrder { get; set; }
        public string Notes { get; set; }
        public string ShopNotes { get; set; }
        public bool IsLate { get { return DueDate < DateTime.Today; } }
        public bool IsStartedLate { get { return SchedStartDate < DateTime.Today && CurrentQty == StartQty; } }
        public List<PickComponent> PickList { get; set; }
        public List<BillComponent> BillList { get; set; }
        public List<Tool> ToolList { get; set; }
        public bool IsDeviated { get; set; }
        public int Shift { get; set; }
        public int Priority { get; set; }
        public int Facility { get; set; }
        public bool IsStarted { get; set; }
        public DateTime OriginStartDate { get; set; }
        public DateTime OriginDueDate { get; set; }
        public Sku Product { get; set; }
        public Machine WorkCenter { get; set; }
        public bool InQue { get; set; }

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
            var _conString = @"SELECT
	wc.Wc_Nbr AS MachineNumber
	,wc.Name AS MachineName
	,wc.D_esc AS MachineDesc
	,wc.Work_Ctr_Group AS MachineGroup
	,0 AS MachineOrder
	,wpo.ID AS WorkOrderID
	,wp.[Wp_Nbr] AS WorkOrder
	,CASE WHEN SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID])) <> '10' AND wpo.[Next_Seq] IS NULL AND wpo.[Prev_Seq] IS NULL
		THEN '10'
		ELSE SUBSTRING(wpo.[ID], CHARINDEX('*', wpo.[ID], 0) + 1, LEN(wpo.[ID]))
	END AS Operation
	,SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID)) AS Routing
	,CAST(CASE WHEN (SELECT COUNT(wpci.[ID1]) FROM [dbo].[WP-INIT_Comp_Info] wpci WHERE wpci.[ID1] = wp.[Wp_Nbr]) = 0
		THEN wp.[Qty_To_Start]
		ELSE wp.[Qty_To_Start] - (SELECT SUM(CAST(wpci.[Qty_Comp] as int)) FROM [dbo].[WP-INIT_Comp_Info] wpci WHERE wpci.[ID1] = wp.[Wp_Nbr]) END AS int) AS WO_CurrentQty
	,ISNULL(wpo.Date_Start, '1999-01-01') AS WO_SchedStartDate
	,ISNULL(wpo.Date_Act_Start, '1999-01-01') AS WO_ActStartDate
	,ISNULL(wpo.Due_Date, ISNULL(wpo.Date_Start, '1999-01-01')) AS WO_DueDate
	,ISNULL(wp.[Date_Orig_Comp], '1999-01-01') as OriginalDueDate
	,ISNULL(wp.[Orig_Start_Date], '1999-01-01') as OriginalStartDate
	,CAST(CASE WHEN (SELECT COUNT(wpci.[ID1]) FROM [dbo].[WP-INIT_Comp_Info] wpci WHERE wpci.[ID1] = wp.[Wp_Nbr]) = 0
		THEN wp.[Qty_To_Start] / (60 / (SELECT rt.[Plan_Run_Mach_Time] FROM [dbo].[RT-INIT] rt WHERE rt.[ID] = CONCAT(im.Part_Number,'*', SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID)))))
		ELSE (wp.[Qty_To_Start] - (SELECT SUM(CAST(wpci.[Qty_Comp] as int)) FROM [dbo].[WP-INIT_Comp_Info] wpci WHERE wpci.[ID1] = wp.[Wp_Nbr])) / (60 / (SELECT rt.[Plan_Run_Mach_Time] FROM [dbo].[RT-INIT] rt WHERE rt.[ID] = CONCAT(im.Part_Number,'*', SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID))))) END as decimal(10,2)) AS 'RunTime'
	,CASE WHEN CAST(wpo.[Due_Date] as date) < CAST(GETDATE() as date)
		THEN 1
		ELSE 0
	END AS IsLate
	,CASE WHEN CAST(wpo.[Date_Start] as date) < CAST(GETDATE() as date) AND wp.[Qty_To_Start] = wpo.[Qty_Avail]
		THEN 1
		ELSE 0
	END AS IsStartLate
	,ISNULL(wp.Wo_Type, 'S') AS WO_Type
	,wp.Qty_To_Start AS WO_StartQty
	,SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - LEN(CHARINDEX('*',REVERSE(wp.[So_Reference])))) AS WO_SalesRef
	,CAST(ISNULL(wp.[User_Def_10], '999') as int) AS Sched_Shift
	,CAST(ISNULL(wp.[User_Def_9], '999') as int) AS Sched_Priority
	,ISNULL(wp.Bom_Rev_Date, '1999-01-01') AS InternalRev
	,ISNULL(wp.Bom_Rev_Level, '') AS CustomerRev
	,wp.Status_Flag AS Status
	,ISNULL(wp.Fa_Dept, 'N') AS Deviation
	,SUBSTRING(im.[Part_Number], 0, CHARINDEX('|', im.[Part_Number], 0)) AS SkuNumber
	,im.Description AS SkuDesc
	,im.Um AS SkuUom
	,im.Drawing_Nbrs AS SkuMasterPrint
	,ISNULL(CASE WHEN (SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) = 'DS1'
			OR (SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) = 'DS3'
			THEN 'A'
		WHEN wp.[Wo_Type] = 'R'
			THEN 'B'
			ELSE wp.[Mgt_Priority_Code] END, 'D') AS WO_Priority
    ,(SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) as 'SalesOrderType'
	,(SELECT Remarks FROM dbo.[RT-INIT_Remarks] AS rt WHERE (ID = { fn CONCAT({ fn CONCAT(im.Part_Number, '*') }, SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID))) }) AND (ID2 = 1)) AS Op_Desc
    ,ISNULL((SELECT ISNULL(Insp_Req, 'N') AS Expr1 FROM dbo.[RT-INIT] AS rt WHERE (ID = { fn CONCAT({ fn CONCAT(im.Part_Number, '*') }, SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID))) })), 'N') AS Inspection
	,(SELECT Cust_Part_Nbr FROM dbo.[SOD-INIT] AS ac WHERE (ID = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1))) AS Cust_Part_Nbr
	,CAST(ISNULL((SELECT Ln_Bal_Qty FROM dbo.[SOD-INIT] AS ad WHERE (ID = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1))), 0) AS int) AS Ln_Bal_Qty
	,CAST(wc.[Fac_Code] as int) as 'Site'
	,CASE WHEN CAST(wp.[Date_Sch_Comp] as date) > CAST(wp.[Date_Orig_Comp] as date) THEN 1 ELSE 0 END as 'IsPastDue'
    ,wpo.[InQue] as 'LaborState'
FROM
	dbo.[WC-INIT] AS wc
LEFT JOIN
	dbo.[WPO-INIT] AS wpo ON wpo.Work_Center = wc.Wc_Nbr
LEFT JOIN
	dbo.[WP-INIT] AS wp ON wp.Wp_Nbr = SUBSTRING(wpo.ID, 0, CHARINDEX('*', wpo.ID, 0))
LEFT JOIN
	dbo.[IM-INIT] AS im ON im.Part_Number = wp.Part_Wo_Desc
WHERE
	(wc.D_esc <> 'DO NOT USE') AND (wpo.Alt_Seq_Status IS NULL) AND (wp.Status_Flag = 'C' OR wp.Status_Flag = 'A' OR wp.Status_Flag = 'R') AND im.[Part_Number] IS NOT NULL AND wc.[Fac_Code] = 1
ORDER BY
	MachineOrder, MachineNumber, WO_Priority, Sched_Shift, Sched_Priority, WO_SchedStartDate, WorkOrderID ASC";

            //var _conString = "SELECT * FROM [dbo].[SFW_Schedule] ORDER BY MachineOrder, MachineNumber, WO_Priority, PriTime, Sched_Priority, WO_SchedStartDate, WorkOrderID ASC";
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; {_conString}", sqlCon))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", site);
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

        /// <summary>
        /// Checks to see if the work order is valid
        /// </summary>
        /// <param name="woNumber">Work Order number to check</param>
        /// <param name="seq">Optional: Machine Name</param>
        /// <returns>Validation as bool; true = valid, false = invalid</returns>
        public static bool Exists(string woNumber, int seq)
        {
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT COUNT(ID) FROM [dbo].[WPO-INIT] WHERE [ID] = @p1", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{woNumber}*{seq}");
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i > 0 : false;
                    }
                }
                catch (SqlException)
                {
                    return false;
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
        /// Update the que state of a work order
        /// </summary>
        /// <param name="orderId">Work Order and seq</param>
        /// <param name="state">Que state to change to</param>
        /// <returns>Pass or Fail as bool</returns>
        public static bool UpdateQue(string orderId, int state)
        {
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; UPDATE [dbo].[WPO-INIT] SET [InQue] = @p1 WHERE [ID] = @p2", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{state}");
                        cmd.Parameters.AddWithValue("p2", $"{orderId}");
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (SqlException)
                {
                    return false;
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
        /// WorkOrder object default constructor
        /// </summary>
        public WorkOrder()
        { }

        /// <summary>
        /// Work Order Object constructor
        /// Will create a new WorkOrder Object based on a Work Order Number
        /// </summary>
        /// <param name="woNumber">Work Order Number</param>
        /// <param name="type">Type of work order to load, 'S' for standard and 'P' for Plan</param>
        public WorkOrder(string woNumber, char type)
        {
            var _rows = type == 'S'
                ? MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{woNumber}'")
                : MasterDataSet.Tables[typeof(WorkPlan).Name].Select($"[WorkOrder] = '{woNumber}'");
            Product = new Sku();
            WorkCenter = new Machine();
            if (_rows.Length > 0)
            {
                var _row = _rows.FirstOrDefault();
                OrderID = _row.Field<string>("WorkOrderID");
                OrderNumber = _row.Field<string>("WorkOrder");
                Seq = _row.Field<string>("Operation");
                Product.Operation = _row.Field<string>("Operation");
                OpDesc = _row.Field<string>("Op_Desc");
                Routing = _row.Field<string>("Routing");
                State = _row.Field<string>("WO_Priority");
                TaskType = _row.Field<string>("WO_Type");
                StartQty = _row.Field<int>("WO_StartQty");
                CurrentQty = _row.Field<int>("WO_CurrentQty");
                SchedStartDate = _row.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = _row.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? _row.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = _row.Field<DateTime>("WO_DueDate");
                Product.SkuNumber = _row.Field<string>("SkuNumber");
                Product.SkuDescription = _row.Field<string>("SkuDesc");
                Product.Uom = _row.Field<string>("SkuUom");
                Product.MasterPrint = _row.Field<string>("SkuMasterPrint");
                Product.InternalRev = _row.Field<DateTime>("InternalRev") != Convert.ToDateTime("1999-01-01") ? _row.Field<DateTime>("InternalRev").ToString("yyMMdd-1") : string.Empty;
                Product.CustomerRev = _row.Field<string>("CustomerRev");
                Product.IsTransfer = _row.Field<int>("Site") == 2 ? Sku.GetIsTransfer(_row.Field<string>("SkuNumber")) : false;
                if (!string.IsNullOrEmpty(_row.Field<string>("WO_SalesRef")))
                {
                    SalesOrder = new Sales.SalesOrder(_row.Field<string>("WO_SalesRef"));
                }
                else
                {
                    SalesOrder = new Sales.SalesOrder();
                }
                WorkCenter.MachineName = _row.Field<string>("MachineName");
                WorkCenter.MachineGroup = _row.Field<string>("MachineGroup");
                IsDeviated = _row.Field<string>("Deviation") == "Y";
                Product.Inspection = _row.Field<string>("Inspection") == "Y";
                Priority = _row.Field<int>("Sched_Priority");
                Shift = _row.Field<int>("Sched_Shift");
                Facility = _row.Field<int>("Site");
                OriginStartDate = _row.Field<DateTime>("OriginalStartDate");
                OriginDueDate = _row.Field<DateTime>("OriginalDueDate");
                InQue = bool.TryParse(_row.Field<int>("LaborState").ToString(), out bool b) ? b : false;
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
                Product = new Sku(dRow.Field<string>("SkuNumber"));
                var _wc = int.TryParse(dRow.Field<string>("MachineNumber"), out int i) ? i: 41000;
                WorkCenter = new Machine(_wc);
                OrderID = dRow.Field<string>("WorkOrderID");
                OrderNumber = dRow.Field<string>("WorkOrder");
                Seq = dRow.Field<string>("Operation");
                OpDesc = dRow.Field<string>("Op_Desc");
                Routing = dRow.Field<string>("Routing");
                State = dRow.Field<string>("WO_Priority");
                TaskType = dRow.Field<string>("WO_Type");
                StartQty = dRow.Field<int>("WO_StartQty");
                CurrentQty = dRow.Field<int>("WO_CurrentQty");
                SchedStartDate = dRow.Field<DateTime>("WO_SchedStartDate");
                ActStartDate = dRow.Field<DateTime>("WO_ActStartDate") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("WO_ActStartDate") : DateTime.MinValue;
                DueDate = dRow.Field<DateTime>("WO_DueDate");
                Product.Operation = dRow.Field<string>("Operation");
                Product.SkuDescription = dRow.Field<string>("SkuDesc");
                Product.Uom = dRow.Field<string>("SkuUom");
                Product.MasterPrint = dRow.Field<string>("SkuMasterPrint");
                Product.InternalRev = dRow.Field<DateTime>("InternalRev") != Convert.ToDateTime("1999-01-01") ? dRow.Field<DateTime>("InternalRev").ToString("yyMMdd-1") : string.Empty;
                Product.CustomerRev = dRow.Field<string>("CustomerRev");
                Product.IsTransfer = dRow.Field<int>("Site") == 2
                    ? Sku.GetIsTransfer(dRow.Field<string>("SkuNumber"))
                    : false;
                if (!string.IsNullOrEmpty(dRow.Field<string>("WO_SalesRef")))
                {
                    SalesOrder = new Sales.SalesOrder(dRow.Field<string>("WO_SalesRef"));
                }
                else
                {
                    SalesOrder = new Sales.SalesOrder();
                }
                IsDeviated = dRow.Field<string>("Deviation") == "Y";
                Product.Inspection = dRow.Field<string>("Inspection") == "Y";
                Priority = dRow.Field<int>("Sched_Priority");
                Shift = dRow.Field<int>("Sched_Shift");
                Facility = dRow.Field<int>("Site");
                IsStarted = true;
                OriginStartDate = dRow.Field<DateTime>("OriginalStartDate");
                OriginDueDate = dRow.Field<DateTime>("OriginalDueDate");
                InQue = bool.TryParse(dRow.Field<int>("LaborState").ToString(), out bool b) ? b : false;
            }
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
                return MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{woNumber}'").Length > 0;
            }
            else if (string.IsNullOrEmpty(machName) && pri != 0)
            {
                return MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{woNumber}' AND [Sched_Priority] = {pri}").Length > 0;
            }
            else if (!string.IsNullOrEmpty(machName) && pri == 0)
            {
                return MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{woNumber}' AND [MachineName] = '{machName}'").Length > 0;
            }
            else
            {
                return MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{woNumber}' AND [MachineName] = '{machName}' AND [Sched_Priority] = {pri}").Length > 0;
            }
        }

        /// <summary>
        /// Get the current Que State of a work order
        /// </summary>
        /// <param name="orderId">Full work order ID</param>
        /// <returns>Que State as an int</returns>
        public static int GetQueState(string orderId)
        {
            var _row = MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[WorkOrder] = '{orderId}'");
            return _row[0].Field<int>("LaborState");
        }

        /// <summary>
        /// Get the work order priority list
        /// </summary>
        /// <param name="machineName">Machine name</param>
        /// <returns>Work order priority as a list of work order objects</returns>
        public static List<WorkOrder> GetWorkOrderPriList(string machineName)
        {
            var _tempList = new List<WorkOrder>();
            var _rows = MasterDataSet.Tables[typeof(WorkOrder).Name].Select($"[MachineName] = '{machineName}' AND [Sched_Priority] <> 999 AND [Status] <> 'C'");
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
