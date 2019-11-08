using System;
using System.Windows.Input;
using SFW.PassReset;

namespace SFW.Commands
{
    public class ShowPassResetWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter?.ToString() == "reset")
            {
                using (var dc = new ViewModel())
                {
                    new View { DataContext = dc }.ShowDialog();
                }
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
