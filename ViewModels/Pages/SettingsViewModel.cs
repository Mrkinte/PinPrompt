using PinPrompt.Helpers;
using PinPrompt.Services;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PinPrompt.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized;
        private readonly ILogger _logger;
        private readonly AppConfigService _appConfigService;
        private readonly NotificationService _notificationService;
        private readonly UpdateService _updateService;

        [ObservableProperty]
        private bool _autoStartup;

        [ObservableProperty]
        private bool _autoHideMainWindow;

        [ObservableProperty]
        private string _appVersion;

        [ObservableProperty]
        private object _selectedTheme;

        [ObservableProperty]
        private ObservableCollection<string>? _themeItems;

        [ObservableProperty]
        private string _logFolder;

        [ObservableProperty]
        private bool _restoreBtnIsEnable = true;

        partial void OnSelectedThemeChanged(object value)
        {
            string item = (string)value;
            if (!string.IsNullOrEmpty(item))
            {
                switch (item)
                {
                    case "浅色":
                        _appConfigService.UserConfig.Theme = "浅色";
                        if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light)
                        {
                            return;
                        }
                        ApplicationThemeManager.Apply(ApplicationTheme.Light);
                        _appConfigService.SaveConfig();
                        break;

                    default:
                        _appConfigService.UserConfig.Theme = "深色";
                        if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
                        {
                            return;
                        }
                        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                        _appConfigService.SaveConfig();
                        break;
                }
            }
        }

        public SettingsViewModel(ILogger logger, AppConfigService appConfigService,
            NotificationService notificationService,
            UpdateService updateService)
        {
            _isInitialized = false;
            _logger = logger;
            _appConfigService = appConfigService;
            _notificationService = notificationService;
            _updateService = updateService;

            AppVersion = string.Empty;
            SelectedTheme = string.Empty;
            LogFolder = AppConfigService.LogFolder;
        }

        [RelayCommand]
        private void OnSetAutoStartup()
        {
            try
            {
                bool result = StartupHelper.SetStartup(AutoStartup);
                if (!result)
                {
                    AutoStartup = !AutoStartup;     // 修改失败，恢复上一状态。
                }
                else
                {
                    if (AutoStartup)
                    {
                        AutoHideMainWindow = true;
                    }
                    _appConfigService.UserConfig.AutoStartup = AutoStartup;
                    _appConfigService.UserConfig.AutoHideMainWindow = AutoHideMainWindow;
                    _appConfigService.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                AutoStartup = !AutoStartup;
                _notificationService.Show("错误", $"开机自启动设置失败：{ex.Message}", InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private void OnSetAutoHideMainWindow()
        {
            _appConfigService.UserConfig.AutoHideMainWindow = AutoHideMainWindow;
            _appConfigService.SaveConfig();
        }

        [RelayCommand]
        private void OnOpenLogFolder()
        {
            if (Directory.Exists(LogFolder))
            {
                Process.Start("explorer.exe", LogFolder);
            }
        }

        [RelayCommand]
        private void OnRemoveConfigFolder()
        {
            try
            {
                Directory.Delete(_appConfigService.ConfigFolder, true);
                _notificationService.Show("成功", "已恢复软件默认设置，提示列表清空需重启生效。", InfoBarSeverity.Success);
                RestoreBtnIsEnable = false;
            }
            catch (Exception ex)
            {
                _notificationService.Show("错误", ex.Message, InfoBarSeverity.Error);
            }
        }

        [RelayCommand]
        private void OnCheckUpdate() => _updateService.CheckUpdate();

        #region INavigationAware methods

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = $"© 版权所有 2025, Mrkinte, 当前版本 {version!.Major}.{version.Minor}.{version.Build}";
            ThemeItems = new ObservableCollection<string> { "浅色", "深色" };

            AutoStartup = _appConfigService.UserConfig.AutoStartup;
            AutoHideMainWindow = _appConfigService.UserConfig.AutoHideMainWindow;
            try
            {
                SelectedTheme = ThemeItems[ThemeItems.IndexOf(_appConfigService.UserConfig.Theme)];
            }
            catch (Exception)
            {
                _logger.Warning($"配置文件中存在无效的主题参数：{_appConfigService.UserConfig.Theme}");
                _notificationService.Show("警告", $"配置文件中存在无效的主题参数：{_appConfigService.UserConfig.Theme}", InfoBarSeverity.Warning);
            }

            CheckStartupEnabled();
            _isInitialized = true;
        }

        #endregion

        private void CheckStartupEnabled()
        {
            if (AutoStartup && !StartupHelper.IsStartupEnabled())
            {
                AutoStartup = false;
                _notificationService.Show("警告", "开机自启动失效，请尝试重新启用。", InfoBarSeverity.Warning);
            }
        }
    }
}
