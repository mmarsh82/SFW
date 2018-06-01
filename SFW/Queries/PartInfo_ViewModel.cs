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
                }
                catch (NullReferenceException)
                {
                    selectedILotRow = value = null;
                }
            }
        }

        public DataTable IthResultsTable { get; set; }

        private bool results;
        public bool NoResults
        {
            get { return results; }
            set { results = value; OnPropertyChanged(nameof(NoResults)); }
        }

        private bool loading;
        public bool IsLoading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged(nameof(IsLoading)); }
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
            NoResults = true;
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="partNrb">Part number to pre-load</param>
        public PartInfo_ViewModel(string partNrb)
        {
            IsLoading = false;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            SearchICommand.Execute(partNrb);
        }

        #region Load Search Results Async Delegation Implementation

        public void ResultsLoading(string partNbr)
        {
            IsLoading = true;
            ILotResultsList = Lot.GetOnHandLotList(partNbr, App.AppSqlCon);
            IthResultsTable = Lot.GetLotHistoryTable(partNbr, App.AppSqlCon);
        }
        public void ResultsLoaded(IAsyncResult r)
        {
            IsLoading = false;
            NoResults = ILotResultsList?.Count == 0;
            OnPropertyChanged(nameof(ILotResultsList));
            OnPropertyChanged(nameof(IthResultsTable));
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
            NoResults = false;
            IthResultsTable = null;
            ILotResultsList = null;
            SearchAsyncResult = ResultsAsyncDelegate.BeginInvoke(parameter.ToString(), new AsyncCallback(ResultsLoaded), null);
        }
        private bool SearchCanExecute(object parameter) => !string.IsNullOrWhiteSpace(parameter.ToString());

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
            IthResultsTable.Search(parameter.ToString());
        }
        private bool FilterCanExecute(object parameter) => !string.IsNullOrEmpty(parameter.ToString()) && IthResultsTable != null;

        #endregion
    }
}
