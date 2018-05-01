using SFW.Commands;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;
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
                    IthResultsView = CollectionViewSource.GetDefaultView(IthResultsList.Where(o => o.LotNumber == selectedILotRow.LotNumber));
                    OnPropertyChanged(nameof(IthResultsView));
                }
                catch (NullReferenceException)
                {
                    selectedILotRow = value = null;
                }
            }
        }

        public List<Lot> IthResultsList { get; set; }
        public ICollectionView IthResultsView { get; set; }

        private bool results;
        public bool NoResults
        {
            get { return results; }
            set { results = value; OnPropertyChanged(nameof(NoResults)); }
        }

        private RelayCommand _search;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartInfo_ViewModel()
        {
            NoResults = true;
        }

        /// <summary>
        /// Pre loaded constructor to show the view with results already loaded
        /// </summary>
        /// <param name="partNrb">Part number to pre-load</param>
        public PartInfo_ViewModel(string partNrb)
        {
            SearchICommand.Execute(partNrb);
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
            IthResultsList = Lot.GetLotHistoryList(parameter.ToString(), App.AppSqlCon);
            IthResultsView = CollectionViewSource.GetDefaultView(IthResultsList);
            OnPropertyChanged(nameof(IthResultsView));
        }
        private bool SearchCanExecute(object parameter) => !string.IsNullOrWhiteSpace(parameter.ToString());

        #endregion
    }
}
