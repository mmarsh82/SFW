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
            var test = "1901-1234";
            System.Windows.MessageBox.Show($"{test.Substring(test.IndexOf('-') +1)}");
        }
        public bool CanExecute(object parameter) => true;
    }
}
