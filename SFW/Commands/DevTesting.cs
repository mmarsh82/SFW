using System;
using System.IO;
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
            File.WriteAllText($"{App.ErpCon.BTIFolder}LOCXFE{App.ErpCon.AdiServer}.DATTest", "TestFile");
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
