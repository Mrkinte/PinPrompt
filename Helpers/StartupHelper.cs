using Microsoft.Win32;
using Serilog;
using System.Diagnostics;

namespace PinPrompt.Helpers
{
    public class StartupHelper
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "PinPrompt";

        public static bool SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return false;

                    if (enable)
                    {
                        // 获取当前可执行文件路径
                        string exePath = Process.GetCurrentProcess().MainModule.FileName;

                        // 或者使用 Assembly 获取路径
                        // string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                        // 如果是调试环境，可能需要处理路径
#if DEBUG
                        exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
#endif
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                    return true;
                }
            }
            catch
            {
                throw;  // 抛出异常，由UI线程处理。
            }
        }

        public static bool IsStartupEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
            {
                if (key == null) return false;
                return key.GetValue(AppName) != null;
            }
        }
    }
}
