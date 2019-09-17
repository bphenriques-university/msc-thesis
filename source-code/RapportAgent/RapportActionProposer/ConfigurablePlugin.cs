using HelpersForNet;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.IO;

namespace RapportActionProposer {

    [ConfigurablePluginMetadata(SaveOnDispose = false)]
    public class RCPlugin<T> : RCPlugin, ISettingsProvider where T : class, new() {
        private RCTPluginConfigurationFileManager<T> ConfigurationFileManager;

        public T Settings => ConfigurationFileManager.SettingsObject;
        public bool Loaded => Settings != null;

        public bool SaveOnDispose { get; } = false;

        public RCPlugin() : base() {
            var attributes = (ConfigurablePluginMetadata[])GetType().GetCustomAttributes(typeof(ConfigurablePluginMetadata), true);
            if (attributes.Length > 0)
                SaveOnDispose = attributes[0].SaveOnDispose;            
        }

        public override void Init() {
            base.Init();

            ConfigurationFileManager = new RCTPluginConfigurationFileManager<T>(this);
            Load();
        }

        public override void Dispose() {
            base.Dispose();
            if (SaveOnDispose)
                Save();
        }

        public void Save() {
            LogInfo("Saving " + OptionsFileName + "...");
            ConfigurationFileManager.Save();
        }

        public void Load() {
            LogInfo("Loading " + OptionsFileName + "...");
            ConfigurationFileManager.Load();
        }
    }

    public class RCTPluginConfigurationFileManager<T> where T : class, new() {
        public string FileName => Plugin.OptionsFileName;
        public string OptionsFolderPath => Plugin.OptionFolderPath;
        public string OptionsFilePath => Path.Combine(OptionsFolderPath, FileName);

        private IRCPlugin Plugin { get; }

        public T SettingsObject;

        bool SaveDefaultSettingsIfError { get; set; } = true;

        public RCTPluginConfigurationFileManager(IRCPlugin plugin) {
            this.Plugin = plugin;

            if (!File.Exists(OptionsFilePath))
                plugin.LogWarn("Warning: File not not found at " + OptionsFilePath);           
        }

        public void Save() {
            try {
                XmlUtil<T>.Save(OptionsFilePath, SettingsObject);
            }
            catch (Exception e) {
                Plugin?.LogWarn("Error saving: " + e.Message);
            }
        }

        public void Load() {
            SettingsObject = default(T);
            try {
                SettingsObject = XmlUtil<T>.Load(OptionsFilePath);
            }
            catch (Exception e) {
                Plugin.LogWarn("Load Configuration failed " + e.Message);
                
                if (SaveDefaultSettingsIfError)
                    SetDefaultSettings();                
            }
        }

        public void OpenConfigurationFile(string args = "") {
            ProcessUtil.ExecuteNewProcess(OptionsFilePath, args);
        }

        public void SetDefaultSettings() {
            SettingsObject = new T();
            Save();
        }
    }
}