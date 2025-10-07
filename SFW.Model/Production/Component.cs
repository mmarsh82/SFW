namespace SFW.Model.Production
{
    public abstract class Component : ModelBase
    {
        #region Properties

        public string ProductNumber { get; set; }
        public decimal AssemblyQuantity { get; set; }
        public string ProductDescription { get; set; }
        public string ProductMasterPrint { get; set; }
        public string ProductUom { get; set; }
        public bool IsLotTrace { get; set; }

        #endregion
    }
}
