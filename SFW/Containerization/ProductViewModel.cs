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
        public bool ShowProduct { get { return (HasContainers && SelectedProduct != null) || NewContainer; } }
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
            CollectionView = new ListCollectionView(new DataView());
            PalletDictionary = SkuContainer.GetPalletDictionary(App.AppSqlCon);
            NewContainer = false;
            ViewFilter = new string[2];
            StatusFilter = true;
            OnPropertyChanged(nameof(HasContainers));
            OnPropertyChanged(nameof(ShowProduct));
            if (ModelBase.MasterDataSet.Tables.Contains(typeof(SkuContainer).Name))
            {
                Initialize();
                ApplicationTimer.ActionList.Add(Refresh);
            }
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
                if (App.LoadedModule == Enumerations.UsersControls.Container)
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
        /// Refresh the container schedule view
        /// </summary>
        public override void Refresh()
        {
            try
            {
                var _index = CollectionView.CurrentPosition;
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                if (CollectionView != null)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.MoveCurrentToPosition(_index); }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prod Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initialize the container schedule view
        /// </summary>
        public override void Initialize()
        {
            CollectionView = new ListCollectionView(ModelBase.MasterDataSet.Tables[typeof(SkuContainer).Name].AsDataView());
            if (CollectionView.GroupDescriptions.Count() != 0)
            {
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate
            {
                CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ContainerID", new ContainerNameConverter()));
            }));
            if (CollectionView.SortDescriptions.Count() != 0)
            {
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.SortDescriptions.Clear(); }));
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate
            {
                CollectionView.SortDescriptions.Add(new SortDescription("ContainerID", ListSortDirection.Ascending));
            }));
            OnPropertyChanged(nameof(CollectionView));
            Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
            CollectionView.CurrentChanged += CollectionView_ItemChanged;
        }

        /// <summary>
        /// Set up the product ZPL string for the printer
        /// </summary>
        /// <param name="product"></param>
        /// <param name="row"></param>
        /// <param name="custName"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public string ProductZPLString(SkuContainer.Product product, int rowNbr, string custName, int dpi)
        {
            var _rowPos = dpi == 300 ? rowNbr * 76 + 192 : rowNbr * 50 + 130;
            var _colPos = dpi == 300 ? 695 : 455;
            return dpi == 300
                ? $@"^FT{_rowPos},{_colPos + 1180}^A0B,50,51^FH\^CI28^FD{product.ProductId}^FS^CI27
^FT{_rowPos},{_colPos + 790}^A0B,50,51^FH\^CI28^FD{product.LotId}^FS^CI27
^FT{_rowPos},{_colPos + 430}^A0B,50,51^FH\^CI28^FD{product.Quantity}^FS^CI27
^FT{_rowPos},{_colPos + 250}^A0B,50,51^FH\^CI28^FD{product.SalesOrderNumber}*{product.SalesLineNumber}^FS^CI27
^FT{_rowPos},{_colPos}^A0B,50,51^FH\^CI28^FD{product.CustomerNumber} {custName}^FS^CI27"
                : $@"^FT{_rowPos},{_colPos + 815}^A0B,34,35^FH\^CI28^FD{product.ProductId}^FS^CI27
^FT{_rowPos},{_colPos + 550}^A0B,34,35^FH\^CI28^FD{product.LotId}^FS^CI27
^FT{_rowPos},{_colPos + 305}^A0B,34,35^FH\^CI28^FD{product.Quantity}^FS^CI27
^FT{_rowPos},{_colPos + 185}^A0B,34,35^FH\^CI28^FD{product.SalesOrderNumber}*{product.SalesLineNumber}^FS^CI27
^FT{_rowPos},{_colPos}^A0B,34,35^FH\^CI28^FD{product.CustomerNumber} {custName}^FS^CI27
";
        }

        /// <summary>
        /// Set up the main ZPL string for the printer
        /// </summary>
        /// <param name="products"></param>
        /// <param name="containerId"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
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
^FT88,220^A0B,50,51^FH\^CI28^FD{ContainerObject.Weight} LBS^FS^CI27
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
^FO95,1013^GB412,0,5^FS
^FO504,21^GB0,1279,8^FS
^FO95,650^GB412,0,20^FS
^FO95,460^GB412,0,5^FS
^FO95,21^GB412,0,20^FS
^FO95,1280^GB412,0,20^FS
^FO141,31^GB0,1264,2^FS
^FO192,31^GB0,1264,2^FS
^FO243,31^GB0,1264,2^FS
^FO293,31^GB0,1264,2^FS
^FO344,31^GB0,1264,2^FS
^FO395,31^GB0,1264,2^FS
^FO446,31^GB0,1264,2^FS
^FO95,769^GB412,0,5^FS
^FT59,160^A0B,34,35^FH\^CI28^FD{ContainerObject.Weight} LBS^FS^CI27
^FT59,371^A0B,34,35^FH\^CI28^FD{ContainerObject.PalletType}^FS^CI27
^FT753,1290^A0B,34,35^FH\^CI28^FD{ContainerObject.Depth} x {ContainerObject.Length} x {ContainerObject.Height}^FS^CI27
^BY7,3,203^FT761,696^B3B,N,,N,N
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
            ApplicationTimer.Pause();
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
            CollectionView.MoveCurrentToFirst();
            ApplicationTimer.Resume();
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
            ContainerObject.Ship(App.AppSqlCon);
            ApplicationTimer.Resume();
        }

        private bool ShipCanExecute(object parameter) => true;

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
                ApplicationTimer.Resume();
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
            var _prtRez = 300;
            Forms.PrintDialog prtDialog = new Forms.PrintDialog();
            if (prtDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                _prtName = prtDialog.PrinterSettings.PrinterName;
                _prtRez = prtDialog.PrinterSettings.DefaultPageSettings.PrinterResolution.X;
            }
            if (!string.IsNullOrEmpty(_prtName))
            {
                var _cntId = SelectedProduct.Row.SafeGetField<int>("ContainerID");
                var _itemString = string.Empty;
                var _lineCounter = 0;
                var _counter = 1;

                foreach (var _item in ContainerObject.ProductCollection.Where(o => !string.IsNullOrEmpty(o.ProductId)))
                {
                    var _custName = _item.CustomerName?.Length > 22 ? _item.CustomerName.Substring(0, 22) : _item.CustomerName;
                    _itemString += ProductZPLString(_item, _lineCounter, _custName, _prtRez);

                    if (_lineCounter == 7 || _counter == ContainerObject.ProductCollection.Count(o => !string.IsNullOrEmpty(o.ProductId)))
                    {
                        _lineCounter = -1;
                        var _zplStr = MainZPLString(_itemString, _cntId.ToString(), _prtRez);
                        RawPrinter.SendStringToPrinter(_prtName, _zplStr, 1);
                        _itemString = string.Empty;
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
                ApplicationTimer.Resume();
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
        }

        #endregion
    }
}
