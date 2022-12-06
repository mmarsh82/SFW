using System;
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
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(IReadOnlyDictionary<string, int> machOrder, SqlConnection sqlCon)
        {
            var _conString = "SELECT * FROM [dbo].[SFW_WorkOrderView] ORDER BY MachineOrder, MachineNumber, WO_Priority, PriTime, Sched_Priority, WO_SchedStartDate, WorkOrderID ASC";
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; {_conString}", sqlCon))
                    {
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
        /// <param name="sqlCon"></param>
        /// <returns></returns>
        public static DataTable GetMachineTable(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    var _selectCmd = string.Empty;
                    if (sqlCon.Database.Contains("WCCO"))
                    {
                        _selectCmd = @"SELECT
	                                        [Wc_Nbr] as 'WorkCenterID'
	                                        ,[Name] as 'Name'
	                                        ,[D_esc] as 'Description'
	                                        ,[Work_Ctr_Group] as 'Group'
	                                        ,CAST(ISNULL([Press_Length], 0) as int) as 'Length'
                                        FROM
	                                        [dbo].[WC-INIT]
                                        WHERE
	                                        [D_esc] <> 'DO NOT USE' AND [Name] IS NOT NULL";
                    }
                    else
                    {
                        _selectCmd = @"SELECT
	                                        [Wc_Nbr] as 'WorkCenterID'
	                                        ,[Name] as 'Name'
	                                        ,[D_esc] as 'Description'
	                                        ,[Work_Ctr_Group] as 'Group'
                                        FROM
	                                        [dbo].[WC-INIT]
                                        WHERE
	                                        [D_esc] <> 'DO NOT USE' AND [Name] IS NOT NULL";
                    }
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE [{sqlCon.Database}]; {_selectCmd}", sqlCon))
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
        /// Get a list of work centers
        /// </summary>
        /// <param name="incAll">Include all at the top of the list</param>
        /// <param name="incNone">Include None at the top of the list</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(bool incAll, bool incNone)
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
                _tempList.Add(new Machine
                {
                    MachineNumber = _row.Field<string>("WorkCenterID")
                    ,MachineName = _row.Field<string>("Name")
                    ,MachineDescription = _row.Field<string>("Description")
                    ,MachineGroup = _row.Field<string>("Group")
                });
            }
            return _tempList;
        }

        /// <summary>
        /// Get a list of work centers names
        /// </summary>
        /// <returns>generic list of workcenter objects</returns>
        public static List<string> GetMachineList(bool incAll)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            foreach (DataRow _row in MasterDataSet.Tables["WC"].Rows)
            {
                _tempList.Add(_row.Field<string>("Name"));
            }
            _tempList = _tempList.OrderBy(o => o).ToList();
            return _tempList;
        }

        /// <summary>
        /// Retrieve a List of strings of each of the groups assigned to the machines
        /// </summary>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetMachineGroupList(bool incAll)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            _tempList.Add("Custom");
            foreach (DataRow _row in MasterDataSet.Tables["WC"].DefaultView.ToTable(true, "Group").Rows)
            {
                _tempList.Add(_row.Field<string>("Group"));
            }
            _tempList = _tempList.OrderBy(o => o).ToList();
            return _tempList;
        }

        /// <summary>
        /// Get a machines display name
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <returns>Machine Name as string</returns>
        public static string GetMachineName(string searchValue)
        {
            var _rVal = string.Empty;
            if (searchValue == "0")
            {
                _rVal = "All";
            }
            else
            {
                if (!Exists(searchValue))
                {
                    var _sRows = MasterDataSet.Tables["SKU"].Select($"[SkuID] = '{searchValue}' AND [Status] = 'A'");
                    if (_sRows.Length > 0)
                    {
                        searchValue = _sRows.FirstOrDefault().Field<string>("WorkCenterID");
                    }
                }
                var _rows = MasterDataSet.Tables["WC"].Select($"[WorkCenterID] = '{searchValue}'");
                if (_rows.Length > 0)
                {
                    _rVal = _rows.FirstOrDefault().Field<string>("Name");
                }
            }
            return _rVal;
        }

        /// <summary>
        /// Checks to see if a machine exists
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <returns>Verification as a bool</returns>
        public static new bool Exists(string searchValue)
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
    }
}
