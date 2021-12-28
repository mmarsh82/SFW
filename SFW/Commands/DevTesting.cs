using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
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
            if (App.AppSqlCon != null && App.AppSqlCon.State != ConnectionState.Closed && App.AppSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (DataTable dt = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT [ID] FROM [WCCO_MAIN].[dbo].[RT-INIT] WHERE [Total_Mach_Hrs] IS NULL", App.AppSqlCon))
                        {
                            adapter.Fill(dt);
                        }

                        foreach (DataRow dr in dt.Rows)
                        {
                            using (SqlCommand cmd = new SqlCommand($@"UPDATE [WCCO_MAIN].[dbo].[RT-INIT]
                                                                        SET[Total_Mach_Hrs] = [dbo].[GetMachineHours](SUBSTRING([ID], 0, CHARINDEX('*', [ID], 0)), SUBSTRING([ID], CHARINDEX('*', [ID], 0) + 1, LEN([ID])))
                                                                    WHERE
                                                                        [ID] = '{dr.Field<string>("ID")}'", App.AppSqlCon))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    using (SqlDataAdapter adapter = new SqlDataAdapter("", App.AppSqlCon))
                    {

                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }


            /*
            var qConTest = new QT9Client.QT9Connection("https://wccobelt.qt9app1.com", "qt9sa", "4WCKxqkFVn26bjaj", "WCCO");
            QT9Client.QT9Request.Create(QT9Client.QT9Services.wsDocuments, QT9Client.wsDocuments.GetAllDocumentsAsDataSet, qConTest);
            var test = QT9Client.QT9Request.GetResponse(qConTest, 0);
            if (test.ToString().StartsWith("ERROR:"))
            {
                System.Windows.MessageBox.Show(test.ToString().Replace("ERROR:", ""), "SOAP Exception", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            QT9Client.QT9Request.Create(QT9Client.QT9Services.wsAuthenticate, QT9Client.wsAuthenticate.LogUserOut, qConTest);
            QT9Client.QT9Request.GetResponse(qConTest, qConTest.UserName);
            if (test.GetType() == typeof(DataSet))
            {

            }
            else
            {

            }
            var testing = string.Empty;
            QT9Client.QT9Request.Create(QT9Client.QT9Services.wsAuthenticate, QT9Client.wsAuthenticate.AuthenticateUser, qConTest);
            QT9Client.QT9Request.GetResponse(qConTest, qConTest.UserName, qConTest.Password);
            Process.Start($"https://wccobelt.qt9app1.com/documents.aspx?docid=1");
            QT9Client.QT9Request.Create(QT9Client.QT9Services.wsAuthenticate, QT9Client.wsAuthenticate.LogUserOut, qConTest);
            QT9Client.QT9Request.GetResponse(qConTest, qConTest.UserName);
            */
        }
        public bool CanExecute(object parameter) => true;
    }
}
