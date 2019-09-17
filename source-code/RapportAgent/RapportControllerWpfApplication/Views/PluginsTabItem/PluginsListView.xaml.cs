using HelpersForNet;
using RapportControllerWpfApplication.RCWrapper;
using RapportControllerWpfApplication.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RapportControllerWpfApplication.Views.PluginsTabItem {
    /// <summary>
    /// Interaction logic for PluginsListView.xaml
    /// </summary>
    public partial class PluginsListView : DataGrid {
        private RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public PluginsListView() {
            InitializeComponent();

            ICollectionView pluginsView = CollectionViewSource.GetDefaultView(source.Plugins);
            pluginsView.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            ItemsSource = pluginsView;
        }

        private void OpenGUIButton_Click(object sender, RoutedEventArgs e) {
            DummyRCPlugin plugin = SelectedItem as DummyRCPlugin;
            if (plugin != null)
                Application.Current.Dispatcher.Invoke(plugin.ShowGUI);
        }

        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e) {
            DummyRCPlugin plugin = SelectedItem as DummyRCPlugin;
            if (plugin != null)
                Task.Factory.StartNew(plugin.OpenSettings);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            DummyRCPlugin plugin = SelectedItem as DummyRCPlugin;
            if (plugin != null)
                if ((bool)((CheckBox)sender).IsChecked)
                    Task.Factory.StartNew(() => plugin.Activate(source.RapportControllerManager));
                else
                    Task.Factory.StartNew(() => plugin.Desactivate(source.RapportControllerManager));
        }
    }
}
