using System.Windows;

//Created by Michael Marsh 4-19-18

namespace SFW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            App.splashScreen.LoadComplete();
            BringIntoView();
            Focus();
        }

        private void SourceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new Commands.SafeShutdown().Execute(null);
        }
    }
}
