using PinPrompt.Controls.ColorSelector;
using PinPrompt.Controls.RichTextEditor;
using PinPrompt.Helpers;
using PinPrompt.Models;
using PinPrompt.Services;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace PinPrompt.ViewModels.Pages
{
    public partial class PromptEditViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        private PromptConfig? _promptConfig;
        private readonly PromptService _promptService;
        private readonly NotificationService _notificationService;

        // 保存窗体原始位置
        private double _oldLeftPosition = 0;
        private double _oldTopPosition = 0;

        [ObservableProperty]
        private FlowDocument? _promptDocument;

        [ObservableProperty]
        private string _currentName = string.Empty; 

        [ObservableProperty]
        private ObservableCollection<string>? _nameList;

        [ObservableProperty]
        private bool _isActivated;

        [ObservableProperty]
        private Brush _backgroundColor = Brushes.Transparent;     //背景色

        [ObservableProperty]
        private double _backgroundOpacity;  // 背景透明度

        [ObservableProperty]
        private double _backgroundRadius;   // 背景圆角

        [ObservableProperty]
        private double _opacity;            // 文本透明度

        [ObservableProperty]
        private bool _isTop;                // 是否置顶显示

        [ObservableProperty]
        private bool _isSnapToEdge;         // 是否启用边缘吸附

        [ObservableProperty]
        private bool _isPreviewing;         // 预览中

        [ObservableProperty]
        private bool _isAutoSize;           // 自动尺寸

        public PromptEditViewModel(PromptService promptService, NotificationService notificationService)
        {
            _promptService = promptService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// 当前提示变更时，更新显示参数。
        /// </summary>
        /// <param name="value">当前选中的提示名</param>
        partial void OnCurrentNameChanged(string value)
        {
            OnClosePreviewPrompt();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            _promptConfig = _promptService.GetPromptConfig(value).Clone();
            IsActivated = _promptConfig.IsActivated;
            BackgroundColor = ColorSelectorHelper.HexToBrush(_promptConfig.BackgroundColor);
            BackgroundOpacity = _promptConfig.BackgroundOpacity;
            BackgroundRadius = _promptConfig.BackgroundRadius;
            Opacity = _promptConfig.Opacity;
            IsTop = _promptConfig.IsTop;
            IsSnapToEdge = _promptConfig.IsSnapToEdge;
            IsAutoSize = _promptConfig.IsAutoSize;
            PromptDocument = RichTextEditorHelper.XamlToFlowDocumentConverter(_promptConfig.XamlBuffer) ?? new FlowDocument();
        
            _oldLeftPosition = _promptConfig.LeftPosition;
            _oldTopPosition = _promptConfig.TopPosition;
        }

        #region 预览提示实时响应

        partial void OnBackgroundColorChanged(Brush value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.BackgroundColor = ColorSelectorHelper.BrushToHex(value);
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        partial void OnBackgroundOpacityChanged(double value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.BackgroundOpacity = value;
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        partial void OnBackgroundRadiusChanged(double value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.BackgroundRadius = value;
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        partial void OnOpacityChanged(double value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.Opacity = value;
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        partial void OnIsSnapToEdgeChanged(bool value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.IsSnapToEdge = value;
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        partial void OnIsAutoSizeChanged(bool value)
        {
            if (_isPreviewing)
            {
                _promptConfig!.IsAutoSize = value;
                _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
                _promptService.PreviewPrompt(_promptConfig);
            }
        }

        #endregion

        /// <summary>
        /// 提示重命名
        /// </summary>
        /// <param name="newName"></param>
        [RelayCommand]
        private void OnRenamePrompt(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                _notificationService.Show("错误", "提示名称不能为空", InfoBarSeverity.Error);
                return;
            }
            if (NameList!.Contains(newName))
            {
                _notificationService.Show("错误", "提示名称已存在，请更换名称后重试", InfoBarSeverity.Error);
                return;
            }
            _promptService.RenamePrompt(CurrentName, newName);
            NameList = _promptService.AllPromptNames;
            CurrentName = newName;
        }

        /// <summary>
        /// 添加新提示
        /// </summary>
        /// <param name="name"></param>
        [RelayCommand]
        private void OnAddPrompt(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _notificationService.Show("错误", "提示名称不能为空", InfoBarSeverity.Error);
                return;
            }
            if (NameList!.Contains(name))
            {
                _notificationService.Show("错误", "提示名称已存在，请更换名称后重试", InfoBarSeverity.Error);
                return;
            }
            _promptService.AddPrompt(name);
            NameList = _promptService.AllPromptNames;
            CurrentName = name;
        }

        /// <summary>
        /// 删除提示
        /// </summary>
        [RelayCommand]
        private void OnRemovePrompt()
        {
            _promptService.RemovePrompt(CurrentName);
            _notificationService.Show("成功", $"提示信息 {CurrentName} 已删除", InfoBarSeverity.Success);
            NameList = _promptService.AllPromptNames;
            CurrentName = NameList.First();
        }

        /// <summary>
        /// 预览提示
        /// </summary>
        [RelayCommand]
        private void OnPreviewPrompt()
        {
            if (RichTextEditorHelper.IsFlowDocumentEmpty(PromptDocument!))
            {
                _notificationService.Show("提示", "无有效内容，请先在编辑框中输入提示信息。", InfoBarSeverity.Warning);
                return;
            }
            _promptConfig!.IsActivated = false;
            _promptConfig.BackgroundColor = ColorSelectorHelper.BrushToHex(BackgroundColor);
            _promptConfig.BackgroundOpacity = BackgroundOpacity;
            _promptConfig.BackgroundRadius = BackgroundRadius;
            _promptConfig.Opacity = Opacity;
            _promptConfig.IsTop = true;
            _promptConfig.IsSnapToEdge = IsSnapToEdge;
            _promptConfig.IsAutoSize = IsAutoSize;
            _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
            _promptService.PreviewPrompt(_promptConfig);
            _promptService.PreviewPromptManualClosed += PreviewPromptManualClosed;
            IsPreviewing = true;
        }

        /// <summary>
        /// 关闭提示预览
        /// </summary>
        [RelayCommand]
        private void OnClosePreviewPrompt()
        {
            _promptService.ClosePreviewPrompt();
            IsPreviewing = false;
        }

        /// <summary>
        /// 应用并保存提示参数
        /// </summary>
        [RelayCommand]
        private void OnApplyConfigs()
        {
            _promptConfig!.BackgroundColor = ColorSelectorHelper.BrushToHex(BackgroundColor);
            _promptConfig.BackgroundOpacity = BackgroundOpacity;
            _promptConfig.BackgroundRadius = BackgroundRadius;
            _promptConfig.IsActivated = IsActivated;
            _promptConfig.Opacity = Opacity;
            _promptConfig.IsTop = IsTop;
            _promptConfig.IsSnapToEdge = IsSnapToEdge;
            _promptConfig.IsAutoSize = IsAutoSize;
            _promptConfig.XamlBuffer = RichTextEditorHelper.FlowDocumentToXamlConverter(PromptDocument!);
            if (!IsPreviewing)
            {
                _promptConfig.LeftPosition = _oldLeftPosition;
                _promptConfig.TopPosition = _oldTopPosition;
            }
            _oldLeftPosition = _promptConfig.LeftPosition;
            _oldTopPosition = _promptConfig.TopPosition;
            var showWindow = IsActivated;
            if (RichTextEditorHelper.IsFlowDocumentEmpty(PromptDocument!) && IsActivated)
            {
                _notificationService.Show("提示", "启用了提示但未编辑具体提示信息", InfoBarSeverity.Warning);
                showWindow = false;
            }
            _promptService.UpdateAndSavePromptConfig(CurrentName, _promptConfig, showWindow);
            OnClosePreviewPrompt();
            _notificationService.Show("成功", "设置已保存", InfoBarSeverity.Success);
        }

        /// <summary>
        /// 关闭所有提示
        /// </summary>
        [RelayCommand]
        private void OnDisableAllPrompts()
        {
            IsActivated = false;
            OnClosePreviewPrompt();
            _promptService.DisableAllPrompts();
            _notificationService.Show("成功", "已禁用所有提示信息", InfoBarSeverity.Success);
        }

        /// <summary>
        /// 在预览提示窗体上主动关闭时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewPromptManualClosed(object? sender, EventArgs e) => OnClosePreviewPrompt();

        #region INavigationAware methods

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            NameList = _promptService.AllPromptNames;
            CurrentName = NameList.First();
            _isInitialized = true;
        }

        #endregion
    }
}
