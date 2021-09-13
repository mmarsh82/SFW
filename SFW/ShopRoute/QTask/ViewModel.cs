using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;

namespace SFW.ShopRoute.QTask
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WorkOrder ShopOrder { get; set; }
        public int CurrentSite { get { return App.SiteNumber; } }

        public bool IsMultiLoading { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="wo">Work order object to load into the view</param>
        /// <param name="dSet">Schedule DataSet</param>
        public ViewModel(WorkOrder wo, DataSet dSet)
        {
            ShopOrder = wo;
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
            foreach (DataRow _dr in dSet.Tables["WN"].Select($"[ID] = '{ShopOrder.OrderNumber}'"))
            {
                ShopOrder.Notes += $"{_dr.Field<string>(1)}\n";
            }
            ShopOrder.Notes = ShopOrder.Notes?.Trim('\n');
            foreach (DataRow _dr in dSet.Tables["SN"].Select($"[ID] = '{ShopOrder.OrderNumber}'"))
            {
                ShopOrder.ShopNotes += $"{_dr.Field<string>(1)}\n";
            }
            ShopOrder.ShopNotes = ShopOrder.ShopNotes?.Trim('\n');
            IsMultiLoading = true;
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            ShopOrder.ToolList = dSet.Tables["TL"].Select($"[ID] = '{ShopOrder.SkuNumber}*{ShopOrder.Seq}'").Select(o => o[1].ToString()).ToList();
                            ShopOrder.Bom = Model.Component.GetComponentBomList(dSet.Tables["BOM"].Select($"[ID] LIKE '{ShopOrder.SkuNumber}*%'"));
                            ShopOrder.Picklist = Model.Component.GetComponentPickList(dSet, dSet.Tables["PL"].Select($"[ID] LIKE '{ShopOrder.OrderNumber}*%'"), ShopOrder.OrderNumber, ShopOrder.StartQty - ShopOrder.CurrentQty);
                            dSet.Dispose();
                            IsMultiLoading = false;
                            OnPropertyChanged(nameof(IsMultiLoading));
                            OnPropertyChanged(nameof(ShopOrder));
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
