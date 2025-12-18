using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace PinPrompt.Controls.ColorSelector
{
    /// <summary>
    /// ColorSelector.xaml 的交互逻辑
    /// </summary>
    public partial class ColorSelector : UserControl
    {
        public ColorSelector()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(SymbolRegular), typeof(ColorSelector),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(ColorSelector),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ColorSelector),
                new PropertyMetadata("选择颜色"));

        public static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register("DefaultColor", typeof(Brush), typeof(ColorSelector),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public SymbolRegular Icon
        {
            get { return (SymbolRegular)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Brush SelectedColor
        {
            get { return (Brush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public Brush DefaultColor
        {
            get { return (Brush)GetValue(DefaultColorProperty); }
            set { SetValue(DefaultColorProperty, value); }
        }

        #region 自定义颜色选择事件

        // 创建一个颜色选择事件
        public static readonly RoutedEvent ColorSelectedEvent =
            EventManager.RegisterRoutedEvent(name: "ColorSelected", routingStrategy: RoutingStrategy.Bubble,
                handlerType: typeof(RoutedEventHandler), ownerType: typeof(ColorSelector));

        public event RoutedEventHandler ColorSelected
        {
            add { AddHandler(ColorSelectedEvent, value); }
            remove { RemoveHandler(ColorSelectedEvent, value); }
        }

        /// <summary>
        /// 触发ColorSelected事件
        /// </summary>
        protected virtual void OnColorSelected()
        {
            RoutedEventArgs args = new RoutedEventArgs(ColorSelectedEvent, this);
            RaiseEvent(args);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSelector = d as ColorSelector;
            if (colorSelector == null)
                return;

            if (colorSelector.DefaultColor == null)
                colorSelector.DefaultColor = colorSelector.SelectedColor;

            colorSelector.PreviewColorBorder.Background = colorSelector.SelectedColor;
            string selectedColorHex = ColorSelectorHelper.BrushToHex(colorSelector.SelectedColor);
            colorSelector.PreviewColorTextBlock.Foreground = ColorSelectorHelper.HexToBrush(ColorSelectorHelper.GetContrastColorWCAG(selectedColorHex));
        }

        #endregion

        #region 普通事件回调方法

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = true;
        }

        private void ColorItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button colorButton && colorButton.Tag is string hexColor)
            {
                var brush = ColorSelectorHelper.HexToBrush(hexColor);
                SelectedColor = brush;
                OnColorSelected();
                ColorPopup.IsOpen = false;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
            if (CustomColorTextBox.Text.Length != 7)
                return;

            SelectedColor = PreviewColorBorder.Background;
            OnColorSelected();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPopup.IsOpen = false;
        }

        private void DefaultColorButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = DefaultColor;
            OnColorSelected();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            string originalText = textBox.Text;

            // 如果文本框为空，自动添加#号
            if (string.IsNullOrEmpty(originalText))
            {
                textBox.Text = "#";
                textBox.CaretIndex = 1;
                return;
            }

            // 确保以#号开头
            if (!originalText.StartsWith("#"))
            {
                textBox.Text = "#" + originalText;
                textBox.CaretIndex = textBox.Text.Length;
                return;
            }

            string hexPart = originalText.Substring(1);
            string filteredHex = Regex.Replace(hexPart, "[^0-9A-Fa-f]", "");
            filteredHex = filteredHex.ToUpper();
            if (filteredHex.Length > 6)
            {
                filteredHex = filteredHex.Substring(0, 6);
            }
            string newText = "#" + filteredHex;

            // 如果文本有变化，更新文本框
            if (newText != originalText)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = newText;

                // 调整光标位置
                if (caretIndex > newText.Length)
                    textBox.CaretIndex = newText.Length;
                else
                    textBox.CaretIndex = caretIndex;
            }

            if (newText.Length == 7)
            {
                PreviewColorBorder.Background = ColorSelectorHelper.HexToBrush(newText);
                PreviewColorTextBlock.Foreground = ColorSelectorHelper.HexToBrush(ColorSelectorHelper.GetContrastColorWCAG(newText));
            }
        }

        #endregion

        #region 自定义颜色输入校验

        private void HexTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // 处理退格键，防止删除#号
            if (e.Key == Key.Back)
            {
                // 如果光标在#号后面第一个字符的位置，且前面只有#号，阻止退格
                if (textBox.CaretIndex == 1 && textBox.Text.Length == 1)
                {
                    e.Handled = true;
                    return;
                }

                // 如果选择的范围包含#号，阻止操作
                if (textBox.SelectionStart == 0 && textBox.SelectionLength > 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            // 处理删除键
            if (e.Key == Key.Delete)
            {
                // 如果删除操作会影响#号，阻止删除
                if (textBox.CaretIndex == 0 && textBox.Text.Length > 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            // 处理Ctrl+V粘贴，在粘贴事件中处理更合适
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // 允许粘贴，在TextChanged中会进行过滤
                return;
            }
        }

        private void HexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 验证输入是否为十六进制字符
            if (!IsHexCharacter(e.Text))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;

            // 检查是否已达到最大长度（#号 + 6个字符）
            if (textBox.Text.Length >= 7)
            {
                e.Handled = true;
                return;
            }

            // 如果用户尝试在#号前输入，将光标移到#号后
            if (textBox.CaretIndex == 0)
            {
                textBox.CaretIndex = 1;
            }
        }

        private bool IsHexCharacter(string text)
        {
            return Regex.IsMatch(text, "^[0-9A-Fa-f]$");
        }

        private void HexTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            // 处理粘贴操作
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));

                // 过滤粘贴内容，只保留十六进制字符
                string filteredPaste = Regex.Replace(pasteText, "[^0-9A-Fa-f]", "");
                filteredPaste = filteredPaste.ToUpper();

                // 限制粘贴长度
                TextBox textBox = sender as TextBox;
                int maxPasteLength = 6 - (textBox.Text.Length - 1); // 减去#号
                if (filteredPaste.Length > maxPasteLength)
                {
                    filteredPaste = filteredPaste.Substring(0, maxPasteLength);
                }

                // 用过滤后的文本替换粘贴板内容
                if (pasteText != filteredPaste)
                {
                    e.DataObject = new DataObject(filteredPaste);
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        #endregion
    }
}
