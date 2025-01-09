using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for NcrSearch_View.xaml
    /// </summary>
    public partial class NcrSearch_View : Window
    {
        public NcrSearch_View()
        {
            InitializeComponent();
            Loaded += delegate { UserEntryTextBox.Focus(); };
            DataContext = new NcrSearch_ViewModel();
        }
    }
}
