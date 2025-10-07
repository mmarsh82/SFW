using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.SupplyChain
{
    public class WorkPlan : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            var _conString = @"SELECT * FROM dbo.[SFW_PlannerView] WHERE [Site] = @p1 ORDER BY MachineOrder, MachineNumber, WO_Priority, Sched_Shift, Sched_Priority, WO_SchedStartDate, WorkOrderID ASC";
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

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public WorkPlan()
        { }

        /// <summary>
        /// Get a collection of planners
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<string> GetPlannerCollection()
        {
            try
            {
                var _rtnCol = new ObservableCollection<string> { "All" };
                if (MasterDataSet.Tables.Contains(typeof(WorkPlan).Name) && MasterDataSet.Tables[typeof(WorkPlan).Name].Rows.Count > 0)
                {
                    var _results = MasterDataSet.Tables[typeof(WorkPlan).Name].AsDataView().ToTable(true, "PlannerName");
                    foreach (DataRow _result in _results.Rows)
                    {
                        _rtnCol.Add(_result.Field<string>("PlannerName"));
                    }
                }
                return _rtnCol;
            }
            catch
            {
                return new ObservableCollection<string> { "All" };
            }
        }
    }
}
