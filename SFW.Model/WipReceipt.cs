using SFW.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//Created by Michael Marsh 10-23-18

namespace SFW.Model
{
    /// <summary>
    /// Wip Receipt Record Object
    /// </summary>
    public class WipReceipt
    {
        /// <summary>
        /// Scrap Sub-Class for the Sku object
        /// </summary>
        public class Scrap : INotifyPropertyChanged
        {
            #region Properties

            public int ID { get; set; }

            private int? qty;
            /// <summary>
            /// Quantity of scrap for the wip receipt
            /// </summary>
            public string Quantity
            {
                get
                { return qty.ToString(); }
                set
                {
                    if (int.TryParse(value, out int _qty))
                    {
                        qty = _qty;
                    }
                    else
                    {
                        qty = null;
                    }
                    OnPropertyChanged(nameof(Quantity));
                }
            }

            private string reason;
            /// <summary>
            /// Reason for scrapping the material
            /// </summary>
            public string Reason
            {
                get
                { return reason; }
                set
                {
                    reason = value;
                    OnPropertyChanged(nameof(Reason));
                    Reference = null;
                    OnPropertyChanged(nameof(Reference));
                }
            }

            /// <summary>
            /// Reference information for scrapping the material
            /// </summary>
            public string Reference { get; set; }

            #endregion

            #region INotifyPropertyChanged Implementation

            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Reflects changes from the ViewModel properties to the View
            /// </summary>
            /// <param name="propertyName">Property Name</param>
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    var e = new PropertyChangedEventArgs(propertyName);
                    handler(this, e);
                }
            }

            #endregion

            /// <summary>
            /// Scrap Default constructor
            /// </summary>
            public Scrap()
            { }
        }

        #region Properties

        /// <summary>
        /// Currently logged in user domain username
        /// </summary>
        public string Submitter { get; private set; }

        /// <summary>
        /// Wip receipt record quantity
        /// </summary>
        public int? WipQty { get; set; }

        /// <summary>
        /// Sequence completion flag
        /// </summary>
        public Complete SeqComplete { get; set; }

        /// <summary>
        /// Receipt Location for the product
        /// </summary>
        public string ReceiptLocation { get; set; }

        /// <summary>
        /// Wip receipt lot object
        /// </summary>
        public Lot WipLot { get; set; }

        /// <summary>
        /// List of possible lots that can be used for transaction
        /// </summary>
        public IList<Lot> LotList { get; set; }

        /// <summary>
        /// Wip receipt work order object
        /// </summary>
        public WorkOrder WipWorkOrder { get; set; }

        /// <summary>
        /// Wip receipt crew validation, some work orders will not require a crew to be submitted
        /// </summary>
        public bool HasCrew { get; set; }

        /// <summary>
        /// Wip receipt crew list to use for the labor part of the transaction
        /// </summary>
        public BindingList<CrewMember> CrewList { get; set; }

        /// <summary>
        /// Wip receipt start time to use for the labor part of the transaction
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Wip receipt lot tracability, used to tell the ERP to assign or not a lot number during wip transaction
        /// </summary>
        public bool IsLotTracable { get; set; }

        private Complete isScrap;
        /// <summary>
        /// Determines if there is scrap for the wip receipt
        /// </summary>
        public Complete IsScrap
        {
            get { return isScrap; }
            set
            {
                isScrap = value;
                if (value == Complete.N && ScrapList != null)
                {
                    ScrapList.Clear();
                }
            }
        }

        public BindingList<Scrap> ScrapList { get; set; }

        private Complete isReclaim;
        /// <summary>
        /// Determines if there is relaim for the wip receipt
        /// </summary>
        public Complete IsReclaim
        {
            get { return isReclaim; }
            set
            {
                isReclaim = value;
                if (value == Complete.N)
                {
                    ReclaimQty = null;
                }
            }
        }

        /// <summary>
        /// Quantity of reclaim for the wip receipt
        /// </summary>
        public int? ReclaimQty { get; set; }

        /// <summary>
        /// Parent part number for the reclaim transaction
        /// </summary>
        public string ReclaimParent { get; set; }

        /// <summary>
        /// Determines if a work order is eligable for the Multi-Wip function
        /// </summary>
        public bool CanMulti { get; set; }

        /// <summary>
        /// Determines if the work order Multi-Wip function is activated
        /// </summary>
        public bool IsMulti { get; set; }

        /// <summary>
        /// Mulit-Wip function roll quantity
        /// </summary>
        public int? RollQty { get; set; }

        #endregion

        /// <summary>
        /// Wip Receipt Constructor
        /// </summary>
        /// <param name="subFName">Currently logged in user First Name</param>
        /// <param name="subLName">Currently logged in user Last Name</param>
        /// <param name="workOrder">Work order object to process</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public WipReceipt(string subFName, string subLName, WorkOrder workOrder, SqlConnection sqlCon)
        {
            Submitter = $"{subFName} {subLName}";
            ModelBase.ModelSqlCon = sqlCon;
            SeqComplete = Complete.N;
            WipLot = new Lot();
            WipWorkOrder = workOrder;
            WipWorkOrder.CrewSize = Sku.GetCrewSize(WipWorkOrder.SkuNumber, WipWorkOrder.Seq, sqlCon);
            //HasCrew = Machine.GetMachineGroup(sqlCon, workOrder.OrderNumber, workOrder.Seq) != "PRESS";
            HasCrew = true;
            if (HasCrew)
            {
                CrewList = new BindingList<CrewMember>
                {
                    new CrewMember { IdNumber = CrewMember.GetCrewIdNumber(sqlCon, subFName, subLName), Name = Submitter, LastClock = "" }
                };
                CrewList.AddNew();
                CrewList.ListChanged += CrewList_ListChanged;
            }
            IsLotTracable = Sku.IsLotTracable(workOrder.SkuNumber, sqlCon);
            IsScrap = Complete.N;
            ScrapList = new BindingList<Scrap>();
            IsReclaim = Complete.N;
            ReclaimParent = null; //workOrder.MachineGroup == "EXT" ? workOrder.Bom.Where(o => o.InventoryType == "RC").FirstOrDefault().CompNumber : null;
            CanMulti = false; //workOrder.MachineGroup == "SLIT";
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor.DisplayName == "IdNumber" )
            {
                ((BindingList<CrewMember>)sender)[e.NewIndex].Name = string.Empty;
                var _dName = CrewMember.GetCrewDisplayName(ModelBase.ModelSqlCon, Convert.ToInt32(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber));
                var _duplicate = ((BindingList<CrewMember>)sender).Any(o => o.Name == _dName);
                if (!string.IsNullOrEmpty(_dName) && !_duplicate)
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Name = _dName;
                    if (((BindingList<CrewMember>)sender).Count == e.NewIndex + 1)
                    {
                        ((BindingList<CrewMember>)sender).AddNew();
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the location entered into any wip field is valid
        /// </summary>
        /// <param name="location">Location to check</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Valid location as bool</returns>
        public static bool ValidLocation(string location, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"USE {sqlCon.Database}; SELECT COUNT([ID]) FROM [dbo].[LOC_MASTER-INIT] WHERE [ID] = CONCAT('01*', @p1);", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", location);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
