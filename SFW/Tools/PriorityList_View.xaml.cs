using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for PriorityList_View.xaml
    /// </summary>
    public partial class PriorityList_View : Window
    {
        public PriorityList_View()
        {
            InitializeComponent();
            DataContext = new PriorityList_ViewModel();
        }
    }
}
