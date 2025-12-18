using PinPrompt.Controls.PromptWindow;
using PinPrompt.Helpers;
using PinPrompt.Models;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;

namespace PinPrompt.Services
{
    internal class Prompt
    {
        private PromptWindowViewModel _viewModel { get; }
        private PromptWindow _window;

        public PromptConfig Config { get; set; }
        public PromptWindow Window
        {
            get
            {
                _viewModel.UpdateConfig(Config);    // 先更新配置再返回窗口实例
                return _window;
            }
        }

        public Prompt(PromptConfig config)
        {
            Config = config;
            _viewModel = new PromptWindowViewModel(config);
            _window = new PromptWindow(_viewModel);

            if (Config.IsActivated)
            {
                Window.Show();
            }
        }
    }

    public class PromptService
    {
        private readonly ILogger _logger;
        private readonly AppConfigService _configService;
        private readonly List<Prompt> _promptList;
        private readonly List<PromptConfig> _promptConfigList;

        // 预览提示信息
        private Prompt? _previewPrompt;
        public event EventHandler PreviewPromptManualClosed;    // 由预览窗口右键菜单“关闭”时触发

        // 属性
        private ObservableCollection<string> _allPromptNames = new ObservableCollection<string>();
        public ObservableCollection<string> AllPromptNames
        {
            get
            {
                _allPromptNames.Clear();
                foreach (var prompt in _promptList)
                {
                    _allPromptNames.Add(prompt.Config.Name);
                }
                return _allPromptNames;
            }
        }

        public PromptService(ILogger logger, AppConfigService configService)
        {
            _logger = logger;
            _configService = configService;
            _promptList = new List<Prompt>();
            _promptConfigList = configService.PromptConfigList;
            foreach (var config in _promptConfigList)
            {
                // 根据配置文件创建提示信息
                if (!string.IsNullOrEmpty(config.Guid))
                {
                    // 从硬盘加载提示信息内容
                    string filePath = Path.Combine(_configService.PromptFolder, config.Guid + ".xaml");
                    config.XamlBuffer = FileHelper.ReadStringFromFile(filePath);
                }
                var prompt = new Prompt(config);
                _promptList.Add(prompt);
            }
        }

        /// <summary>
        /// 获取提示信息配置
        /// </summary>
        /// <param name="promptName">提示信息名称</param>
        /// <returns></returns>
        public PromptConfig GetPromptConfig(string promptName)
        {
            foreach (var prompt in _promptList)
            {
                if (promptName == prompt.Config.Name)
                {
                    return prompt.Config;
                }
            }
            _logger.Error($"未找到提示信息配置：{promptName}");
            return null;
        }

        /// <summary>
        /// 提示信息重命名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void RenamePrompt(string oldName, string newName)
        {
            var config = GetPromptConfig(oldName);
            config.Name = newName;
            _configService.SaveConfig();
        }

        /// <summary>
        /// 添加新提示信息
        /// </summary>
        /// <param name="promptName"></param>
        public void AddPrompt(string promptName)
        {
            var newConfig = new PromptConfig { Name = promptName };
            var newPrompt = new Prompt(newConfig);
            _promptConfigList.Add(newConfig);
            _promptList.Add(newPrompt);
            _configService.SaveConfig();
        }

        /// <summary>
        /// 删除提示信息
        /// </summary>
        /// <param name="promptName"></param>
        public void RemovePrompt(string promptName)
        {
            foreach (var prompt in _promptList)
            {
                if (prompt.Config.Name == promptName)
                {
                    prompt.Window.Close();
                    _promptConfigList.Remove(prompt.Config);
                    _promptList.Remove(prompt);
                    string filePath = Path.Combine(_configService.PromptFolder, prompt.Config.Guid + ".xaml");
                    if (!File.Exists(filePath))
                    {
                        break;
                    }
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Prompt保存在硬盘的xaml文件删除失败：{ex.Message}");
                    }
                    break;
                }
            }

            if (_promptConfigList.Count <= 0)
            {
                AddPrompt("默认提示");
            }
            else
            {
                _configService.SaveConfig();
            }
        }

        /// <summary>
        /// 显示提示信息窗口
        /// </summary>
        /// <param name="promptName"></param>
        public void ShowPromptWindow(string promptName)
        {
            foreach (var prompt in _promptList)
            {
                if (promptName == prompt.Config.Name)
                {
                    prompt.Window.Show();
                    break;
                }
            }
        }

        /// <summary>
        /// 隐藏提示信息窗口
        /// </summary>
        /// <param name="promptName"></param>
        public void HidePromptWindow(string promptName)
        {
            foreach (var prompt in _promptList)
            {
                if (promptName == prompt.Config.Name)
                {
                    prompt.Window.Hide();
                    break;
                }
            }
        }

        /// <summary>
        /// 根据配置文件打开一个提示信息预览窗口
        /// </summary>
        /// <param name="config"></param>
        public void PreviewPrompt(PromptConfig config)
        {
            if (_previewPrompt == null)
            {
                config.LeftPosition = 0;
                config.TopPosition = 0;
                _previewPrompt = new Prompt(config);
                _previewPrompt.Window.Closed += (s, e) =>
                {
                    PreviewPromptManualClosed?.Invoke(this, EventArgs.Empty);
                };
            }
            if (_previewPrompt.Config != config)
            {
                config.LeftPosition = 0;
                config.TopPosition = 0;
                _previewPrompt.Config = config;
            }
            _previewPrompt.Window.Show();
        }

        /// <summary>
        /// 关闭已经打开的提示信息预览窗口
        /// </summary>
        public void ClosePreviewPrompt()
        {
            _previewPrompt?.Window.Close();
            _previewPrompt = null;
        }


        /// <summary>
        /// 更新并保存提示信息参数
        /// </summary>
        /// <param name="promptName"></param>
        /// <param name="newConfig"></param>
        public void UpdateAndSavePromptConfig(string promptName, PromptConfig newConfig, bool showWindow)
        {
            foreach (var prompt in _promptList)
            {
                if (promptName == prompt.Config.Name)
                {
                    var oldConfig = prompt.Config;
                    if (string.IsNullOrEmpty(oldConfig.Guid))
                    {
                        oldConfig.Guid = Guid.NewGuid().ToString();
                    }
                    oldConfig.LeftPosition = newConfig.LeftPosition;
                    oldConfig.TopPosition = newConfig.TopPosition;
                    oldConfig.IsActivated = newConfig.IsActivated;
                    oldConfig.BackgroundColor = newConfig.BackgroundColor;
                    oldConfig.BackgroundOpacity = newConfig.BackgroundOpacity;
                    oldConfig.BackgroundRadius = newConfig.BackgroundRadius;
                    oldConfig.Opacity = newConfig.Opacity;
                    oldConfig.IsTop = newConfig.IsTop;
                    oldConfig.IsSnapToEdge = newConfig.IsSnapToEdge;
                    oldConfig.IsAutoSize = newConfig.IsAutoSize;
                    oldConfig.Width = newConfig.Width;
                    oldConfig.Height = newConfig.Height;
                    oldConfig.XamlBuffer = newConfig.XamlBuffer;
                    HidePromptWindow(promptName);
                    if (showWindow)
                    {
                        ShowPromptWindow(promptName);
                    }
                    string filePath = Path.Combine(_configService.PromptFolder, oldConfig.Guid + ".xaml");
                    FileHelper.WriteToFile(filePath, oldConfig.XamlBuffer);
                    _configService.SaveConfig();
                    break;
                }
            }
        }

        /// <summary>
        /// 禁用所有提示信息窗口
        /// </summary>
        public void DisableAllPrompts()
        {
            foreach (var prompt in _promptList)
            {
                prompt.Config.IsActivated = false;
                prompt.Window.Hide();
            }
            _configService.SaveConfig();
        }
    }
}
