using System.Windows;

namespace SFW.Reports
{
    /// <summary>
    /// Interaction logic for Press_View.xaml
    /// </summary>
    public partial class Press_View : Window
    {
        public Press_View()
        {
            InitializeComponent();
            Loaded += delegate { TransferSlat.Focus(); };
        }
    }
}
