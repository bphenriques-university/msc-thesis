using HelpersForNet;
using Log4NetWrapperLite;
using RapportActionProposer;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace RapportControllerLib {

    public class PluginsManagerSettings {
        public class PluginSetting {
            public string Name { get; set; }
            public bool Enable { get; set; } = false;

            public PluginSetting() { }
            public PluginSetting(string name) : this(name, false) { }
            public PluginSetting(string name, bool enable) {
                this.Name = name;
                this.Enable = enable;
            }
        }

        public SerializableDictionary<string, PluginSetting> PluginsEnableStatus { get; set; } = new SerializableDictionary<string, PluginSetting>();

        public bool IsPluginActive(string id) {
            return ContainsPlugin(id) ? PluginsEnableStatus[id].Enable : false;
        }

        public void SetEnableStatus(string id, bool value) {
            if (!ContainsPlugin(id))
                throw new ArgumentException("Cant change status for id " + id + " as it was not found");
            
            PluginsEnableStatus[id].Enable = value;
        }

        public bool ContainsPlugin(string id) {
            return PluginsEnableStatus.ContainsKey(id);
        }

        public void CreatePluginSettings(string id, bool initialEnableStatus) {
            var setting = new PluginsManagerSettings.PluginSetting(id, initialEnableStatus);
            PluginsEnableStatus.Add(setting.Name, setting);
        }
    }

    public class PluginsManager : IDisposable {
        #region Fields and properties

        //potentially contains repeated elements
        [ImportMany(typeof(IRCPlugin))]
        public IEnumerable<IRCPlugin> pluginsEnumerable;

        public string PluginsPath { get; }
        public int NumberOfPlugins => plugins.Count;
        public string OptionsFolderPath { get; }
        public string ActivePluginsConfigurationFilePath => Path.Combine(OptionsFolderPath, "Plugins.xml");

        private Dictionary<string, IRCPlugin> plugins = new Dictionary<string, IRCPlugin>();
        
        public event EventHandler<PluginLoadedEventArgs> PluginLoadedEvent = delegate { };
        public event EventHandler<PluginEnableStatusChangedEventArgs> PluginEnableStatusChangedEvent = delegate { };

        private PluginsManagerSettings Settings { get; set; }
        #endregion

        public PluginsManager(string pluginsPath, string optionsFolderPath) {
            this.PluginsPath = pluginsPath;
            this.OptionsFolderPath = optionsFolderPath;

            if (!Directory.Exists(PluginsPath))
                throw new Exception("There is no such folder at " + ActivePluginsConfigurationFilePath);
        }

        private void LoadPluginsFromCatalog() {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Adds all the parts found in all assemblies in the same directory as the executing program
            // iterate over all directories in Plugins dir and add all Plugin* dirs to catalogs
            catalog.Catalogs.Add(new DirectoryCatalog(PluginsPath));
            foreach (var path in Directory.EnumerateDirectories(PluginsPath, "*", SearchOption.TopDirectoryOnly)) {
                if (Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)).StartsWith("_"))
                    continue;                
                catalog.Catalogs.Add(new DirectoryCatalog(path));
            }

            try {
                //Create the CompositionContainer with the parts in the catalog
                CompositionContainer container = new CompositionContainer(catalog);
                //Fill the imports of this object
                container.ComposeParts(this);
            }
            catch (Exception e) {
                Logger.Error("Error reading plugin due to: " + e.Message);
                if (e is ReflectionTypeLoadException) {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    throw new AggregateException(typeLoadException.LoaderExceptions);
                }
            }
        }

        private void AddDebugPlugins() {
            /*
            var currentPlugins = new List<IRCPlugin>(pluginsEnumerable);
            currentPlugins.Add(new (...));
            pluginsEnumerable = currentPlugins;
            */
        }

        internal void ImportAvailablePlugins() {
            LoadPluginsFromCatalog();

            AddDebugPlugins();

            //ignore list set to false
            try {
                Logger.Info("Loading Plugins.xml: List of plugins that are enabled");
                Settings = XmlUtil<PluginsManagerSettings>.Load(ActivePluginsConfigurationFilePath);
            }
            catch (Exception e) {
                Logger.Warn("Error reading Plugins.xml due to: " + e.Message + ". Recreating...");
                Settings = new PluginsManagerSettings();
                Save();                
            }
            
            if(pluginsEnumerable == null)
                throw new Exception("Error reading plugin list due to error when reading plugins. Remove the plugin one by one to find the culprit.");
                       

            //populate new plugins with default value true
            foreach (var p in pluginsEnumerable.Where((IRCPlugin pl) => !plugins.ContainsKey(pl.Id)))  {
                //skips already loaded dlls
                string id = p.Id;
                Logger.Info("Importing " + id);
                plugins.Add(id, p);

                //add new setting if there isn't one for newly arrived plugins
                if (!Settings.ContainsPlugin(id))
                    Settings.CreatePluginSettings(id, true);                
            }
        }

        internal void StartPlugins() {
            foreach (var plugin in plugins.Values.Where((IRCPlugin pl) => IsPluginActive(pl.Id)))
                StartPlugin(plugin);            
        }

        public void InitializePlugins(IRapportController controller) {
            var alreadySentEvent = new HashSet<string>();

            foreach (var plugin in plugins.Values) {
                plugin.Configure(controller, OptionsFolderPath);

                if (IsPluginActive(plugin.Id) || plugin.IsEssential) {
                    try { InitializePlugin(plugin); }
                    catch (Exception e) {
                        string errorMsg = FailureHandlerException(plugin, "", true, e).Message;
                        var args = new PluginLoadedEventArgs(plugin, errorMsg, false);
                        foreach (EventHandler<PluginLoadedEventArgs> receiver in PluginLoadedEvent.GetInvocationList())
                            receiver.BeginInvoke(this, args, null, null);
                        alreadySentEvent.Add(plugin.Id);
                    }
                }
            }

            foreach (var plugin in plugins.Values) {
                string errorMsg = "";
                bool enabled = IsPluginActive(plugin.Id);

                if (enabled) {
                    try { InitializeDependencies(plugin); }
                    catch (Exception e) { errorMsg = FailureHandlerException(plugin, "InitDependencies", enabled, e).Message; }
                }

                if (!alreadySentEvent.Contains(plugin.Id)) {
                    enabled = IsPluginActive(plugin.Id);
                    var args = new PluginLoadedEventArgs(plugin, errorMsg, enabled);
                    foreach (EventHandler<PluginLoadedEventArgs> receiver in PluginLoadedEvent.GetInvocationList())
                        receiver.BeginInvoke(this, args, null, null);
                }
            }
        }

        internal void PausePlugins() {
            foreach (var plugin in plugins.Values.Where((IRCPlugin pl) => IsPluginActive(pl.Id)))
                PausePlugin(plugin);            
        }

        public bool IsPluginActive(string id) {
            return Settings.IsPluginActive(id);
        }

        private void StartPlugin(IRCPlugin plugin) {
            Logger.Info("[" + plugin.Id + "] Starting");
            plugin.Start();
        }

        private void InitializeDependencies(IRCPlugin plugin) {
            Logger.Info("[" + plugin.Id + "] InitDependencies");
            plugin.InitDependencies();

            Settings.SetEnableStatus(plugin.Id, true);
            var args = new PluginEnableStatusChangedEventArgs(plugin, true);
            foreach (EventHandler<PluginEnableStatusChangedEventArgs> receiver in PluginEnableStatusChangedEvent.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void InitializePlugin(IRCPlugin plugin) {
            Logger.Info("[" + plugin.Id + "] Init");
            plugin.Init();
        }

        private void DesactivatePlugin(IRCPlugin plugin, string errorMessage = "") {
            var id = plugin.Id;
            Logger.Info("[" + id + "] Disposing");

            //when disabling, send a event preemtively 
            var args = new PluginEnableStatusChangedEventArgs(plugin, false, errorMessage);
            foreach (EventHandler<PluginEnableStatusChangedEventArgs> receiver in PluginEnableStatusChangedEvent.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);

            Settings.SetEnableStatus(plugin.Id, false);
            plugin.Dispose();
        }

        public void SetPluginStatus(string id, bool targetEnabled, bool startPlugin) {
            if (targetEnabled == IsPluginActive(id))
                return;

            var plugin = plugins[id];
            if (targetEnabled) {                
                try { InitializePlugin(plugin); }
                catch(Exception e) { FailureHandlerException(plugin, "Init",  targetEnabled, e); return; }

                try { InitializeDependencies(plugin); }
                catch (Exception e) { FailureHandlerException(plugin, "InitDependencies", targetEnabled, e); return; }

                if (startPlugin) {
                    try { StartPlugin(plugin); }
                    catch (Exception e) { FailureHandlerException(plugin, "Start", targetEnabled, e); }
                }
            } else {
                if (!plugin.IsEssential) {
                    try { PausePlugin(plugin); }
                    catch (Exception e) { FailureHandlerException(plugin, "Pause", targetEnabled, e, false); }

                    try { DesactivatePlugin(plugin); }
                    catch (Exception e) { FailureHandlerException(plugin, "Desactivate", targetEnabled, e, false); }
                }
            }
        }

        private void PausePlugin(IRCPlugin plugin) {
            Logger.Info("[" + plugin.Id + "] Pausing");
            plugin.Pause();
        }

        private Exception FailureHandlerException(IRCPlugin plugin, string message, bool targetEnabled, Exception ex, bool desactivate=true) {
            string msg = "Failed to " + message + " while setting " + plugin.Id + " status to " + targetEnabled + " disabling it due to: " + ex.Message;
            Logger.Error(msg);

            if (desactivate) {
                try { DesactivatePlugin(plugin, msg); }
                catch (Exception) { /*intentionally ignore */ }
            }
                        
            return new Exception(msg);
        }

        public T RetrievePlugin<T>(string id) where T : IRCPlugin {
            if (!IsPluginActive(id))
                throw new Exception("Requested plugin " + id + " is not active");
            
            var plugin = plugins.ContainsKey(id) ? plugins[id] : null;
            if(plugin == null)
                throw new Exception("Requested plugin " + id + " does not exist");            

            return (T)plugin;
        }

        private void Save() {
            XmlUtil<PluginsManagerSettings>.Save(ActivePluginsConfigurationFilePath, Settings);
        }

        public void Dispose() {
            Logger.Debug("Saving active plugins configuration");
            Save();

            foreach(var plugin in plugins.Values.Where((IRCPlugin p) => this.IsPluginActive(p.Id))) {
                Task.Factory.StartNew(() => {
                    Logger.Info("[" + plugin.Id + "] Disposing");
                    try { plugin.Dispose(); }
                    catch (Exception e) {
                        Logger.Error("ERROR DISPOSING " + plugin.Id + " due to: " + e.Message + "\n" + e.StackTrace);
                    }
                });
            }
        }       
    }
}
