using System;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class ScheduleLoad : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Schedule Load Execution
        /// </summary>
        /// <param name="parameter">Skew object</param>
        public void Execute(object parameter)
        {
            try
            {
                MainWindowViewModel.WorkSpaceDock.Children.Clear();
                MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View());
                MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
