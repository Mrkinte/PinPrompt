using System.Text.Json.Serialization;

namespace PinPrompt.Models
{
    public class AppConfig
    {
        public UserConfig UserConfig { get; set; } = new UserConfig();
        public List<PromptConfig> PromptConfigList { get; set; } = new List<PromptConfig> { new PromptConfig() };
    }

    public class UserConfig
    {
        public bool AutoStartup { get; set; } = false;
        public bool AutoHideMainWindow { get; set; } = false;
        public string Theme { get; set; } = "浅色";
    }

    public class PromptConfig
    {
        public string Name { get; set; } = "默认提示";
        public string Guid { get; set; } = "";          // 唯一标识符
        public double LeftPosition { get; set; } = 0;
        public double TopPosition { get; set; } = 0;
        public bool IsActivated { get; set; } = false;
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public double BackgroundOpacity { get; set; } = 0.5;
        public double BackgroundRadius { get; set; } = 5;
        public double Opacity { get; set; } = 1;
        public bool IsTop { get; set; } = true;
        public bool IsSnapToEdge { get; set; } = true;
        public bool IsAutoSize { get; set; } = true;
        public double Width { get; set; } = 180;
        public double Height { get; set; } = 120;

        [JsonIgnore]
        public string XamlBuffer { get; set; } = string.Empty;

        public PromptConfig Clone()
        {
            return new PromptConfig
            {
                Name = this.Name,
                Guid = this.Guid,
                LeftPosition = this.LeftPosition,
                TopPosition = this.TopPosition,
                IsActivated = this.IsActivated,
                BackgroundColor = this.BackgroundColor,
                BackgroundOpacity = this.BackgroundOpacity,
                BackgroundRadius = this.BackgroundRadius,
                Opacity = this.Opacity,
                IsTop = this.IsTop,
                IsSnapToEdge = this.IsSnapToEdge,
                IsAutoSize = this.IsAutoSize,
                Width = this.Width,
                Height = this.Height,
                XamlBuffer = this.XamlBuffer
            };
        }
    }
}
