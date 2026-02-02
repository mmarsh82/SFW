using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using SFW.Model.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Forms = System.Windows.Forms;

namespace SFW.Containerization
{
    public class ProductViewModel : ScheduleBase
    {
        #region Properties

        private bool _statusFltr;
        public bool StatusFilter
        {
            get
            { return _statusFltr; }
            set
            {
                _statusFltr = value;
                if (value)
                {
                    Filter("[Status] = 'A'", 1);
                }
                else
                {
                    Filter("[Status] = 'S'", 1);
                }
                OnPropertyChanged(nameof(StatusFilter));
                OnPropertyChanged(nameof(StatusText));
            }
        }
        public string StatusText { get { return StatusFilter ? "Active:" : "Shipped:"; } }

        private DataRowView _selProduct;
        public DataRowView SelectedProduct
        {
            get
            { return _selProduct; }
            set
            {
                if (_selProduct == null || value != _selProduct)
                {
                    ShowResults = false;
                    NewContainer = false;
                    using (BackgroundWorker bw = new BackgroundWorker())
                    {
                        try
                        {
                            bw.DoWork += new DoWorkEventHandler(
                                delegate (object sender, DoWorkEventArgs e)
                                {
                                    if (value != null)
                                    {
                                        try
                                        {
                                            if (value.Row.RowState != DataRowState.Detached)
                                            {
                                                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                                            }
                                            var _id = int.TryParse(value.Row.ItemArray[0].ToString(), out int i) ? i : 0;
                                            ContainerObject = SkuContainer.GetContainer(_id, App.AppSqlCon);
                                            OnPropertyChanged(nameof(ContainerObject));
                                            if (value.Row.SafeGetField<string>("LotTraceable").ToString() == "T")
                                            {
                                                IthResultsTable = Lot.GetHistoryTable(value.Row.SafeGetField<string>("ProductId").ToString(), value.Row.SafeGetField<string>("LotId").ToString(), 1, App.AppSqlCon);
                                            }
                                            else
                                            {
                                                IthResultsTable = new DataTable();
                                            }
                                            OnPropertyChanged(nameof(IthResultsTable));
                                            OnPropertyChanged(nameof(ResultsCount));
                                            ShowResults = true;
                                            SelectedType = PalletDictionary.FirstOrDefault(o => o.Key == ContainerObject.PalletType);
                                            OnPropertyChanged(nameof(SelectedType));
                                        }
                                        catch
                                        {
                                            Refresh();
                                        }
                                    }
                                });
                            bw.RunWorkerAsync();
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                _selProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                OnPropertyChanged(nameof(ShowProduct));
            }
        }
        private int _contId;

        public DataTable IthResultsTable { get; set; }
        public bool ResultsCount { get { return IthResultsTable != null && IthResultsTable.Rows.Count == 0; } }

        private bool _showResults;
        public bool ShowResults
        {
            get
            { return _showResults; }
            set
            {
                _showResults = value;
                OnPropertyChanged(nameof(ShowResults));
            }
        }

        private bool _new;
        public bool NewContainer
        {
            get
            { return _new; }
            set
            {
                _new = value;
                OnPropertyChanged(nameof(NewContainer));
            }
        }

        public SkuContainer ContainerObject { get; set; }

        public IReadOnlyDictionary<string, string> PalletDictionary { get; set; }

        private KeyValuePair<string, string> _selType;
        public KeyValuePair<string, string> SelectedType
        {
            get
            { return _selType; }
            set
            {
                if (!string.IsNullOrEmpty(value.Key))
                {
                    _selType = PalletDictionary.FirstOrDefault(o => o.Key == value.Key);
                }
                else
                {
                    _selType = PalletDictionary.FirstOrDefault();
                }
                if (ContainerObject != null)
                {
                    ContainerObject.PalletType = value.Key;
                }
                OnPropertyChanged(nameof(SelectedType));
            }
        }

        public bool HasContainers { get { return CollectionView.Count > 0; } }
        public bool ShowProduct { get { return HasContainers || SelectedProduct != null || NewContainer; } }
        public bool HasDims 
        {
            get
            {
                return int.TryParse(ContainerObject?.Height, out int h) && h > 0 && int.TryParse(ContainerObject?.Length, out int l) && l > 0 && int.TryParse(ContainerObject?.Depth, out int d) && d > 0;
            }
        }

        RelayCommand _addCon;
        RelayCommand _addPrt;
        RelayCommand _submit;
        RelayCommand _update;
        RelayCommand _ship;
        RelayCommand _delete;
        RelayCommand _print;
        RelayCommand _cancel;
        RelayCommand _remove;
        RelayCommand _commit;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductViewModel()
        {
            ApplicationTimer.ActionList.Add(Refresh);
            CollectionView = new ListCollectionView(new DataView());
            PalletDictionary = SkuContainer.GetPalletDictionary(App.AppSqlCon);
            NewContainer = false;
            ViewFilter = new string[2];
            StatusFilter = true;
            OnPropertyChanged(nameof(HasContainers));
            OnPropertyChanged(nameof(ShowProduct));
            Initialize();
        }

        /// <summary>
        /// Tracks the item selections from the CollectionView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionView_ItemChanged(object sender, EventArgs e)
        {
            try
            {
                var _dRow = (DataRowView)CollectionView.CurrentItem;
                if (_dRow != null)
                {
                    if (_dRow.Row.RowState != DataRowState.Detached)
                    {
                        _contId = int.TryParse(_dRow.Row.ItemArray[0].ToString(), out int i) ? i : 0;
                    }
                    SelectedProduct = _dRow;
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Move a product in the ERP
        /// </summary>
        /// <param name="product">Product object that needs to be moved</param>
        public void ProductMove(SkuContainer.Product product)
        {
            var _sku = new Sku(product.ProductId);
            if (product.LotTraceable)
            {
                if (!product.NewContainer)
                {
                    if (product.Submit(App.AppSqlCon))
                    {
                        ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.LotId == product.LotId).NewProduct = false;
                        var _loc = Lot.GetLocation(product.LotId);
                        M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], product.LotId, _sku.Uom, _loc, ContainerObject.Location, product.Quantity, $"Container {product.ParentId}", "01", App.ErpCon);
                    }
                }
                else
                {
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.LotId == product.LotId).NewProduct = false;
                    var _loc = Lot.GetLocation(product.LotId);
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], product.LotId, _sku.Uom, _loc, ContainerObject.Location, product.Quantity, $"Container {product.ParentId}", "01", App.ErpCon);
                }
            }
            else
            {
                var _qty = int.TryParse(product.QuantityInput, out int i) ? i : 0;
                if (_qty != product.Quantity && !product.NewProduct)
                {
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], "", _sku.Uom, product.LocationInput, ContainerObject.Location, _qty, $"Container {product.ParentId}", "01", App.ErpCon);
                    product.Update('Q', ContainerObject.ContainerId, App.AppSqlCon);
                }
                else if (product.NewProduct && !product.NewContainer)
                {
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.ProductId == product.ProductId).NewProduct = false;
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], "", _sku.Uom, product.LocationInput, ContainerObject.Location, _qty, $"Container {product.ParentId}", "01", App.ErpCon);
                    product.Submit(App.AppSqlCon);
                }
                else
                {
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.ProductId == product.ProductId).NewProduct = false;
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.ProductId == product.ProductId).NewContainer = false;
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], "", _sku.Uom, product.LocationInput, ContainerObject.Location, _qty, $"Container {product.ParentId}", "01", App.ErpCon);
                }
                ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.ProductId == product.ProductId).Quantity = _qty;
            }
        }

        /// <summary>
        /// Initialize the production schedule view
        /// </summary>
        public override void Initialize()
        {
            var _index = CollectionView.CurrentPosition;
            ModelBase.MasterDataSet.RefreshTable(typeof(SkuContainer), new SkuContainer().GetTable(0, App.AppSqlCon));
            CollectionView = new ListCollectionView(ModelBase.MasterDataSet.Tables[typeof(SkuContainer).Name].AsDataView());
            if (CollectionView.GroupDescriptions.Count() != 0)
            {
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate
            {
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ContainerID", new ContainerNameConverter()));
            }));

            OnPropertyChanged(nameof(CollectionView));
            Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
            if (CollectionView != null)
            {
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_index); }));
            }
            if (!string.IsNullOrEmpty(SearchFilter))
            {
                Filter(SearchFilter, 0);
            }
            CollectionView.CurrentChanged += CollectionView_ItemChanged;
        }

        public string ProductZPLString(int row, int dpi)
        {
            var _rtnStr = string.Empty;



            return _rtnStr;
        }

        public string MainZPLString(string products, string containerId, int dpi)
        {
            return dpi == 300 
                ? $@"^XA
^FT105,1920^A0B,75,76^FH\^CI28^FDContainer Id:^FS^CI27
^FT105,1350^A0B,75,76^FH\^CI28^FD{containerId}^FS^CI27
^FO129,30^GB0,1890,12^FS
{products}
^FO141,1496^GB609,0,8^FS
^FO744,30^GB0,1890,12^FS
^FO141,960^GB609,0,30^FS
^FO141,700^GB609,0,8^FS
^FO141,30^GB609,0,30^FS
^FO141,1890^GB609,0,30^FS
^FO209,44^GB0,1868,3^FS
^FO284,44^GB0,1868,3^FS
^FO358,44^GB0,1868,3^FS
^FO434,44^GB0,1868,3^FS
^FO508,44^GB0,1868,3^FS
^FO584,44^GB0,1868,3^FS
^FO659,44^GB0,1868,3^FS
^FO141,1136^GB609,0,8^FS
^FT88,197^A0B,50,51^FH\^CI28^FD{ContainerObject.Weight} LBS^FS^CI27
^FT88,500^A0B,50,51^FH\^CI28^FD{ContainerObject.PalletType}^FS^CI27
^FT1112,1905^A0B,50,51^FH\^CI28^FD{ContainerObject.Depth} x {ContainerObject.Length} x {ContainerObject.Height}^FS^CI27
^BY10,3,300^FT1125,995^B3B,N,,N,N
^FD{containerId}^FS
^PQ1,0,1,Y
^XZ"
                : $@"^XA
^FT71,1300^A0B,51,51^FH\^CI28^FDContainer Id:^FS^CI27
^FT71,914^A0B,51,51^FH\^CI28^FD{containerId}^FS^CI27
^FO87,21^GB0,1279,8^FS
{products}
^FO141,1496^GB609,0,8^FS
^FO744,30^GB0,1890,12^FS
^FO141,960^GB609,0,30^FS
^FO141,700^GB609,0,8^FS
^FO141,30^GB609,0,30^FS
^FO141,1890^GB609,0,30^FS
^FO209,44^GB0,1868,3^FS
^FO284,44^GB0,1868,3^FS
^FO358,44^GB0,1868,3^FS
^FO434,44^GB0,1868,3^FS
^FO508,44^GB0,1868,3^FS
^FO584,44^GB0,1868,3^FS
^FO659,44^GB0,1868,3^FS
^FO141,1136^GB609,0,8^FS
^FT88,197^A0B,50,51^FH\^CI28^FD{ContainerObject.Weight} LBS^FS^CI27
^FT88,500^A0B,50,51^FH\^CI28^FD{ContainerObject.PalletType}^FS^CI27
^FT1112,1905^A0B,50,51^FH\^CI28^FD{ContainerObject.Depth} x {ContainerObject.Length} x {ContainerObject.Height}^FS^CI27
^BY10,3,300^FT1125,995^B3B,N,,N,N
^FD{containerId}^FS
^PQ1,0,1,Y
^XZ";
        }

        #region Add Container ICommand

        public ICommand AddContainerICommand
        {
            get
            {
                if (_addCon == null)
                {
                    _addCon = new RelayCommand(AddContainerExecute);
                }
                return _addCon;
            }
        }

        private void AddContainerExecute(object parameter)
        {
            ShowResults = true;
            IthResultsTable = new DataTable();
            OnPropertyChanged(nameof(ResultsCount));
            NewContainer = true;
            SelectedType = PalletDictionary.FirstOrDefault();
            ContainerObject = new SkuContainer(CurrentUser.ErpId, CurrentUser.DisplayName);
            ContainerObject.PalletType = SelectedType.Key;
            OnPropertyChanged(nameof(ContainerObject));
            OnPropertyChanged(nameof(HasContainers));
            OnPropertyChanged(nameof(ShowProduct));
        }

        #endregion

        #region Add Product ICommand

        public ICommand AddProductICommand
        {
            get
            {
                if (_addPrt == null)
                {
                    _addPrt = new RelayCommand(AddProductExecute);
                }
                return _addPrt;
            }
        }

        private void AddProductExecute(object parameter)
        {
            ContainerObject.ProductCollection.Add(new SkuContainer.Product(ContainerObject.ContainerId, CurrentUser.ErpId, true, string.IsNullOrEmpty(ContainerObject.ContainerId)));
        }

        #endregion

        #region Submit ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        private void SubmitExecute(object parameter)
        {
            ContainerObject.Submit(App.AppSqlCon);
            foreach (var _product in ContainerObject.ProductCollection)
            {
                if ((_product.LotTraceable && _product.LocationInput != ContainerObject.Location) || !_product.LotTraceable)
                {
                    ProductMove(_product);
                }
            }
            Initialize();
        }

        private bool SubmitCanExecute(object parameter)
        {
            var _validated = false;
            if (ContainerObject == null)
            {
                return _validated;
            }
            foreach (var _prod in ContainerObject.ProductCollection)
            {
                _validated = _prod.Validated && _prod.ValidLocation && _prod.ValidQuantity;
            }
            return ContainerObject.ValidLocation && _validated;
        }

        #endregion

        #region Update ICommand

        public ICommand UpdateICommand
        {
            get
            {
                if (_update == null)
                {
                    _update = new RelayCommand(UpdateExecute);
                }
                return _update;
            }
        }

        private void UpdateExecute(object parameter)
        {
            SkuContainer.UpdateHeader(ContainerObject, CurrentUser.ErpId, App.AppSqlCon);
            foreach (var _item in ContainerObject.ProductCollection.Where(o => o.ValidSalesOrder))
            {
                if (_item.SalesOrderNumber != SkuContainer.Product.GetSalesOrder(_item.ProductId, _item.ParentId))
                {
                    _item.Update('S', ContainerObject.ContainerId, App.AppSqlCon);
                }
            }
            Initialize();
        }

        #endregion

        #region Ship ICommand

        public ICommand ShipICommand
        {
            get
            {
                if (_ship == null)
                {
                    _ship = new RelayCommand(ShipExecute, ShipCanExecute);
                }
                return _ship;
            }
        }

        private void ShipExecute(object parameter)
        {
            
        }

        private bool ShipCanExecute(object parameter) => ContainerObject != null && int.TryParse(ContainerObject.Weight, out int w) && w > 0 && HasDims && ContainerObject.ProductCollection.Count(o => !o.ValidSalesOrder) == 0;

        #endregion

        #region Delete ICommand

        public ICommand DeleteICommand
        {
            get
            {
                if (_delete == null)
                {
                    _delete = new RelayCommand(DeleteExecute, DeleteCanExecute);
                }
                return _delete;
            }
        }

        private void DeleteExecute(object parameter)
        {
            var _result = MessageBox.Show("Are you sure you want to delete the entire container?", "Delete verfication", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (_result == MessageBoxResult.Yes && int.TryParse(SelectedProduct.Row.ItemArray[0].ToString(), out int i))
            {
                SkuContainer.Delete(i, App.AppSqlCon);
                Initialize();
            }
        }

        private bool DeleteCanExecute(object parameter) => !string.IsNullOrEmpty(ContainerObject?.ContainerId);

        #endregion

        #region Print ICommand

        public ICommand PrintICommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(PrintExecute, PrintCanExecute);
                }
                return _print;
            }
        }

        private void PrintExecute(object parameter)
        {
            var _prtName = string.Empty;
            Forms.PrintDialog prtDialog = new Forms.PrintDialog();
            if (prtDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                _prtName = prtDialog.PrinterSettings.PrinterName;
            }
            if (!string.IsNullOrEmpty(_prtName))
            {
                var _cntId = SelectedProduct.Row.SafeGetField<string>("ContainerID");
                var _itemString = string.Empty;
                var _lineCounter = 1;
                var _counter = 1;
                var _rowPos = 192;

                foreach (var _item in ContainerObject.ProductCollection)
                {
                    var _custName = _item.CustomerName?.Length > 22 ? _item.CustomerName.Substring(0, 22) : _item.CustomerName;
                    _itemString += $@"^FT{_rowPos},1875^A0B,50,51^FH\^CI28^FD{_item.ProductId}^FS^CI27
^FT{_rowPos},1485^A0B,50,51^FH\^CI28^FD{_item.LotId}^FS^CI27
^FT{_rowPos},1125^A0B,50,51^FH\^CI28^FD{_item.Quantity}^FS^CI27
^FT{_rowPos},945^A0B,50,51^FH\^CI28^FD{_item.SalesOrderNumber}*{_item.SalesLineNumber}^FS^CI27
^FT{_rowPos},695^A0B,50,51^FH\^CI28^FD{_item.CustomerNumber} {_custName}^FS^CI27";
                    _rowPos += _lineCounter == 9 ? -532 : 76;

                    if (_lineCounter == 8 || _counter == ContainerObject.ProductCollection.Count())
                    {
                        _lineCounter = 1;
                        string s = $@"^XA
^FT105,1920^A0B,75,76^FH\^CI28^FDContainer Id:^FS^CI27
^FT105,1350^A0B,75,76^FH\^CI28^FD{_cntId}^FS^CI27
^FO129,30^GB0,1890,12^FS
{_itemString}
^FO141,1496^GB609,0,8^FS
^FO744,30^GB0,1890,12^FS
^FO141,960^GB609,0,30^FS
^FO141,700^GB609,0,8^FS
^FO141,30^GB609,0,30^FS
^FO141,1890^GB609,0,30^FS
^FO209,44^GB0,1868,3^FS
^FO284,44^GB0,1868,3^FS
^FO358,44^GB0,1868,3^FS
^FO434,44^GB0,1868,3^FS
^FO508,44^GB0,1868,3^FS
^FO584,44^GB0,1868,3^FS
^FO659,44^GB0,1868,3^FS
^FO141,1136^GB609,0,8^FS
^FT88,197^A0B,50,51^FH\^CI28^FD{ContainerObject.Weight} LBS^FS^CI27
^FT88,500^A0B,50,51^FH\^CI28^FD{ContainerObject.PalletType}^FS^CI27
^FT1112,1905^A0B,50,51^FH\^CI28^FD{ContainerObject.Depth} x {ContainerObject.Length} x {ContainerObject.Height}^FS^CI27
^BY10,3,300^FT1125,995^B3B,N,,N,N
^FD{_cntId}^FS
^PQ1,0,1,Y
^XZ";
                        RawPrinter.SendStringToPrinter(_prtName, s, 1);
                    }
                    _lineCounter++;
                    _counter++;
                }
            }
        }

        private bool PrintCanExecute(object parameter) => !string.IsNullOrEmpty(ContainerObject?.ContainerId);

        #endregion

        #region Cancel ICommand

        public ICommand CancelICommand
        {
            get
            {
                if (_cancel == null)
                {
                    _cancel = new RelayCommand(CancelExecute);
                }
                return _cancel;
            }
        }

        private void CancelExecute(object parameter)
        {
            ShowResults = true;
            NewContainer = false;
            SelectedProduct = CollectionView != null && CollectionView.Count > 0 ? ((DataView)CollectionView.SourceCollection)[5] : null;
            OnPropertyChanged(nameof(HasContainers));
        }

        #endregion

        #region Remove ICommand

        public ICommand RemoveICommand
        {
            get
            {
                if (_remove == null)
                {
                    _remove = new RelayCommand(RemoveExecute);
                }
                return _remove;
            }
        }

        private void RemoveExecute(object parameter)
        {
            var _product = ((SkuContainer.Product)parameter);
            var _refresh = false;
            if (_product.LotTraceable)
            {
                if (!_product.NewProduct)
                {
                    _refresh = ProductActions.Delete(_product, App.AppSqlCon);
                }
                ContainerObject.ProductCollection.FirstOrDefault(o => o.LotId == _product.LotId);
            }
            else
            {
                if (!_product.NewProduct)
                {
                    _refresh = ProductActions.Delete(_product, App.AppSqlCon);
                }
                ContainerObject.ProductCollection.FirstOrDefault(o => o.ProductId == _product.ProductId);
            }
            ContainerObject.ProductCollection.Remove(_product);
            if (ContainerObject.ProductCollection.Count == 0)
            {
                var _result = MessageBox.Show("Would you like to delete the entire container?", "Empty container", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (_result == MessageBoxResult.Yes && int.TryParse(_product.ParentId, out int i))
                {
                    _refresh = SkuContainer.Delete(i, App.AppSqlCon);
                }
            }
            if (_refresh)
            {
                Initialize();
            }
        }

        #endregion

        #region Commit ICommand

        public ICommand CommitICommand
        {
            get
            {
                if (_commit == null)
                {
                    _commit = new RelayCommand(CommitExecute);
                }
                return _commit;
            }
        }

        private void CommitExecute(object parameter)
        {
            if (parameter.ToString() == "LocationChange")
            {
                var _id = int.Parse(ContainerObject.ContainerId);
                SkuContainer.Update(_id, CurrentUser.DisplayName, ContainerObject.Location, App.AppSqlCon);
                foreach (var _product in ContainerObject.ProductCollection)
                {
                    var _sku = new Sku(_product.ProductId);
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, _product.ProductId.Split('|')[0], _product.LotId, _sku.Uom, ContainerObject.LoadedLocation, ContainerObject.Location, _product.Quantity, $"Container {_product.ParentId}", "01", App.ErpCon);
                }
                ContainerObject.LoadedLocation = ContainerObject.Location;
                ContainerObject.Location = ContainerObject.LoadedLocation;
            }
            else
            {
                ProductMove((SkuContainer.Product)parameter);
            }
            Initialize();
        }

        #endregion
    }
}
