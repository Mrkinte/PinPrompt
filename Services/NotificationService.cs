using PinPrompt.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace PinPrompt.Services
{
    public class NotificationService
    {
        public ObservableCollection<NotificationItem> Notifications { get; } = new();

        private readonly Dispatcher _dispatcher;

        public NotificationService()
        {
            _dispatcher = Application.Current.Dispatcher;
        }

        public void Show(
            string title,
            string message,
            InfoBarSeverity severity = InfoBarSeverity.Informational,
            TimeSpan? duration = null)
        {
            duration ??= TimeSpan.FromSeconds(5);

            var item = new NotificationItem
            {
                Title = title,
                Message = message,
                Severity = severity,
                Duration = duration.Value
            };

            // 确保在 UI 线程操作集合（ObservableCollection 非线程安全）
            if (_dispatcher.CheckAccess())
            {
                AddAndStartTimer(item);
            }
            // 如果从后台线程调用，切回 UI 线程
            else
            {
                _dispatcher.Invoke(() => AddAndStartTimer(item));
            }
        }

        private void AddAndStartTimer(NotificationItem item)
        {
            Notifications.Add(item);

            // 启动超时关闭
            var timer = new DispatcherTimer { Interval = item.Duration };
            timer.Tick += (s, _) =>
            {
                item.IsOpen = false;
                timer.Stop();
                // 延迟移除（等待关闭动画完成）
                Task.Delay(300).ContinueWith(_ =>
                {
                    _dispatcher.Invoke(() =>
                    {
                        Notifications.Remove(item);
                    });
                });
            };
            timer.Start();
        }
    }
}
