using SFW.Model.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SFW.Model.Production.Wip
{
    public class Receipt : ModelBase
    {
        #region Properties

        /// <summary>
        /// Currently logged in user domain username
        /// </summary>
        public string Submitter { get; private set; }

        /// <summary>
        /// Logged in user facility code
        /// </summary>
        public string Facility { get; set; }

        /// <summary>
        /// Wip receipt record quantity
        /// </summary>
        public int? WipQty { get; set; }

        /// <summary>
        /// Sequence completion flag
        /// </summary>
        public Enumerations.Complete SeqComplete { get; set; }

        /// <summary>
        /// Receipt Location for the product
        /// </summary>
        public string ReceiptLocation { get; set; }

        /// <summary>
        /// Wip receipt lot object
        /// </summary>
        public Product.Lot WipLot { get; set; }

        /// <summary>
        /// List of possible lots that can be used for transaction
        /// </summary>
        public IList<Product.Lot> LotList { get; set; }

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
        public BindingList<Employee> CrewList { get; set; }

        /// <summary>
        /// Wip receipt start time to use for the labor part of the transaction
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Wip receipt lot tracability, used to tell the ERP to assign or not a lot number during wip transaction
        /// </summary>
        public bool IsLotTracable { get; set; }

        private Enumerations.Complete isScrap;
        /// <summary>
        /// Determines if there is scrap for the wip receipt
        /// </summary>
        public Enumerations.Complete IsScrap
        {
            get { return isScrap; }
            set
            {
                isScrap = value;
                if (value == Enumerations.Complete.N && ScrapList != null)
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

        private Enumerations.Complete isReclaim;
        /// <summary>
        /// Determines if there is relaim for the wip receipt
        /// </summary>
        public Enumerations.Complete IsReclaim
        {
            get { return isReclaim; }
            set
            {
                isReclaim = value;
                if (value == Enumerations.Complete.N && ReclaimObject != null)
                {
                    ReclaimObject = null;
                }
            }
        }

        /// <summary>
        /// Wip receipt reclaim to use for the adjust part of the transaction
        /// </summary>
        public Reclaim ReclaimObject { get; set; }

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

        /// <summary>
        /// Product weight
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// Default pull location for the work center
        /// </summary>
        public string PullLocation { get; set; }
        
        /// <summary>
        /// Wip receipt compnent list
        /// </summary>
        public IList<Component> ComponentList { get; set; }

        public static string[] ErpCon { get; set; }

        #endregion

        /// <summary>
        /// Wip Receipt Constructor
        /// </summary>
        /// <param name="submitter">The crew member that is submitting the wip</param>
        /// <param name="facCode">Currently logged in user facility code</param>
        /// <param name="workOrder">Work order object to process</param>
        /// <param name="erpCon">ERP connection</param>
        public Receipt(Employee submitter, int facCode, WorkOrder workOrder, string[] erpCon)
        {
            if (ErpCon == null)
            {
                ErpCon = erpCon;
            }
            Submitter = submitter.Name;
            Facility = $"0{facCode}";
            SeqComplete = Enumerations.Complete.N;
            WipLot = new Product.Lot();
            WipWorkOrder = workOrder;
            WipWorkOrder.Product.CrewSize = Product.Sku.GetCrewSize(WipWorkOrder.Product.SkuNumber);
            HasCrew = true;
            CrewList = new BindingList<Employee>
            {
                submitter
                ,new Employee() { ListId = 1 }
            };
            CrewList.ListChanged += CrewList_Changed;
            IsLotTracable = Product.Sku.IsLotTracable(WipWorkOrder.Product.SkuNumber, facCode);
            IsScrap = Enumerations.Complete.N;
            ScrapList = new BindingList<Scrap>();
            IsReclaim = Enumerations.Complete.N;
            ReclaimObject = new Reclaim();
            CanMulti = WipWorkOrder.WorkCenter.MachineGroup == "SLIT";
            CanReclaim = WipWorkOrder.WorkCenter.MachineGroup == "EXT";
            ReceiptLocation = Machine.GetDefaultLocation(WipWorkOrder.WorkCenter.MachineName);
            PullLocation = Machine.GetPullLocation(WipWorkOrder.WorkCenter.MachineName);
            ComponentList = new List<Component>();
            var _tempComp = PickComponent.GetList(WipWorkOrder.OrderNumber, WipWorkOrder.Seq, 1, WipWorkOrder.WorkCenter.MachineName);
            foreach (var _comp in _tempComp)
            {
                ComponentList.Add(new Component(!string.IsNullOrEmpty(_comp.BackFlushLocation), _comp.ProductNumber, _comp.ProductUom, _comp.AssemblyQuantity, WipWorkOrder.OrderNumber, WipWorkOrder.Seq));
            }
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<Component> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_Changed(object sender, ListChangedEventArgs e)
        {
            var _add = false;
            ((BindingList<Employee>)sender).RaiseListChangedEvents = false;
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "ErpId")
            {
                if (Employee.ValidErpId(((BindingList<Employee>)sender)[e.NewIndex].ErpId) && ((BindingList<Employee>)sender).Count(o => o.ErpId == ((BindingList<Employee>)sender)[e.NewIndex].ErpId) == 1)
                {
                    var _tempCrew = new Employee(((BindingList<Employee>)sender)[e.NewIndex].ErpId, true, false);
                    ((BindingList<Employee>)sender)[e.NewIndex].Facility = _tempCrew.Facility;
                    ((BindingList<Employee>)sender)[e.NewIndex].IsDirect = _tempCrew.IsDirect;
                    ((BindingList<Employee>)sender)[e.NewIndex].Name = _tempCrew.Name;
                    ((BindingList<Employee>)sender)[e.NewIndex].Shift = _tempCrew.Shift;
                    ((BindingList<Employee>)sender)[e.NewIndex].ShiftEnd = _tempCrew.ShiftEnd;
                    ((BindingList<Employee>)sender)[e.NewIndex].ShiftStart = _tempCrew.ShiftStart;
                    ((BindingList<Employee>)sender)[e.NewIndex].LaborData = _tempCrew.LaborData;
                    ((BindingList<Employee>)sender)[e.NewIndex].SapId = _tempCrew.SapId;
                    if (((BindingList<Employee>)sender).Count() == ((BindingList<Employee>)sender).Count(o => !string.IsNullOrEmpty(o.Name)))
                    {
                        _add = true;
                    }
                }
                else if (!string.IsNullOrEmpty(((BindingList<Employee>)sender)[e.NewIndex].Name))
                {
                    //TODO: add in logic to remove the second entry when deleting the first and list only has 2 entrys
                    ((BindingList<Employee>)sender)[e.NewIndex].Name = null;
                    ((BindingList<Employee>)sender)[e.NewIndex].IsDirect = false;
                    ((BindingList<Employee>)sender)[e.NewIndex].Shift = 0;
                    ((BindingList<Employee>)sender)[e.NewIndex].Facility = null;
                    ((BindingList<Employee>)sender)[e.NewIndex].LaborData = null;
                }
            }
            ((BindingList<Employee>)sender).RaiseListChangedEvents = true;
            if (_add)
            {
                ((BindingList<Employee>)sender).Add(new Employee(){ ListId = ((BindingList<Employee>)sender).Count });
            }
        }
    }
}
