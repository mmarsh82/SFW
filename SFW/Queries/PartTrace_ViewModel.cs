using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace SFW.Queries
{
    public class PartTrace_ViewModel : ViewModelBase
    {
        #region Properties

        private string _partNbr;
        public string PartNumber
        {
            get
            { return _partNbr; }
            set
            {
                _partNbr = value.ToUpper();
                OnPropertyChanged(nameof(PartNumber));
                SetupPrint = ErrorMsg = VerifyText = string.Empty;
                SkuResultDictionary = null;
                OnPropertyChanged(nameof(SkuResultDictionary));
                SkuWIList = null;
                OnPropertyChanged(nameof(SkuWIList));
                SkuPartStructure = null;
                OnPropertyChanged(nameof(SkuPartStructure));
            }
        }

        private string _setupPrint;
        public string SetupPrint
        {
            get
            { return _setupPrint; }
            set
            {
                _setupPrint = value;
                OnPropertyChanged(nameof(SetupPrint));
            }
        }

        private string _verifyTxt;
        public string VerifyText
        {
            get
            { return _verifyTxt; }
            set
            {
                _verifyTxt = string.IsNullOrEmpty(value) ? "Verify" : value;
                OnPropertyChanged(nameof(VerifyText));
            }
        }

        private string _errMsg;
        public string ErrorMsg
        {
            get
            { return _errMsg; }
            set
            {
                _errMsg = value;
                OnPropertyChanged(nameof(ErrorMsg));
            }
        }

        public IDictionary<Sku, bool> SkuResultDictionary { get; set; }
        public List<string> SkuWIList { get; set; }
        public IDictionary<Sku, int> SkuPartStructure { get; set; }

        RelayCommand _print;
        RelayCommand _setup;
        RelayCommand _verify;
        RelayCommand _doc;

        #endregion

        /// <summary>
        /// PartTrace ViewModel default constructor
        /// </summary>
        public PartTrace_ViewModel()
        {
            VerifyText = string.Empty;
            SkuResultDictionary = new Dictionary<Sku, bool>();
            SkuWIList = new List<string>();
        }

        #region Verify Part ICommand

        public ICommand VerifyICommand
        {
            get
            {
                if (_verify == null)
                {
                    _verify = new RelayCommand(VerifyExecute, VerifyCanExecute);
                }
                return _verify;
            }
        }

        public void VerifyExecute(object parameter)
        {
            if (!string.IsNullOrEmpty(PartNumber))
            {
                ErrorMsg = string.Empty;
                var _machName = Machine.GetMachineName(App.AppSqlCon, PartNumber);
                if (!string.IsNullOrEmpty(_machName))
                {
                    var _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, _machName, "\\\\manage2\\server\\Engineering\\Product\\Press Setups\\press setup and part number crossreference.xlsm", "Production");
                    if (_fileName.Contains("ERR:"))
                    {
                        ErrorMsg = "Main file is open, contact Engineering.";
                        VerifyText = "Accepted with errors";
                    }
                    else if (!string.IsNullOrEmpty(_fileName))
                    {
                        var _fileheader = string.Empty;
                        for (int i = 0; i < 8 - _fileName.Length; i++)
                        {
                            _fileheader += "0";
                        }
                        _fileName = _fileheader + _fileName;
                        SetupPrint = $"\\\\manage2\\Prints\\{_fileName}.PDF";
                        VerifyText = "Accepted";
                    }
                    else
                    {
                        _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, Machine.GetMachineName(App.AppSqlCon, PartNumber), "\\\\manage2\\server\\Engineering\\Product\\Sysco Press Setups\\SYSCO PRESS - Setup cross reference.xlsx", "PRODUCTION");
                        if (!string.IsNullOrEmpty(_fileName))
                        {
                            SetupPrint = $"\\\\manage2\\Prints\\{_fileName}.PDF";
                            VerifyText = "Accepted";
                        }
                        else
                        {
                            ErrorMsg = "No Setup exists.";
                            VerifyText = "Accepted with errors";
                        }
                    }
                    SkuWIList = Sku.GetInstructions(PartNumber, CurrentUser.GetSite(), App.AppSqlCon);
                    OnPropertyChanged(nameof(SkuWIList));
                    var _unOrderedDict = Sku.GetStructure(PartNumber, App.AppSqlCon);
                    SkuPartStructure = new Dictionary<Sku, int>();
                    foreach (var items in _unOrderedDict.OrderBy(o => o.Value))
                    {
                        SkuPartStructure.Add(items.Key, items.Value);
                    }
                    OnPropertyChanged(nameof(SkuPartStructure));
                }
                else
                {

                    VerifyText = "Not Found";
                }
            }
            else
            {
                ErrorMsg = "No value entered.";
            }
        }
        private bool VerifyCanExecute(object parameter) => true;

        #endregion

        #region Print Search ICommand

        public ICommand PrintSearchICommand
        {
            get
            {
                if (_print == null)
                {
                    _print = new RelayCommand(SearchExecute, SearchCanExecute);
                }
                return _print;
            }
        }

        public void SearchExecute(object parameter)
        {
            if (parameter == null)
            {
                if (Sku.Exists(PartNumber, App.AppSqlCon) && parameter == null)
                {
                    new Commands.PartSearch().Execute(Sku.GetMasterNumber(PartNumber, App.AppSqlCon));
                }
                else if (File.Exists($"\\\\manage2\\Prints\\{PartNumber}.pdf"))
                {
                    Process.Start($"\\\\manage2\\Prints\\{PartNumber}.pdf");
                }
                else
                {
                    SkuResultDictionary = Sku.Search(PartNumber.Replace(" ", "%"), App.AppSqlCon);
                    OnPropertyChanged(nameof(SkuResultDictionary));
                }
            }
            else
            {
                var _sku = (Sku)parameter;
                new Commands.PartSearch().Execute(Sku.GetMasterNumber(_sku.SkuNumber, App.AppSqlCon));
            }
        }

        private bool SearchCanExecute(object parameter) => !string.IsNullOrEmpty(PartNumber);

        #endregion

        #region View Setup Print ICommand

        public ICommand SetupICommand
        {
            get
            {
                if (_setup == null)
                {
                    _setup = new RelayCommand(SetupExecute, SetupCanExecute);
                }
                return _setup;
            }
        }

        public void SetupExecute(object parameter)
        {
            Process.Start(SetupPrint);
        }

        private bool SetupCanExecute(object parameter) => !string.IsNullOrEmpty(SetupPrint);

        #endregion

        #region View Document ICommand

        public ICommand ViewDocICommand
        {
            get
            {
                if (_doc == null)
                {
                    _doc = new RelayCommand(ViewDocExecute, ViewDocCanExecute);
                }
                return _doc;
            }
        }

        public void ViewDocExecute(object parameter)
        {
            if (parameter != null && !string.IsNullOrEmpty(parameter.ToString()))
            {
                try
                {
                    var _file = $"\\\\manage2\\server\\Document Center\\Production\\{parameter}";
                    Process.Start(_file);
                }
                catch (Exception)
                {

                }
            }
        }

        private bool ViewDocCanExecute(object parameter) => true;

        #endregion
    }
}
