using SFW.Controls;
using SFW.Queries;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class PartSpec : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            WorkSpaceDock.SwitchView(7, new PartSpec_ViewModel());
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
