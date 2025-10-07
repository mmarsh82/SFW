using SFW.Controls;
using SFW.Model.Quality;
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
            var _ncr = new QmsForm();
            var _rev = 0;
            if (int.TryParse(parameter.ToString(), out int i))
            {
                _ncr = new QmsForm(i);
                _rev = _ncr.RevisionList.Count();
            }
            else if (int.TryParse(parameter.ToString().Split(' ')[0], out int n))
            {
                _ncr = new QmsForm(n);
                _rev = _ncr.RevisionList.Count();
            }
            else if (parameter.ToString().Contains('^'))
            {
                if (int.TryParse(parameter.ToString().Split('^')[0], out int _id))
                {
                    _ncr = new QmsForm(_id, parameter.ToString().Split('^')[1]);
                    _rev = _ncr.RevisionList.Count();
                }
            }

            if (!string.IsNullOrEmpty(_ncr.OrderId) && _rev != 0)
            {
                switch (App.LoadedModule)
                {
                    case Enumerations.UsersControls.Schedule:
                        WorkSpaceDock.UpdateChildDock(1, 1, new QMS.Form.View { DataContext = new QMS.Form.ViewModel(_ncr, _rev, true) });
                        break;
                    case Enumerations.UsersControls.Plan:
                        WorkSpaceDock.UpdateChildDock(11, 1, new QMS.Form.View { DataContext = new QMS.Form.ViewModel(_ncr, _rev, true) });
                        break;
                }
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
