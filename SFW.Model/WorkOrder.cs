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

        #endregion

        /// <summary>
        /// WorkOrder object default constructor
        /// </summary>
        public WorkOrder()
        { }

        /// <summary>
        /// Work Order Object constructor
        /// Will create a new WorkOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the work order</param>
        /// <param name="siteNbr">Work Site number</param>
        /// <param name="docFilePath">Document File Path</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public WorkOrder(DataRow dRow, int siteNbr, string docFilePath, SqlConnection sqlCon)
        {
            if (dRow != null)
            {
                var _wo = dRow.Field<string>("WO_Number").Split('*');
                OrderNumber = _wo[0];
                Seq = _wo[1];
                Operation = dRow.Field<string>("Operation");
                OpDesc = GetOperationDescription(dRow.Field<string>("WO_Number"), sqlCon);
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
                        LineQuantity = dRow.SafeGetField<int>("Ln_Bal_Qty"),
                        LoadPattern = dRow.Field<string>("LoadPattern").ToUpper() == "PLASTIC"
                     };
                    SalesOrder.GetInternalComments(sqlCon);
                }
                else
                {
                    SalesOrder = new SalesOrder();
                }
                ToolList = GetTools(SkuNumber, Operation, sqlCon);
                Machine = dRow.Field<string>("MachineName");
                MachineGroup = dRow.Field<string>("MachineGroup");
                Bom = Component.GetComponentBomList(SkuNumber, Operation, sqlCon);
                Picklist = Component.GetComponentPickList(_wo[0], Operation, StartQty - CurrentQty, sqlCon);
                Notes = GetNotes(_wo[0],sqlCon);
                ShopNotes = GetShopNotes(_wo[0], sqlCon);
                InstructionList = GetInstructions(SkuNumber, siteNbr, docFilePath, sqlCon);
            }
        }

        /// <summary>
        /// Retrieve a list of Work orders based on a work center
        /// </summary>
        /// <param name="workCntNbr">Work Center Number or ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>List of WorkOrder objects</returns>
        public static List<WorkOrder> GetWorkOrderList(string workCntNbr, SqlConnection sqlCon)
        {
            var _tempList = new List<WorkOrder>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(@"SELECT 
                                                                SUBSTRING(a.[ID], 0, CHARINDEX('*',a.[ID],0)) as 'WoNumber',
	                                                            SUBSTRING(a.[ID], CHARINDEX('*',a.[ID],0) + 1, LEN(a.[ID])) as 'Seq',
	                                                            a.[Qty_Avail] as 'CurrentQty', ISNULL(a.[Qty_Scrap], 0) as 'Scrap', a.[Date_Start] as 'SchedStartDate', a.[Date_Act_Start] as 'ActStartDate', a.[Due_Date] as 'DueDate',
	                                                            b.[Part_Wo_Desc] as 'WoDesc', ISNULL(b.[Mgt_Priority_Code], 'D') as 'Priority', b.[Qty_To_Start] as 'StartQty', b.[So_Reference] as 'SalesOrder'
                                                            FROM
                                                                [dbo].[WPO-INIT] a
                                                            RIGHT JOIN
                                                                [dbo].[WP-INIT] b on a.[ID] LIKE CONCAT(b.[Wp_Nbr], '%')
                                                            WHERE
                                                                (b.[Status_Flag] = 'R' or b.[Status_Flag] = 'A') AND a.[Seq_Complete_Flag] IS NULL AND a.[Work_Center] = @p1
                                                            ORDER BY
                                                                StartDate, WoNumber ASC;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", workCntNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new WorkOrder
                                    {
                                        OrderNumber = reader.SafeGetString("WoNumber"),
                                        Seq = reader.SafeGetString("Seq"),
                                        Priority = reader.SafeGetString("Priority"),
                                        StartQty = reader.SafeGetInt32("StartQty"),
                                        CurrentQty = reader.SafeGetInt32("CurrentQty"),
                                        ScrapQty = reader.SafeGetInt32("Scrap"),
                                        SchedStartDate = reader.SafeGetDateTime("SchedStartDate"),
                                        DueDate = reader.SafeGetDateTime("DueDate"),
                                        SalesOrder = new SalesOrder(reader.SafeGetString("SalesOrder"), sqlCon)
                                    });
                                }
                            }
                        }
                    }
                    foreach(var o in _tempList)
                    {
                        using (SqlCommand nCmd = new SqlCommand("SELECT * FROM [dbo].[WP-INIT_Wo_Notes] WHERE [ID] = @p1;", sqlCon))
                        {
                            nCmd.Parameters.AddWithValue("p1", o.OrderNumber);
                            using (SqlDataReader reader = nCmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        o.Notes += reader.SafeGetString("");
                                    }
                                }
                            }
                        }
                        using (SqlCommand sCmd = new SqlCommand("SELECT [Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] WHERE [ID] = @p1;", sqlCon))
                        {
                            sCmd.Parameters.AddWithValue("p1", o.OrderNumber);
                            using (SqlDataReader reader = sCmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        o.ShopNotes += reader.SafeGetString("");
                                    }
                                }
                            }
                        }
                    }
                    return _tempList.OrderBy(o => o.Priority).ToList();
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

        /// <summary>
        /// Get a work order's notes
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A concatonation of notes into a string</returns>
        public static string GetNotes(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Wo_Notes] FROM [dbo].[WP-INIT_Wo_Notes] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Wo_Notes")}\n";
                                }
                            }
                        }
                    }
                    return _notes;
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

        /// <summary>
        /// Get a work order's shop floor notes
        /// </summary>
        /// <param name="woNbr">Work Order Number</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A concatonation of shop floor notes into a string</returns>
        public static string GetShopNotes(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Wo_Sf_Notes] FROM [dbo].[WP-INIT_Wo_Sf_Notes] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Wo_Sf_Notes")}\n";
                                }
                            }
                        }
                    }
                    return _notes;
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

        /// <summary>
        /// Get a work order's operation description
        /// </summary>
        /// <param name="woNbr">Work order number with sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Operation description as string</returns>
        public static string GetOperationDescription(string woNbr, SqlConnection sqlCon)
        {
            var _notes = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Remarks] FROM [dbo].[WPO_REMARKS-INIT_Remarks] WHERE [ID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _notes += $"{reader.SafeGetString("Remarks")} ";
                                }
                            }
                        }
                    }
                    return _notes;
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

        /// <summary>
        /// Get the machine ID that is assigned to a specific work order and sequence
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Machine ID as string</returns>
        public static string GetAssignedMachineID(string woNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = CONCAT(@p1, '*', @p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        return cmd.ExecuteScalar().ToString();
                    }
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

        /// <summary>
        /// Get the machine name that is assigned to a specific work order and sequence
        /// </summary>
        /// <param name="woNbr">Work order number</param>
        /// <param name="seq">Work order sequence</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Machine name as string</returns>
        public static string GetAssignedMachineName(string woNbr, string seq, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT 
	                                                                b.[Name]
                                                                FROM
	                                                                [dbo].[WPO-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[WC-INIT] b ON b.[Wc_Nbr] = a.[Work_Center]
                                                                WHERE
	                                                                a.[ID] = CONCAT(@p1, '*', @p2);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woNbr);
                        cmd.Parameters.AddWithValue("p2", seq);
                        return cmd.ExecuteScalar().ToString();
                    }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="woNbr"></param>
        /// <param name="seq"></param>
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        public static DataTable GetReportData(WorkOrder wo, SqlConnection sqlCon)
        {
            return null;
            //TODO: need to remove the hardcoded value to segregate CSI from WCCO when CSI reports become live
            /*if (wo != null && sqlCon.Database != "CSI_MAIN")
            {
                using (var dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            //Populate the main portion of the report sheet
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database};
                                                                                    SELECT
	                                                                                    CAST(a.[Tran_Date] as DATE) as 'Date',
	                                                                                    CASE WHEN a.[Scan_Station_ID] IS NULL
		                                                                                    THEN
			                                                                                    'N/A'
		                                                                                    ELSE
			                                                                                    CAST((SELECT [Shift] FROM [dbo].[EMPLOYEE_MASTER-INIT] WHERE [First_Name] = SUBSTRING(a.[Scan_Station_ID], 0, CHARINDEX(' ',a.[Scan_Station_ID])) AND [Last_Name] = LTRIM(SUBSTRING(a.[Scan_Station_ID], CHARINDEX(' ',a.[Scan_Station_ID]), LEN(a.[Scan_Station_ID])))) as NVARCHAR(5))
		                                                                                    END as 'Shift',
	                                                                                    CASE WHEN a.[Scan_Station_ID] IS NULL THEN SUBSTRING(a.[Logon],0,CHARINDEX(':',a.[Logon])) ELSE a.[Scan_Station_ID] END as 'Name',
	                                                                                    a.[Qty] as 'QtyGood',
	                                                                                    '' as 'FromLot',
	                                                                                    SUBSTRING(a.[Lot_Number],0,CHARINDEX('|',a.[Lot_Number])) as 'ToLot',
	                                                                                    @p2 as 'FromPart',
                                                                                        '' as 'BomScrap',
                                                                                        '' as 'BomQir',
	                                                                                    '' as 'PartScrap',
                                                                                        ISNULL((SELECT CAST([QIRNumber] as varchar(15)) FROM [OMNI].[dbo].[qir_metrics_view] WHERE [LotNumber] = SUBSTRING(a.[Lot_Number],0,CHARINDEX('|',a.[Lot_Number])) AND [PartNumber] = @p3), '-') as 'PartQir',
                                                                                        a.[Process_Time_Date] as 'PTD'
                                                                                    FROM
	                                                                                    [dbo].[IT-INIT] a
                                                                                    WHERE
	                                                                                    a.[ID] LIKE CONCAT(@p3,'*%') AND a.[Tran_Code] = '40' AND a.[Reference] LIKE CONCAT('%',@p1,'%');", sqlCon))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("p1", wo.OrderNumber);
                                adapter.SelectCommand.Parameters.AddWithValue("p2", wo.Bom.FirstOrDefault(o => o.IsLotTrace).CompNumber);
                                adapter.SelectCommand.Parameters.AddWithValue("p3", wo.SkuNumber);
                                adapter.Fill(dt);

                                //Relooping through the IT table extended the query by 30 seconds so moved to a code loop to complete the scrap
                                foreach (DataRow d in dt.Rows)
                                {
                                    //Get the shift for any old report data
                                    if (d.Field<string>("Shift") == "N/A")
                                    {
                                        var fullName = CrewMember.GetCrewMemberFullName(d.Field<string>("Name"));
                                        d.SetField("Name", fullName);
                                        if (fullName.Contains(' '))
                                        {
                                            var nameSplit = fullName.Split(' ');
                                            using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                [Shift]
                                                                                FROM
	                                                                                [dbo].[EMPLOYEE_MASTER-INIT]
                                                                                WHERE
	                                                                                [First_Name] = @p1 AND [Last_Name] = @p2;", sqlCon))
                                            {
                                                cmd.Parameters.AddWithValue("p1", nameSplit[0]);
                                                cmd.Parameters.AddWithValue("p2", nameSplit[1]);
                                                var shift = cmd.ExecuteScalar()?.ToString();
                                                if (!string.IsNullOrEmpty(shift))
                                                {
                                                    d.SetField("Shift", shift);
                                                }
                                                else
                                                {
                                                    d.SetField("Shift", "N/A");
                                                }
                                            }
                                        }
                                    }

                                    //From Lot
                                    var ptdArray = d.Field<string>("PTD").Split('*');
                                    var ptdTime = Convert.ToInt32(ptdArray[1]);
                                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                                SELECT
	                                                                                ISNULL(SUBSTRING([Lot_Number],0,CHARINDEX('|',[Lot_Number])), 'Not Found') as 'FromLot'
                                                                                FROM
	                                                                                [dbo].[IT-INIT]
                                                                                WHERE
	                                                                                [ID] LIKE CONCAT(@p2,'*%')
	                                                                                AND [Tran_Code] = '44'
	                                                                                AND [Reference] LIKE CONCAT('%',@p1,'%')
	                                                                                AND [Lot_Number] IS NOT NULL
	                                                                                AND ([Process_Time_Date] = CONCAT(@p3,'*',@p4) OR [Process_Time_Date] = CONCAT(@p3,'*',@p4-1) OR [Process_Time_Date] = CONCAT(@p3,'*',@p4+1));", sqlCon))
                                    {
                                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                                        cmd.Parameters.AddWithValue("p2", d.Field<string>("FromPart"));
                                        cmd.Parameters.AddWithValue("p3", ptdArray[0]);
                                        cmd.Parameters.AddWithValue("p4", ptdTime);
                                        var fLot = cmd.ExecuteScalar()?.ToString();
                                        if (!string.IsNullOrEmpty(fLot))
                                        {
                                            d.SetField("FromLot", fLot);
                                        }
                                        else
                                        {
                                            d.SetField("FromLot", "Not Found");
                                        }
                                    }

                                    //Part Scrap
                                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                                    SELECT
	                                                                                    CAST([Qty] as INT) * -1 as 'Qty'
                                                                                    FROM
	                                                                                    [dbo].[IT-INIT]
                                                                                    WHERE
	                                                                                    [Tran_Code] = '50' AND [Reference] LIKE CONCAT('%',@p1,'%') AND [Lot_Number] = CONCAT(@p2,'|P');", sqlCon))
                                    {
                                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                                        cmd.Parameters.AddWithValue("p2", d.Field<string>("ToLot"));
                                        var pScrap = cmd.ExecuteScalar()?.ToString();
                                        if (!string.IsNullOrEmpty(pScrap))
                                        {
                                            d.SetField("PartScrap", pScrap);
                                        }
                                        else
                                        {
                                            d.SetField("PartScrap", "-");
                                        }
                                    }

                                    //BOM Scrap
                                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                                    SELECT
	                                                                                    CAST([Qty]  as INT) * -1 as 'Qty'
                                                                                    FROM
	                                                                                    [dbo].[IT-INIT]
                                                                                    WHERE
	                                                                                    [Tran_Code] = '50' AND [Reference] LIKE CONCAT('%',@p1,'%') AND [Lot_Number] = CONCAT(@p2,'|P') AND [ID] LIKE CONCAT(@p3,'*%');", sqlCon))
                                    {
                                        cmd.Parameters.AddWithValue("p1", wo.OrderNumber);
                                        cmd.Parameters.AddWithValue("p2", d.Field<string>("FromLot"));
                                        cmd.Parameters.AddWithValue("p3", wo.Bom.FirstOrDefault(o => o.IsLotTrace).CompNumber);
                                        var bScrap = cmd.ExecuteScalar()?.ToString();
                                        if (!string.IsNullOrEmpty(bScrap))
                                        {
                                            d.SetField("BomScrap", bScrap);
                                        }
                                        else
                                        {
                                            d.SetField("BomScrap", "-");
                                        }
                                    }

                                    //BOM QIR
                                    using (SqlCommand cmd = new SqlCommand($@"SELECT
	                                                                            [QIRNumber]
                                                                            FROM
	                                                                            [OMNI].[dbo].[qir_metrics_view]
                                                                            WHERE
	                                                                            [LotNumber] = @p1 AND [PartNumber] = @p2;", sqlCon))
                                    {
                                        cmd.Parameters.AddWithValue("p1", d.Field<string>("FromLot"));
                                        cmd.Parameters.AddWithValue("p2", wo.Bom.FirstOrDefault(o => o.IsLotTrace).CompNumber);
                                        var bQir = cmd.ExecuteScalar()?.ToString();
                                        if (!string.IsNullOrEmpty(bQir))
                                        {
                                            d.SetField("BomQir", bQir);
                                        }
                                        else
                                        {
                                            d.SetField("BomQir", "-");
                                        }
                                    }
                                }
                                return dt;
                            }
                        }
                        catch (SqlException)
                        {
                            return null;
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }
            return null;*/
        }
    }
}
