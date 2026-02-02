using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Quality
{
    public class QmsForm : ModelBase
    {
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
                IsValidOrder = Production.WorkOrder.Exists(value, int.TryParse(OrderSeqId, out int i) ? i : 0);
                OnPropertyChanged(nameof(OrderId));
            }
        }
        public string LoadedOrderId { get; set; }

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
                    IsValidOrder = Production.WorkOrder.Exists(OrderId, i);
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
                    PartCollection = Product.Sku.GetCollection(OrderId, i);
                    OnPropertyChanged(nameof(PartCollection));
                    Part = PartCollection.FirstOrDefault();
                    FoundWorkCenter = new Production.Machine(Production.Machine.GetID(OrderId, i));
                }
                OnPropertyChanged(nameof(IsValidOrder));
            }
        }

        public ObservableCollection<Product.Sku> PartCollection { get; set; }

        private Product.Sku _part;
        public Product.Sku Part
        {
            get
            { return _part; }
            set
            {
                _part = value;
                ProductValue = Product.Sku.GetValue(value.SkuNumber);
                if (Product.Sku.IsLotTracable(value.SkuNumber, value.Facility) && (LotList == null || LotList.Count == 0))
                {
                    LotList = new BindingList<Product.Lot>
                    {
                        new Product.Lot()
                    };
                    LotList.ListChanged += LotList_Changed;
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


        public BindingList<Product.Lot> LotList { get; set; }
        public bool IsLotLoading { get; set; }

        private Production.Machine _foundWC;
        public Production.Machine FoundWorkCenter
        {
            get
            { return _foundWC; }
            set
            {
                _foundWC = value;
                OnPropertyChanged(nameof(FoundWorkCenter));
            }
        }

        private Management.Employee _reporter;
        public Management.Employee Reporter
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

        private bool _isSel;
        public bool IsSelected
        {
            get
            { return _isSel; }
            set
            {
                _isSel = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public int Site { get; set; }
        public IList<Revision> RevisionList { get; set; }

        public ObservableCollection<string> PhotoCollection { get; set; }

        public static bool LotChanging;

        #endregion

        #region Data Access

        /// <summary>
        /// Load a list with all the NCR lot information
        /// </summary>
        /// <param name="ncrId">Ncr object ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A list of NCR lot information</returns>
        public static BindingList<Product.Lot> GetLotList(int ncrId, string uom, SqlConnection sqlCon)
        {
            var _rtnList = new BindingList<Product.Lot>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    var _dtDefect = new DataTable();
                    using (SqlDataAdapter _dataAdapter = new SqlDataAdapter($@"SELECT sl.[NcrId], sl.[LotId], sl.[ImportType], CAST(SUM(sl.[Quantity]) as int) as 'Quantity'
FROM [dbo].[SFW_ScrapLots] sl
WHERE ISNUMERIC([NcrId]) = 1 AND [NcrId] = @p1
GROUP BY sl.[NcrId], sl.[LotId], sl.[ImportType]", sqlCon))
                    {
                        _dataAdapter.SelectCommand.Parameters.AddWithValue("p1", ncrId);
                        _dataAdapter.Fill(_dtDefect);
                        if (_dtDefect.Rows.Count > 0)
                        {
                            foreach (DataRow _row in _dtDefect.Rows)
                            {
                                var _lotId = _row.SafeGetField<string>("LotId");
                                var _import = _row.SafeGetField<string>("ImportType") == "M";
                                var _qty = _row.SafeGetField<int>("Quantity");
                                _rtnList.Add(new Product.Lot(_lotId, _qty, uom, _import, true));
                            }
                        }
                        _dataAdapter.SelectCommand.CommandText = $@"SELECT CAST(del.[NcrId] as varchar) as 'NcrId', del.[LotId], 0 as 'Quantity', 'M' as 'ImportType' FROM [dbo].[DEFECT-CSTM_EscapeLot] del WHERE [NcrId] = @p1";
                        _dataAdapter.Fill(_dtDefect);
                        if (_dtDefect.Rows.Count > 0)
                        {
                            foreach (DataRow _row in _dtDefect.Rows)
                            {
                                if (_rtnList.Count(o => o.LotNumber == _row.SafeGetField<string>("LotId")) == 0)
                                {
                                    var _lotId = _row.SafeGetField<string>("LotId");
                                    var _import = _row.SafeGetField<string>("ImportType") == "M";
                                    _rtnList.Add(new Product.Lot(_lotId, _row.SafeGetField<int>("Quantity"), uom, _import, true));
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
        /// Load a list with all the NCR information
        /// </summary>
        /// <param name="lotId">Lot ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A list of NCR ID's based on a lot ID</returns>
        public static IList<string> GetNcrList(string lotId, SqlConnection sqlCon)
        {
            var _rtnList = new List<string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    if (!lotId.Contains("|"))
                    {
                        lotId = $"{lotId}|P|01";
                    }
                    using (SqlCommand cmd = new SqlCommand($@"SELECT ncrLot.[NcrId] FROM [dbo].[SFW_DefectLotLink] ncrLot WHERE [LotId] = @p1 AND [ImportType] = 'M'", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", lotId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _rtnList.Add(reader.SafeGetString("NcrId"));
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
        /// Validated if a lot is attached to an NCR
        /// </summary>
        /// <param name="ncrId">Ncr object ID</param>
        /// <param name="lotId">Lot object ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Pass or fail as bool</returns>
        public static bool ValidNcrLot(int ncrId, string lotId, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"SELECT COUNT(ncrLot.[NcrId]) FROM [dbo].[SFW_ScrapLots] ncrLot WHERE ncrLot.[NcrId] = @p1 AND ncrLot.[LotId] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId.ToString());
                        cmd.Parameters.AddWithValue("p2", lotId.ToString());
                        var _count = int.TryParse(cmd.ExecuteScalar().ToString(), out int scr) ? scr : 0;
                        cmd.CommandText = $@"SELECT COUNT(ncrLot.[NcrId]) FROM [dbo].[DEFECT-CSTM_EscapeLot] ncrLot WHERE ncrLot.[NcrId] = @p1 AND ncrLot.[LotId] = @p2";
                        _count += int.TryParse(cmd.ExecuteScalar().ToString(), out int def) ? def : 0;
                        return _count > 0;
                    }
                }
                catch (SqlException)
                {
                    return false;
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

        /// <summary>
        /// Submit NCR lot number to the lot escape table
        /// </summary>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="lotId">Lot ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void SubmitDefectLot(int ncrId, string lotId, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand { Connection = sqlCon })
                {
                    cmd.CommandText = "SELECT COUNT([NcrId]) as 'Exists' FROM [dbo].[DEFECT-CSTM_EscapeLot] WHERE [NCrId] = @p1 AND [LotId] = @p2";
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", lotId);
                    var _lotExists = int.TryParse(cmd.ExecuteScalar().ToString(), out int i) ? i : 1;
                    if (_lotExists == 0)
                    {
                        cmd.CommandText = $@"INSERT INTO [dbo].[Defect-CSTM_EscapeLot] ([NcrId], [LotId]) Values(@p1, @p2)";
                        cmd.Parameters.AddWithValue("p1", ncrId);
                        cmd.Parameters.AddWithValue("p2", lotId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        /// <summary>
        /// Ncr Default Constructor
        /// </summary>
        public QmsForm()
        { }

        /// <summary>
        /// Ncr Overridden Constructor
        /// </summary>
        public QmsForm(Management.Employee submitter, FormType frmType)
        {
            RevisionList = new List<Revision> { new Revision(submitter, frmType) };
            OrderId = string.Empty;
            Site = int.TryParse(submitter.Facility, out int i) ? i : 1;
            TempId = int.TryParse(DateTime.Now.ToString("HHmmss"), out int id) ? id : 123456;
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// <param name="id">NCR Id to load</param>
        /// <param name="loadedId">Order ID that sent the request</param>
        /// </summary>
        public QmsForm(int id, string loadedId = "")
        {
            var ncrDataRows = MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = '{id}'", "[NcrRevisionId] DESC");
            FormId = id;
            OrderId = ncrDataRows[0].Field<string>("WorkOrderId");
            OrderSeqId = ncrDataRows[0].Field<int>("WorkOrderSeqId").ToString();
            LoadedOrderId = loadedId;
            Part = new Product.Sku(ncrDataRows[0].Field<string>("PartId"));
            FoundWorkCenter = new Production.Machine(ncrDataRows[0].Field<int>("FoundWorkCenterId"));
            Reporter = new Management.Employee(ncrDataRows[0].Field<string>("ReporterId"), false);
            ProductValue = double.TryParse(ncrDataRows[0].Field<decimal>("ProductValue").ToString(), out double d) ? d : 0.00;
            Site = ncrDataRows[0].Field<int>("Site");
            RevisionList = new List<Revision>();
            foreach (var ncr in ncrDataRows)
            {
                RevisionList.Add(new Revision(ncr, ProductValue));
            }
            using (BackgroundWorker bw1 = new BackgroundWorker())
            {
                try
                {
                    bw1.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            PartCollection = int.TryParse(OrderSeqId, out int i) ? Product.Sku.GetCollection(OrderId, i) : Product.Sku.GetCollection(OrderId, 10);
                            OnPropertyChanged(nameof(PartCollection));
                            var _photoCol = GetNcrPhotoList(id, ModelSqlCon);
                            PhotoCollection = _photoCol == null ? new ObservableCollection<string>() : new ObservableCollection<string>(_photoCol);
                            OnPropertyChanged(nameof(PhotoCollection));
                        });
                    bw1.RunWorkerAsync();
                }
                catch (Exception)
                {

                }
            }
            using (BackgroundWorker bw2 = new BackgroundWorker())
            {
                try
                {
                    IsLotLoading = true;
                    OnPropertyChanged(nameof(IsLotLoading));
                    bw2.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            LotList = GetLotList(id, Part.Uom, ModelSqlCon);
                            LotList.ListChanged += LotList_Changed;
                            OnPropertyChanged(nameof(LotList));
                            IsLotLoading = false;
                            OnPropertyChanged(nameof(IsLotLoading));
                        });
                    bw2.RunWorkerAsync();
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
        public QmsForm(Production.WorkOrder workOrder, Management.Employee submitter, FormType frmType)
        {
            OrderId = workOrder.OrderNumber;
            OrderSeqId = workOrder.Seq;
            IsValidOrder = true;
            RevisionList = new List<Revision> { new Revision(submitter, frmType) };
            Site = workOrder.Facility;
            TempId = int.TryParse(DateTime.Now.ToString("HHmmss"), out int id) ? id : 123456;
            LotList = new BindingList<Product.Lot>();
            LotList.ListChanged += LotList_Changed;
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

        /// <summary>
        /// Happens when an item is added or changed in the Lot Binding List property
        /// </summary>
        /// <param name="sender">BindingList<Lot> list passed without changes</param>
        /// <param name="e">Change info</param>
        public void LotList_Changed(object sender, ListChangedEventArgs e)
        {
            LotList.ListChanged -= LotList_Changed;
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (sender != null && e.PropertyDescriptor.DisplayName == "LotNumber" && !LotChanging)
                {
                    if (!string.IsNullOrEmpty(((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber))
                    {
                        var _lot = ((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber.Contains("|")
                            ? ((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber.Split('|')[0]
                            : ((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber;
                        var _isValid = Product.Lot.IsValid($"{_lot}|P|01", ModelSqlCon);
                        if (_isValid)
                        {
                            ((BindingList<Product.Lot>)sender)[e.NewIndex].Validated = _isValid;
                            LotChanging = true;
                            ((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber += ((BindingList<Product.Lot>)sender)[e.NewIndex].LotNumber.Contains("|")
                                ? ""
                                : "|P|01";
                        }
                        else
                        {
                            ((BindingList<Product.Lot>)sender)[e.NewIndex].Validated = _isValid;
                        }
                    }
                }
            }
            LotList.ListChanged += LotList_Changed;
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
            try
            {
                var _rows = MasterDataSet.Tables[typeof(Notice).Name].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]");
                if (_rows.Count() > 0)
                {
                    foreach (var _row in _rows)
                    {
                        _rtnList.Add($"{_row.SafeGetField<int>("NcrId")} {_row.SafeGetField<string>("SubTypeDescription")}");
                    }
                }
                return _rtnList;
            }
            catch
            {
                return _rtnList;
            }
        }

        /// <summary>
        /// Gets a list of NCR IDs that exist on a work order
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <returns>List of NCR ID's as strings</returns>
        public static BindingList<QmsForm> GetList(string orderId)
        {
            var _rtnList = new BindingList<QmsForm>();
            try
            {
                var _rows = MasterDataSet.Tables[typeof(Notice).Name].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]");
                if (_rows.Count() > 0)
                {
                    foreach (var _row in _rows)
                    {
                        var _qmsObj = new QmsForm(_row.SafeGetField<int>("NcrId"));
                        _rtnList.Add(_qmsObj);
                    }
                }
                return _rtnList;
            }
            catch
            {
                return _rtnList;
            }
        }

        /// <summary>
        /// Get NCR ID that exist on a work order
        /// </summary>
        /// <param name="orderId">Work Order ID</param>
        /// <returns>NCR IDs' as string</returns>
        public static string GetNcrId(string orderId)
        {
            var _rtnVal = string.Empty;
            foreach (var _row in MasterDataSet.Tables[typeof(Notice).Name].Select($"[WorkOrderId] = '{orderId}' AND [NcrRevisionId] = [RevisionFilter]"))
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
            return MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = '{ncrId}' AND [NcrRevisionId] = [RevisionFilter]").FirstOrDefault().SafeGetField<string>("DefectReason");
        }

        /// <summary>
        /// Checks to see if it is a valid NCR
        /// </summary>
        /// <param name="ncrId">Ncr ID</param>
        /// <returns>pass flag as bool</returns>
        public static bool IsValid(int ncrId)
        {
            return MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = '{ncrId}' AND [NcrRevisionId] = [RevisionFilter]").Count() > 0;
        }

        /// <summary>
        /// Checks to see if it is a valid NCR
        /// </summary>
        /// <param name="ncrId">Ncr ID</param>
        /// <param name="workOrder">Work Order ID</param>
        /// <param name="reference">Part Number or Lot Number</param>
        /// <param name="type">Changes the query type P for part number reference, L for lot number reference, C for component lot number reference</param>
        /// <returns>pass flag as bool</returns>
        public static bool IsValid(int ncrId, string workOrder, string reference, char type)
        {
            var _valid = false;
            switch (type)
            {
                case 'P':
                    reference = reference.Contains("|") ? reference : $"{reference}|01";
                    return MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = {ncrId} AND [PartId] = '{reference}' AND [WorkOrderId] = '{workOrder}'").Count() > 0;
                case 'L':
                    reference = reference.Contains("|") ? reference : $"{reference}|P|01";
                    _valid = MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = {ncrId} AND [WorkOrderId] = '{workOrder}'").Count() > 0;
                    if (_valid)
                    {
                        return ValidNcrLot(ncrId, reference, ModelSqlCon);
                    }
                    return false;
                case 'C':
                    reference = reference.Contains("|") ? reference : $"{reference}|P|01";
                    _valid = MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = {ncrId}").Count() > 0;
                    if (_valid)
                    {
                        return ValidNcrLot(ncrId, reference, ModelSqlCon);
                    }
                    return false;
                default: return false;
            }
        }

        /// <summary>
        /// Gets the last NCR ID in the database
        /// </summary>
        /// <returns>Last NCR ID as an int</returns>
        public static int GetLastNcrId()
        {
            return MasterDataSet.Tables[typeof(Notice).Name].Select().ToList().LastOrDefault().SafeGetField<int>("NcrId");
        }
    }
}
