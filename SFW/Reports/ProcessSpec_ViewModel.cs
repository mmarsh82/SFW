using SFW.Model;
using SFW.Model.Production;

namespace SFW.Reports
{
    public class ProcessSpec_ViewModel
    {
        #region Properties

        public WorkOrder ShopOrder { get; set; }
        public UdefSku SkuSpec { get; set; }
        public string Compound { get; set; }
        public string CompoundDesc { get; set; }
        public string Fabric { get; set; }
        public string FabricDesc { get; set; }
        public string Poly { get; set; }
        public string PolyDesc { get; set; }
        public string FrictionRoll { get; set; }
        public string FrictionDesc { get; set; }
        public bool SlitSpec { get; set; }
        public int PackLoc { get; set; }

        #endregion

        /// <summary>
        /// Process Spec ViewModel Constructor
        /// </summary>
        /// <param name="wo"></param>
        public ProcessSpec_ViewModel(WorkOrder wo)
        {
            ShopOrder = wo;
            SkuSpec = new UdefSku(wo.Product.SkuNumber, wo.Seq, App.AppSqlCon);
            foreach (var s in wo.PickList)
            {
                switch (s.InventoryType)
                {
                    case "RC":
                        Compound = s.ProductNumber;
                        CompoundDesc = s.ProductDescription;
                        break;
                    case "FR":
                        Fabric = s.ProductNumber;
                        FabricDesc = s.ProductDescription;
                        break;
                    case "PO":
                        Poly = s.ProductNumber;
                        PolyDesc = s.ProductDescription;
                        break;
                    case "CA":
                        FrictionRoll = s.ProductNumber;
                        FrictionDesc = s.ProductDescription;
                        break;
                }
            }
            SlitSpec = SkuSpec.SpecDesc.Contains("SLIT");
            PackLoc = SkuSpec.SpecDesc.Contains("SLIT") ? 3 : 4;
        }
    }
}
