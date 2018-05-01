using System.Windows.Controls;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for ItemsLot_View.xaml
    /// </summary>
    public partial class PartInfo_View : UserControl
    {
        public PartInfo_View()
        {
            InitializeComponent();
            DataContext = new PartInfo_ViewModel();
        }
    }
}
