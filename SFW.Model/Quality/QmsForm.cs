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
        /// <returns>A table of NCR Notice information</returns>
        public static BindingList<Product.Lot> GetNcrLotList(int ncrId, string uom, SqlConnection sqlCon)
        {
            var _rtnList = new BindingList<Product.Lot>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"SELECT
	ncrLot.[NcrId]
	,ncrLot.[LotId]
	,SUM(ncrLot.[Quantity]) as 'Scrap'
	,ncrlot.[ImportType]
FROM
	[dbo].[SFW_DefectLotLink] ncrLot
WHERE
	ncrLot.[NcrId] = @p1 AND ncrLot.[LotId] <> ''
GROUP BY
	ncrLot.[NcrId], ncrLot.[LotId], ncrLot.[ImportType]", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var _lotId = reader.SafeGetString("LotId");
                                    var _import = reader.SafeGetString("ImportType") == "M";
                                    if (_rtnList.Count(o => o.LotNumber == _lotId) > 0)
                                    {
                                        if (!_import)
                                        {
                                            if (_rtnList.Count(o => o.LotNumber == _lotId && o.Imported) > 0)
                                            {
                                                _rtnList.Remove(_rtnList.FirstOrDefault(o => o.LotNumber == _lotId && o.Imported));
                                                _rtnList.Add(new Product.Lot(_lotId, reader.SafeGetInt32("Scrap"), uom, _import, true));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _rtnList.Add(new Product.Lot(_lotId, reader.SafeGetInt32("Scrap"), uom, _import, true));
                                    }
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
                    var _lotString = "";
                    foreach (var _lotId in lotIdList)
                    {
                        var _lot = _lotId;
                        if (!_lot.Contains("|"))
                        {
                            _lot = $"{_lotId}|P|01";
                        }
                        _lotString = string.IsNullOrEmpty(_lotString) ? $"[LotId] = '{_lot}'" : $" OR [LotId] = '{_lot}'";
                    }
                    var _cmdString = $@"SELECT ncrLot.[NcrId] FROM [dbo].[SFW_DefectLotLink] ncrLot WHERE {_lotString}";
                    using (SqlCommand cmd = new SqlCommand(_cmdString, sqlCon))
                    {
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
            var _rtnList = new BindingList<Product.Lot>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"SELECT COUNT(ncrLot.[NcrId]) FROM [dbo].[SFW_DefectLotLink] ncrLot WHERE ncrLot.[NcrId] = @p1 AND ncrLot.[LotId] = @p2", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrId.ToString());
                        cmd.Parameters.AddWithValue("p2", lotId.ToString());
                        return int.TryParse(cmd.ExecuteScalar().ToString(), out int i) && i > 0;
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

        #endregion

        /// <summary>
        /// Ncr Default Constructor
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
        /// </summary>
        public QmsForm(int id)
        {
            var ncrDataRows = MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = '{id}'", "[NcrRevisionId] DESC");
            FormId = id;
            OrderId = ncrDataRows[0].Field<string>("WorkOrderId");
            OrderSeqId = ncrDataRows[0].Field<int>("WorkOrderSeqId").ToString();
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
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            PartCollection = int.TryParse(OrderSeqId, out int i) ? Product.Sku.GetCollection(OrderId, i) : Product.Sku.GetCollection(OrderId, 10);
                            OnPropertyChanged(nameof(PartCollection));
                            LotList = GetNcrLotList(id, Part.Uom, ModelSqlCon);
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
        public QmsForm(Production.WorkOrder workOrder, Management.Employee submitter, FormType frmType)
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
                        _rtnList.Add($"{_row.SafeGetField<int>("NcrId")} {_row.SafeGetField<string>("TypeDescription")}");
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
        /// <returns>pass flag as bool</returns>
        public static bool IsValid(int ncrId, string workOrder, string reference, char type)
        {
            switch (type)
            {
                case 'P':
                    reference = reference.Contains("|") ? reference : $"{reference}|01";
                    return MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = {ncrId} AND [PartId] = '{reference}' AND [WorkOrderId] = '{workOrder}'").Count() > 0;
                case 'L':
                    reference = reference.Contains("|") ? reference : $"{reference}|P|01";
                    var _valid = MasterDataSet.Tables[typeof(Notice).Name].Select($"[NcrId] = {ncrId} AND [WorkOrderId] = '{workOrder}'").Count() > 0;
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
