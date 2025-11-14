using SFW.Model.InventoryControl;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SFW.CycleCount
{
    public class Sched_ViewModel : ScheduleBase
    {
        #region Properties

        private bool _emptyCnt;
        public bool EmptyCount
        {
            get
            { return _emptyCnt; }
            set
            {
                _emptyCnt = value;
                OnPropertyChanged(nameof(EmptyCount));
            }
        }

        public string ZoneLocation
        {
            get { return AppGlobal.ZoneLock; }
            set
            {
                if (value != AppGlobal.ZoneLock)
                {
                    AppGlobal.ZoneLock = value;
                }
                OnPropertyChanged(nameof(ZoneLocation));
            }
        }

        #endregion

        /// <summary>
        /// Default Cycle Count ViewModel Constructor
        /// </summary>
        public Sched_ViewModel()
        {
            if (CurrentUser.IsInventoryControl)
            {
                ApplicationTimer.ActionList.Add(Refresh);
                CollectionView = new ListCollectionView(new DataView());
                Initialize();
            }
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
                if (App.LoadedModule == Enumerations.UsersControls.CycleCount)
                {
                    var _dRow = (DataRowView)CollectionView.CurrentItem;
                    if (_dRow != null)
                    {
                        var _cnt = new CountReceipt(_dRow.Row);
                        var _action = new Action(delegate { Controls.WorkSpaceDock.UpdateChildDock(3, 1, new Form_ViewModel(_cnt)); });
                        Application.Current.Dispatcher.Invoke(_action);
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Overridden Refresh action
        /// </summary>
        public override void Refresh()
        {
            if (CollectionView != null)
            {
                base.Refresh();
            }
            else
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initiliazation action for the schedule data
        /// </summary>
        public override void Initialize()
        { 
            try
            {
                CollectionView = new ListCollectionView(new CountReceipt().GetTable(1, App.AppSqlCon).AsDataView());
                EmptyCount = CollectionView.Count == 0;
                Application.Current?.Dispatcher.Invoke(new Action(delegate { CollectionView.Refresh(); }));
                CollectionView.CurrentChanged += CollectionView_ItemChanged;
            }
            catch (Exception)
            { }
        }
    }
}
