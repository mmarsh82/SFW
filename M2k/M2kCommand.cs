using IBMU2.UODOTNET;
using M2kClient.M2kADIArray;
using Microsoft.Win32;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace M2kClient
{
    public class M2kCommand
    { 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public M2kCommand()
        { }

        /// <summary>
        /// Will edit any record in Manage 2000
        /// ***WARNING***
        /// DO NOT attempt to edit any record value that has business logic associated with it
        /// Contact your ERP administrator if you need help verifing
        /// </summary>
        /// <param name="file">Manage 2000 file to be edited</param>
        /// <param name="recordID">Record ID value to be edited</param>
        /// <param name="attribute">The attribute number that the new value will be associated with, see the warning</param>
        /// <param name="newValue">New value to be written into the record</param>
        /// <param name="connection">UniConnection to use for the edit</param>
        /// <returns>Change request error, if none exists then it will return a null value</returns>
        public static string EditRecord(string file, string recordID, int attribute, string newValue, M2kConnection connection)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(connection.HostName, connection.UserName, connection.Password, connection.UniAccount, connection.UniService))
                {
                    try
                    {
                        using (UniFile uFile = uSession.CreateUniFile(file))
                        {
                            using (UniDynArray udArray = uFile.Read(recordID))
                            { 
                                udArray.Replace(attribute, newValue);
                                uFile.Write(recordID, udArray);
                            }
                        }
                        UniObjects.CloseSession(uSession);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        UniObjects.CloseSession(uSession);
                        return ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        ///<summary>
        /// Will edit any Multi-Value record in Manage 2000
        /// ***WARNING***
        /// DO NOT attempt to edit any record value that has business logic associated with it
        /// Contact your ERP administrator if you need help verifing
        /// </summary>
        /// <param name="file">Manage 2000 file to be edited</param>
        /// <param name="recordID">Record ID value to be edited</param>
        /// <param name="attribute">The attribute number that the new value will be associated with, see the warning</param>
        /// <param name="newValue">Arry of new values to be written into the record</param>
        /// <param name="connection">UniConnection to use for the edit</param>
        /// <returns>All errors are passed back as a string, otherwise it will return a null value</returns>
        public static string EditMVRecord(string file, string recordID, int attribute, string[] newValue, M2kConnection connection)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(connection.HostName, connection.UserName, connection.Password, connection.UniAccount, connection.UniService))
                {
                    try
                    {
                        using (UniFile uFile = uSession.CreateUniFile(file))
                        {
                            using (UniDynArray udArray = uFile.Read(recordID))
                            {
                                var _udCount = udArray.Dcount(attribute);
                                var counter = 0;
                                foreach (var s in newValue)
                                {
                                    udArray.Replace(attribute, counter, s);
                                    counter++;
                                }
                                if (_udCount >= counter)
                                {
                                    udArray.Replace(attribute, counter, "");
                                    counter++;
                                }
                                uFile.Write(recordID, udArray);
                            }
                        }
                        UniObjects.CloseSession(uSession);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        UniObjects.CloseSession(uSession);
                        return ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Delete records in manage
        /// !!WARNING!! DO NOT USE this method to modify business logic transactions like wip reciepts
        /// The intent of this method is to delete single records that are stand alone in the M2k database i.e. N location reason
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="recordID">Record ID</param>
        /// <param name="attribute">Name of the attribute to modidy</param>
        public static void RemoveRecord(string file, string recordID, int attribute, M2kConnection connection)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(connection.HostName, connection.UserName, connection.Password, connection.UniAccount, connection.UniService))
                {
                    try
                    {
                        using (UniFile uFile = uSession.CreateUniFile(file))
                        {
                            using (UniDynArray udArray = uFile.Read(recordID))
                            {
                                udArray.Remove(attribute);
                                uFile.Write(recordID, udArray);
                                UniObjects.CloseSession(uSession);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        UniObjects.CloseSession(uSession);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Get the next available lot number in M2k
        /// </summary>
        /// <param name="connection">Your Manage 2000 Connection object</param>
        /// <returns>
        /// IReadOnlyDictionary file
        /// Key is the Pass/Fail check
        /// On Pass the value will be the lot number created
        /// On Fail the value will be the error message
        /// </returns>
        public static IReadOnlyDictionary<bool, string> GetLotNumber(M2kConnection connection)
        {
            try
            {
                var _subResult = new Dictionary<bool, string>();
                using (UniSession uSession = UniObjects.OpenSession(connection.HostName, connection.UserName, connection.Password, connection.UniAccount, connection.UniService))
                {
                    try
                    {
                        using (UniSubroutine uSubRout = uSession.CreateUniSubroutine("SUB.GET.NEXTLOT", 1))
                        {
                            uSubRout.SetArg(0, "");
                            uSubRout.Call();
                            _subResult.Add(true, uSubRout.GetArg(0));
                            UniObjects.CloseSession(uSession);
                            return _subResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        UniObjects.CloseSession(uSession);
                        _subResult.Add(false, ex.Message);
                        return _subResult;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Location transfer in the current ERP system
        /// </summary>
        /// <param name="_skew">Current inventory skew object</param>
        /// <param name="from">Transfer From location</param>
        /// <param name="to">Transfer to location</param>
        /// <param name="qty">Quantity to transfer</param>
        /// <param name="nonLot">Is the item being relocated non-lot traceable</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <returns>Suffix for the file that needs to be watched on the ERP server</returns>
        public static int InventoryMove(string from, string to, int qty, bool nonLot, M2kConnection connection)
        {
            /*var uId = new Random();
            var suffix = uId.Next(128, 512);
            from = _skew.OnHand.Count > 1 || nonLot ? from.ToUpper() : _skew.OnHand.First().Key.ToUpper();
            if (!nonLot)
            {
                //String Format for non lot tracable = false
                //1~Transaction type~2~Station ID~3~Transaction time~4~Transaction date~5~Facility code~6~Partnumber~7~From location~8~To location~9~Quantity #1~10~Lot #1~9~Quantity #2~10~Lot #2~~99~COMPLETE
                //Must meet this format in order to work with M2k

                var moveText = $"1~LOCXFER~2~{CurrentUser.DomainName}~3~{DateTime.Now.ToString("HH:mm")}~4~{DateTime.Today.ToString("MM-dd-yyyy")}~5~01~6~{_skew.PartNumber}~7~{from.ToUpper()}~8~{to.ToUpper()}~9~{qty}~10~{_skew.LotNumber.ToUpper()}|P~99~COMPLETE";
                File.WriteAllText($"{connection.BTIFolder}LOCXFERC2K.DAT{suffix}", moveText);
            }
            else
            {
                //String Format for non lot tracable = true
                //1~Transaction type~2~Station ID~3~Transaction time~4~Transaction date~5~Facility code~6~Partnumber~7~From location~8~To location~9~Quantity~12~UoM~99~COMPLETE
                //Must meet this format in order to work with M2k

                var moveText = $"1~LOCXFER~2~{CurrentUser.DomainName}~3~{DateTime.Now.ToString("HH:mm")}~4~{DateTime.Today.ToString("MM-dd-yyyy")}~5~01~6~{_skew.PartNumber}~7~{from.ToUpper()}~8~{to.ToUpper()}~9~{qty}~12~{_skew.UOM.ToUpper()}~99~COMPLETE";
                File.WriteAllText($"{connection.BTIFolder}LOCXFERC2K.DAT{suffix}", moveText);
            }
            return suffix;*/
            return 10;
        }

        /// <summary>
        /// Production Wip in the current ERP system
        /// </summary>
        /// <param name="wipRecord">Wip Record object to be processed</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <returns>Suffix for the file that needs to be watched on the ERP server and new lot number if required</returns>
        public static IReadOnlyDictionary<int, string> ProductionWip(WipReceipt wipRecord, M2kConnection connection)
        {
            //TODO Remove the hardcoded safefiledialog and automate this process
            //will also need to add in the issue commands, would be best to code in the issue pieces as an adjacent method

            /*var uId = new Random();
            var suffix = uId.Next(128, 512);
            var wipBtiText = new Wip(wipRecord).ToString();
            File.WriteAllLines(connection.BTIFolder, btiText.Split('\n'));
            return suffix;*/

            var _subResult = new Dictionary<int, string>();
            var btiText = string.Empty;
            if (string.IsNullOrEmpty(wipRecord.WipLot.LotNumber))
            {
                var _response = GetLotNumber(new M2kConnection("manage", "omniquery", "omniquery", Database.WCCOTRAIN));
                if (_response.Count == 1 && _response.First().Key)
                {
                    wipRecord.WipLot.LotNumber = _response.First().Value.Replace("|P", "");
                    System.Windows.MessageBox.Show($"Assinged to Lot Number:\n{wipRecord.WipLot.LotNumber}", "New Lot Number", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (_response.Count == 1)
                {
                    System.Windows.MessageBox.Show(_response.First().Value, "Connection Error");
                    _subResult.Add(0, string.Empty);
                    return _subResult;
                }
                else
                {
                    System.Windows.MessageBox.Show("Please contact IT for further assistance", "NEXT.NBR corruption");
                    _subResult.Add(0, string.Empty);
                    return _subResult;
                }
            }
            foreach (var c in wipRecord.WipWorkOrder.Bom)
            {
                var _issue = new Issue(wipRecord.Submitter, "010", c.CompNumber, wipRecord.WipWorkOrder.OrderNumber, wipRecord.WipWorkOrder.Seq, "II", new List<Transaction>());
                foreach (var w in c.WipInfo)
                {
                    if (c.IsLotTrace && !string.IsNullOrEmpty(w.LotNbr) && w.LotQty != null)
                    {
                        if (string.IsNullOrEmpty(c.BackflushLoc))
                        {
                            _issue.TranList.Add(new Transaction { Quantity = Convert.ToInt32(w.LotQty), Location = w.RcptLoc, LotNumber = w.LotNbr });
                        }
                        else
                        {
                            _issue.TranList.Add(new Transaction { Quantity = Convert.ToInt32(w.LotQty), Location = c.BackflushLoc, LotNumber = w.LotNbr });
                        }
                    }
                    else if (w.LotQty != null && w.LotQty > 0)
                    {
                        _issue.TranList.Add(new Transaction { Quantity = Convert.ToInt32(Math.Round(Convert.ToDouble(wipRecord.WipQty) * c.AssemblyQty)), Location = c.BackflushLoc });
                    }
                }

                btiText = _issue.ToString();

                //Hardcoded for testing
                SaveFileDialog dialog = new SaveFileDialog
                {
                    FileName = "BtiTestDoc",
                    DefaultExt = ".txt",
                    Filter = "Text Documents |*.txt"
                };
                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllLines(dialog.FileName, btiText.Split('\n'));
                }
            }
            _subResult.Add(1, wipRecord.WipLot.LotNumber);
            return _subResult;
        }


        public bool InventoryAdjustment()
        {
            return true;
        }
        public bool ProductionIssue()
        {
            return true;
        }
        public string LaborTransaction()
        { return string.Empty; }
    }
}
