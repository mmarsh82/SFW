using M2kClient;
using Microsoft.Win32;
using SFW.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Tools
{
    public class ErpUpload_ViewModel : ViewModelBase
    {
        #region Properties

        private string _erpTbl;
        public string ErpTableName
        {
            get
            { return _erpTbl; }
            set
            {
                _erpTbl = value.ToUpper();
                OnPropertyChanged(nameof(ErpTableName));
            }
        }

        private int? _attNbr;
        public string AttributeNumber
        {
            get
            { return _attNbr.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _attNbr = i;
                }
                else
                {
                    _attNbr = null;
                }
                OnPropertyChanged(nameof(AttributeNumber));
            }
        }

        private string _filePath;
        public string UploadFilePath
        {
            get
            { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(UploadFilePath));
                OnPropertyChanged(nameof(ValidFile));
            }
        }
        public bool ValidFile { get { return !string.IsNullOrEmpty(UploadFilePath); } }

        private bool _submitted;
        public bool Submitted
        {
            get
            { return _submitted; }
            set
            {
                _submitted = value;
                OnPropertyChanged(nameof(Submitted));
            }
        }

        public string ErpLibrary { get { return App.ErpCon.Database.ToString(); } }

        public IReadOnlyDictionary<string, string> NewValueDictionary;

        RelayCommand _submit;
        RelayCommand _selectFile;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ErpUpload_ViewModel()
        {
            NewValueDictionary = new Dictionary<string, string>();
        }

        #region Submit ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submit == null)
                {
                    _submit = new RelayCommand(SubmitExecute, SubmitCanExecute);
                }
                return _submit;
            }
        }

        private void SubmitExecute(object parameter)
        {
            if (int.TryParse(AttributeNumber, out int _att))
            {
                Submitted = true;
                OnPropertyChanged(nameof(Submitted));
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    try
                    {
                        bw.DoWork += new DoWorkEventHandler(
                            delegate (object sender, DoWorkEventArgs e)
                            {
                                var _error = M2kCommand.EditRecords(ErpTableName, _att, NewValueDictionary, UdArrayCommand.Replace, App.ErpCon);
                                ErpTableName = string.Empty;
                                AttributeNumber = string.Empty;
                                UploadFilePath = string.Empty;
                                NewValueDictionary = new Dictionary<string, string>();
                                Submitted = false;
                                OnPropertyChanged(nameof(Submitted));
                            });
                        bw.RunWorkerAsync();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        private bool SubmitCanExecute(object parameter) => !string.IsNullOrEmpty(ErpTableName) && !string.IsNullOrEmpty(AttributeNumber) && ValidFile && NewValueDictionary?.Count() > 0;

        #endregion

        #region SelectFile ICommand

        public ICommand SelectFileICommand
        {
            get
            {
                if (_selectFile == null)
                {
                    _selectFile = new RelayCommand(SelectFileExecute);
                }
                return _selectFile;
            }
        }

        private void SelectFileExecute(object parameter)
        {

            var _fileDlg = new OpenFileDialog
            {
                Title = "Select excel file for Upload.",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".csv",
                Filter = "Excel Workbook|*.xlsx",
                Multiselect = false
            };
            var _file = _fileDlg.ShowDialog();
            if (_file == true)
            {
                NewValueDictionary = ExcelReader.Read(_fileDlg.FileName);
                if (NewValueDictionary == null && NewValueDictionary.Count() == 0)
                {
                    MessageBox.Show("No records were found.", "Empty Dictionary");
                }
                else if (NewValueDictionary.First().Key == "ERR")
                {
                    MessageBox.Show(NewValueDictionary.First().Value, "File Read Error.");
                }
                else
                {
                    UploadFilePath = _fileDlg.SafeFileName;
                }
            }
        }

        #endregion
    }
}
