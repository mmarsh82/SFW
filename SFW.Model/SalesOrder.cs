﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

//Created by Michael Marsh 5-4-18

namespace SFW.Model
{
    public class SalesOrder : ModelBase
    {
        #region Properties

        public string SalesNumber { get; set; }
        public string PartNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPart { get; set; }
        public int LineNumber { get; set; }
        public int LineTotalNumber { get; set; }
        public int LineBaseQuantity { get; set; }
        public int LineBalQuantity { get; set; }
        public string LineDesc { get; set; }
        public string LineNotes { get; set; }
        public bool LoadPattern { get; set; }
        public string InternalComments { get; set; }
        public bool IsExpedited { get; set; }
        public string ShipName { get; set; }
        public string[] ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipState { get; set; }
        public string ShipZip { get; set; }
        public string ShipCountry { get; set; }
        public bool IsBackOrder { get { return LineBalQuantity < LineBaseQuantity; } }
        public DateTime CommitDate { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool IsLate { get { return CommitDate > DateTime.Today; } }
        public IList<SalesOrder> LineList { get; set; }

        #endregion

        /// <summary>
        /// SalesOrder Object Default Constructor
        /// </summary>
        public SalesOrder()
        { }

        /// <summary>
        /// SalesOrder Constructor
        /// Load a SalesOrder object based on a sales and line number
        /// </summary>
        /// <param name="soNbr">SalesOrder number to load, delimiter for line item is '*' i.e. SalesNumber*LineNumber</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public SalesOrder(string soNbr, SqlConnection sqlCon)
        {
            if (!string.IsNullOrEmpty(soNbr))
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    if (soNbr.Contains("*"))
                    {
                        var soNbrArray = soNbr.Split('*');
                        soNbr = $"{soNbrArray[0]}*{soNbrArray[1]}";
                        SalesNumber = soNbrArray[0];
                        LineNumber = Convert.ToInt32(soNbrArray[1]);
                    }
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database}; SELECT
	                                                                a.[Cust_Nbr], a.[Cust_Part_Nbr], a.[Ln_Bal_Qty],
	                                                                b.[Name] as 'Cust_Name'
                                                                FROM
	                                                                [dbo].[SOD-INIT] a
                                                                RIGHT JOIN
	                                                                [dbo].[CM-INIT] b ON b.[Cust_Nbr] = a.[Cust_Nbr]
                                                                WHERE
	                                                                a.[ID] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", soNbr);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        CustomerNumber = reader.SafeGetString("Cust_Nbr");
                                        CustomerName = reader.SafeGetString("Cust_Name");
                                        CustomerPart = reader.SafeGetString("Cust_Part_Nbr");
                                        LineBalQuantity = reader.SafeGetInt32("Ln_Bal_Qty");
                                        LineNotes = string.Empty;
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
        }

        /// <summary>
        /// Sales Order Object constructor
        /// Will create a new SalesOrder Object based on a DataRow from any DataTable Object
        /// </summary>
        /// <param name="dRow">DataRow with the item array values for the sales order</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        public SalesOrder(DataRow dRow, SqlConnection sqlCon)
        {
            SalesNumber = dRow.Field<string>("SoNbr");
            PartNumber = dRow.Field<string>("PartNbr");
            CustomerNumber = dRow.Field<string>("CustNbr");
            CustomerName = dRow.Field<string>("CustName");
            CustomerPart = dRow.Field<string>("CustPartNbr");
            LineNumber = dRow.Field<int>("LineNbr");
            LineTotalNumber = dRow.Field<int>("LineCount");
            LineBalQuantity = dRow.Field<int>("BalQty");
            LineBaseQuantity = dRow.Field<int>("BaseQty");
            LineDesc = dRow.Field<string>("Description");
            LoadPattern = dRow.Field<string>("LoadPattern").ToUpper() == "PLASTIC";
            GetInternalComments(sqlCon);
            IsExpedited = dRow.Field<int>("IsExpedited") > 0;
            ShipName = dRow.Field<string>("ShipName");
            ShipAddress = new string[2];
            ShipAddress[0] = dRow.Field<string>("ShipAddr1");
            ShipAddress[1] = dRow.Field<string>("ShipAddr2");
            ShipCity = dRow.Field<string>("ShipCity");
            ShipState = dRow.Field<string>("ShipState");
            ShipZip = dRow.Field<string>("ShipZip");
            ShipCountry = dRow.Field<string>("ShipCountry");
            CommitDate = dRow.Field<DateTime>("ShipDate");
            RequestDate = dRow.Field<DateTime>("ReqDate");
            DeliveryDate = dRow.Field<DateTime>("DelDate");
        }

        /// <summary>
        /// Get the sales order internal comments
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        public void GetInternalComments(SqlConnection sqlCon)
        {
            if (!string.IsNullOrEmpty(SalesNumber))
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    var _tempSales = SalesNumber.Contains("*") ? SalesNumber.Split('*')[0] : SalesNumber;
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand($@"USE {sqlCon.Database};
                                                                SELECT
                                                                    [Internal_Comments] as 'IntComm'
                                                                FROM
                                                                    [dbo].[SOH-INIT_Internal_Comments]
                                                                WHERE
                                                                    [So_Nbr] = @p1;", sqlCon))
                        {
                            cmd.Parameters.AddWithValue("p1", _tempSales);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.SafeGetString("IntComm").Contains("bag"))
                                        {
                                            InternalComments += $" {reader.SafeGetString("IntComm").Replace('"', ' ')}";
                                        }
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
        }

        /// <summary>
        /// Retrieve a DataTable with all the data relevent to a schedule
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>DataTable with the schedule data results</returns>
        public static DataTable GetScheduleData(SqlConnection sqlCon)
        {
            using (var _tempTable = new DataTable())
            {
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(@"SELECT
                                                                                b.[ID]
	                                                                            ,a.[So_Nbr] as 'SoNbr'
	                                                                            ,CAST(SUBSTRING(b.[ID], CHARINDEX('*', b.[ID], 0) + 1, LEN(b.[ID])) as int) as 'LineNbr'
	                                                                            ,(SELECT COUNT(ac.[ID]) FROM [dbo].[SOD-INIT] ac WHERE ac.[ID] LIKE CONCAT(a.[So_Nbr], '*%')) as 'LineCount'
	                                                                            ,b.[Part_Wo_Gl] as 'PartNbr'
                                                                                ,CAST(b.[Ln_Bal_Qty] as int) as 'BalQty'
	                                                                            ,CAST(b.[Ln_Del_Qty] as int) as 'BaseQty'
	                                                                            ,b.[Um_Base] as 'Uom'
	                                                                            ,ISNULL(b.[D_esc], (SELECT aa.[Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = b.[Part_Wo_Gl])) as 'Description'
                                                                                ,a.[Cust_Nbr] as 'CustNbr'
	                                                                            ,(SELECT ab.[Name] FROM [dbo].[CM-INIT] ab WHERE ab.[Cust_Nbr] = a.[Cust_Nbr]) as 'CustName'
                                                                                ,CONCAT((SELECT ab.[Name] FROM [dbo].[CM-INIT] ab WHERE ab.[Cust_Nbr] = a.[Cust_Nbr]),' (', a.[Cust_Nbr], ')') as 'FullCustName'
                                                                                ,b.[Cust_Part_Nbr] as 'CustPartNbr'
	                                                                            ,a.[Credit_Code] as 'CredStatus'
	                                                                            ,CASE WHEN a.[Ord_Type] = 'FSE' OR a.[Ord_Type] LIKE '%DE' THEN 'EOP' ELSE a.[Ord_Type] END as 'Type'
	                                                                            ,CAST(CASE WHEN a.[Jump_Reason] IS NULL THEN 0 ELSE 1 END as int) as 'IsExpedited'
	                                                                            ,a.[Ship_To_Name] as 'ShipName'
	                                                                            ,a.[Ship_To_Addr1] as 'ShipAddr1'
	                                                                            ,a.[Ship_To_Addr2] as 'ShipAddr2'
	                                                                            ,a.[Ship_To_City] as 'ShipCity'
	                                                                            ,a.[Ship_To_State] as 'ShipState'
	                                                                            ,a.[Ship_To_Zip] as 'ShipZip'
	                                                                            ,ISNULL(a.[Ship_To_Country], 'US') as 'ShipCountry'
	                                                                            ,CAST(a.[Date_Added] as date) as 'DateAdded'
	                                                                            ,CAST(a.[Delivery_Date] as date) as 'DelDate'
	                                                                            ,CAST(a.[Requested_Date] as date) as 'ReqDate'
	                                                                            ,CAST(a.[Commit_Ship_Date] as date) as 'ShipDate'
                                                                                ,(SELECT ISNULL(aa.[Load_Pattern], '') FROM [dbo].[CM-INIT] aa WHERE aa.[Cust_Nbr] = a.[Cust_Nbr]) AS 'LoadPattern'
                                                                                ,CASE WHEN b.[Make_To_Order] = 'Y' THEN 1 ELSE 0 END as 'MTO'
                                                                                ,CASE WHEN CAST(b.[Ln_Del_Qty] as int) - CAST(b.[Ln_Bal_Qty] as int) = 0 THEN 0 ELSE 1 END AS 'IsBackOrder'
                                                                            FROM
	                                                                            [dbo].[SOH-INIT] a
                                                                            RIGHT JOIN
	                                                                            [dbo].[SOD-INIT] b ON SUBSTRING(b.[ID], 0, CHARINDEX('*', b.[ID], 0)) = a.[So_Nbr]
                                                                            WHERE
	                                                                            a.[Order_Status] IS NULL AND b.[Comp] = 'O' AND (b.[Part_Wo_Gl] IS NOT NULL OR b.[D_esc] IS NOT NULL) AND a.[So_Nbr] IS NOT NULL
                                                                            ORDER BY
                                                                                b.[ID] ASC", sqlCon))
                        {
                            adapter.Fill(_tempTable);
                            return _tempTable;
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

        /// <summary>
        /// Get a list of the differnt types of sales orders
        /// </summary>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <returns>list of sales order types as IList<string></returns>
        public static IList<string> GetOrderTypeList(SqlConnection sqlCon)
        {
            var _rtnList = new List<string>();
            if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
            {
                try
                {
                    using (SqlCommand _cmd = new SqlCommand(@"SELECT
	                                                            DISTINCT(CASE WHEN a.[Ord_Type] = 'FSE' OR a.[Ord_Type] LIKE '%DE' THEN 'EOP' ELSE a.[Ord_Type] END) as 'Type'     
                                                            FROM
	                                                            [dbo].[SOH-INIT] a
                                                            WHERE
	                                                            a.[Order_Status] IS NULL", sqlCon))
                    {
                        using (SqlDataReader reader = _cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _rtnList.Add(reader.SafeGetString("Type"));
                            }
                        }
                    }
                    return _rtnList;
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

        /// <summary>
        /// Get a list of all the line items on a sales order
        /// </summary>
        /// <param name="soNbr">Sales order number to search</param>
        /// <param name="sqlCon">Sql Connection to use</param>
        /// <param name="lineNbr">Optional: Any line number not to include in the list</param>
        /// <returns>list of sales order line items as IList<string></returns>
        public static IList<SalesOrder> GetLineList(string soNbr, SqlConnection sqlCon, params int[] lineNbr)
        {
            if (!string.IsNullOrEmpty(soNbr))
            {
                var _condString = string.Empty;
                foreach (int i in lineNbr)
                {
                    _condString += i != 0 ? $" AND a.[ID] != CONCAT('{soNbr}', '*', '{i}')" : "";
                }
                var _rtnList = new List<SalesOrder>();
                if (sqlCon != null && sqlCon.State != ConnectionState.Closed && sqlCon.State != ConnectionState.Broken)
                {
                    try
                    {
                        using (SqlCommand _cmd = new SqlCommand(@"SELECT
	                                                                SUBSTRING(a.[ID], CHARINDEX('*', a.[ID],0) + 1, LEN(a.[ID])) as 'LineNbr'
	                                                                ,a.[Part_Wo_Gl] as 'PartNbr'
	                                                                ,(SELECT aa.[Description] FROM [dbo].[IM-INIT] aa WHERE aa.[Part_Number] = a.[Part_Wo_Gl]) as 'PartDesc'
	                                                                ,a.[Ln_Bal_Qty] as 'BalQty'
	                                                                ,a.[Ln_Del_Qty] as 'BaseQty'
	                                                                ,a.[Um_Base] as 'Uom'
                                                                FROM [dbo].[SOD-INIT] a
                                                                WHERE a.[ID] LIKE CONCAT(@p1, '*%')", sqlCon))
                        {
                            if (!string.IsNullOrEmpty(_condString))
                            {
                                _cmd.CommandText += _condString;
                            }
                            _cmd.CommandText += " ORDER BY a.[ID] ASC";
                            _cmd.Parameters.AddWithValue("p1", soNbr);
                            using (SqlDataReader reader = _cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _rtnList.Add(new SalesOrder
                                    {
                                        LineBalQuantity = reader.SafeGetInt32("BalQty")
                                        ,LineDesc = reader.SafeGetString("PartDesc")
                                        ,LineNumber = reader.SafeGetInt32("LineNbr")
                                        ,PartNumber = reader.SafeGetString("PartNbr")
                                        ,LineBaseQuantity = reader.SafeGetInt32("BaseQty")
                                        ,LineNotes = reader.SafeGetString("Uom")
                                    });
                                }
                            }
                        }
                        return _rtnList;
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
            return null;
        }
    }
}
