using System.Windows;

namespace RapportActionProposer.Util {
    public class RCPluginWindow : Window {
        public void Init() {
            Closing += Window_Closing;
        }

        public bool ShouldHide { get; set; } = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (ShouldHide) {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
