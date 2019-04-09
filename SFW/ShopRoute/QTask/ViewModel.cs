using SFW.Model;

namespace SFW.ShopRoute.QTask
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        public WorkOrder ShopOrder { get; set; }
        public int CurrentSite { get { return App.SiteNumber; } }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewModel()
        {
            if (ShopOrder == null)
            {
                ShopOrder = new WorkOrder();
            }
        }

        /// <summary>
        /// Overridden Constructor
        /// </summary>
        /// <param name="wo">Work order object to load into the view</param>
        public ViewModel(WorkOrder wo)
        {
            ShopOrder = wo;
        }
    }
}
