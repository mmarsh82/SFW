using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for CrewList_View.xaml
    /// </summary>
    public partial class CrewList_View : Window
    {
        public CrewList_View()
        {
            DataContext = new CrewList_ViewModel();
            InitializeComponent();
        }
    }
}
