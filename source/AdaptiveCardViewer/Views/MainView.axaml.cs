using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using Newtonsoft.Json;

namespace AdaptiveCardViewer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;
        if (viewModel != null)
        {
            var item = e.AddedItems[0] as ComboBoxItem;
            var ver = item.Content as string;
            if (!string.IsNullOrEmpty(ver))
            {
                viewModel.Cards.Clear();
                foreach (var path in Directory.EnumerateFiles(@$"..\..\..\..\samples\{ver}", "*.json", SearchOption.AllDirectories)
                    .Where(p => !p.ToLower().Contains("tests")))
                {
                    Debug.WriteLine(path);
                    var json = File.ReadAllText(path);
                    if (json != null)
                    {
                        try
                        {
                            var card = JsonConvert.DeserializeObject<AdaptiveCard>(json);
                            viewModel.Cards.Add(new CardModel()
                            {
                                Name = Path.GetFileName(path),
                                Card = card
                            });
                        }
                        catch (Exception err)
                        {
                            Debug.WriteLine(err);
                        }
                    }
                }

                // viewModel.CardText = File.ReadAllText(ver);
            }
        }
    }

}
