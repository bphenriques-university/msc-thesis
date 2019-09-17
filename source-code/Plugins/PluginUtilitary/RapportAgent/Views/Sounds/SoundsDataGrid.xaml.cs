using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Sounds;
using System.Windows;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Sounds {
    /// <summary>
    /// Interaction logic for SoundsDataGrid.xaml
    /// </summary>
    public partial class SoundsDataGrid : DataGrid {
        SoundsManagerModel source = Singleton<ActionsManagerViewModel>.Instance.SoundsManager;

        public SoundsDataGrid() {
            InitializeComponent();
        }

        private void Run_Click(object sender, RoutedEventArgs e) {
            SoundFileInfo s = SelectedItem as SoundFileInfo;
            s?.Run(source.Plugin, source.Plugin.Actions);
        }
    }
}
