using System.Windows.Controls;

namespace SFW.Schedule
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : UserControl
    {
        public View()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
