using HelpersForNet;
using RapportControllerWpfApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RapportControllerWpfApplication.Views.ConsoleTabItem {
    /// <summary>
    /// Interaction logic for ConsoleTabItem.xaml
    /// </summary>
    public partial class ConsoleTabItem : TabItem {
        ConsoleViewManager source = Singleton<ConsoleViewManager>.Instance;

        public ConsoleTabItem() {
            InitializeComponent();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e) {
            ProcessUtil.OpenFolder(source.LogFolderPath);
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e) {
            Application.Current.Dispatcher.Invoke(source.TextBox.Document.Blocks.Clear);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Button b = sender as Button;
            b.ContextMenu.IsEnabled = true;
            b.ContextMenu.PlacementTarget = b;
            b.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            b.ContextMenu.IsOpen = true;
        }
    }
}
