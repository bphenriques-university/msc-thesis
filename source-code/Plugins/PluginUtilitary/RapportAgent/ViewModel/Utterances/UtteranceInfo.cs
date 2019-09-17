using System;
using RapportAgentPlugin.Utterances;
using System.ComponentModel;
using RapportAgentPlugin.ViewModel.Utterances.VariableSubsitution;
using System.Collections.Generic;
using System.Windows.Data;
using HelpersForNet;
using RapportAgentPlugin.ViewModel.Utterances;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel {
    public class UtteranceInfo : IDataErrorInfo, INotifyPropertyChanged {
        public UtterancesManager UtterancesManager { get; } = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        //failed to use the ivalue converter
        private string _displayedText = "";
        public string DisplayedText {
            get { return _displayedText; }
            set {
                _displayedText = value;
                OnPropertyChanged();
            }
        }

        private bool _shouldShowUtteranceWithTagsReplaced = false;
        public bool ShouldShowUtteranceWithTagsReplaced {
            get { return _shouldShowUtteranceWithTagsReplaced; }
            set {
                _shouldShowUtteranceWithTagsReplaced = value;
                DisplayedText = value ? UtterancesManager.SubstitutionManager.ReplaceTags(Text, null) : Text;

                OnPropertyChanged();
            }
        }

        public string ReplacedTextWithDefaultTags { get; set; } = "ReplacedUtterance";
        public ushort Priority { get; set; } = 0;

        private string _text = "Utterance";
        public string Text {
            get { return _text; }
            set {
                _text = value;
                OnPropertyChanged();
            }
        }

        private int _initialDelay = 0;
        public int InitialDelay {
            get { return _initialDelay; }
            set {
                _initialDelay = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        private int _timeoutMs = 30000;
        public int TimeOutMs {
            get { return _timeoutMs; }
            set {
                _timeoutMs = Math.Max(0, value);
                OnPropertyChanged();
            }
        }

        private bool _validUtterance = true;
        public bool ValidUtterance {
            get { return _validUtterance; }
            set {
                _validUtterance = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public UtteranceInfo() {
            Text = "Utterance";
            ShouldShowUtteranceWithTagsReplaced = false;
        }

        public UtteranceInfo(UtteranceSpecification u) {
            this.Text = u.Text;
            this.InitialDelay = u.InitialDelay;
            this.TimeOutMs = u.TimeOutMs;
            this.Priority = u.Priority;

            ShouldShowUtteranceWithTagsReplaced = false;
        }

        public string Error { get { throw new NotImplementedException(); } }
        public string this[string columnName] {
            get {
                string errorMsg = string.Empty;
                ValidUtterance = true;
                switch (columnName) {
                    case "Text":
                        errorMsg = string.Empty;//UtteranceActionProposalGenerator.Validate(Text); removed until improvements/optimizations are made
                        break;
                }

                ValidUtterance = string.IsNullOrEmpty(errorMsg);

                return errorMsg;
            }
        }

        public void ApplyTagReplacement(VariableSubtituitionManager substitutionManager) {
            ReplacedTextWithDefaultTags = substitutionManager.ReplaceTags(Text, null);
        }

        public Utterance Load(VariableSubtituitionManager substitutionManager, UtteranceActionProposalGenerator parser, ushort priority, HashSet<Tuple<string, string>> tagsValues) {
            ReplacedTextWithDefaultTags = substitutionManager.ReplaceTags(Text, tagsValues);
            try {
                return parser.GenerateActionProposal(priority, ReplacedTextWithDefaultTags, TimeOutMs);
            }catch(Exception e) {
                substitutionManager.Plugin.LogError(e.Message);
            }

            return null;
        }
    }
}