using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SFW.ShopRoute
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : UserControl
    {
        public View()
        {
            InitializeComponent();
        }

        private void Deviation_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var _oldPath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                var _woNbr = ((ViewModel)DataContext).ShopOrder.OrderNumber;
                var _newPath = $"\\\\fs-wcco\\WCCO-Prints\\Deviations\\{_woNbr}-1.pdf";
                if (((ViewModel)DataContext).ShopOrder.IsDeviated && File.Exists(_newPath))
                {
                    File.Delete(_newPath);
                }
                File.Move(_oldPath, _newPath);
                M2kClient.M2kCommand.EditRecord("WP", _woNbr, 47, "Y", M2kClient.UdArrayCommand.Replace, App.ErpCon);
                var _row = Model.ModelBase.MasterDataSet.Tables["Master"].Select($"[WorkOrder] = '{_woNbr}'").FirstOrDefault();
                var _index = Model.ModelBase.MasterDataSet.Tables["Master"].Rows.IndexOf(_row);
                Model.ModelBase.MasterDataSet.Tables["Master"].Rows[_index].SetField("Deviation", "Y");
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show("Deviation was denied.\nUnable to access the orginal file path.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
