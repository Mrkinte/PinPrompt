using PinPrompt.Views.Windows;
using Serilog;
using System.Net.Http;
using System.Xml.Linq;
using Wpf.Ui.Controls;

namespace PinPrompt.Services
{
    public class UpdateService
    {
        private readonly ILogger _logger;
        private readonly NotificationService _notificationService;

        // 更新源
        private readonly List<string> _urlList = new List<string> {
            "https://raw.githubusercontent.com/Mrkinte/PinPrompt/refs/heads/main/version.xml",
            "https://sourceforge.net/p/pin-prompt/code/ci/main/tree/version.xml?format=raw"};

        private bool _isBusy = false;
        private Version _onlineVersion;
        private string _githubDownloadLink = string.Empty;
        private string _sourceForgeDownloadLink = string.Empty;
        private string _onlineUpdateLog = string.Empty;

        public UpdateService(ILogger logger, NotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
            _onlineVersion = new Version(1, 0, 0);
        }

        public void CheckUpdate()
        {
            Task.Run(async () =>
            {
                if (_isBusy)
                {
                    _notificationService.Show("提示", "获取更新中...", InfoBarSeverity.Informational);
                    return;
                }
                _isBusy = true;
                
                // 初始化版本信息
                Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
                _onlineVersion = new Version(1, 0, 0);
                
                bool result = await GetOnlineVersion();
                if (_onlineVersion > currentVersion)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateDialogWindow updateDialog = new UpdateDialogWindow(
                            _onlineVersion.ToString(),
                            _githubDownloadLink,
                            _sourceForgeDownloadLink,
                            _onlineUpdateLog);

                        updateDialog.ShowDialog();
                    });
                }
                else if (result)
                {
                    _notificationService.Show("提示", "当前已是最新版本", InfoBarSeverity.Informational);
                }
                _isBusy = false;
            });
        }

        private async Task<bool> GetOnlineVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                // 设置超时时间
                client.Timeout = TimeSpan.FromSeconds(10);

                // 发送请求获取XML内容
                string xmlContent = string.Empty;
                _notificationService.Show("提示", "获取更新中...", InfoBarSeverity.Informational);
                foreach (var url in _urlList)
                {
                    try
                    {
                        _logger.Information("获取更新中...");
                        _logger.Information($"更新源：{url}");
                        xmlContent = await client.GetStringAsync(url);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"当前源获取更新失败：{ex.Message}");
                    }
                }

                if (string.IsNullOrEmpty(xmlContent))
                {
                    _notificationService.Show("错误", "获取更新失败", InfoBarSeverity.Error);
                    return false;
                }

                // 解析XML
                XDocument doc = XDocument.Parse(xmlContent);
                if (doc.Root == null)
                {
                    _logger.Error("更新文件解析失败");
                    _notificationService.Show("错误", "更新文件解析失败", InfoBarSeverity.Error);
                    return false;
                }
                // 获取版本号节点
                var version = doc.Root.Element("version")?.Value;
                if (version != null)
                {
                    _onlineVersion = new Version(version);
                }
                _githubDownloadLink = doc.Root.Element("download_link")?.Value ?? string.Empty;
                _sourceForgeDownloadLink = doc.Root.Element("download_link2")?.Value ?? string.Empty;
                _onlineUpdateLog = doc.Root.Element("update_contents")?.Value ?? string.Empty;
                _logger.Information($"获取更新成功，最新版本：{_onlineVersion}");
                return true;
            }
        }
    }
}
