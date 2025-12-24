using PinPrompt.Helpers;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace PinPrompt.Controls.PromptWindow
{
    /// <summary>
    /// 附加属性
    /// </summary>
    internal class AttachedProperties
    {
        // IsActivated Property
        public static readonly DependencyProperty IsActivatedProperty =
            DependencyProperty.RegisterAttached("IsActivated", typeof(bool), typeof(AttachedProperties),
                new PropertyMetadata(false, OnIsActivatedChanged));

        public static bool GetIsActivated(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsActivatedProperty);
        }

        public static void SetIsActivated(DependencyObject obj, bool value)
        {
            obj.SetValue(IsActivatedProperty, value);
        }

        private static void OnIsActivatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window &&
                e.NewValue is bool newState &&
                window.IsInitialized) // 确保窗口已初始化
            {
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (newState)
                        {
                            MouseThroughHelper.EnableMouseThrough(window);
                        }
                        else
                        {
                            MouseThroughHelper.DisableMouseThrough(window);
                        }
                    }
                    catch { }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        // IsActivated Property
        public static readonly DependencyProperty IsSnapToEdgeProperty =
            DependencyProperty.RegisterAttached("IsSnapToEdge", typeof(bool), typeof(AttachedProperties),
                new PropertyMetadata(false));

        public static bool GetIsSnapToEdge(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSnapToEdgeProperty);
        }

        public static void SetIsSnapToEdge(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSnapToEdgeProperty, value);
        }

        // ResizeMode Property
        public static readonly DependencyProperty ResizeModeProperty =
            DependencyProperty.RegisterAttached("ResizeMode", typeof(ResizeMode), typeof(AttachedProperties),
                new PropertyMetadata(ResizeMode.NoResize, OnResizeModeChanged));

        public static ResizeMode GetResizeMode(DependencyObject obj)
        {
            return (ResizeMode)obj.GetValue(ResizeModeProperty);
        }

        public static void SetResizeMode(DependencyObject obj, ResizeMode value)
        {
            obj.SetValue(ResizeModeProperty, value);
        }

        private static void OnResizeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window &&
                e.NewValue is ResizeMode newMode &&
                window.IsInitialized) // 确保窗口已初始化
            {
                // 通过 Dispatcher 延迟执行，提高成功率
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { window.ResizeMode = newMode; } catch { }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
    }

    /// <summary>
    /// LocalWatermarkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PromptWindow : Window
    {
        private double _snapThreshold = 15;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;

        public PromptWindow(PromptWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            this.SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            // 获取并改变窗口样式，不在Alt+Tab中显示
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
            // 刷新窗口
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        #region 窗口拖动相关方法

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!AttachedProperties.GetIsActivated(this))
            {
                Cursor = Cursors.SizeAll;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AttachedProperties.GetIsActivated(this))
            {
                DragMove();
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AttachedProperties.GetIsSnapToEdge(this))
            {
                SnapToEdge();
            }
        }

        private void SnapToEdge()
        {
            var (screenWidth, screenHeight) = ScreenHelper.GetLogicalScreenSize(this, useWorkArea: true);

            // 上边缘
            if (Top < _snapThreshold)
            {
                Top = 0;
            }

            // 下边缘
            if ((screenHeight - (Top + Height)) < _snapThreshold)
            {
                Top = screenHeight - Height;
            }

            // 左边缘
            if (Left < _snapThreshold)
            {
                Left = 0;
            }

            // 右边缘
            if ((screenWidth - (Left + Width)) < _snapThreshold)
            {
                Left = screenWidth - Width;
            }
        }

        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
