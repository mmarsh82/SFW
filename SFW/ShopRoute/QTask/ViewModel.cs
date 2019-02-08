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

        }
    }
}
