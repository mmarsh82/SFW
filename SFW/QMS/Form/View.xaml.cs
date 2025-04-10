using System.Data;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Controls;
using System.IO;
using SFW.Model;

namespace SFW.QMS.Form
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
                        var _frm = ((ViewModel)DataContext).FormObject.FormId > 0
                            ? ((ViewModel)DataContext).FormObject.FormId
                            : ((ViewModel)DataContext).FormObject.TempId;
                        var _folderPath = ((ViewModel)DataContext).FormObject.FormId > 0
                            ? $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\"
                            : $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\Temp\\";
                        var _fileCount = Directory.GetFiles(_folderPath, $"{_frm}-*", SearchOption.TopDirectoryOnly).Count();
                        var _newPath = $"{_folderPath}{_frm}-{_fileCount + 1}{_fileExt}";
                        File.Copy(_oldPath, _newPath);
                        ((ViewModel)DataContext).FormObject.PhotoCollection.Add($"{_folderPath}{_frm}-{_fileCount + 1}{_fileExt}");
                        if (((ViewModel)DataContext).FormObject.FormId > 0)
                        {
                            Model.QmsForm.SubmitPhotoPath(((ViewModel)DataContext).FormObject.FormId, $"{_frm}-{_fileCount + 1}{_fileExt}", App.AppSqlCon);
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
