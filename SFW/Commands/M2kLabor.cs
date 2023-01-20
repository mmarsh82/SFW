using SFW.Model;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kLabor : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _labWindow = new Labor.View();
            using (Labor.ViewModel _labVM = new Labor.ViewModel((WorkOrder)parameter))
            {
                _labWindow.DataContext = _labVM;
                _labWindow.ShowDialog();
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
