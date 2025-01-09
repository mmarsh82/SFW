using System.Data;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Controls;
using System.IO;

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
            try
            {
                if (((ViewModel)DataContext).NcrObject?.NcrId > 0)
                {
                    var _oldPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                    var _fileExt = Path.GetExtension(_oldPath);
                    var _ncr = ((ViewModel)DataContext).NcrObject.NcrId;
                    var _folderPath = $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                    var _fileCount = Directory.GetFiles(_folderPath, $"{_ncr}-*", SearchOption.TopDirectoryOnly).Count();
                    var _newPath = $"{_folderPath}{_ncr}-{_fileCount + 1}{_fileExt}";
                    File.Move(_oldPath, _newPath);
                    ((ViewModel)DataContext).NcrObject.PhotoCollection.Add($"{_ncr}-{_fileCount + 1}{_fileExt}");
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
}
