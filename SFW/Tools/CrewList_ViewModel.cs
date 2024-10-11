using SFW.Helpers;
using SFW.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SFW.Tools
{
    public class CrewList_ViewModel : ViewModelBase
    {
        #region Properties

        public BindingList<CrewMember> CrewList { get; set; }

        public bool NoData { get; set; }

        public string PublishDate { get; set; }

        public string ActionInput { get; set; }

        private bool _isLoading { get; set; }
        private char _actionType { get; set; }

        private RelayCommand _submitICommand;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewList_ViewModel()
        {
            var _shift = new CrewMember(CurrentUser.FirstName, CurrentUser.LastName).Shift;
            var _site = CurrentUser.GetSite();
            _isLoading = false;
            PublishDate = DateTime.Today.ToShortDateString();
            var _tempDict = CrewMember.GetCrewList(_shift, _site);
            if (_tempDict.Count > 0)
            {
                CrewList = new BindingList<CrewMember>(_tempDict.FirstOrDefault().Value);
                _actionType = _tempDict.FirstOrDefault().Key;
            }
            NoData = CrewList.Count == 0;
            CrewList.ListChanged += CrewList_ListChanged;
        }

        /// <summary>
        /// Happens when an item is added or changed in the WipInfo Binding List property
        /// </summary>
        /// <param name="sender">BindingList<CompWipInfo> list passed without changes</param>
        /// <param name="e">Change info</param>
        private void CrewList_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "IsWorking" && !_isLoading)
            {
                _isLoading = true;
                if (((BindingList<CrewMember>)sender)[e.NewIndex].IsWorking)
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked = ((BindingList<CrewMember>)sender)[e.NewIndex].Facility == "1" ? 8 : 10;
                }
                else
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked = 0;
                }
                _isLoading = false;
            }
            if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor?.DisplayName == "HoursWorked" && !_isLoading)
            {
                _isLoading = true;
                if (((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked > 8 && ((BindingList<CrewMember>)sender)[e.NewIndex].Facility == "1")
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked = 8;
                }
                else if (((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked > 10 && ((BindingList<CrewMember>)sender)[e.NewIndex].Facility == "2")
                {
                    ((BindingList<CrewMember>)sender)[e.NewIndex].HoursWorked = 10;
                }
                _isLoading = false;
            }
        }

        #region Submit Command ICommand

        public ICommand SubmitICommand
        {
            get
            {
                if (_submitICommand == null)
                {
                    _submitICommand = new RelayCommand(ActionCommandExecute, ActionCommandCanExecute);
                }
                return _submitICommand;
            }
        }

        private void ActionCommandExecute(object parameter)
        {
            var _msgText = CrewMember.PublishLabor(CrewList.ToList(), _actionType, App.AppSqlCon);
            MessageBox.Show(_msgText, "Publishing Message", MessageBoxButton.OK, MessageBoxImage.Information);
            if (_msgText.Contains("Successfully"))
            {
                _actionType = 'U';
            }
        }
        private bool ActionCommandCanExecute(object parameter) => true;

        #endregion

    }
}
