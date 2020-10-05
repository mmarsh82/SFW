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
            if (CurrentUser.IsAdmin || CurrentUser.IsSupervisor || CurrentUser.IsInventoryControl)
            {
                App.DefualtWorkCenter.Clear();
                App.IsFocused = false;
            }
            else
            {
                App.DefualtWorkCenter = App.LoadUserAppConfig();
            }
            Schedule.ViewModel.UserRefresh = true;
            RefreshTimer.RefreshTimerTick();
        }

        public bool CanExecute(object parameter) => true;
    }
}
