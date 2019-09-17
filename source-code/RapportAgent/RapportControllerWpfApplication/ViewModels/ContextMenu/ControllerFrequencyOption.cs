using HelpersForNet;
using System.ComponentModel;

namespace RapportControllerWpfApplication.ViewModels.ContextMenu {
    public class ControllerFrequencyOption : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private static RapportControllerAppSettings Settings = Singleton<RapportControllerAppSettings>.Instance;

        private static ControllerFrequencyOption _selectedFrequency = null;
        public static ControllerFrequencyOption SelectedFrequency {
            get { return _selectedFrequency; }
            set {
                if (_selectedFrequency != null) {
                    _selectedFrequency.Untoggle();
                }

                _selectedFrequency = value;
                _selectedFrequency.Toggle();
            }
        }

        //does not work in wpf
        public string DisplayedText => string.Format("{0} Hz", Frequency);
        public int Frequency { get; }

        private bool _selected = false;
        public bool Selected {
            get { return _selected; }
            set {
                if (value != _selected) {
                    _selected = value;
                    if (value)
                        SelectedFrequency = this;
                    
                    OnPropertyChanged("Toggled");
                }

                OnPropertyChanged("Selected");
            }
        }

        public void Toggle() {
            Selected = true;
            Settings.ControllerFrequency = Frequency;
        }

        public void Untoggle() {
            Selected = false;
        }

        public ControllerFrequencyOption(int frequency) {
            this.Frequency = frequency;
        }
    }
}