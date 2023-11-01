using System.Diagnostics;
using AdaptiveCards.Rendering.Avalonia;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

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

}

