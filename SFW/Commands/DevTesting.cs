using SFW.Model;
using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            System.Windows.MessageBox.Show(CrewMember.GetLastClockTime(1844, 1, App.AppSqlCon));
        }
        public bool CanExecute(object parameter) => true;
    }
}
