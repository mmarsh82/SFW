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

        private int loadProgress;
        public int LoadProgress
        {
            get { return loadProgress; }
            set { loadProgress = value; OnPropertyChanged(nameof(LoadProgress)); }
        }

        private RelayCommand _search;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartInfo_ViewModel()
        {
            NoResults = true;
            LoadProgress = -1;
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="partNrb">Part number to pre-load</param>
        public PartInfo_ViewModel(string partNrb)
        {
            SearchICommand.Execute(partNrb);
            LoadProgress = -1;
        }

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
            ILotResultsList = Lot.GetOnHandLotList(parameter.ToString(), App.AppSqlCon);
            OnPropertyChanged(nameof(ILotResultsList));
            NoResults = ILotResultsList?.Count == 0;
            IthResultsTable = Lot.GetLotHistoryTable(parameter.ToString(), App.AppSqlCon);
            OnPropertyChanged(nameof(IthResultsTable));
        }
        private bool SearchCanExecute(object parameter) => !string.IsNullOrWhiteSpace(parameter.ToString());

        #endregion
    }
}
