using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RapportAgentPlugin.ViewModel.Utterances.VariableSubsitution {
    public class Substitution : INotifyPropertyChanged {

        private Regex FindRegex { get; }

        private string _variableName = "";
        public string VariableName {
            get { return _variableName; }
            set {
                _variableName = value;
                OnPropertyChanged();
            }
        }

        private string _targetSubstitution = "Replacement";
        public string TargetSubstitution {
            get { return _targetSubstitution; }
            set {
                _targetSubstitution = value;
                OnPropertyChanged();
            }
        }

        public Substitution() : this(Guid.NewGuid().ToString(), "Replacement") { }
        public Substitution(string variable, string substitution){
            this.VariableName = variable;
            this.TargetSubstitution = substitution;
            this.FindRegex = new Regex(@"<\s*" + VariableName + @"\s*>");
        }

        public string Replace(string original) {
            return FindRegex.Replace(original, TargetSubstitution);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
