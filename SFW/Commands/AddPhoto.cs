using Microsoft.Win32;
using SFW.Controls;
using SFW.Model;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class AddPhoto : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                var _ncrId = 0;
                var _folderPath = "";
                var _isNew = ((QMS.Form.ViewModel)parameter).FormObject.FormId == 0;
                if (((QMS.Form.ViewModel)parameter).FormObject != null)
                {
                    _ncrId = ((QMS.Form.ViewModel)parameter).FormObject.FormId > 0 
                        ? ((QMS.Form.ViewModel)parameter).FormObject.FormId 
                        : ((QMS.Form.ViewModel)parameter).FormObject.TempId;

                    _folderPath = ((QMS.Form.ViewModel)parameter).FormObject.FormId > 0
                        ? "\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\"
                        : "\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\Temp\\";
                }
                if (parameter != null && _ncrId > 0)
                {
                    var _fileCount = Directory.GetFiles(_folderPath, $"{_ncrId}-*", SearchOption.TopDirectoryOnly).Count();
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.DefaultExt = ".jpg";
                    ofd.Filter = "Photos (.jpg)|*.jpg";
                    var _result = ofd.ShowDialog();
                    if (_result == true)
                    {
                        File.Copy(ofd.FileName, $"{_folderPath}{_ncrId}-{_fileCount + 1}.jpg");
                        if (!_isNew)
                        {
                            QmsForm.SubmitPhotoPath(_ncrId, $"{_ncrId}-{_fileCount + 1}.jpg", App.AppSqlCon);
                        }
                        ((QMS.Form.ViewModel)parameter).FormObject.PhotoCollection.Add($"{_folderPath}{_ncrId}-{_fileCount + 1}.jpg");
                        if (((QMS.Form.ViewModel)parameter).FromSchedule)
                        {
                            ((QMS.Form.View)WorkSpaceDock.SchedDock.Children[1]).DataContext = ((QMS.Form.ViewModel)parameter);
                        }
                        else
                        {
                            ((QMS.Form.View)WorkSpaceDock.QmsFormDock.Children[1]).DataContext = ((QMS.Form.ViewModel)parameter);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Linking was denied.\nUnable to access the orginal file path.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
