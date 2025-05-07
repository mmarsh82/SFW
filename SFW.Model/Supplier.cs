using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Supplier : ModelBase
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public char Classification { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public Supplier()
        { }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        public Supplier(int id)
        {
            if (id > 0)
            {
                var _row = MasterDataSet.Tables["Supplier"].Select($"[SupplierId] = '{id}'").FirstOrDefault();
                Id = id;
                Name = _row.SafeGetField<string>("SupplierName");
                ContactId = _row.SafeGetField<int>("ContactId");
                ContactName = _row.SafeGetField<string>("ContactFullName");
                ContactEmail = _row.SafeGetField<string>("ContactEmail");
                Classification = _row.SafeGetField<string>("Type").FirstOrDefault();
            }
            else
            {
                Id = 0;
                Name = "None";
                ContactId = 0;
                ContactName = "Contact Empty";
                ContactEmail = "Not on file";
                Classification = 'N';
            }
        }

        #region Data access

        /// <summary>
        /// Load a table of suppliers
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of supplier information</returns>
        public static DataTable GetSupplierTable(SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable _dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_Suppliers]", sqlCon))
                        {
                            adapter.Fill(_dt);
                            return _dt;
                        }
                    }
                }
                catch (SqlException)
                {
                    return null;
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
        /// Load a list of suppliers
        /// </summary>
        /// <param name="addAll">Add an all to the beginning of the list</param>
        /// <returns>A list of suppliers</returns>
        public static List<Supplier> GetSupplierList(bool addNone)
        {
            var _rtnList = new List<Supplier>();
            if (addNone)
            {
                _rtnList.Add(new Supplier
                {
                    Id = 0
                    ,Name = "None"
                    ,ContactId = 0
                    ,ContactName = "Contact Empty"
                    ,ContactEmail = "Not on file"
                    ,Classification = 'N'
                });
            }
            foreach (DataRow _row in MasterDataSet.Tables["Supplier"].Rows)
            {
                _rtnList.Add(new Supplier
                {
                    Id = _row.SafeGetField<int>("SupplierId")
                    ,Name = _row.SafeGetField<string>("SupplierName")
                    ,ContactId = _row.SafeGetField<int>("ContactId")
                    ,ContactName = _row.SafeGetField<string>("ContactFullName")
                    ,ContactEmail = _row.SafeGetField<string>("ContactEmail")
                    ,Classification = _row.SafeGetField<string>("Type").FirstOrDefault()
                });
            }
            return _rtnList;
        }
    }
}
