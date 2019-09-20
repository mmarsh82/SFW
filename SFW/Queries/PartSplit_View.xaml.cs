using System.Windows;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for PartSplit_View.xaml
    /// </summary>
    public partial class PartSplit_View : Window
    {
        public PartSplit_View()
        {
            InitializeComponent();
            Loaded += delegate { LotInput.Focus(); };
        }
    }
}
