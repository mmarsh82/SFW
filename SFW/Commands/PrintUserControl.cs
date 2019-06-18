using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SFW.Commands
{
    public class PrintUserControl : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var printDialog = new PrintDialog();
            printDialog.PrintVisual((Visual)parameter, "SFW Printing.");
        }
        public bool CanExecute(object parameter) => true;
    }
}
