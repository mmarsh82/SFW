using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace SFW_Service
{
    [RunInstaller(true)]
    public partial class SFWInstaller : Installer
    {
        public SFWInstaller()
        {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            string parameter = "SFWSource\" \"SFWLogFile";
            Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" \"" + parameter + "\"";
            base.OnBeforeInstall(savedState);
        }

        private void sfwServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void sfwServiceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
