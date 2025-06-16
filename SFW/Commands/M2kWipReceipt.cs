using SFW.Model.Production;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kWipReceipt : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add {  }
            remove { }
        }

        public void Execute(object parameter)
        {
            var _wipWindow = new WIP.View();
            using (WIP.ViewModel wipVM = new WIP.ViewModel((WorkOrder)parameter))
            {
                wipVM.WipQuantity = null;
                _wipWindow.DataContext = wipVM;
                _wipWindow.ShowDialog();
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
