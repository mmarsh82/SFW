using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SFW.Model
{
    public class QualityTask : ModelBase
    {
        #region Properties

        public string QTaskType { get; set; }

        private string qType;
        public string QTypeDesc
        {
            get { return qType; }
            set
            {
                switch (value)
                {
                    case "Q1":
                        value = "PPAP";
                        break;
                    case "Q2":
                        value = "ISIR";
                        break;
                    case "Q3":
                        value = "Quality or Engineering Involvement";
                        break;
                    case "Q4":
                        value = "Tooling Validation";
                        break;
                    case "Q5":
                        value = "Special Need";
                        break;
                    case "Q6":
                        value = "Documentation";
                        break;
                    default:
                        value = "";
                        break;
                }
                qType = value;
            }
        }
        public string Notes { get; set; }
        public string IntialBy { get; set; }
        public string IntialDate { get; set; }

        #endregion

        public QualityTask()
        { }

        public static List<QualityTask> GetQTaskList(string partNumber, SqlConnection sqlCon)
        {
            var _tempList = new List<QualityTask>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[IM_UDEF-INIT_Quality_Tasks] WHERE [ID1] = @p1;", sqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNumber);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new QualityTask
                                    {
                                        QTaskType = reader.SafeGetString("Qtask_Type").Trim(),
                                        QTypeDesc = reader.SafeGetString("Qtask_Type").Trim(),
                                        Notes = reader.SafeGetString("Qtask_Notes"),
                                        IntialBy = reader.SafeGetString("Qtask_Initiated_By"),
                                        IntialDate = reader.SafeGetDateTime("Qtask_Init_Date").ToString("ddMMMyyyy")
                                    });
                                }
                            }
                        }
                    }
                    return _tempList;
                }
                catch (SqlException sqlEx)
                {
                    throw sqlEx;
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
}
