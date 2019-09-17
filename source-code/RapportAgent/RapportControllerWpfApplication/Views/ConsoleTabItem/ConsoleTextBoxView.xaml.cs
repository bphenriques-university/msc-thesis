using HelpersForNet;
using Log4NetWrapperLite;
using RapportControllerWpfApplication.ViewModels;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Views.ConsoleTabItem {
    /// <summary>
    /// Interaction logic for ConsoleTextBoxView.xaml
    /// </summary>
    public partial class ConsoleTextBoxView : UserControl {
        public ConsoleTextBoxView() {
            InitializeComponent();
            RichTextBoxLog4NetAppender appender = Logger.GetAppender<RichTextBoxLog4NetAppender>();
            if (appender != null)
                appender.RichTextBox = LogTextBox;

            Singleton<ConsoleViewManager>.Instance.TextBox = LogTextBox;
        }

        
        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // set the current caret position to the end
            LogTextBox.ScrollToEnd();
        }
    }
}