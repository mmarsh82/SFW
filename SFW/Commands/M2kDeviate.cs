using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class M2kDeviate : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                if (parameter != null)
                {
                    var _wpNbr = ((DataRowView)parameter).Row.Field<string>("WorkOrder");
                    var _filePath = $"\\\\waxfs001\\WAXG-Wahpeton\\Prints\\Deviations\\{_wpNbr}-1.pdf";
                    var _row = Model.ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{_wpNbr}'").FirstOrDefault();
                    var _index = Model.ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row);
                    if (((DataRowView)parameter).Row.Field<string>("Deviation") == "Y")
                    {
                        File.Delete(_filePath);
                        M2kClient.M2kCommand.EditRecord("WP", _wpNbr, 47, "N", M2kClient.UdArrayCommand.Replace, App.ErpCon);
                        Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Deviation", "N");
                    }
                    else
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.DefaultExt = ".pdf";
                        ofd.Filter = "Part Prints (.pdf)|*.pdf";
                        var _result = ofd.ShowDialog();
                        if (_result == true)
                        {
                            File.Move(ofd.FileName, _filePath);
                        }
                        M2kClient.M2kCommand.EditRecord("WP", _wpNbr, 47, "Y", M2kClient.UdArrayCommand.Replace, App.ErpCon);
                        Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Deviation", "Y");
                    }
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show("Deviation was denied.\nUnable to access the orginal file path.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Unhandled Exception");
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
