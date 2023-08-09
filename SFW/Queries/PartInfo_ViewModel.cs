using SFW.Commands;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;

//Created by Michael Marsh 4-25-18

namespace SFW.Queries
{
    public class PartInfo_ViewModel : ViewModelBase
    {
        #region Properties

        public Sku Part { get; set; }
        public ObservableCollection<Sku> MoveHistory { get; set; }

        public IList<Lot> ILotResultsList { get; set; }
        private Lot selectedILotRow;
        public Lot SelectedILotRow
        {
            get { return selectedILotRow; }
            set
            {
                selectedILotRow = value;
                try
                {
                    if (value != null)
                    {
                        UseLot = !string.IsNullOrEmpty(value.LotNumber);
                        IthResultsTable.DefaultView.RowFilter = $"LotNumber = '{value.LotNumber}'";
                        FilterText = value.LotNumber;
                        _lot = value.LotNumber;
                        FromLocation = value.Location;
                        NonConReason = FromLocation.EndsWith("N") ? Lot.GetNCRNote(value.LotNumber) : string.Empty;
                        QuantityInput = value.Onhand;
                    }
                    OnPropertyChanged(nameof(FilterText));
                    OnPropertyChanged(nameof(SelectedILotRow));
                }
                catch (NullReferenceException)
                {
                    selectedILotRow = null;
                }
            }
        }

        private string _lot;
        private string uInput;
        public string UserInput
        {
            get { return uInput; }
            set { uInput = value?.ToUpper(); OnPropertyChanged(nameof(UserInput)); }
        }
        public string PartNbrText { get; set; }
        private string filter;
        public string Filter
        {
            get { return filter; }
            set { filter = value; OnPropertyChanged(nameof(Filter)); }
        }
        public string FilterText { get; set; }
        private string PreFilter;

        public DataTable IthResultsTable { get; set; }

        public bool NoResults
        {
            get { return NoLotResults && NoHistoryResults; }
        }

        private bool lotResults;
        public bool NoLotResults
        {
            get { return lotResults; }
            set { lotResults = value; OnPropertyChanged(nameof(NoLotResults)); }
        }
        private bool lhResults;
        public bool NoHistoryResults
        {
            get { return lhResults; }
            set { lhResults = value; OnPropertyChanged(nameof(NoHistoryResults)); }
        }

        private bool loading;
        public bool IsLoading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private bool nonLot;
        public bool NonLotPart
        {
            get { return nonLot; }
            set { nonLot = value; OnPropertyChanged(nameof(NonLotPart)); }
        }

        private bool _useLot;
        public bool UseLot
        {
            get { return _useLot; }
            set { _useLot = value; OnPropertyChanged(nameof(UseLot)); }
        }

        private int? qInput;
        public int? QuantityInput
        {
            get { return qInput; }
            set { qInput = value; OnPropertyChanged(nameof(QuantityInput)); }
        }

        private string _nonConReason;
        public string NonConReason 
        {
            get { return _nonConReason; }
            set { _nonConReason = value;  OnPropertyChanged(nameof(NonConReason)); }
        }

        private string _tLoc;
        public string ToLocation
        {
            get
            { return _tLoc; }
            set
            {
                _tLoc = value.ToUpper();
                OnPropertyChanged(nameof(ToLocation));
                OnPropertyChanged(nameof(IsToValid));
                OnPropertyChanged(nameof(ToLocSize));
                OnPropertyChanged(nameof(IsNCR));
            }
        }
        public bool IsToValid { get { return string.IsNullOrEmpty(ToLocation) || Sku.IsValidLocation(ToLocation, App.SiteNumber); } }
        public int ToLocSize { get { return IsToValid ? 1 : 3; } }
        public bool IsNCR { get { return ToLocation.EndsWith("N") || FromLocation.EndsWith("N"); } }

        private string _fLoc;
        public string FromLocation
        {
            get
            { return _fLoc; }
            set
            {
                _fLoc = value?.ToUpper();
                OnPropertyChanged(nameof(FromLocation));
                OnPropertyChanged(nameof(IsFromValid));
                OnPropertyChanged(nameof(FromLocSize));
                OnPropertyChanged(nameof(IsNCR));
            }
        }
        public bool IsFromValid { get { return ILotResultsList == null || string.IsNullOrEmpty(FromLocation) || ILotResultsList.Any(o => o.Location == FromLocation); } }
        public int FromLocSize { get { return IsFromValid ? 1 : 3; } }

        public string MoveReference { get; set; }

        public int Site { get; set; }

        private bool _canRefresh;
        public bool CanRefresh
        {
            get { return _canRefresh; }
            set { _canRefresh = value; OnPropertyChanged(nameof(CanRefresh)); }
        }

        public delegate void ResultsDelegate(string s);
        public ResultsDelegate ResultsAsyncDelegate { get; private set; }
        public IAsyncResult SearchAsyncResult { get; set; }

        private RelayCommand _search;
        private RelayCommand _filter;
        private RelayCommand _mPrint;
        private RelayCommand _move;
        private RelayCommand _clear;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartInfo_ViewModel()
        {
            NoLotResults = true;
            NoHistoryResults = true;
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            NonLotPart = false;
            if (MoveHistory == null)
            {
                MoveHistory = new ObservableCollection<Sku>();
            }
            ToLocation = FromLocation = string.Empty;
            Site = App.SiteNumber;
            CanRefresh = !NoLotResults && !NoHistoryResults;
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="partNrb">Part number to pre-load</param>
        public PartInfo_ViewModel(string partNrb)
        {
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            UserInput = partNrb;
            Site = App.SiteNumber;
            SearchICommand.Execute(partNrb);
            NonLotPart = false;
            if (MoveHistory == null)
            {
                MoveHistory = new ObservableCollection<Sku>();
            }
            ToLocation = FromLocation = string.Empty;
            CanRefresh = !NoLotResults && !NoHistoryResults;
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="wo">Work Order to load</param>
        public PartInfo_ViewModel(WorkOrder wo)
        {
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            PreFilter = wo.OrderNumber;
            UserInput = wo.SkuNumber;
            SearchICommand.Execute(wo.SkuNumber);
            if (MoveHistory == null)
            {
                MoveHistory = new ObservableCollection<Sku>();
            }
            ToLocation = FromLocation = string.Empty;
            Site = wo.Facility;
            CanRefresh = !NoLotResults && !NoHistoryResults;
        }

        #region Load Results Async Delegation Implementation

        public void ResultsLoading(string inputVal)
        {
            IsLoading = true;
            CanRefresh = false;
            ILotResultsList = Lot.GetOnHandLotList(inputVal, true, Site);
            NonLotPart = false;
            if (ILotResultsList.Count == 0)
            {
                ILotResultsList = Lot.GetOnHandLotList(inputVal, false, Site);
                NonLotPart = true;
            }
            IthResultsTable = Lot.GetLotHistoryTable(inputVal, Site == App.SiteNumber ? 0 : Site, App.AppSqlCon);
        }
        public void ResultsLoaded(IAsyncResult r)
        {
            IsLoading = false;
            NoLotResults = ILotResultsList?.Count == 0;
            if(!NoLotResults && UseLot && ILotResultsList.Count(o => o.LotNumber == _lot) == 1)
            {
                Part.Location = ILotResultsList.FirstOrDefault(o => o.LotNumber == _lot).Location;
            }
            NoHistoryResults = IthResultsTable?.Rows?.Count == 0;
            if (!string.IsNullOrEmpty(PreFilter))
            {
                FilterICommand.Execute(PreFilter);
                PreFilter = null;
            }
            PartNbrText = UserInput;
            UserInput = null;
            if (UseLot)
            {
                SelectedILotRow = ILotResultsList.FirstOrDefault(o => o.LotNumber == _lot);
            }
            OnPropertyChanged(nameof(PartNbrText));
            OnPropertyChanged(nameof(ILotResultsList));
            OnPropertyChanged(nameof(IthResultsTable));
            OnPropertyChanged(nameof(NoResults));
            CanRefresh = !NoLotResults && !NoHistoryResults;
        }

        #endregion

        #region Search ICommand

        /// <summary>
        /// Search ICommand Instantiation
        /// </summary>
        public ICommand SearchICommand
        {
            get
            {
                if (_search == null)
                {
                    _search = new RelayCommand(SearchExecute, SearchCanExecute);
                }
                return _search;
            }
        }

        /// <summary>
        /// Search ICommand Validation and Execution
        /// </summary>
        /// <param name="parameter">User input</param>
        private void SearchExecute(object parameter)
        {
            if (parameter != null && parameter.ToString() == "r")
            {
                UserInput = UseLot ? _lot : Part.SkuNumber;
                ModelBase.MasterDataSet.RefreshTable(Tables.LOT);
            }
            NoLotResults = false;
            NoHistoryResults = false;
            OnPropertyChanged(nameof(NoResults));
            IthResultsTable = null;
            OnPropertyChanged(nameof(IthResultsTable));
            ILotResultsList = null;
            OnPropertyChanged(nameof(ILotResultsList));
            Filter = FilterText = string.IsNullOrEmpty(PreFilter) ? null : PreFilter;
            OnPropertyChanged(nameof(FilterText));
            if (UserInput != null)
            {
                Site = UserInput.Contains('|')
                    ? int.TryParse(UserInput.Split('|')[1], out int i) ? i : App.SiteNumber
                    : App.SiteNumber;
                UserInput = UserInput.Contains('|')
                    ? UserInput.Split('|')[0]
                    : UserInput;
                Part = UseLot ? new Sku(UserInput, 'L', Site) : new Sku(UserInput, 'S', Site, true);
                if (UseLot && Part != null)
                {
                    QuantityInput = Part.TotalOnHand;
                    FromLocation = string.IsNullOrEmpty(Part.Location) ? string.Empty : Part.Location;
                    ToLocation = string.Empty;
                }
                OnPropertyChanged(nameof(Part));
                _lot = UseLot ? UserInput : string.Empty;
                UserInput = Part.SkuNumber;
                SearchAsyncResult = ResultsAsyncDelegate.BeginInvoke(Part.SkuNumber, new AsyncCallback(ResultsLoaded), null);
            }
        }
        private bool SearchCanExecute(object parameter) => !string.IsNullOrWhiteSpace(parameter?.ToString());

        #endregion

        #region Filter ICommand

        /// <summary>
        /// Search ICommand Instantiation
        /// </summary>
        public ICommand FilterICommand
        {
            get
            {
                if (_filter == null)
                {
                    _filter = new RelayCommand(FilterExecute, FilterCanExecute);
                }
                return _filter;
            }
        }

        /// <summary>
        /// Filter ICommand Validation and Execution
        /// </summary>
        /// <param name="parameter">User input</param>
        private void FilterExecute(object parameter)
        {
            if (parameter.ToString() == "r")
            {
                parameter = string.Empty;
                UseLot = false;
                SelectedILotRow = null;
                ToLocation = FromLocation = MoveReference = string.Empty;
                OnPropertyChanged(nameof(MoveReference));
            }
            IthResultsTable.Search(parameter.ToString());
            FilterText = parameter.ToString();
            OnPropertyChanged(nameof(FilterText));
            Filter = null;
            if (Part == null || Part.TotalOnHand == 0)
            {
                QuantityInput = null;
            }
        }
        private bool FilterCanExecute(object parameter) => !string.IsNullOrEmpty(parameter?.ToString()) && IthResultsTable != null;

        #endregion

        #region Material Card Print ICommand

        public ICommand MPrintICommand
        {
            get
            {
                if (_mPrint == null)
                {
                    _mPrint = new RelayCommand(MPrintExecute, MPrintCanExecute);
                }
                return _mPrint;
            }
        }

        private void MPrintExecute(object parameter)
        {
            if (App.SiteNumber == 1)
            {
                var _dmd = UseLot ? Lot.GetDiamondNumber(_lot, App.SiteNumber) : "";
                if (_dmd == "error")
                {
                    _dmd = DiamondEntry.Show();
                }
                var _qir = UseLot ? Lot.GetAssociatedQIR(_lot, App.AppSqlCon) : 0;
                TravelCard.Create("", "technology#1",
                    Part.SkuNumber,
                    _lot,
                    Part.SkuDescription,
                    _dmd,
                    Convert.ToInt32(QuantityInput),
                    Part.Uom,
                    _qir
                    );
                switch (parameter.ToString())
                {
                    case "T":
                        TravelCard.Display(FormType.Portrait);
                        break;
                    case "R":
                        TravelCard.Display(FormType.Landscape);
                        break;

                }
            }
            if (App.SiteNumber == 2)
            {
                TravelCard.Create("", "",
                        Part.SkuNumber,
                        _lot,
                        Part.SkuDescription,
                        "",
                        Convert.ToInt32(QuantityInput),
                        Part.Uom,
                        0,
                        submitter:CurrentUser.DisplayName
                        );
                TravelCard.Display(FormType.CoC);
            }
        }
        private bool MPrintCanExecute(object parameter) => QuantityInput > 0;

        #endregion

        #region Move ICommand

        /// <summary>
        /// Unplanned Move Command
        /// </summary>
        public ICommand MoveICommand
        {
            get
            {
                if (_move == null)
                {
                    _move = new RelayCommand(MoveExecute, MoveCanExecute);
                }
                return _move;
            }
        }

        /// <summary>
        /// Unplanned Move Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveExecute(object parameter)
        {
            //TODO: add in logic to add the values to each of the tables here
            if (UseLot)
            {
                M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, Part.SkuNumber, _lot, Part.Uom, FromLocation, ToLocation, Convert.ToInt32(QuantityInput), MoveReference, $"0{App.SiteNumber}", App.ErpCon, NonConReason);
            }
            else
            {
                M2kClient.M2kCommand.InventoryMove(CurrentUser.DisplayName, Part.SkuNumber, "", Part.Uom, FromLocation, ToLocation, Convert.ToInt32(QuantityInput), MoveReference, $"0{App.SiteNumber}", App.ErpCon, NonConReason);
            }
            var _tran = new Sku
            {
                SkuNumber = Part.SkuNumber
                ,Inspection = UseLot
                ,MasterPrint = _lot
                ,SkuDescription = ToLocation
                ,TotalOnHand = Convert.ToInt32(QuantityInput)
                ,Location = FromLocation
            };
            MoveHistory.Insert(0, _tran);
            ToLocation = MoveReference = string.Empty;
            OnPropertyChanged(nameof(MoveReference));
            SearchExecute("r");
            QuantityInput = null;
        }
        private bool MoveCanExecute(object parameter)
        {
            if (!NoResults && QuantityInput > 0 && IsToValid && IsFromValid && !string.IsNullOrEmpty(ToLocation) && !string.IsNullOrEmpty(FromLocation))
            {
                if (!string.IsNullOrEmpty(ToLocation) && (!ToLocation.EndsWith("N") || (ToLocation.EndsWith("N") && !string.IsNullOrEmpty(NonConReason))))
                {
                    return Part.TotalOnHand <= 0 && !UseLot || !string.IsNullOrEmpty(FromLocation);
                }
            }
            return false;
        }

        #endregion

        #region Clear History ICommand

        /// <summary>
        /// Unplanned Move Command
        /// </summary>
        public ICommand ClearICommand
        {
            get
            {
                if (_clear == null)
                {
                    _clear = new RelayCommand(ClearExecute, ClearCanExecute);
                }
                return _clear;
            }
        }

        /// <summary>
        /// Unplanned Move Command Execution
        /// </summary>
        /// <param name="parameter"></param>
        private void ClearExecute(object parameter)
        {
            MoveHistory.Clear();
            OnPropertyChanged(nameof(MoveHistory));
        }
        private bool ClearCanExecute(object parameter) => MoveHistory.Count > 0;

        #endregion
    }
}
