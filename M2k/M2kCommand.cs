using IBMU2.UODOTNET;
using M2kClient.M2kADIArray;
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
        /// Will edit any attribute in a Manage 2000 record
        /// ***WARNING***
        /// DO NOT attempt to edit any record value that has business logic associated with it
        /// Contact your ERP administrator if you need help verifing
        /// </summary>
        /// <param name="file">Manage 2000 file to be edited</param>
        /// <param name="recordID">Record ID value to be edited</param>
        /// <param name="attribute">The attribute number that the new value will be associated with, see the warning</param>
        /// <param name="newValue">New value to be written into the record</param>
        /// <param name="arrayCommand">UniDynArray Command to execute on the record</param>
        /// <param name="connection">UniConnection to use for the edit</param>
        /// <returns>Change request error, if none exists then it will return a null value</returns>
        public static string EditRecord(string file, string recordID, int attribute, string newValue, UdArrayCommand arrayCommand, M2kConnection connection)
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
                                switch(arrayCommand)
                                {
                                    case UdArrayCommand.Insert:
                                        udArray.Insert(attribute, newValue);
                                        break;
                                    case UdArrayCommand.Replace:
                                        udArray.Replace(attribute, newValue);
                                        break;
                                    case UdArrayCommand.Remove:
                                        udArray.Remove(attribute);
                                        break;
                                }
                                uFile.Write(recordID, udArray);
                            }
                        }
                        UniObjects.CloseSession(uSession);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (uSession != null)
                        {
                            UniObjects.CloseSession(uSession);
                        }
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
        /// Will edit multiple attributes in a Manage 2000 record
        /// ***WARNING***
        /// DO NOT attempt to edit any record value that has business logic associated with it
        /// Contact your ERP administrator if you need help verifing
        /// </summary>
        /// <param name="file">Manage 2000 file to be edited</param>
        /// <param name="recordID">Record ID value to be edited</param>
        /// <param name="attributes">Array of attribute numbers that the new value will be associated with, see the warning</param>
        /// <param name="newValues">Array of New values to be written into the record</param>
        /// <param name="arrayCommand">UniDynArray Command to execute on the record</param>
        /// <param name="connection">UniConnection to use for the edit</param>
        /// <returns>Change request error, if none exists then it will return a null value</returns>
        public static string EditRecord(string file, string recordID, int[] attributes, string[] newValues, UdArrayCommand arrayCommand, M2kConnection connection)
        {
            if (attributes.Length == newValues.Length || arrayCommand == UdArrayCommand.Remove)
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
                                    foreach (var attr in attributes)
                                    {
                                        var attrIndx = Array.IndexOf(attributes, attr);
                                        switch (arrayCommand)
                                        {
                                            case UdArrayCommand.Insert:
                                                udArray.Insert(attr, newValues[attrIndx]);
                                                break;
                                            case UdArrayCommand.Replace:
                                                udArray.Replace(attr, newValues[attrIndx]);
                                                break;
                                            case UdArrayCommand.Remove:
                                                udArray.Remove(attr);
                                                break;
                                        }
                                    }
                                    uFile.Write(recordID, udArray);
                                }
                            }
                            UniObjects.CloseSession(uSession);
                            return null;
                        }
                        catch (Exception ex)
                        {
                            if (uSession != null)
                            {
                                UniObjects.CloseSession(uSession);
                            }
                            return ex.Message;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return "There is either more attributes to edit than new edit values or more edit values have been included that attributes to edit.";
        }

        ///<summary>
        /// Will edit any Multi-Value attribute in a Manage 2000 record
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
                                var counter = 1;
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
        /// Inventory move in the current ERP
        /// </summary>
        /// <param name="stationId">Submitter name</param>
        /// <param name="partNbr">Part number to move</param>
        /// <param name="lotNbr">Lot number to move, leave blank if one does not exist, unit of measure is required with no lot</param>
        /// <param name="uom">Unit of measure, only required on a non-lot transaction</param>
        /// <param name="from">Location the material will be moving from</param>
        /// <param name="to">Location the material will be moving to</param>
        /// <param name="qty">Amount of material to move</param>
        /// <param name="reference">Move reference as open text</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <param name="nonConf">Optional: Non-conformance reason associated with the part being moved</param>
        /// <returns>Error number and description, when returned error number is 0 the suffix will be in the description</returns>
        public static IReadOnlyDictionary<int, string> InventoryMove(string stationId, string partNbr, string lotNbr, string uom, string from, string to, int qty, string reference, M2kConnection connection, string nonConf = "")
        {
            var _subResult = new Dictionary<int, string>();
            try
            {
                //Move the product
                var suffix = DateTime.Now.ToString($"HHmmssffff");
                var _move = new Locxfer(stationId, partNbr.ToUpper(), from.ToUpper(), to.ToUpper(), qty, lotNbr, reference, uom.ToUpper());
                File.WriteAllText($"{connection.BTIFolder}LOCXFER{connection.AdiServer}.DAT{suffix}", _move.ToString());
                //Check to see if you need to clear or add a non-conformance reason

                if (!to.EndsWith("N") && !string.IsNullOrEmpty(nonConf))
                {
                    //Remove note
                    EditRecord("LOT.MASTER", $"{lotNbr}|P", 42, "", UdArrayCommand.Remove, connection);
                }
                else if (to.EndsWith("N"))
                {
                    //Add note
                    EditRecord("LOT.MASTER", $"{lotNbr}|P", 42, nonConf, UdArrayCommand.Replace, connection);
                }
                _subResult.Add(0, string.Empty);
                return _subResult;
            }
            catch (Exception ex)
            {
                _subResult.Add(1, ex.Message);
                return _subResult;
            }
        }

        /// <summary>
        /// Production Wip in the current ERP system
        /// </summary>
        /// <param name="wipRecord">Wip Record object to be processed</param>
        /// <param name="postLabor">Tells the method if it should also post labor with the wip transaction</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// /// <param name="isLot">Tells the method if the current wip transaction is for a lot tracable part or non lot tracable</param>
        /// <param name="machID">Optional: Machine ID, passed when labor needs to posted.  It is also required for posting labor</param>
        /// <returns>Error number and error description, when returned as 0 and a empty string the transaction posted with errors</returns>
        public static IReadOnlyDictionary<int, string> ProductionWip(WipReceipt wipRecord, bool postLabor, M2kConnection connection, bool isLot, string machID = "")
        {
            var _subResult = new Dictionary<int, string>();
            var tranCount = 0;
            var suffix = DateTime.Now.ToString($"ssffff");
            var _tWip = new Wip();
            var _lotList = new List<string>();

            #region Wip Process

            if (!wipRecord.IsMulti)
            {
                var _lotEntered = !string.IsNullOrEmpty(wipRecord.WipLot.LotNumber) || !isLot;
                if (string.IsNullOrEmpty(wipRecord.WipLot.LotNumber) && isLot)
                {
                    var _response = GetLotNumber(connection);
                    if (_response.Count == 1 && !_response.First().Key)
                    {
                        System.Windows.MessageBox.Show(_response.First().Value, "Connection Error");
                        _subResult.Add(0, string.Empty);
                        return _subResult;
                    }
                    else if (_response.Count == 0)
                    {
                        System.Windows.MessageBox.Show("Please contact IT for further assistance", "NEXT.NBR corruption");
                        _subResult.Add(0, string.Empty);
                        return _subResult;
                    }
                    wipRecord.WipLot.LotNumber = _response.First().Value.Replace("|P", "");
                }
                //File creation for the WIP ADI, needs to account for all database scenarios (i.e. one to one, one to many, and many to many)
                _tWip = new Wip(wipRecord);
                if (!string.IsNullOrEmpty(_tWip.StationId))
                {
                    File.WriteAllText($"{connection.SFDCFolder}WP{connection.AdiServer}.DAT{suffix}w{tranCount}", _tWip.ToString());
                    tranCount++;
                    if (_lotEntered)
                    {
                        var _msgString = !string.IsNullOrEmpty(wipRecord.WipLot.LotNumber)
                            ? $"Your transaction of {wipRecord.WipWorkOrder.SkuNumber} using {wipRecord.WipLot.LotNumber} has been submitted."
                            : $"Your transaction of {wipRecord.WipWorkOrder.SkuNumber} has been submitted.";
                        System.Windows.MessageBox.Show(_msgString, "Transaction Accepted", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else if (!string.IsNullOrEmpty(wipRecord.WipLot.LotNumber))
                    {
                        System.Windows.MessageBox.Show($"Assigned to Lot Number:\n{wipRecord.WipLot.LotNumber}", "New Lot Number", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Unable to process WIP file, please check to see that the wip is with in standards.\nPlease contact IT for further assistance.", "Abnormal WIP file");
                    _subResult.Add(0, string.Empty);
                    return _subResult;
                }
            }

            #endregion

            #region Multi Wip Process

            else
            {
                var _tComp = new List<CompInfo>();
                //Break out the BOM so that it only has a single instance of the multiple parent
                foreach (var c in wipRecord.WipWorkOrder.Picklist.Where(o => o.IsLotTrace))
                {
                    foreach (var w in c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)))
                    {
                        var _backFlush = c.BackflushLoc;
                        _tComp = new List<CompInfo>
                        {
                            new CompInfo
                            {
                                Lot = w.LotNbr,
                                PartNbr = w.PartNbr,
                                Quantity = Convert.ToInt32(w.LotQty / wipRecord.RollQty),
                                WorkOrderNbr = wipRecord.WipWorkOrder.OrderNumber,
                                IssueLoc = !string.IsNullOrEmpty(_backFlush) ? _backFlush : w.RcptLoc
                            }
                        };
                    }
                }
                var _counter = 1;
                while (_counter <= wipRecord.RollQty)
                {
                    var _response = GetLotNumber(connection);
                    if (_response.Count == 1 && !_response.First().Key)
                    {
                        System.Windows.MessageBox.Show(_response.First().Value, "Connection Error");
                        _subResult.Add(0, string.Empty);
                        return _subResult;
                    }
                    else if (_response.Count == 0)
                    {
                        System.Windows.MessageBox.Show("Please contact IT for further assistance", "NEXT.NBR corruption");
                        _subResult.Add(0, string.Empty);
                        return _subResult;
                    }
                    wipRecord.WipLot.LotNumber = _response.First().Value.Replace("|P", "");
                    _lotList.Add(_response.First().Value.Replace("|P", ""));
                    //File creation for the WIP ADI, needs to account for all database scenarios (i.e. one to one, one to many, and many to many)
                    _tWip = new Wip(wipRecord) { QtyReceived = Convert.ToInt32(wipRecord.WipQty), ComponentInfoList = _tComp };
                    if (!string.IsNullOrEmpty(_tWip.StationId))
                    {
                        File.WriteAllText($"{connection.SFDCFolder}WP{connection.AdiServer}.DAT{suffix}w{tranCount}", _tWip.ToString());
                        tranCount++;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Unable to process WIP file, please check to see that the wip is with in standards.\nPlease contact IT for further assistance.", "Abnormal WIP file");
                        _subResult.Add(0, string.Empty);
                        return _subResult;
                    }
                    _counter++;
                }
                System.Windows.MessageBox.Show($"Multiple transaction of {wipRecord.WipWorkOrder.SkuNumber} has been submitted.", "Multi-Transaction Accepted", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }

            #endregion

            #region Issue for Repair Orders

            if (wipRecord.WipWorkOrder.TaskType == "R")
            {
                try
                {
                    foreach (var c in wipRecord.WipWorkOrder.Picklist)
                    {
                        var _issue = new Issue(wipRecord.Submitter, "010", c.CompNumber, wipRecord.WipWorkOrder.OrderNumber, "II", new List<Transaction>());

                        foreach (var w in c.WipInfo.Where(o => !string.IsNullOrEmpty(o.LotNbr)))
                        {
                            _issue.TranList.Add(new Transaction { Location = w.RcptLoc, LotNumber = w.LotNbr, Quantity = Convert.ToInt32(w.LotQty) });
                        }
                        if (c.WipInfo.Sum(o => o.BaseQty) > 0)
                        {
                            File.WriteAllText($"{connection.BTIFolder}ISSUE{connection.AdiServer}.DAT{suffix}i{tranCount}", _issue.ToString());
                            tranCount++;
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show($"Unable to process Issue\nPlease contact IT immediately!\n\n{e.Message}", "M2k Issue file error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            #endregion

            #region Post Labor

            try
            {
                //Posting labor if a crew exists, if the machine is running an interactive report sheet then the labor should be posted there
                if (postLabor && !string.IsNullOrEmpty(machID))
                {
                    var _crew = wipRecord.CrewList.Count(o => !string.IsNullOrEmpty(o.Name));
                    var _wipQty = wipRecord.IsMulti ? Convert.ToInt32(wipRecord.WipQty * wipRecord.RollQty) : Convert.ToInt32(wipRecord.WipQty);
                    foreach (var c in wipRecord.CrewList.Where(o => !string.IsNullOrEmpty(o.Name) && o.IsDirect))
                    {
                        var _pl = PostLabor("SFW WIP", c.IdNumber, c.Shift, $"{wipRecord.WipWorkOrder.OrderNumber}*{wipRecord.WipWorkOrder.Seq}", _wipQty, machID, ' ', connection, c.LastClock, _crew);
                        if (_pl.Any(o => o.Key == 1))
                        {
                            System.Windows.MessageBox.Show($"Unable to process Labor\nPlease contact IT immediately!\n\n{_pl[1]}", "M2k Labor file error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"Unable to process Labor\nPlease contact IT immediately!\n\n{e.Message}", "M2k Labor file error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            #endregion

            #region Scrap Adjustment

            var _adjustString = string.Empty;
            //Adjusting any scrap out of the system that was recorded during the wip
            foreach (var s in wipRecord.ScrapList.Where(o => int.TryParse(o.Quantity, out int i) && i > 0))
            {
                InventoryAdjustment(wipRecord.Submitter,
                    !string.IsNullOrEmpty(s.Reference) ? $"{s.Reference}*{wipRecord.WipWorkOrder.OrderNumber}" : wipRecord.WipWorkOrder.OrderNumber,
                    wipRecord.WipWorkOrder.SkuNumber,
                    (AdjustCode)Enum.Parse(typeof(AdjustCode), s.Reason.GetValueFromDescription<AdjustCode>().ToString(), true),
                    'S',
                    Convert.ToInt32(s.Quantity),
                    wipRecord.ReceiptLocation,
                    connection,
                    wipRecord.WipLot.LotNumber == "NonLotWip" || wipRecord.WipLot.LotNumber == "Multiple" ? "" : wipRecord.WipLot.LotNumber);
            }
            if (_tWip?.AdjustmentList?.Count > 0)
            {
                foreach (var s in _tWip.AdjustmentList)
                {
                    File.WriteAllText($"{connection.BTIFolder}ADJUST{connection.AdiServer}.DAT{suffix}s{tranCount}", s.ToString());
                    tranCount++;
                }
            }

            #endregion

            #region Reclaim Adjustment

            //Adjusting out any reclaim from the system and then adjusting it back in as the raw compound
            if (wipRecord.IsReclaim == SFW.Model.Enumerations.Complete.Y)
            {
                var test = Convert.ToInt32(Math.Round(Convert.ToDouble(wipRecord.ReclaimQty) * wipRecord.ReclaimAssyQty, 0, MidpointRounding.AwayFromZero));
                //Adjustment out
                InventoryAdjustment(wipRecord.Submitter,
                    $"{wipRecord.ReclaimReference}*{wipRecord.WipWorkOrder.OrderNumber}",
                    wipRecord.WipWorkOrder.SkuNumber,
                    AdjustCode.REC,
                    'S',
                    Convert.ToInt32(wipRecord.ReclaimQty),
                    wipRecord.ReceiptLocation,
                    connection);
                //Adjustment in
                InventoryAdjustment(wipRecord.Submitter,
                    $"{wipRecord.ReclaimReference}*{wipRecord.WipWorkOrder.OrderNumber}",
                    wipRecord.ReclaimParent,
                    AdjustCode.REC,
                    'A',
                    Convert.ToInt32(Math.Round(Convert.ToDouble(wipRecord.ReclaimQty) * wipRecord.ReclaimAssyQty, 0, MidpointRounding.AwayFromZero)),
                    "EXT-1",
                    connection);
            }

            #endregion

            #region Roll Marked Gone

            foreach (var mat in wipRecord.WipWorkOrder.Picklist.Where(o => o.WipInfo.Count(p => p.RollStatus) > 0))
            {
                foreach (var info in mat.WipInfo.Where(o => o.RollStatus && o.OnHandCalc > 0))
                {
                    InventoryMove(wipRecord.Submitter, info.PartNbr, info.LotNbr, info.Uom, info.RcptLoc, "SCRAP", info.OnHandCalc, "Roll Marked Gone", connection);
                }
            }

            #endregion

            if (!wipRecord.IsMulti)
            {
                _subResult.Add(1, wipRecord.WipLot.LotNumber);
            }
            else
            {
                _subResult.Add(1, string.Join("*", _lotList));
            }
            return _subResult;
        }

        /// <summary>
        /// Post labor in current ERP system within a standard template
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="empID">Employee ID</param>
        /// <param name="shift">Employee Shift, used to calc third shift labor varience</param>
        /// <param name="woAndSeq">Work order and sequence seporated by a '*'</param>
        /// <param name="qtyComp">Quantity completed for this transaction</param>
        /// <param name="machID">Machine ID that will receive the labor posting</param>
        /// <param name="clockTranType">Clock transaction type, when passed will need to be formated as 'I' or 'O', by leaving this parameter blank will cause the method to calculate labor</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <param name="time">Optional: Post time, must be in a 24 hour clock format using only hours and minutes</param>
        /// <param name="crew">Optional: Crew size, only needs to be passed when the crewsize listed in the ERP is smaller or larger that the amount of crew members posting labor to the work order</param>
        /// <param name="tranDate">Optional: Date of transaction</param>
        /// <returns>Error number and error description, when returned as 0 and a empty string the transaction posted with no errors</returns>
        public static IReadOnlyDictionary<int, string> PostLabor(string stationId, string empID, int shift, string woAndSeq, int qtyComp, string machID, char clockTranType, M2kConnection connection, string time = "", int crew = 0, string tranDate = "")
        {
            var _subResult = new Dictionary<int, string>();
            try
            {
                var suffix = DateTime.Now.ToString($"ssffff");
                if (!woAndSeq.Contains('*'))
                {
                    _subResult.Add(1, "Work order or sequence is not in the correct format to pass into M2k.");
                    return _subResult;
                }
                else
                {
                    var _wSplit = woAndSeq.Split('*');
                    if (char.IsWhiteSpace(clockTranType))
                    {
                        if (string.IsNullOrEmpty(tranDate))
                        {
                            if (shift == 3)
                            {
                                if (DateTime.TryParse(time, out DateTime dt))
                                {
                                    if (dt.TimeOfDay > new TimeSpan(19, 00, 00) && dt.TimeOfDay < new TimeSpan(23, 59, 59))
                                    {
                                        if (DateTime.Now.TimeOfDay > new TimeSpan(19,00,00) && DateTime.Now.TimeOfDay < new TimeSpan(23,59,59))
                                        {
                                            tranDate = DateTime.Now.ToString("MM-dd-yyyy");
                                        }
                                        else
                                        {
                                            tranDate = DateTime.Now.AddDays(-1).ToString("MM-dd-yyyy");
                                        }
                                    }
                                    else
                                    {
                                        tranDate = DateTime.Now.ToString("MM-dd-yyyy");
                                    }
                                }
                            }
                            else
                            {
                                tranDate = DateTime.Now.ToString("MM-dd-yyyy");
                            }
                        }
                        var _inDL = new DirectLabor(stationId, empID, 'I', time, _wSplit[0], _wSplit[1], 0, 0, machID, CompletionFlag.N, crew, tranDate);

                        //posting the clock out time for DateTime.Now

                        time = DateTime.Now.ToString("HH:mm");
                        var _outDL = crew > 0
                            ? new DirectLabor(stationId, empID, 'O', time, _wSplit[0], _wSplit[1], qtyComp, 0, machID, CompletionFlag.N, crew)
                            : new DirectLabor(stationId, empID, 'O', time, _wSplit[0], _wSplit[1], qtyComp, 0, machID, CompletionFlag.N);
                        File.WriteAllText($"{connection.SFDCFolder}LB{connection.AdiServer}.DAT{suffix}{empID}", $"{_inDL.ToString()}\n\n{_outDL.ToString()}");
                    }
                    else
                    {
                        var _tempDL = crew > 0
                            ? new DirectLabor(stationId, empID, clockTranType, time, _wSplit[0], _wSplit[1], qtyComp, 0, machID, CompletionFlag.N, crew)
                            : new DirectLabor(stationId, empID, clockTranType, time, _wSplit[0], _wSplit[1], qtyComp, 0, machID, CompletionFlag.N);
                        File.WriteAllText($"{connection.SFDCFolder}LB{connection.AdiServer}.DAT{suffix}{empID}", _tempDL.ToString());
                    }
                    _subResult.Add(0, string.Empty);
                    return _subResult;
                }
            }
            catch (Exception ex)
            {
                _subResult.Add(1, ex.Message);
                return _subResult;
            }
        }

        /// <summary>
        /// Process an inventory adjustment in current ERP system with in the standard ADI template
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="reference">Adjustment reference</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="aCode">Adjustment code</param>
        /// <param name="tranOp">Transaction Operation, see adjust object for more information</param>
        /// <param name="tranQty">Transation quantity</param>
        /// <param name="location">Location</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <param name="lot">Optional: Lot number</param>
        /// <returns>Error number and error description, when returned as 0 and a empty string the transaction posted with no errors</returns>
        public static IReadOnlyDictionary<int, string> InventoryAdjustment(string stationId, string reference, string partNbr, AdjustCode aCode, char tranOp, int tranQty, string location, M2kConnection connection, string lot = "")
        {
            var _subResult = new Dictionary<int, string>();
            try
            {
                var suffix = DateTime.Now.ToString("ssffff");
                suffix += string.IsNullOrEmpty(lot) ? $"{partNbr}{aCode}" : lot.Replace("-", "");
                if (tranQty <= 0)
                {
                    _subResult.Add(1, "Transaction Quantity must have a value greater then 0.");
                    return _subResult;
                }
                var _tScrap = new Adjust(
                    stationId,
                    "01",
                    reference,
                    partNbr,
                    aCode,
                    tranOp,
                    tranQty,
                    location,
                    lot);
                File.WriteAllText($"{connection.BTIFolder}ADJUST{connection.AdiServer}.DAT{suffix}", _tScrap.ToString());
                _subResult.Add(0, string.Empty);
                return _subResult;
            }
            catch (Exception ex)
            {
                _subResult.Add(1, ex.Message);
                return _subResult;
            }
        }

        /// <summary>
        /// Process a cycle count entry in current ERP system with in the standard ADI template
        /// </summary>
        /// <param name="stationId">Station ID</param>
        /// <param name="ccNbr">Cycle Count Number</param>
        /// <param name="partNbr">Part Number</param>
        /// <param name="aCode">Adjustment code</param>
        /// <param name="tranQty">Transation quantity</param>
        /// <param name="location">Location</param>
        /// <param name="connection">Current M2k Connection to be used for processing the transaction</param>
        /// <param name="lot">Optional: Lot number</param>
        /// <returns>Error number and error description, when returned as 0 and a empty string the transaction posted with no errors</returns>
        public static IReadOnlyDictionary<int, string> CycleCount(string stationId, string ccNbr, string partNbr, AdjustCode aCode, int tranQty, string location, M2kConnection connection, string lot = "")
        {
            var _subResult = new Dictionary<int, string>();
            try
            {
                var suffix = DateTime.Now.ToString("ssffff");
                suffix += string.IsNullOrEmpty(lot) ? $"{partNbr}{aCode}" : lot.Replace("-", "");
                if (tranQty < 0)
                {
                    _subResult.Add(1, "Transaction Quantity must have a value greater then 0.");
                    return _subResult;
                }
                var _count = new Cyclect(
                    stationId,
                    "01",
                    ccNbr,
                    CompletionFlag.N,
                    partNbr,
                    aCode,
                    tranQty,
                    location,
                    lot);
                File.WriteAllText($"{connection.BTIFolder}CYCLECT{connection.AdiServer}.DAT{suffix}", _count.ToString());
                _subResult.Add(0, string.Empty);
                return _subResult;
            }
            catch (Exception ex)
            {
                _subResult.Add(1, ex.Message);
                return _subResult;
            }
        }

        //Not implemented yet
        public static IReadOnlyDictionary<int, string> ItemIssue(string stationId, string compNbr, string woNbr)
        {
            var suffix = DateTime.Now.ToString("HHmmssfff");
            return null;
        }

        //Not implemented yet
        public static IReadOnlyDictionary<int, string> Shipment(string stationId, string compNbr, string woNbr)
        {
            var suffix = DateTime.Now.ToString("HHmmssfff");
            return null;
        }
    }
}
