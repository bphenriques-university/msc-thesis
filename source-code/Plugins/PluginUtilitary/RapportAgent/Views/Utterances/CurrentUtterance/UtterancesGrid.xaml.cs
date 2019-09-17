using HelpersForNet;
using System.Windows.Controls;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;

namespace RapportAgentPlugin.Views.Utterances.CurrentUtterance {
    /// <summary>
    /// Interaction logic for UtterancesGrid.xaml
    /// </summary>
    public partial class UtterancesGrid : DataGrid {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public UtterancesGrid() {
            InitializeComponent();            
        }

        private void Run_Click(object sender, System.Windows.RoutedEventArgs e) {
            UtteranceInfo spec = SelectedItem as UtteranceInfo;

            if(spec != null) {
                var utterance = spec.Load(source.SubstitutionManager, source.Plugin.Actions.UtterancesParser, spec.Priority, null);
                source.Plugin.ProposeAction(source.Plugin.Actions.PerformUtterance(utterance, spec.ReplacedTextWithDefaultTags, spec.Priority, spec.InitialDelay, spec.TimeOutMs));
            }            
        }

        private void ToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e) {
            UtteranceInfo spec = SelectedItem as UtteranceInfo;
            if(spec != null) {
                spec.ShouldShowUtteranceWithTagsReplaced = !spec.ShouldShowUtteranceWithTagsReplaced;
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            UtteranceInfo spec = SelectedItem as UtteranceInfo;
            if (!spec.ShouldShowUtteranceWithTagsReplaced) {
                string text = ((TextBox)e.EditingElement).Text;
                spec.Text = text;
            }
        }
    }
}
