using AdaptiveCards;
using AdaptiveCards.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
    public MainViewModel()
    {
        AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(GreetingCard);


        this.Cards.Add(new CardModel()
        {
            Name = "welcome.json",
            Card = parseResult.Card
        });
        LoadHostConfig("microsoft-teams-dark");
    }

    public string Greeting => "Welcome to Avalonia!";

    public ObservableCollection<CardModel> Cards { get; set; } = new ObservableCollection<CardModel>();

    private AdaptiveHostConfig _hostConfig;
    public AdaptiveHostConfig HostConfig { get => _hostConfig; set => base.SetProperty(ref _hostConfig, value); }

    public void LoadHostConfig(string hostConfigName)
    {
        Debug.WriteLine(hostConfigName);
        var hostConfigPath = Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .FirstOrDefault(p => p.Contains(hostConfigName));
        string json = null;
        using (TextReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(hostConfigPath)))
        {
            json = reader.ReadToEnd();
        }

        if (json != null)
        {
            try
            {
                this.HostConfig = JsonConvert.DeserializeObject<AdaptiveHostConfig>(json);
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
            }
        }
    }

    public void LoadSchema(string ver)
    {
        ver = ver.Replace(".", "._");
        this.Cards.Clear();

        foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(p => p.Contains(ver) && !p.ToLower().Contains("tests")))
        {
            Debug.WriteLine(name);
            string json = null;
            using (TextReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(name)))
            {
                json = reader.ReadToEnd();
            }

            if (json != null)
            {
                try
                {
                    AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(json);
                    this.Cards.Add(new CardModel()
                    {
                        Name = Path.GetFileName(name),
                        Card = parseResult.Card
                    });
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err);
                }
            }
        }

    }
}
