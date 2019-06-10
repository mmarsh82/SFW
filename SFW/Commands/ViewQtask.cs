using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ViewQtask : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            new ShopRoute.Temp.QTask.View { DataContext = new ShopRoute.Temp.QTask.ViewModel(parameter.ToString()) }.ShowDialog();
        }
        public bool CanExecute(object parameter) => true;
    }
}
