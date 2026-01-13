using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for Pallet_View.xaml
    /// </summary>
    public partial class Pallet_View : Window
    {
        public Pallet_View()
        {
            DataContext = new Pallet_ViewModel();
            InitializeComponent();
        }
    }
}
