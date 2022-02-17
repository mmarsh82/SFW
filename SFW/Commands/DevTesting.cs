using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using M2kClient;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public class TempObject
        {
            //All of the beginning properties are from the excel sheet
            public string PartNumber { get; set; }
            public double CatalogPrice { get; set; }
            public int CatalogNumber { get; set; }
            public string PriceUm { get; set; }
            public int FromQty { get; set; }
            public DateTime EffDate { get; set; }
            //This is an added property so that you dont end up inserting mutiple entries on 1 record
            public int Status { get; set; }
        }

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            try
            {
                //First grab the data from a temp table that I copied from the excel sheet to SQL
                var _tempList = new List<TempObject>();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[TempTable] WHERE [Status] < 1;", App.AppSqlCon))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _tempList.Add(new TempObject
                            {
                                PartNumber = reader.GetString(0)
                                ,CatalogPrice = reader.GetDouble(1)
                                ,CatalogNumber = reader.GetInt32(2)
                                ,PriceUm = reader.GetString(3)
                                ,FromQty = reader.GetInt32(4)
                                ,EffDate = reader.GetDateTime(5)
                                ,Status = reader.GetInt32(6)
                            });
                        }
                    }
                }
                //Write each item to M2k individually and then update the SQL table so that if you interupt the service it will not duplicate records
                foreach (var item in _tempList)
                {
                    //this is the array of attribute numbers we will be modifying and should not be updated
                    var att = new int[5]
                    {
                    41
                    ,88
                    ,89
                    ,90
                    ,91
                    };

                    //this is the list of the new values that will be pulled individually for each item
                    var val = new string[5]
                    {
                    item.CatalogPrice.ToString()
                    ,item.CatalogNumber.ToString()
                    ,item.PriceUm
                    ,item.FromQty.ToString()
                    ,item.EffDate.ToString("MM-dd-yy")
                    };
                    //modification of the record, if a string is returned then you will have an error
                    var _msg = M2kCommand.EditRecord("IM", item.PartNumber, att, val, UdArrayCommand.Insert, App.ErpCon);
                    //if there is no error then you are safe to update the status to 1 in the database and move on
                    if (string.IsNullOrEmpty(_msg))
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE [dbo].[TempTable] SET [Status] = 1 WHERE [PartNumber] = @p1", App.AppSqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", item.PartNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
