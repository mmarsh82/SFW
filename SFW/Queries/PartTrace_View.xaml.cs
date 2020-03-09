using System.Windows.Controls;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for PartTrace_View.xaml
    /// </summary>
    public partial class PartTrace_View : UserControl
    {
        public PartTrace_View()
        {
            InitializeComponent();
            DataContext = new PartTrace_ViewModel();
            Loaded += delegate { SearchTextBox.Focus(); };
        }
    }
}
