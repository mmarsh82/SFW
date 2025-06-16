using SFW.Model.Enumerations;
using SFW.Model.Product;
using System;
using System.ComponentModel;
using System.Linq;

namespace SFW.Model.Production.Wip
{
    public class Component : Production.Component
    {
        #region Properties

        private BindingList<Lot> _lotList;
        public BindingList<Lot> LotList 
        { 
            get
            { return _lotList; }
            set
            { _lotList = value; OnPropertyChanged(nameof(LotList)); }
        }
        public bool LotTraceable { get; set; }

        private string bfLoc;
        public string BackFlushLoc
        {
            get { return bfLoc; }
            set { bfLoc = value; OnPropertyChanged(nameof(BackFlushLoc)); }
        }

        public bool IsBackFlush { get; set; }
        public string WorkOrderNumber { get; set; }
        public string WorkOrderSequence { get; set; }

        private Complete isScrap;
        public Complete IsScrap
        {
            get { return isScrap; }
            set
            {
                isScrap = value;
                if (value == Complete.N)
                {
                    ScrapList.Clear();
                }
                else
                {
                    ScrapList.Add(new Scrap { ID = $"0*{ProductNumber}" });
                }
                OnPropertyChanged(nameof(IsScrap));
            }
        }

        public double ScrapFactor { get; set; }

        public BindingList<Scrap> ScrapList { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Component()
        {
            ScrapList = new BindingList<Scrap>();
            LotList = new BindingList<Lot>();
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="hasBFLoc">Does the component have a default backflush location</param>
        /// <param name="partNbr">Part Number of the component</param>
        /// <param name="uom">Part Unit of Measure of the component</param>
        /// <param name="workOrderNumber">Work order number</param>
        /// <param name="seq">Work order routing or sequence</param>
        public Component(bool hasBFLoc, string partNbr, string uom, decimal assembQty, string workOrderNumber, string seq)
        {
            ScrapList = new BindingList<Scrap>();
            LotTraceable = Sku.IsLotTracable(partNbr, ModelFacility);
            if (LotTraceable)
            {
                LotList = new BindingList<Lot>
                {
                    new Lot(workOrderNumber, partNbr)
                };
                LotList.ListChanged += LotList_Changed;
            }
            IsBackFlush = hasBFLoc;
            ProductNumber = partNbr;
            ProductDescription = new Sku(partNbr).SkuDescription;
            ProductUom = uom;
            IsScrap = Complete.N;
            WorkOrderNumber = workOrderNumber;
            WorkOrderSequence = seq;
            AssemblyQuantity = assembQty;
            ScrapFactor = double.TryParse(PickComponent.GetScrapFactor(partNbr, workOrderNumber, seq).ToString(), out double d) ? d : 0;
        }

        /// <summary>
        /// Lot List property list changed event method
        /// </summary>
        /// <param name="sender">LotList property</param>
        /// <param name="e">event arguments</param>
        private void LotList_Changed(object sender, ListChangedEventArgs e)
        {
            var _tempList = (BindingList<Lot>)sender;
            var _tempComp = e.ListChangedType != ListChangedType.ItemDeleted && e.ListChangedType != ListChangedType.Reset ? _tempList[e.NewIndex] : null;
            var _reCalc = false;
            var _action = 'N';
            ((BindingList<Lot>)sender).RaiseListChangedEvents = false;

            switch(e.ListChangedType)
            {
                //Calculating the new lot quantities when an item is deleted
                case ListChangedType.ItemDeleted:
                    _reCalc = _tempList.Count > 0;
                    break;

                //Calculating the new lot quantities when an item is added
                case ListChangedType.ItemAdded:
                    _reCalc = _tempComp.Valid;
                    break;

                //Actions for each of the properties when they change
                //Some of the changes are taken care of in setters, this is for when the change affects other properties
                case ListChangedType.ItemChanged:

                    switch (e.PropertyDescriptor.DisplayName)
                    {
                        //Manually entered quantity by the end user which will lock this quantity
                        case "Quantity":
                            ((BindingList<Lot>)sender)[e.NewIndex].QuantityLocked = true;
                            _reCalc = _tempList[e.NewIndex].Valid;
                            break;

                        //Manually entered lot number that needs to be validated
                        //If validated then will add a new line for the next input
                        case "ID":
                            if (!string.IsNullOrEmpty(_tempComp.ID))
                            {
                                _tempComp.Valid = Product.Lot.IsValid(_tempComp.ID, _tempComp.ProductId) && _tempList.Count(o => o.ID == _tempComp.ID) == 1;
                                if (_tempComp.Valid)
                                {
                                    ((BindingList<Lot>)sender)[e.NewIndex].Valid = _tempComp.Valid;
                                    ((BindingList<Lot>)sender)[e.NewIndex].Location = Product.Lot.GetLocation(_tempComp.ID);
                                    ((BindingList<Lot>)sender)[e.NewIndex].SystemStock = Product.Lot.GetOnHandQuantity(_tempComp.ID);
                                    ((BindingList<Lot>)sender)[e.NewIndex].LotUom = Product.Lot.GetUom(_tempComp.ID);
                                    ((BindingList<Lot>)sender)[e.NewIndex].QuantityLocked = false;
                                    ((BindingList<Lot>)sender)[e.NewIndex].ValidAmount = false;
                                    _reCalc = true;
                                    _action = 'A';
                                }
                                else if (_tempList.Count > 1 && !_tempList.Last().Valid && _tempList.Last() != _tempComp)
                                {
                                    _action = 'D';
                                }
                            }
                            break;

                    }
                    break;
            }

            //The calculation process
            if (_reCalc)
            {
                var _divCnt = _tempList.Count(o => o.Valid && !o.QuantityLocked);
                var _balance = ((BindingList<Lot>)sender).First().RequiredQuantity - _tempList.Where(o => o.Valid && o.QuantityLocked).Sum(o => int.TryParse(o.Quantity, out int i) ? i : 0);
                if (_divCnt > 0 && _balance > 0)
                {
                    foreach (var _lot in _tempList.Where(o => o.Valid && !o.QuantityLocked))
                    {
                        var _qty = Convert.ToInt32(Math.Round(Convert.ToDecimal(_balance) / Convert.ToDecimal(_divCnt), 0));
                        ((BindingList<Lot>)sender).FirstOrDefault(o => o.ID == _lot.ID).Quantity = _qty.ToString();
                        _balance -= _qty;
                        _divCnt--;
                    }
                }
                else if (_balance < 0)
                {
                    foreach (var _comp in _tempList.Where(o => !o.QuantityLocked))
                    {
                        ((BindingList<Lot>)sender).FirstOrDefault(o => o.ID == _comp.ID).Quantity = "0";
                    }
                }
                else if (e.ListChangedType != ListChangedType.ItemDeleted && e.ListChangedType != ListChangedType.Reset)
                {
                    ((BindingList<Lot>)sender)[e.NewIndex].Quantity = _balance.ToString();
                }

                var _validAmount = ((BindingList<Lot>)sender).Sum(o => int.TryParse(o.Quantity, out int i) ? i : 0) == _tempList.First().RequiredQuantity;
                foreach (var _comp in _tempList.Where(o => o.Valid))
                {
                    ((BindingList<Lot>)sender).FirstOrDefault(o => o.ID == _comp.ID).ValidAmount = _validAmount;
                }
            }
            ((BindingList<Lot>)sender).RaiseListChangedEvents = true;

            //Allowing the list to refresh if a valid lot was entered or if one was removed
            switch(_action)
            {
                case 'A':
                    ((BindingList<Lot>)sender).Add(new Lot(_tempComp.RequiredQuantity, _tempComp.OrderId, _tempComp.ProductId));
                    break;
                case 'D':
                    ((BindingList<Lot>)sender).Remove(((BindingList<Lot>)sender).Last());
                    break;
            }
        }
    }
}
