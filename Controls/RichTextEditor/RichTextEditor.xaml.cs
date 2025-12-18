using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PinPrompt.Controls.RichTextEditor
{
    /// <summary>
    /// RichTextEditor.xaml 的交互逻辑
    /// </summary>
    public partial class RichTextEditor : UserControl
    {
        private bool _suppressDocumentUpdate = false;

        public RichTextEditor()
        {
            InitializeComponent();

            // 将内部区域的数据上下文设置为内部ViewModel
            Loaded += (s, e) =>
            {
                if (InternalContentArea != null)
                {
                    InternalContentArea.DataContext = new RichTextEditorViewModel();
                }

                // 初始同步
                if (Document != null)
                    RichTextBox.Document = Document;
            };

            RichTextBox.TextChanged += OnRichTextBoxTextChanged;
        }

        private void OnRichTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressDocumentUpdate) return;

            _suppressDocumentUpdate = true;
            SetCurrentValue(DocumentProperty, RichTextBox.Document);
            _suppressDocumentUpdate = false;
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument), typeof(RichTextEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDocumentChanged));

        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RichTextEditor;
            if (control == null)
                return;

            if (control._suppressDocumentUpdate || control.RichTextBox == null)
                return;

            // 只有当文档确实不同时才更新
            if (!ReferenceEquals(control.RichTextBox.Document, e.NewValue))
            {
                control._suppressDocumentUpdate = true;

                // 使用Dispatcher确保在UI线程空闲时执行
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        control.RichTextBox.Document = e.NewValue as FlowDocument ?? new FlowDocument();
                    }
                    catch (InvalidOperationException)
                    {
                        // 忽略此异常，通常是由于RichTextBox内部状态导致的
                    }
                    finally
                    {
                        control._suppressDocumentUpdate = false;
                    }
                }), DispatcherPriority.ContextIdle);
            }
        }
    }
}