using System;
using System.Windows.Input;
using SFW.ShopRoute.Temp.QTask;

namespace SFW.Commands
{
    public class ViewQtask : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            new View { DataContext = new ViewModel(parameter.ToString()) }.Show();
        }
        public bool CanExecute(object parameter) => true;
    }
}
