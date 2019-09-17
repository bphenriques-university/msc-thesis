using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Utterances {
    /// <summary>
    /// Interaction logic for UtterancesFilesGrid.xaml
    /// </summary>
    public partial class UtterancesFilesGrid : DataGrid {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public UtterancesFilesGrid() {
            InitializeComponent();
        }

        private void Select_Click(object sender, RoutedEventArgs e) {
            UtteranceFile s = SelectedItem as UtteranceFile;

            Task.Factory.StartNew(() => {
                source.ChangeUtteranceFile(s.Location);
                Application.Current.Dispatcher.Invoke(source.RefreshGUI);
            });
        }
    }
}
