using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Quality
{
    public class Supplier : ModelBase, IModuleData
    {
        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public char Classification { get; set; }

        #endregion

        #region Data access

        /// <summary>
        /// Load a table of suppliers
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of supplier information</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
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

        /// <summary>
        /// Get a supplier ID by product and work order IDs's
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Supplier ID as int</returns>
        public static int GetSupplierID(string orderId, string productId, SqlConnection sqlCon)
        {
            var _rtnVal = 0;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _type = string.Empty;
                    using (SqlCommand _cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT im.[Class_Data] FROM [dbo].[IM-INIT] im WHERE im.[Part_Number] = @p1", sqlCon))
                    {
                        _cmd.Parameters.AddWithValue("p1", productId);
                        _type = _cmd.ExecuteScalar().ToString();
                    }
                    if (_type != "RR" || _type != "RC")
                    {
                        var _partList = new List<string> { productId };
                        while (_rtnVal == 0)
                        {
                            using (SqlCommand _cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT
	im.[Part_Number] as 'ProductID'
	,im.[Class_Data] as 'Type'
    ,im.[Prod_Code] as 'Code'
FROM
	[dbo].[PS-INIT] ps
LEFT JOIN
	[dbo].[IM-INIT] im ON im.[Part_Number] = SUBSTRING(ps.[ID], CHARINDEX('*', ps.[ID])+1, LEN(ps.[ID]))
WHERE
	 im.[Class_Data] <> 'LA'", sqlCon))
                            {
                                if (_partList.Count == 1)
                                {
                                    _cmd.CommandText += " AND ps.[ID] LIKE CONCAT(@p1, '*%')";
                                    _cmd.Parameters.AddWithValue("p1", _partList[0]);
                                }
                                else
                                {
                                    foreach (var _part in _partList)
                                    {
                                        var _index = _partList.IndexOf(_part);
                                        _cmd.CommandText += $" AND ps.[ID] LIKE CONCAT(@p{_index}, '*%')";
                                        _cmd.Parameters.AddWithValue($"p{_index}", _part);
                                    }
                                }
                                _partList.Clear();
                                using (SqlDataReader _reader = _cmd.ExecuteReader())
                                {
                                    if (_reader.HasRows)
                                    {
                                        while (_reader.Read())
                                        {
                                            _type = _reader.SafeGetString("Type");
                                            var _code = _reader.SafeGetString("Code");
                                            if (_type == "RR" || _type == "RC" || _code == "P")
                                            {
                                                productId = _reader.SafeGetString("ProductID");
                                                _rtnVal = 1;
                                                break;
                                            }
                                            else
                                            {
                                                _partList.Add(_reader.SafeGetString("ProductID"));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return _rtnVal;
                                    }    
                                }
                            }
                        }
                    }
                    using (SqlCommand _cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT TOP 1 immi.[ID2] FROM [IM-INIT_Mfgr_Info] immi WHERE immi.[ID1] = @p1 AND immi.[Primary_Vendor_Nbr] = 'P'", sqlCon))
                    {
                        _cmd.Parameters.AddWithValue("p1", productId);
                        _rtnVal = int.TryParse(_cmd.ExecuteScalar()?.ToString(), out int i) ? i : 0;
                    }
                    return _rtnVal;
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
                var _row = MasterDataSet.Tables[typeof(Supplier).Name].Select($"[SupplierId] = '{id}'").FirstOrDefault();
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
            foreach (DataRow _row in MasterDataSet.Tables[typeof(Supplier).Name].Rows)
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
