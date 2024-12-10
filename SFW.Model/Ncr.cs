using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model
{
    public class Ncr : ModelBase
    {
        public class Disposition
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load an observable collection with all the NCR disposition information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR dispositions</returns>
            public static ObservableCollection<Disposition> GetDispositionCollection(SqlConnection sqlCon)
            {
                var _rtnDict = new ObservableCollection<Disposition>();
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"SELECT * FROM [dbo].[NCR-CSTM_Disposition]", sqlCon))
                        {
                            using (SqlDataReader _reader = cmd.ExecuteReader())
                            {
                                if (_reader.HasRows)
                                {
                                    while (_reader.Read())
                                    {
                                        _rtnDict.Add(new Disposition(_reader.GetFieldValue<int>(0), _reader.GetFieldValue<string>(1), _reader.GetFieldValue<string>(2)));
                                    }
                                }
                            }
                        }
                        return _rtnDict;
                    }
                    catch (SqlException)
                    {
                        return _rtnDict;
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
        }

        public class DefectType
        {
            #region Properties

            public int Id { get; set; }
            public string Description { get; set; }

            #endregion

            #region Data Access

            /// <summary>
            /// Load an observable collection with all the NCR defect type information
            /// </summary>
            /// <param name="sqlCon">Sql Connection to use</param>
            /// <returns>An ObservableCollection of NCR defect types</returns>
            public static ObservableCollection<DefectType> GetDefectTypeCollection(SqlConnection sqlCon)
            {
                var _rtnDict = new ObservableCollection<DefectType>();
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"SELECT * FROM [dbo].[NCR-CSTM_DefectType]", sqlCon))
                        {
                            using (SqlDataReader _reader = cmd.ExecuteReader())
                            {
                                if (_reader.HasRows)
                                {
                                    while (_reader.Read())
                                    {
                                        _rtnDict.Add(new DefectType(_reader.GetFieldValue<int>(0), _reader.GetFieldValue<string>(1)));
                                    }
                                }
                            }
                        }
                        return _rtnDict;
                    }
                    catch (SqlException)
                    {
                        return _rtnDict;
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

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public DefectType(int dType, string dDescrip)
            {
                Id = dType;
                Description = dDescrip;
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
            public static ObservableCollection<DefectReason> GetDefectReasonCollection(SqlConnection sqlCon)
            {
                var _rtnDict = new ObservableCollection<DefectReason>();
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"SELECT tmar.[Reason_Code] as 'DefectReasonId', tmar.[Reason_Description] as 'DefectReasonDescription' FROM [dbo].[TM-INIT_Adjust_Reasons] tmar WHERE tmar.[Reason_Code] LIKE 'Q%'", sqlCon))
                        {
                            using (SqlDataReader _reader = cmd.ExecuteReader())
                            {
                                if (_reader.HasRows)
                                {
                                    while (_reader.Read())
                                    {
                                        _rtnDict.Add(new DefectReason(_reader.GetFieldValue<string>(0), _reader.GetFieldValue<string>(1)));
                                    }
                                }
                            }
                        }
                        return _rtnDict;
                    }
                    catch (SqlException)
                    {
                        return _rtnDict;
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

            #endregion

            /// <summary>
            /// Default Constructor
            /// </summary>
            public DefectReason(string dReason, string dDescrip)
            {
                Id = dReason;
                Description = dDescrip;
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

        public class Revision
        {
            #region Properties

            public int RevisionId { get; set; }
            public CrewMember Submitter { get; set; }
            public DateTime SubmitDateTime { get; set; }
            public bool IsEscape { get; set; }
            public Machine OriginWorkCenter { get; set; }
            public DefectReason DefectReason { get; set; }
            public DefectType DefectType { get; set; }
            public int PotentialLoss { get; set; }
            public double PotentialValue { get; set; }
            public int ActualLoss { get; set; }
            public double ActualCost { get; set; }
            public Disposition Disposition { get; set; }
            public string Description { get; set; }

            #endregion

            /// <summary>
            /// NCR revisions default constructor
            /// </summary>
            public Revision()
            { }

            /// <summary>
            /// Ncr Overridden Constructor
            /// <param name="ncrId">NCR Id to load</param>
            /// </summary>
            public Revision(DataRow ncrDataRow, double prodVal)
            {
                RevisionId = ncrDataRow.Field<int>("NcrRevisionId");
                Submitter = new CrewMember(ncrDataRow.Field<int>("SubmitterId").ToString(), false);
                SubmitDateTime = ncrDataRow.Field<DateTime>("RevisionDateTime");
                IsEscape = ncrDataRow.Field<short>("IsEscape") == 1;
                OriginWorkCenter = new Machine(ncrDataRow.Field<int>("OriginWorkCenterId"));
                DefectReason = new DefectReason(ncrDataRow.Field<string>("DefectReason"), ncrDataRow.Field<string>("DefectReasonDescription"));
                DefectType = new DefectType(ncrDataRow.Field<int>("DefectType"), ncrDataRow.Field<string>("DefectTypeDescription"));
                PotentialLoss = ncrDataRow.Field<int>("PotentialLoss");
                PotentialValue = ncrDataRow.Field<int>("PotentialLoss") * prodVal;
                ActualLoss = int.TryParse(ncrDataRow.Field<decimal>("ActualLoss").ToString(), out int i) ? i : 0;
                ActualCost = double.TryParse(ncrDataRow.Field<decimal>("ActualCost").ToString(), out double d) ? d : 0.00;
                Disposition = new Disposition(ncrDataRow.Field<int>("DispositionId"), ncrDataRow.Field<string>("DispositionDescription"), ncrDataRow.Field<string>("LinkedStatus"));
                Description = ncrDataRow.Field<string>("Description");
            }
        }

        #region Properties

        public int NcrId { get; set; }
        public string OrderId { get; set; }
        public int OrderSeqId { get; set; }
        public Sku Part { get; set; }
        public IList<string> LotList { get; set; }
        public Machine FoundWorkCenter { get; set; }
        public CrewMember Reporter { get; set; }
        public double ProductValue { get; set; }
        public int Site { get; set; }
        public IList<Revision> RevisionList { get; set; }

        #endregion

        /// <summary>
        /// Ncr Default Constructor
        /// </summary>
        public Ncr()
        { }

        /// <summary>
        /// Ncr Overridden Constructor
        /// <param name="id">NCR Id to load</param>
        /// </summary>
        public Ncr(int id)
        {
            var ncrDataRows = MasterDataSet.Tables["NcrNotice"].Select($"[NcrId] = '{id}'", "[NcrRevisionId] DESC");
            NcrId = id;
            OrderId = ncrDataRows[0].Field<string>("WorkOrderId");
            OrderSeqId = ncrDataRows[0].Field<int>("WorkOrderSeqId");
            Part = new Sku(ncrDataRows[0].Field<string>("PartId"));
            LotList = GetNcrLotList(id, ModelSqlCon);
            FoundWorkCenter = new Machine(ncrDataRows[0].Field<int>("FoundWorkCenterId"));
            Reporter = new CrewMember(ncrDataRows[0].Field<int>("ReporterId").ToString(), false);
            ProductValue = double.TryParse(ncrDataRows[0].Field<decimal>("ProductValue").ToString(), out double d) ? d : 0.00;
            Site = ncrDataRows[0].Field<int>("Site");
            RevisionList = new List<Revision>();
            foreach (var ncr in ncrDataRows)
            {
                RevisionList.Add(new Revision(ncr, ProductValue));
            }
        }

        /// <summary>
        /// Ncr Overridden Constructor
        /// </summary>
        /// <param name="workOrder">WorkOrder object</param>
        public Ncr(WorkOrder workOrder)
        {
            OrderId = workOrder.OrderNumber;
            OrderSeqId = int.TryParse(workOrder.Seq, out int seq) ? seq : 0;
            var _machNumber = int.TryParse(Machine.GetMachineNumber(workOrder.Machine), out int mach) ? mach : 0;
            FoundWorkCenter = new Machine(_machNumber);
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM [dbo].[SFW_NcrNotice] ncr WHERE ncr.[Site] = @p1", sqlCon))
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
        public static IList<string> GetNcrLotList(int ncrId, SqlConnection sqlCon)
        {
            var _rtnList = new List<string>();
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
                                    _rtnList.Add(reader.SafeGetString("LotId"));
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

        #endregion

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
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM] ([WorkOrderId], [WorkOrderSeqId], [PartId], [FoundWorkCenterId], [ReporterId], [ProductValue], [Site])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7);
                                                        SELECT [NcrId] FROM [dbo].[NCR-CSTM] WHERE [NcrId] = @@IDENTITY;", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrObject.OrderId);
                    cmd.Parameters.AddWithValue("p2", ncrObject.OrderSeqId);
                    cmd.Parameters.AddWithValue("p3", ncrObject.Part.SkuNumber);
                    cmd.Parameters.AddWithValue("p4", ncrObject.FoundWorkCenter.MachineNumber);
                    cmd.Parameters.AddWithValue("p5", ncrObject.Reporter.IdNumber);
                    cmd.Parameters.AddWithValue("p6", ncrObject.ProductValue);
                    cmd.Parameters.AddWithValue("p7", ncrObject.Site);
                    _idNumber = Convert.ToInt32(cmd.ExecuteScalar());
                }
                ncrObject.RevisionList.Last().Submit(_idNumber, 1, sqlCon);
                return _idNumber;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Submit a NCR to the NCR Master DataBase
        /// </summary>
        /// <param name="ncrRev">NCR revision object</param>
        /// <param name="ncrId">NCR ID</param>
        /// <param name="ncrRevId">NCR Revision ID</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static void Submit(this Ncr.Revision ncrRev, int ncrId, int ncrRevId, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_Revisions] ([NcrId], [NcrRevisionId], [Submitter], [RevisionDateTime], [IsEscape], [OriginWorkCenterId], [DefectReason], [DefectType], [PotentialLoss], [Disposition], [Status], [Description])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12);", sqlCon))
                {
                    cmd.Parameters.AddWithValue("p1", ncrId);
                    cmd.Parameters.AddWithValue("p2", ncrRevId);
                    cmd.Parameters.AddWithValue("p3", ncrRev.Submitter.IdNumber);
                    cmd.Parameters.AddWithValue("p4", ncrRev.SubmitDateTime.ToString("yyyy-MM-dd HH:mm"));
                    cmd.Parameters.AddWithValue("p5", ncrRev.IsEscape ? 1 : 0);
                    cmd.Parameters.AddWithValue("p6", ncrRev.OriginWorkCenter.MachineNumber);
                    cmd.Parameters.AddWithValue("p7", ncrRev.DefectReason.Id);
                    cmd.Parameters.AddWithValue("p8", ncrRev.DefectType.Id);
                    cmd.Parameters.AddWithValue("p9", ncrRev.PotentialLoss);
                    cmd.Parameters.AddWithValue("p10", ncrRev.Disposition.Id);
                    cmd.Parameters.AddWithValue("p11", ncrRev.Disposition.Status);
                    cmd.Parameters.AddWithValue("p12", ncrRev.Description);
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// Submit a NCR to the NCR Master DataBase
        /// </summary>
        /// <param name="ncrObject">QIR Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>Last inserted NCR ID</returns>
        public static void SubmitLot(this Ncr ncrObject, SqlConnection sqlCon)
        {
            var _idNumber = 0;
            try
            {
                foreach (var lot in ncrObject.LotList)
                {
                    using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[NCR-CSTM_LotInfo] ([NcrId], [LotId]) Values(@p1, @p2)", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", ncrObject.NcrId);
                        cmd.Parameters.AddWithValue("p2", lot);
                        _idNumber = Convert.ToInt32(cmd.ExecuteNonQuery());
                    }
                    ncrObject.RevisionList.Last().Submit(_idNumber, 1, sqlCon);
                }
            }
            catch (Exception)
            {
                
            }
        }

    }
}
