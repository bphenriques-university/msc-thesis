using HelpersForNet;
using RapportControllerWpfApplication.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views {
    /// <summary>
    /// Interaction logic for PluginsListTabItem.xaml
    /// </summary>
    public partial class PluginsListTabItem : TabItem {
        RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public PluginsListTabItem() {
            InitializeComponent();
        }

        private void Reload_Click(object sender, System.Windows.RoutedEventArgs e) {
            Task.Factory.StartNew(source.Restart);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (source.StatusThreeStatusButtonTag.Equals("All")) {
                Task.Factory.StartNew(source.DisableAllPlugins);
            }else if (source.StatusThreeStatusButtonTag.Equals("Partial")) {
                Task.Factory.StartNew(source.DisableAllPlugins);
            }
            else {
                Task.Factory.StartNew(source.EnableAllPlugins);
            }
        }
    }
}

