﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work Center
    /// </summary>
    public class Machine : WorkOrder
    {
        #region Properties

        public string MachineNumber { get; set; }
        public string MachineName { get; set; }
        public string MachineDescription { get; set; }
        new public string MachineGroup { get; set; }
        public bool IsLoaded { get; set; }

        #endregion

        /// <summary>
        /// Work Center Constructor
        /// </summary>
        public Machine()
        { }

        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="machOrder">Dictionary containing the order property for the machines, based on the user config</param>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(IReadOnlyDictionary<string, int> machOrder, int site, SqlConnection sqlCon)
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
	,CASE WHEN wpo.Qty_Req - ISNULL(wpo.Qty_Compl, 0) <= 0
		THEN 0
		ELSE wpo.Qty_Req - ISNULL(wpo.Qty_Compl, 0) END AS WO_CurrentQty
	,ISNULL(wpo.Date_Start, '1999-01-01') AS WO_SchedStartDate
	,ISNULL(wpo.Date_Act_Start, '1999-01-01') AS WO_ActStartDate
	,ISNULL(wpo.Due_Date, ISNULL(wpo.Date_Start, '1999-01-01')) AS WO_DueDate
	,ISNULL(CAST(ROUND(wpo.Mach_Load_Hrs_Rem, 1) AS float), 0) AS RunTime
	,CASE WHEN wpo.[Due_Date] < GETDATE()
		THEN 1
		ELSE 0
	END AS IsLate
	,CASE WHEN wpo.[Date_Start] < GETDATE() AND wp.[Qty_To_Start] = wpo.[Qty_Avail]
		THEN 1
		ELSE 0
	END AS IsStartLate
	,ISNULL(wp.Wo_Type, 'S') AS WO_Type
	,wp.Qty_To_Start AS WO_StartQty
	,SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1) AS WO_SalesRef
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
	,ISNULL(CASE WHEN (SELECT aa.[Ord_Type] FROM [dbo].[SOH-INIT] aa WHERE aa.[So_Nbr] = SUBSTRING(wp.[So_Reference], 0, CHARINDEX('*', wp.[So_Reference], 0))) = 'DAI' THEN 'A' WHEN wp.[Wo_Type] = 'R'
		THEN 'B'
		ELSE wp.[Mgt_Priority_Code]
	END, 'D') AS WO_Priority
	,(SELECT Remarks FROM dbo.[RT-INIT_Remarks] AS rt WHERE (ID = { fn CONCAT({ fn CONCAT(im.Part_Number, '*') }, SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID))) }) AND (ID2 = 1)) AS Op_Desc
    ,ISNULL((SELECT ISNULL(Insp_Req, 'N') AS Expr1 FROM dbo.[RT-INIT] AS rt WHERE (ID = { fn CONCAT({ fn CONCAT(im.Part_Number, '*') }, SUBSTRING(wpo.ID, CHARINDEX('*', wpo.ID, 0) + 1, LEN(wpo.ID))) })), 'N') AS Inspection
	,(SELECT Cust_Part_Nbr FROM dbo.[SOD-INIT] AS ac WHERE (ID = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1))) AS Cust_Part_Nbr
	,CAST(ISNULL((SELECT Ln_Bal_Qty FROM dbo.[SOD-INIT] AS ad WHERE (ID = SUBSTRING(wp.So_Reference, 0, LEN(wp.So_Reference) - 1))), 0) AS int) AS Ln_Bal_Qty
	,CAST(wc.[Fac_Code] as int) as 'Site'
FROM
	dbo.[WC-INIT] AS wc
LEFT JOIN
	dbo.[WPO-INIT] AS wpo ON wpo.Work_Center = wc.Wc_Nbr
LEFT JOIN
	dbo.[WP-INIT] AS wp ON wp.Wp_Nbr = SUBSTRING(wpo.ID, 0, CHARINDEX('*', wpo.ID, 0))
LEFT JOIN
	dbo.[IM-INIT] AS im ON im.Part_Number = wp.Part_Wo_Desc
WHERE
	(wc.D_esc <> 'DO NOT USE') AND (wpo.Alt_Seq_Status IS NULL) AND (wp.Status_Flag = 'C' OR wp.Status_Flag = 'A' OR wp.Status_Flag = 'R') AND im.[Part_Number] IS NOT NULL AND wc.[Fac_Code] = @p1
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
                        foreach (var _keyValPair in machOrder)
                        {
                            DataRow[] _rows = _tempTable.Select($"MachineNumber={_keyValPair.Key}");
                            foreach (DataRow _row in _rows)
                            {
                                var _index = _tempTable.Rows.IndexOf(_row);
                                _tempTable.Rows[_index].SetField("MachineOrder", _keyValPair.Value);
                            }
                        }
                        _tempTable.DefaultView.Sort = "MachineOrder ASC";
                        _tempTable = _tempTable.DefaultView.ToTable();
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
        /// Get a table containing all of the Machines
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns></returns>
        public static DataTable GetMachineTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    var _selectCmd = string.Empty;
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE [{sqlCon.Database}]; SELECT * FROM [dbo].[SFW_Machine] WHERE [Site] = @p1", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_tempTable);
                            return _tempTable;
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
        /// Get a list of work centers
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<object> GetMachineList(SqlConnection sqlCon)
        {
            var _rtnList = new List<object>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE [{sqlCon.Database}]; SELECT * FROM [dbo].[SFW_Machine] WHERE [Site] = @p1", sqlCon))
                    {
                        using (SqlDataReader _reader = cmd.ExecuteReader())
                        {
                            if (_reader.HasRows)
                            {
                                while (_reader.Read())
                                {
                                    _rtnList.Add(new Machine
                                    {
                                        MachineNumber = _reader["WorkCenterID"].ToString()
                                        ,MachineName = _reader["Name"].ToString()
                                        ,MachineDescription = _reader["Description"].ToString()
                                        ,MachineGroup = _reader["Group"].ToString()
                                        ,IsLoaded = true
                                    });
                                }
                            }
                        }
                    }
                    return _rtnList;
                }
                catch (SqlException sqlEx)
                {
                    return _rtnList;
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
        /// Get a list of work centers
        /// </summary>
        /// <param name="incAll">Include all at the top of the list</param>
        /// <param name="incNone">Include None at the top of the list</param>
        /// <param name="facCode">Facility Code</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(bool incAll, bool incNone, int facCode)
        {
            var _tempList = new List<Machine>();
            if (incAll)
            {
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "All", IsLoaded = true, MachineGroup = "All" });
            }
            if (incNone)
            {
                _tempList.Add(new Machine { MachineNumber = "0", MachineName = "None", IsLoaded = false, MachineGroup = "None" });
            }
            foreach (DataRow _row in MasterDataSet.Tables["WC"].Rows)
            {
                if (_row.Field<int>("Site") == facCode)
                {
                    _tempList.Add(new Machine
                {
                    MachineNumber = _row.Field<string>("WorkCenterID")
                    ,MachineName = _row.Field<string>("Name")
                    ,MachineDescription = _row.Field<string>("Description")
                    ,MachineGroup = _row.Field<string>("Group")
                });
                }
            }
            return _tempList;
        }

        /// <summary>
        /// Get a list of work centers names
        /// </summary>
        /// <param name="incAll">Include all in the return list</param>
        /// <param name="facCode">Facility Code</param>
        /// <returns>generic list of workcenter objects</returns>
        public static List<string> GetMachineNameList(bool incAll, int facCode)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            foreach (DataRow _row in MasterDataSet.Tables["WC"].Rows)
            {
                if (_row.Field<int>("Site") == facCode)
                {
                    _tempList.Add(_row.Field<string>("Name"));
                }
            }
            _tempList = _tempList.OrderBy(o => o).ToList();
            return _tempList;
        }

        /// <summary>
        /// Retrieve a List of strings of each of the groups assigned to the machines
        /// </summary>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <param name="facCode">Facility Code</param>
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetMachineGroupList(bool incAll, int facCode)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            _tempList.Add("Custom");
            foreach (DataRow _row in MasterDataSet.Tables["WC"].DefaultView.ToTable(true, "Group", "Site").Rows)
            {
                if (_row.Field<int>("Site") == facCode)
                {
                    _tempList.Add(_row.Field<string>("Group"));
                }
            }
            _tempList = _tempList.OrderBy(o => o).ToList();
            return _tempList;
        }

        /// <summary>
        /// Get a machines display name
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <param name="searchType">Type of search to perform (P is Part Number, M is Machine ID, W is Work Order)</param>
        /// <returns>Machine Name as string</returns>
        public static string GetMachineName(string searchValue, char searchType)
        {
            var _rVal = string.Empty;
            if (searchValue == "0")
            {
                _rVal = "All";
            }
            else
            {
                switch (searchType)
                {
                    case 'P':
                        var _sRows = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                        if (_sRows.Length > 0)
                        {
                            searchValue = _sRows.FirstOrDefault().Field<string>("WorkCenterID");
                        }
                        break;
                    case 'W':
                        var _wRows = MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{searchValue}'");
                        if (_wRows.Length > 0)
                        {
                            _rVal = _wRows.FirstOrDefault().Field<string>("MachineName");
                        }
                        break;
                }
                if(searchType != 'W')
                {
                    var _rows = MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{searchValue}'");
                    if (_rows.Length > 0)
                    {
                        _rVal = _rows.FirstOrDefault().Field<string>("Name");
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// Checks to see if a machine exists
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <returns>Verification as a bool</returns>
        public static bool Exists(string searchValue)
        {
            return int.TryParse(searchValue, out int i)
                ? MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{i}'").Length > 0
                : MasterDataSet.Tables["WC"].Select($"[Name] = '{searchValue}'").Length > 0;
        }

        /// <summary>
        /// Get the machine group that a specific machine is a part of
        /// </summary>
        /// <param name="searchValue">Search value when looking for the machine group</param>
        /// <param name="type">Type of search. M = Machine Name, N = Machine Number</param>
        /// <returns>machine group</returns>
        public static string GetMachineGroup(string searchValue, char type)
        {
            switch(type)
            {
                case 'M':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables["WC"].Select($"[Name] = '{searchValue}'")[0].Field<string>("Group");
                case 'N':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{searchValue}'")[0].Field<string>("Group");
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the machine number from a machine name
        /// </summary>
        /// <param name="machineName">Machine name to get the group of</param>
        /// <returns>machine number</returns>
        public static string GetMachineNumber(string machineName)
        {
            return machineName == "All"
                ? machineName
                : MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'")[0].Field<string>("WorkCenterID");
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="machineNbr">Press ID number</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(int machineNbr)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{machineNbr}'");
            return _tempRow.Length > 1 || _tempRow[0].Field<int>("Length") == 0 ? 0 : _tempRow[0].Field<int>("Length") + 1;
        }

        /// <summary>
        /// Get the length of a press
        /// </summary>
        /// <param name="machineName">Name of the press</param>
        /// <returns>Press length as int</returns>
        public static int GetPress_Length(string machineName)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'");
            return _tempRow.Length > 1 || _tempRow[0].Field<int>("Length") == 0 ? 0 : _tempRow[0].Field<int>("Length") + 1;
        }

        /// <summary>
        /// Get the default receipt location
        /// </summary>
        /// <param name="machineName">Name of the machine</param>
        /// <returns>Receipt Location as a string</returns>
        public static string GetReceiptLocation(string machineName)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'");
            return string.IsNullOrEmpty(_tempRow[0].Field<string>("ReceiptLoc")) ? string.Empty : _tempRow[0].Field<string>("ReceiptLoc");
        }

        /// <summary>
        /// Get the default pull location
        /// </summary>
        /// <param name="machineName">Name of the machine</param>
        /// <returns>Pull Location as a string</returns>
        public static string GetPullLocation(string machineName)
        {
            var _tempRow = MasterDataSet.Tables["WC"].Select($"[Name] = '{machineName}'");
            return string.IsNullOrEmpty(_tempRow[0].Field<string>("PullLoc")) ? string.Empty : _tempRow[0].Field<string>("PullLoc");
        }
    }
}
