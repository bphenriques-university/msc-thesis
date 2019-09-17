using HelpersForNet;
using System.Windows;
using RapportAgentPlugin.ViewModel;
using RapportActionProposer.Util;

namespace RapportAgentPlugin {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RCPluginWindow {
        ActionsManagerViewModel source = Singleton<ActionsManagerViewModel>.Instance;

        public MainWindow() {
            InitializeComponent();
            Init();
            
            DataContext = source;
            source.RefreshGUI();
        }
    }
}