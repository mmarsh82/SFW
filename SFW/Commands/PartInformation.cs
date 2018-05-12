using System;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class PartInformation : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Part Information ICommand execution
        /// </summary>
        /// <param name="parameter">Skew object</param>
        public void Execute(object parameter)
        {
            try
            {
                MainWindowViewModel.WorkSpaceDock.Children.Clear();
                MainWindowViewModel.WorkSpaceDock.Children.Add(new Queries.PartInfo_View());
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
