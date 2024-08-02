using AdaptiveCards;
using AdaptiveCards.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AdaptiveCardViewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    public MainViewModel()
    {
        LoadHostConfig("microsoft-teams-light");

        this.Cards.Add(LoadCardResource("AdaptiveCardViewer.samples.Welcome.json"));
        this.Cards.Add(LoadCardResource("AdaptiveCardViewer.samples.Flow.json"));
        this.Cards = new ObservableCollection<CardModel>(Assembly.GetExecutingAssembly().GetManifestResourceNames()
            .Where(path => !path.ToLower().Contains("tests"))
            .Select(path => LoadCardResource(path))
            .Where(card => card != null));
        var welcome = Cards.Single(p => p.Name.EndsWith("Welcome.json"));
        this.Cards.Remove(welcome);
        this.Cards.Insert(0, welcome);
        this.SelectedCard = welcome;
        this.PropertyChanged += MainViewModel_PropertyChanged; ;
        this.FilteredCards = new ObservableCollection<CardModel>(this.Cards);
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

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            this.FilteredCards = new ObservableCollection<CardModel>(this.Cards.Where(p => p.Name.ToLower().Contains(this.SearchText.ToLower())));
        }
    }

    public string Greeting => "Welcome to Avalonia!";

    private List<CardModel> _allCards;

    private ObservableCollection<CardModel> _cards = new ObservableCollection<CardModel>();
    public ObservableCollection<CardModel> Cards { get => _cards; set => SetProperty(ref _cards, value); }

    private ObservableCollection<CardModel> _filteredCards = new ObservableCollection<CardModel>();
    public ObservableCollection<CardModel> FilteredCards { get => _filteredCards; set => SetProperty(ref _filteredCards, value); }

    private string _search = "";
    public String SearchText { get => _search; set => base.SetProperty(ref _search, value); }

    private CardModel _selectedCard;
    public CardModel SelectedCard { get => _selectedCard; set => base.SetProperty(ref _selectedCard, value); }

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
                    Name = fullPath.Substring(fullPath.IndexOf("samples") + "samples".Length),
                    Uri = fullPath,
                    Card = parseResult.Card,
                    HostConfig = this.HostConfig
                };
            }
            catch (Exception err)
            {
                Debug.WriteLine($"{name} : {err.Message}");
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
