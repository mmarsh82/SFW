using System.Windows.Controls;

namespace SFW.CycleCount
{
    /// <summary>
    /// Interaction logic for CC_View.xaml
    /// </summary>
    public partial class Sched_View : UserControl
    {
        public Sched_View()
        {
            InitializeComponent();
            DataContext = new Sched_ViewModel();
        }
    }
}
