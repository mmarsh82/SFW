using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for ItemLink_View.xaml
    /// </summary>
    public partial class ItemLink_View : Window
    {
        public ItemLink_View()
        {
            InitializeComponent();
            Loaded += delegate { UserEntryTextBox.Focus(); };
        }
    }
}
