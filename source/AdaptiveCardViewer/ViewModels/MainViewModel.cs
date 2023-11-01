using AdaptiveCards;
using AdaptiveCards.Rendering;
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
        LoadHostConfig("microsoft-teams-light");
        var json = GreetingCard;
        string name = "Welcome";
        string path = null;
        if (Debugger.IsAttached)
        {
            path = Path.GetFullPath(@"C:\source\github\AdaptiveCards.Rendering.Avalonia\source\AdaptiveCardViewer\samples\v1.2\Elements\Action.OpenUrl.Style.json");
            json = File.ReadAllText(path);
            name = Path.GetFileName(path);
        }

        AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(json);

        this.Cards.Add(new CardModel()
        {
            Uri = $"file:{path}",
            Name = name,
            Card = parseResult.Card,
            HostConfig = this.HostConfig
        });
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
                this.HostConfig = AdaptiveHostConfig.FromJson(json);
                foreach (var card in this.Cards)
                    card.HostConfig = this.HostConfig;
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
            }
        }
    }

    public void LoadVersionSamples(string ver)
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
                    string fullPath = GetResourcePath(name);
                    AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(json);
                    this.Cards.Add(new CardModel()
                    {
                        Name = Path.GetFileName(fullPath),
                        Uri = fullPath,
                        Card = parseResult.Card,
                        HostConfig = this.HostConfig
                    });
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err);
                }
            }
        }

    }

    private static string GetResourcePath(string? name)
    {
        var path = Path.GetFullPath(Path.Combine(@"..", "..", "..", "..", "AdaptiveCardViewer", "samples"));
        var fullPath = name.Replace("AdaptiveCardViewer.samples.", path + Path.DirectorySeparatorChar)
            .Replace("v1._", "v1.")
            .Replace(".Elements.", $"{Path.DirectorySeparatorChar}Elements{Path.DirectorySeparatorChar}")
            .Replace(".Scenarios.", $"{Path.DirectorySeparatorChar}Scenarios{Path.DirectorySeparatorChar}")
            .Replace("/", $"{Path.DirectorySeparatorChar}");
        return fullPath;
    }
}
