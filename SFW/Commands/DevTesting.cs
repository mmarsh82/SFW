using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Input;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /*public class TempObject
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
        }*/

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            var test = new Model.XMLTesting();
        }
        public bool CanExecute(object parameter) => true;
    }
}
