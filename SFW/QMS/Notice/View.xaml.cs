using System.Windows.Controls;

namespace SFW.QMS.Notice
{
    /// <summary>
    /// Interaction logic for NCRNoticeView.xaml
    /// </summary>
    public partial class View : UserControl
    {
        public View()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
