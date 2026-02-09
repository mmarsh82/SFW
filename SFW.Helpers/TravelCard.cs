using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

//Created 1-22-2019 by Michael Marsh

namespace SFW.Helpers
{
    public enum FormType
    {
        Portrait = 0,
        Landscape = 1,
        CoC = 2
    }
    public static class TravelCard
    {
        #region Properties

        public static string PrinterName { get; set; }
        public static string FilePath { get; set; }
        public static string Password { get; set; }
        public static string PartNbr { get; set; }
        public static string PartDesc { get; set; }
        public static string LotNbr { get; set; }
        public static string DiamondNbr { get; set; }
        public static int Quantity { get; set; }
        public static string Uom { get; set; }
        public static string Ncr { get; set; }
        public static int Weight { get; set; }
        public static string Submitter { get; set; }
        public static bool Deviation { get; set; }
        public static string[] CompPart { get; set; }
        public static string[] CompLot { get; set; }

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
        /// <param name="ncr"></param>
        /// <param name="weight"></param>
        public static void Create(string filePath, string password, string partNbr, string lotNbr, string desc, string dmdNbr, int qty, string uom, string ncr, int weight = 0, string submitter = "", string[] cPart = null, string[] clot = null, bool deviation = false)
        {
            FilePath = filePath;
            Password = password;
            PartNbr = partNbr;
            LotNbr = lotNbr;
            PartDesc = desc;
            DiamondNbr = dmdNbr;
            Quantity = qty;
            Uom = uom;
            Ncr = ncr;
            Weight = weight;
            Deviation = deviation;
            Submitter = submitter;
            CompPart = cPart;
            CompLot = clot;
        }

        /// <summary>
        /// Create a PDF travel card of the object
        /// </summary>
        /// <param name="formType">Type of form that is created</param>
        /// <param name="filePath">Path to the standard travel card</param>
        /// <returns>Successful Creation will return true, with file name.  Failed creation will return false with the error message</returns>
        public static IReadOnlyDictionary<bool, string> CreatePDF(FormType formType, string filePath)
        {
            //TODO:Need to write the parts of this into the global config
            try
            {
                FilePath = filePath;
                var _folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var _fileName = string.IsNullOrEmpty(LotNbr) ? $"{PartNbr}{DateTime.Now:MMyyHHmm}" : $"{LotNbr.Replace("-","")}{DateTime.Now:MMyyHHmm}";
                var _documentPath = $"{_folder}\\SFW\\TravelCard\\{_fileName}.pdf";
                using (PdfReader reader = new PdfReader(FilePath, PdfEncodings.ConvertToBytes(Password, "ASCII")))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream(_documentPath, FileMode.Create)))
                    {
                        if (formType == FormType.Portrait)
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("Date Printed", DateTime.Today.ToString("MM/dd/yyyy"));
                            pdfField.SetField("Time", DateTime.Now.ToString("HH:mm"));
                            pdfField.SetField("P/N", PartNbr);
                            pdfField.SetField("Part No Bar", $"*{PartNbr}*");
                            pdfField.SetField("Part No Bar Sm", $"*{PartNbr}*");
                            if (!string.IsNullOrEmpty(LotNbr))
                            {
                                pdfField.SetField("Lot", LotNbr);
                                pdfField.SetField("Lot Bar", $"*{LotNbr}*");
                                pdfField.SetField("Lot Bar Sm", $"*{LotNbr}*");
                            }
                            pdfField.SetField("Description", PartDesc);
                            if (!string.IsNullOrEmpty(DiamondNbr))
                            {
                                pdfField.SetField("D/N", DiamondNbr);
                            }
                            pdfField.SetField("Qty", Quantity.ToString());
                            pdfField.SetField("UOM", Uom);
                            if (!string.IsNullOrEmpty(Ncr))
                            {
                                pdfField.SetField("QIR", Ncr);
                            }
                            if (Deviation)
                            {
                                pdfField.SetField("Deviation", "!! DEVIATED !!");
                            }
                        }
                        else if (formType == FormType.Landscape)
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("Date", DateTime.Today.ToString("MM/dd/yyyy"));
                            pdfField.SetField("P/N", PartNbr);
                            pdfField.SetField("PartBar", $"*{PartNbr}*");
                            if (!string.IsNullOrEmpty(LotNbr))
                            {
                                pdfField.SetField("L/N", LotNbr);
                                pdfField.SetField("LotBar", $"*{LotNbr}*");
                            }
                            if (!string.IsNullOrEmpty(DiamondNbr))
                            {
                                pdfField.SetField("D/N", DiamondNbr);
                            }
                            pdfField.SetField("Qty", $"{Quantity} {Uom}");
                            if (!string.IsNullOrEmpty(Ncr))
                            {
                                pdfField.SetField("QIR", Ncr);
                            }
                            if (Deviation)
                            {
                                pdfField.SetField("Deviation", "!! DEVIATED !!");
                            }
                        }
                        else if (formType == FormType.CoC)
                        {
                            var pdfField = stamp.AcroFields;
                            pdfField.SetField("Date", DateTime.Today.ToString("MM/dd/yyyy"));
                            pdfField.SetField("Operator", Submitter);
                            pdfField.SetField("Weight", Weight.ToString());
                            pdfField.SetField("Part", PartNbr);
                            pdfField.SetField("Qty", Quantity.ToString());
                            pdfField.SetField("UoM", Uom);
                            pdfField.SetField("Description", PartDesc);
                            pdfField.SetField("Lot", LotNbr);
                            pdfField.SetField("LotBar", $"*{LotNbr}*");
                            if (CompPart != null)
                            {
                                pdfField.SetField("RubPart1", CompPart[0]);
                                pdfField.SetField("RubPart2", CompPart[1]);
                                pdfField.SetField("RubPart3", CompPart[2]);
                                pdfField.SetField("RubPart4", CompPart[3]);
                            }
                            if (CompLot != null)
                            {
                                pdfField.SetField("RubLot1", CompLot[0]);
                                pdfField.SetField("RubLot2", CompLot[1]);
                                pdfField.SetField("RubLot3", CompLot[2]);
                                pdfField.SetField("RubLot4", CompLot[3]);
                            }
                        }
                        stamp.FormFlattening = false;
                    }
                }
                return new Dictionary<bool, string> { { true, _documentPath } };
            }
            catch (Exception ex)
            {
                return new Dictionary<bool, string> { { false, ex.Message } };
            }
        }

        /// <summary>
        /// Print a Travel Card for any Sku object
        /// </summary>
        /// <param name="formType">Type of form to create</param>
        /// <param name="filePath">Path to the standard travel document</param>
        public static void Display(FormType formType, string filePath)
        {
            try
            {
                var _response = CreatePDF(formType, filePath);
                if (_response.ContainsKey(true))
                {
                    _response.TryGetValue(true, out string _documentPath);
                    Process.Start(_documentPath);
                    DeleteDocuments();
                }
                else
                {
                    _response.TryGetValue(false, out string _message);
                    MessageBox.Show(_message, "Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
        ///<param name="filePath">Path to the standard file</param>
        public static string PrintPDF(FormType formType, string filePath)
        {
            try
            {
                var _response = CreatePDF(formType, filePath);
                if (_response.ContainsKey(true))
                {
                    _response.TryGetValue(true, out string _documentPath);
                    using (Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument(_documentPath, "technology#1"))
                    {
                        using (PrintDialog pdialog = new PrintDialog { AllowPrintToFile = true, AllowSomePages = true })
                        {
                            pdialog.PrinterSettings.MinimumPage = 1;
                            pdialog.PrinterSettings.MaximumPage = doc.Pages.Count;
                            pdialog.PrinterSettings.FromPage = 1;
                            pdialog.PrinterSettings.ToPage = doc.Pages.Count;
                            pdialog.PrinterSettings.DefaultPageSettings.Landscape = formType == FormType.Landscape;
                            doc.PageSettings.Orientation = formType == FormType.Landscape ? Spire.Pdf.PdfPageOrientation.Landscape : Spire.Pdf.PdfPageOrientation.Portrait;
                            using (PrintDocument pDoc = doc.PrintDocument)
                            {
                                pdialog.Document = pDoc;
                                pDoc.Print();
                            }
                        }
                    }
                    DeleteDocuments();
                    return string.Empty;
                }
                else
                {
                    return _response.TryGetValue(false, out string _message) ? _message : "Error";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Closes any open temporary documents created for printing
        /// </summary>
        private static void DeleteDocuments(string fileName = "")
        {
            try
            {
                var _folder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\SFW\\TravelCard";
                var _dir = new DirectoryInfo(_folder);
                foreach (var _file in _dir.GetFiles())
                {
                    if (_file.LastAccessTime < DateTime.Now.AddHours(-1))
                    {
                        File.Delete(_file.FullName);
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static double ExtractPDFText(string filePath)
        {
            try
            {
                var docParse = string.Empty;
                var _rtnVal = 0.0;
                using (PdfReader reader = new PdfReader(filePath))
                {
                    ITextExtractionStrategy its = new LocationTextExtractionStrategy();
                    var s = PdfTextExtractor.GetTextFromPage(reader, 1, its);
                    docParse += Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
                    var _index = docParse.IndexOf("Lb/Ft:") + "Lb/Ft:".Length;
                    var _getRtnVal = double.TryParse(docParse.Substring(_index, _index.ToString().Length + 4).Trim(), out _rtnVal);
                }
                return _rtnVal;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PrintZPL(int dpi)
        {
            var _barCode = string.IsNullOrEmpty(LotNbr) ? PartNbr : LotNbr;
            if (!string.IsNullOrEmpty(PrinterName))
            {
                var _zpl = dpi == 300 
                    ? $@"^XA
^MMT
^PW1200
^LL1950
^LS0

^FXCurrent Date
^FT30,80^A0N,50,51
^FC%,{{,#
^FH\^CI28^FD%m/%d/%Y^FS^CI27

^FXDiamond Number
^FT720,80^A0N,50,51^FH\^CI28^FDDiamond:^FS^CI27
^FT970,80^A0N,50,51^FH\^CI28^FD{DiamondNbr}^FS^CI27

^FXProduct Number
^FO30,148^GB1140,4,4^FS
^FT30,225^A0N,58,58^FH\^CI28^FDProduct:^FS^CI27
^FT30,425^A0N,200,200^FH\^CI28^FD{PartNbr}^FS^CI27
^FT30,500^A0N,50,51^FH\^CI28^FD{PartDesc}^FS^CI27

^FXLot Number
^FO30,530^GB1140,4,4^FS
^FT30,600^A0N,58,58^FH\^CI28^FDLot:^FS^CI27
^FT30,750^A0N,150,150^FH\^CI28^FD{LotNbr}^FS^CI27

^FXQuantity with UOM
^FO30,790^GB1140,4,4^FS
^FT30,870^A0N,58,58^FH\^CI28^FDQuantity:^FS^CI27
^FT30,1070^A0N,200,200^FH\^CI28^FD{Quantity} {Uom}^FS^CI27

^FXNCR or SCAR Number
^FO30,1125^GB1140,4,4^FS
^FT30,1200^A0N,58,58^FH\^CI28^FDNCR/SCAR:^FS^CI27
^FT30,1400^A0N,200,200^FH\^CI28^FD{Ncr}^FS^CI27

^FXBarcoded Part or Lot Number
^SL0,1
^BY5,3,300^FT100,1900^B3N,N,,N,N
^FD{_barCode}^FS
^PQ1,0,1,Y

^XZ"
                    : $@"^XA
^MMT
^PW812
^LL1320
^LS0
^FT20,288^A0N,135,134^FH\^CI28^FD{PartNbr}^FS^CI27
^FT20,338^A0N,34,35^FH\^CI28^FD{PartDesc}^FS^CI27
^FO20,360^GB771,0,3^FS
^FO20,100^GB771,0,3^FS
^FT20,509^A0N,102,101^FH\^CI28^FD{LotNbr}^FS^CI27
^FO20,536^GB771,0,3^FS
^FT20,725^A0N,135,134^FH\^CI28^FD{Quantity} {Uom}^FS^CI27
^FT658,54^A0N,34,35^FH\^CI28^FD{DiamondNbr}^FS^CI27
^FT20,151^A0N,39,41^FH\^CI28^FDProduct:^FS^CI27
^FT20,404^A0N,39,41^FH\^CI28^FDLot:^FS^CI27
^FT20,587^A0N,39,41^FH\^CI28^FDQuantity:^FS^CI27
^FO20,760^GB771,0,3^FS
^FT20,810^A0N,39,41^FH\^CI28^FDNCR/SCAR:^FS^CI27
^FT20,948^A0N,135,134^FH\^CI28^FD{Ncr}^FS^CI27
^FT487,54^A0N,34,35^FH\^CI28^FDDiamond:^FS^CI27
^SL0,1
^FT20,54^A0N,34,33
^FC%,{{,#
^FH\^CI28^FD%m/%d/%Y^FS^CI27
^BY3,3,203^FT102,1279^B3N,N,,N,N
^FD{_barCode}^FS
^PQ1,0,1,Y
^XZ
";
                RawPrinter.SendStringToPrinter(PrinterName, _zpl, 1);
            }
        }
    }
}
