using System.Windows;

namespace SFW.UserLogIn
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : Window
    {
        public View()
        {
            InitializeComponent();
            Loaded += delegate { UserName.Focus(); };
        }
    }
}
