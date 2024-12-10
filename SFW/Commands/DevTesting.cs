using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            try
            {
                LdapConnection adDatabaseCon = new LdapConnection("LDAP://WAX2C201A.tiretech2.contiwan.com/OU=wak1,OU=us,OU=lda,DC=tiretech2,DC=contiwan,DC=com");
                NetworkCredential credential = new NetworkCredential("tiretech2\\uif89547", "Law-of-change-think@!");
                adDatabaseCon.Credential = credential;
                adDatabaseCon.Bind();
                MessageBox.Show("You are logged in successfully!");
            }
            catch (LdapException lexc)
            {
                String error = lexc.ServerErrorMessage;
                // MessageBox.Show(lexc);
            }
            catch (Exception exc)
            {
                // MessageBox.Show(exc);
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
