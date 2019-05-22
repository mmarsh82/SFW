using M2kClient;
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
            M2kCommand.PostLabor("Testing", 2017, 3, "12345*10", 7, "41006", ' ', null, "23:00", 3);
        }
        public bool CanExecute(object parameter) => true;
    }
}
