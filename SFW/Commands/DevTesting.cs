using System;
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
            parameter = "123546";
            var _test = string.Empty;
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains("GX420"))
                {
                    _test = printer;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(_test))
            {
                string s = $@"CT~~CD,~CC^~CT~
                            ^XA~TA000~JSN^LT0^MNW^MTD^PON^PMN^LH0,0^JMA^PR6,6~SD15^JUS^LRN^CI0^XZ
                            ^XA
                            ^MMT
                            ^PW609
                            ^LL0406
                            ^LS0
                            ^BY2,3,335^FT431,38^B3I,N,,Y,N
                            ^FDW{parameter}^FS
                            ^PQ1,0,1,Y^XZ";
                Helpers.RawPrinter.SendStringToPrinter(_test, s, 1);
            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
