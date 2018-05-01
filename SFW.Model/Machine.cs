using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

//Created by Michael Marsh 4-19-18

namespace SFW.Model
{
    /// <summary>
    /// Schedule's Work Center object
    /// </summary>
    public sealed class Machine : ModelBase
    {
        #region Properties

        public string MachineNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<WorkOrder> WorkOrderList { get; set; }
        public bool IsLoaded { get; set; }

        #endregion

        /// <summary>
        /// Work Center Constructor
        /// </summary>
        public Machine()
        {
            WorkOrderList = new List<WorkOrder>();
        }

        /// <summary>
        /// Get a list of work centers
        /// </summary>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetWorkCenterList(SqlConnection sqlCon)
        {
            var _tempList = new List<Machine>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT [Wc_Nbr], [Name], [D_esc] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _tempWoList = WorkOrder.GetWorkOrderList(reader.GetString(0), sqlCon);
                                    if (_tempWoList.Count > 0)
                                    {
                                        _tempList.Add(new Machine
                                        {
                                            MachineNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                            WorkOrderList = reader.IsDBNull(0) ? new List<WorkOrder>() : _tempWoList
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return _tempList;
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
        /// Get a list of work centers
        /// </summary>
        /// <returns>generic list of worcenter objects</returns>
        public static List<Machine> GetMachineList(SqlConnection sqlCon)
        {
            var _tempList = new List<Machine>();
            if (sqlCon != null || sqlCon.State != ConnectionState.Closed || sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT [Wc_Nbr], [Name] FROM [dbo].[WC-INIT] WHERE [D_esc] <> 'DO NOT USE'", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _tempWoList = WorkOrder.GetWorkOrderList(reader.GetString(0), sqlCon);
                                    if (_tempWoList.Count > 0)
                                    {
                                        _tempList.Add(new Machine
                                        {
                                            MachineNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                            IsLoaded = true
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return _tempList;
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
    }
}
