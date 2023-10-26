using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using AdaptiveCards;
using AdaptiveCardViewer.ViewModels;
using Avalonia.Controls;
using Newtonsoft.Json;
using System.Reflection;

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
                ver = ver.Replace(".", "._");
                viewModel.Cards.Clear();
                foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Where(p => p.Contains(ver) && !p.ToLower().Contains("tests")))
                {
                    Debug.WriteLine(name);
                    string json = null;
                    using(TextReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name))) 
                    {
                        json = reader.ReadToEnd();
                    }
                        
                    if (json != null)
                    {
                        try
                        {
                            var card = JsonConvert.DeserializeObject<AdaptiveCard>(json);
                            viewModel.Cards.Add(new CardModel()
                            {
                                Name = Path.GetFileName(name),
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
