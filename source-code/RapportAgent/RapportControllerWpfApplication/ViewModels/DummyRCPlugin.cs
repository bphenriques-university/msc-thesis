using HelpersForNet;
using Log4NetWrapperLite;
using RapportActionProposer.RCPluginDefinition;
using RapportControllerLib;
using System.ComponentModel;
using System.IO;

namespace RapportControllerWpfApplication.RCWrapper {
    class DummyRCPlugin : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #region fields 
        IRCPlugin original;

        public string Name => original.Name;
        public string Description => original.Description;
        public string Type => original.Type.ToString();

        public bool ProvidesGUI => original.ProvidesGUI;
        public bool CanOpenGUI => Active && ProvidesGUI;

        public bool IsNotEssential => !original.IsEssential;

        public bool ProvidesSettings => original is ISettingsProvider;
        public string OptionsSourceFolder => original.OptionFolderPath;
        public string OptionsFileName => ProvidesSettings ? original.OptionsFileName : null;

        private bool _active;
        public bool Active {
            get { return _active; }
            set {
                _active = value;

                if (value)
                    ErrorInformation = string.Empty;

                OnPropertyChanged("Active");
                OnPropertyChanged("CanOpenGUI");
            }
        }

        private string _errorInformation;
        public string ErrorInformation {
            get { return _errorInformation; }
            set {
                _errorInformation = value;
                ToolTipMessage = !string.IsNullOrEmpty(value) ? "Contains errors: " + value : string.Empty;
                OnPropertyChanged("ContainsErrors");
            }
        }
        public bool ContainsErrors => !string.IsNullOrEmpty(ErrorInformation);
        #endregion

        private string _toolTipMessage;
        public string ToolTipMessage {
            get { return _toolTipMessage; }
            set {
                _toolTipMessage = value;
                OnPropertyChanged("TooltipMessage");
            }
        }

        public DummyRCPlugin(IRCPlugin plugin, bool active) {
            this.original = plugin;
            this._active = active;
            _toolTipMessage = plugin.Description;        
        }

        internal void OpenSettings() {
            Logger.Info(Path.Combine(OptionsSourceFolder, OptionsFileName));
            ProcessUtil.ExecuteNewProcess(Path.Combine(OptionsSourceFolder, OptionsFileName), "");
        }

        public void Dispose() {
            original.Dispose();
            original = null;
        }

        public void Activate(IRapportControllerManager controller) {
            controller.ActivatePlugins(Name);
        }

        public void Desactivate(IRapportControllerManager controller) {
            if(IsNotEssential)
                Active = false;

            controller.DesactivatePlugins(Name);            
        }

        public void ShowGUI() {
            original.ShowGUI();
        }
    }
}
