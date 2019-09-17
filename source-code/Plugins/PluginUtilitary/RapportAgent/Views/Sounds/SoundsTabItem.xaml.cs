using HelpersForNet;
using RapportAgentPlugin.ViewModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportAgentPlugin.Views.Sounds {
    /// <summary>
    /// Interaction logic for SoundsTabItem.xaml
    /// </summary>
    public partial class SoundsTabItem : TabItem {
        SoundsManagerModel source = Singleton<ActionsManagerViewModel>.Instance.SoundsManager;

        public SoundsTabItem() {
            InitializeComponent();
        }

        private void Reload_Click(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(() => {
                source.Reload();
                Application.Current.Dispatcher.Invoke(source.RefreshGUI);
            });
        }

        private void ChangeFolder_Click(object sender, RoutedEventArgs e) {
            string filePath = FileUtil.FindFolder(source.SoundsFolderPath);
            if (!string.IsNullOrEmpty(filePath)) {
                Task.Factory.StartNew(() => {
                    source.SoundsFolderPath = filePath;
                    source.Reload();
                    Application.Current.Dispatcher.Invoke(source.RefreshGUI);
                });
            }
        }
    }
}
