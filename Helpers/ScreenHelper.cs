using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace PinPrompt.Helpers
{

    public static class ScreenHelper
    {
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;     // 全屏区域（含任务栏）
            public RECT rcWork;        // 工作区域（不含任务栏）
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// 获取当前窗口所在屏幕的逻辑尺寸（与鼠标事件坐标同单位：DIP）
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="useWorkArea">true=返回工作区尺寸(不含任务栏)，false=返回全屏尺寸</param>
        /// <returns>(宽度, 高度) 元组，单位：DIP（逻辑像素）</returns>
        public static (double Width, double Height) GetLogicalScreenSize(
            Window window,
            bool useWorkArea = false)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            // 1. 获取窗口句柄
            var hwnd = new WindowInteropHelper(window).EnsureHandle();

            // 2. 获取窗口所在显示器句柄
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero)
                throw new InvalidOperationException("无法获取显示器句柄");

            // 3. 获取显示器物理区域
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (!GetMonitorInfo(monitor, ref mi))
                throw new InvalidOperationException("无法获取显示器信息");

            // 4. 获取当前显示器的DPI缩放比例
            var dpi = VisualTreeHelper.GetDpi(window);

            // 5. 计算物理像素尺寸
            int physicalWidth = useWorkArea
                ? mi.rcWork.right - mi.rcWork.left
                : mi.rcMonitor.right - mi.rcMonitor.left;

            int physicalHeight = useWorkArea
                ? mi.rcWork.bottom - mi.rcWork.top
                : mi.rcMonitor.bottom - mi.rcMonitor.top;

            // 6. 转换为逻辑像素 (DIP) - 与鼠标坐标单位一致
            double logicalWidth = physicalWidth / dpi.DpiScaleX;
            double logicalHeight = physicalHeight / dpi.DpiScaleY;

            return (logicalWidth, logicalHeight);
        }
    }
}
