using SFW.Helpers;
using SFW.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace SFW.Labor
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WipReceipt WipRecord { get; set; }

        public string LaborQuantity
        {
            get { return WipRecord.WipQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int _iVal) && _iVal > 0)
                {
                    WipRecord.WipQty = _iVal;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    WipRecord.WipQty = null;
                }
                OnPropertyChanged(nameof(LaborQuantity));
                OnPropertyChanged(nameof(WipRecord));
            }
        }

        RelayCommand _removeCrew;
        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ViewModel() 
        { }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="woObject"></param>
        public ViewModel(WorkOrder woObject)
        {
            RefreshTimer.Stop();
            var erpCon = new string[5] { App.ErpCon.HostName, App.ErpCon.UserName, App.ErpCon.Password, App.ErpCon.UniAccount, App.ErpCon.UniService };
            WipRecord = new WipReceipt(CurrentUser.UserIDNbr, CurrentUser.FirstName, CurrentUser.LastName, App.SiteNumber, woObject, erpCon);
        }

        #region Submit ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        private void SubmitExecute(object parameter)
        {
            var _qty = int.TryParse(WipRecord.WipQty.ToString(), out int i) ? i : 0;
            var _machId = Machine.GetMachineNumber(WipRecord.WipWorkOrder.Machine);
            var _crewSize = WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.IdNumber));
            foreach (var _crew in WipRecord.CrewList.Where(o => o.IsDirect))
            {
                //Posting Labor for Time in
                M2kClient.M2kCommand.PostLabor(
                    "SFWLabor"
                    , _crew.IdNumber
                    , _crew.Shift
                    , WipRecord.WipWorkOrder.OrderID
                    , _qty
                    , _machId
                    , 'I'
                    , _crew.Facility
                    , App.ErpCon
                    , _crew.LastClock
                    , _crewSize
                    , DateTime.Now.ToString("MM-dd-yyyy"));
                //Posting Labor for Time out
                M2kClient.M2kCommand.PostLabor(
                    "SFWLabor"
                    , _crew.IdNumber
                    , _crew.Shift
                    , WipRecord.WipWorkOrder.OrderID
                    , _qty
                    , _machId
                    , 'O'
                    , _crew.Facility
                    , App.ErpCon
                    , DateTime.Now.ToString("HH:mm")
                    , _crewSize
                    , DateTime.Now.ToString("MM-dd-yyyy"));
            }
        }
        private bool SubmitCanExecute(object parameter)
        {
            return WipRecord.CrewList.Count(o => o.IsDirect) > 0
                && WipRecord.CrewList.Count(o => o.IsDirect) == WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.LastClock))
                && WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.IdNumber)) == WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.Name))
                && WipRecord.WipQty != null
                && WipRecord.WipQty > 0;
        }

        #endregion

        #region Remove Crew List Item ICommand

        public ICommand RemoveCrewICommand
        {
            get
            {
                if (_removeCrew == null)
                {
                    _removeCrew = new RelayCommand(RemoveCrewExecute, RemoveCrewCanExecute);
                }
                return _removeCrew;
            }
        }

        private void RemoveCrewExecute(object parameter)
        {
            WipRecord.CrewList.Remove(WipRecord.CrewList.FirstOrDefault(c => c.IdNumber.ToString() == parameter.ToString()));
        }
        private bool RemoveCrewCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion
    }
}
