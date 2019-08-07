using SFW.Queries;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class UnplanScrap : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            new UnplanScrap_View { DataContext = new UnplanScrap_ViewModel() }.ShowDialog();
        }

        public bool CanExecute(object parameter) => true;
    }
}
