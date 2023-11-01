using AdaptiveCards.Rendering.Avalonia;
using AdaptiveCards.Rendering;
using AdaptiveCards;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AdaptiveCardViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AddHandler(AdaptiveCardView.ActionEvent, (object? sender, RoutedAdaptiveActionEventArgs e) =>  App.OnAdaptiveAction(this, sender, e));
    }
}
