using System.Windows;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for PartLabel_View.xaml
    /// </summary>
    public partial class PartLabel_View : Window
    {
        public PartLabel_View()
        {
            InitializeComponent();
            DataContext = new PartLabel_ViewModel();
            Loaded += delegate { DmdInput.Focus(); };
        }
    }
}
