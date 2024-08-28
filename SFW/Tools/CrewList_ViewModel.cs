using SFW.Helpers;
using SFW.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SFW.Tools
{
    public class CrewList_ViewModel
    {
        #region Properties

        public ObservableCollection<IList<CrewMember>> CrewCollection;

        private bool IsLoading;

        RelayCommand _removeCrew;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewList_ViewModel()
        {
            IsLoading = true;

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
            //CrewCollection.Remove()
            //WipRecord.CrewList.Remove(WipRecord.CrewList.FirstOrDefault(c => c.IdNumber.ToString() == parameter.ToString()));
        }
        private bool RemoveCrewCanExecute(object parameter) => parameter != null && !string.IsNullOrEmpty(parameter.ToString());

        #endregion
    }
}
