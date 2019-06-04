using System.Windows;

namespace SFW.Tools
{
    /// <summary>
    /// Interaction logic for PriorityEdit_View.xaml
    /// </summary>
    public partial class PriorityEdit_View : Window
    {
        public PriorityEdit_View()
        {
            InitializeComponent();
            Loaded += delegate { ShiftTextBox.Focus(); };
        }
    }
}
