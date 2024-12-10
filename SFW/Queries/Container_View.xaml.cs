using System.Windows.Controls;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for Container_View.xaml
    /// </summary>
    public partial class Container_View : UserControl
    {
        public Container_View()
        {
            InitializeComponent();
            DataContext = new Container_ViewModel();
        }
    }
}
