using iTextSharp.text.pdf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

//Created 1-22-2019 by Michael Marsh

namespace SFW.Helpers
{
    public enum FormType
    {
        Portrait = 0,
        Landscape = 1
    }
    public static class TravelCard
    {
        #region Properties

        public static string FilePath { get; set; }
        public static string Password { get; set; }
        public static string PartNbr { get; set; }
        public static string LotNbr { get; set; }
        public static string Desc { get; set; }
        public static string DiamondNbr { get; set; }
        public static int Quantity { get; set; }
        public static string Uom { get; set; }
        public static int QirNbr { get; set; }

        #endregion

        /// <summary>
        /// Create a travel card object
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password"></param>
        /// <param name="partNbr"></param>
        /// <param name="lotNbr"></param>
        /// <param name="desc"></param>
        /// <param name="dmdNbr"></param>
        /// <param name="qty"></param>
        /// <param name="uom"></param>
        /// <param name="qirNbr"></param>
        public static void Create(string filePath, string password, string partNbr, string lotNbr, string desc, string dmdNbr, int qty, string uom, int qirNbr)
        {
            FilePath = filePath;
            Password = password;
            PartNbr = partNbr;
            LotNbr = lotNbr;
            Desc = desc;
            DiamondNbr = dmdNbr;
            Quantity = qty;
            Uom = uom;
            QirNbr = qirNbr;
        }

        /// <summary>
        /// Create a PDF travel card of the object
        /// </summary>
        /// <param name="formType">Type of form to create 0 = Portrait, 1 = Landscape</param>
        /// <returns>Successful completion of file creation</returns>
        public static bool CreatePDF(FormType formType)
        {
            try
            {
                switch (formType)
                {
                    case FormType.Portrait:
                        FilePath = "\\\\manage2\\server\\Document Center\\Production\\FORM5125 - Travel Card.pdf";
                        break;
                    case FormType.Landscape:
                        FilePath = "\\\\manage2\\server\\Document Center\\Production\\FORM5127 - Reference Travel Card.pdf";
                        break;
                }
                using (PdfReader reader = new PdfReader(FilePath, PdfEncodings.ConvertToBytes(Password, "ASCII")))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf", FileMode.Create)))
                    {
                        if (formType == FormType.Portrait)
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("Date Printed", DateTime.Today.ToString("MM/dd/yyyy"));
                            pdfField.SetField("P/N", PartNbr);
                            pdfField.SetField("Part No Bar", $"*{PartNbr}*");
                            pdfField.SetField("Part No Bar Sm", $"*{PartNbr}*");
                            if (!string.IsNullOrEmpty(LotNbr))
                            {
                                pdfField.SetField("Lot", LotNbr);
                                pdfField.SetField("Lot Bar", $"*{LotNbr}*");
                                pdfField.SetField("Lot Bar Sm", $"*{LotNbr}*");
                            }
                            pdfField.SetField("Description", Desc);
                            if (!string.IsNullOrEmpty(DiamondNbr))
                            {
                                pdfField.SetField("D/N", DiamondNbr);
                            }
                            pdfField.SetField("Qty", Quantity.ToString());
                            pdfField.SetField("UOM", Uom);
                            if (QirNbr > 0)
                            {
                                pdfField.SetField("QIR", QirNbr.ToString());
                                pdfField.SetField("QIR Bar", $"*{QirNbr}*");
                                pdfField.SetField("QIR Bar Sm", $"*{QirNbr}*");
                            }
                        }
                        else if (formType == FormType.Landscape)
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("P/N", PartNbr);
                            if (!string.IsNullOrEmpty(LotNbr))
                            {
                                pdfField.SetField("L/N", LotNbr);
                            }
                            if (!string.IsNullOrEmpty(DiamondNbr))
                            {
                                pdfField.SetField("D/N", DiamondNbr);
                            }
                            pdfField.SetField("Qty", Quantity.ToString());
                            if (QirNbr > 0)
                            {
                                pdfField.SetField("QIR", QirNbr.ToString());
                            }
                        }
                        stamp.FormFlattening = false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Print a Travel Card for any Sku object
        /// </summary>
        public static void Display(FormType formType)
        {
            try
            {
                CreatePDF(formType);
                Process.Start($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf");
                CloseOpenDocuments();
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Print any travel card in the form of a pdf
        /// </summary>
        ///<param name="formType"></param>
        public static string PrintPDF(FormType formType)
        {
            var _adobeApp = string.Empty;
            try
            {
                var _adobeFilePath = string.Empty;
                if (File.Exists(@"C:\Program Files (x86)\Adobe\Acrobat DC\Acrobat\Acrobat.exe"))
                {
                    _adobeFilePath = @"C:\Program Files (x86)\Adobe\Acrobat DC\Acrobat\Acrobat.exe";
                    _adobeApp = "Acrobat";
                }
                else if (File.Exists(@"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe"))
                {
                    _adobeFilePath = @"C:\Program Files (x86)\Adobe\Acrobat Reader DC\Reader\AcroRd32.exe";
                    _adobeApp = "AcroRd32";
                }
                else
                {
                    return "No Adobe application installed, please contact IT.";
                }
                CreatePDF(formType);
                var _filePath = $"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf";
                Process proc = new Process();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                proc.StartInfo.Verb = "print";
                proc.StartInfo.FileName = _adobeFilePath;
                proc.StartInfo.Arguments = $@"/p /h {_filePath}";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                if (proc.HasExited == false)
                {
                    proc.WaitForExit(6000);
                }
                proc.EnableRaisingEvents = true;
                proc.Close();
                KillAdobe(_adobeApp);
                CloseOpenDocuments();
                return string.Empty;
            }
            catch (Exception ex)
            {
                KillAdobe(_adobeApp);
                return ex.Message;
            }
        }

        /// <summary>
        /// When silent printing any Adobe PDF you will find that it will sometimes stay open, this will kill the application
        /// </summary>
        /// <param name="name">File name</param>
        private static void KillAdobe(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses().Where(
                         clsProcess => clsProcess.ProcessName.StartsWith(name)))
            {
                clsProcess.Kill();
                return;
            }
            return;
        }

        /// <summary>
        /// Closes any open temporary documents created for printing
        /// </summary>
        private static void CloseOpenDocuments()
        {
            foreach (var f in Directory.GetFiles("\\\\manage2\\server\\OMNI\\Application Data\\temp\\"))
            {
                try
                {
                    if (f != $"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf")
                    {
                        File.Delete(f);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
