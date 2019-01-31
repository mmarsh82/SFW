using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;

namespace SFW.Helpers
{
    public class ExcelReader
    {
        /// <summary>
        /// Reads the setup information from the Excel cross reference workbook
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="machineName">Machine name</param>
        public static string GetSetupPrintNumber(string partNbr, string machineName)
        {
            try
            {
                var ssPack = Package.Open("\\\\manage2\\server\\Technology\\Program Testing\\SFW_press setup and part number crossreference.xlsm", FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var ssDoc = SpreadsheetDocument.Open(ssPack))
                {
                    var wbPart = ssDoc.WorkbookPart;
                    var setupSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "Production").FirstOrDefault();
                    var wsPart = (WorksheetPart)wbPart.GetPartById(setupSheet.Id);
                    var sheetRows = wsPart.Worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    var _row = string.Empty;
                    var _col = sheetRows.First().Descendants<Cell>()
                        .Where(c => stringTable.SharedStringTable.ElementAt(int.Parse(c.InnerText)).InnerText == machineName).FirstOrDefault().CellReference;
                    _col = Regex.Replace(_col, "[^A-Z]+", string.Empty);
                    foreach (var r in sheetRows)
                    {
                        foreach (var c in r.Descendants<Cell>())
                        {
                            var _cellValue = c.InnerText;
                            if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                            {
                                _cellValue = stringTable.SharedStringTable.ElementAt(int.Parse(c.InnerText)).InnerText;
                            }
                            if (_cellValue == partNbr)
                            {
                                _row = c.CellReference;
                                _row = Regex.Replace(_row, "[^0-9]+", string.Empty);
                            }
                            break;
                        }
                        if (!string.IsNullOrEmpty(_row))
                        {
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(_col) && !string.IsNullOrEmpty(_row))
                    {
                        var _cellRef = _col.ToString() + _row.ToString();
                        var _cellVal = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == _cellRef).FirstOrDefault();
                        var _file = _cellVal.DataType != null && _cellVal.DataType.Value == CellValues.SharedString
                            ? stringTable.SharedStringTable.ElementAt(int.Parse(_cellVal.InnerText)).InnerText
                            : _cellVal.InnerText;
                        var _fileheader = string.Empty;
                        for(int i = 0; i < 8 - _file.Length; i++)
                        {
                            _fileheader += "0";
                        }
                        return _fileheader + _file;
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
