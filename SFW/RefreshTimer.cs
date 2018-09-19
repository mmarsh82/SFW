﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;

//Created by Michael Marsh 9-14-18

namespace SFW
{
    public sealed class RefreshTimer
    {
        #region Properties

        public static DispatcherTimer RefreshDispatchTimer { get; private set; }
        public static Action RefreshActionGroup { get; set; }

        public static bool isRefresh;
        public static bool IsRefreshing
        {
            get
            { return isRefresh; }
            set
            {
                isRefresh = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsRefreshing)));
            }
        }

        public static DateTime lastRefresh;
        public static DateTime LastRefresh
        {
            get
            { return lastRefresh; }
            set
            {
                lastRefresh = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastRefresh)));
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Defualt Constructor
        /// </summary>
        public RefreshTimer()
        { }

        /// <summary>
        /// Intialization and start call for the refresh timer
        /// </summary>
        /// <param name="timeSpan">Time increment to use for data refresh</param>
        public static void Start(TimeSpan increment)
        {
            IsRefreshing = false;
            RefreshDispatchTimer = new DispatcherTimer();
            RefreshDispatchTimer.Tick += new EventHandler(RefreshTimerTick);
            RefreshDispatchTimer.Interval = increment;
            RefreshDispatchTimer.Start();
        }

        /// <summary>
        /// Tick event for the Action Dispatch Timer
        /// </summary>
        /// <param name="sender">Object call from the sending thread</param>
        /// <param name="e">Events that can be called on the sending object</param>
        public static void RefreshTimerTick(object sender, EventArgs e)
        {
            IsRefreshing = true;
            //RefreshActionGroup?.Invoke();
            LastRefresh = DateTime.Now;
            IsRefreshing = false;
        }

        /// <summary>
        /// Checks the invocation list to see if it contains the method group
        /// </summary>
        /// <param name="action">Method group to search for</param>
        /// <returns>True = invocation list contains method, False = action has not been added yet</returns>
        public static bool ActionExists(Action action)
        {
            if (RefreshActionGroup == null)
            {
                return false;
            }
            foreach (Action _action in RefreshActionGroup.GetInvocationList())
            {
                if (_action.Method.Name.Equals(action.Method.Name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add an action to the refresh timer que
        /// </summary>
        /// <param name="action">Method or Action to add</param>
        public static void Add(Action action)
        {
            RefreshActionGroup += action;
        }

        /// <summary>
        /// Remove an action from the refresh timer que
        /// </summary>
        /// <param name="action">Method or Action to remove</param>
        public static void Remove(Action action)
        {
            if (ActionExists(action))
            {
                RefreshActionGroup -= action;
            }
        }
    }
}
