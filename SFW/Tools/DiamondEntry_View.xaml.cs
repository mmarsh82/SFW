using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for DiamondEntry_View.xaml
    /// </summary>
    public partial class DiamondEntry_View : Window
    {
        public DiamondEntry_View()
        {
            InitializeComponent();
            Loaded += delegate { UserEntryTextBox.Focus(); };
        }
    }
}
