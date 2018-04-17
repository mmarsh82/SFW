using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace SFW.Model
{
    public sealed class WorkCenter : ModelBase
    {
        #region Properties

        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Work Center Constructor
        /// </summary>
        public WorkCenter()
        {

        }

        /// <summary>
        /// Get a list of work centers
        /// </summary>
        /// <returns>generic list of worcenter objects</returns>
        public static List<WorkCenter> GetWorkCenterList()
        {
            var _tempList = new List<WorkCenter>();
            if (SqlModelCon != null)
            {
                try
                {
                    SqlModelCon.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT [Wc_Nbr], [Name], [D_esc] FROM [dbo].[WC-INIT]", SqlModelCon))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    _tempList.Add(new WorkCenter { ID = reader.GetString(0), Name = reader.GetString(1), Description = reader.GetString(2) });
                                }
                            }
                        }
                    }
                    SqlModelCon.Close();
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
                finally
                {
                    if (SqlModelCon.State == ConnectionState.Open || SqlModelCon.State == ConnectionState.Connecting)
                    {
                        SqlModelCon.Close();
                    }
                }
            }
            else
            {
                throw new Exception("A connection could not be made to pull accurate data, please contact your administrator");
            }
        }
    }
}
