using HelpersForNet;
using Log4NetWrapperLite;
using RapportControllerWpfApplication.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace RapportControllerWpfApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        private RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public MainWindow() {
            InitializeComponent();
            this.DataContext = source;

            string[] args = Environment.GetCommandLineArgs();

            //contains plugins path
            if(args.Length >= 2) {
                string pluginsPath = args[1];
                if (!Path.IsPathRooted(pluginsPath))
                    pluginsPath = Path.GetFullPath(Path.Combine(FileUtil.CurrentDirectory, pluginsPath));

                Logger.Info("Initial Argument: Plugins path: " + pluginsPath);
                source.Settings.SetDefaultPluginsFolder(pluginsPath);

                //contains options path
                if (args.Length >= 3) {
                    string optionsPath = args[2];
                    if (!Path.IsPathRooted(optionsPath))
                        optionsPath = Path.GetFullPath(Path.Combine(FileUtil.CurrentDirectory, optionsPath));

                    Logger.Info("Initial Argument: Options path: " + optionsPath);
                    source.Settings.SetDefaultOptionsPath(optionsPath);

                    //contains frequency
                    if (args.Length >= 4) {
                        int controllerFrequency = 0;
                        if(!int.TryParse(args[3], out controllerFrequency))
                            controllerFrequency = 10;                        

                        Logger.Info("Initial Argument: Controller Frequency path: " + controllerFrequency);
                        source.Settings.SetDefaultControllerFrequency(controllerFrequency);

                        //contains logging level
                        if (args.Length >= 5) {
                            string logLevel = args[4];
                            if (!source.ConsoleManager.SetLoggingLevel(logLevel)) {
                                Logger.Warn("Invalid argument LogLevel: Available options: Off, Fatal, Error, Warn, Info, Debug. Setting to Info.");
                            }

                            Logger.Info("Initial Argument: Logging Level: " + logLevel);
                            source.Settings.SetLoggingLevel(logLevel);

                            if(args.Length >= 6) {
                                source.AutomaticStart = args[5].ToLower().Equals("autostart");
                            }
                        }
                        else {
                            string logLevel = "Info";
                            Logger.Info("Initial Argument: Logging Level: " + logLevel);
                            source.ConsoleManager.SetLoggingLevel(logLevel);
                        }
                    }
                }
            }
            else {
                Logger.Info("No initial arguments provided, using the default values");
            }
        }

        private void RapportControllerWpfGUI_Loaded(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(source.Init);
        }

        private void RapportControllerWpfGUI_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Task.Factory.StartNew(source.Dispose);
        }
    }
}
