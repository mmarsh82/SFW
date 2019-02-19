using SFW.Commands;
using SFW.Model;
using System.Linq;
using System.Windows.Input;

namespace SFW.Reports
{
    public class PressShift_ViewModel : ViewModelBase
    {
        #region Properties

        public Press_ShiftReport PSReport { get; set; }

        public string ReportAction { get; set; }

        private RelayCommand _removeCrew;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wo"></param>
        public PressShift_ViewModel(Press_ShiftReport psReport)
        {
            PSReport = psReport;
        }

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
            PSReport.CrewList.Remove(PSReport.CrewList.FirstOrDefault(c => c.IdNumber.ToString() == parameter.ToString()));
        }
        private bool RemoveCrewCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion
    }
}
