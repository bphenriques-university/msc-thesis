using RapportAgentPlugin.ViewModel.Sounds;
using SoundPlayerLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel {
    public class SoundsManagerModel : IDisposable, INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public AgentActionsManager Plugin { get; }
        private SoundPlayerManager SoundPlayer { get; } = new SoundPlayerManager();

        public static readonly string DEFAULT_DIRECTORY_NAME = "SoundLibrary";

        public string SoundsFolderPath {
            get { return Plugin.Settings.SoundsFolder; }
            set {
                Plugin.Settings.SoundsFolder = value;
                OnPropertyChanged();
            }
        }

        //assigned from the datagrid
        private ObservableCollection<SoundFileInfo> _availableSounds = new ObservableCollection<SoundFileInfo>();
        public ObservableCollection<SoundFileInfo> AvailableSounds {
            get { return _availableSounds; }
            set {
                _availableSounds = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, SoundFileInfo> SoundsFilesMap { get; } = new Dictionary<string, SoundFileInfo>(); 

        public SoundsManagerModel(AgentActionsManager plugin) {
            this.Plugin = plugin;
            string soundsFolderPath = Plugin.Settings.SoundsFolder;
            SoundPlayer.SoundFinished += PlaybackStopped;

            if (!Path.IsPathRooted(soundsFolderPath))
                soundsFolderPath = Path.Combine(Plugin.OptionFolderPath, soundsFolderPath);                            

            if (!Directory.Exists(soundsFolderPath)) {
                Plugin.LogWarn("Provided sound folder " + soundsFolderPath + " does not exist. Resetting to default");
                soundsFolderPath = Path.Combine(Plugin.OptionFolderPath, DEFAULT_DIRECTORY_NAME);
            }

            SoundsFolderPath = soundsFolderPath;            
        }

        private void PlaybackStopped(object sender, EventArgs e) {
            SoundPlayer p = sender as SoundPlayer;
            Plugin.ActionFinished(p.Id);
        }

        public void Reload() {
            SoundsFilesMap.Clear();

            if (Directory.Exists(SoundsFolderPath)) {
                string[] soundFiles = Directory.GetFiles(SoundsFolderPath, "*.mp3");
                foreach(var s in soundFiles) {
                    string fileName = Path.GetFileNameWithoutExtension(s);
                    var sound = new SoundFileInfo(fileName, s);
                   
                    SoundsFilesMap.Add(sound.Id, sound);
                }
            }
        }

        public void RefreshGUI() {
            AvailableSounds.Clear();
            foreach(var sound in SoundsFilesMap.Values)
                AvailableSounds.Add(sound);            
        }

        internal string RequestPath(string id) {
            if (SoundsFilesMap.ContainsKey(id))
                return SoundsFilesMap[id].Location;
                        
            throw new Exception("File id named \"" + id + "\" does not exist");            
        }

        public void Dispose() {
            SoundPlayer.SoundFinished -= PlaybackStopped;
            SoundsFilesMap.Clear();
            AvailableSounds.Clear();
        }

        internal void PlaySound(string filePath, string id) {
            SoundPlayer.PlaySound(filePath, id);
        }

        internal void StopSound(string id) {
            SoundPlayer.StopSound(id);
        }
    }
}
