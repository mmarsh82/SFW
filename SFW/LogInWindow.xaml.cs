using System.Windows;

namespace SFW
{
    /// <summary>
    /// Interaction logic for LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        public LogInWindow()
        {
            InitializeComponent();
            Loaded += delegate { UserName.Focus(); };
        }
    }
}
