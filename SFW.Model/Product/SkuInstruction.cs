using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SFW.Model.Product
{
    public class SkuInstruction : ModelBase, IModuleData
    {
        #region Data Access

        /// <summary>
        /// Get a Table of all work instructions in the database
        /// </summary>
        /// <param name="site">Facility to load</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>A table of URL strings to open the work instructions</returns>
        public DataTable GetTable(int site, SqlConnection sqlCon)
        {
            using (DataTable _dt = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {sqlCon.Database}; SELECT * FROM [dbo].[SFW_WorkInstructions]", sqlCon))
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
        public SkuInstruction()
        { }

        /// <summary>
        /// Get a Sku's work instructions
        /// </summary>
        /// <param name="partNbr">Sku ID Number</param>
        /// <param name="siteNbr">Facility number to run on</param>
        /// <param name="filepath">Path to the file</param>
        /// <returns>A list of URL strings to open the work instructions</returns>
        public static List<string> GetList(string partNbr, int siteNbr, string filepath)
        {
            var _inst = new List<string>();
            try
            {
                if (siteNbr == 2)
                {
                    if (File.Exists($"{filepath}{partNbr}.pdf"))
                    {
                        _inst.Add(partNbr);
                    }
                }
                else
                {
                    var _rows = MasterDataSet.Tables[typeof(SkuInstruction).Name].Select($"[SkuID] = '{partNbr}'");
                    foreach (var _row in _rows)
                    {
                        var dir = new DirectoryInfo(filepath);
                        var fileList = dir.GetFiles($"*{_row.Field<int>("WI")} *");
                        foreach (var file in fileList)
                        {
                            _inst.Add(file.Name);
                        }
                    }
                }
                return _inst;
            }
            catch
            {
                return _inst;
            }
        }
    }
}
