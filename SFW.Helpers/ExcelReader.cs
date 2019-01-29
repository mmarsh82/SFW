using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SFW.Helpers
{
    public class ExcelReader
    {
        public static void ReadSetup(string partNbr, string machineName)
        {
            try
            {
                var ssPack = Package.Open("\\\\manage2\\server\\Technology\\Program Testing\\SFW_press setup and part number crossreference.xlsm", FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var ssDoc = SpreadsheetDocument.Open(ssPack))
                {
                    //TODO: Add in the search logic
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
