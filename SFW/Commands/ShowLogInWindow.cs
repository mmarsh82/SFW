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
            if (parameter?.ToString() == "in" && !CurrentUser.IsLoggedIn)
            {
                new View { DataContext = new ViewModel() }.ShowDialog();
            }
            else if (parameter?.ToString() == "reset")
            {
                new View(0) { DataContext = new ViewModel(true) }.ShowDialog();
            }
            else if(parameter?.ToString() == "out")
            {
                CurrentUser.LogOff();
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
