using PinPrompt.Services;
using PinPrompt.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PinPrompt.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        private bool isRealExit = false;
        private bool isFirstHidden = true;

        public MainWindowViewModel ViewModel { get; }
        public NotificationService NotificationService { get; }
        public TrayNotificationService TrayNotificationService { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService,
            NotificationService notificationService,
            TrayNotificationService trayNotificationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);

            NotificationService = notificationService;
            TrayNotificationService = trayNotificationService;
            // 初始化系统托盘通知服务
            TrayNotificationService.Initialize(TrayIcon);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private void ShowWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;     // 隐藏任务栏图标
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            isRealExit = true;
            this.Close();
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isRealExit)
            {
                e.Cancel = true;    // 取消关闭操作
                this.Hide();        // 隐藏窗口
                this.ShowInTaskbar = false;     // 隐藏任务栏图标

                // 显示托盘提示
                if (isFirstHidden)
                {
                    TrayNotificationService.ShowNotification("提示", "程序已最小化到系统托盘，单击图标可重新打开主界面");
                    isFirstHidden = false;
                }
                return;
            }
            base.OnClosed(e);
        }
    }
}
