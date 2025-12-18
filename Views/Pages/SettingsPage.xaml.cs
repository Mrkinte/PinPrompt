using PinPrompt.ViewModels.Pages;
using System.Windows.Controls;

namespace PinPrompt.Views.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void RestoreConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            RestoreConfigPopup.IsOpen = !RestoreConfigPopup.IsOpen;
        }
    }
}
