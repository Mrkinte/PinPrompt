using Serilog;
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace PinPrompt.Controls.RichTextEditor
{
    public class TextProperties
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public bool IsStrikethrough { get; set; }
        public string FontFamily { get; set; } = "Microsoft YaHei UI";
        public double FontSize { get; set; } = 14;
        public Brush Foreground { get; set; } = Brushes.Black;
        public Brush Background { get; set; } = Brushes.Transparent;
        public TextAlignment Alignment { get; set; }
        public double LineSpacing { get; set; }
    }

    public static class RichTextEditorHelper
    {
        public static TextProperties GetTextProperties(RichTextBox richTextBox)
        {
            TextProperties properties = new TextProperties();
            TextSelection selection = richTextBox.Selection;

            if (selection.IsEmpty)
            {
                TextPointer caretPos = richTextBox.CaretPosition;
                DependencyObject element = caretPos.Parent;
                while (element != null && !(element is Run))
                {
                    element = LogicalTreeHelper.GetParent(element);
                }
                Run? currentRun = element as Run;
                if (currentRun != null)
                {
                    properties.IsBold = currentRun.FontWeight == FontWeights.Bold;
                    properties.IsItalic = currentRun.FontStyle == FontStyles.Italic;
                    if (currentRun.TextDecorations != null)
                    {
                        properties.IsUnderline = currentRun.TextDecorations.Any(d =>
                            d.Location == TextDecorationLocation.Underline);
                        properties.IsStrikethrough = currentRun.TextDecorations.Any(d =>
                            d.Location == TextDecorationLocation.Strikethrough);
                    }
                    properties.FontFamily = GetFontDisplayName(currentRun.FontFamily);
                    properties.FontSize = currentRun.FontSize;
                    properties.Foreground = currentRun.Foreground;
                    properties.Background = currentRun.Background;
                    properties.Alignment = caretPos.Paragraph.TextAlignment;
                }
            }
            else
            {
                TextRange range = new TextRange(selection.Start, selection.End);

                object fontWeight = range.GetPropertyValue(TextElement.FontWeightProperty);
                properties.IsBold = fontWeight != DependencyProperty.UnsetValue &&
                                   (FontWeight)fontWeight == FontWeights.Bold;

                object fontStyle = range.GetPropertyValue(TextElement.FontStyleProperty);
                properties.IsItalic = fontStyle != DependencyProperty.UnsetValue &&
                                     (FontStyle)fontStyle == FontStyles.Italic;

                object textDecorations = range.GetPropertyValue(Inline.TextDecorationsProperty);
                if (textDecorations != DependencyProperty.UnsetValue && textDecorations is TextDecorationCollection decorations)
                {
                    properties.IsUnderline = decorations.Any(d => d.Location == TextDecorationLocation.Underline);
                    properties.IsStrikethrough = decorations.Any(d => d.Location == TextDecorationLocation.Strikethrough);
                }

                object fontFamily = range.GetPropertyValue(TextElement.FontFamilyProperty);
                properties.FontFamily = fontFamily != DependencyProperty.UnsetValue ? GetFontDisplayName((FontFamily)fontFamily) : "Microsoft YaHei UI";

                object fontSize = range.GetPropertyValue(TextElement.FontSizeProperty);
                properties.FontSize = fontSize != DependencyProperty.UnsetValue ? (double)fontSize : SystemFonts.MessageFontSize;

                object foreground = range.GetPropertyValue(TextElement.ForegroundProperty);
                properties.Foreground = foreground != DependencyProperty.UnsetValue ? (Brush)foreground : Brushes.Black;

                object background = range.GetPropertyValue(TextElement.BackgroundProperty);
                properties.Background = background != DependencyProperty.UnsetValue ? (Brush)background : Brushes.Transparent;

                object alignment = range.GetPropertyValue(Paragraph.TextAlignmentProperty);
                if ((alignment != DependencyProperty.UnsetValue) && alignment is TextAlignment)
                {
                    properties.Alignment = (TextAlignment)alignment;
                }
            }
            properties.LineSpacing = GetLineSpacing(richTextBox);
            return properties;
        }

        public static string GetFontDisplayName(FontFamily fontFamily)
        {
            // 优先尝试中文名称
            var culture = System.Windows.Markup.XmlLanguage.GetLanguage("zh-CN");
            string displayName = string.Empty;

            
            // 尝试获取中文名称
            if (fontFamily.FamilyNames.TryGetValue(culture, out displayName))
            {
                return displayName;
            }

            // 如果没有中文名称，尝试英文名称
            culture = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
            if (fontFamily.FamilyNames.TryGetValue(culture, out displayName))
            {
                return displayName;
            }

            // 如果都没有，使用第一个可用的名称或Source
            if (fontFamily.FamilyNames.Count > 0)
            {
                return fontFamily.FamilyNames.First().Value;
            }

            return fontFamily.Source;
        }

        /// <summary>
        /// 获取光标所在行里最大字符的尺寸
        /// </summary>
        /// <param name="richTextBox"></param>
        /// <returns></returns>
        public static double GetMaxFontSizeInCursorLine(RichTextBox richTextBox)
        {
            if (richTextBox == null) return 14.0;

            // 获取当前光标位置
            TextPointer caretPosition = richTextBox.CaretPosition;

            // 获取光标所在的段落
            Paragraph? currentParagraph = caretPosition.Paragraph;
            if (currentParagraph == null) return richTextBox.FontSize;

            double maxFontSize = richTextBox.FontSize;

            // 遍历段落中的所有文本元素
            foreach (Inline inline in currentParagraph.Inlines)
            {
                // 检查Run元素
                if (inline is Run run)
                {
                    // 获取Run元素的字体大小
                    double fontSize = (double)run.GetValue(TextElement.FontSizeProperty);
                    if (fontSize > maxFontSize)
                    {
                        maxFontSize = fontSize;
                    }
                }
                // 检查Span元素
                else if (inline is Span span)
                {
                    // 递归检查Span中的内联元素
                    double spanMaxFontSize = GetMaxFontSizeInInline(span);
                    if (spanMaxFontSize > maxFontSize)
                    {
                        maxFontSize = spanMaxFontSize;
                    }
                }
            }

            return maxFontSize;
        }

        /// <summary>
        /// 获取Paragraph里最大字符的尺寸
        /// </summary>
        /// <param name="paragraph"></param>
        /// <returns></returns>
        public static double GetMaxFontSizeInParagraph(Paragraph paragraph)
        {
            double maxFontSize = 14;

            if (paragraph == null) return maxFontSize;

            // 遍历段落中的所有文本元素
            foreach (Inline inline in paragraph.Inlines)
            {
                // 检查Run元素
                if (inline is Run run)
                {
                    // 获取Run元素的字体大小
                    double fontSize = (double)run.GetValue(TextElement.FontSizeProperty);
                    if (fontSize > maxFontSize)
                    {
                        maxFontSize = fontSize;
                    }
                }
                // 检查Span元素
                else if (inline is Span span)
                {
                    // 递归检查Span中的内联元素
                    double spanMaxFontSize = GetMaxFontSizeInInline(span);
                    if (spanMaxFontSize > maxFontSize)
                    {
                        maxFontSize = spanMaxFontSize;
                    }
                }
            }

            return maxFontSize;
        }

        public static string FlowDocumentToXamlConverter(FlowDocument document)
        {
            if (document == null) return string.Empty;

            return XamlWriter.Save(document);
        }

        public static FlowDocument? XamlToFlowDocumentConverter(string xamlString)
        {
            try
            {
                if (string.IsNullOrEmpty(xamlString))
                    return null;

                using (StringReader stringReader = new StringReader(xamlString))
                using (XmlReader xmlReader = XmlReader.Create(stringReader))
                {
                    return (FlowDocument)XamlReader.Load(xmlReader);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Xaml解析失败：{ ex.Message }");
                throw new Exception(ex.Message);
            }
        }

        public static bool IsFlowDocumentEmpty(FlowDocument doc)
        {
            if (doc == null) return true;

            var textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
            return string.IsNullOrWhiteSpace(textRange.Text);
        }

        /// <summary>
        /// 获取行间距
        /// </summary>
        /// <param name="richTextBox"></param>
        /// <returns></returns>
        private static double GetLineSpacing(RichTextBox richTextBox)
        {
            TextPointer caretPosition = richTextBox.CaretPosition;
            Paragraph? currentParagraph = caretPosition.Paragraph;
            if (currentParagraph != null)
            {
                // 从段落向上查找FlowDocument
                FlowDocument document = GetFlowDocumentFromParagraph(currentParagraph);
                if (document != null)
                {
                    // 计算行间距倍数
                    return currentParagraph.LineHeight / GetMaxFontSizeInCursorLine(richTextBox);
                }
            }
            return 0.0;
        }

        private static FlowDocument? GetFlowDocumentFromParagraph(Paragraph paragraph)
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(paragraph);
            while (parent != null && !(parent is FlowDocument))
            {
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return parent as FlowDocument;
        }

        private static double GetMaxFontSizeInInline(Inline inline)
        {
            double maxFontSize = (double)inline.GetValue(TextElement.FontSizeProperty);

            // 如果是Span或其派生类，递归检查子元素
            if (inline is Span span)
            {
                foreach (Inline childInline in span.Inlines)
                {
                    double childMaxFontSize = GetMaxFontSizeInInline(childInline);
                    if (childMaxFontSize > maxFontSize)
                    {
                        maxFontSize = childMaxFontSize;
                    }
                }
            }

            return maxFontSize;
        }
    }
}
