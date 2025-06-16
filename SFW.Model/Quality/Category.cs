using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Quality
{
    public class Category : IModuleData
    {
        #region Properties

        public int Id { get; set; }
        public string Description { get; set; }
        public FormType QmsFormType { get; set; }

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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Type]", sqlCon))
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
        public Category()
        { }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        public Category(int dType, string dDescrip, FormType formType)
        {
            Id = dType;
            Description = dDescrip;
            QmsFormType = formType;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect type information
        /// </summary>
        /// <returns>An ObservableCollection of NCR defect types</returns>
        public static ObservableCollection<Category> GetCategoryCollection()
        {
            var _rtnColl = new ObservableCollection<Category>();
            foreach (DataRow _row in ModelBase.MasterDataSet.Tables[typeof(Category).Name].Rows)
            {
                _rtnColl.Add(new Category(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), Enum.TryParse(_row.SafeGetField<string>("FormType"), out FormType ft) ? ft : FormType.NCR));
            }
            return _rtnColl;
        }

        /// <summary>
        /// Load an observable collection with all the NCR defect type information
        /// </summary>
        /// <returns>An ObservableCollection of NCR defect types</returns>
        public static ObservableCollection<Category> GetCategoryCollection(int subTypeId)
        {
            var _rtnColl = new ObservableCollection<Category>();
            foreach (DataRow _cRow in ModelBase.MasterDataSet.Tables["CategoryFilter"].Select($"[SubTypeId] = {subTypeId}"))
            {
                foreach (DataRow _tRow in ModelBase.MasterDataSet.Tables[typeof(Category).Name].Select($"[ID]={_cRow.SafeGetField<int>("TypeId")}"))
                {
                    if (_rtnColl.Count(o => o.Id == _tRow.SafeGetField<int>("ID")) == 0)
                    {
                        _rtnColl.Add(new Category(_tRow.SafeGetField<int>("ID"), _tRow.SafeGetField<string>("Description"), Enum.TryParse(_tRow.SafeGetField<string>("FormType"), out FormType ft) ? ft : FormType.NCR));
                    }
                }
            }
            return _rtnColl;
        }
    }
}
