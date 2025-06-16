using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Production
{
    public class Machine : ModelBase, IModuleData
    {
        #region Properties

        public string MachineNumber { get; set; }
        public string MachineName { get; set; }
        public string MachineDescription { get; set; }
        public string MachineGroup { get; set; }
        public bool IsLoaded { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Get a table containing all of the Machines
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns></returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
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
        /// Gets a machine ID to load
        /// </summary>
        /// <param name="woNumber">Work Order number to check</param>
        /// <param name="seq">Optional: Machine Name</param>
        /// <returns>Validation as bool; true = valid, false = invalid</returns>
        public static int GetID(string woNumber, int seq)
        {
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT [Work_Center] FROM [dbo].[WPO-INIT] WHERE [ID] = @p1", ModelSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", $"{woNumber}*{seq}");
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                    }
                }
                catch (SqlException)
                {
                    return 0;
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
        /// Work Center Constructor
        /// </summary>
        public Machine()
        { }

        /// <summary>
        /// Work Center Overridden Constructor
        /// </summary>
        public Machine(int machId)
        {
            var _machineDataRow = MasterDataSet.Tables[new Machine().GetType().Name].Select($"[WorkCenterID] = '{machId}'").FirstOrDefault();
            MachineNumber = machId.ToString();
            MachineName = _machineDataRow?.Field<string>("Name");
            MachineDescription = _machineDataRow?.Field<string>("Description");
            MachineGroup = _machineDataRow?.Field<string>("Group");
        }

        /// <summary>
        /// Get a list of work centers
        /// </summary>
        /// <param name="incAll">Include all at the top of the list</param>
        /// <param name="incNone">Include None at the top of the list</param>
        /// <param name="facCode">Facility Code</param>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetList(bool incAll, bool incNone, int facCode)
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
            foreach (DataRow _row in MasterDataSet.Tables[typeof(Machine).Name].Rows)
            {
                if (_row.Field<int>("Site") == facCode)
                {
                    _tempList.Add(new Machine
                    {
                        MachineNumber = _row.Field<string>("WorkCenterID")
                    ,
                        MachineName = _row.Field<string>("Name")
                    ,
                        MachineDescription = _row.Field<string>("Description")
                    ,
                        MachineGroup = _row.Field<string>("Group")
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
        public static List<string> GetNameList(bool incAll, int facCode)
        {
            var _tempList = new List<string>();
            if (MasterDataSet != null && MasterDataSet.Tables.Contains(typeof(Machine).Name))
            {
                if (incAll)
                {
                    _tempList.Add("All");
                }
                foreach (DataRow _row in MasterDataSet.Tables[typeof(Machine).Name].Rows)
                {
                    if (_row.Field<int>("Site") == facCode)
                    {
                        _tempList.Add(_row.Field<string>("Name"));
                    }
                }
                _tempList = _tempList.OrderBy(o => o).ToList();
            }
            return _tempList;
        }

        /// <summary>
        /// Get a machines shift capacity
        /// </summary>
        /// <param name="machName">Name of machine</param>
        /// <returns>machine shift capacity as a string</returns>
        public static string GetShift(string machName)
        {
            if (machName.Contains('('))
            {
                machName = machName.Split('(')[1].Replace(')', ' ').Trim();
            }
            return MasterDataSet.Tables[typeof(Machine).Name].Select($"[WorkCenterID] = '{machName}'")[0].Field<int>("Shifts").ToString();
        }

        /// <summary>
        /// Retrieve a List of strings of each of the groups assigned to the machines
        /// </summary>
        /// <param name="incAll">Include all in the top of the list</param>
        /// <param name="facCode">Facility Code</param>
        /// <returns>List of work center groups as strings</returns>
        public static List<string> GetGroupList(bool incAll, int facCode)
        {
            var _tempList = new List<string>();
            if (incAll)
            {
                _tempList.Add("All");
            }
            _tempList.Add("Custom");
            if (MasterDataSet != null)
            {
                foreach (DataRow _row in MasterDataSet.Tables[typeof(Machine).Name].DefaultView.ToTable(true, "Group", "Site").Rows)
                {
                    if (_row.Field<int>("Site") == facCode)
                    {
                        _tempList.Add(_row.Field<string>("Group"));
                    }
                }
                _tempList = _tempList.OrderBy(o => o).ToList();
            }
            return _tempList;
        }

        /// <summary>
        /// Get a machines display name
        /// </summary>
        /// <param name="searchValue">Value to use in the search</param>
        /// <param name="searchType">Type of search to perform (P is Part Number, M is Machine ID, W is Work Order)</param>
        /// <returns>Machine Name as string</returns>
        public static string GetName(string searchValue, char searchType)
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
                if (searchType != 'W')
                {
                    var _rows = MasterDataSet.Tables[typeof(Machine).Name].Select($"[WorkCenterID] = '{searchValue}'");
                    if (_rows.Length > 0)
                    {
                        _rVal = _rows.FirstOrDefault().Field<string>("Name");
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// Get the machine group that a specific machine is a part of
        /// </summary>
        /// <param name="searchValue">Search value when looking for the machine group</param>
        /// <param name="type">Type of search. M = Machine Name, N = Machine Number</param>
        /// <returns>machine group</returns>
        public static string GetGroup(string searchValue, char type)
        {
            switch (type)
            {
                case 'M':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables[typeof(Machine).Name].Select($"[Name] = '{searchValue}'")[0].Field<string>("Group");
                case 'N':
                    return searchValue == "All" || string.IsNullOrEmpty(searchValue)
                        ? searchValue
                        : MasterDataSet.Tables[typeof(Machine).Name].Select($"[WorkCenterID] = '{searchValue}'")[0].Field<string>("Group");
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the machine number from a machine name
        /// </summary>
        /// <param name="machineName">Machine name to get the group of</param>
        /// <returns>machine number</returns>
        public static string GetNumber(string machineName)
        {
            return machineName == "All"
                ? machineName
                : MasterDataSet.Tables[typeof(Machine).Name].Select($"[Name] = '{machineName}'")[0].Field<string>("WorkCenterID");
        }

        /// <summary>
        /// Get the default receipt location
        /// </summary>
        /// <param name="machineName">Name of the machine</param>
        /// <returns>Receipt Location as a string</returns>
        public static string GetDefaultLocation(string machineName)
        {
            var _tempRow = MasterDataSet.Tables[typeof(Machine).Name].Select($"[Name] = '{machineName}'");
            return string.IsNullOrEmpty(_tempRow[0].Field<string>("ReceiptLoc")) ? string.Empty : _tempRow[0].Field<string>("ReceiptLoc");
        }

        /// <summary>
        /// Get the default pull location
        /// </summary>
        /// <param name="machineName">Name of the machine</param>
        /// <returns>Pull Location as a string</returns>
        public static string GetPullLocation(string machineName)
        {
            var _tempRow = MasterDataSet.Tables[typeof(Machine).Name].Select($"[Name] = '{machineName}'");
            return string.IsNullOrEmpty(_tempRow[0].Field<string>("PullLoc")) ? string.Empty : _tempRow[0].Field<string>("PullLoc");
        }
    }
}
