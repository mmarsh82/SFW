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
                    var _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, _machName, $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PressSetup}", "Production");
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
                        SetupPrint = $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{_fileName}.PDF";
                        VerifyText = "Accepted";
                    }
                    else
                    {
                        _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, Machine.GetMachineName(App.AppSqlCon, PartNumber), $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).SyscoSetup}", "PRODUCTION");
                        if (!string.IsNullOrEmpty(_fileName))
                        {
                            SetupPrint = $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{ _fileName}.PDF";
                            VerifyText = "Accepted";
                        }
                        else
                        {
                            ErrorMsg = "No Setup exists.";
                            VerifyText = "Accepted with errors";
                        }
                    }
                }
                else
                {

                    VerifyText = "No Setup.";
                }
                SkuWIList = Sku.GetInstructions(PartNumber, CurrentUser.GetSite(), App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI, App.AppSqlCon);
                if (SkuWIList == null)
                {
                    VerifyText += " No Work Instructions.";
                }
                OnPropertyChanged(nameof(SkuWIList));
                var _unOrderedDict = Sku.GetStructure(PartNumber, App.AppSqlCon);
                SkuPartStructure = new Dictionary<Sku, int>();
                if (_unOrderedDict != null)
                {
                    foreach (var items in _unOrderedDict.OrderBy(o => o.Value))
                    {
                        SkuPartStructure.Add(items.Key, items.Value);
                    }
                }
                OnPropertyChanged(nameof(SkuPartStructure));
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
                var _master = string.Empty;
                //Check CSI first then move to WCCO
                if(App.SiteNumber == 0)
                {
                    if (Sku.Exists(PartNumber, App.AppSqlCon))
                    {
                        _master = Sku.GetMasterNumber(PartNumber, App.AppSqlCon);
                    }
                    else
                    {
                        App.DatabaseChange("WCCO_MAIN");
                        if (Sku.Exists(PartNumber, App.AppSqlCon))
                        {
                            _master = Sku.GetMasterNumber(PartNumber, App.AppSqlCon);
                        }
                        App.DatabaseChange("CSI_MAIN");
                    }
                }
                //Check WCCO first then move to CSI
                else
                {
                    if (Sku.Exists(PartNumber, App.AppSqlCon))
                    {
                        _master = Sku.GetMasterNumber(PartNumber, App.AppSqlCon);
                    }
                    else
                    {
                        App.DatabaseChange("CSI_MAIN");
                        if(Sku.Exists(PartNumber, App.AppSqlCon))
                        {
                            _master = Sku.GetMasterNumber(PartNumber, App.AppSqlCon);
                        }
                        App.DatabaseChange("WCCO_MAIN");
                    }
                }
                //Check to see how to open the print based on the results from the master print search
                if (!string.IsNullOrEmpty(_master))
                {
                    new Commands.PartSearch().Execute(_master);
                }
                else if (File.Exists($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{ PartNumber}.pdf"))
                {
                    Process.Start($"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).PartPrint}{ PartNumber}.pdf");
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
                    var _file = $"{App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI}{ parameter}";
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
