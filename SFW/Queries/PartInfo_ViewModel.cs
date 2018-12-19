using SFW.Commands;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;

//Created by Michael Marsh 4-25-18

namespace SFW.Queries
{
    public class PartInfo_ViewModel : ViewModelBase
    {
        #region Properties

        public List<Lot> ILotResultsList { get; set; }
        private Lot selectedILotRow;
        public Lot SelectedILotRow
        {
            get { return selectedILotRow; }
            set
            {
                selectedILotRow = value;
                try
                {
                    IthResultsTable.DefaultView.RowFilter = $"LotNumber = '{value.LotNumber}'";
                    FilterText = value.LotNumber;
                    OnPropertyChanged(nameof(FilterText));
                }
                catch (NullReferenceException)
                {
                    selectedILotRow = value = null;
                }
            }
        }

        private string part;
        public string PartNbr
        {
            get { return part; }
            set { part = value?.ToUpper(); OnPropertyChanged(nameof(PartNbr)); }
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

        public delegate void ResultsDelegate(string s);
        public ResultsDelegate ResultsAsyncDelegate { get; private set; }
        public IAsyncResult SearchAsyncResult { get; set; }

        private RelayCommand _search;
        private RelayCommand _filter;

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
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="partNrb">Part number to pre-load</param>
        public PartInfo_ViewModel(string partNrb)
        {
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            PartNbr = partNrb;
            SearchAsyncResult = ResultsAsyncDelegate.BeginInvoke(partNrb, new AsyncCallback(ResultsLoaded), null);
            NonLotPart = false;
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="wo">Work Order to load</param>
        public PartInfo_ViewModel(WorkOrder wo)
        {
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            Filter = PreFilter = wo.OrderNumber;
            PartNbr = wo.SkuNumber;
            SearchICommand.Execute(wo.SkuNumber);
        }

        #region Load Results Async Delegation Implementation

        public void ResultsLoading(string partNbr)
        {
            IsLoading = true;
            ILotResultsList = Lot.GetOnHandLotList(partNbr, App.AppSqlCon);
            NonLotPart = false;
            if (ILotResultsList.Count == 0)
            {
                ILotResultsList = Lot.GetOnHandNonLotList(partNbr, App.AppSqlCon);
                NonLotPart = true;
            }
            IthResultsTable = Lot.GetLotHistoryTable(partNbr, App.AppSqlCon);
        }
        public void ResultsLoaded(IAsyncResult r)
        {
            IsLoading = false;
            NoLotResults = ILotResultsList?.Count == 0;
            NoHistoryResults = IthResultsTable?.Rows?.Count == 0;
            if (!string.IsNullOrEmpty(PreFilter))
            {
                FilterICommand.Execute(PreFilter);
                PreFilter = null;
            }
            PartNbrText = PartNbr;
            PartNbr = null;
            OnPropertyChanged(nameof(PartNbrText));
            OnPropertyChanged(nameof(ILotResultsList));
            OnPropertyChanged(nameof(IthResultsTable));
            OnPropertyChanged(nameof(NoResults));
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
            NoLotResults = false;
            NoHistoryResults = false;
            IthResultsTable = null;
            ILotResultsList = null;
            Filter = FilterText = null;
            OnPropertyChanged(nameof(FilterText));
            SearchAsyncResult = ResultsAsyncDelegate.BeginInvoke(parameter.ToString(), new AsyncCallback(ResultsLoaded), null);
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
            }
            IthResultsTable.Search(parameter.ToString());
            FilterText = parameter.ToString();
            OnPropertyChanged(nameof(FilterText));
            Filter = null;
        }
        private bool FilterCanExecute(object parameter) => !string.IsNullOrEmpty(parameter?.ToString()) && IthResultsTable != null;

        #endregion
    }
}
