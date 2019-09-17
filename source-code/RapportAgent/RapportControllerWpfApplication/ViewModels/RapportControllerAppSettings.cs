using System;
using System.ComponentModel;

namespace RapportControllerWpfApplication.ViewModels {
    class RapportControllerAppSettings : INotifyPropertyChanged, IDisposable {
        #region fields

        public string DefaultPluginsPath { get; private set; }
        private string _pluginsFolderPath;
        public string PluginsFolderPath {
            get { return _pluginsFolderPath; }
            set {
                _pluginsFolderPath = value;
                OnPropertyChanged("PluginsFolderPath");
            }
        }

        public string DefaultOptionsPath { get; private set; }
        private string _optionsFolderPath;
        public string OptionsFolderPath {
            get { return _optionsFolderPath; }
            set {
                _optionsFolderPath = value;
                OnPropertyChanged("OptionsFolderPath");
            }
        }     

        public int DefaultControllerFrequency { get; private set; } = 5;
        private int _controllerFrequency = 5;
        public int ControllerFrequency {
            get { return _controllerFrequency; }
            set {
                _controllerFrequency = value;
                OnPropertyChanged("ControllerFrequency");
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        internal void SetDefaultPluginsFolder(string pluginsPath) {
            DefaultPluginsPath = pluginsPath;
            PluginsFolderPath = DefaultPluginsPath;
        }

        internal void SetDefaultOptionsPath(string optionsPath) {
            DefaultOptionsPath = optionsPath;
            OptionsFolderPath = DefaultOptionsPath;
        }

        public void ResetToDefault() {
            PluginsFolderPath = DefaultPluginsPath;
            OptionsFolderPath = DefaultOptionsPath;
            ControllerFrequency = DefaultControllerFrequency;
        }

        internal void SetDefaultControllerFrequency(int controllerFrequency) {
            DefaultControllerFrequency = controllerFrequency;
            ControllerFrequency = DefaultControllerFrequency;
        }

        public void SetDefaultSettings(string pluginsPath, string optionsPath, int frequency) {
            ResetToDefault();
        }

        public void Dispose() {
            /* does nothing*/
        }

        internal void SetLoggingLevel(string logLevel) {
            /* does nothing for now */
        }
    }
}