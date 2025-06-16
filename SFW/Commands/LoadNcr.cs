using SFW.Controls;
using System;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    public class LoadNcr : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add {  }
            remove { }
        }

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                var _ncr = new Model.Quality.QmsForm(i);
                var _rev = _ncr.RevisionList.Count();
                WorkSpaceDock.UpdateChildDock(1, 1, new QMS.Form.View { DataContext = new QMS.Form.ViewModel(_ncr, _rev, true) });
            }
            else if (int.TryParse(parameter.ToString().Split(' ')[0], out int n))
            {
                var _ncr = new Model.Quality.QmsForm(n);
                var _rev = _ncr.RevisionList.Count();
                WorkSpaceDock.UpdateChildDock(1, 1, new QMS.Form.View { DataContext = new QMS.Form.ViewModel(_ncr, _rev, true) });
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
