using AdaptiveCardViewer.ViewModels;
using AdaptiveCardViewer.Views;
using AvaloniaWebView;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AdaptiveCards.Rendering.Avalonia;
using AdaptiveCards.Rendering;
using AdaptiveCards;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AdaptiveCardViewer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    public override void RegisterServices()
    {
        base.RegisterServices();

        // if you use only WebView  
        AvaloniaWebViewBuilder.Initialize(default);
    }

    public static async void OnAdaptiveAction(Window window, object? sender, RoutedAdaptiveActionEventArgs e)
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
            var cardView = new AdaptiveCardView()
            {
                Card = showCardAction.Card,
                HostConfig = e.HostConfig
            };

            // route it back to the original window (so it can be bubbled up again)
            cardView.Action += (sender, e) =>
            {
                if (!(e.Action is AdaptiveShowCardAction))
                    window.RaiseEvent(e);
            };

            var dialog = new AdaptiveCardWindow()
            {
                Title = showCardAction.Title,
                CardView = cardView,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 500,
                Height = 500
            };

            dialog.ShowDialog(window);
        }
    }
}
