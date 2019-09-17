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

namespace RapportAgentPlugin.Views.Utterances.Elements {
    /// <summary>
    /// Interaction logic for CurrentSettingsGroupBox.xaml
    /// </summary>
    public partial class CurrentSettingsGroupBox : UserControl {
        UtterancesManager source = Singleton<ActionsManagerViewModel>.Instance.UtterancesManager;

        public CurrentSettingsGroupBox() {
            InitializeComponent();
        }

        private void ChangeFolder_Click(object sender, RoutedEventArgs e) {
            string folderPath = FileUtil.FindFolder(source.FileListManager.UtterancesSourceFolderPath);
            if (!string.IsNullOrEmpty(folderPath)) {
                Task.Factory.StartNew(() => {
                    source.SetUtterancesFolder(folderPath);
                    Application.Current.Dispatcher.Invoke(source.RefreshGUI);
                });
            }
        }
    }
}
