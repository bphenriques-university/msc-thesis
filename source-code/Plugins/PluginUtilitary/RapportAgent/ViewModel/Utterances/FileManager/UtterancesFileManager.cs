using RapportActionProposer.RCPluginDefinition;
using System.Collections.Generic;
using RapportAgentPlugin.Utterances;
using RapportAgentPlugin.ViewModel;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System;
using System.Runtime.CompilerServices;
using System.IO;

namespace RapportAgentPlugin {
    public class UtterancesFileManager : INotifyPropertyChanged, IDisposable {
        #region Variables and Properties

        public AgentActionsManager Plugin { get; }

        //readonly from the file
        public Dictionary<string, UtteranceCategory> CategoriesIds = new Dictionary<string, UtteranceCategory>();
        //assigned from the datagrid
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();
        public ObservableCollection<Category> Categories {
            get { return _categories; }
            set {
                _categories = value;
                OnPropertyChanged();
            }
        }

        private bool _editModeLocked = true;
        public bool EditModeLocked {
            get { return _editModeLocked; }
            set {
                _editModeLocked = value;
                OnPropertyChanged();

                if (value) {
                    Save(); //when going to state locked, verify and refresh
                    Reload();
                }
            }
        }

        private string _utteranceFilePath = "";
        public string UtteranceFilePath {
            get { return _utteranceFilePath; }
            set {
                if (!value.Equals(_utteranceFilePath)) {
                    _utteranceFilePath = value;                    
                    OnPropertyChanged();
                }
            }
        }
        
        #endregion

        public UtterancesFileManager(AgentActionsManager plugin) {
            this.Plugin = plugin;            
        }
        
        public UtteranceInfo RetrieveUtterance(IRCPlugin plugin, string category, string subCategory) {
            if (!CategoriesIds.ContainsKey(category)) {
                plugin.LogError(category + ":*" + " does not exist.");
                return null;
            }

            var cat = CategoriesIds[category];
            var subCat = cat.GetSubCategory(subCategory);
            if(subCat == null) {
                plugin.LogError(category + ":" + subCategory + " does not exist.");
                return null;
            }

            var spec = subCat.GetRandomUtterance();
            if(spec == null) {
                plugin.LogError("Requested SubCategory " + category + " does not contain any utterance");
                return null;
            }

            return spec;
        }

        List<UtteranceSpecification> storedUtterances;
        public void Reload() {
            string filePath = UtteranceFilePath;

            Plugin.LogInfo("Loading utterances from " + filePath);
            CategoriesIds.Clear();
            if (!File.Exists(filePath)) {
                Plugin.LogError("File " + filePath + " does not exist.");
                return;
            }

            CSVUtterancesReader u = new CSVUtterancesReader();
            storedUtterances = u.ReadCSV(Plugin, filePath);
            ReloadUtterancesCategoriesFromMemory();
        }

        private void ReloadUtterancesCategoriesFromMemory() {
            foreach (var utterance in storedUtterances) {
                if (!CategoriesIds.ContainsKey(utterance.Category)) {
                    UtteranceCategory cat = new UtteranceCategory(utterance.Category);

                    UtteranceSubCategory subCat = new UtteranceSubCategory(utterance.SubCategory);
                    var info = new UtteranceInfo(utterance);
                    subCat.Utterances.Add(new UtteranceInfo(utterance));

                    cat.SubCategories.Add(subCat.SubCategory, subCat);
                    CategoriesIds.Add(cat.Category, cat);
                }
                else {
                    var cat = CategoriesIds[utterance.Category];
                    var subCat = cat.GetSubCategory(utterance.SubCategory);
                    if (subCat == null) {
                        subCat = new UtteranceSubCategory(utterance.SubCategory);
                        subCat.Utterances.Add(new UtteranceInfo(utterance));
                        cat.SubCategories.Add(subCat.SubCategory, subCat);
                    }
                    else {
                        subCat.Utterances.Add(new UtteranceInfo(utterance));
                    }
                }
            }
        }

        public void RefreshGUI() {
            Plugin.LogInfo("Refreshing Categories");
            Categories.Clear();
            foreach (var c in CategoriesIds.Values)
                Categories.Add(new Category(c));
        }

        internal void Save() {
            string filePath = UtteranceFilePath;

            Plugin.LogInfo("Saving current utterances file at " + filePath);
            CSVUtterancesReader u = new CSVUtterancesReader();
            u.Save(Plugin, new List<Category>(Categories), filePath);
        }

        public void Dispose() {
            /* does nothing */
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}