using System.ComponentModel;

namespace RapportControllerWpfApplication.ViewModels.ContextMenu {
    public class RecordOptionTime : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private static RecordOptionTime _selectedRecordTime = null;
        public static RecordOptionTime SelectedRecordingTime {
            get { return _selectedRecordTime; }
            set {
                if (_selectedRecordTime != null)
                    _selectedRecordTime.Untoggle();

                _selectedRecordTime = value;
                _selectedRecordTime.Toggle();
            }
        }

        public string DisplayedText { get; }
        public int Time { get; }

        private bool _selected = false;
        public bool Selected {
            get { return _selected; }
            set {
                if (value != _selected) {
                    _selected = value;
                    if (value)
                        SelectedRecordingTime = this;
                    
                    OnPropertyChanged("Toggled");
                }

                OnPropertyChanged("Selected");
            }
        }

        public void Toggle() {
            Selected = true;
        }

        public void Untoggle() {
            Selected = false;
        }

        public RecordOptionTime(string text, int time) {
            this.DisplayedText = text;
            this.Time = time;
        }
    }
}
