using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Utterances.Elements {
    /// <summary>
    /// Interaction logic for UtterancesFileListGroupBox.xaml
    /// </summary>
    public partial class UtterancesFileListGroupBox : UserControl {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public UtterancesFileListGroupBox() {
            InitializeComponent();
        }

        private void Reload_Folders_Click(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => {
                source.Reload();
                Application.Current.Dispatcher.Invoke(source.RefreshGUI);
            });
        }
    }
}
