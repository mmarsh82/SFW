using SFW.UserConfig;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ShowUserPrefWindow : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            new View { DataContext = new ViewModel() }.ShowDialog();
        }

        public bool CanExecute(object parameter) => CurrentUser.IsLoggedIn;
    }
}
