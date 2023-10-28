using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using AdaptiveCards.Rendering;
using System.Collections.Generic;

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
            var ver = ((ComboBoxItem)Schema?.SelectedItem)?.Content as string;
            if (ver != null)
            {
                viewModel.LoadSchema(ver);
            }
            else
            {
            }
        }
    }

    private void Schema_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel != null)
        {
            var item = e.AddedItems[0] as ComboBoxItem;
            var ver = item.Content as string;
            if (!string.IsNullOrEmpty(ver))
            {
                viewModel.LoadSchema(ver);
            }
        }
    }

}
