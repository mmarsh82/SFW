using SFW.Model;
using System.Collections.Generic;
using System.ComponentModel;

namespace SFW.Tools
{
    public class CrewList_ViewModel : ViewModelBase
    {
        #region Properties

        public BindingList<CrewMember> CrewList { get; set; }

        public bool NoData { get; set; }

        public string PublishDate { get; set; }

        public string ActionInput { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CrewList_ViewModel()
        {
            var _shift = 2;  //new CrewMember(CurrentUser.FirstName, CurrentUser.LastName).Shift;
            var _site = CurrentUser.GetSite();
            if (_shift == 4 && _site == 2)
            {
                _shift = 1;
            }
            else if (_shift == 5 && _site == 2)
            {
                _shift = 2;
            }
            CrewList = new BindingList<CrewMember>(CrewMember.GetCrewList(_shift, _site));
            NoData = CrewList.Count == 0;
        }
    }
}
