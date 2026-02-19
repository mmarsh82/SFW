using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for DiamondEntry_View.xaml
    /// </summary>
    public partial class DownReason_View : Window
    {
        public DownReason_View(string orderId, string workCenter)
        {
            DataContext = new DownReason_ViewModel(workCenter, orderId);
            InitializeComponent();
        }
    }
}
