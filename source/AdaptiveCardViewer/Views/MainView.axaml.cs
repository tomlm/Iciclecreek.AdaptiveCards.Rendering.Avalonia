using System.Diagnostics;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using Newtonsoft.Json;
using AdaptiveCards.Rendering.Avalonia;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Interactivity;
using MsBox.Avalonia.Dto;
using Newtonsoft.Json.Linq;

namespace AdaptiveCardViewer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void HostConfig_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel != null)
        {
            var item = e.AddedItems[0] as ComboBoxItem;
            var hostConfigName = item.Content as string;
            viewModel.LoadHostConfig(hostConfigName);
        }
    }

    private void Version_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel != null)
        {
            var item = e.AddedItems[0] as ComboBoxItem;
            var ver = item.Content as string;
            if (!string.IsNullOrEmpty(ver))
            {
                viewModel.LoadVersionSamples(ver);
            }
        }
    }

    private void Edit_Click(object? sender, RoutedEventArgs e)
    {
        var cardModel = ((Button)e.Source).DataContext as CardModel;

        // open cardModel.Uri in default browser
        if (cardModel != null)
        {
            Process.Start(new ProcessStartInfo(cardModel.Uri) { UseShellExecute = true });
        }
    }

    private void Compare_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new CompareView();
        dialog.DataContext = ((Control)sender).DataContext;
        dialog.Show();
    }

    private async void OnAction(object? sender, RoutedAdaptiveActionEventArgs e)
    {
        if (e.Action is AdaptiveOpenUrlAction openUrlAction)
        {
            Process.Start(new ProcessStartInfo(openUrlAction.Url.AbsoluteUri) { UseShellExecute = true });
        }
        //else if (args.Action is AdaptiveShowCardAction showCardAction)
        //{
        //    // Action.ShowCard can be rendered inline automatically
        //    // ... but if you want to handle show card as a "popup", you handle this event
        //    if (_myHostConfig.Actions.ShowCard.ActionMode == ShowCardActionMode.Popup)
        //    {
        //        var dialog = new ShowCardWindow(showCardAction.Title, showCardAction, Resources);
        //        dialog.Owner = this;
        //        dialog.ShowDialog();
        //    }
        //}
        else if (e.Action is AdaptiveSubmitAction submitAction)
        {
            string payload = string.Empty;
            var inputs = e.UserInputs.AsJson();
            if (submitAction.Data is JObject jobj)
            {
                inputs.Merge(jobj);
                payload = JsonConvert.SerializeObject(inputs, Formatting.Indented);
            }
            else
                payload = JsonConvert.SerializeObject(submitAction.Data);

            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams()
            {
                ContentTitle = "Action.Submit",
                ContentMessage = payload,
                ButtonDefinitions = ButtonEnum.Ok,
                MinWidth = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await box.ShowAsync();
        }
        else if (e.Action is AdaptiveExecuteAction executeAction)
        {
            string payload = string.Empty;
            var inputs = e.UserInputs.AsJson();
            if (executeAction.Data is JObject jobj)
            {
                inputs.Merge(jobj);
                payload = JsonConvert.SerializeObject(inputs, Formatting.Indented);
            }
            else
                payload = JsonConvert.SerializeObject(executeAction.Data);
            
            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams()
            {
                ContentTitle = "Action.Execute",
                ContentMessage = payload,
                ButtonDefinitions = ButtonEnum.Ok,
                MinWidth = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });
            await box.ShowAsync();
        }
    }
}
