using PinPrompt.Controls.RichTextEditor;
using System.Windows.Documents;
using Wpf.Ui.Controls;

namespace PinPrompt.Views.Windows
{
    /// <summary>
    /// UpdateDialogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateDialogWindow : FluentWindow
    {
        public UpdateDialogWindow(
            string version,
            string githubDownloadLink,
            string sourceForgeDownloadLink,
            string UpdateLog)
        {
            Owner = App.Current.MainWindow;

            InitializeComponent();

            VersionTextBlock.Text = $"发现新版本：{version}";
            if (string.IsNullOrWhiteSpace(githubDownloadLink))
            {
                GithubDownloadButton.Visibility = Visibility.Collapsed;
            }
            if (string.IsNullOrWhiteSpace(sourceForgeDownloadLink))
            {
                SourceForgeDownloadButton.Visibility = Visibility.Collapsed;
            }
            GithubDownloadButton.NavigateUri = githubDownloadLink;
            SourceForgeDownloadButton.NavigateUri = sourceForgeDownloadLink;
            FlowDocument? updateLogDocument = RichTextEditorHelper.XamlToFlowDocumentConverter(UpdateLog);
            if (updateLogDocument != null)
            {
                UpdateLogViewer.Document = updateLogDocument;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
