using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Quality
{
    public class Reason : ModelBase, IModuleData
    {
        #region Properties

        public int Id { get; set; }
        public string Description { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Load an observable collection with all the NCR defect reason information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>An ObservableCollection of NCR reason types</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Reason] WHERE [Status] = 'Active'", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
                    }
                    catch (SqlException)
                    {
                        return _dt;
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
        public Reason()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        public Reason(int dReason, string dDescrip)
        {
            Id = dReason;
            Description = dDescrip;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect reason information
        /// </summary>
        /// <returns>An ObservableCollection of NCR reason types</returns>
        public static ObservableCollection<Reason> GetReasonCollection()
        {
            var _rtnColl = new ObservableCollection<Reason>();
            foreach (DataRow _row in MasterDataSet.Tables[typeof(Reason).Name].Rows)
            {
                _rtnColl.Add(new Reason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
            }
            return _rtnColl;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect reason information
        /// </summary>
        /// <returns>An ObservableCollection of NCR reason types</returns>
        public static ObservableCollection<Reason> GetReasonCollection(int typeId, int subTypeId)
        {
            var _rtnColl = new ObservableCollection<Reason>();
            foreach (DataRow _cRow in MasterDataSet.Tables[typeof(CategoryFilter).Name].Select($"[TypeId] = {typeId} AND [SubTypeId] = {subTypeId}"))
            {
                foreach (DataRow _row in MasterDataSet.Tables[typeof(Reason).Name].Select($"[ID]={_cRow.SafeGetField<int>("ReasonId")}"))
                {
                    if (_rtnColl.Count(o => o.Id == _row.SafeGetField<int>("ID")) == 0)
                    {
                        _rtnColl.Add(new Reason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
                    }
                }
            }
            return _rtnColl;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect reason information
        /// </summary>
        /// <returns>An ObservableCollection of NCR reason types</returns>
        public static ObservableCollection<Reason> GetReasonCollection(char supplierCategory)
        {
            var _rtnColl = new ObservableCollection<Reason>();
            var _reason = supplierCategory == 'E' ? 2 : 3;
            var _row = MasterDataSet.Tables[typeof(Reason).Name].Select($"[ID] = {_reason}").FirstOrDefault();
            _rtnColl.Add(new Reason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
            return _rtnColl;
        }
    }
}
