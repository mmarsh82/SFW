using System;
using System.Windows;
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
            MessageBox.Show($"{TimeSpan.Parse("21:00")}\n{DateTime.Now.TimeOfDay}");
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
