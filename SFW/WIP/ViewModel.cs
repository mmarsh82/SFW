using SFW.Commands;
using SFW.Model;
using System.Windows.Input;

//Created by Michael Marsh 10-23-18

namespace SFW.WIP
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WipReceipt WipRecord { get; set; }
        public bool HasCrew { get { return WipRecord.CrewList != null; } }

        RelayCommand _wip;

        #endregion

        /// <summary>
        /// WIP ViewModel Default Constructor
        /// </summary>
        public ViewModel(WorkOrder woObject)
        {
            WipRecord = new WipReceipt(CurrentUser.DomainUserName, woObject, App.AppSqlCon);
        }

        #region Process Wip ICommand

        public ICommand WipICommand
        {
            get
            {
                if (_wip == null)
                {
                    _wip = new RelayCommand(WipExecute, WipCanExecute);
                }
                return _wip;
            }
        }

        private void WipExecute(object parameter)
        {
            M2kClient.M2kCommand.ProductionWip(WipRecord, App.ErpCon);
        }
        private bool WipCanExecute(object parameter) => true;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }
    }
}
