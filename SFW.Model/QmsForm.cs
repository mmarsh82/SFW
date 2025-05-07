using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SFW.Model
{
    public class QmsForm : ModelBase
    {
        public enum FormType
        {
            NCR = 0,
            SCAR = 1
        }

        public class ClosingAction
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load a table with all the NCR closing action information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR closing actions</returns>
            public static DataTable GetClosingActionTable(SqlConnection sqlCon)
            {
                using (var _dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Disposition] WHERE [ID] <> 7", sqlCon))
                            {
                                adapter.Fill(_dt);
                            }
                            return _dt;
                        }
                        catch (SqlException)
                        {
                            return _dt;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public ClosingAction(int dType, string dDescrip, string status)
            {
                Id = dType;
                Description = dDescrip;
                Status = status;
            }

            /// <summary>
            /// Load an observable collection with all the NCR disposition information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR dispositions</returns>
            public static ObservableCollection<ClosingAction> GetClosingActionCollection()
            {
                var _rtnColl = new ObservableCollection<ClosingAction>();
                foreach (DataRow _row in MasterDataSet.Tables["NcrDispo"].Rows)
                {
                    _rtnColl.Add(new ClosingAction(_row.SafeGetField<int>("ActionId"), _row.SafeGetField<string>("ActionDescription"), _row.SafeGetField<string>("LinkedStatus")));
                }
                return _rtnColl;
            }
        }

        public class Disposition
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load a table with all the NCR disposition information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR dispositions</returns>
            public static DataTable GetDispositionTable(SqlConnection sqlCon)
            {
                using (var _dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Disposition] WHERE [ID] <> 7", sqlCon))
                            {
                                adapter.Fill(_dt);
                            }
                            return _dt;
                        }
                        catch (SqlException)
                        {
                            return _dt;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public Disposition(int dType, string dDescrip, string status)
            {
                Id = dType;
                Description = dDescrip;
                Status = status;
            }

            /// <summary>
            /// Load an observable collection with all the NCR disposition information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR dispositions</returns>
            public static ObservableCollection<Disposition> GetDispositionCollection()
            {
                var _rtnColl = new ObservableCollection<Disposition>();
                foreach (DataRow _row in MasterDataSet.Tables["DefectDisposition"].Rows)
                {
                    _rtnColl.Add(new Disposition(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), _row.SafeGetField<string>("FormStatus")));
                }
                return _rtnColl;
            }
        }

        public class DefectSubType
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }
            public string ToolTip { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load a table with all the NCR defect type information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static DataTable GetTable(SqlConnection sqlCon)
            {
                using (var _dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_SubType]", sqlCon))
                            {
                                adapter.Fill(_dt);
                            }
                            return _dt;
                        }
                        catch (SqlException)
                        {
                            return _dt;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public DefectSubType(int dType, string dDescrip, string toolTip)
            {
                Id = dType;
                Description = dDescrip;
                ToolTip = toolTip;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect type information
            /// </summary>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static ObservableCollection<DefectSubType> GetCollection()
            {
                var _rtnColl = new ObservableCollection<DefectSubType>();
                foreach (DataRow _row in MasterDataSet.Tables["DefectSubType"].Rows)
                {
                    if (_rtnColl.Count(o => o.Id == _row.SafeGetField<int>("ID")) == 0)
                    {
                        _rtnColl.Add(new DefectSubType(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), _row.SafeGetField<string>("ToolTip")));
                    }
                }
                return _rtnColl;
            }
        }

        public class DefectType
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }
            public FormType QmsFormType { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load a table with all the NCR defect type information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static DataTable GetDefectTypeTable(SqlConnection sqlCon)
            {
                using (var _dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Type]", sqlCon))
                            {
                                adapter.Fill(_dt);
                            }
                            return _dt;
                        }
                        catch (SqlException)
                        {
                            return _dt;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public DefectType(int dType, string dDescrip, FormType formType)
            {
                Id = dType;
                Description = dDescrip;
                QmsFormType = formType;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect type information
            /// </summary>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static ObservableCollection<DefectType> GetDefectTypeCollection()
            {
                var _rtnColl = new ObservableCollection<DefectType>();
                foreach (DataRow _row in MasterDataSet.Tables["DefectType"].Rows)
                {
                    _rtnColl.Add(new DefectType(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description"), Enum.TryParse(_row.SafeGetField<string>("FormType"), out FormType ft) ? ft : FormType.NCR));
                }
                return _rtnColl;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect type information
            /// </summary>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static ObservableCollection<DefectType> GetDefectTypeCollection(int subTypeId)
            {
                var _rtnColl = new ObservableCollection<DefectType>();
                foreach (DataRow _cRow in MasterDataSet.Tables["CategoryFilter"].Select($"[SubTypeId] = {subTypeId}"))
                {
                    foreach (DataRow _tRow in MasterDataSet.Tables["DefectType"].Select($"[ID]={_cRow.SafeGetField<int>("TypeId")}"))
                    {
                        if (_rtnColl.Count(o => o.Id == _tRow.SafeGetField<int>("ID")) == 0)
                        {
                            _rtnColl.Add(new DefectType(_tRow.SafeGetField<int>("ID"), _tRow.SafeGetField<string>("Description"), Enum.TryParse(_tRow.SafeGetField<string>("FormType"), out FormType ft) ? ft : FormType.NCR));
                        }
                    }
                }
                return _rtnColl;
            }
        }

        public class DefectReason
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load an observable collection with all the NCR defect reason information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR reason types</returns>
            public static DataTable GetDefectReasonTable(SqlConnection sqlCon)
            {
                using (var _dt = new DataTable())
                {
                    if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                    {
                        try
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[DEFECT-CSTM_Reason]", sqlCon))
                            {
                                adapter.Fill(_dt);
                            }
                            return _dt;
                        }
                        catch (SqlException)
                        {
                            return _dt;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                    }
                }
            }

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public DefectReason(int dReason, string dDescrip)
            {
                Id = dReason;
                Description = dDescrip;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect reason information
            /// </summary>
            /// <returns>An ObservableCollection of NCR reason types</returns>
            public static ObservableCollection<DefectReason> GetDefectReasonCollection()
            {
                var _rtnColl = new ObservableCollection<DefectReason>();
                foreach (DataRow _row in MasterDataSet.Tables["DefectReason"].Rows)
                {
                    _rtnColl.Add(new DefectReason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
                }
                return _rtnColl;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect reason information
            /// </summary>
            /// <returns>An ObservableCollection of NCR reason types</returns>
            public static ObservableCollection<DefectReason> GetDefectReasonCollection(int typeId, int subTypeId)
            {
                var _rtnColl = new ObservableCollection<DefectReason>();
                foreach (DataRow _cRow in MasterDataSet.Tables["CategoryFilter"].Select($"[TypeId] = {typeId} AND [SubTypeId] = {subTypeId}"))
                {
                    foreach (DataRow _row in MasterDataSet.Tables["DefectReason"].Select($"[ID]={_cRow.SafeGetField<int>("ReasonId")}"))
                    {
                        if (_rtnColl.Count(o => o.Id == _row.SafeGetField<int>("ID")) == 0)
                        {
                            _rtnColl.Add(new DefectReason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
                        }
                    }
                }
                return _rtnColl;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect reason information
            /// </summary>
            /// <returns>An ObservableCollection of NCR reason types</returns>
            public static ObservableCollection<DefectReason> GetDefectReasonCollection(char supplierCategory)
            {
                var _rtnColl = new ObservableCollection<DefectReason>();
                var _reason = supplierCategory == 'E' ? 2 : 3;
                var _row = MasterDataSet.Tables["DefectReason"].Select($"[ID] = {_reason}").FirstOrDefault();
                _rtnColl.Add(new DefectReason(_row.SafeGetField<int>("ID"), _row.SafeGetField<string>("Description")));
                return _rtnColl;
            }
        }

        public class Notice
        {
            #region Properties

            public DataTable Table { get; set; }

            #endregion

            public Notice()
            {
                if (Table == null)
                {
                    Table = MasterDataSet.Tables.Contains("QmsNotice")
                        ? MasterDataSet.Tables["QmsNotice"].Select("[NcrRevisionId] = [RevisionFilter]").CopyToDataTable()
                        : new DataTable();
                }
            }
        }

        public class Revision : ModelBase
        {
            #region Properties

            public int RevisionId { get; set; }

            private CrewMember _submitter;
            public CrewMember Submitter
            {
                get
                { return _submitter; }
                set
                {
                    _submitter = value;
                    OnPropertyChanged(nameof(Submitter));
                }
            }

            private DateTime _submitDT;
            public DateTime SubmitDateTime
            {
                get
                { return _submitDT; }
                set
                {
                    _submitDT = value;
                    OnPropertyChanged(nameof(SubmitDateTime));
                }
            }
            public DefectReason DefectReason { get; set; }
            public DefectType DefectType { get; set; }
            public DefectSubType DefectSubType { get; set; }

            private int _actLoss;
            public int ActualLoss
            {
                get
                { return _actLoss; }
                set
                {
                    _actLoss = value;
                    OnPropertyChanged(nameof(ActualLoss));
                }
            }

            private double _actCost;
            public double ActualCost
            {
                get
                { return _actCost; }
                set
                {
                    _actCost = value;
                    OnPropertyChanged(nameof(ActualCost));
                }
            }

            public Disposition Disposition { get; set; }
            public string Description { get; set; }

            private bool _cur;
            public bool Current
            {
                get
                { return _cur; }
                set
                {
                    _cur = value;
                    OnPropertyChanged(nameof(Current));
                }
            }

            private FormType _ftype;
            public FormType RevFormType
            {
                get
                { return _ftype; }
                set
                {
                    _ftype = value;
                    OnPropertyChanged(nameof(RevFormType));
                }
            }

            public Supplier FormSupplier { get; set; }

            #endregion

            /// <summary>
            /// NCR revisions default constructor
            /// </summary>
            public Revision(CrewMember submitter, FormType type)
            {
                RevisionId = 1;
                Submitter = submitter;
                SubmitDateTime = DateTime.Now;
                RevFormType = type;
            }

            /// <summary>
            /// Ncr Overridden Constructor
            /// <param name="ncrId">NCR Id to load</param>
            /// </summary>
            public Revision(DataRow ncrDataRow, double prodVal)
            {
                RevisionId = ncrDataRow.Field<int>("NcrRevisionId");
                Submitter = new CrewMember(ncrDataRow.Field<string>("SubmitterId"), false);
                SubmitDateTime = ncrDataRow.Field<DateTime>("RevisionDateTime");
                RevFormType = Enum.TryParse(ncrDataRow.Field<string>("FormType"), out FormType ft) ? ft : FormType.NCR;
                DefectReason = new DefectReason(ncrDataRow.Field<int>("ReasonId"), ncrDataRow.Field<string>("ReasonDescription"));
                DefectSubType = new DefectSubType(ncrDataRow.Field<int>("SubTypeId"), ncrDataRow.Field<string>("SubTypeDescription"), "");
                DefectType = new DefectType(ncrDataRow.Field<int>("TypeId"), ncrDataRow.Field<string>("TypeDescription"), RevFormType);
                Disposition = new Disposition(ncrDataRow.Field<int>("DispositionId"), ncrDataRow.Field<string>("DispositionDescription"), ncrDataRow.Field<string>("FormStatus"));
                FormSupplier = new Supplier(ncrDataRow.Field<int>("SupplierId"));
                Description = ncrDataRow.Field<string>("Description");
                Current = ncrDataRow.Field<int>("RevisionFilter") == RevisionId;
                ActualCost = ncrDataRow.SafeGetField<double>("ScrapCost");
                ActualLoss = ncrDataRow.SafeGetField<int>("ScrapQuantity");
            }
        }

        #region Properties

        private int _frmId;
        public int FormId
        {
            get
            { return _frmId; }
            set
            {
                _frmId = value;
                OnPropertyChanged(nameof(FormId));
            }
        }
        public int TempId;

        private string _orderId;
        public string OrderId 
        {
            get
            { return _orderId; }
            set
            {
                _orderId = value;
                IsValidOrder = WorkOrder.Exists(value, int.TryParse(OrderSeqId, out int i) ? i : 0);
                OnPropertyChanged(nameof(OrderId));
            }
        }
        private int? _ordSeqId;
        public string OrderSeqId
        { 
            get
            { return _ordSeqId.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    _ordSeqId = i;
                    IsValidOrder = WorkOrder.Exists(OrderId, i);
                }
                else
                {
                    _ordSeqId = null;
                }
                OnPropertyChanged(nameof(OrderSeqId));
            }
        }

        private bool _isValOrd;
        public bool IsValidOrder
        {
            get
            { return _isValOrd; }
            set
            {
                _isValOrd = value;
                if (value && int.TryParse(OrderSeqId, out int i))
                {
                    PartCollection = Sku.GetSkuCollection(OrderId, i);
                    OnPropertyChanged(nameof(PartCollection));
                    Part = PartCollection.FirstOrDefault();
                    FoundWorkCenter = new Machine(Machine.GetMachineID(OrderId, i));
                }
                OnPropertyChanged(nameof(IsValidOrder));
            }
        }

        public ObservableCollection<Sku> PartCollection { get; set; }

        private Sku _part;
        public Sku Part 
        {
            get
            { return _part; }
            set
            {
                _part = value;
                ProductValue = Sku.GetPartValue(value.SkuNumber);
                if (Sku.IsLotTracable(value.SkuNumber, value.Facility) && (LotList == null || LotList.Count == 0))
                {
                    LotList = new BindingList<Lot>();
                    LotList.ListChanged += LotList_Changed;
                    LotList.Add(new Lot());
                }
                OnPropertyChanged(nameof(Part));
                OnPropertyChanged(nameof(IsEscape));
                OnPropertyChanged(nameof(LotList));
            }
        }
        public bool IsEscape 
        { 
            get 
            {
                if (Part != null && PartCollection != null)
                {
                    var _part = PartCollection.FirstOrDefault(o => o.SkuNumber == Part.SkuNumber);
                    return PartCollection.IndexOf(_part) > 0;
                }
                return false;
            } 
        }

        
        public BindingList<Lot> LotList { get; set; }

        private Machine _foundWC;
        public Machine FoundWorkCenter
        {
            get
            { return _foundWC; }
            set
            {
                _foundWC = value;
                OnPropertyChanged(nameof(FoundWorkCenter));
            }
        }

        private CrewMember _reporter;
        public CrewMember Reporter
        {
            get
            { return _reporter; }
            set
            {
                _reporter = value;
                OnPropertyChanged(nameof(Reporter));
            }
        }

        private double _prodValue;
        public double ProductValue
        {
            get
            { return _prodValue; }
            set
            {
                _prodValue = value;
                OnPropertyChanged(nameof(ProductValue));
            }
        }
        public int Site { get; set; }
        public IList<Revision> RevisionList { get; set; }

        public ObservableCollection<string> PhotoCollection { get; set; }

        public static bool LotChanging;

        #endregion

        /// <summary>
        /// Ncr Default Constructor
        /// </summary>
        public QmsForm(CrewMember submitter, FormType frmType)
        {
            RevisionList = new List<Revision>{ new Revision(submitter, frmType) };
            OrderId = string.Empty;
            Site = int.TryParse(submitter.Facility, out int i) ? i : 1;
            TempId = int.TryParse(DateTime.Now.ToString("HHmmss"), out int id) ? id : 123456;
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// <param name="id">NCR Id to load</param>
        /// </summary>
        public QmsForm(int id)
        {
            var ncrDataRows = MasterDataSet.Tables["QmsNotice"].Select($"[NcrId] = '{id}'", "[NcrRevisionId] DESC");
            FormId = id;
            OrderId = ncrDataRows[0].Field<string>("WorkOrderId");
            OrderSeqId = ncrDataRows[0].Field<int>("WorkOrderSeqId").ToString();
            Part = new Sku(ncrDataRows[0].Field<string>("PartId"));
            FoundWorkCenter = new Machine(ncrDataRows[0].Field<int>("FoundWorkCenterId"));
            Reporter = new CrewMember(ncrDataRows[0].Field<string>("ReporterId"), false);
            ProductValue = double.TryParse(ncrDataRows[0].Field<decimal>("ProductValue").ToString(), out double d) ? d : 0.00;
            Site = ncrDataRows[0].Field<int>("Site");
            RevisionList = new List<Revision>();
            foreach (var ncr in ncrDataRows)
            {
                RevisionList.Add(new Revision(ncr, ProductValue));
            }
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            PartCollection = int.TryParse(OrderSeqId, out int i) ? Sku.GetSkuCollection(OrderId, i) : Sku.GetSkuCollection(OrderId, 10);
                            OnPropertyChanged(nameof(PartCollection));
                            LotList = GetNcrLotList(id, ModelSqlCon);
                            LotList.ListChanged += LotList_Changed;
                            OnPropertyChanged(nameof(LotList));
                            PhotoCollection = new ObservableCollection<string>(GetNcrPhotoList(id, ModelSqlCon));
                            OnPropertyChanged(nameof(PhotoCollection));
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// </summary>
        /// <param name="workOrder">WorkOrder object</param>
        /// <param name="submitter">Current User</param>
        /// <param name="frmType">Type of form to create</param>
        public QmsForm(WorkOrder workOrder, CrewMember submitter, FormType frmType)
        {
            OrderId = workOrder.OrderNumber;
            OrderSeqId = workOrder.Seq;
            IsValidOrder = true;
            RevisionList = new List<Revision> { new Revision(submitter, frmType) };
            Site = workOrder.Facility;
            TempId = int.TryParse(DateTime.Now.ToString("HHmmss"), out int id) ? id : 123456;
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// </summary>
        public QmsForm(QmsForm frmObj)
        {
            FormId = frmObj.FormId;
            OrderId = frmObj.OrderId;
            OrderSeqId = frmObj.OrderSeqId;
            IsValidOrder = true;
            Part = PartCollection.FirstOrDefault(o => o.SkuNumber == frmObj.Part.SkuNumber);
            LotList = frmObj.LotList;
            Reporter = frmObj.Reporter;
            ProductValue = frmObj.ProductValue;
            Site = frmObj.Site;
            RevisionList = frmObj.RevisionList;
            TempId = int.TryParse(DateTime.Now.ToString("HHmmss"), out int id) ? id : 123456;
        }

        #region Data Access

        /// <summary>
        /// Load a datatable with all the NCR Notice information
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of NCR Notice information</returns>
        public static DataTable GetNoticeTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [dbo].[SFW_NcrNotice] ncr WHERE ncr.[Site] = @p1 ORDER BY ncr.[RevisionDateTime] DESC", sqlCon))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("p1", site);
                            adapter.Fill(_dt);
                        }
                        using (DataTable _sdt = new DataTable())
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT
	CASE WHEN ISNUMERIC([NcrId]) = 0
		THEN REPLACE(UPPER([NcrId]), 'NCR', '')
		ELSE [NcrId] end as 'NcrId'
	,SUM([Quantity]) as 'Quantity'
	,CAST(ROUND(SUM([ScrapCost]),3) as numeric(12,3)) as 'Cost'
FROM
	[dbo].[SFW_ScrapCost]
WHERE
	[NcrId] IS NOT NULL AND [NcrId] <> ''
GROUP BY
	[NcrId]", sqlCon))
                            {
                                adapter.Fill(_sdt);
                            }
                            _dt.Columns.Add(new DataColumn("ScrapQuantity", typeof(int)));
                            _dt.Columns.Add(new DataColumn("ScrapCost", typeof(double)));
                            foreach (DataRow _row in _sdt.Rows)
                            {
                                var _id = int.TryParse(_row.ItemArray[0].ToString(), out int i) ? i : 0;
                                var _qty = int.TryParse(_row.ItemArray[1].ToString(), out i) ? i : 0;
                                var _cost = double.TryParse(_row.ItemArray[2].ToString(), out double d) ? d : 0.0;
                                if (_id > 0 && _qty > 0 && _cost > 0)
                                {
                                    var _dtRows = _dt.Select($"[NcrId] = {_id}");
                                    foreach (var _dtRow in _dtRows)
                                    {
                                        var _index = _dt.Rows.IndexOf(_dtRow);
                                        _dt.Rows[_index].SetField("ScrapQuantity", _qty);
                                        _dt.Rows[_index].SetField("ScrapCost", _cost);
                                    }
                                }
                            }
                        }
                        return _dt;
                    }
                    catch (SqlException)
                    {
                        return _dt;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        /// <summary>
        /// Load a datatable with all the QMS form automation information
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of form automation information</returns>
        public static DataTable GetCategoryLinkTable(SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [dbo].[DEFECT-CSTM_CategoryLinks]", sqlCon))
                        {
                            adapter.Fill(_dt);
                        }
                        return _dt;
                    }
                    catch (SqlException)
                    {
                        return _dt;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
                }
            }
        }

        /// <summary>
        /// Load a list with all the NCR lot information
        /// </summary>
        /// <param name="ncrId">Ncr object ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of NCR Notice information</returns>
        public static BindingList<Lot> GetNcrLotList(int ncrId, SqlConnection sqlCon)
        {
            var _rtnList = new BindingList<Lot>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[SFW_DefectLotLink] ncrLot WHERE ncrLot.[NcrId] = @p1 AND ncrLot.[LotId] <> ''", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _rtnList.Add(new Lot(reader.SafeGetString("LotId"), true));
                                }
                            }
                        }
                    }
                    return _rtnList;
                }
                catch (SqlException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Load a list with all the NCR lot information
        /// </summary>
        /// <param name="ncrId">Ncr object ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of NCR Notice information</returns>
        public static string GetNcrId(string lotId, SqlConnection sqlCon)
        {
            var _rtnVal = string.Empty;
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT [NcrId] FROM [dbo].[SFW_DefectLotLink] ncrLot WHERE ncrLot.[LotId] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (string.IsNullOrEmpty(_rtnVal))
                                    {
                                        _rtnVal = reader.SafeGetString("NcrId");
                                    }
                                    else
                                    {
                                        _rtnVal += $", {reader.SafeGetString("NcrId")}";
                                    }
                                }
                            }
                        }
                    }
                    return _rtnVal;
                }
                catch (SqlException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Load a list with all the NCR photo information
        /// </summary>
        /// <param name="ncrId">Ncr object ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of NCR Notice information</returns>
        public static List<string> GetNcrPhotoList(int ncrId, SqlConnection sqlCon)
        {
            var _rtnList = new List<string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[NCR-CSTM_PhotoPath] ncrPic WHERE ncrPic.[NcrId] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                var _folderPath = $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                                while (reader.Read())
                                {
                                    _rtnList.Add($"{_folderPath}{reader.SafeGetString("PhotoPath")}");
                                }
                            }
                        }
                    }
                    return _rtnList;
                }
                catch (SqlException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }

        /// <summary>
        /// Find out if a photo exists already in the database
        /// </summary>
        /// <param name="ncrId">NCR Id</param>
        /// <param name="photoId">Photo ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool PhotoExists(int ncrId, string photoId, SqlConnection sqlCon)
        {
            if (photoId.Contains("waxfs001"))
            {
                photoId = photoId.Replace($"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\", "");
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"SELECT COUNT([NcrId]) FROM [dbo].[NCR-CSTM_PhotoPath] WHERE [NcrId] = @p1 AND [PhotoPath] = @p2", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", photoId);
                    return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i > 0 : true;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// Submit NCR photo path
        /// </summary>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="photo">Photo path</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void SubmitPhotoPath(int ncrId, string photo, SqlConnection sqlCon)
        {
            if (photo.Contains("waxfs001"))
            {
                photo = photo.Replace($"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\", "");
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_PhotoPath] ([NcrId], [PhotoPath]) Values(@p1, @p2)", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", photo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Submit NCR photo path
        /// </summary>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="photo">Photo path</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void DeletePhotoPath(int ncrId, string photo, SqlConnection sqlCon)
        {
            if (photo.Contains("waxfs001"))
            {
                photo = photo.Replace($"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\", "");
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [dbo].[NCR-CSTM_PhotoPath] WHERE [NcrId] = @p1 AND [PhotoPath] = @p2", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", photo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        /// <summary>
        /// Happens when an item is added or changed in the Lot Binding List property
        /// </summary>
        /// <param name="sender">BindingList<Lot> list passed without changes</param>
        /// <param name="e">Change info</param>
        public static void LotList_Changed(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (sender != null && e.PropertyDescriptor.DisplayName == "LotNumber" && !LotChanging)
                {
                    if (!string.IsNullOrEmpty(((BindingList<Lot>)sender)[e.NewIndex].LotNumber))
                    {
                        var _lot = ((BindingList<Lot>)sender)[e.NewIndex].LotNumber.Contains("|")
                            ? ((BindingList<Lot>)sender)[e.NewIndex].LotNumber.Split('|')[0]
                            : ((BindingList<Lot>)sender)[e.NewIndex].LotNumber;
                        var _isValid = Lot.IsValid($"{_lot}|P|01", ModelSqlCon);
                        if (_isValid)
                        {
                            ((BindingList<Lot>)sender)[e.NewIndex].Validated = _isValid;
                            LotChanging = true;
                            ((BindingList<Lot>)sender)[e.NewIndex].LotNumber += ((BindingList<Lot>)sender)[e.NewIndex].LotNumber.Contains("|")
                                ? ""
                                : "|P|01";
                        }
                        else
                        {
                            ((BindingList<Lot>)sender)[e.NewIndex].Validated = _isValid;
                        }
                    }
                }
            }
            LotChanging = false;
        }

        /// <summary>
        /// Gets a list of NCR IDs that exist on a work order
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <returns>List of NCR ID's as strings</returns>
        public static IList<string> GetNcrList(string orderId)
        {
            var _rtnList = new List<string>();
            var _rows = MasterDataSet.Tables["QmsNotice"].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]");
            if (_rows.Count() > 0)
            {
                foreach (var _row in _rows)
                {
                    _rtnList.Add($"{_row.SafeGetField<int>("NcrId")} {_row.SafeGetField<string>("TypeDescription")}");
                }
            }
            return _rtnList;
        }

        /// <summary>
        /// Get NCR ID that exist on a work order
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <returns>NCR IDs' as string</returns>
        public static string GetNcrId(string orderId)
        {
            var _rtnVal = string.Empty;
            foreach (var _row in MasterDataSet.Tables["QmsNotice"].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]"))
            {
                if (string.IsNullOrEmpty(_rtnVal))
                {
                    _rtnVal = _row.SafeGetField<int>("NcrId").ToString();
                }
                else
                {
                    _rtnVal += $", {_row.SafeGetField<int>("NcrId")}";
                }
            }
            return _rtnVal;
        }

        /// <summary>
        /// Gets the NCR Reason
        /// </summary>
        /// <param name="orderId">ncrId</param>
        /// <returns>NCR Reason as string</returns>
        public static string GetNcrReason(int ncrId)
        {
            return MasterDataSet.Tables["NcrNotice"].Select($"[NcrId] = '{ncrId}' AND [NcrRevisionId] = [RevisionFilter]").FirstOrDefault().SafeGetField<string>("DefectReason");
        }

        /// <summary>
        /// Checks to see if it is a valid NCR
        /// </summary>
        /// <param name="ncrId">Ncr ID</param>
        /// <returns>pass flag as bool</returns>
        public static bool IsValid(int ncrId)
        {
            return MasterDataSet.Tables["QmsNotice"].Select($"[NcrId] = '{ncrId}' AND [NcrRevisionId] = [RevisionFilter]").Count() > 0;
        }

        /// <summary>
        /// Gets the last NCR ID in the database
        /// </summary>
        /// <returns>Last NCR ID as an int</returns>
        public static int GetLastNcrId()
        {
            return MasterDataSet.Tables["NcrNotice"].Select().ToList().LastOrDefault().SafeGetField<int>("NcrId");
        }
    }

    public static class NcrExtensions
    {
        /// <summary>
        /// Submit a NCR to the NCR Master DataBase
        /// </summary>
        /// <param name="ncrObject">QIR Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Last inserted NCR ID</returns>
        public static int Submit(this QmsForm ncrObject, SqlConnection sqlCon)
        {
            var _idNumber = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[DEFECT-CSTM] ([WorkOrderId], [WorkOrderSeqId], [PartId], [FoundWorkCenterId], [ReporterId], [ProductValue], [Site], [IsEscape])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8);
                                                        SELECT [NcrId] FROM [dbo].[DEFECT-CSTM] WHERE [NcrId] = @@IDENTITY;", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrObject.OrderId);
                    cmd.Parameters.AddWithValue("p2", ncrObject.OrderSeqId);
                    cmd.Parameters.AddWithValue("p3", ncrObject.Part.SkuNumber);
                    cmd.Parameters.AddWithValue("p4", ncrObject.FoundWorkCenter.MachineNumber);
                    cmd.Parameters.AddWithValue("p5", ncrObject.Reporter.ErpId);
                    cmd.Parameters.AddWithValue("p6", ncrObject.ProductValue);
                    cmd.Parameters.AddWithValue("p7", ncrObject.Site);
                    cmd.Parameters.AddWithValue("p8", ncrObject.IsEscape ? 1 : 0);
                    _idNumber = Convert.ToInt32(cmd.ExecuteScalar());
                    ncrObject.FormId = _idNumber;
                }
                ncrObject.RevisionList.Last().Submit(_idNumber, 1, sqlCon);
                if (ncrObject.Part.IsLotTrace && ncrObject.LotList.Count(o => !string.IsNullOrEmpty(o.LotNumber)) > 0)
                {
                    ncrObject.SubmitLots(sqlCon);
                }
                if (ncrObject.PhotoCollection != null && ncrObject.PhotoCollection.Count > 0)
                {
                    ncrObject.SubmitPhotoPath(true, sqlCon);
                }
                return _idNumber;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Submit a NCR revision to the NCR revision DataBase
        /// </summary>
        /// <param name="ncrRev">NCR revision object</param>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="ncrRevId">NCR Revision ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Submit(this QmsForm.Revision ncrRev, int ncrId, int ncrRevId, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[DEFECT-CSTM_Revisions] ([NcrId], [NcrRevisionId], [SubmitterId], [RevisionDateTime], [ReasonId], [SubTypeId], [TypeId], [DispositionId], [SupplierId], [Description])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10);", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", ncrRevId);
                    cmd.Parameters.AddWithValue("p3", ncrRev.Submitter.ErpId);
                    cmd.Parameters.AddWithValue("p4", ncrRev.SubmitDateTime.ToString("yyyy-MM-dd HH:mm"));
                    cmd.Parameters.AddWithValue("p5", ncrRev.DefectReason.Id);
                    cmd.Parameters.AddWithValue("p6", ncrRev.DefectSubType.Id);
                    cmd.Parameters.AddWithValue("p7", ncrRev.DefectType.Id);
                    cmd.Parameters.AddWithValue("p8", ncrRev.Disposition.Id);
                    cmd.Parameters.AddWithValue("p9", ncrRev.FormSupplier != null ? ncrRev.FormSupplier.Id : 0);
                    cmd.Parameters.AddWithValue("p10", ncrRev.Description);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Submit a NCR to the NCR Master DataBase
        /// </summary>
        /// <param name="ncrObj">QIR Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Last inserted NCR ID</returns>
        public static void SubmitLots(this QmsForm ncrObj, SqlConnection sqlCon)
        {
            try
            {
                var _oldLotList = QmsForm.GetNcrLotList(ncrObj.FormId, sqlCon);
                foreach (var lot in ncrObj.LotList.Where(o => o.Validated))
                {
                    if (_oldLotList.Count(o => o.LotNumber == lot.LotNumber) == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[DEFECT-CSTM_EscapeLot] ([NcrId], [LotId]) Values(@p1, @p2)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", ncrObj.FormId);
                            cmd.Parameters.AddWithValue("p2", lot.LotNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                foreach (var oldLot in _oldLotList)
                {
                    if (ncrObj.LotList.Count(o => o.LotNumber == oldLot.LotNumber) == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [dbo].[DEFECT-CSTM_EscapeLot] WHERE [NcrId] = @p1 AND [LotId] = @p2", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", ncrObj.FormId);
                            cmd.Parameters.AddWithValue("p2", oldLot.LotNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Submit NCR photo path
        /// </summary>
        /// <param name="frmObj">QMS Form Object</param>
        /// <param name="newFrm">Validation that it is coming from a new QMS Form</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void SubmitPhotoPath(this QmsForm frmObj, bool newFrm, SqlConnection sqlCon)
        {
            try
            {
                var _folderPath = newFrm ? $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\Temp\\" : $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                foreach (var fullPathPhoto in frmObj.PhotoCollection)
                {
                    var _photo = fullPathPhoto.Replace(_folderPath, "");
                    using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_PhotoPath] ([NcrId], [PhotoPath]) Values(@p1, @p2)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", frmObj.FormId);
                        cmd.Parameters.AddWithValue("p2", _photo);
                        cmd.ExecuteNonQuery();
                    }
                    if (newFrm)
                    {
                        File.Move(fullPathPhoto, $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\{_photo}");
                        frmObj.PhotoCollection[frmObj.PhotoCollection.IndexOf(fullPathPhoto)] = "";
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
