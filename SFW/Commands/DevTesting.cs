using SFW.Deviation;
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
            Deviation.Deviation.CreatePDF(@"\\manage2\prints\1009523.pdf");
        }
        public bool CanExecute(object parameter) => true;
    }
}
