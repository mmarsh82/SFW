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
            string s = $@"CT~~CD,~CC^~CT~
                            ^XA~TA000~JSN^LT0^MNW^MTD^PON^PMN^LH0,0^JMA^PR6,6~SD15^JUS^LRN^CI0^XZ
                            ^XA
                            ^MMT
                            ^PW609
                            ^LL0406
                            ^LS0
                            ^BY2,3,335^FT431,38^B3I,N,,Y,N
                            ^FD{parameter}^FS
                            ^PQ1,0,1,Y^XZ";

            using (PrintDialog pd = new PrintDialog { PrinterSettings = new PrinterSettings() })
            {
                if (DialogResult.OK == pd.ShowDialog())
                {
                    RawPrinter.SendStringToPrinter(pd.PrinterSettings.PrinterName, s, pd.PrinterSettings.Copies);
                }
            }
        }

        public bool CanExecute(object parameter) => true;
    }
}
