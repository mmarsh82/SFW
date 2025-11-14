using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SFW.Model.Quality
{
    public static class Extensions
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
        public static void Submit(this Revision ncrRev, int ncrId, int ncrRevId, SqlConnection sqlCon)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($@"INSERT INTO [dbo].[DEFECT-CSTM_Revisions] ([NcrId], [NcrRevisionId], [SubmitterId], [RevisionDateTime], [ReasonId], [SubTypeId], [TypeId], [DispositionId], [SupplierId], [Description], [EstimatedLoss])
                                                        Values(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11);", sqlCon))
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
                    cmd.Parameters.AddWithValue("p11", ncrRev.EstimatedLoss);
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
                var _oldLotList = QmsForm.GetLotList(ncrObj.FormId, ncrObj.Part.Uom, sqlCon);
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
