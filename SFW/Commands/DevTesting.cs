using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Office.Interop.Word;

namespace SFW.Commands
{
    public class DevTesting : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command for testing
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            var _filePath = "C:\\Users\\uif28100\\OneDrive - Continental AG\\Desktop\\Need to make PDF";
            var _directory = new DirectoryInfo(_filePath);
            foreach (var _file in _directory.GetFiles())
            {
                var _fileName = _file.Name.Replace(_file.Extension, ".pdf");
                var wordApp = new Application();
                if (wordApp.Documents != null)
                {
                    var wordDoc = wordApp.Documents.Open(_file.FullName);
                    if(wordDoc != null)
                    {
                        wordDoc.ExportAsFixedFormat(_fileName, WdExportFormat.wdExportFormatPDF);
                        wordDoc.Close();
                    }
                }
                wordApp.Quit();
            }
        }

        public bool CanExecute(object parameter) { return true; }

    }
}
