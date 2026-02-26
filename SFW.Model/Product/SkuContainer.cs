using SFW.Model.Sales;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace SFW.Model.Product
{
    public partial class SkuContainer : ModelBase, IModuleData
    {
        public class Product : ModelBase
        {
            #region Properties

            private string _parentid;
            public string ParentId
            {
                get
                { return _parentid; }
                set
                {
                    _parentid = value;
                    OnPropertyChanged(nameof(ParentId));
                }
            }

            private string _userid;
            public string UserId
            {
                get
                { return _userid; }
                set
                {
                    _userid = value;
                    OnPropertyChanged(nameof(UserId));
                }
            }

            private string _id;
            public string ProductId
            {
                get
                { return _id; }
                set
                {
                    _id = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }

            private string _desc;
            public string ProductDescription
            {
                get
                { return _desc; }
                set
                {
                    _desc = value;
                    OnPropertyChanged(nameof(ProductDescription));
                }
            }

            private string _input;
            public string ProductInput
            {
                get
                { return _input; }
                set
                {
                    _input = value;
                    if (Sku.Exists(value, false))
                    {
                        var _sku = new Sku(value);
                        if (_sku.IsLotTrace)
                        {
                            MessageBox.Show("Product is lot traceable and the part number was entered and not the lot number.\nPlease try again with the lot number.", "Lot traceable product", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                            _input = string.Empty;
                        }
                        else if (GetProductCount(ParentId, 'P', value, ModelSqlCon) > 0)
                        {
                            MessageBox.Show("Product is already listed in the container.\nPlease update the quantity of the original product.", "Duplicate product", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                            _input = string.Empty;
                        }
                        else
                        {
                            ProductId = $"{_sku.SkuNumber}|01";
                            ProductDescription = _sku.SkuDescription;
                            LotTraceable = false;
                            Validated = true;
                            NewProduct = true;
                            _input = string.Empty;
                        }
                    }
                    else if (Lot.IsValid($"{value}|P|01", ModelSqlCon) && value.Length > 3)
                    {
                        if (GetProductCount(ParentId, 'L', value, ModelSqlCon) > 0)
                        {
                            MessageBox.Show("Product is already listed in the container.", "Duplicate product", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                            _input = string.Empty;
                        }
                        else
                        {
                            var _sku = new Sku(Lot.GetSkuNumber(value));
                            if (_sku != null && !string.IsNullOrEmpty(_sku.SkuNumber))
                            {
                                if (!_sku.SkuNumber.Contains("|"))
                                {
                                    _sku.SkuNumber += "|01";
                                }
                                MultipleLots = Lot.CheckMultiple(value);
                                ProductId = _sku.SkuNumber;
                                ProductDescription = _sku.SkuDescription;
                                Quantity = Lot.GetOnHandQuantity(value);
                                LocationInput = !NewContainer ? GetContainerLocation(int.Parse(ParentId), ModelSqlCon) : Lot.GetLocation(value);
                                Location = Lot.GetLocation(value);
                                QuantityInput = Quantity.ToString();
                                LotTraceable = true;
                                LotId = value;
                                Validated = true;
                                NewProduct = true;
                                _input = MultipleLots ? _input : string.Empty;
                                if (Exists(ProductId, ParentId) > 0)
                                {
                                    var _order = GetSalesOrder(ProductId, ParentId);
                                    if (!string.IsNullOrEmpty(_order))
                                    {
                                        SalesOrderNumber = _order.Split('*')[0];
                                        SalesLineNumber = _order.Split('*')[1];
                                    }
                                }
                            }
                            else if (value.Length >= 10)
                            {
                                MessageBox.Show("The lot number you entered is no longer on file.\nPlease contact IT for further assistance.", "Lot not on file", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                                _input = string.Empty;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(ProductInput));
                }
            }

            private int _qty;
            public int Quantity
            {
                get
                { return _qty; }
                set
                {
                    _qty = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }

            private int _qtyIn;
            public string QuantityInput
            {
                get
                { return _qtyIn > 0 ? _qtyIn.ToString() : ""; }
                set
                {
                    _qtyIn = int.TryParse(value, out int i) ? i : 0;
                    ValidQuantity = false;
                    OnPropertyChanged(nameof(QuantityInput));
                    OnPropertyChanged(nameof(QuantityChange));
                }
            }
            public bool QuantityChange
            {
                get
                {
                    var _qty = int.TryParse(QuantityInput, out int i) ? i : Quantity;
                    return Quantity != _qty && !NewProduct && !NewContainer;
                }
            }

            private bool _valQty;
            public bool ValidQuantity
            {
                get
                { return _valQty; }
                set
                {
                    _valQty = int.TryParse(QuantityInput, out int i) && i > 0;
                    OnPropertyChanged(nameof(ValidQuantity));
                }
            }

            private bool _mLot;
            public bool MultipleLots
            {
                get
                { return _mLot; }
                set
                {
                    _mLot = value;
                    OnPropertyChanged(nameof(MultipleLots));
                }
            }

            private bool _trace;
            public bool LotTraceable
            {
                get
                { return _trace; }
                set
                {
                    _trace = value;
                    OnPropertyChanged(nameof(LotTraceable));
                }
            }

            private string _lot;
            public string LotId
            {
                get
                { return _lot; }
                set
                {
                    _lot = value;
                    OnPropertyChanged(nameof(LotId));
                }
            }

            private string _loc;
            public string Location
            {
                get
                { return _loc; }
                set
                {
                    _loc = value;
                    OnPropertyChanged(nameof(Location));
                }
            }

            private bool _valid;
            public bool Validated
            {
                get
                { return _valid; }
                set
                {
                    _valid = value;
                    OnPropertyChanged(nameof(Validated));
                }
            }

            private bool _newProd;
            public bool NewProduct
            {
                get
                { return _newProd; }
                set
                {
                    _newProd = value;
                    OnPropertyChanged(nameof(NewProduct));
                }
            }

            private bool _newCon;
            public bool NewContainer
            {
                get
                { return _newCon; }
                set
                {
                    _newCon = value;
                    OnPropertyChanged(nameof(NewContainer));
                }
            }

            private string _locIn;
            public string LocationInput
            {
                get
                { return _locIn; }
                set
                {
                    _locIn = value;
                    ValidLocation = Production.Location.Valid(value, 1);
                    OnPropertyChanged(nameof(LocationInput));
                }
            }

            private bool _valLoc;
            public bool ValidLocation
            {
                get
                { return _valLoc; }
                set
                {
                    _valLoc = value;
                    OnPropertyChanged(nameof(ValidLocation));
                }
            }

            private string _soNbr;
            public string SalesOrderNumber
            {
                get 
                { return _soNbr; }
                set
                {
                    _soNbr = value;
                    if (string.IsNullOrEmpty(SalesLineNumber))
                    {
                        var _line = SalesOrder.GetLineNumber(value, ProductId);
                        if (_line != 0)
                        {
                            SalesLineNumber = _line.ToString();
                            ValidSalesOrder = true;
                            var _cust = SalesOrder.GetCustomer(value);
                            CustomerNumber = _cust.Contains('*') ? _cust.Split('*')[0] : "";
                            OnPropertyChanged(nameof(CustomerNumber));
                            CustomerName = _cust.Contains('*') ? _cust.Split('*')[1] : "";
                            OnPropertyChanged(nameof(CustomerName));
                            CustomerProductID = SalesOrder.GetCustomerProductID(value, SalesLineNumber);
                        }
                        else
                        {
                            ValidSalesOrder = false;
                        }
                    }
                    else
                    {
                        ValidSalesOrder = false;
                    }
                    OnPropertyChanged(nameof(SalesOrderNumber));
                }
            }
            public string Customer { get; set; }

            private bool _valSo;
            public bool ValidSalesOrder
            {
                get
                { return _valSo; }
                set
                {
                    _valSo = value;
                    OnPropertyChanged(nameof(ValidSalesOrder));
                }
            }

            private int? _lnNbr;
            public string SalesLineNumber
            {
                get
                { return int.TryParse(_lnNbr.ToString(), out int i) ? i.ToString() : null; }
                set
                {
                    if(int.TryParse(value, out int i))
                    {
                        _lnNbr = i;
                    }
                    else
                    {
                        _lnNbr = null;
                    }
                    OnPropertyChanged(nameof(SalesLineNumber));
                }
            }

            public string CustomerNumber { get; set; }
            public string CustomerName { get; set; }
            public string CustomerProductID { get; set; }

            #endregion

            /// <summary>
            /// Default constructor
            /// </summary>
            public Product(string parentId, string userId, bool newProd, bool newCont)
            {
                ParentId = parentId;
                UserId = userId;
                Validated = ValidLocation = !newProd;
                NewProduct = newProd;
                NewContainer = newCont;
                Location = "Soon";
            }

            /// <summary>
            /// Check to see if a container already containers a product
            /// </summary>
            /// <param name="productId">Product ID</param>
            /// <param name="containerId">container ID</param>
            /// <returns>Number of the same products in the container</returns>
            public int Exists(string productId, string containerId)
            {
                return !string.IsNullOrEmpty(containerId)
                    ? MasterDataSet.Tables[typeof(SkuContainer).Name].Select($"[ProductId] = '{productId}' AND [ContainerID] = '{containerId}'").Count()
                    : 0;
            }

            /// <summary>
            /// Check to see if a container already containers a product
            /// </summary>
            /// <param name="productId">Product ID</param>
            /// <param name="containerId">container ID</param>
            /// <returns>Number of the same products in the container</returns>
            public static string GetSalesOrder(string productId, string containerId)
            {
                var _rows = MasterDataSet.Tables[typeof(SkuContainer).Name].Select($"[ProductId] = '{productId}' AND [ContainerID] = '{containerId}'");
                if (_rows.Count(o => !string.IsNullOrEmpty(o.Field<string>("SalesOrderNumber"))) > 0)
                {
                    var _row = _rows.FirstOrDefault(o => !string.IsNullOrEmpty(o.Field<string>("SalesOrderNumber")));
                    return $"{_row.Field<string>("SalesOrderNumber")}*{_row.Field<string>("SalesLineNumber")}";
                }
                return string.Empty;
            }
        }

        public class TableChanges
        {
            #region Properties

            public DataRowAction Action { get; set; }
            public DataRow TableRow { get; set; }

            #endregion
        }

        #region Properties

        private int _id;
        public string ContainerId
        {
            get
            { return _id > 0 ? _id.ToString() : string.Empty; }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _id = i;
                }
                OnPropertyChanged(nameof(ContainerId));
            }
        }

        private string _loc;
        public string Location
        {
            get
            { return _loc; }
            set
            {
                _loc = value;
                ValidLocation = Production.Location.Valid(value, 1);
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(LocationChange));
            }
        }
        public string LoadedLocation;

        private bool _valLoc;
        public bool ValidLocation
        {
            get
            { return _valLoc; }
            set
            {
                _valLoc = value;
                OnPropertyChanged(nameof(ValidLocation));
            }
        }

        public bool LocationChange
        {
            get
            { return Location != LoadedLocation && ValidLocation; }
        }

        private string _userId;
        public string UserId
        {
            get
            { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        private string _userName;
        public string UserName
        {
            get
            { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private DateTime _revDate;
        public DateTime RevisionDateTime
        {
            get
            { return _revDate; }
            set
            {
                _revDate = value;
                OnPropertyChanged(nameof(RevisionDateTime));
            }
        }

        private int? _weight;
        public string Weight
        {
            get
            { return _weight.ToString(); }
            set
            {
                if(int.TryParse(value, out int i))
                {
                    _weight = i;
                }
                else
                {
                    _weight = null;
                }
                OnPropertyChanged(nameof(Weight));
            }
        }

        private int? _lgt;
        public string Length
        {
            get
            { return _lgt.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _lgt = i;
                }
                else
                {
                    _lgt = null;
                }
                OnPropertyChanged(nameof(Length));
            }
        }

        private int? _hgt;
        public string Height
        {
            get
            { return _hgt.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _hgt = i;
                }
                else
                {
                    _hgt = null;
                }
                OnPropertyChanged(nameof(Height));
            }
        }

        private int? _dpt;
        public string Depth
        {
            get
            { return _dpt.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _dpt = i;
                }
                else
                {
                    _dpt = null;
                }
                OnPropertyChanged(nameof(Depth));
            }
        }

        private string _status;
        public string Status
        {
            get
            { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private string _palType;
        public string PalletType
        {
            get
            { return _palType; }
            set
            {
                _palType = value;
                OnPropertyChanged(nameof(PalletType));
            }
        }

        public ObservableCollection<Product> ProductCollection { get; set; }

        #endregion

        #region Data Access

        /// <summary>
        /// Get the container data
        /// </summary>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM dbo.[SFW_Containers]", sqlCon))
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

        /// <summary>
        /// Get the container pallet data
        /// </summary>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DataTable GetPalletTable(SqlConnection sqlCon)
        {
            var _tempTable = new DataTable();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [Nexus_Main].[dbo].[ContainerTypes]", sqlCon))
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

        /// <summary>
        /// Get the container pallet data as a list
        /// </summary>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IReadOnlyDictionary<string, string> GetPalletDictionary(SqlConnection sqlCon)
        {
            var _rtnDict = new Dictionary<string, string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT ct.[ContainerType], ct.[ContainerTypeDesc] FROM [Nexus_Main].[dbo].[ContainerTypes] ct WHERE ct.[Enabled] = 1", sqlCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _rtnDict.Add(reader.SafeGetString("ContainerType"), reader.SafeGetString("ContainerTypeDesc"));
                                }
                            }
                        }
                    }
                    return _rtnDict;
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

        /// <summary>
        /// Commit any changes to the container pallet table
        /// </summary>
        /// <param name="changes">List of changes and the data to be changed</param>
        /// <param name="sqlCon">Application SQL connection</param>
        public static void ChangePalletTable(List<TableChanges> changes, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _cmdString = string.Empty;
                    var _parameters = new Dictionary<int, string>();
                    var _columnNumber = 0;
                    foreach (var _change in changes)
                    {
                        switch (_change.Action)
                        {
                            //SQL INSERT logic for the local database
                            case DataRowAction.Add:
                                _cmdString += @"INSERT INTO [Nexus_Main].[dbo].[ContainerTypes] ";
                                var _cols = "(";
                                var _values = " VALUES (";
                                foreach (var _colVal in _change.TableRow.ItemArray)
                                {
                                    if (_colVal != null && !string.IsNullOrEmpty(_colVal.ToString()))
                                    {
                                        _values += $"@p{_parameters.Count}, ";
                                        _parameters.Add(_parameters.Count, _colVal.ToString());
                                        _cols += $"[{_change.TableRow.Table.Columns[_columnNumber]}], ";
                                    }
                                    _columnNumber++;
                                }
                                _cols = _cols.TrimEnd(' ', ',');
                                _cols += ")";
                                _values = _values.TrimEnd(' ', ',');
                                _values += ");";
                                _cmdString = _cmdString + _cols + _values;
                                _columnNumber = 0;
                                break;

                            //SQL UPDATE logic for the local database
                            case DataRowAction.Change:
                                _cmdString += @"UPDATE [Nexus_Main].dbo.[ContainerTypes] SET ";
                                foreach (var _colVal in _change.TableRow.ItemArray)
                                {
                                    if (_colVal != null && _columnNumber != 0 && !string.IsNullOrEmpty(_colVal.ToString()))
                                    {
                                        _cmdString += $"[{_change.TableRow.Table.Columns[_columnNumber]}] = @p{_parameters.Count}, ";
                                        _parameters.Add(_parameters.Count, _colVal.ToString());
                                    }
                                    _columnNumber++;
                                }
                                _cmdString = _cmdString.TrimEnd(' ', ',');
                                _cmdString += $" WHERE [ContainerType] = @p{_parameters.Count};";
                                _parameters.Add(_parameters.Count, _change.TableRow.ItemArray.GetValue(0).ToString());
                                _columnNumber = 0;
                                break;

                            //SQL DELETE logic for the local database
                            case DataRowAction.Delete:
                                var _pk = _change.TableRow.Table.PrimaryKey[0].ColumnName;
                                _cmdString += $@"DELETE FROM [Nexus_Main].[dbo].[ContainerTypes] WHERE [{_pk}] = @p{_parameters.Count};";
                                _parameters.Add(_parameters.Count, _change.TableRow.RowError);
                                break;
                        }
                    }
                    using (SqlCommand _sqlCmd = new SqlCommand(_cmdString, sqlCon))
                    {
                        foreach (var _param in _parameters)
                        {
                            _sqlCmd.Parameters.AddWithValue($"p{_param.Key}", _param.Value);
                        }
                        _sqlCmd.ExecuteNonQuery();
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

        /// <summary>
        /// Get a container's data
        /// </summary>
        /// <param name="id">Id of the container</param>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SkuContainer GetContainer(int id, SqlConnection sqlCon)
        {
            var _tempCon = new SkuContainer() { ContainerId = id.ToString(), ProductCollection = new ObservableCollection<Product>() };
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT * FROM dbo.[SFW_Containers] WHERE [ContainerID] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (string.IsNullOrEmpty(_tempCon.Location))
                                    {
                                        _tempCon.Location = _tempCon.LoadedLocation = reader.SafeGetString("ContainerLocation");
                                        _tempCon.Weight = reader.SafeGetInt32("Weight").ToString();
                                        _tempCon.Height = reader.SafeGetInt32("Height").ToString();
                                        _tempCon.Length = reader.SafeGetInt32("Length").ToString();
                                        _tempCon.Depth = reader.SafeGetInt32("Depth").ToString();
                                        _tempCon.PalletType = reader.SafeGetString("ContainerType");
                                    }
                                    if (string.IsNullOrEmpty(_tempCon.UserId))
                                    {
                                        _tempCon.UserId = reader.SafeGetString("LastUserID");
                                        _tempCon.UserName = reader.SafeGetString("LastUserName");
                                        _tempCon.RevisionDateTime = DateTime.TryParse(reader.SafeGetString("LastEditDate"), out DateTime dt) ? dt : DateTime.Now;
                                    }
                                    var _prod = new Product(_tempCon.ContainerId, _tempCon.UserId, false, false)
                                    {
                                        ProductId = reader.SafeGetString("ProductId")
                                        ,ProductDescription = reader.SafeGetString("ProductDescription")
                                        ,Quantity = reader.SafeGetInt32("Quantity")
                                        ,LotTraceable = reader.SafeGetString("LotTraceable") == "T"
                                        ,QuantityInput = reader.SafeGetInt32("Quantity").ToString()
                                        ,ValidQuantity = true
                                        ,SalesOrderNumber = reader.SafeGetString("SalesOrderNumber")
                                        ,SalesLineNumber = reader.SafeGetString("SalesLineNumber")
                                    };
                                    if (_prod.LotTraceable)
                                    {
                                        _prod.LotId = reader.SafeGetString("LotId");
                                        _prod.Location = Lot.GetLocation(reader.SafeGetString("LotId"));
                                    }
                                    _tempCon.ProductCollection.Add(_prod);
                                }
                            }
                        }
                    }
                    return _tempCon;
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

        /// <summary>
        /// Get a containers product count
        /// </summary>
        /// <param name="ctnId">Container ID</param>
        /// <param name="type">Type to search as char. P for part and L for lot</param>
        /// <param name="prodId">Product ID</param>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int GetProductCount(string ctnId, char type, string prodId, SqlConnection sqlCon)
        {
            var _colName = type == 'P' ? "ProductId" : "LotId";
            if (type == 'P' && !prodId.Contains("|"))
            {
                prodId += "|01";
            }
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT COUNT([ContainerID]) FROM dbo.[SFW_Containers] WHERE [ContainerID] = @p1 AND [{_colName}] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ctnId);
                        cmd.Parameters.AddWithValue("p2", prodId);
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
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

        /// <summary>
        /// Get a containers location
        /// </summary>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetContainerLocation(int ctnId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT TOP 1 [ContainerLocation] FROM dbo.[SFW_Containers] WHERE [ContainerID] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ctnId);
                        return cmd.ExecuteScalar()?.ToString();
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

        /// <summary>
        /// Delete a container
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="sqlCon">Application SQL connection</param>
        public static bool Delete(int containerId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [Nexus_Main].dbo.[ContainerHeader] WHERE [ContainerID] = @p1;
DELETE FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1;
DELETE FROM [Nexus_Main].dbo.[ContainerDetailLot] WHERE [ContainerID] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", containerId);
                        return cmd.ExecuteNonQuery() > 0;
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

        /// <summary>
        /// Update a container
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="userId"></param>
        /// <param name="location"></param>
        /// <param name="sqlCon">Application SQL connection</param>
        public static bool Update(int containerId, string userId, string location, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (!string.IsNullOrEmpty(location))
                    {
                        using (SqlCommand cmd = new SqlCommand($@"UPDATE [Nexus_Main].dbo.[ContainerHeader] SET [LastUserID]=@p1, [LastEditDate]=@p2, [XrefID]=@p3, [ContainerLocation]=@p3 WHERE [ContainerID]=@p4", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", userId);
                            cmd.Parameters.AddWithValue("p2", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                            cmd.Parameters.AddWithValue("p3", location);
                            cmd.Parameters.AddWithValue("p4", containerId);
                            return cmd.ExecuteNonQuery() > 0;
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand($@"UPDATE [Nexus_Main].dbo.[ContainerHeader] SET [LastUserID]=@p1, [LastEditDate]=@p2 WHERE [ContainerID]=@p3", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", userId);
                            cmd.Parameters.AddWithValue("p2", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                            cmd.Parameters.AddWithValue("p3", containerId);
                            return cmd.ExecuteNonQuery() > 0;
                        }
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

        /// <summary>
        /// Update a container header information
        /// </summary>
        /// <param name="container">Container object to update</param>
        /// <param name="userId">User ID that is makeing the change</param>
        /// <param name="sqlCon">Application SQL connection</param>
        public static bool UpdateHeader(SkuContainer container, string userId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"UPDATE
    [Nexus_Main].dbo.[ContainerHeader]
SET
    [ContainerType]=@p1, [ContainerStatus]=@p2, [LastUserID]=@p3, [LastEditDate]=@p4, [ContainerWeight]=@p5, [ContainerHeight]=@p6, [User_Def_1]=@p7, [User_Def_2]=@p8
WHERE
    [ContainerID]=@p9", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", container.PalletType);
                        cmd.Parameters.AddWithValue("p2", container.Status);
                        cmd.Parameters.AddWithValue("p3", userId);
                        cmd.Parameters.AddWithValue("p4", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        cmd.Parameters.AddWithValue("p5", container.Weight);
                        cmd.Parameters.AddWithValue("p6", container.Height);
                        cmd.Parameters.AddWithValue("p7", container.Length);
                        cmd.Parameters.AddWithValue("p8", container.Depth);
                        cmd.Parameters.AddWithValue("p9", container.ContainerId);
                        return cmd.ExecuteNonQuery() > 0;
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
        /// Default constructor
        /// </summary>
        public SkuContainer()
        {
            RevisionDateTime = DateTime.Now;
            ProductCollection = new ObservableCollection<Product>();
            Status = "A";
        }

        /// <summary>
        /// Overridden constructor
        /// </summary>
        /// <param name="userId">Current user ERP ID</param>
        /// <param name="userName">Current user full name</param>
        public SkuContainer(string userId, string userName)
        {
            UserId = userId;
            UserName = userName;
            RevisionDateTime = DateTime.Now;
            ProductCollection = new ObservableCollection<Product>();
            Status = "A";
        }
    }

    public static class ProductActions
    {
        #region Data Access

        /// <summary>
        /// Submit a container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="sqlCon"></param>
        public static void Submit(this SkuContainer container, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand($@"DECLARE @newId int
SELECT @newId = CASE WHEN MAX(CAST([ContainerID] as int)) = 0 THEN 1 ELSE MAX(CAST([ContainerID] as int))+1 END FROM [Nexus_Main].[dbo].[ContainerHeader]
INSERT INTO [Nexus_Main].[dbo].[ContainerHeader]
	([ContainerID], [ContainerType], [ContainerStatus], [LastUserID], [LastEditDate], [XrefID], [ContainerWeight], [XrefType], [ContainerHeight], [ContainerLocation], [User_Def_1], [User_Def_2])
VALUES
	(@newId, @p1, @p2, @p3, @p4, @p5, @p6, 'INT', @p7, @p5, @p8, @p9)
SELECT @newId", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", container.PalletType);
                                cmd.Parameters.AddWithValue("p2", container.Status);
                                cmd.Parameters.AddWithValue("p3", container.UserId);
                                cmd.Parameters.AddWithValue("p4", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                cmd.Parameters.AddWithValue("p5", container.Location);
                                if (string.IsNullOrEmpty(container.Weight)) { cmd.Parameters.AddWithValue("p6", DBNull.Value); } else { cmd.Parameters.AddWithValue("p6", container.Weight); }
                                if (string.IsNullOrEmpty(container.Height)) { cmd.Parameters.AddWithValue("p7", DBNull.Value); } else { cmd.Parameters.AddWithValue("p7", container.Height); }
                                if (string.IsNullOrEmpty(container.Length)) { cmd.Parameters.AddWithValue("p8", DBNull.Value); } else { cmd.Parameters.AddWithValue("p8", container.Length); }
                                if (string.IsNullOrEmpty(container.Depth)) { cmd.Parameters.AddWithValue("p9", DBNull.Value); } else { cmd.Parameters.AddWithValue("p9", container.Depth); }
                                container.ContainerId = cmd.ExecuteScalar().ToString();
                            }
                            foreach (var _prod in container.ProductCollection)
                            {
                                _prod.ParentId = container.ContainerId;
                                var _loc = _prod.LocationInput;
                                _prod.LocationInput = container.Location;
                                _prod.Submit(sqlCon);
                                _prod.LocationInput = _loc;
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

        /// <summary>
        /// Delete a container
        /// </summary>
        /// <param name="product">Product object</param>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool Delete(this SkuContainer.Product product, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (product.LotTraceable)
                    {
                        //Get the row ID from the lot table to use in the container detail table
                        var _rowId = 0;
                        using (SqlCommand cmd = new SqlCommand($"SELECT cl.[ContainerRowID] FROM [Nexus_Main].dbo.[ContainerDetailLot] cl WHERE cl.[ContainerID] = @p1 AND cl.[LotNumber] = @p2;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", product.ParentId);
                            cmd.Parameters.AddWithValue("p2", product.LotId);
                            _rowId = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                        }
                        //If a positive integer is return then the row exists and you can continue
                        if (_rowId > 0)
                        {
                            //Get the quantity that will be left over after the update, if the value is 0 then delete the records from both the detail and lot tables
                            var _newQty = 0;
                            using (SqlCommand cmd = new SqlCommand($@"SELECT SUM(cd.[Qty_Stock] - cl.[LotQty]) FROM [Nexus_Main].dbo.[ContainerDetail] cd 
	RIGHT JOIN [Nexus_Main].dbo.[ContainerDetailLot] cl ON cl.[ContainerID] = cd.[ContainerID]
	WHERE cl.[ContainerID] = @p1 AND cl.[LotNumber] = @p2 AND cd.[ContainerRow] = @p3", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", product.ParentId);
                                cmd.Parameters.AddWithValue("p2", product.LotId);
                                cmd.Parameters.AddWithValue("p3", _rowId);
                                _newQty = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 0;
                            }
                            if (_newQty > 0)
                            {
                                using (SqlCommand cmd = new SqlCommand($@"UPDATE [Nexus_Main].dbo.[ContainerDetail] SET [Qty_Stock] = @p1 WHERE [ContainerID] = @p2 AND [ContainerRow] = @p3
DELETE FROM [Nexus_Main].dbo.[ContainerDetailLot] WHERE [ContainerID] = @p2 AND [ContainerRowID] = @p3 AND [LotNumber] = @p4", sqlCon))
                                {
                                    cmd.Parameters.AddWithValue("p1", _newQty);
                                    cmd.Parameters.AddWithValue("p2", product.ParentId);
                                    cmd.Parameters.AddWithValue("p3", _rowId);
                                    cmd.Parameters.AddWithValue("p4", product.LotId);
                                    return int.TryParse(cmd.ExecuteNonQuery().ToString(), out int i) && i > 0;
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1 AND [ContainerRow] = @p2
DELETE FROM [Nexus_Main].dbo.[ContainerDetailLot] WHERE [ContainerID] = @p1 AND [ContainerRowID] = @p2 AND [LotNumber] = @p3", sqlCon))
                                {
                                    cmd.Parameters.AddWithValue("p1", product.ParentId);
                                    cmd.Parameters.AddWithValue("p2", _rowId);
                                    cmd.Parameters.AddWithValue("p3", product.LotId);
                                    return int.TryParse(cmd.ExecuteNonQuery().ToString(), out int i) && i > 0;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1 AND [ItemNumber] = @p2", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", product.ParentId);
                            cmd.Parameters.AddWithValue("p2", product.ProductId);
                            return cmd.ExecuteNonQuery() > 0;
                        }
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

        /// <summary>
        /// Delete a container
        /// </summary>
        /// <param name="product">Product object</param>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool Submit(this SkuContainer.Product product, SqlConnection sqlCon)
        {
            var _revised = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (product.LotTraceable)
                    {
                        var _exists = false;
                        using (SqlCommand cmd = new SqlCommand($@"SELECT CASE WHEN COUNT([ContainerRow]) > 0 THEN 1 ELSE 0 END FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1 AND [ItemNumber] = @p2", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", product.ParentId);
                            cmd.Parameters.AddWithValue("p2", product.ProductId);
                            _exists = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0;
                        }
                        if (_exists)
                        {
                            using (SqlCommand cmd = new SqlCommand($@"DECLARE @rowId int
SELECT @rowId = [ContainerRow] FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1 AND [ItemNumber] = @p2
UPDATE [Nexus_Main].dbo.[ContainerDetail] SET [Qty_Stock] = [Qty_Stock]+@p3, [OrderNumber] = @p5, [LineNumber] = @p6 WHERE [ContainerID] = @p1 AND [ItemNumber] = @p2
INSERT INTO [Nexus_Main].dbo.[ContainerDetailLot] ([ContainerID], [ContainerRowID], [LotNumber], [LotQty]) VALUES(@p1, @rowId, @p4, @p3)", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", product.ParentId);
                                cmd.Parameters.AddWithValue("p2", product.ProductId);
                                cmd.Parameters.AddWithValue("p3", product.Quantity);
                                cmd.Parameters.AddWithValue("p4", product.LotId);
                                cmd.Parameters.AddWithValue("p5", !string.IsNullOrEmpty(product.SalesOrderNumber) ? product.SalesOrderNumber : "");
                                cmd.Parameters.AddWithValue("p6", !string.IsNullOrEmpty(product.SalesLineNumber) ? product.SalesLineNumber : "");
                                _revised = cmd.ExecuteNonQuery() > 0;
                            }
                        }
                        else
                        {
                            using (SqlCommand cmd = new SqlCommand($@"DECLARE @rowId int
SELECT @rowId = CASE WHEN COUNT([ContainerRow]) = 0 THEN 1 ELSE MAX([ContainerRow])+1 END FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1
INSERT INTO [Nexus_Main].dbo.[ContainerDetail] ([ContainerID], [ContainerRow], [ItemNumber], [Qty_Stock], [OrderNumber], [LineNumber]) VALUES(@p1,@rowId,@p2,@p3,@p5,@p6)
INSERT INTO [Nexus_Main].dbo.[ContainerDetailLot] ([ContainerID], [ContainerRowID], [LotNumber], [LotQty]) VALUES(@p1, @rowId, @p4, @p3)", sqlCon))
                            {
                                cmd.Parameters.AddWithValue("p1", product.ParentId);
                                cmd.Parameters.AddWithValue("p2", product.ProductId);
                                cmd.Parameters.AddWithValue("p3", product.Quantity);
                                cmd.Parameters.AddWithValue("p4", product.LotId);
                                cmd.Parameters.AddWithValue("p5", !string.IsNullOrEmpty(product.SalesOrderNumber) ? product.SalesOrderNumber : "");
                                cmd.Parameters.AddWithValue("p6", !string.IsNullOrEmpty(product.SalesLineNumber) ? product.SalesLineNumber : "");
                                _revised = cmd.ExecuteNonQuery() > 0;
                            }
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand($@"DECLARE @rowId int
SELECT @rowId = CASE WHEN COUNT([ContainerRow]) = 0 THEN 1 ELSE MAX([ContainerRow])+1 END FROM [Nexus_Main].dbo.[ContainerDetail] WHERE [ContainerID] = @p1
INSERT INTO [Nexus_Main].dbo.[ContainerDetail] ([ContainerID], [ContainerRow], [ItemNumber], [Qty_Stock], [OrderNumber], [LineNumber]) VALUES (@p1,@rowId,@p2,@p3,@p4,@p5)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", product.ParentId);
                            cmd.Parameters.AddWithValue("p2", product.ProductId);
                            cmd.Parameters.AddWithValue("p3", product.QuantityInput);
                            cmd.Parameters.AddWithValue("p4", !string.IsNullOrEmpty(product.SalesOrderNumber) ? product.SalesOrderNumber : "");
                            cmd.Parameters.AddWithValue("p5", !string.IsNullOrEmpty(product.SalesLineNumber) ? product.SalesLineNumber : "");
                            _revised = cmd.ExecuteNonQuery() > 0;
                        }
                    }
                    if (_revised && !product.NewContainer)
                    {
                        _revised = SkuContainer.Update(int.Parse(product.ParentId), product.UserId, "", sqlCon);
                    }
                    return _revised;
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

        /// <summary>
        /// Update a container
        /// </summary>
        /// <param name="product">Product object</param>
        /// <param name="type">Type of update Q for Quantity, S for sales order</param>
        /// <param name="contId">Container ID</param>
        /// <param name="sqlCon">Application SQL connection</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool Update(this SkuContainer.Product product, char type, string contId, SqlConnection sqlCon)
        {
            var _revised = false;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (type == 'Q')
                    {
                        using (SqlCommand cmd = new SqlCommand($@"UPDATE [Nexus_Main].dbo.[ContainerDetail] SET [Qty_Stock]=@p1 WHERE [ItemNumber]=@p2 AND [ContainerID]=@p3", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", product.QuantityInput);
                            cmd.Parameters.AddWithValue("p2", product.ProductId);
                            cmd.Parameters.AddWithValue("p3", contId);
                            _revised = cmd.ExecuteNonQuery() > 0;
                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand($@"UPDATE [Nexus_Main].dbo.[ContainerDetail] SET [OrderNumber]=@p1, [LineNumber]=@p2 WHERE [ItemNumber]=@p3 AND [ContainerID]=@p4", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", !string.IsNullOrEmpty(product.SalesOrderNumber) ? product.SalesOrderNumber : "");
                            cmd.Parameters.AddWithValue("p2", !string.IsNullOrEmpty(product.SalesLineNumber) ? product.SalesLineNumber : "");
                            cmd.Parameters.AddWithValue("p3", product.ProductId);
                            cmd.Parameters.AddWithValue("p4", contId);
                            _revised = cmd.ExecuteNonQuery() > 0;
                        }
                    }
                    if (_revised)
                    {
                        _revised = SkuContainer.Update(int.Parse(product.ParentId), product.UserId, "", sqlCon);
                    }
                    return _revised;
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
    }
}
