using HelpersForNet;
using RapportControllerWpfApplication.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views {
    /// <summary>
    /// Interaction logic for StatusStrip.xaml
    /// </summary>
    public partial class StatusStrip : UserControl {
        RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public StatusStrip() {
            InitializeComponent();
            DataContext = source;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ProcessUtil.OpenFolder(source.Settings.PluginsFolderPath);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            ProcessUtil.OpenFolder(source.Settings.OptionsFolderPath);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            b.ContextMenu.IsEnabled = true;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            b.ContextMenu.IsOpen = true;
        }
    }
}
