using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances.VariableSubsitution;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Utterances {
    /// <summary>
    /// Interaction logic for VariablesGrid.xaml
    /// </summary>
    public partial class VariablesGrid : DataGrid {
        VariableSubtituitionManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager.SubstitutionManager;

        public VariablesGrid() {
            InitializeComponent();
        }
    }
}
