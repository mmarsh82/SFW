using SFW.Helpers;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SFW
{
    public abstract class ScheduleBase : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// View filter array
        /// Contains all the expressions the CollectionView source is using
        /// </summary>
        public string[] ViewFilter;

        /// <summary>
        /// Model data CollectionView to be used to bind in the view
        /// </summary>
        public ListCollectionView CollectionView { get; set; }

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)CollectionView.SourceCollection).Table.SearchRowFilter(value);
                Filter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        private RelayCommand _refresh;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScheduleBase()
        { }

        /// <summary>
        /// Filter the schedule view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Work Center Filter
        /// 2 = Work Center Group Filter
        /// 3 = Site Filter
        /// 4 = Custom Filter
        /// 5 = Custom Filter
        /// 6 = Custom Filter
        /// 7 = Custom Filter
        /// 8 = Custom Filter
        /// 9 = Custom Filter
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <param name="index">Filter index</param>
        public void Filter(string filter, int index)
        {
            if (ViewFilter == null)
            {
                ViewFilter = new string[10];
            }
            ViewFilter[index] = filter;
            var _filterStr = string.Empty;
            foreach (var s in ViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = _filterStr;
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
        }

        /// <summary>
        /// Used to get the current CollectionView sourece filter
        /// </summary>
        /// <returns>Filter as a string</returns>
        public string GetFilter()
        {
            var _filterStr = string.Empty;
            foreach (var s in ViewFilter.Where(o => !string.IsNullOrEmpty(o)))
            {
                _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
            }
            return _filterStr;
        }

        public virtual void ResetFilter()
        { }

        public virtual void Refresh()
        {
            try
            {
                var _index = CollectionView == null ? -1 : CollectionView.CurrentPosition;
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

        public virtual void Initialize()
        { }

        #region Schedule Refresh ICommand

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

        private void RefreshExecute(object parameter)
        {
            Refresh();
        }
        private bool RefreshCanExecute(object parameter) => ApplicationTimer.Status != TimerState.Running;

        #endregion
    }
}
