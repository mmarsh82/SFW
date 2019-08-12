using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Queries
{
    public class UnplanScrap_ViewModel : ViewModelBase
    {
        #region Properties

        public string LotNbr { get; set; }
        public string PartNbr { get; set; }
        public string QirNbr { get; set; }

        private int? scrapQty;
        public string ScrapQty
        {
            get
            { return scrapQty.ToString(); }
            set
            {
                if (int.TryParse(value, out int i))
                {
                    scrapQty = i;
                }
                else
                {
                    scrapQty = null;
                }
                OnPropertyChanged(nameof(ScrapQty));
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UnplanScrap_ViewModel()
        { }
    }
}
