using Log4NetWrapperLite;
using RapportActionProposer;
using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.IO;
using System.Threading;

namespace RapportControllerLib {
    public class RapportProposerController : IRapportControllerManager {
        #region fields

        public ActionProposersManager apm = new ActionProposersManager();

        private PluginsManager PluginsManager { get; }
        public string PluginsPath { get; }
        public string OptionsFolderPath { get; }

        public int NumberOfPlugins => PluginsManager.NumberOfPlugins;

        private Thread monitorThread;
        public bool IsRunning => CurrentStatus == ControllerStatus.Running;
        private bool shutdownFlag = false;

        private ControllerStatus _currentStatus = ControllerStatus.Disposed;
        private ControllerStatus CurrentStatus {
            get { return _currentStatus; }
            set {
                if(value != _currentStatus) {
                    Logger.Info("[ - " + value.ToString() + " - ] Rapport Controller");
                    _currentStatus = value;
                    var statusChangedArgs = new ControllerStatusChangedEventArgs(_currentStatus);
                    foreach (EventHandler<ControllerStatusChangedEventArgs> receiver in ControllerStatusChanged.GetInvocationList())
                        receiver.BeginInvoke(this, statusChangedArgs, null, null);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<ControllerStatusChangedEventArgs> ControllerStatusChanged = delegate { };
        public event EventHandler<PluginEnableStatusChangedEventArgs> PluginEnableStatusChangedEvent {
            add { PluginsManager.PluginEnableStatusChangedEvent += value; }
            remove { PluginsManager.PluginEnableStatusChangedEvent -= value; }
        }

        public event EventHandler<SnapshotEventEventArgs> SnapShotEventHandler {
            add { apm.SnapShotEventHandler += value; }
            remove { apm.SnapShotEventHandler -= value; }
        }

        public event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutingActionGroup {
            add { apm.ExecutingActionGroup += value; }
            remove { apm.ExecutingActionGroup -= value; }
        }

        public event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutedActionGroup {
            add { apm.ExecutedActionGroup += value; }
            remove { apm.ExecutedActionGroup -= value; }
        }

        public event EventHandler<ActionGroupsStatusChangedEventArgs> InterruptedActionGroup {
            add { apm.InterruptedActionGroup += value; }
            remove { apm.InterruptedActionGroup -= value; }
        }

        public event EventHandler<PluginLoadedEventArgs> PluginLoadedEventArgs {
            add { PluginsManager.PluginLoadedEvent += value; }
            remove { PluginsManager.PluginLoadedEvent -= value; }
        }

        #endregion

        public RapportProposerController(string pluginsPath, string optionsFolderPath){
            CurrentStatus = ControllerStatus.Initializing;

            if (!Directory.Exists(pluginsPath))
                throw new FileNotFoundException("Couldn't find plugins folder at: " + pluginsPath);            
            if (!Directory.Exists(optionsFolderPath))
                throw new FileNotFoundException("Couldn't find options folder at: " + optionsFolderPath);

            Logger.Info("\t Plugins Path: " + pluginsPath);
            Logger.Info("\t Options Path: " + optionsFolderPath);

            PluginsPath = pluginsPath;
            OptionsFolderPath = optionsFolderPath;
            PluginsManager = new PluginsManager(pluginsPath, optionsFolderPath);            
        }

        public int ImportPlugins() {
            CurrentStatus = ControllerStatus.Importing;

            //import plugins and initialize components
            PluginsManager.ImportAvailablePlugins();
            PluginsManager.InitializePlugins(this);

            CurrentStatus = ControllerStatus.Ready;

            return PluginsManager.NumberOfPlugins;
        }

        public void ProposeAction(IActionProposal proposal){
            if (IsRunning && proposal != null && PluginsManager.IsPluginActive(proposal.ProposerId))
                apm.AddAction(proposal);            
        }

        private object refreshRateTimeLock = new object();
        public bool StartMonitoring(int frequency) {
            if (IsRunning) {
                Logger.Error("Failed to start monitor because it is already running");
                return false;
            }

            Logger.Warn("Started monitorization with frequency " + frequency + " Hz");
            int msBetweenSnapshots = 1000 / frequency;
            shutdownFlag = false;
            monitorThread = new Thread(() => {
                while (!shutdownFlag) {
                    //capture snapshot and executing snapshot
                    try {
                        IActionsSnapshot currentSnapshot = apm.CaptureSnapshot();
                        currentSnapshot.Execute();
                    }catch(AggregateException e) {
                        Logger.Fatal("FATAL ERROR WHEN CAPTURING AND EXECUTING SNAPSHOT. Unexpected: " + e.Message);
                        Logger.Fatal(e.StackTrace);
                        foreach(var ex in e.InnerExceptions) {
                            Logger.Fatal("\t" + e.Message);
                        }
                    }catch(Exception e) {
                        Logger.Fatal("FATAL ERROR WHEN CAPTURING AND EXECUTING SNAPSHOT. Unexpected: " + e.Message);
                        Logger.Fatal(e.StackTrace);
                    }

                    //wait                    
                    lock (refreshRateTimeLock) { Monitor.Wait(refreshRateTimeLock, TimeSpan.FromMilliseconds(msBetweenSnapshots)); }
                }
            });
            monitorThread.IsBackground = true;
            monitorThread.Start();

            PluginsManager.StartPlugins();

            CurrentStatus = ControllerStatus.Running;

            return true;
        }

        public void StopMonitoring() {
            if (monitorThread != null) {
                shutdownFlag = true;
                monitorThread.Join();
                monitorThread = null;
            }

            PluginsManager.PausePlugins();

            CurrentStatus = ControllerStatus.Paused;
        }

        public void ActivatePlugins(params string[] ids) {
            SetStatusAux(true, ids);
        }

        private void SetStatusAux(bool targetStatus, params string[] ids) {
            foreach (string id in ids) {
                Logger.Info((targetStatus ? "[Activating] " : "[Desactivating] ") + id);
                PluginsManager.SetPluginStatus(id, targetStatus, IsRunning);
            }
        }

        public void DesactivatePlugins(params string[] ids) {
            SetStatusAux(false, ids);
        }

        public void ActionFinished(string id) {
            Logger.Debug("Action " + id + " has finished");
            apm.ActionFinished(id);
        }

        public void InterruptAction(string id) {
            apm.InterruptAction(id);
        }

        public void Dispose() {
            CurrentStatus = ControllerStatus.Disposing;

            if (monitorThread != null) {
                shutdownFlag = true;
                monitorThread.Join();
                monitorThread = null;
            }

            if (PluginsManager != null)
                PluginsManager.Dispose();

            CurrentStatus = ControllerStatus.Disposed;
        }

        public T GetPlugin<T>() where T : IRCPlugin {
            return PluginsManager.RetrievePlugin<T>(typeof(T).Name);
        }

        public bool IsPluginActive(string name) {
            return PluginsManager.IsPluginActive(name);
        }

        #region Logging 
        public void LogInfo(IRCPlugin proposer, string msg) {
            Logger.Info("\t[" + proposer.Id + "] - " + msg);
        }

        public void LogError(IRCPlugin proposer, string msg) {
            Logger.Error("\t[" + proposer.Id + "] - " + msg);
        }

        public void LogWarn(IRCPlugin proposer, string msg) {
            Logger.Warn("\t[" + proposer.Id + "] - " + msg);
        }

        public void LogDebug(IRCPlugin proposer, string msg) {
            Logger.Debug("\t[" + proposer.Id + "] - " + msg);
        }

        public void LogFatal(IRCPlugin proposer, string msg) {
            Logger.Fatal("\t[" + proposer.Id + "] - " + msg);
        }
        #endregion
    }
}