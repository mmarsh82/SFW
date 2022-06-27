using SFW.Model.Enumerations;
using System.Collections.Generic;
using System.ComponentModel;
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

            public string ID { get; set; }

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

        /// <summary>
        /// Reclaim Sub-Class for the Sku object
        /// </summary>
        public class Reclaim : INotifyPropertyChanged
        {
            #region Properties

            public int ID { get; set; }

            private int? qty;
            /// <summary>
            /// Quantity of reclaim for the wip receipt
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

            /// <summary>
            /// Parent part number for the reclaim transaction
            /// </summary>
            public string Parent { get; set; }

            /// <summary>
            /// Reference information for a reclaim transaction, typically the work order and QIR number
            /// </summary>
            public string Reference { get; set; }

            /// <summary>
            /// Assembly Quantity for a reclaim transaction
            /// </summary>
            public decimal ParentAssyQty { get; set; }

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
            public Reclaim()
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

        /// <summary>
        /// Wip receipt scrap list to use for the adjust part of the transaction
        /// </summary>
        public BindingList<Scrap> ScrapList { get; set; }

        /// <summary>
        /// Determines if the Wip is capable of processing reclaim
        /// </summary>
        public bool CanReclaim { get; set; }

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
                if (value == Complete.N && ReclaimList != null)
                {
                    ReclaimList.Clear();
                }
            }
        }

        /// <summary>
        /// Wip receipt reclaim list to use for the adjust part of the transaction
        /// </summary>
        public BindingList<Reclaim> ReclaimList { get; set; }

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

        public bool IsLoading { get; set; }
        public static string[] ErpCon { get; set; }

        #endregion

        /// <summary>
        /// Wip Receipt Constructor
        /// </summary>
        /// <param name="userId">Currently logged in user ID</param>
        /// <param name="subFName">Currently logged in user First Name</param>
        /// <param name="subLName">Currently logged in user Last Name</param>
        /// <param name="workOrder">Work order object to process</param>
        public WipReceipt(string userId, string subFName, string subLName, WorkOrder workOrder, string[] erpCon)
        {
            if (ErpCon == null)
            {
                ErpCon = erpCon;
            }
            Submitter = $"{subFName} {subLName}";
            var _crewID = userId;
            SeqComplete = Complete.N;
            WipLot = new Lot();
            WipWorkOrder = workOrder;
            WipWorkOrder.CrewSize = Sku.GetCrewSize(WipWorkOrder.SkuNumber);
            HasCrew = true;
            if (HasCrew)
            {
                CrewList = new BindingList<CrewMember>();
                CrewList.ListChanged += CrewList_ListChanged;
                CrewList.AddNew();
                CrewList[0].IdNumber = _crewID;
            }
            IsLotTracable = Sku.IsLotTracable(workOrder.SkuNumber);
            IsScrap = Complete.N;
            ScrapList = new BindingList<Scrap>();
            IsReclaim = Complete.N;
            ReclaimList = new BindingList<Reclaim>();
            CanMulti = workOrder.MachineGroup == "SLIT";
            CanReclaim = workOrder.MachineGroup == "EXT";
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "IdNumber" && !IsLoading)
            {
                if (CrewMember.IsCrewIDValid(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber))
                {
                    IsLoading = true;
                    var _tempCrew = new CrewMember(((BindingList<CrewMember>)sender)[e.NewIndex].IdNumber);
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Name = _tempCrew.Name;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].IsDirect = _tempCrew.IsDirect;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Shift = _tempCrew.Shift;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].LastClock = _tempCrew.LastClock;
                    if (((BindingList<CrewMember>)sender).Count() == ((BindingList<CrewMember>)sender).Count(o => !string.IsNullOrEmpty(o.Name)))
                    {
                        ((BindingList<CrewMember>)sender).AddNew();
                    }
                    IsLoading = false;
                }
                else if (!string.IsNullOrEmpty(((BindingList<CrewMember>)sender)[e.NewIndex].Name))
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Name = null;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].IsDirect = false;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].Shift = 0;
                    ((BindingList<CrewMember>)sender)[e.NewIndex].LastClock = null;
                }
            }
        }
    }
}
