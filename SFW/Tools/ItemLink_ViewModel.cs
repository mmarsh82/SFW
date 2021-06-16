using SFW.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SFW.Tools
{
    public class ItemLink_ViewModel
    {
        #region Properties



        RelayCommand _submit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ItemLink_ViewModel()
        { }

        #region Schedule Action ICommand

        public ICommand SchedActionICommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SchedActionExecute, SchedActionCanExecute);
                }
                return _submit;
            }
        }

        private void SchedActionExecute(object parameter)
        {
            
        }
        private bool SchedActionCanExecute(object parameter) => true;

        #endregion
    }
}
