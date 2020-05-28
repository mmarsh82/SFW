using System;
using System.Windows.Input;

namespace SFW.Commands
{
    public class PrintBarLabels : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var _dmdNbr = parameter.ToString().Split('*')[0];
            var _copy = int.TryParse(parameter.ToString().Split('*')[1], out int i) ? i : 1;
            var _prtName = string.Empty;
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer.Contains("GX420"))
                {
                    _prtName = printer;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(_prtName))
            {
                string s = $@"CT~~CD,~CC^~CT~
                            ^XA~TA000~JSN^LT0^MNW^MTD^PON^PMN^LH0,0^JMA^PR6,6~SD15^JUS^LRN^CI0^XZ
                            ^XA
                            ^MMT
                            ^PW609
                            ^LL0406
                            ^LS0
                            ^BY2,3,335^FT431,38^B3I,N,,Y,N
                            ^FDW{_dmdNbr}^FS
                            ^PQ1,0,1,Y^XZ";
                Helpers.RawPrinter.SendStringToPrinter(_prtName, s, _copy);
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
