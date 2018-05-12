using SFW.Converters;
using SFW.Model;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public static ICollectionView ScheduleView { get; set; }
        private DataRowView _selectedWO;
        public DataRowView SelectedWorkOrder
        {
            get { return _selectedWO; }
            set
            {
                _selectedWO = value;
                if (value != null)
                {
                    ((ShopRoute.ViewModel)((ShopRoute.View)MainWindowViewModel.WorkSpaceDock.Children[1]).DataContext).ShopOrder = new WorkOrder(value.Row, App.AppSqlCon);
                }
                OnPropertyChanged(nameof(SelectedWorkOrder));
            }
        }

        #endregion

        public ViewModel()
        {
            if (ScheduleView == null)
            {
                ScheduleView = CollectionViewSource.GetDefaultView(Machine.GetScheduleData(App.AppSqlCon));
                ScheduleView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter()));
            }
        }
    }
}
