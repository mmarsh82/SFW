using SFW.Controls;
using SFW.Queries;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows;
using System.Windows.Input;

//Created by Michael Marsh 4-19-18

namespace SFW.Commands
{
    public class ViewLoad : ICommand
    {
        public static IList<int> HistoryList { get; set; }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ViewLoad()
        {
            if (HistoryList == null)
            {
                HistoryList = new List<int>();
            }
        }

        /// <summary>
        /// View Load Execution
        /// </summary>
        /// <param name="parameter">View to Load</param>
        public void Execute(object parameter)
        {
            try
            {
                if (int.TryParse(parameter.ToString(), out int z))
                {
                    switch (z)
                    {
                        case -3:
                            RefreshTimer.Stop();
                            return;
                        case -4:
                            RefreshTimer.Start();
                            parameter = -2;
                            break;
                    }
                }
                var _wo = new object();
                _wo = null;
                if (parameter.GetType() == typeof(Model.WorkOrder) || parameter.ToString().Length > 3)
                {
                    _wo = parameter;
                    parameter = 0;
                }
                var _view = int.TryParse(parameter.ToString(), out int i) ? i : App.SiteNumber;
                var _addhist = _view == App.SiteNumber ? false : true;
                var _viewModel = new object();
                _viewModel = null;
                var refreshView = _view == 1 || _view == 2;
                //Handling the back function
                if (_view == -1)
                {
                    _addhist = false;
                    if (HistoryList.Count > 0)
                    {
                        HistoryList.RemoveAt(HistoryList.Count - 1);
                        if (HistoryList.Count == 0)
                        {
                            _view = App.SiteNumber;
                        }
                        else
                        {
                            _view = HistoryList.Last();
                        }
                    }
                }
                if (_view == App.SiteNumber)
                {
                    _addhist = false;
                    HistoryList.Clear();
                }
                switch (_view)
                {
                    //The part information command calls can either send a work order object, part number or a null variable.  Need to handle each case
                    case 0:
                        if (_wo != null)
                        {
                            _viewModel = _wo.GetType() == typeof(Model.WorkOrder) ? new PartInfo_ViewModel((Model.WorkOrder)_wo) : new PartInfo_ViewModel(_wo.ToString());
                        }
                        break;
                    //Handles the refresh schedule calls
                    case -2:
                        if (!RefreshTimer.IsRefreshing)
                        {
                            RefreshTimer.RefreshTimerTick();
                            RefreshTimer.Reset();
                        }
                        else
                        {
                            MessageBox.Show("The work load is currently refreshing.");
                        }
                        break;
                }
                if(_view != -2)
                {
                    if(_addhist)
                    {
                        HistoryList.Add(_view);
                    }
                    WorkSpaceDock.SwitchView(_view, _viewModel, refreshView);
                }
            }
            catch (Exception)
            {

            }
        }
        public bool CanExecute(object parameter) => true;
    }
}
