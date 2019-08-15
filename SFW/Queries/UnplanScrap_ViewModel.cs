using SFW.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        RelayCommand _scrapSubmit;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UnplanScrap_ViewModel()
        { }

        #region Unplanned Scrap Submit ICommand

        public ICommand ScrapSubmitICommand
        {
            get
            {
                if (_scrapSubmit == null)
                {
                    _scrapSubmit = new RelayCommand(ScrapSubmitExecute, ScrapSubmitCanExecute);
                }
                return _scrapSubmit;
            }
        }

        private void ScrapSubmitExecute(object parameter)
        {
            
        }
        private bool ScrapSubmitCanExecute(object parameter)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _scrapSubmit = null;
            }
        }
    }
}
