using log4net.Appender;
using log4net.Core;
using Log4NetWrapperLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using HelpersForNet;
using System.Collections.ObjectModel;
using System.Windows.Media;
using RapportControllerWpfApplication.ViewModels.ContextMenu;

namespace RapportControllerWpfApplication.ViewModels {

    class ConsoleViewManager : IDisposable, INotifyPropertyChanged {
        private Dictionary<string, LoggingLevelOption> DicAvailableLevels { get; } = new Dictionary<string, LoggingLevelOption>();

        public ObservableCollection<LoggingLevelOption> _availableLoggingLevels = new ObservableCollection<LoggingLevelOption>();
        public ObservableCollection<LoggingLevelOption> AvailableLoggingLevels {
            get { return _availableLoggingLevels; }
            set {
                _availableLoggingLevels = value;
                OnPropertyChanged("AvailableLoggingLevels");
            }
        }

        public RichTextBox TextBox { get; internal set; }

        private string _logFolderPath = string.Empty;
        public string LogFolderPath {
            get { return _logFolderPath; }
            set {
                _logFolderPath = value;
                OnPropertyChanged("LogFolderPath");
            }
        }

        internal bool SetLoggingLevel(string level) {
            level = level.UpperCaseFirstLetter();
            if (DicAvailableLevels.ContainsKey(level)) {
                DicAvailableLevels[level].Toggle();
                return true;
            }
            return false;
        }

        public ConsoleViewManager() {
            LogFolderPath = Path.GetDirectoryName(Logger.GetAppender<FileAppender>()?.File ?? string.Empty);

            AddEntry("Debug", Level.Debug, Brushes.White);
            AddEntry("Info", Level.Info, Brushes.Lime);
            AddEntry("Warn", Level.Warn, Brushes.Yellow);
            AddEntry("Error", Level.Error, Brushes.DarkOrange);
            AddEntry("Fatal", Level.Fatal, Brushes.Red);
            AddEntry("Off", Level.Off, Brushes.Transparent);
        }

        private void AddEntry(string levelName, Level level, Brush color) {
            LoggingLevelOption loggingLevel = new LoggingLevelOption(levelName, level, color);

            DicAvailableLevels.Add(loggingLevel.Name, loggingLevel);
            AvailableLoggingLevels.Add(loggingLevel);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose() {
            /* does nothing */
        }
    }
}