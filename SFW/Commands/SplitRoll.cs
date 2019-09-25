using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class SplitRoll : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _rollSplitWindow = new Queries.PartSplit_View();
            using (Queries.PartSplit_ViewModel _rsVM = new Queries.PartSplit_ViewModel())
            {
                _rollSplitWindow.DataContext = _rsVM;
                _rollSplitWindow.ShowDialog();
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
