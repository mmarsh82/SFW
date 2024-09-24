using SFW.Converters;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
                SkuResultDictionary = new Dictionary<Sku, bool>();
                OnPropertyChanged(nameof(SkuResultDictionary));
                SkuWIList = new List<string>();
                OnPropertyChanged(nameof(SkuWIList));
                SkuPartStructure = new Dictionary<Sku, int>();
                OnPropertyChanged(nameof(SkuPartStructure));
                SkuToolList = null;
                OnPropertyChanged(nameof(SkuToolList));
                OnPropertyChanged(nameof(EmptyToolList));
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

        private bool _isLoad;
        public bool IsLoading
        {
            get
            { return _isLoad; }
            set
            {
                _isLoad = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public IDictionary<Sku, bool> SkuResultDictionary { get; set; }
        public List<string> SkuWIList { get; set; }
        public IDictionary<Sku, int> SkuPartStructure { get; set; }
        public ICollectionView SkuToolList { get; set; }
        public bool EmptyToolList { get { return SkuToolList != null && !SkuToolList.IsEmpty; } }

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
            SkuPartStructure = new Dictionary<Sku, int>();
            SkuWIList = new List<string>();
            IsLoading = false;
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
                //Get a products set up print
                ErrorMsg = string.Empty;
                if (new Sku(PartNumber, 'C', App.SiteNumber).EngStatus == "O" && !CurrentUser.IsEngineer)
                {
                    ErrorMsg = "Obsolete parts and can only be viewed by Engineering.";
                }
                else
                {
                    var _machName = Machine.GetMachineName(PartNumber, 'P');
                    if (!string.IsNullOrEmpty(_machName))
                    {
                        switch (App.SiteNumber)
                        {
                            case 0:
                                try
                                {
                                    SetupPrint = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup}{PartNumber}.pdf";
                                    VerifyText = "Accepted";
                                    break;
                                }
                                catch (Exception)
                                {
                                    ErrorMsg = "No Setup exists.";
                                    VerifyText = "Accepted with errors";
                                    break;
                                }
                            case 1:
                                var _fileName = string.Empty;
                                var _machGrp = Machine.GetMachineGroup(_machName, 'M');
                                switch (_machGrp)
                                {
                                    case "PRESS":
                                    case "ENG":
                                        _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, _machName, App.GlobalConfig.First(o => o.Site == App.Facility).PressSetup, "Production");
                                        if (!string.IsNullOrEmpty(_fileName) && !_fileName.Contains("ERR:"))
                                        {
                                            var _fileheader = string.Empty;
                                            for (int i = 0; i < 8 - _fileName.Length; i++)
                                            {
                                                _fileheader += "0";
                                            }
                                            _fileName = _fileheader + _fileName;
                                            SetupPrint = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                            VerifyText = "Accepted";
                                        }
                                        else
                                        {
                                            ErrorMsg = "No Setup exists.";
                                            VerifyText = "Accepted with errors";
                                        }
                                        break;
                                    case "FABE":
                                        _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, _machName, App.GlobalConfig.First(o => o.Site == App.Facility).SyscoSetup, "PRODUCTION");
                                        SetupPrint = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                        VerifyText = "Accepted";
                                        break;
                                    case "EXT":
                                        _fileName = ExcelReader.GetSetupPrintNumber(PartNumber, _machName, App.GlobalConfig.First(o => o.Site == App.Facility).ExtSetup, "PRODUCTION");
                                        SetupPrint = $"{App.GlobalConfig.First(o => o.Site == App.Facility).PartPrint}{_fileName}.PDF";
                                        VerifyText = "Accepted";
                                        break;
                                    default:
                                        SetupPrint = string.Empty;
                                        VerifyText = "Accepted";
                                        break;
                                }
                                break;
                        }
                    }

                    //Get the sku work instruction list
                    SkuWIList = Sku.GetInstructions(PartNumber, App.SiteNumber, App.GlobalConfig.First(o => o.Site == App.Facility).WI);
                    OnPropertyChanged(nameof(SkuWIList));

                    //Get a sku tooling list
                    SkuToolList = CollectionViewSource.GetDefaultView(Tool.GetToolList(PartNumber));
                    SkuToolList.GroupDescriptions.Add(new PropertyGroupDescription("MachineID", new WorkCenterNameConverter()));
                    OnPropertyChanged(nameof(SkuToolList));
                    OnPropertyChanged(nameof(EmptyToolList));

                    //Background task to get the part structure list
                    IsLoading = true;
                    using (BackgroundWorker bw = new BackgroundWorker())
                    {
                        try
                        {
                            bw.DoWork += new DoWorkEventHandler(
                                delegate (object sender, DoWorkEventArgs e)
                                {
                                    SkuPartStructure = new Dictionary<Sku, int>();
                                    var _structList = Sku.GetStructure(PartNumber, $"{App.SiteNumber}");
                                    if (_structList == null)
                                    {
                                        ErrorMsg += "Trace list was to large to display.";
                                    }
                                    else
                                    {
                                        var _groupedDict = Sku.GetStructure(PartNumber, $"{App.SiteNumber}").OrderBy(o => o.Value).GroupBy(o => o.Key.DiamondNumber);
                                        if (_groupedDict.Count(o => o.FirstOrDefault().Key.SkuNumber != null) > 0)
                                        {
                                            foreach (var _group in _groupedDict)
                                            {
                                                foreach (var _item in _group)
                                                {
                                                    SkuPartStructure.Add(_item);
                                                }
                                            }
                                        }
                                        if (SkuPartStructure.Values.Count(o => o >= 2) > 0)
                                        {
                                            _groupedDict = null;
                                            _groupedDict = SkuPartStructure.GroupBy(o => o.Key.Location);
                                            SkuPartStructure = new Dictionary<Sku, int>();
                                            foreach (var _group in _groupedDict)
                                            {
                                                foreach (var _item in _group)
                                                {
                                                    SkuPartStructure.Add(_item);
                                                }
                                            }
                                        }
                                    }
                                    OnPropertyChanged(nameof(SkuPartStructure));
                                    IsLoading = false;
                                    OnPropertyChanged(nameof(IsLoading));
                                });
                            bw.RunWorkerAsync();
                        }
                        catch (Exception)
                        {

                        }
                    }
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
                var _master = string.Empty;
                var _part = PartNumber;
                var _site = App.Facility;
                var _siteNbr = App.SiteNumber;
                if (PartNumber.Contains("|"))
                {
                    _part = PartNumber.Split('|')[0];
                    if(int.TryParse(PartNumber.Split('|')[1], out int i))
                    {
                        if (i == 1)
                        {
                            _site = "WCCO";
                            _siteNbr = i;
                        }
                        else
                        {
                            _site = "CSI";
                            _siteNbr = i;
                        }
                    }
                }
                if (Sku.Exists(_part, CurrentUser.IsEngineer, _siteNbr))
                {
                    _master = Sku.GetMasterNumber(_part, CurrentUser.IsEngineer);
                }
                else if (Sku.Exists(_part, CurrentUser.IsEngineer, _siteNbr, true))
                {
                    _master = _part;
                }
                else
                {
                    MessageBox.Show("The part number you entered does not exist.", "Invalid Part Number");
                }
                //Check to see how to open the print based on the results from the master print search
                if (!string.IsNullOrEmpty(_master))
                {
                    new Commands.PartSearch().Execute($"{_master}|0{_siteNbr}");
                }
                else if (File.Exists($"{App.GlobalConfig.First(o => o.Site == _site.ToString()).PartPrint}{_part}.pdf"))
                {
                    Process.Start($"{App.GlobalConfig.First(o => o.Site == _site.ToString()).PartPrint}{_part}.pdf");
                }
                else
                {
                    SkuResultDictionary = Sku.Search(PartNumber.Replace(" ", "%"));
                    if (SkuResultDictionary == null || SkuResultDictionary.Count() == 0)
                    {
                        ErrorMsg = "No results found, check your entry.";
                    }
                    OnPropertyChanged(nameof(SkuResultDictionary));
                }
            }
            else
            {
                var _sku = (Sku)parameter;
                new Commands.PartSearch().Execute(!string.IsNullOrEmpty(_sku.MasterPrint) ? _sku.MasterPrint : $"{_sku.SkuNumber}|0{_sku.Facility}");
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
                    var _file = $"{App.GlobalConfig.First(o => o.Site == App.Facility).WI}{ parameter}";
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
