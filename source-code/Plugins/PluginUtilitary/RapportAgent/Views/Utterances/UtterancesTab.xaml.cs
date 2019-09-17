using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RapportAgentPlugin.Views.Utterances {
    /// <summary>
    /// Interaction logic for UtterancesTab.xaml
    /// </summary>
    public partial class UtterancesTab : TabItem {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public UtterancesTab() {
            InitializeComponent();
        }

        private void Reload_Utterances_Disk_Click(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => {
                source.FileManager.Reload();
                Application.Current.Dispatcher.Invoke(source.FileManager.RefreshGUI);
            });
        }
    }
}