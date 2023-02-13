using System;
using System.IO;
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
                File.Move(_oldPath, _newPath);
                M2kClient.M2kCommand.EditRecord("WP", _woNbr, 47, "Y", M2kClient.UdArrayCommand.Replace, App.ErpCon);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
