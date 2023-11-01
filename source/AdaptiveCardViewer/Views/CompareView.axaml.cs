using AdaptiveCards.Rendering.Avalonia;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Diagnostics;
using AdaptiveCards.Rendering;
using Avalonia.VisualTree;

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
                e.Handled = true;
            }
            else if (e.IsDataAction())
            {
                var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams()
                {
                    ContentTitle = e.Action.GetType().Name.Replace("Adaptive", string.Empty),
                    ContentMessage = JsonConvert.SerializeObject(e.GetActionPayload(), Formatting.Indented),
                    ButtonDefinitions = ButtonEnum.Ok,
                    MinWidth = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
                await box.ShowAsync();
                e.Handled = true;
            }
            else if (e.Action is AdaptiveShowCardAction showCardAction && e.HostConfig.Actions.ShowCard.ActionMode == ShowCardActionMode.Popup)
            {
                var dialog = new AdaptiveCardWindow()
                {
                    Title = showCardAction.Title,
                    Card = showCardAction.Card,
                    HostConfig = e.HostConfig,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 500,
                    Height = 500
                };
                dialog.ShowDialog(this.FindAncestorOfType<Window>());
            }

        }
    }
}
