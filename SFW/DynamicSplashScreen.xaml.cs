using System.Windows;

namespace SFW
{
    /// <summary>
    /// Interaction logic for DynamicSplashScreen.xaml
    /// </summary>
    public partial class DynamicSplashScreen : Window, ISplashScreen
    {
        #region ISplashScreen Implementation

        public void AddMessage(string message)
        {
            Dispatcher.Invoke(delegate ()
            {
                UpdateMessage.Text = message;
            });
        }

        public void LoadComplete()
        {
            Dispatcher.InvokeShutdown();
        }

        #endregion

        public int Site { get; set; }

        public DynamicSplashScreen()
        {
            Site = CurrentUser.GetSite();
            InitializeComponent();
        }
    }
}
