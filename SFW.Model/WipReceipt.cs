using SFW.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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
        /// Wip receipt crew list to use for the labor part of the transaction
        /// </summary>
        public IDictionary<int, string> CrewList { get; set; }

        /// <summary>
        /// Wip receipt start time to use for the labor part of the transaction
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// Wip receipt shift to use for the labor part of the transaction
        /// </summary>
        public int Shift { get; set; }

        #endregion

        /// <summary>
        /// Wip Receipt Constructor
        /// </summary>
        /// <param name="submitter">Currently logged in user</param>
        /// <param name="workOrder">Work order object to process</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public WipReceipt(string submitter, WorkOrder workOrder, SqlConnection sqlCon)
        {
            Submitter = submitter;
            SeqComplete = Complete.N;
            WipLot = new Lot();
            WipWorkOrder = workOrder;
            WipWorkOrder.CrewSize = Sku.GetCrewSize(WipWorkOrder.SkuNumber, WipWorkOrder.Seq, sqlCon);
            if (WipWorkOrder.CrewSize > 1)
            {
                CrewList = new Dictionary<int, string>();
            }
            //TODO: Remove shift hardcode and push it to the config file
            //TODO: remove start time hardcode and move it to a dynamic pull
            if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour < 15)
            {
                Shift = 1;
                StartTime = "7:00";
            }
            else if(DateTime.Now.Hour >= 15 && DateTime.Now.Hour < 23)
            {
                Shift = 2;
                StartTime = "15:00";
            }
            else
            {
                Shift = 3;
                StartTime = "23:00";
            }
        }
    }
}
