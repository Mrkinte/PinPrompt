using Hardcodet.Wpf.TaskbarNotification;

namespace PinPrompt.Services
{
    public class TrayNotificationService
    {
        private TaskbarIcon? _trayIcon;

        /// <summary>
        /// 初始化系统托盘通知服务
        /// </summary>
        /// <param name="trayIcon">系统托盘图标对象</param>
        public void Initialize(TaskbarIcon trayIcon)
        {
            _trayIcon = trayIcon;
        }

        /// <summary>
        /// 显示系统托盘通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        /// <param name="icon">通知图标</param>
        public void ShowNotification(string title, string message, BalloonIcon icon = BalloonIcon.Info)
        {
            if (_trayIcon != null)
            {
                _trayIcon.ShowBalloonTip(title, message, icon);
            }
        }
    }
}