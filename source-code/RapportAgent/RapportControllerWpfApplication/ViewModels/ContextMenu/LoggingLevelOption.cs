using log4net.Core;
using Log4NetWrapperLite;
using System.ComponentModel;
using System.Windows.Media;

namespace RapportControllerWpfApplication.ViewModels.ContextMenu {
    public class LoggingLevelOption : INotifyPropertyChanged {
        private static LoggingLevelOption _selectedLoggingLevel = null;
        public static LoggingLevelOption SelectedLoggingLevel {
            get { return _selectedLoggingLevel; }
            set {
                if (_selectedLoggingLevel != null)
                    _selectedLoggingLevel.Untoggle();
                
                _selectedLoggingLevel = value;
                _selectedLoggingLevel.Toggle();

                Logger.CurrentLogLevel = _selectedLoggingLevel.Log4NetLevel;
            }
        }


        public string Name { get; } = string.Empty;
        public Brush ColorHint { get; }

        private bool _toggled = false;
        public bool Toggled {
            get { return _toggled; }
            set {
                if (value != _toggled) {
                    _toggled = value;
                    if (value)
                        SelectedLoggingLevel = this;
                    
                    OnPropertyChanged("Toggled");
                }
            }
        }

        public Level Log4NetLevel { get; }

        public LoggingLevelOption(string name, Level log4NetLevel, Brush color) {
            this.Name = name;
            this.Log4NetLevel = log4NetLevel;
            this.ColorHint = color;
        }

        public void Toggle() {
            Toggled = true;
        }

        public void Untoggle() {
            Toggled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

}
