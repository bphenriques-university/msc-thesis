using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel.Utterances.VariableSubsitution {
    public class VariableSubtituitionManager : INotifyPropertyChanged, IDisposable {
        public AgentActionsManager Plugin { get; }

        //cached for thalamus
        public HashSet<Tuple<string, string>> DefaultSubstitutions = new HashSet<Tuple<string, string>>();

        //stored
        private Dictionary<string, Substitution> SubstitionDic = new Dictionary<string, Substitution>();

        //gui
        private ObservableCollection<Substitution> _availableSubstitutions = new ObservableCollection<Substitution>();
        public ObservableCollection<Substitution> AvailableSubstitutions {
            get { return _availableSubstitutions; }
            set {
                _availableSubstitutions = value;
                OnPropertyChanged();
            }
        }

        private bool _editModeLocked = true;
        public bool EditModeLocked {
            get { return _editModeLocked; }
            set {
                _editModeLocked = value;

                //when going to state locked, verify and refresh
                if (value) {
                    Save();
                    Reload();
                }

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public VariableSubtituitionManager(AgentActionsManager plugin) {
            this.Plugin = plugin;
        }

        public string Substitute(string original, string variable, string value) {
            var s = new Substitution(variable, value);
            return s.Replace(original);
        }

        public string ReplaceTags(string original, HashSet<Tuple<string, string>> tagsValues = null) {
            string replaced = original;

            //prioritize tagValues over default tag replacement values. The user can basically override

            HashSet<string> alreadyReplacedTags = new HashSet<string>();
            if(tagsValues != null) {
                foreach(var t in tagsValues) {
                    replaced = Substitute(replaced, t.Item1, t.Item2);
                    alreadyReplacedTags.Add(t.Item1);
                }
            }

            foreach(var t in DefaultSubstitutions) {
                if (!alreadyReplacedTags.Contains(t.Item1)) {
                    replaced = Substitute(replaced, t.Item1, t.Item2);
                }
            }

            return replaced;
        }

        internal void Reload() {
            SubstitionDic.Clear();
            DefaultSubstitutions.Clear();
            foreach (var u in Plugin.Settings.AvailableReplacementVariables) {
                string id = u.Key;
                string replacement = u.Value;

                if (!SubstitionDic.ContainsKey(id)) {
                    var s = new Substitution(id, replacement);
                    SubstitionDic.Add(id, s);
                    DefaultSubstitutions.Add(new Tuple<string, string>(id, replacement));
                }
            }
        }

        internal void RefreshGUI() {
            AvailableSubstitutions.Clear();
            foreach (var c in SubstitionDic.Values)
                AvailableSubstitutions.Add(c);
        }

        public void Save() {
            Plugin.LogInfo("Saving list of variables definitions");
            Plugin.Settings.AvailableReplacementVariables.Clear();
            foreach (var c in AvailableSubstitutions)
                if (!Plugin.Settings.AvailableReplacementVariables.ContainsKey(c.VariableName))
                    Plugin.Settings.AvailableReplacementVariables.Add(c.VariableName, c.TargetSubstitution);
            Plugin.Save();
        }

        public void Dispose() {
            //copy all the values to the settings object
            Save();
        }
    }
}
