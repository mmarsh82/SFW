using System;
using System.Windows.Input;
using SFW.UserLogIn;

namespace SFW.Commands
{
    public class ShowLogInWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter?.ToString() == "in")
            {
                new View { DataContext = new ViewModel() }.ShowDialog();
            }
            else
            {
                CurrentUser.LogOff();
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
