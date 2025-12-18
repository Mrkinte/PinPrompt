using PinPrompt.Models;
using PinPrompt.Services;
using System.Collections.ObjectModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PinPrompt.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _firstStartup = true;
        private readonly UserConfig _userConfig;

        [ObservableProperty]
        private string _applicationTitle = "钉提示";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "编辑",
                Icon = new SymbolIcon { Symbol = SymbolRegular.TextBulletListSquareEdit24 },
                TargetPageType = typeof(Views.Pages.PromptEditPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        public MainWindowViewModel(AppConfigService appConfigService)
        {
            _userConfig = appConfigService.UserConfig;
        }

        [RelayCommand]
        public void OnLoaded()
        {
            switch (_userConfig.Theme)
            {
                case "浅色":
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;

                default:
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    break;
            }
        }

        [RelayCommand]
        public void OnWindowActivated()
        {
            if (_firstStartup && _userConfig.AutoHideMainWindow)
            {
                App.Current.MainWindow.Hide();
                _firstStartup = false;
            }
        }
    }
}
