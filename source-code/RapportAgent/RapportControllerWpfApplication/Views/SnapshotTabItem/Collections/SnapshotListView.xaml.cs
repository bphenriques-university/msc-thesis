using RapportControllerLib;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views.Collections {
    /// <summary>
    /// Interaction logic for SnapshotListView.xaml
    /// </summary>
    public partial class SnapshotListView : DataGrid {
        public SnapshotListView() {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e) {
            if (HasItems && SelectedIndex == -1) {
                SelectedIndex = 0;
            }
        }

        private void ExecuteSnapshotButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            ImmutableActionsSnapshot snapshot = SelectedItem as ImmutableActionsSnapshot;
            if (snapshot != null)
                snapshot.Execute();
        }
    }
}
