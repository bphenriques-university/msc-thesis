using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Utterances.Elements {
    /// <summary>
    /// Interaction logic for VariablesGridGroupBox.xaml
    /// </summary>
    public partial class VariablesGridGroupBox : UserControl {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public VariablesGridGroupBox() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => {
                source.SubstitutionManager.Reload();
                Application.Current.Dispatcher.Invoke(source.SubstitutionManager.RefreshGUI);
            });
        }
    }
}
