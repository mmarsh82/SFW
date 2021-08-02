using SFW.Helpers;
using SFW.Model;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace SFW.Queries
{
    public class Quality_ViewModel : ViewModelBase
    {
        #region Properties

        public IList<Lot> MasterIList { get; set; }
        public IList<Lot> DiamondIList { get; set; }
        private Lot _selectedRow;
        public Lot SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                _selectedRow = value;
                OnPropertyChanged(nameof(SelectedRow));
            }
        }

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = string.IsNullOrEmpty(value) ? null : value;
                FilterView(_sFilter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }
        public string[] Filter { get; set; }

        private bool _vType;
        public bool ViewType
        {
            get { return _vType; }
            set 
            { 
                _vType = value;
                FilterView(value.ToString(), 1);
                OnPropertyChanged(nameof(ViewType));
                OnPropertyChanged(nameof(ViewTypeContent));
            }
        }
        public string ViewTypeContent { get { return ViewType ? "Valid" : "Invalid"; } }

        private RelayCommand _refresh;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Quality_ViewModel()
        {
            DiamondIList = MasterIList = Lot.GetDiamondList(App.AppSqlCon);
            Filter = new string[2];
            ViewType = false;
        }

        /// <summary>
        /// Filter the Diamondlist
        /// </summary>
        /// <param name="filter">String to filter the diamondlist view</param>
        /// <param name="loc">Location with in the array to save the filter</param>
        public void FilterView(string filter, int loc)
        {
            DiamondIList = MasterIList;
            Filter[loc] = filter;
            if (!string.IsNullOrEmpty(Filter[0]) || !string.IsNullOrEmpty(Filter[1]))
            {
                if(string.IsNullOrEmpty(Filter[0]))
                {
                    var _val = bool.TryParse(Filter[1], out bool b) ? b : false;
                    DiamondIList = DiamondIList.Where(o => o.Validated == _val).ToList();
                }
                else if (string.IsNullOrEmpty(Filter[1]))
                {
                    DiamondIList = DiamondIList.Where(o => o.LotNumber.Contains(Filter[0]) || o.Location.Contains(Filter[0]) || o.ReceivedDate.ToString().Contains(Filter[0])).ToList();
                }
                else
                {
                    var _val = bool.TryParse(Filter[1], out bool b) ? b : false;
                    DiamondIList = DiamondIList.Where(o => o.Validated == _val && (o.LotNumber.Contains(Filter[0]) || o.Location.Contains(Filter[0]) || o.ReceivedDate.ToString().Contains(Filter[0]))).ToList();
                }
            }
            OnPropertyChanged(nameof(DiamondIList));
        }

        #region Refresh ICommand

        /// <summary>
        /// Refresh ICommand Instantiation
        /// </summary>
        public ICommand RefreshICommand
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = new RelayCommand(RefreshExecute, RefreshCanExecute);
                }
                return _refresh;
            }
        }

        /// <summary>
        /// Refresh ICommand Validation and Execution
        /// </summary>
        /// <param name="parameter">User input</param>
        private void RefreshExecute(object parameter)
        {
            Filter = new string[2];
            DiamondIList = MasterIList = Lot.GetDiamondList(App.AppSqlCon);
            OnPropertyChanged(nameof(DiamondIList));
            SearchFilter = string.Empty;
            ViewType = false;
        }
        private bool RefreshCanExecute(object parameter) => true;

        #endregion
    }
}
