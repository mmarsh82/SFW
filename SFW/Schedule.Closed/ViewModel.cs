using SFW.Controls;
using SFW.Converters;
using SFW.Model;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Data;

//Created by Michael Marsh 4-11-19

namespace SFW.Schedule.Closed
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static ICollectionView ClosedScheduleView { get; set; }
        public static string[] ClosedScheduleViewFilter;
        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                _selectedWO = value;
                if (value != null)
                {
                    var _wo = new WorkOrder(value.Row);
                    WorkSpaceDock.ClosedDock.Children.RemoveAt(1);
                    WorkSpaceDock.ClosedDock.Children.Insert(1, new ShopRoute.View { DataContext = new ShopRoute.ViewModel(_wo) });
                }
                OnPropertyChanged(nameof(SelectedWorkOrder));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private string _sFilter;
        public string SearchFilter
        {
            get { return _sFilter; }
            set
            {
                _sFilter = value == "" ? null : value;
                var _filter = string.IsNullOrEmpty(value) ? "" : ((DataView)ClosedScheduleView.SourceCollection).Table.SearchRowFilter(value);
                ScheduleFilter(_filter, 0);
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            if (ClosedScheduleViewFilter == null)
            {
                ClosedScheduleViewFilter = new string[5];
            }
            if (ClosedScheduleView == null)
            {
                ClosedScheduleView = CollectionViewSource.GetDefaultView(ModelBase.MasterDataSet.Tables["ClosedMaster"]);
                ClosedScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter()));
                ScheduleFilter(UserConfig.BuildMachineFilter(), 1);
                if (MainWindowViewModel.SelectedMachine != MainWindowViewModel.MachineList.FirstOrDefault())
                {
                    ScheduleFilter($"MachineNumber = '{Machine.GetMachineNumber(MainWindowViewModel.SelectedMachine)}'", 1);
                }
                else if (MainWindowViewModel.SelectedMachineGroup != MainWindowViewModel.MachineGroupList.FirstOrDefault())
                {
                    ScheduleFilter($"MachineGroup = '{MainWindowViewModel.SelectedMachineGroup}'", 2);
                }
                ScheduleFilter(UserConfig.BuildPriorityFilter(), 3);
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ClosedScheduleView)));
            }
        }

        /// <summary>
        /// Filter the schedule view
        /// Index values
        /// 0 = Search Filter
        /// 1 = Work Center Filter
        /// 2 = Work Center Group Filter
        /// 3 = Work Order Priority Filter
        /// </summary>
        /// <param name="filter">Filter string to use on the default view</param>
        /// <param name="index">Index of the filter string list you are adding to our changing</param>
        public static void ScheduleFilter(string filter, int index)
        {
            if (ClosedScheduleViewFilter != null)
            {
                ClosedScheduleViewFilter[index] = filter;
                var _filterStr = string.Empty;
                foreach (var s in ClosedScheduleViewFilter.Where(o => !string.IsNullOrEmpty(o)))
                {
                    _filterStr += string.IsNullOrEmpty(_filterStr) ? $"({s})" : $" AND ({s})";
                }
                ((DataView)ClosedScheduleView.SourceCollection).RowFilter = _filterStr;
                ClosedScheduleView.Refresh();
            }
        }

        /// <summary>
        /// Clears the schedule filter string array
        /// </summary>
        public static void ClearFilter()
        {
            if (ClosedScheduleViewFilter != null)
            {
                ClosedScheduleViewFilter = new string[5];
                ((DataView)ClosedScheduleView.SourceCollection).RowFilter = "";
                ClosedScheduleView.Refresh();
            }
        }
    }
}
