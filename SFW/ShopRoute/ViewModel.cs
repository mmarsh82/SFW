using M2kClient;
using SFW.Helpers;
using SFW.Model;
using SFW.Reports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.ShopRoute
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private WorkOrder shopOrder;
        public WorkOrder ShopOrder
        {
            get { return shopOrder; }
            set
            {
                shopOrder = value;
                shopOrder.ToolList = shopOrder.ToolList ?? new List<string>();
                OnPropertyChanged(nameof(ShopOrder));
                OnPropertyChanged(nameof(FqSalesOrder));
                ShopOrderNotes = null;
                MachineGroup = string.Empty;
                OnPropertyChanged(nameof(CanCheckHistory));
            }
        }

        public string FqSalesOrder
        {
            get { return $"{ShopOrder?.SalesOrder?.SalesNumber}*{ShopOrder?.SalesOrder?.LineNumber}"; }
        }

        public int CurrentSite { get { return App.SiteNumber; } }

        private string _shopNotes;
        public string ShopOrderNotes
        {
            get
            { return _shopNotes; }
            set
            {
                _shopNotes = string.IsNullOrEmpty(value) ? ShopOrder?.Notes : value;
                OnPropertyChanged(nameof(ShopOrderNotes));
            }
        }

        private string machGroup;
        public string MachineGroup
        {
            get
            { return machGroup; }
            set
            { machGroup = string.IsNullOrEmpty(value) ? Machine.GetMachineGroup(App.AppSqlCon, ShopOrder?.OrderNumber, ShopOrder?.Seq) : value; OnPropertyChanged(nameof(MachineGroup)); }
        }

        private bool loading;
        public bool IsLoading
        {
            get { return loading; }
            set { loading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public bool CanCheckHistory { get { return ShopOrder?.StartQty != ShopOrder?.CurrentQty; } }
        public bool CanReport { get { return MachineGroup == "PRESS"; } }
        public bool CanSeeTrim { get { return MachineGroup == "PRESS"; } }
        public bool CanWip { get; set; }

        public bool IsMultiLoading { get; set; }

        private RelayCommand _noteChange;
        private RelayCommand _loadReport;

        #endregion

        /// <summary>
        /// Shop Route Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
        }

        /// <summary>
        /// Shop Route Constructor for loading work orders
        /// </summary>
        /// <param name="workOrder">Work Order Object</param>
        /// <param name="dSet">Schedule DataSet</param>
        public ViewModel(WorkOrder workOrder, DataSet dSet)
        {
            CanWip = false;
            ShopOrder = workOrder;
            //Getting the Work order work instructions
            if (App.SiteNumber == 0)
            {
                ShopOrder.InstructionList = Sku.GetInstructions(ShopOrder.SkuNumber, App.SiteNumber, App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI, App.AppSqlCon);
            }
            else
            {
                ShopOrder.InstructionList = new List<string>();
                var _tempList = dSet.Tables["WI"].Select($"[ID] = '{ShopOrder.SkuNumber}'").Select(o => o[1].ToString()).ToList();
                foreach (var wiNbr in _tempList)
                {
                    var dir = new DirectoryInfo(App.GlobalConfig.First(o => $"{o.Site}_MAIN" == App.Site).WI);
                    var fileList = dir.GetFiles($"*{wiNbr}*");
                    foreach (var file in fileList)
                    {
                        ShopOrder.InstructionList.Add(file.Name);
                    }
                }
            }

            //Getting the work order notes and the shop floor notes
            foreach (DataRow _dr in dSet.Tables["Notes"].Select($"[NoteID] = '{ShopOrder.OrderNumber}' AND [NoteType] = 'WN'", "[LineID] ASC"))
            {
                ShopOrder.Notes += $"{_dr.Field<string>("Note")}\n";
            }
            ShopOrderNotes = ShopOrder.Notes = ShopOrder.Notes?.Trim('\n');
            foreach (DataRow _dr in dSet.Tables["Notes"].Select($"([NoteID] = '{ShopOrder.SkuNumber}' OR [NoteID] = '{ShopOrder.OrderNumber}') AND [NoteType] = 'SN'", "[Priority], [LineID] ASC"))
            {
                ShopOrder.ShopNotes += $"{_dr.Field<string>("Note")}\n";
            }
            ShopOrder.ShopNotes = ShopOrder.ShopNotes?.Trim('\n');

            //Getting the sales order internal comments
            foreach (DataRow _dr in dSet.Tables["SOIC"].Select($"[ID] = '{ShopOrder.SalesOrder}'"))
            {
                ShopOrder.SalesOrder.InternalComments += $"{_dr.Field<string>(1)}\n";
            }
            ShopOrder.SalesOrder.InternalComments = ShopOrder.SalesOrder.InternalComments?.Trim('\n');

            //Bill of Material and picklist loading, needs to be done in the background due to the recursive search
            IsMultiLoading = true;
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            ShopOrder.ToolList = dSet.Tables["TL"].Select($"[ID] = '{ShopOrder.SkuNumber}*{ShopOrder.Seq}'").Select(o => o[1].ToString()).ToList();
                            ShopOrder.Bom = Model.Component.GetComponentBomList(dSet.Tables["BOM"].Select($"[ID] LIKE '{ShopOrder.SkuNumber}-%' AND ([Routing_Seq] = '{ShopOrder.Seq}' OR [Routing_Seq] IS NULL)"));
                            ShopOrder.Picklist = Model.Component.GetComponentPickList(dSet, dSet.Tables["PL"].Select($"[ID] LIKE '{ShopOrder.OrderNumber}*%' AND [Routing] = '{ShopOrder.Seq}'"), ShopOrder.OrderNumber, ShopOrder.StartQty - ShopOrder.CurrentQty);
                            dSet.Dispose();
                            IsMultiLoading = false;
                            OnPropertyChanged(nameof(IsMultiLoading));
                            OnPropertyChanged(nameof(ShopOrder));
                            CanWip = CurrentUser.CanWip;
                            OnPropertyChanged(nameof(CanWip));
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception)
                {
                    
                }
            }
        }

        #region Work Order Note Change ICommand

        public ICommand WONoteChgICommand
        {
            get
            {
                if (_noteChange == null)
                {
                    _noteChange = new RelayCommand(NoteChgExecute, NoteChgCanExecute);
                }
                return _noteChange;
            }
        }

        private void NoteChgExecute(object parameter)
        {
            var _noteArray = ShopOrderNotes.Replace("\r", "").Replace("\n", "|").Split('|');
            var _changeRequest = M2kCommand.EditMVRecord("WP", ShopOrder.OrderNumber, 39, _noteArray, App.ErpCon);
            if (!string.IsNullOrEmpty(_changeRequest))
            {
                MessageBox.Show(_changeRequest, "ERP Record Error");
                ShopOrderNotes = ShopOrder.Notes;
            }
        }
        private bool NoteChgCanExecute(object parameter) => true;

        #endregion

        #region Press Report ICommand

        public ICommand ReportICommand
        {
            get
            {
                if (_loadReport == null)
                {
                    _loadReport = new RelayCommand(ReportExecute, ReportCanExecute);
                }
                return _loadReport;
            }
        }

        private void ReportExecute(object parameter)
        {
            if (int.TryParse(parameter.ToString(), out int i))
            {
                var _repType = (Enumerations.PressReportActions)i;
                var _rep = new PressReport(shopOrder, App.AppSqlCon);
                if (_repType == Enumerations.PressReportActions.LogProgress && (_rep.IsNew || _rep.ShiftReportList.Count == 0))
                {
                    MessageBox.Show("There is currently no report created for this work order.\nPlease click on the report sheet button and submit a new report.", "No Report Sheet", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    if (ShopOrder != null)
                    {
                        if (!App.IsWindowOpen<Press_View>(new Press_ViewModel()))
                        {
                            new Press_View { DataContext = new Press_ViewModel(ShopOrder, _repType) }.Show();
                        }
                        else
                        {
                            var _win = App.GetWindow<Press_View>();
                            if (_win != null)
                            {
                                _win.DataContext = new Press_ViewModel(ShopOrder, _repType);
                                _win.Focus();
                            }
                        }
                    }
                }
            }
        }
        private bool ReportCanExecute(object parameter) => true;

        #endregion
    }
}
