using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Quality
{
    public class Defect : ModelBase, IModuleData
    {
        #region Properties

        public int Id { get; set; }
        public string Description { get; set; }
        public string ToolTip { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Load a table with all the NCR defect type information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>An ObservableCollection of NCR defect types</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_SubType] WHERE [Status] = 'Active'", sqlCon))
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
        public Defect()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        public Defect(int dType, string dDescrip, string toolTip)
        {
            Id = dType;
            Description = dDescrip;
            ToolTip = toolTip;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect type information
        /// </summary>
        /// <returns>An ObservableCollection of NCR defect types</returns>
        public static ObservableCollection<Defect> GetCollection()
        {
            var _rtnColl = new ObservableCollection<Defect>();
            foreach (DataRow _row in MasterDataSet.Tables[typeof(Defect).Name].Rows)
            {
                if (_rtnColl.Count(o => o.Id == _row.SafeGetField<int>("ID")) == 0)
                {
                    _rtnColl.Add(new Defect(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), _row.SafeGetField<string>("ToolTip")));
                }
            }
            return _rtnColl;
        }
    }
}
