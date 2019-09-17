using RapportActionProposer.ActionProposalDefinition;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views.Collections {
    /// <summary>
    /// Interaction logic for SnapshotDetailListView.xaml
    /// </summary>
    public partial class SnapshotDetailListView : DataGrid {
        public SnapshotDetailListView() {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
            var proposal = SelectedItem as ImmutableActionProposal;
            if (proposal != null)
                Task.Factory.StartNew(proposal.Execute);
        }
    }
}
