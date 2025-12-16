using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SFW.Helpers
{
    public class ExcelReader
    {
        /// <summary>
        /// Reads the setup information from the Excel cross reference workbook
        /// </summary>
        /// <param name="partNbr">Part number</param>
        /// <param name="machineName">Machine name</param>
        /// <param name="filePath">File path to the excel document to parse</param>
        /// <param name="sheetName">Name of the worksheet to parse in the excel workbook</param>
        /// <returns>File name as string</returns>
        public static string GetSetupPrintNumber(string partNbr, string machineName, string filePath, string sheetName)
        {
            try
            {
                var ssPack = Package.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var ssDoc = SpreadsheetDocument.Open(ssPack))
                {
                    var wbPart = ssDoc.WorkbookPart;
                    var setupSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
                    var wsPart = (WorksheetPart)wbPart.GetPartById(setupSheet.Id);
                    var sheetRows = wsPart.Worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    var _row = string.Empty;
                    var _col = sheetRows?.First().Descendants<Cell>()
                        .Where(c => stringTable.SharedStringTable.ElementAt(int.Parse(c.InnerText)).InnerText == machineName).FirstOrDefault()?.CellReference;
                    if (_col != null)
                    {
                        _col = Regex.Replace(_col, "[^A-Z]+", string.Empty);
                    }
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
                        return _file;
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return "ERR:File Open";
            }
        }

        /// <summary>
        /// Reads the trimming setup information from the Excel cross reference workbook
        /// </summary>
        /// <param name="partNbr">Part Number</param>
        /// <param name="filePath">File path to the excel document to parse</param>
        /// <param name="sheetName">Name of the workshee to parse in the excel workbook</param>
        /// <returns>List of strings that contains the set up information</returns>
        public static List<string> GetTrimmingSetupInfo(string partNbr, string filePath, string sheetName)
        {
            var _returnList = new List<string>();
            try
            {
                var ssPack = Package.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var ssDoc = SpreadsheetDocument.Open(ssPack))
                {
                    var wbPart = ssDoc.WorkbookPart;
                    var setupSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
                    var wsPart = (WorksheetPart)wbPart.GetPartById(setupSheet.Id);
                    var sheetRows = wsPart.Worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
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
                                var _counter = 1;
                                foreach (var o in r.Descendants<Cell>())
                                {
                                    var _colAsInt = Convert.ToChar(o.CellReference.ToString().Substring(0, 1)) % 32;
                                    while (_counter != _colAsInt)
                                    {
                                        _returnList.Add(string.Empty);
                                        _counter++;
                                    }
                                    if (o.DataType != null && o.DataType.Value == CellValues.SharedString)
                                    {
                                        _returnList.Add(stringTable.SharedStringTable.ElementAt(int.Parse(o.InnerText)).InnerText);
                                    }
                                    else
                                    {
                                        _returnList.Add(o.InnerText);
                                    }
                                    _counter++;
                                }
                                while (_returnList.Count < 13)
                                {
                                    _returnList.Add(string.Empty);
                                }
                                return _returnList;
                            }
                            break;
                        }
                        if (_returnList.Count > 0)
                        {
                            break;
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a key and an value from an excel sheet and returns it as a dictionary
        /// </summary>
        /// <param name="filePath">File path for the excel sheet</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, string> Read(string filePath)
        {
            var _returnDict = new Dictionary<string, string>();
            try
            {
                var ssPack = Package.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var ssDoc = SpreadsheetDocument.Open(ssPack))
                {
                    var wbPart = ssDoc.WorkbookPart;
                    var setupSheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                    var wsPart = (WorksheetPart)wbPart.GetPartById(setupSheet.Id);
                    var sheetRows = wsPart.Worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    foreach (var r in sheetRows)
                    {
                        var _key = string.Empty;
                        var _value = string.Empty;
                        foreach (var c in r.Descendants<Cell>())
                        {
                            //Read the value and based on the type the being a decimal round it down to a precision of 3
                            var _cellValue = c.InnerText;
                            if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                            {
                                _cellValue = stringTable.SharedStringTable.ElementAt(int.Parse(c.InnerText)).InnerText;
                            }
                            //Check the type and change the precision
                            if (float.TryParse(_cellValue, out float cv))
                            {
                                _cellValue = Math.Round(cv, 3).ToString("N3");
                            }

                            //Add to the key or the value based on the column placement
                            if (string.IsNullOrEmpty(_key))
                            {
                                _key = _cellValue;
                            }
                            else
                            {
                                _value = _cellValue;
                            }
                        }
                        _returnDict.Add(_key, _value);
                    }
                }
                return _returnDict;
            }
            catch (ArgumentException ax)
            {
                _returnDict.Clear();
                _returnDict.Add("ERR", ax.Message);
                return _returnDict;
            }
            catch (Exception ex)
            {
                _returnDict.Clear();
                _returnDict.Add("ERR", ex.Message);
                return _returnDict;
            }
        }
    }

    public class ExcelWriter
    {
        /// <summary>
        /// Convert a datatable to an excel sheet
        /// </summary>
        /// <param name="dataTable">Datatable to export</param>
        /// <returns>Pass or Fail as bool</returns>
        public static bool ExportData(DataTable dataTable)
        {
            var _sfd = new SaveFileDialog();
            _sfd.Filter = "MS Excel|*.xlsx";
            _sfd.Title = "Save the NCR excel data";
            _sfd.FileName = $"NcrExport_{DateTime.Today.ToString("ddMMMyyyy")}";
            var _result = _sfd.ShowDialog();
            if (!string.IsNullOrEmpty(_sfd.FileName) && _result != DialogResult.Cancel)
            {
                try
                {
                    //Create the Excel document
                    using (var _stream = _sfd.OpenFile())
                    {
                        using (var _ssDoc = SpreadsheetDocument.Create(_stream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                        {
                            try
                            {
                                //Create a workbook with in the new Excel document
                                var _wbPart = _ssDoc.AddWorkbookPart();
                                _wbPart.Workbook = new Workbook();

                                //Add a worksheet to the workbook for the data dump
                                var _wsPart = _wbPart.AddNewPart<WorksheetPart>();
                                var _wsData = new SheetData();
                                _wsPart.Worksheet = new Worksheet(_wsData);
                                var _sheets = _ssDoc.WorkbookPart.Workbook.AppendChild(new Sheets());
                                var _sheet = new Sheet() { Id = _ssDoc.WorkbookPart.GetIdOfPart(_wsPart), SheetId = 1, Name = "NcrExport" };
                                _sheets.Append(_sheet);

                                //Build header row with the column names
                                var _originColumns = new Dictionary<string, CellValues>();
                                var _headerRow = new Row();
                                foreach (DataColumn _col in dataTable.Columns)
                                {
                                    _originColumns.Add(_col.ColumnName, Extensions.TypeToCellType(_col.DataType));
                                    var _headerCell = new Cell() { DataType = CellValues.InlineString, InlineString = new InlineString() { Text = new Text(_col.ColumnName) } };
                                    _headerRow.AppendChild(_headerCell);
                                }
                                _wsData.AppendChild(_headerRow);

                                //Run through all the rows and populate the data in the worksheet
                                foreach (DataRow _row in dataTable.Rows)
                                {
                                    var _newRow = new Row();
                                    foreach (var _hdrCol in _originColumns)
                                    {
                                        var _newCell = new Cell() { DataType = _hdrCol.Value };
                                        if (_hdrCol.Value == CellValues.InlineString)
                                        {
                                            _newCell.InlineString = new InlineString() { Text = new Text(_row[_hdrCol.Key].ToString()) };
                                        }
                                        else
                                        {
                                            _newCell.CellValue = new CellValue(_row[_hdrCol.Key].ToString());
                                        }
                                        _newRow.AppendChild(_newCell);
                                    }
                                    _wsData.AppendChild(_newRow);
                                }
                                _wbPart.Workbook.Save();
                            }
                            catch
                            {
                                _ssDoc.Close();
                                _ssDoc.Dispose();
                                _stream.Close();
                                _stream.Dispose();
                                Thread.Sleep(2000);
                                File.Delete(_sfd.FileName);
                                return false;
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }

    public static class Extensions
    {
        public static CellValues TypeToCellType(this Type dataType)
        {
            return dataType == typeof(int) || dataType == typeof(decimal) || dataType == typeof(bool) || dataType == typeof(short)
                ? CellValues.Number
                : CellValues.InlineString;
        }
    }

}
