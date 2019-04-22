using SFW.Controls;
using SFW.Converters;
using SFW.Model;
using System.Collections.Generic;
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

        public ICollectionView ClosedScheduleView { get; set; }
        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                _selectedWO = value;
                if (value != null)
                {
                    var _wo = new WorkOrder(value.Row, App.AppSqlCon);
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
                ((DataView)ClosedScheduleView.SourceCollection).Table.Search(value);
                _sFilter = value;
                OnPropertyChanged(nameof(SearchFilter));
            }
        }

        public List<Machine> MachineList { get; set; }
        public List<string> MachineGroupList { get; set; }

        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            MachineList = Machine.GetMachineList(App.AppSqlCon, true);
            MachineGroupList = MachineList.Where(o => !string.IsNullOrEmpty(o.MachineGroup)).Select(o => o.MachineGroup).Distinct().ToList();
            var _filter = App.DefualtWorkCenter?.Count > 0 ? App.DefualtWorkCenter.FirstOrDefault(o => o.SiteNumber == App.SiteNumber).MachineNumber : null;
            ClosedScheduleView = CollectionViewSource.GetDefaultView(Machine.GetClosedScheduleData(App.AppSqlCon));
            ClosedScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter(MachineList)));
        }
    }
}
