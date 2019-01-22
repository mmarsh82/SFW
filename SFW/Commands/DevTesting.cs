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
            var test = Math.Round(10.0 / 7.0, 3, MidpointRounding.AwayFromZero);
            System.Windows.MessageBox.Show($"{test}");
        }
        public bool CanExecute(object parameter) => true;
    }
}
