using IBMU2.UODOTNET;
using Microsoft.Win32;
using SFW.Model;
using System;
using System.IO;

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
        /// Location transfer in the current ERP system
        /// </summary>
        /// <param name="_skew">Current inventory skew object</param>
        /// <param name="from">Transfer From location</param>
        /// <param name="to">Transfer to location</param>
        /// <param name="qty">Quantity to transfer</param>
        /// <param name="nonLot">Is the item being relocated non-lot traceable</param>
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

        public static int ProductionWip(WorkOrder woObject, M2kConnection connection)
        {
            /*var uId = new Random();
            var suffix = uId.Next(128, 512);
            var btiText = new M2kWipADIArray(woObject).ToString();
            File.WriteAllLines(connection.BTIFolder, btiText.Split('\n'));
            return suffix;*/

            var btiText = new M2kWipADIArray(woObject).ToString();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "BtiTestDoc";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents |*.txt";
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllLines(dialog.FileName, btiText.Split('\n'));
            }
            return 0;
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
