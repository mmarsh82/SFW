using SFW.Commands;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SFW.Queries
{
    public class PartSplit_ViewModel : ViewModelBase
    {
        #region Properties

        private string _lot;
        public string LotNumber
        {
            get
            { return _lot; }
            set
            {
                if (value.Count() >= 7)
                {
                    ValidLot = Lot.IsValid(value, App.AppSqlCon);
                }
                _lot = value;
                OnPropertyChanged(nameof(LotNumber));
            }
        }
        public bool ValidLot { get; set; }

        public BindingList<Lot> SplitLotList { get; set; }

        RelayCommand _split;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PartSplit_ViewModel()
        { }

        #region Roll Split Submit ICommand

        public ICommand SplitSubmitICommand
        {
            get
            {
                if (_split == null)
                {
                    _split = new RelayCommand(SplitExecute, SplitCanExecute);
                }
                return _split;
            }
        }

        private void SplitExecute(object parameter)
        {
            
        }
        private bool SplitCanExecute(object parameter) => true;

        #endregion

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }

    }
}
