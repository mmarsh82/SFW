using SFW.Model.Management;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SFW.Model.Production
{
    public partial class WorkOrderComment : ModelBase, IModuleData
    {
        #region Properties

        public int CommentId { get; set; }
        public string WpoId { get; set; }

        private string _comText;
        public string CommentText
        {
            get
            { return _comText; }
            set
            {
                _comText = value;
                OnPropertyChanged(nameof(CommentText));
            }
        }

        public DateTime SubmitDate { get; set; }

        public string SubmitterId { get; set; }
        public string SubmitterName { get; set; }

        private char _status;
        public char Status
        {
            get
            { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsHidden));
            }
        }
        public bool IsHidden { get { return Status == 'H'; } }

        #endregion

        #region Data Access

        /// <summary>
        /// Get work order note's table
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>All work order notes in a datatable</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($"USE {sqlCon.Database}; SELECT * FROM [dbo].[WPO-CSTM_GembaNotes]", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
                        }
                    }
                    catch (SqlException)
                    {
                        return new DataTable();
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
        /// Submit a comment to the SQL database
        /// </summary>
        /// <param name="woCom"></param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static int SubmitComment(WorkOrderComment woCom, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; INSERT INTO [dbo].[WPO-CSTM_GembaNotes] 
([WPO_ID], [CommentText], [CommentDate], [SubmitterID], [CommentStatus])
Values(@p1, @p2, @p3, @p4, @p5);
SELECT [ID] FROM [dbo].[WPO-CSTM_GembaNotes] WHERE [ID] = @@IDENTITY;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", woCom.WpoId);
                        cmd.Parameters.AddWithValue("p2", woCom.CommentText);
                        cmd.Parameters.AddWithValue("p3", woCom.SubmitDate);
                        cmd.Parameters.AddWithValue("p4", woCom.SubmitterId);
                        cmd.Parameters.AddWithValue("p5", woCom.Status);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (SqlException)
                {
                    return 0;
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

        /// <summary>
        /// Modify the status of a work order comment
        /// </summary>
        /// <param name="woCom">Comment Object</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public static char ModifyStatus(WorkOrderComment woCom, SqlConnection sqlCon)
        {
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand _cmd = new SqlCommand($"USE {sqlCon.Database}; UPDATE [dbo].[WPO-CSTM_GembaNotes] SET [CommentStatus] = @p1 WHERE [ID] = @p2", sqlCon))
                    {
                        woCom.Status = woCom.Status == 'A' ? 'H' : 'A';
                        _cmd.Parameters.AddWithValue("p1", woCom.Status);
                        _cmd.Parameters.AddWithValue("p2", woCom.CommentId);
                        _cmd.ExecuteNonQuery();
                        return woCom.Status;
                    }
                }
                catch (SqlException)
                {
                    return 'E';
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
        public WorkOrderComment()
        { }

        /// <summary>
        /// Get a work order's operator comments
        /// </summary>
        /// <param name="workOrder">Work order ID</param>
        /// <returns>A concatonation of comments into a list of objects</returns>
        public static ObservableCollection<WorkOrderComment> GetCollection(string workOrderId)
        {
            var _rtnCol = new ObservableCollection<WorkOrderComment>();

            var _rows = MasterDataSet.Tables[typeof(WorkOrderComment).Name].Select($"[WPO_ID] = '{workOrderId}'");
            foreach (var _row in _rows)
            {
                _rtnCol.Insert(0, new WorkOrderComment
                {
                    CommentId = _row.SafeGetField<int>("ID")
                    ,WpoId = _row.SafeGetField<string>("WPO_ID")
                    ,CommentText = _row.SafeGetField<string>("CommentText")
                    ,SubmitDate = _row.SafeGetField<DateTime>("CommentDate")
                    ,SubmitterId = _row.SafeGetField<string>("SubmitterId")
                    ,SubmitterName = Employee.GetDisplayName(_row.SafeGetField<string>("SubmitterId"))
                    ,Status = _row.SafeGetField<string>("CommentStatus").First()
                });
            }

            return _rtnCol;
        }
    }
}
