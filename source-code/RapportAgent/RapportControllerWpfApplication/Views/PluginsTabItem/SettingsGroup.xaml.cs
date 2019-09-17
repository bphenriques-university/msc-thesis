using HelpersForNet;
using RapportControllerWpfApplication.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views.PluginsTabItem {
    /// <summary>
    /// Interaction logic for SettingsGroup.xaml
    /// </summary>
    public partial class SettingsGroup : UserControl {
        RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public SettingsGroup() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            string folder = FileUtil.FindFolder(source.Settings.PluginsFolderPath);
            if (!string.IsNullOrEmpty(folder) && folder != source.Settings.PluginsFolderPath) {
                source.Settings.PluginsFolderPath = folder;
                Task.Factory.StartNew(source.Restart);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            string folder = FileUtil.FindFolder(source.Settings.OptionsFolderPath);
            if (!string.IsNullOrEmpty(folder) && folder != source.Settings.OptionsFolderPath) {
                source.Settings.OptionsFolderPath = folder;
                Task.Factory.StartNew(source.Restart);
            }
        }

        private void Restore_defaults_Click(object sender, RoutedEventArgs e) {
            source.Settings.PluginsFolderPath = source.Settings.DefaultPluginsPath;
            source.Settings.OptionsFolderPath = source.Settings.DefaultOptionsPath;
            Task.Factory.StartNew(source.Restart);
        }
    }
}
