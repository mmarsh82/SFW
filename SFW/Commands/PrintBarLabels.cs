using SFW.Helpers;
using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Windows.Input;

namespace SFW.Commands
{
    public class PrintBarLabels : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            string s = $@"CT~~CD,~CC^~CT~
                        ^XA~TA000~JSN^LT0^MNW^MTD^PON^PMN^LH0,0^JMA^PR5,5~SD15^JUS^LRN^CI0^XZ
                        ^XA
                        ^MMT
                        ^PW609
                        ^LL0406
                        ^LS0
                        ^BY3,3,335^FT591,50^B3I,N,,Y,N
                        ^FD{parameter}^FS
                        ^PQ1,0,1,Y^XZ";

            using (PrintDialog pd = new PrintDialog { PrinterSettings = new PrinterSettings() })
            {
                if (DialogResult.OK == pd.ShowDialog())
                {
                    RawPrinter.SendStringToPrinter(pd.PrinterSettings.PrinterName, s);
                }
            }
        }

        public bool CanExecute(object parameter) => parameter != null;
    }
}
