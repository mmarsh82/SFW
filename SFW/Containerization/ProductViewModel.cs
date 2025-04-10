using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Containerization
{
    public class ProductViewModel : ViewModelBase
    {
        #region Properties

        public string[] ViewFilter;

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : ContainerView.Table.SearchRowFilter(value);
                NoticeFilter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public DataView ContainerView { get; set; }

        private DataRowView _selProduct;
        public DataRowView SelectedProduct
        {
            get
            { return _selProduct; }
            set
            {
                if ((value == null || _selProduct == null) && ContainerView != null && ContainerView.Count >= 1)
                {
                    value = ContainerView[0];
                }
                if (_selProduct == null || value != _selProduct)
                {
                    ShowResults = false;
                    IsNew = false;
                    using (BackgroundWorker bw = new BackgroundWorker())
                    {
                        try
                        {
                            bw.DoWork += new DoWorkEventHandler(
                                delegate (object sender, DoWorkEventArgs e)
                                {
                                    if (value != null)
                                    {
                                        var _id = int.TryParse(value.Row.ItemArray[0].ToString(), out int i) ? i : 0;
                                        ContainerObject = SkuContainer.GetContainer(_id, App.AppSqlCon);
                                        OnPropertyChanged(nameof(ContainerObject));
                                        if (value.Row.SafeGetField<string>("LotTraceable").ToString() == "T")
                                        {
                                            IthResultsTable = Lot.GetLotHistoryTable(value.Row.SafeGetField<string>("ProductId").ToString(), value.Row.SafeGetField<string>("LotId").ToString(), 1, App.AppSqlCon);
                                        }
                                        else
                                        {
                                            IthResultsTable = new DataTable();
                                        }
                                        OnPropertyChanged(nameof(IthResultsTable));
                                        OnPropertyChanged(nameof(ResultsCount));
                                        ShowResults = true;
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
            }
        }

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
        public bool IsNew
        {
            get
            { return _new; }
            set
            {
                _new = value;
                OnPropertyChanged(nameof(IsNew));
            }
        }

        public SkuContainer ContainerObject { get; set; }

        RelayCommand _refresh;
        RelayCommand _addCon;
        RelayCommand _addPrt;
        RelayCommand _submit;
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
            ContainerView = SkuContainer.GetContainerData(App.AppSqlCon).AsDataView();
            IsNew = false;
            ViewFilter = new string[2];
        }

        /// <summary>
        /// Filter the notice view
        /// Index values
        /// 0 = Search Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public void NoticeFilter(string filter, int index)
        {
            if (ViewFilter != null)
            {
                ViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in ViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                var _tempList = new List<DataView>();
                if (ContainerView != null)
                {
                    ContainerView.RowFilter = _filterStr;
                    OnPropertyChanged(nameof(ContainerView));
                }
            }
            else
            {
                ViewFilter = new string[2];
            }
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
                    if (product.Submit(CurrentUser.DisplayName, App.AppSqlCon))
                    {
                        ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.LotId == product.LotId).NewProduct = false;
                        var _loc = Lot.GetLotLocation(product.LotId);
                        M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], product.LotId, _sku.Uom, _loc, ContainerObject.Location, product.Quantity, $"Container {product.ParentId}", "01", App.ErpCon);
                    }
                }
                else
                {
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.LotId == product.LotId).NewProduct = false;
                    var _loc = Lot.GetLotLocation(product.LotId);
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], product.LotId, _sku.Uom, _loc, ContainerObject.Location, product.Quantity, $"Container {product.ParentId}", "01", App.ErpCon);
                }
            }
            else
            {
                var _qty = int.TryParse(product.QuantityInput, out int i) ? i : 0;
                if (_qty != product.Quantity && !product.NewProduct)
                {
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], "", _sku.Uom, product.LocationInput, ContainerObject.Location, _qty, $"Container {product.ParentId}", "01", App.ErpCon);
                    product.Update(CurrentUser.DisplayName, App.AppSqlCon);
                }
                else if (product.NewProduct && !product.NewContainer)
                {
                    ContainerObject.ProductCollection.FirstOrDefault(o => o.ParentId == product.ParentId && o.ProductId == product.ProductId).NewProduct = false;
                    M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, product.ProductId.Split('|')[0], "", _sku.Uom, product.LocationInput, ContainerObject.Location, _qty, $"Container {product.ParentId}", "01", App.ErpCon);
                    product.Submit(CurrentUser.DisplayName, App.AppSqlCon);
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

        #region Refresh ICommand

        public ICommand RefreshICommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(RefreshExecute);
                }
                return _refresh;
            }
        }

        private void RefreshExecute(object parameter)
        {
            ContainerView = SkuContainer.GetContainerData(App.AppSqlCon).AsDataView();
            var _index = ContainerView.Table.Rows.IndexOf(SelectedProduct.Row);
            SelectedProduct = null;
            SelectedProduct = _index == -1 ? ContainerView[0] : ContainerView?[_index];
            OnPropertyChanged(nameof(ContainerView));
            NoticeFilter(SearchFilter, 0);
        }

        #endregion

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
            IsNew = true;
            ContainerObject = new SkuContainer(CurrentUser.DisplayName);
            OnPropertyChanged(nameof(ContainerObject));
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
            ContainerObject.ProductCollection.Add(new SkuContainer.Product(ContainerObject.ContainerId, CurrentUser.DisplayName, true, string.IsNullOrEmpty(ContainerObject.ContainerId)));
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
            RefreshExecute(null);
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
                RefreshExecute(null);
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
            IsNew = false;            
            SelectedProduct = ContainerView != null && ContainerView.Count > 0 ? ContainerView[5] : null;
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
                RefreshExecute(null);
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
            RefreshExecute(null);
        }

        #endregion
    }
}
