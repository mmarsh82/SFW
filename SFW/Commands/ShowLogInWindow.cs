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
            else if (parameter?.ToString() == "reset")
            {
                new View { DataContext = new ViewModel(true) }.ShowDialog();
            }
            else
            {
                CurrentUser.LogOff();
                if ((ShopRoute.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<ShopRoute.View>().DataContext != null)
                {
                    ((ShopRoute.ViewModel)Controls.WorkSpaceDock.WccoDock.GetChildOfType<ShopRoute.View>().DataContext).UpdateView();
                }
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
