using System;
using System.Collections.Generic;
using System.IO;
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
            var _files = Directory.GetFiles("\\\\fs-csi\\CSI-Prints\\Part", "*.pdf", SearchOption.TopDirectoryOnly);
            var _test = new Dictionary<string, double>();
            foreach (var _file in _files)
            {
                _test.Add(Path.GetFileName(_file).Replace(".pdf", ""), Helpers.TravelCard.ExtractPDFText(_file));
            }
            var end = 0;
        }
        public bool CanExecute(object parameter) => true;
    }
}
