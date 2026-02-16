using SFW.Enumerations;
using System;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ChangeQueState : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            if (parameter != null && parameter.ToString().Contains("^"))
            {
                var _split = parameter.ToString().Split('^');
                var _stateNumber = Enum.TryParse(_split[0], out QueState qs) ? qs : 0;
                if (_stateNumber == QueState.Down)
                {

                }
                
                if (!Model.Production.WorkOrder.UpdateQue(_split[1], (int)_stateNumber))
                {
                    MessageBox.Show("Unable to update the que state.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
