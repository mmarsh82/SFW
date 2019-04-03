using System.Windows.Controls;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for PartDetail_View.xaml
    /// </summary>
    public partial class PartDetail_View : UserControl
    {
        public PartDetail_View()
        {
            InitializeComponent();
            DataContext = new PartDetail_ViewModel();
            Loaded += delegate { LotTextBox.Focus(); };
        }
    }
}
