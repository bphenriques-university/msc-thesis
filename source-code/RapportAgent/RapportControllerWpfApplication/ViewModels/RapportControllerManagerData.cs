using HelpersForNet;
using Log4NetWrapperLite;
using RapportActionProposer;
using RapportControllerLib;
using RapportControllerWpfApplication.RCWrapper;
using RapportControllerWpfApplication.ViewModels.ContextMenu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RapportControllerWpfApplication.ViewModels {

    class RapportControllerManagerData : IDisposable, INotifyPropertyChanged {

        #region fields

        public RapportControllerAppSettings Settings { get; } = Singleton<RapportControllerAppSettings>.Instance;
        public SnapshotsViewManager SnapshotsManager { get; } = Singleton<SnapshotsViewManager>.Instance;
        public ConsoleViewManager ConsoleManager { get; } = Singleton<ConsoleViewManager>.Instance;
        public IRapportControllerManager RapportControllerManager { get; private set; }

        private Dictionary<int, ControllerFrequencyOption> DicAvailableFrequencies { get; } = new Dictionary<int, ControllerFrequencyOption>();

        public ObservableCollection<ControllerFrequencyOption> _availableFrequencyOptions = new ObservableCollection<ControllerFrequencyOption>();
        public ObservableCollection<ControllerFrequencyOption> AvailableFrequencyOptions {
            get { return _availableFrequencyOptions; }
            set {
                _availableFrequencyOptions = value;
                OnPropertyChanged("AvailableLoggingLevels");
            }
        }

        private bool _canReloadController = false;
        public bool CanReloadController {
            get { return _canReloadController; }
            set {
                _canReloadController = value;
                OnPropertyChanged("CanReloadController");
            }
        }

        private string _statusThreeStatusButtonTag = "None";
        public string StatusThreeStatusButtonTag {
            get { return _statusThreeStatusButtonTag; }
            set {
                _statusThreeStatusButtonTag = value;
                OnPropertyChanged("StatusThreeStatusButtonTag");
            }
        }

        private ObservableCollection<DummyRCPlugin> _plugins = new ObservableCollection<DummyRCPlugin>();
        public ObservableCollection<DummyRCPlugin> Plugins {
            get { return _plugins; }            
            set {
                _plugins = value;
                OnPropertyChanged("Plugins");
            }
        }

        private int _totalNumberOfPlugins = 0;
        public int TotalNumberOfPlugins {
            get { return _totalNumberOfPlugins; }
            set {
                _totalNumberOfPlugins = value;
                OnPropertyChanged("TotalNumberOfPlugins");
                OnPropertyChanged("ProgressPluginsLoad");
            }
        }

        private bool _canManipulateMonitoringStatus = false;
        public bool CanManipulateMonitoringStatus {
            get { return _canManipulateMonitoringStatus; }
            set {
                if(value != _canManipulateMonitoringStatus) {
                    _canManipulateMonitoringStatus = value;
                    OnPropertyChanged("CanManipulateMonitoringStatus");
                }
            }
        }

        private bool _isRunning = false;
        public bool IsRunning {
            get { return _isRunning; }
            set {
                if (value)
                    Task.Factory.StartNew(StartMonitoring);               
                else
                    Task.Factory.StartNew(StopMonitoring);
            }
        }

        private string _status = "None";
        public string Status {
            get { return _status; }
            set {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public string ProgressPluginsLoad => Plugins.Count == TotalNumberOfPlugins ? TotalNumberOfPlugins.ToString() : (Plugins.Count + " / " + TotalNumberOfPlugins);

        private int NumberEssentialPlugins { get; set; } = 0;
        private int _numberEnabledPlugins = 0;
        private int NumberEnabledPlugins {
            get { return _numberEnabledPlugins; }
            set {
                _numberEnabledPlugins = value;
                if (NumberEnabledPlugins == TotalNumberOfPlugins) {
                    StatusThreeStatusButtonTag = "All";
                }
                else if (NumberEnabledPlugins - NumberEssentialPlugins == 0) {
                    StatusThreeStatusButtonTag = "None";
                }
                else {
                    StatusThreeStatusButtonTag = "Partial";
                }
            }
        }

        public bool AutomaticStart { get; internal set; } = false;
        #endregion

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));            
        }

        public RapportControllerManagerData() {
            AddControllerFrequencyOption(new ControllerFrequencyOption(1));
            AddControllerFrequencyOption(new ControllerFrequencyOption(5));
            AddControllerFrequencyOption(new ControllerFrequencyOption(10));
            AddControllerFrequencyOption(new ControllerFrequencyOption(15));
            AddControllerFrequencyOption(new ControllerFrequencyOption(20));

            //setting saved option
            int settedFrequency = Settings.ControllerFrequency;
            if (DicAvailableFrequencies.ContainsKey(settedFrequency))
                DicAvailableFrequencies[settedFrequency].Toggle();
        }

        private void AddControllerFrequencyOption(ControllerFrequencyOption controllerFrequencyOption) {
            AvailableFrequencyOptions.Add(controllerFrequencyOption);
            DicAvailableFrequencies.Add(controllerFrequencyOption.Frequency, controllerFrequencyOption);
        }

        public void Init() {
            try {
                RapportControllerManager = new RapportProposerController(Settings.PluginsFolderPath, Settings.OptionsFolderPath);
                RapportControllerManager.PluginLoadedEventArgs += Rp_PluginLoadedEvent;
                RapportControllerManager.ControllerStatusChanged += RapportControllerManager_ControllerStatusChanged;

                TotalNumberOfPlugins = RapportControllerManager.ImportPlugins();
            }
            catch(AggregateException ex) {
                foreach(Exception e in ex.InnerExceptions) {
                    Logger.Error("Can't initialize RapportController: " + e.Message);
                    Logger.Error("Stack trace: " + ex.StackTrace);
                }
                try { Dispose(); } catch (Exception) { }
            }
            catch (Exception ex) {
                Logger.Error("Can't initialize RapportController: " + ex.Message);
                Logger.Error("Stack trace: " + ex.StackTrace);
                try { Dispose(); } catch (Exception) { }
            }
        }

        public void Restart() {
            Settings.Dispose();
            if (RapportControllerManager != null)
                Dispose();
            Init();
        }

        private void RapportControllerManager_ControllerStatusChanged(object sender, ControllerStatusChangedEventArgs e) {
            CanReloadController = e.Status == ControllerStatus.Disposed || e.Status == ControllerStatus.Paused || e.Status == ControllerStatus.Ready;
            CanManipulateMonitoringStatus = e.Status == ControllerStatus.Ready || e.Status == ControllerStatus.Running || e.Status == ControllerStatus.Paused;

            _isRunning = RapportControllerManager.IsRunning;
            OnPropertyChanged("IsRunning");

            if (e.Status == ControllerStatus.Ready) {
                RapportControllerManager.PluginEnableStatusChangedEvent += PluginEnableStatusChangedEvent;
                RapportControllerManager.SnapShotEventHandler += SnapShotEventHandler;

                if (TotalNumberOfPlugins == 0)
                    Logger.Info("There are no plugins in the current folder");

                if (AutomaticStart)
                    StartMonitoring();               
            }

            Status = e.Status.ToString();
        }

        private void PluginEnableStatusChangedEvent(object sender, PluginEnableStatusChangedEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => HandlePluginEnableStatus(e));
        }

        private void SnapShotEventHandler(object sender, SnapshotEventEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => SnapshotsManager.AddSnapshot(e.Snapshot));
        }

        private void Rp_PluginLoadedEvent(object sender, PluginLoadedEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => HandlePluginLoaded(e));
        }

        internal void HandlePluginLoaded(PluginLoadedEventArgs e) {
            var plugin = new DummyRCPlugin(e.Plugin, e.Enabled);

            if (!plugin.IsNotEssential) {
                NumberEssentialPlugins += 1;
            }

            if (string.IsNullOrEmpty(e.ErrorMsg) && e.Enabled)
                NumberEnabledPlugins += 1;
            
            plugin.ErrorInformation = e.ErrorMsg;
            Plugins.Add(plugin);
            OnPropertyChanged("ProgressPluginsLoad");
        }

        internal void DisableAllPlugins() {
            Parallel.ForEach(Plugins, plugin => plugin.Desactivate(RapportControllerManager));
        }

        internal void EnableAllPlugins() {
            Parallel.ForEach(Plugins, plugin => plugin.Activate(RapportControllerManager));
        }

        internal void HandlePluginEnableStatus(PluginEnableStatusChangedEventArgs e) {
            var plugin = Plugins.FirstOrDefault(s => s.Name == e.Plugin.Id);
            plugin.Active = e.Active;
            plugin.ErrorInformation = e.ErrorMessage;
            if (!e.ContainsError)
                NumberEnabledPlugins = e.Active ? Math.Min(NumberEnabledPlugins + 1,TotalNumberOfPlugins) : Math.Max(NumberEnabledPlugins - 1, 0);
        }

        public void StartMonitoring() {
            Task.Factory.StartNew(()=> RapportControllerManager.StartMonitoring(Settings.ControllerFrequency));
        }

        public void StopMonitoring() {
            RapportControllerManager.StopMonitoring();
        }

        public void Dispose() {
            Settings.Dispose();
            Application.Current?.Dispatcher.Invoke(() => {
                Plugins.Clear();
                SnapshotsManager.Clear();
            });

            if(RapportControllerManager != null) {
                RapportControllerManager.PluginLoadedEventArgs -= Rp_PluginLoadedEvent;
                RapportControllerManager.ControllerStatusChanged -= RapportControllerManager_ControllerStatusChanged;
                RapportControllerManager.PluginEnableStatusChangedEvent -= PluginEnableStatusChangedEvent;
                RapportControllerManager.SnapShotEventHandler -= SnapShotEventHandler;

            }

            RapportControllerManager.Dispose();
            RapportControllerManager = null;

            TotalNumberOfPlugins = 0;
            NumberEssentialPlugins = 0;
        }
    }
}
