using SFW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFW.Queries
{
    public class PartSpec_ViewModel : ViewModelBase
    {
        #region Properties

        public UdefSku CustomSku { get; set; }

        private string input;
        public string InputSku
        {
            get { return input; }
            set
            {
                input = value.ToUpper();
                CustomSku = new UdefSku(value.ToUpper(), App.AppSqlCon);
                OnPropertyChanged(nameof(CustomSku));
                ValidSku = CustomSku.Length > 0;
                OnPropertyChanged(nameof(ValidSku));
                OnPropertyChanged(nameof(InputSku));
            }
        }

        public bool ValidSku { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartSpec_ViewModel()
        { }
    }
}
