using Serilog;
using System.IO;
using System.Text;

namespace PinPrompt.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// 将字符串内容异步写入文件
        /// </summary>
        /// <param name="filePath">保存路径</param>
        /// <param name="content">要写入的内容</param>
        /// <returns></returns>
        public static bool WriteToFile(string filePath, string content)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(filePath, content, Encoding.UTF8);
                Log.Logger.Information($"文件写入成功：{filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"文件写入失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从文件读取内容为字符串
        /// </summary>
        /// <param name="filePath">读取路径</param>
        /// <returns></returns>
        public static string ReadStringFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Logger.Error($"文件不存在：{filePath}");
                    return string.Empty;
                }
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                Log.Logger.Information($"文件读取成功：{filePath}");
                return content;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"文件读取失败：{ex.Message}");
                return string.Empty;
            }
        }
    }
}
