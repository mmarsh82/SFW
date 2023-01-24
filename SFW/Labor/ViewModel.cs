using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SFW.Labor
{ 
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WipReceipt WipRecord { get; set; }

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
            WipRecord = new WipReceipt(CurrentUser.UserIDNbr, CurrentUser.FirstName, CurrentUser.LastName, CurrentUser.Facility, woObject, erpCon);
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
            //foreach (var _crew in WipRecord.CrewList.Where())
            //M2kClient.M2kCommand.PostLabor()
        }
        private bool SubmitCanExecute(object parameter)
        {
            return WipRecord.CrewList.Count(o => o.IsDirect) > 0 
                && WipRecord.CrewList.Count(o => o.IsDirect) == WipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.LastClock)) 
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
