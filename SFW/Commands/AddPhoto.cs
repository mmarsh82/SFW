using Microsoft.Win32;
using SFW.Controls;
using SFW.Model;
using System;
using System.IO;
using System.Linq;
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
                var _ncrId = ((QMS.NcrForm.ViewModel)parameter).NcrObject != null ? ((QMS.NcrForm.ViewModel)parameter).NcrObject.NcrId : 0;
                if (parameter != null)
                {
                    var _folderPath = $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                    var _fileCount = Directory.GetFiles(_folderPath, $"{_ncrId}-*", SearchOption.TopDirectoryOnly).Count();
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.DefaultExt = ".jpg";
                    ofd.Filter = "Photos (.jpg)|*.jpg";
                    var _result = ofd.ShowDialog();
                    if (_result == true)
                    {
                        File.Move(ofd.FileName, $"{_folderPath}{_ncrId}-{_fileCount + 1}.jpg");
                        if (_ncrId > 0)
                        {
                            Ncr.SubmitPhotoPath(_ncrId, $"{_ncrId}-{_fileCount + 1}.jpg", App.AppSqlCon);
                        }
                        ((QMS.NcrForm.ViewModel)parameter).NcrObject.PhotoCollection.Add($"{_folderPath}{_ncrId}-{_fileCount + 1}.jpg");
                        if (((QMS.NcrForm.ViewModel)parameter).FromSchedule)
                        {
                            ((QMS.NcrForm.View)WorkSpaceDock.SchedDock.Children[1]).DataContext = ((QMS.NcrForm.ViewModel)parameter);
                        }
                        else
                        {
                            ((QMS.NcrForm.View)WorkSpaceDock.NcrDock.Children[1]).DataContext = ((QMS.NcrForm.ViewModel)parameter);
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
