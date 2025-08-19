using SFW.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFW
{
    public class ApplicationTimer
    {
        #region Properties

        public static Timer MainTimer { get; set; }
        public static TimeSpan RefreshInterval { get; set; }

        private static TimerState _status;
        public static TimerState Status
        {
            get 
            { return _status; }
            set
            {
                _status = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
            }
        }
        public static IList<Action> ActionList { get; set; }

        private static string _lastRefresh;
        public static string LastRefresh
        {
            get => _lastRefresh;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _lastRefresh = value;
                }
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastRefresh)));
            }
        }

        private static int _lastdurationMin;
        public static int LastDurationMinutes
        {
            get => _lastdurationMin;
            set
            {
                _lastdurationMin = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastDurationMinutes)));
            }
        }

        private static int _lastdurationSec;
        public static int LastDurationSeconds
        {
            get => _lastdurationSec;
            set
            {
                _lastdurationSec = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastDurationSeconds)));
            }
        }

        public static string TickMessage { get; set; }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ApplicationTimer()
        {
            Start();
        }

        public static void CleanActionList(IList<Action> actList)
        {
            var _tempList = new List<Action>();
            foreach (var _action in actList)
            {
                if (_tempList.Count(o => o.Target.ToString() == _action.Target.ToString()) == 0)
                {
                    _tempList.Add(_action);
                }
            }
            ActionList = _tempList;
        }

        /// <summary>
        /// Tick operation for when the timer interval expires
        /// </summary>
        /// <param name="state"></param>
        public static async void Tick(object state)
        {
            if (Status != TimerState.Aborted || Status != TimerState.Paused || Status != TimerState.Running || Status != TimerState.Stopped)
            {
                await Task.Run(new Action(delegate
                {
                    var _start = DateTime.Now;
                    Status = TimerState.Running;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
                    CleanActionList(ActionList);
                    ModelBase.LoadedModules.Clear();
                    ModelBase.LoadedModules = Module.GetModuleList(CurrentUser.Modules);
                    foreach (var _mod in ModelBase.LoadedModules)
                    {
                        TickMessage = $"{_mod.Group}.{_mod.TableType.Name}";
                        StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(TickMessage)));
                        if (!ModelBase.MasterDataSet.Tables.Contains(_mod.TableType.Name))
                        {
                            ModelBase.MasterDataSet.Tables.Add(_mod.TableType.Name);
                        }
                        ModelBase.MasterDataSet.LoadTable(_mod, App.SiteNumber, App.AppSqlCon);
                    }
                    TickMessage = $"Refreshing Views";
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(TickMessage)));
                    foreach (var _action in ActionList)
                    {
                        _action.Invoke();
                    }
                    TickMessage = string.Empty;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(TickMessage)));
                    LastRefresh = DateTime.Now.ToString("MM-dd-yyyy HH:mm");
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastRefresh)));
                    Status = TimerState.Sleeping;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Status)));
                    var _duration = DateTime.Now.Subtract(_start);
                    LastDurationMinutes = _duration.Minutes;
                    LastDurationSeconds = _duration.Seconds;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastDurationMinutes)));
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(LastDurationSeconds)));
                }));
            }
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        public static void Start()
        {
            if (ActionList == null)
            {
                ActionList = new List<Action>();
            }
            if (MainTimer == null)
            {
                if (RefreshInterval == null)
                {
                    RefreshInterval = new TimeSpan(0, 0, 5, 0, 0);
                }
                MainTimer = new Timer(new TimerCallback(Tick));
            }
            Status = MainTimer.Change(RefreshInterval, RefreshInterval) ? TimerState.Sleeping : TimerState.Aborted;
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public static void Stop() => Status = MainTimer.Change(Timeout.Infinite, Timeout.Infinite) ? TimerState.Stopped : TimerState.Aborted;

        /// <summary>
        /// Pause the timer
        /// </summary>
        public static void Pause() => Status = MainTimer.Change(Timeout.Infinite, Timeout.Infinite) ? TimerState.Paused : TimerState.Aborted;

        /// <summary>
        /// Resume the timer
        /// </summary>
        public static void Resume()
        {
            Status = MainTimer.Change(RefreshInterval, RefreshInterval) ? TimerState.Sleeping : TimerState.Aborted;
            Tick(0);
        }
    }

    public enum TimerState
    {
        Running = 0,
        Sleeping = 1,
        Paused = 2,
        Aborted = 3,
        Stopped = 4
    }
}
