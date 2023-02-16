using SFW.Model;
using System;
using System.Collections.Generic;

//Created by Michael Marsh 5-17-18

namespace SFW.Scheduler
{
    public class ViewModel : ViewModelBase
    {
        #region Properties

        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); }
        }

        public List<Machine> WorkCenter { get; set; }

        #endregion

        public ViewModel()
        {
            if (SelectedDate == DateTime.MinValue)
            {
                SelectedDate = DateTime.Today;
            }
            if (WorkCenter == null)
            {
                WorkCenter = Machine.GetMachineList(false, false, App.SiteNumber);
                if (WorkCenter.Count > 11)
                {
                    while (WorkCenter.Count > 11)
                    {
                        WorkCenter.RemoveAt(WorkCenter.Count - 1);
                    }
                }
            }
        }
    }
}
