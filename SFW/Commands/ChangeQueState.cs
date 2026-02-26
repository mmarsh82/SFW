using SFW.Enumerations;
using SFW.Model.Production;
using SFW.Tools;
using System;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class ChangeQueState : ICommand
    {
        public static bool IsChanged;

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
                var _change = true;
                var _curState = Enum.TryParse(WorkOrder.GetQueState(_split[1]).ToString(), out qs) ? qs : 0;
                if (_stateNumber == QueState.Down)
                {
                    IsChanged = false;
                    new DownReason_View(_split[1], _split[2]).ShowDialog();
                    _change = IsChanged;
                }
                else if (_curState == QueState.Down)
                {
                    Machine.UpdateDownReason(CurrentUser.ErpId, _split[2], _split[1], App.AppSqlCon);
                    _change = true;
                }
                if (_change)
                {
                    if (!WorkOrder.UpdateQue(_split[1], (int)_stateNumber))
                    {
                        MessageBox.Show("Unable to update the que state.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ApplicationTimer.Resume();
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
