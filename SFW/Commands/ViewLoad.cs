using System;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class ViewLoad : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// View Load Execution
        /// </summary>
        /// <param name="parameter">View to Load</param>
        public void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "Schedule":
                        MainWindowViewModel.WorkSpaceDock.Children.Clear();
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View());
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                    case "Scheduler":
                        MainWindowViewModel.WorkSpaceDock.Children.Clear();
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Scheduler.View { DataContext = new Scheduler.ViewModel() });
                        break;
                    default:
                        MainWindowViewModel.WorkSpaceDock.Children.Clear();
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new Schedule.View { DataContext = new Schedule.ViewModel(parameter.ToString()) });
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new ShopRoute.View { DataContext = new ShopRoute.ViewModel() });
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
