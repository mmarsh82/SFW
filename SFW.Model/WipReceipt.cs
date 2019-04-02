using SFW.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            HasCrew = Machine.GetMachineGroup(sqlCon, workOrder.OrderNumber, workOrder.Seq) != "PRESS";
            if (HasCrew)
            {
                CrewList = new BindingList<CrewMember>
                {
                    new CrewMember { IdNumber = CrewMember.GetCrewIdNumber(sqlCon, subFName, subLName), Name = Submitter, LastClock = "" }
                };
                CrewList.AddNew();
                CrewList.ListChanged += CrewList_ListChanged;
            }
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
    }
}
