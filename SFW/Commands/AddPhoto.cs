using Microsoft.Win32;
using SFW.Model;
using System;
using System.Data;
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
                if (parameter != null && int.TryParse(parameter.ToString(), out int nRef))
                {
                    var _folderPath = $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                    var _fileCount = Directory.GetFiles(_folderPath, $"{nRef}-*", SearchOption.TopDirectoryOnly).Count();
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.DefaultExt = ".jpg";
                    ofd.Filter = "Photos (.jpg)|*.jpg";
                    var _result = ofd.ShowDialog();
                    if (_result == true)
                    {
                        File.Move(ofd.FileName, $"{_folderPath}{nRef}-{_fileCount + 1}.jpg");
                    }
                    Ncr.SubmitPhotoPath(nRef, $"{nRef}-{_fileCount + 1}.jpg", App.AppSqlCon);
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
