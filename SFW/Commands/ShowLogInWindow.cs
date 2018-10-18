using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ShowLogInWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter?.ToString() == "in")
            {
                new LogInWindow { DataContext = new LogInWindowViewModel() }.ShowDialog();
            }
            else
            {
                CurrentUser.LogOff();
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
