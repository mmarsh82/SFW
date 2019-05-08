using System.Windows.Controls;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for PartSpec_View.xaml
    /// </summary>
    public partial class PartSpec_View : UserControl
    {
        public PartSpec_View()
        {
            InitializeComponent();
            DataContext = new PartSpec_ViewModel();
        }
    }
}
