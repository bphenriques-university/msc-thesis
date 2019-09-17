using HelpersForNet;
using RapportControllerWpfApplication.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views {
    /// <summary>
    /// Interaction logic for SnapshotListTabItem.xaml
    /// </summary>
    public partial class SnapshotListTabItem : TabItem {
        RapportControllerManagerData source = Singleton<RapportControllerManagerData>.Instance;

        public SnapshotListTabItem() {
            InitializeComponent();
        }

        private void ClearSnapshotsButton_Click(object sender, RoutedEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "This will clear the snapshot list", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                    source.SnapshotsManager.Clear();
            });
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
