using AdaptiveCards;
using AdaptiveCards.Rendering;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace AdaptiveCardViewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public MainViewModel()
    {
        LoadHostConfig("microsoft-teams-light");
        this.Cards.Add(LoadCardResource("AdaptiveCardViewer.samples.Welcome.json"));
    
        // if (Debugger.IsAttached)
        // {
        //     string fullPath = @"C:\source\github\AdaptiveCards.Rendering.Avalonia\source\AdaptiveCardViewer\samples\v1.6\Elements\Media.json";
        //     var json = File.ReadAllText(fullPath);  
        //     AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(json);
        //     this.Cards.Add(new CardModel()
        //     {
        //         Name = Path.GetFileName(fullPath),
        //         Uri = fullPath,
        //         Card = parseResult.Card,
        //         HostConfig = this.HostConfig
        //     });
        // }
    }

    public string Greeting => "Welcome to Avalonia!";

    private ObservableCollection<CardModel> _cards = new ObservableCollection<CardModel>();
    public ObservableCollection<CardModel> Cards { get => _cards ; set => SetProperty(ref _cards, value); } 

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

    public void LoadScenarios()
    {
        this.Cards = new ObservableCollection<CardModel>(Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(p => p.ToLower().Contains("scenarios"))
            .Select(p => LoadCardResource(p)));
    }

    public void LoadVersionSamples(string ver)
    {
        ver = ver.Replace(".", "._");
        this.Cards = new ObservableCollection<CardModel>(Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(p => p.Contains(ver) && !p.ToLower().Contains("tests"))
            .Select(p => LoadCardResource(p)));
    }

    private CardModel LoadCardResource(string? name)
    {
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
                return new CardModel()
                {
                    Name = Path.GetFileName(fullPath),
                    Uri = fullPath,
                    Card = parseResult.Card,
                    HostConfig = this.HostConfig
                };
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
            }
        }
        return null;
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
