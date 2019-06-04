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
            new ShopRoute.Temp.QTask.View().Show();
        }
        public bool CanExecute(object parameter) => true;
    }
}
