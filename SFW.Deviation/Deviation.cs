using iTextSharp.text.pdf;
using System;
using System.IO;

namespace SFW.Deviation
{
    public class Deviation
    {
        #region Properties

        public int IdNumber { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Deviation()
        { }

        public static string CreatePDF(string filePath)
        {
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    using (PdfStamper stamp = new PdfStamper(reader, new FileStream($"\\\\manage2\\server\\OMNI\\Application Data\\temp\\TEST.pdf", FileMode.Create)))
                    {
                        iTextSharp.text.pdf.parser.ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.LocationTextExtractionStrategy();
                        var docParse = string.Empty;
                        for (int page = 1; page < reader.NumberOfPages; page++)
                        {
                            var s = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, 1, its);
                            docParse += System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Convert(System.Text.Encoding.Default, System.Text.Encoding.UTF8, System.Text.Encoding.Default.GetBytes(s)));
                        }
                        
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}
