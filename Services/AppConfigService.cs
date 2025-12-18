using Microsoft.Extensions.Configuration;
using PinPrompt.Models;
using Serilog;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace PinPrompt.Services
{
    public class AppConfigService
    {
        private readonly ILogger _logger;
        private readonly string _configFilePath;
        private readonly AppConfig _appConfig;
        private readonly IConfigurationRoot? _configuration;

        public string ConfigFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Config");
        public string PromptFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Config", "Prompts");
        public static string LogFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Logs");
        public UserConfig UserConfig
        {
            get => _appConfig.UserConfig;
            set { _appConfig.UserConfig = value; }
        }

        public List<PromptConfig> PromptConfigList
        {
            get => _appConfig.PromptConfigList;
            set { _appConfig.PromptConfigList = value; }
        }

        public AppConfigService(ILogger logger)
        {
            _logger = logger;
            _configFilePath = Path.Combine(ConfigFolder, "AppConfig.json");
            _appConfig = new AppConfig();

            try
            {
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                    Directory.CreateDirectory(PromptFolder);
                    CopyDefaultConfig();
                }

                _configuration = new ConfigurationBuilder()
                    .AddJsonFile(_configFilePath)
                    .Build();

                _appConfig.UserConfig = _configuration.GetSection("UserConfig").Get<UserConfig>() ?? new UserConfig();
                _appConfig.PromptConfigList = _configuration.GetSection("PromptConfigList").Get<List<PromptConfig>>()
                    ?? new List<PromptConfig> { new PromptConfig() };
            }
            catch (Exception ex)
            {
                _logger.Warning($"配置文件加载失败：{ex.Message}");
                _logger.Information("使用默认配置启动");
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                _logger.Information("保存配置文件");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_appConfig, options);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.Error($"配置文件保存失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 创建默认配置文件和默认提示文件
        /// </summary>
        private void CopyDefaultConfig()
        {
            _logger.Information("创建默认配置文件和默认提示文件");

            ResourceFile configFile = new ResourceFile
            {
                Name = "AppConfig.json",
                Path = "PinPrompt.Assets.Default.AppConfig.json",
                Type = "json"
            };
            ResourceFile PromptFile1 = new ResourceFile
            {
                Name = "4e727e54-abc9-472f-924e-3b6b86a48712.xaml",
                Path = "PinPrompt.Assets.Default.Prompts.4e727e54-abc9-472f-924e-3b6b86a48712.xaml",
                Type = "xaml"
            };
            ResourceFile PromptFile2 = new ResourceFile
            {
                Name = "79b0fb5e-109b-4f67-ac47-67e95be3f267.xaml",
                Path = "PinPrompt.Assets.Default.Prompts.79b0fb5e-109b-4f67-ac47-67e95be3f267.xaml",
                Type = "xaml"
            };
            var resourceFiles = new List<ResourceFile>
                {
                    configFile,
                    PromptFile1,
                    PromptFile2
                };

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var resourceFile in resourceFiles)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceFile.Path)!)
                {
                    if (stream != null)
                    {
                        string fileName = Path.GetFileName(resourceFile.Name);
                        string destPath = string.Empty;
                        if (resourceFile.Type == "json")
                        {
                            destPath = Path.Combine(ConfigFolder, fileName);
                        }
                        else if (resourceFile.Type == "xaml")
                        {
                            destPath = Path.Combine(PromptFolder, fileName);
                        }

                        using (FileStream fileStream = new FileStream(destPath, FileMode.Create))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
        }
    }

    internal class ResourceFile
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required string Type { get; set; }
    }
}
