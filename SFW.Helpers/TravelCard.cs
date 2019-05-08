using iTextSharp.text.pdf;
using System;
using System.Diagnostics;
using System.IO;

//Created 1-22-2019 by Michael Marsh

namespace SFW.Helpers
{
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
        /// Print a Travel Card for any Sku object
        /// </summary>
        public static void Display()
        {
            try
            {
                FilePath = "\\\\manage2\\server\\Document Center\\Production\\FORM5125 - Travel Card.pdf";
                using (PdfReader reader = new PdfReader(FilePath, PdfEncodings.ConvertToBytes(Password, "ASCII")))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf", FileMode.Create)))
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
                        stamp.FormFlattening = false;
                    }
                }
                Process.Start($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf");
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
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Create a Reference Travel Card for a Inventory Skew Object
        /// </summary>
        public static void DisplayReference()
        {
            try
            {
                FilePath = "\\\\manage2\\server\\Document Center\\Production\\FORM5127 - Reference Travel Card.pdf";
                using (PdfReader reader = new PdfReader(FilePath))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf", FileMode.Create)))
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
                        stamp.FormFlattening = false;
                    }
                }
                Process.Start($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\{LotNbr}.pdf");
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
            catch (Exception)
            {

            }
        }
    }
}
