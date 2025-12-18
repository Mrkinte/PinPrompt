using PinPrompt.ViewModels.Pages;
using System.Windows.Controls;

namespace PinPrompt.Views.Pages
{
    public partial class PromptEditPage
    {
        public PromptEditPage(PromptEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            RenameTextBox.Text = string.Empty;
            RenamePopup.IsOpen = !RenamePopup.IsOpen;
        }

        private void AddPromptButton_Click(object sender, RoutedEventArgs e)
        {
            PromptNameTextBox.Text = string.Empty;
            SetPromptNamePopup.IsOpen = !SetPromptNamePopup.IsOpen;
        }

        private void RemovePromptButton_Click(object sender, RoutedEventArgs e)
        {
            RemovePromptPopup.IsOpen = !RemovePromptPopup.IsOpen;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyPopup.IsOpen = !ApplyPopup.IsOpen;
        }

        private void DisableAllPromptsButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAllPromptsPopup.IsOpen = !DisableAllPromptsPopup.IsOpen;
        }

        private void BackgroundOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                int percent = (int)(slider.Value * 100);
                BackgroundOpacityPercentText.Text = $"{percent} %";
            }
        }

        private void PromptOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                int percent = (int)(slider.Value * 100);
                OpacityPercentText.Text = $"{percent} %";
            }
        }

        private void BackgroundRadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                int percent = (int)slider.Value;
                BackgroundRadiusText.Text = $"{percent} px";
            }
        }
    }
}
