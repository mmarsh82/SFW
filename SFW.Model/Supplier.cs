using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Supplier : ModelBase
    {
        public class Contact
        {
            #region Properties

            public int ContactId { get; set; }
            public string FullName { get; set; }
            public string email { get; set; }

            #endregion

            /// <summary>
            /// Default constructor
            /// </summary>
            public Contact()
            { }
        }

        #region Properties

        public int SupplierId { get; set; }
        public string Name { get; set; }
        public List<Contact> ContactList { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public Supplier()
        { }

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
        /// <returns>A list of suppliers</returns>
        public static List<Supplier> GetSupplierList()
        {
            var _rtnList = new List<Supplier>();
            foreach (DataRow _row in MasterDataSet.Tables["Supplier"].Rows)
            {
                var _supId = _row.SafeGetField<int>("SupplierId");
                var _conId = _row.SafeGetField<int>("ContactId");
                if (_rtnList.Count(o => o.SupplierId == _supId) > 0)
                {
                    if (_conId > 0)
                    {
                        _rtnList.FirstOrDefault(o => o.SupplierId == _supId).ContactList.Add(new Contact
                        {
                            ContactId = _conId
                            ,FullName = _row.SafeGetField<string>("ContactFullName")
                            ,email = _row.SafeGetField<string>("ContactEmail")
                        });
                    }
                }
                else
                {
                    _rtnList.Add(new Supplier
                    {
                        SupplierId = _supId
                        ,Name = _row.SafeGetField<string>("SupplierName")
                        ,ContactList = new List<Contact>()
                    });
                    _rtnList.Last().ContactList.Add(new Contact
                    {
                        ContactId = _conId
                        ,FullName = _row.SafeGetField<string>("ContactFullName")
                        ,email = _row.SafeGetField<string>("ContactEmail")
                    });
                }
            }
            return _rtnList;
        }
    }
}
