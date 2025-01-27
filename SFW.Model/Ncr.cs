using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Ncr : ModelBase
    {
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
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[NCR-CSTM_Disposition] WHERE [DispositionId] <> 7", sqlCon))
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
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[NCR-CSTM_Disposition] WHERE [DispositionId] <> 7", sqlCon))
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
                foreach (DataRow _row in MasterDataSet.Tables["NcrDispo"].Rows)
                {
                    _rtnColl.Add(new Disposition(_row.SafeGetField<int>("DispositionId"), _row.SafeGetField<string>("DispositionDescription"), _row.SafeGetField<string>("LinkedStatus")));
                }
                return _rtnColl;
            }
        }

        public class DefectType
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }

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
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT * FROM [dbo].[NCR-CSTM_DefectType]", sqlCon))
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
            public DefectType(int dType, string dDescrip)
            {
                Id = dType;
                Description = dDescrip;
            }

            /// <summary>
            /// Load an observable collection with all the NCR defect type information
            /// </summary>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static ObservableCollection<DefectType> GetDefectTypeCollection()
            {
                var _rtnColl = new ObservableCollection<DefectType>();
                foreach (DataRow _row in MasterDataSet.Tables["NcrType"].Rows)
                {
                    _rtnColl.Add(new DefectType(_row.SafeGetField<int>("DefectTypeId"), _row.SafeGetField<string>("DefectTypeDescription")));
                }
                return _rtnColl;
            }
        }

        public class DefectReason
        {
            #region Properties

            public string Id { get; set; }
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
                            using (SqlDataAdapter adapter = new SqlDataAdapter($@"SELECT tmar.[Reason_Code] as 'DefectReasonId', tmar.[Reason_Description] as 'DefectReasonDescription' FROM [dbo].[TM-INIT_Adjust_Reasons] tmar WHERE tmar.[Reason_Code] LIKE 'Q%'", sqlCon))
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
            public DefectReason(string dReason, string dDescrip)
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
                foreach (DataRow _row in MasterDataSet.Tables["NcrReason"].Rows)
                {
                    _rtnColl.Add(new DefectReason(_row.SafeGetField<string>("DefectReasonId"), _row.SafeGetField<string>("DefectReasonDescription")));
                }
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
                    Table = new DataTable();
                    Table = MasterDataSet.Tables["NcrNotice"].Select("[NcrRevisionId] = [RevisionFilter]").CopyToDataTable();
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

            private int _potLoss;
            public int PotentialLoss
            {
                get
                { return _potLoss; }
                set
                {
                    _potLoss = value;
                    OnPropertyChanged(nameof(PotentialLoss));
                }
            }

            private double _potVal;
            public double PotentialValue
            {
                get
                { return _potVal; }
                set
                {
                    _potVal = value;
                    OnPropertyChanged(nameof(PotentialValue));
                }
            }

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

            #endregion

            /// <summary>
            /// NCR revisions default constructor
            /// </summary>
            public Revision(CrewMember submitter)
            {
                RevisionId = 1;
                Submitter = submitter;
                SubmitDateTime = DateTime.Now;
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
                DefectReason = new DefectReason(ncrDataRow.Field<string>("DefectReason"), ncrDataRow.Field<string>("DefectReasonDescription"));
                DefectType = new DefectType(ncrDataRow.Field<int>("DefectType"), ncrDataRow.Field<string>("DefectTypeDescription"));
                PotentialLoss = ncrDataRow.Field<int>("PotentialLoss");
                PotentialValue = ncrDataRow.Field<int>("PotentialLoss") * prodVal;
                Disposition = new Disposition(ncrDataRow.Field<int>("DispositionId"), ncrDataRow.Field<string>("DispositionDescription"), ncrDataRow.Field<string>("LinkedStatus"));
                Description = ncrDataRow.Field<string>("Description");
                Current = ncrDataRow.Field<int>("RevisionFilter") == RevisionId;
            }
        }

        #region Properties

        private int _ncrId;
        public int NcrId
        {
            get
            { return _ncrId; }
            set
            {
                _ncrId = value;
                OnPropertyChanged(nameof(NcrId));
            }
        }

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
                if (Sku.IsLotTracable(value.SkuNumber) && (LotList == null || LotList.Count == 0))
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
        public Ncr(CrewMember submitter)
        {
            RevisionList = new List<Revision>{ new Revision(submitter) };
            OrderId = string.Empty;
            Site = int.TryParse(submitter.Facility, out int i) ? i : 1;
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// <param name="id">NCR Id to load</param>
        /// </summary>
        public Ncr(int id)
        {
            var ncrDataRows = MasterDataSet.Tables["NcrNotice"].Select($"[NcrId] = '{id}'", "[NcrRevisionId] DESC");
            NcrId = id;
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
                            LotList = GetNcrLotList(id, ModelSqlCon);
                            LotList.ListChanged += LotList_Changed;
                            PhotoCollection = new ObservableCollection<string>(GetNcrPhotoList(id, ModelSqlCon));
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
        public Ncr(WorkOrder workOrder, CrewMember submitter)
        {
            OrderId = workOrder.OrderNumber;
            OrderSeqId = workOrder.Seq;
            IsValidOrder = true;
            RevisionList = new List<Revision> { new Revision(submitter) };
            Site = workOrder.Facility;
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
                    using (SqlCommand cmd = new SqlCommand($"SELECT * FROM [dbo].[NCR-CSTM_LotInfo] ncrLot WHERE ncrLot.[NcrId] = @p1", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId);
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
        /// Submit Lot numbers to an NCR
        /// </summary>
        /// <param name="ncrId">NCR object ID</param>
        /// <param name="lotId">Lot ID</param>
        public static void SubmitLot(int ncrId, string lotId)
        {
            if (!lotId.Contains("|"))
            {
                lotId += $"{lotId}|P|01";
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"SELECT COUNT([NcrId]) FROM [dbo].[NCR-CSTM_LotInfo] WHERE [NcrId] = )", ModelSqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", lotId);
                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_LotInfo] ([NcrId], [LotId]) Values(@p1, @p2)", ModelSqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", lotId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {

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

        /// <summary>
        /// Get NCR actual loss and actual cost
        /// </summary>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static IReadOnlyDictionary<int, double> GetActuals(int ncrId, SqlConnection sqlCon)
        {
            var _rtnDict = new Dictionary<int, double>();
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"SELECT SUM(Quantity) as 'Quantity', SUM(ScrapCost) as 'ScrapCost' FROM [dbo].[SFW_ScrapCost] WHERE [NcrId] = @p1", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while(reader.Read())
                            {
                                _rtnDict.Add(reader.SafeGetInt32("Quantity"), reader.SafeGetDouble("ScrapCost"));
                            }
                        }
                    }
                }
                return _rtnDict;
            }
            catch (Exception)
            {
                return _rtnDict;
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
                        var _isValid = Lot.IsValid($"{((BindingList<Lot>)sender)[e.NewIndex].LotNumber}|P|01", ModelSqlCon);
                        if (_isValid)
                        {
                            ((BindingList<Lot>)sender)[e.NewIndex].Validated = _isValid;
                            LotChanging = true;
                            ((BindingList<Lot>)sender)[e.NewIndex].LotNumber += "|P|01";
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
            var _rows = MasterDataSet.Tables["NcrNotice"].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]");
            if (_rows.Count() > 0)
            {
                foreach (var _row in _rows)
                {
                    _rtnList.Add(_row.SafeGetField<int>("NcrId").ToString());
                }
            }
            return _rtnList;
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
            return MasterDataSet.Tables["NcrNotice"].Select($"[NcrId] = '{ncrId}' AND [NcrRevisionId] = [RevisionFilter]").Count() > 0;
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
        public static int Submit(this Ncr ncrObject, SqlConnection sqlCon)
        {
            var _idNumber = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM] ([WorkOrderId], [WorkOrderSeqId], [PartId], [FoundWorkCenterId], [ReporterId], [ProductValue], [Site], [IsEscape])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8);
                                                        SELECT [NcrId] FROM [dbo].[NCR-CSTM] WHERE [NcrId] = @@IDENTITY;", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrObject.OrderId);
                    cmd.Parameters.AddWithValue("p2", ncrObject.OrderSeqId);
                    cmd.Parameters.AddWithValue("p3", ncrObject.Part.SkuNumber);
                    cmd.Parameters.AddWithValue("p4", ncrObject.FoundWorkCenter.MachineNumber);
                    cmd.Parameters.AddWithValue("p5", ncrObject.Reporter.IdNumber);
                    cmd.Parameters.AddWithValue("p6", ncrObject.ProductValue);
                    cmd.Parameters.AddWithValue("p7", ncrObject.Site);
                    cmd.Parameters.AddWithValue("p8", ncrObject.IsEscape ? 1 : 0);
                    _idNumber = Convert.ToInt32(cmd.ExecuteScalar());
                    ncrObject.NcrId = _idNumber;
                }
                ncrObject.RevisionList.Last().Submit(_idNumber, 1, sqlCon);
                if (ncrObject.Part.IsLotTrace && ncrObject.LotList.Count(o => !string.IsNullOrEmpty(o.LotNumber)) > 0)
                {
                    ncrObject.SubmitLots(sqlCon);
                }
                if (ncrObject.PhotoCollection != null && ncrObject.PhotoCollection.Count > 0)
                {
                    ncrObject.SubmitPhotoPath(sqlCon);
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
        public static void Submit(this Ncr.Revision ncrRev, int ncrId, int ncrRevId, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_Revisions] ([NcrId], [NcrRevisionId], [SubmitterId], [RevisionDateTime], [DefectReason], [DefectType], [PotentialLoss], [DispositionId], [Description])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9);", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", ncrRevId);
                    cmd.Parameters.AddWithValue("p3", ncrRev.Submitter.IdNumber);
                    cmd.Parameters.AddWithValue("p4", ncrRev.SubmitDateTime.ToString("yyyy-MM-dd HH:mm"));
                    cmd.Parameters.AddWithValue("p5", ncrRev.DefectReason.Id);
                    cmd.Parameters.AddWithValue("p6", ncrRev.DefectType.Id);
                    cmd.Parameters.AddWithValue("p7", ncrRev.PotentialLoss);
                    cmd.Parameters.AddWithValue("p8", ncrRev.Disposition.Id);
                    cmd.Parameters.AddWithValue("p9", ncrRev.Description);
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
        public static void SubmitLots(this Ncr ncrObj, SqlConnection sqlCon)
        {
            try
            {
                var _oldLotList = Ncr.GetNcrLotList(ncrObj.NcrId, sqlCon);
                foreach (var lot in ncrObj.LotList.Where(o => o.Validated))
                {
                    if (_oldLotList.Count(o => o.LotNumber == lot.LotNumber) == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_LotInfo] ([NcrId], [LotId]) Values(@p1, @p2)", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", ncrObj.NcrId);
                            cmd.Parameters.AddWithValue("p2", lot.LotNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                foreach (var oldLot in _oldLotList)
                {
                    if (ncrObj.LotList.Count(o => o.LotNumber == oldLot.LotNumber) == 0)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"DELETE FROM [dbo].[NCR-CSTM_LotInfo] WHERE [NcrId] = @p1 AND [LotId] = @p2", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", ncrObj.NcrId);
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
        /// <param name="ncrObj">QIR Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void SubmitPhotoPath(this Ncr ncrObj, SqlConnection sqlCon)
        {
            try
            {
                var _folderPath = $"\\\\waxfs001\\WAXG-SFW\\QMS Pictures\\";
                foreach (var fullPathPhoto in ncrObj.PhotoCollection)
                {
                    var _photo = fullPathPhoto.Replace(_folderPath, "");
                    using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_PhotoPath] ([NcrId], [PhotoPath]) Values(@p1, @p2)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrObj.NcrId);
                        cmd.Parameters.AddWithValue("p2", _photo);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
