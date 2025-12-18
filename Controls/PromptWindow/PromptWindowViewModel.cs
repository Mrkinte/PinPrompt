using PinPrompt.Controls.ColorSelector;
using PinPrompt.Controls.RichTextEditor;
using PinPrompt.Models;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace PinPrompt.Controls.PromptWindow
{
    public partial class PromptWindowViewModel : ObservableObject
    {
        private PromptConfig? _promptConfig;

        [ObservableProperty]
        private FlowDocument? _document;

        [ObservableProperty]
        private double _leftPosition;       // 水印坐标 X方向

        [ObservableProperty]
        private double _topPosition;        // 水印坐标 Y方向

        [ObservableProperty]
        private bool _isActivated;

        [ObservableProperty]
        private Brush _backgroundColor = Brushes.Transparent;   // 背景色

        [ObservableProperty]
        private double _backgroundOpacity;  // 背景透明度

        [ObservableProperty]
        private double _backgroundRadius;   // 背景圆角

        [ObservableProperty]
        private double _opacity;            // 水印透明度

        [ObservableProperty]
        private bool _isTop;                // 是否置顶显示

        [ObservableProperty]
        private bool _isSnapToEdge;         // 是否启用边缘吸附

        [ObservableProperty]
        private Cursor _cursor = Cursors.Arrow;     // 指针类型

        [ObservableProperty]
        private string _title = string.Empty;       // 窗口标题 = 提示名称

        [ObservableProperty]
        private ResizeMode _wResizeMode = ResizeMode.NoResize;

        [ObservableProperty]
        private double _windowWidth;

        [ObservableProperty]
        private double _windowHeight;

        public PromptWindowViewModel(PromptConfig config)
        {
            UpdateConfig(config);
        }

        partial void OnLeftPositionChanged(double oldValue, double newValue)
        {
            if (oldValue != newValue)
                _promptConfig!.LeftPosition = newValue;
        }

        partial void OnTopPositionChanged(double oldValue, double newValue)
        {
            if (oldValue != newValue)
                _promptConfig!.TopPosition = newValue;
        }

        partial void OnWindowWidthChanged(double oldValue, double newValue)
        {
            if (oldValue != newValue)
                _promptConfig!.Width = newValue;
        }

        partial void OnWindowHeightChanged(double oldValue, double newValue)
        {
            if (oldValue != newValue)
                _promptConfig!.Height = newValue;
        }


        /// <summary>
        /// 更新水印配置
        /// </summary>
        /// <param name="newConfig"></param>
        public void UpdateConfig(PromptConfig newConfig)
        {
            _promptConfig = newConfig;
            Title = _promptConfig.Name;
            LeftPosition = _promptConfig.LeftPosition;
            TopPosition = _promptConfig.TopPosition;
            IsActivated = _promptConfig.IsActivated;
            BackgroundOpacity = _promptConfig.BackgroundOpacity;
            BackgroundColor = ColorSelectorHelper.HexToBrush(_promptConfig.BackgroundColor, BackgroundOpacity);
            BackgroundRadius = _promptConfig.BackgroundRadius;
            Opacity = _promptConfig.Opacity;
            IsTop = _promptConfig.IsTop;
            IsSnapToEdge = _promptConfig.IsSnapToEdge;
            WResizeMode = _promptConfig.IsAutoSize ? ResizeMode.NoResize : ResizeMode.CanResize;

            FlowDocument flowDocument = RichTextEditorHelper.XamlToFlowDocumentConverter(_promptConfig.XamlBuffer) ?? new FlowDocument();
            if (_promptConfig.IsAutoSize)
            {
                double w = CalculateMaxWidth(flowDocument);
                double h = CalculateMinHeight(flowDocument);
                WindowWidth = double.IsNaN(w) ? 180 : w;      // NaN校验，避免保存配置时序列化失败。
                WindowHeight = double.IsNaN(h) ? 120 : h;
            }
            else
            {
                WindowWidth = _promptConfig.Width;
                WindowHeight = _promptConfig.Height;
            }
            Document = flowDocument;
        }


        #region 辅助方法

        /// <summary>
        /// 计算FlowDocument的最大宽度
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private double CalculateMaxWidth(FlowDocument document)
        {
            string textContent = new TextRange(document.ContentStart, document.ContentEnd).Text;
            if (string.IsNullOrWhiteSpace(textContent))
            {
                return double.NaN;  // 自动宽度
            }

            // 创建TextBlock并设置相同的字体属性
            TextBlock textBlock = new TextBlock();
            textBlock.Text = textContent;
            textBlock.FontFamily = document.FontFamily;
            textBlock.FontSize = document.FontSize;
            textBlock.FontStyle = document.FontStyle;
            textBlock.FontWeight = document.FontWeight;
            textBlock.FontStretch = document.FontStretch;
            textBlock.TextWrapping = TextWrapping.NoWrap;

            // 测量文本宽度
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return textBlock.DesiredSize.Width + 100;
        }

        /// <summary>
        /// 计算FlowDocument的最小高度
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private double CalculateMinHeight(FlowDocument document)
        {
            string xamlString = XamlWriter.Save(document);
            using (StringReader stringReader = new StringReader(xamlString))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                FlowDocument copyDocument = (FlowDocument)XamlReader.Load(xmlReader);
                if (copyDocument == null)
                    return double.NaN;
                copyDocument.PageWidth = 4096;  // 设置一个较大的PageWidth，确保不会自动换行。
                FlowDocumentScrollViewer viewer = new FlowDocumentScrollViewer{ Document = copyDocument };
                viewer.Measure(new Size(copyDocument.PageWidth, double.PositiveInfinity));
                return viewer.DesiredSize.Height + 40;
            }
        }

        #endregion
    }
}
