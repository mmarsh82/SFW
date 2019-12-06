using iTextSharp.text.pdf;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

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
                DeleteDocuments();
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
                CreatePDF(formType);
                var _documentName = $"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf";
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
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Closes any open temporary documents created for printing
        /// </summary>
        private static void DeleteDocuments()
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
