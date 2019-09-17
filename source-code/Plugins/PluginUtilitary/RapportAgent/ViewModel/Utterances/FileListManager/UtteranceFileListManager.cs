using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel.Utterances.FileListManager {
    public class UtteranceFileListManager : INotifyPropertyChanged, IDisposable {
        public static readonly string UTTERANCE_FILE_EXTENSION = ".csv";

        public AgentActionsManager Plugin { get; }

        //readonly from the file
        private Dictionary<string, UtteranceFile> UtterancesFiles { get; } = new Dictionary<string, UtteranceFile>();
        //from the datagrid
        private ObservableCollection<UtteranceFile> _availableUtterancesFiles = new ObservableCollection<UtteranceFile>();
        public ObservableCollection<UtteranceFile> AvailableUtterancesFiles {
            get { return _availableUtterancesFiles; }
            set {
                _availableUtterancesFiles = value;
                OnPropertyChanged();
            }
        }

        private UtteranceFile _currentSelectedUtteranceFile;
        public UtteranceFile SelectedUtteranceFile {
            get { return _currentSelectedUtteranceFile; }
            set {
                if (_currentSelectedUtteranceFile != null) {
                    _currentSelectedUtteranceFile.Unselect();
                }

                _currentSelectedUtteranceFile = value;
                _currentSelectedUtteranceFile.Select();
                Plugin.Settings.UtteranceFileName = value.Name;

                OnPropertyChanged();
            }
        }
        
        public string UtterancesSourceFolderPath {
            get { return Plugin.Settings.UtterancesFolder; }
            set {                
                string folderPath = value;
                if (!Path.IsPathRooted(folderPath))
                    folderPath = Path.Combine(Plugin.OptionFolderPath, folderPath);                

                if (!Directory.Exists(folderPath))
                    throw new ArgumentException("Cannot find " + folderPath);                

                Plugin.LogInfo("Changing utterances source folder to " + folderPath);
                Plugin.Settings.UtterancesFolder = folderPath;
                OnPropertyChanged();
            }
        }

        internal string GetFile(string fileName) {
            if (UtterancesFiles.ContainsKey(fileName))
                SelectedUtteranceFile = UtterancesFiles[fileName];            
            else if (UtterancesFiles.Count > 0) {
                foreach (var f in UtterancesFiles.Values) {
                    SelectedUtteranceFile = f;
                    break;
                }
            }else
                return "FILE_LOCATION_NOT_FOUND_FOR_" + fileName;

            return SelectedUtteranceFile.Location;
        }

        public UtteranceFileListManager(AgentActionsManager plugin) {
            this.Plugin = plugin;
        }

        public void Reload() {
            Plugin.LogInfo("Reloading list of utterances files from " + UtterancesSourceFolderPath);
            UtterancesFiles.Clear();
            
            foreach (var s in Directory.GetFiles(UtterancesSourceFolderPath, "*" + UTTERANCE_FILE_EXTENSION)) {
                var fileName = Path.GetFileNameWithoutExtension(s);
                var utteranceFile = new UtteranceFile(fileName, s);
                UtterancesFiles.Add(fileName, utteranceFile);
            }
        }

        public void Dispose() {
            /* does nothing */
        }

        public void RefreshGUI() {
            AvailableUtterancesFiles.Clear();
            foreach (var file in UtterancesFiles.Values)
                AvailableUtterancesFiles.Add(file);            
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
