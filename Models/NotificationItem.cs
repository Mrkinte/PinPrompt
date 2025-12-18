using Wpf.Ui.Controls;

namespace PinPrompt.Models
{
    public class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public InfoBarSeverity Severity { get; set; }
        public bool IsOpen { get; set; } = true;    // 用于双向绑定
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; } = DateTime.Now;
    }
}
