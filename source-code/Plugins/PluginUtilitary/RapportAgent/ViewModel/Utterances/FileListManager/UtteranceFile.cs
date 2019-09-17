using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RapportAgentPlugin.ViewModel.Utterances {
    public class UtteranceFile : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name { get; }
        public string Location { get; }

        private bool _selected = false;
        public bool Selected {
            get { return _selected; }
            set {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public void Select() {
            Selected = true;
        }

        public void Unselect() {
            Selected = false;
        }

        public UtteranceFile(string name, string location) {
            this.Name = name;
            this.Location = location;
        }
    }
}