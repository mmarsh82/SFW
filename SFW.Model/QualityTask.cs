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
                    using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; 
                                                                SELECT
	                                                                a.[Qtask_Type],
	                                                                a.[Qtask_Init_Date],
	                                                                a.[Qtask_Initiated_By]
                                                                FROM
	                                                                [dbo].[IM_UDEF-INIT_Quality_Tasks] a
                                                                RIGHT JOIN
	                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = a.[ID1]
                                                                WHERE
	                                                                a.[ID1] = @p1 AND a.[Qtask_Initiated_By] IS NOT NULL AND a.[Qtask_Release_Date] IS NULL AND b.[Engineering_Status] != 'O';", sqlCon))
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
                                        IntialBy = reader.SafeGetString("Qtask_Initiated_By"),
                                        IntialDate = reader.SafeGetDateTime("Qtask_Init_Date").ToString("ddMMMyyyy")
                                    });
                                }
                            }
                        }
                    }
                    foreach (var q in _tempList)
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; 
                                                                SELECT
	                                                                a.[Qtask_Notes]
                                                                FROM
	                                                                [dbo].[IM_UDEF_Qtasks] a
                                                                RIGHT JOIN
	                                                                [dbo].[IPL-INIT] b ON b.[Part_Nbr] = a.[ID]
                                                                RIGHT JOIN
	                                                                [dbo].[IM_UDEF-INIT_Quality_Tasks] c ON c.[ID1] = a.[ID] AND c.[Qtask_Type] = a.[Qtask_Type]
                                                                WHERE
	                                                                a.[ID] = @p1 AND a.[Qtask_Type] = @p2 AND c.[Qtask_Initiated_By] IS NOT NULL AND c.[Qtask_Release_Date] IS NULL AND b.[Engineering_Status] != 'O';", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", partNumber);
                            cmd.Parameters.AddWithValue("p2", q.QTaskType);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        q.Notes += $"{reader.SafeGetString("Qtask_Notes").Trim()} ";
                                    }
                                }
                            }
                        }
                        q.Notes += "\n";
                        q.Notes.Trim();
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
