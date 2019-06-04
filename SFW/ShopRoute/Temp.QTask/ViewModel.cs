using SFW.Model;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SFW.ShopRoute.Temp.QTask
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public string QTask { get; set; }

        private string qType;
        public string QType
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
                OnPropertyChanged(nameof(QType));
            }
        }
        public string Notes { get; set; }
        public string IntialBy { get; set; }
        public string IntialDate { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partNbr"></param>
        public ViewModel(string partNbr)
        {
            if (App.AppSqlCon != null && App.AppSqlCon.State != ConnectionState.Closed && App.AppSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.AppSqlCon.Database}; SELECT * FROM [dbo].[IM_UDEF-INIT_Quality_Tasks] WHERE [ID1] = @p1;", App.AppSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    IntialDate = reader.SafeGetDateTime("Qtask_Init_Date").ToString("ddMMMyyyy");
                                    IntialBy = reader.SafeGetString("Qtask_Initiated_By");
                                    Notes = reader.SafeGetString("Qtask_Notes");
                                    QTask = reader.SafeGetString("Qtask_Type").Trim();
                                    QType = reader.SafeGetString("Qtask_Type").Trim();
                                }
                            }
                        }
                    }
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

        public static bool HasQTask(string partNbr)
        {
            if (App.AppSqlCon != null && App.AppSqlCon.State != ConnectionState.Closed && App.AppSqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand($@"USE {App.AppSqlCon.Database}; SELECT COUNT(ID1) FROM [dbo].[IM_UDEF-INIT_Quality_Tasks] WHERE [ID1] = @p1;", App.AppSqlCon))
                    {
                        cmd.Parameters.AddWithValue("p1", partNbr);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
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
