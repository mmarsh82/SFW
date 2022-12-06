using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Model
{
    public class XMLTesting : ModelBase
    {
        public DataSet SfwDataSet { get; set; }
        public IList<string> DataTableList { get; set; }
        public SqlDependency SfwDataDependency { get; set; }

        public XMLTesting()
        {
            if (DataTableList == null)
            {
                var _nameList = GetTableNames();
                if (_nameList[0].Contains("Exception"))
                {
                    //TODO: Need to add in error handling here
                    return;
                }
                DataTableList = GetTableNames();
            }
            if (SfwDataDependency == null)
            {

            }
            BuildXmlDataSet();
        }

        public void BuildXmlDataSet()
        {
            SfwDataSet = new DataSet();
            foreach (var _table in DataTableList)
            {

            }
        }

        /// <summary>
        /// Gets a list of tables names related to the SFW application
        /// </summary>
        /// <returns>List of names as strings</returns>
        public IList<string> GetTableNames()
        {
            var _rtnList = new List<string>();
            if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand command = new SqlCommand($"USE {ModelSqlCon.Database}; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME LIKE 'SFW_%'", ModelSqlCon))
                    {
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                _rtnList.Add(reader.GetString(0));
                            }
                        }
                    }
                    return _rtnList;
                }
                catch (SqlException sqlEx)
                {
                    _rtnList.Add("SQL Exception");
                    _rtnList.Add(sqlEx.Message);
                    _rtnList.Add(sqlEx.StackTrace);
                    return _rtnList;
                }
                catch (Exception ex)
                {
                    _rtnList.Add("Unhandled Exception");
                    _rtnList.Add(ex.Message);
                    _rtnList.Add(ex.StackTrace);
                    return _rtnList;
                }
            }
            else
            {
                _rtnList.Add("Connection Exception");
                _rtnList.Add("A connection could not be made to pull accurate data, please contact your administrator");
                _rtnList.Add("GetTableNames Method");
                return _rtnList;
            }
        }

        /// <summary>
        /// Populate a single table in the main dataset
        /// </summary>
        /// <returns>DataTable of bill of materials</returns>
        public static DataTable GetSfwTables(string tableName)
        {
            using (var _tempTable = new DataTable())
            {
                if (ModelSqlCon != null && ModelSqlCon.State != ConnectionState.Closed && ModelSqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {ModelSqlCon.Database}; SELECT * FROM [dbo].[SFW_{tableName}]", ModelSqlCon))
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
    }
}
