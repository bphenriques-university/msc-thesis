using RapportActionProposer.Util;
using System;
using System.Windows;

namespace RapportActionProposer.RCPluginDefinition {

    public abstract class RCPlugin : IRCPlugin {
        public IRapportController RapportController { get; private set; }
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsEssential { get; }

        public virtual PluginType Type => PluginType.Utilitary;

        public Type windowType = null;
        public bool ProvidesGUI => windowType != null;

        public string OptionFolderPath { get; private set; }
        public string OptionsFileName => Id + ".xml";

        public RCPlugin() {
            Id = GetType().Name;

            //default values
            Name = GetType().Name;
            Description = GetType().Name + " - Description";

            var rcPluginMetadataAttributes = (RCPluginMetadata[])GetType().GetCustomAttributes(typeof(RCPluginMetadata), true);
            if (rcPluginMetadataAttributes.Length > 0) {
                var name = rcPluginMetadataAttributes[0].Name;
                var attributeDescription = rcPluginMetadataAttributes[0].Description;
                Type attributeWindowType = rcPluginMetadataAttributes[0].WindowType;

                if (!string.IsNullOrEmpty(name))
                    Name = name;

                Description = attributeDescription;
                IsEssential = rcPluginMetadataAttributes[0].IsEssential;

                if (attributeWindowType != null && attributeWindowType.IsSubclassOf(typeof(RCPluginWindow)))
                    windowType = attributeWindowType;
            }
        }

        public void Configure(IRapportController controller, string optionsFolder) {
            RapportController = controller;
            OptionFolderPath = optionsFolder;
        }

        public void LogInfo(string msg) {
            RapportController.LogInfo(this, msg);
        }

        public void LogWarn(string msg) {
            RapportController.LogWarn(this, msg);
        }

        public void LogError(string msg) {
            RapportController.LogError(this, msg);
        }

        public void LogFatal(string msg) {
            RapportController.LogFatal(this, msg);
        }

        public void LogDebug(string msg) {
            RapportController.LogDebug(this, msg);
        }

        protected RCPluginWindow openedWindow = null;
        public void ShowGUI() {
            if (ProvidesGUI) {
                if(openedWindow == null) {
                    var constuctor = windowType.GetConstructor(System.Type.EmptyTypes);
                    openedWindow = constuctor.Invoke(null) as RCPluginWindow;
                }

                Application.Current.Dispatcher.Invoke(openedWindow.Show);
            }
        }

        public virtual void Dispose() {
            Application.Current?.Dispatcher.Invoke(() => {
                if (openedWindow != null) {
                    openedWindow.ShouldHide = false;
                    openedWindow.Close();
                }

                openedWindow = null;
            });
        }

        public virtual void Init() { /* does nothing */ }
        public virtual void InitDependencies() { /* does nothing */ }
        public virtual void Start() { /* does nothing */ }
        public virtual void Pause() { /* does nothing */ }
    }
}