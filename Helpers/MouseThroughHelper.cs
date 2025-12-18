using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace PinPrompt.Helpers
{
    public class MouseThroughHelper
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        /// <summary>
        /// 启用鼠标穿透
        /// </summary>
        /// <param name="window"></param>
        public static void EnableMouseThrough(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        /// <summary>
        /// 禁用鼠标穿透
        /// </summary>
        /// <param name="window"></param>
        public static void DisableMouseThrough(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
        }
    }
}
