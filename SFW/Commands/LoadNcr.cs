using SFW.Controls;
using SFW.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    public class LoadNcr : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                var _ncr = new Ncr(i);
                var _rev = _ncr.RevisionList.Count();
                WorkSpaceDock.UpdateChildDock(1, 1, new QMS.NcrForm.View { DataContext = new QMS.NcrForm.ViewModel(_ncr, _rev, true) });
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
