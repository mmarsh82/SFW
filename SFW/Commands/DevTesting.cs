using System;
using System.Diagnostics;
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
            MessageBox.Show(QT9Client.Class1.Execute());
            Process.Start("https://wccobelt.qt9app1.com/documents.aspx?docid=9");
        }
        public bool CanExecute(object parameter) => true;
    }
}
