using SFW.Model;
using SFW.Model.Product;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SFW.Schedule.SalesOrder
{
    public class ViewModel : ScheduleBase
    {
        #region Properties

        public IList<string> OrderTypeList { get; set; }
        private string _selType;
        public string SelectedType
        {
            get { return _selType; }
            set
            {
                var _filter = value != "All" ? $"[Type]='{value}'" : "";
                Filter(_filter, 4);
                _selType = value;
                OnPropertyChanged(nameof(SelectedType));
            }
        }

        public IList<string> CreditStatusList { get; set; }
        private string _credStatus;
        public string SelectedCredStatus
        {
            get { return _credStatus; }
            set
            {
                var _filter = value != "Any" ? $"[CredStatus]='{value}'" : "";
                Filter(_filter, 5);
                _credStatus = value;
                OnPropertyChanged(nameof(SelectedCredStatus));
            }
        }

        private bool _isNew;
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                var _filter = value ? $"[DateAdded]='{DateTime.Today.ToString("yyyy-MM-dd")}'" : "";
                Filter(_filter, 7);
                _isNew = value;
                OnPropertyChanged(nameof(IsNew));
            }
        }

        private bool _pickSel;
        public bool PickSelected
        {
            get { return _pickSel; }
            set
            {
                var _filter = value ? "[MTO]='0'" : "[MTO]='1'";
                Filter(_filter, 8);
                _pickSel = value;
                OnPropertyChanged(nameof(PickSelected));
                OnPropertyChanged(nameof(PickContent));
            }
        }
        public string PickContent { get { return PickSelected == true ? "Pick:" : "MTO:"; } }

        private bool _schedType;
        public bool ScheduleType
        {
            get { return _schedType; }
            set
            {
                _schedType = value;
                if (CollectionView != null)
                {
                    Refresh();
                }
                OnPropertyChanged(nameof(ScheduleType));
                OnPropertyChanged(nameof(ScheduleTypeContent));
            }
        }
        public string ScheduleTypeContent { get { return ScheduleType ? "Detail:" : "Header:"; } }

        private bool _isSched;
        public bool IsSchedule
        {
            get { return _isSched; }
            set
            {
                if (value)
                {
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("DS"));
                    PickSelected = true;
                }
                else
                {
                    SelectedType = OrderTypeList.FirstOrDefault(o => o.Contains("All"));
                    PickSelected = false;
                }
                _isSched = value;
                OnPropertyChanged(nameof(IsSchedule));
                OnPropertyChanged(nameof(IsScheduleContent));
            }
        }
        public string IsScheduleContent { get { return IsSchedule ? "New:" : "Open:"; } }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            ScheduleType = true;
            if (App.SiteNumber == 1)
            {
                ApplicationTimer.ActionList.Add(Refresh);
                CollectionView = new ListCollectionView(new DataView());
                OrderTypeList = Model.Sales.SalesOrder.GetOrderTypeList();
                OrderTypeList.Insert(0, "All");
                IsSchedule = false;
                CreditStatusList = new List<string>
                {
                    "Any"
                    ,"A"
                    ,"H"
                    ,"W"
                };
                SelectedCredStatus = CreditStatusList[0];
                Initialize();
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
                if (App.LoadedModule == Enumerations.UsersControls.SalesOrder)
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        var _sku = new Sku(_dRow.Row.Field<string>("PartNbr"), 'S', App.SiteNumber, true);
                        var _soObj = new Model.Sales.SalesOrder(_dRow.Row);
                        var _action = new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(8, 1, new ShopRoute.SalesOrder.ViewModel(_soObj, _sku)); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Refresh action for the schedule data
        /// </summary>
        public override void Initialize()
        {
            try
            {
                var _tempTable = ModelBase.MasterDataSet.Tables[typeof(Model.Sales.SalesOrder).Name];
                if (!ScheduleType)
                {
                    _tempTable = ((DataView)CollectionView.SourceCollection).Table.AsEnumerable()
                        .GroupBy(r => r.Field<string>("SoNbr"))
                        .Select(g => g.First())
                        .CopyToDataTable();
                }
                CollectionView = new ListCollectionView(_tempTable.AsDataView());
                if (CollectionView.GroupDescriptions.Count() != 0)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
                }
                Application.Current?.Dispatcher.Invoke(new Action(delegate
                {
                    CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("FullCustName"));
                }));
                OnPropertyChanged(nameof(CollectionView));
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                CollectionView.CurrentChanged += CollectionView_ItemChanged;
                if (OrderTypeList.Count == 1)
                {
                    OrderTypeList.Clear();
                    OrderTypeList = Model.Sales.SalesOrder.GetOrderTypeList();
                    OrderTypeList.Insert(0, "All");
                    OnPropertyChanged(nameof(OrderTypeList));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sales Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
