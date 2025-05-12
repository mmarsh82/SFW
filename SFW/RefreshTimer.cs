using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;

//Created by Michael Marsh 9-14-18

namespace SFW
{
    public sealed class RefreshTimer
    {
        #region Properties

        public static DispatcherTimer RefreshDispatchTimer { get; private set; }
        public static IList<Action> RefreshActionGroup { get; set; }
        public static TimeSpan RefreshTimeSpan { get; set; }
        public static bool Status => RefreshDispatchTimer.IsEnabled;
            

        public static bool isRefresh;
        public static bool IsRefreshing
        {
            get => isRefresh;
            set
            {
                isRefresh = value;
                LastRefresh = value ? "Refreshing..." : "";
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsRefreshing)));
            }
        }

        public static string lastRefresh;
        public static string LastRefresh
        {
            get => lastRefresh;
            set
            {
                lastRefresh = string.IsNullOrEmpty(value) ? DateTime.Now.ToString("MM-dd-yyyy HH:mm tt") : value;
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
        public static void Start()
        {
            if (RefreshTimeSpan != null)
            {
                IsRefreshing = false;
                RefreshDispatchTimer = new DispatcherTimer();
                RefreshDispatchTimer.Tick += new EventHandler(RefreshTimerTick);
                RefreshDispatchTimer.Interval = RefreshTimeSpan;
                RefreshDispatchTimer.Start();
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

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
            RefreshDispatchTimer.Dispatcher.Thread.SetApartmentState(System.Threading.ApartmentState.STA);
            RefreshDispatchTimer.Dispatcher.Thread.IsBackground = true;
            RefreshDispatchTimer.Start();
            RefreshTimeSpan = increment;
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
        }

        /// <summary>
        /// Stops the internal dispatch timer object
        /// </summary>
        public static void Stop()
        {
            IsRefreshing = false;
            RefreshDispatchTimer.Stop();
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
        }

        /// <summary>
        /// Resets the internal dispatch timer object
        /// </summary>
        public static void Reset()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Tick event for the Action Dispatch Timer
        /// </summary>
        public static void RefreshTimerTick()
        {
            var _count = 0;
            foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
            {
                if (w.Name.Contains("_Window"))
                {
                    _count++;
                }
            }
            if (!IsRefreshing && _count == 1)
            {
                MainWindowViewModel.DisplayAction = true;
                foreach (var _action in RefreshActionGroup)
                {
                    _action.Invoke();
                }
            }
            if (UserIdleTimer.GetIdleTimeInfo().IdleTime.TotalMinutes >= 5 && !CurrentUser.IsNamedUser)
            {
                CurrentUser.LogOff();
            }
            else if (CurrentUser.IsNamedUser)
            {
                CurrentUser.RefreshLogIn();
            }
        }

        /// <summary>
        /// Tick event for the Action Dispatch Timer
        /// </summary>
        /// <param name="sender">Object call from the sending thread</param>
        /// <param name="e">Events that can be called on the sending object</param>
        public static void RefreshTimerTick(object sender, EventArgs e)
        {
            var _count = 0;
            foreach (System.Windows.Window w in System.Windows.Application.Current.Windows)
            {
                if (w.Name.Contains("_Window"))
                {
                    _count++;
                }
            }
            if (!IsRefreshing)
            {
                MainWindowViewModel.DisplayAction = true;
                foreach (var _action in RefreshActionGroup)
                {
                    _action.Invoke();
                }
            }
            if (UserIdleTimer.GetIdleTimeInfo().IdleTime.TotalMinutes >= 5 && !CurrentUser.IsNamedUser)
            {
                CurrentUser.LogOff();
            }
            else if (CurrentUser.IsNamedUser)
            {
                CurrentUser.RefreshLogIn();
            }
        }
    }
}
