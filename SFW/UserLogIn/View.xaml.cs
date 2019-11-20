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

        public View(int i)
        {
            InitializeComponent();
            Loaded += delegate { oldPwdBox.Focus(); };
        }
    }
}
