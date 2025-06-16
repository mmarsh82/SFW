using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model.Quality
{
    public class Disposition : ModelBase, IModuleData
    {
        #region Properties

        public int Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Load a table with all the NCR disposition information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>An ObservableCollection of NCR dispositions</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Disposition] WHERE [ID] <> 7", sqlCon))
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
        public Disposition()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        public Disposition(int dType, string dDescrip, string status)
        {
            Id = dType;
            Description = dDescrip;
            Status = status;
        }

        /// <summary>
        /// Load an observable collection with all the NCR disposition information
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>An ObservableCollection of NCR dispositions</returns>
        public static ObservableCollection<Disposition> GetDispositionCollection()
        {
            var _rtnColl = new ObservableCollection<Disposition>();
            foreach (DataRow _row in MasterDataSet.Tables[typeof(Disposition).Name].Rows)
            {
                _rtnColl.Add(new Disposition(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), _row.SafeGetField<string>("FormStatus")));
            }
            return _rtnColl;
        }
    }
}
