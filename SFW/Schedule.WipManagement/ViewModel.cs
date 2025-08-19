using SFW.Converters;
using SFW.Model;
using SFW.Model.Production;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;

//Created by Michael Marsh 4-21-18

namespace SFW.Schedule.WipManagement
{
    public class ViewModel : ScheduleBase
    {
        #region Properties



        #endregion

        /// <summary>
        /// Schedule ViewModel default constructor
        /// </summary>
        public ViewModel()
        {
            ApplicationTimer.ActionList.Add(Refresh);
            CollectionView = new ListCollectionView(new DataView());
            Initialize();
        }

        /// <summary>
        /// Tracks the item selections from the CollectionView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionView_ItemChanged(object sender, EventArgs e)
        {
            try
            {
                if (App.LoadedModule == Enumerations.UsersControls.WipManagement)
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        var _wo = new WorkOrder(_dRow.Row);
                        var _action = new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(12, 1, new ShopRoute.WipManagement.ViewModel()); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Reset the filter after a log in event
        /// </summary>
        public override void ResetFilter()
        {
            if (CollectionView != null)
            {
                ((DataView)CollectionView.SourceCollection).RowFilter = GetFilter();
            }
            Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
        }

        /// <summary>
        /// Initialize the production schedule view
        /// </summary>
        public override void Initialize()
        {
            try
            {
                var _tempTable = ModelBase.MasterDataSet.Tables[typeof(WorkOrder).Name];
                foreach (var _keyValPair in UserConfig.GetIROD())
                {
                    DataRow[] _rows = _tempTable.Select($"MachineNumber={_keyValPair.Key}");
                    foreach (DataRow _row in _rows)
                    {
                        var _index = _tempTable.Rows.IndexOf(_row);
                        _tempTable.Rows[_index].SetField("MachineOrder", _keyValPair.Value);
                    }
                }
                if (_tempTable != null)
                {
                    _tempTable.DefaultView.Sort = "MachineOrder ASC";
                }
                CollectionView = new ListCollectionView(_tempTable.AsDataView());
                if (CollectionView.GroupDescriptions.Count() != 0)
                {
                    Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.GroupDescriptions.Clear(); }));
                }
                Application.Current?.Dispatcher.Invoke(new Action(delegate
                {
                    CollectionView.GroupDescriptions.Add(new PropertyGroupDescription("MachineNumber", new WorkCenterNameConverter()));
                }));
                OnPropertyChanged(nameof(CollectionView));
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                CollectionView.CurrentChanged += CollectionView_ItemChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prod Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
