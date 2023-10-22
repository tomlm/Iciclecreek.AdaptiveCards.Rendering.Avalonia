using AdaptiveCards;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdaptiveCardViewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const string GreetingCard = @"
{
  ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
  ""type"": ""AdaptiveCard"",
  ""version"": ""1.0"",
  ""body"": [
    {
      ""type"": ""TextBlock"",
      ""text"": ""Welcome to AdaptiveCards on Avalonia "",
      ""size"": ""large""
    }
  ]
}
";
    private string _cardText;

    public MainViewModel()
    {
        this.Cards.Add(new CardModel()
        {
            Name = "test",
            Card = JsonConvert.DeserializeObject<AdaptiveCard>(/*lang=json,strict*/ GreetingCard)
        });

    }

    public string Greeting => "Welcome to Avalonia!";

    public ObservableCollection<CardModel> Cards { get; set; } = new ObservableCollection<CardModel>();
}
