using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PinPrompt.Controls.RichTextEditor
{
    public partial class RichTextEditorViewModel : ObservableObject
    {
        private RichTextBox _richTextBox;

        [ObservableProperty]
        private bool _boldIsChecked;

        [ObservableProperty]
        private bool _italicIsChecked;

        [ObservableProperty]
        private bool _underlineIsChecked;

        [ObservableProperty]
        private bool _strikethroughIsChecked;

        [ObservableProperty]
        private string _currentFontFamily = "Microsoft YaHei UI";

        [ObservableProperty]
        private ObservableCollection<string> _fontFamilies = new ObservableCollection<string>();

        [ObservableProperty]
        private int _currentFontSize = 14;

        [ObservableProperty]
        private int[] _fontSizes = [14, 16, 18, 20, 24, 28, 32, 36, 48, 72];

        [ObservableProperty]
        private Brush _fontColor = Brushes.Black;

        [ObservableProperty]
        private Brush _backgroundColor = Brushes.Transparent;

        [ObservableProperty]
        private bool _leftAlignIsChecked;

        [ObservableProperty]
        private bool _centerAlignIsChecked;

        [ObservableProperty]
        private bool _rightAlignIsChecked;

        [ObservableProperty]
        private double _lineSpacing = 1.0;

        [ObservableProperty]
        private double[] _lineSpacings = [1.0, 1.15, 1.5, 2.0, 2.5, 3.0];

        [RelayCommand]
        public void EditorLoaded(RichTextEditor richTextEditor)
        {
            if (richTextEditor == null)
                return;

            _richTextBox = richTextEditor.RichTextBox;
            _richTextBox.Focus();
            _richTextBox.Document.FontSize = (double)CurrentFontSize;
            _richTextBox.Document.LineHeight = LineSpacing * _richTextBox.FontSize;
            _richTextBox.Document.FontFamily = new FontFamily(CurrentFontFamily);

            var fonts = Fonts.SystemFontFamilies;
            foreach (FontFamily font in fonts)
            {
                FontFamilies.Add(RichTextEditorHelper.GetFontDisplayName(font));
            }
        }

        [RelayCommand]
        public void UpdateButtonStatus()
        {
            var properties = RichTextEditorHelper.GetTextProperties(_richTextBox);
            BoldIsChecked = properties.IsBold;
            ItalicIsChecked = properties.IsItalic;
            UnderlineIsChecked = properties.IsUnderline;
            StrikethroughIsChecked = properties.IsStrikethrough;
            CurrentFontFamily = properties.FontFamily;
            CurrentFontSize = (int)properties.FontSize;
            FontColor = properties.Foreground;
            BackgroundColor = properties.Background;
            LeftAlignIsChecked = properties.Alignment == TextAlignment.Left;
            CenterAlignIsChecked = properties.Alignment == TextAlignment.Center;
            RightAlignIsChecked = properties.Alignment == TextAlignment.Right;

            LineSpacing = properties.LineSpacing == 0.0 ? LineSpacing : properties.LineSpacing;
        }

        [RelayCommand]
        public void OnToggleBold()
        {
            _richTextBox.Focus();
            if (EditingCommands.ToggleBold.CanExecute(null, _richTextBox))
            {
                EditingCommands.ToggleBold.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnToggleUnderline()
        {
            _richTextBox.Focus();
            if (EditingCommands.ToggleUnderline.CanExecute(null, _richTextBox))
            {
                EditingCommands.ToggleUnderline.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnToggleItalic()
        {
            _richTextBox.Focus();
            if (EditingCommands.ToggleItalic.CanExecute(null, _richTextBox))
            {
                EditingCommands.ToggleItalic.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnToggleStrikethrough()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;

            // 获取当前选择的文本装饰
            var currentDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            bool hasStrikethrough = currentDecorations != null &&
                                   currentDecorations.Count > 0 &&
                                   currentDecorations.Any(d => d.Location == TextDecorationLocation.Strikethrough);

            // 创建新的装饰集合，保留除删除线外的所有装饰
            TextDecorationCollection newDecorations = new TextDecorationCollection();
            if (currentDecorations != null && currentDecorations.Count > 0)
            {
                // 复制除删除线外的所有装饰
                foreach (var decoration in currentDecorations)
                {
                    if (decoration.Location != TextDecorationLocation.Strikethrough)
                    {
                        newDecorations.Add(decoration);
                    }
                }
            }

            // 切换删除线状态
            if (!hasStrikethrough)
            {
                // 添加删除线
                newDecorations.Add(TextDecorations.Strikethrough[0]);
            }

            // 应用新的装饰集合到选择范围
            selection.ApplyPropertyValue(Inline.TextDecorationsProperty,
                newDecorations.Count > 0 ? newDecorations : null);

            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnSetFontFamily()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;
            selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(CurrentFontFamily));
        }

        [RelayCommand]
        public void OnSetFontSize()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;

            // 获取选择范围的起始和结束段落
            Paragraph startParagraph = selection.Start.Paragraph;
            Paragraph endParagraph = selection.End.Paragraph;

            if (startParagraph != null)
            {
                if (startParagraph == endParagraph)
                {
                    // 选择范围在同一个段落内
                    double orignalLineSpacing = startParagraph.LineHeight / RichTextEditorHelper.GetMaxFontSizeInParagraph(startParagraph);
                    selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)CurrentFontSize);
                    startParagraph.LineHeight = RichTextEditorHelper.GetMaxFontSizeInParagraph(startParagraph) * orignalLineSpacing;
                    startParagraph.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                }
                else
                {
                    // 选择范围跨多个段落
                    Paragraph? currentParagraph = startParagraph;
                    while (currentParagraph != null)
                    {
                        double orignalLineSpacing = currentParagraph.LineHeight / RichTextEditorHelper.GetMaxFontSizeInParagraph(currentParagraph);
                        TextRange textRange = new TextRange(currentParagraph.ContentStart, currentParagraph.ContentEnd);
                        textRange.ApplyPropertyValue(TextElement.FontSizeProperty, (double)CurrentFontSize);
                        currentParagraph.LineHeight = RichTextEditorHelper.GetMaxFontSizeInParagraph(currentParagraph) * orignalLineSpacing;
                        currentParagraph.LineStackingStrategy = LineStackingStrategy.MaxHeight;

                        // 如果到达结束段落，退出循环
                        if (currentParagraph == endParagraph)
                        {
                            break;
                        }
                        currentParagraph = currentParagraph.NextBlock as Paragraph;
                    }
                }
            }
        }

        [RelayCommand]
        public void OnSetFontColor()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;
            selection.ApplyPropertyValue(TextElement.ForegroundProperty, FontColor);
        }

        [RelayCommand]
        public void OnSetBackgroundColor()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;
            selection.ApplyPropertyValue(TextElement.BackgroundProperty, BackgroundColor);
        }

        [RelayCommand]
        public void OnLeftAlign()
        {
            _richTextBox.Focus();
            if (EditingCommands.AlignLeft.CanExecute(null, _richTextBox))
            {
                EditingCommands.AlignLeft.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnCenterAlign()
        {
            _richTextBox.Focus();
            if (EditingCommands.AlignCenter.CanExecute(null, _richTextBox))
            {
                EditingCommands.AlignCenter.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnRightAlign()
        {
            _richTextBox.Focus();
            if (EditingCommands.AlignRight.CanExecute(null, _richTextBox))
            {
                EditingCommands.AlignRight.Execute(null, _richTextBox);
            }
            UpdateButtonStatus();
        }

        [RelayCommand]
        public void OnSetLineSpacing()
        {
            _richTextBox.Focus();
            TextSelection selection = _richTextBox.Selection;
            // 获取选择范围的起始和结束段落
            Paragraph startParagraph = selection.Start.Paragraph;
            Paragraph endParagraph = selection.End.Paragraph;

            if (startParagraph != null)
            {
                if (startParagraph == endParagraph)
                {
                    // 选择范围在同一个段落内
                    startParagraph.LineHeight = RichTextEditorHelper.GetMaxFontSizeInParagraph(startParagraph) * LineSpacing;
                    startParagraph.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                }
                else
                {
                    // 选择范围跨多个段落
                    Paragraph? currentParagraph = startParagraph;
                    while (currentParagraph != null)
                    {
                        currentParagraph.LineHeight = RichTextEditorHelper.GetMaxFontSizeInParagraph(currentParagraph) * LineSpacing;
                        currentParagraph.LineStackingStrategy = LineStackingStrategy.MaxHeight;

                        // 如果到达结束段落，退出循环
                        if (currentParagraph == endParagraph)
                        {
                            break;
                        }
                        currentParagraph = currentParagraph.NextBlock as Paragraph;
                    }
                }
            }
        }

        [RelayCommand]
        public void OnClearFormat()
        {
            FlowDocument document = _richTextBox.Document;
            if (document == null) return;

            // 清除整个文档的所有属性
            TextRange entireDocument = new TextRange(document.ContentStart, document.ContentEnd);
            entireDocument.ClearAllProperties();
        }
    }
}
