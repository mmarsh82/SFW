using System.Windows;

namespace SFW.Queries
{
    /// <summary>
    /// Interaction logic for UnplanScrap_View.xaml
    /// </summary>
    public partial class UnplanScrap_View : Window
    {
        public UnplanScrap_View()
        {
            InitializeComponent();
            Loaded += delegate { LotInput.Focus(); };
        }
    }
}
