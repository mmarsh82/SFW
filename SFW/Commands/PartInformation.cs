using SFW.Queries;
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
        /// <param name="parameter">Skew object or Skew Number</param>
        public void Execute(object parameter)
        {
            try
            {
                MainWindowViewModel.WorkSpaceDock.Children.Clear();
                if (parameter != null)
                {
                    if(parameter.GetType() == typeof(Model.WorkOrder))
                    {
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new PartInfo_View { DataContext = new PartInfo_ViewModel((Model.WorkOrder)parameter) });
                    }
                    else
                    {
                        MainWindowViewModel.WorkSpaceDock.Children.Add(new PartInfo_View { DataContext = new PartInfo_ViewModel(parameter.ToString()) });
                    }
                }
                else
                {
                    MainWindowViewModel.WorkSpaceDock.Children.Add(new PartInfo_View());
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
