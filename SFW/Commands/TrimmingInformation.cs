using SFW.Queries;
using System;
using System.Linq;
using System.Windows.Input;

namespace SFW.Commands
{
    class TrimmingInformation : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _trimList = Helpers.ExcelReader.GetTrimmingSetupInfo(parameter.ToString(), App.GlobalConfig.FirstOrDefault(o => o.Site == App.Facility).TrimSetup, "Trimming");
            if (_trimList != null)
            {
                new PartTrimInfo_View { DataContext = new PartTrimInfo_ViewModel(_trimList) }.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("No Trimming Setup Information for this part.\nIf you feel you reach this message in error please contact engineering.", "No Trim Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
