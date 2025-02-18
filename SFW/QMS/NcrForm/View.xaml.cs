using System.Data;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Controls;
using System.IO;
using SFW.Model;

namespace SFW.QMS.NcrForm
{
    /// <summary>
    /// Interaction logic for NCRForm.xaml
    /// </summary>
    public partial class View : UserControl
    {
        public View()
        {
            InitializeComponent();
            Loaded += delegate { OrderId.Focus(); };
        }

        private void Photo_Drop(object sender, DragEventArgs e)
        {
            if (CurrentUser.IsQuality)
            {
                MainGrid.Opacity = 1;
                AttachPhotoText.Visibility = Visibility.Hidden;
                try
                {
                    var _oldPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                    var _fileExt = Path.GetExtension(_oldPath);
                    if (CurrentUser.IsQuality && _fileExt.ToUpper() == ".JPG")
                    {
                        var _ncr = ((ViewModel)DataContext).NcrObject.NcrId > 0
                            ? ((ViewModel)DataContext).NcrObject.NcrId
                            : ((ViewModel)DataContext).NcrObject.TempId;
                        var _folderPath = ((ViewModel)DataContext).NcrObject.NcrId > 0
                            ? $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\"
                            : $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\Temp\\";
                        var _fileCount = Directory.GetFiles(_folderPath, $"{_ncr}-*", SearchOption.TopDirectoryOnly).Count();
                        var _newPath = $"{_folderPath}{_ncr}-{_fileCount + 1}{_fileExt}";
                        File.Copy(_oldPath, _newPath);
                        ((ViewModel)DataContext).NcrObject.PhotoCollection.Add($"{_folderPath}{_ncr}-{_fileCount + 1}{_fileExt}");
                        if (((ViewModel)DataContext).NcrObject.NcrId > 0)
                        {
                            Ncr.SubmitPhotoPath(((ViewModel)DataContext).NcrObject.NcrId, $"{_ncr}-{_fileCount + 1}{_fileExt}", App.AppSqlCon);
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
        }

        private void Photo_DragOver(object sender, DragEventArgs e)
        {
            if (CurrentUser.IsQuality)
            {
                MainGrid.Opacity = .3;
                AttachPhotoText.Visibility = Visibility.Visible;
                AttachPhotoText.Opacity = 1;
            }
        }

        private void Photo_DragLeave(object sender, DragEventArgs e)
        {
            if (CurrentUser.IsQuality)
            {
                MainGrid.Opacity = 1;
                AttachPhotoText.Visibility = Visibility.Hidden;
            }
        }
    }
}
