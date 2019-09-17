using RapportAgentPlugin.ViewModel.Utterances.FileListManager;
using RapportAgentPlugin.ViewModel.Utterances.VariableSubsitution;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel.Utterances {
    public class UtterancesManager : INotifyPropertyChanged, IDisposable {
        public static readonly string DEFAULT_UTTERANCES_FILE_NAME = "Utterances";
        public static readonly string DEFAULT_DIRECTORY_NAME = "Utterances";

        public AgentActionsManager Plugin { get; }
        public UtteranceFileListManager FileListManager { get; }
        public UtterancesFileManager FileManager { get; }
        public VariableSubtituitionManager SubstitutionManager { get; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void ChangeUtteranceFile(string location) {
            FileManager.UtteranceFilePath = FileListManager.GetFile(Path.GetFileNameWithoutExtension(location));            
            FileManager.Reload();
        }

        public UtterancesManager(AgentActionsManager plugin) {
            this.Plugin = plugin;
            FileListManager = new UtteranceFileListManager(plugin);
            FileManager = new UtterancesFileManager(plugin);
            SubstitutionManager = new VariableSubtituitionManager(plugin);
        }

        public void Reload() {
            Plugin.LogInfo("Reloading everything Utterances related");
            string folderPath = FileListManager.UtterancesSourceFolderPath;
            if (!Directory.Exists(folderPath)) {
                Plugin.LogError("Cannot read folder path as " + folderPath + ". Setting to default values.");
                folderPath = Path.Combine(Plugin.OptionFolderPath, DEFAULT_DIRECTORY_NAME);
                FileListManager.UtterancesSourceFolderPath = folderPath;
            }
            FileListManager.Reload();

            //get path for selected path and reload
            FileManager.UtteranceFilePath = FileListManager.GetFile(Plugin.Settings.UtteranceFileName);
            FileManager.Reload();
            SubstitutionManager.Reload();
        }

        public void SetUtterancesFolder(string folderPath) {
            try {
                FileListManager.UtterancesSourceFolderPath = folderPath;
            }
            catch(Exception) {
                Plugin.LogError("Cannot set folder path as " + folderPath);
            }

            Reload();
        }

        public void RefreshGUI() {
            RefreshUtterancesGUI();
            SubstitutionManager.RefreshGUI();
        }

        public void RefreshUtterancesGUI() {
            FileListManager.RefreshGUI();
            FileManager.RefreshGUI();
        }

        public void Dispose() {
            FileManager.Dispose();
        }
    }
}
