using SFW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Queries
{
    public class PartDetail_ViewModel : ViewModelBase
    {
        #region Properties

        public Sku Part { get; set; }

        private string lotNbr;
        public string LotNbr
        {
            get { return lotNbr; }
            set { lotNbr = value; OnPropertyChanged(nameof(LotNbr)); }
        }

        public bool NonLot { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartDetail_ViewModel()
        {
            NonLot = false;
        }
    }
}
