using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Security;
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

        public static string FilePath { get; set; }
        public static string Password { get; set; }
        public static string PartNbr { get; set; }
        public static string LotNbr { get; set; }
        public static string Desc { get; set; }
        public static string DiamondNbr { get; set; }
        public static int Quantity { get; set; }
        public static string Uom { get; set; }
        public static int QirNbr { get; set; }
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
        /// <param name="qirNbr"></param>
        /// <param name="weight"></param>
        public static void Create(string filePath, string password, string partNbr, string lotNbr, string desc, string dmdNbr, int qty, string uom, int qirNbr, int weight = 0, string submitter = "", string[] cPart = null, string[] clot = null, bool deviation = false)
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
            Weight = weight;
            Deviation = deviation;
            Submitter = submitter;
            CompPart = cPart;
            CompLot = clot;
        }

        /// <summary>
        /// Create a PDF travel card of the object
        /// </summary>
        /// <param name="formType">Type of form to create 0 = Portrait, 1 = Landscape</param>
        /// <returns>Successful Creation will return true, with file name.  Failed creation will return false with the error message</returns>
        public static IReadOnlyDictionary<bool, string> CreatePDF(FormType formType)
        {
            //TODO:Need to write the parts of this into the global config
            try
            {
                switch (formType)
                {
                    case FormType.Portrait:
                        FilePath = "\\\\fs-wcco\\WCCO-PublishedDocuments\\FORM5125 - Travel Card.pdf";
                        break;
                    case FormType.Landscape:
                        FilePath = "\\\\fs-wcco\\WCCO-PublishedDocuments\\FORM5127 - Reference Travel Card.pdf";
                        break;
                    case FormType.CoC:
                        FilePath = "\\\\fs-wcco\\WCCO-SFW\\CSI Travel Card.pdf";
                        break;
                }
                var _fileName = string.IsNullOrEmpty(LotNbr) ? $"{PartNbr}{DateTime.Now:MMyyHHmm}" : $"{LotNbr.Replace("-","")}{DateTime.Now:MMyyHHmm}";
                using (PdfReader reader = new PdfReader(FilePath, PdfEncodings.ConvertToBytes(Password, "ASCII")))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"\\\\fs-wcco\\WCCO-OMNI\\Application Data\\temp\\{_fileName}.pdf", FileMode.Create)))
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
                            if (QirNbr > 0)
                            {
                                pdfField.SetField("QIR", QirNbr.ToString());
                                pdfField.SetField("QIRBar", QirNbr.ToString());
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
                            pdfField.SetField("Description", Desc);
                            pdfField.SetField("Lot", LotNbr);
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
                return new Dictionary<bool, string> { { true, _fileName } };
            }
            catch (Exception ex)
            {
                return new Dictionary<bool, string> { { false, ex.Message } };
            }
        }

        /// <summary>
        /// Print a Travel Card for any Sku object
        /// </summary>
        public static void Display(FormType formType)
        {
            try
            {
                var _response = CreatePDF(formType);
                if (_response.ContainsKey(true))
                {
                    _response.TryGetValue(true, out string _fileName);
                    Process.Start($"\\\\fs-wcco\\WCCO-OMNI\\Application Data\\temp\\{_fileName}.pdf");
                    DeleteDocuments(_fileName);
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
        public static string PrintPDF(FormType formType)
        {
            try
            {
                var _response = CreatePDF(formType);
                if (_response.ContainsKey(true))
                {
                    _response.TryGetValue(true, out string _fileName);
                    var _documentName = $"\\\\fs-wcco\\WCCO-OMNI\\Application Data\\temp\\{_fileName}.pdf";
                    using (Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument(_documentName, "technology#1"))
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
            //TODO:Rewreite to handle multiple deletions
            foreach (var f in Directory.GetFiles("\\\\fs-wcco\\WCCO-OMNI\\Application Data\\temp\\"))
            {
                try
                {
                    if (f != $"\\\\fs-wcco\\WCCO-OMNI\\Application Data\\temp\\{fileName}.pdf")
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
    }
}
