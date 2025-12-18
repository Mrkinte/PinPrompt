using Wpf.Ui.Controls;

namespace PinPrompt.Controls
{
    /// <summary>
    /// SettingCard.xaml 的交互逻辑
    /// </summary>
    public partial class SettingCard : CardControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingCard),
                new PropertyMetadata("Title"));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(SettingCard),
                new PropertyMetadata("Description"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public SettingCard()
        {
            InitializeComponent();
        }
    }
}
