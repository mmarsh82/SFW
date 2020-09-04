using M2kClient;
using SFW.Helpers;
using SFW.Model;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SFW.CycleCount
{
    public class Form_ViewModel : ViewModelBase
    {
        #region Properties

        private Count _countTran;
        public Count CountTran 
        { 
            get { return _countTran; }
            set
            {
                _countTran = value;
                OnPropertyChanged(nameof(CountTran));
            }
        }

        public string CountLocation
        {
            get
            { return CountTran.CountLoc; }
            set
            {
                CountTran.CountLoc = value.ToUpper();
                OnPropertyChanged(nameof(CountLocation));
                OnPropertyChanged(nameof(IsLocValid));
                OnPropertyChanged(nameof(LocSize));
            }
        }

        public bool IsLocValid { get { return !string.IsNullOrEmpty(CountLocation) && Sku.IsValidLocation(CountTran.CountLoc, App.AppSqlCon); } }
        public int LocSize { get { return IsLocValid ? 1 : 3; } }

        public List<Lot> ILotResultsList { get; set; }

        public delegate void ResultsDelegate(string s);
        public ResultsDelegate ResultsAsyncDelegate { get; private set; }
        public IAsyncResult SearchAsyncResult { get; set; }

        private RelayCommand _countSubmit;

        #endregion

        /// <summary>
        /// Cycle Count Form ViewModel default constructor
        /// </summary>
        public Form_ViewModel()
        {
            CountTran = new Count();
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
        }

        /// <summary>
        /// Cycle Count Form ViewModel overloaded constructor
        /// </summary>
        /// <param name="_cnt">Cycle Count Object</param>
        public Form_ViewModel(Count _cnt)
        {
            CountTran = _cnt;
            ResultsAsyncDelegate = new ResultsDelegate(ResultsLoading);
            SearchAsyncResult = ResultsAsyncDelegate.BeginInvoke(_cnt.PartNumber, new AsyncCallback(ResultsLoaded), null);
        }

        #region Load Results Async Delegation Implementation

        public void ResultsLoading(string inputVal)
        {
            ILotResultsList = Lot.GetOnHandLotList(inputVal, App.AppSqlCon);
            if (ILotResultsList.Count == 0)
            {
                ILotResultsList = Lot.GetOnHandNonLotList(inputVal, App.AppSqlCon);
            }
        }
        public void ResultsLoaded(IAsyncResult r)
        {
            OnPropertyChanged(nameof(ILotResultsList));
        }

        #endregion

        #region Cycle Count Submit ICommand

        public ICommand CntSubmitICommand
        {
            get
            {
                if (_countSubmit == null)
                {
                    _countSubmit = new RelayCommand(CountSubmitExecute, CountSubmitCanExecute);
                }
                return _countSubmit;
            }
        }

        private void CountSubmitExecute(object parameter)
        {
            M2kCommand.CycleCount(CurrentUser.DisplayName, CountTran.CountNumber, CountTran.PartNumber, AdjustCode.CC, CountTran.CountQty, CountTran.CountLoc, App.ErpCon, CountTran.LotNumber);
            ((Sched_ViewModel)Controls.WorkSpaceDock.CountDock.GetChildOfType<Sched_View>().DataContext).RefreshSchedule(CountTran.CountID);
        }
        private bool CountSubmitCanExecute(object parameter) => IsLocValid;

        #endregion
    }
}
