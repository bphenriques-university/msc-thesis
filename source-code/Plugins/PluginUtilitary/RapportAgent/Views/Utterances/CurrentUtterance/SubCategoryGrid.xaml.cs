using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Utterances.CurrentUtterance {
    /// <summary>
    /// Interaction logic for UtterancesGrid.xaml
    /// </summary>
    public partial class SubCategoryGrid : DataGrid {

        public SubCategoryGrid() {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e) {
            if (HasItems && SelectedIndex == -1) {
                SelectedIndex = 0;
            }
        }
    }
}
