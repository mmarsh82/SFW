using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

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
        public ICollectionView CollectionView { get; set; }

        /// <summary>
        /// The current Selected item that is synced from the view
        /// Used in the change event and the refresh to load the dynamic object into the form view
        /// </summary>
        public KeyValuePair<string, string> SelectedItemFilter;

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
        { }
    }
}
