using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for ErpUpload_View.xaml
    /// </summary>
    public partial class ErpUpload_View : Window
    {
        public ErpUpload_View()
        {
            InitializeComponent();
            DataContext = new ErpUpload_ViewModel();
        }
    }
}
