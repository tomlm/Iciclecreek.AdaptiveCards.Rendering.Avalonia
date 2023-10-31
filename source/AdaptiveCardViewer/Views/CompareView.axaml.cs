using AdaptiveCards.Rendering.Avalonia;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AdaptiveCardViewer.Views
{
    public partial class CompareView : Window
    {
        public CompareView()
        {
            InitializeComponent();
        }

        private void Edit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cardModel = ((Button)e.Source).DataContext as CardModel;

            // open cardModel.Uri in default browser
            if (cardModel != null)
            {
                Process.Start(new ProcessStartInfo(cardModel.Uri) { UseShellExecute = true });
            }
        }

        private async void OnAction(object? sender, RoutedAdaptiveActionEventArgs e)
        {
            if (e.Action is AdaptiveOpenUrlAction openUrlAction)
            {
                Process.Start(new ProcessStartInfo(openUrlAction.Url.AbsoluteUri) { UseShellExecute = true });
            }
            else if (e.Action is AdaptiveSubmitAction submitAction)
            {
                var inputs = e.UserInputs.AsJson();
                inputs.Merge(submitAction.Data);
                var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams()
                {
                    ContentTitle = "Action.Submit",
                    ContentMessage = $"{JsonConvert.SerializeObject(inputs, Formatting.Indented)}",
                    ButtonDefinitions = ButtonEnum.Ok,
                    MinWidth = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
                await box.ShowAsync();
            }
            else if (e.Action is AdaptiveExecuteAction executeAction)
            {
                var inputs = e.UserInputs.AsJson();
                inputs.Merge(executeAction.Data);
                var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams()
                {
                    ContentTitle = "Action.Execute",
                    ContentMessage = $"{JsonConvert.SerializeObject(inputs, Formatting.Indented)}",
                    ButtonDefinitions = ButtonEnum.Ok,
                    MinWidth = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
                await box.ShowAsync();
            }
        }
    }
}
