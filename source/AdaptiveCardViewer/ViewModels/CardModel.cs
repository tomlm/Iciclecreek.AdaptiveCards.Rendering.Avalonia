using AdaptiveCards;
using AdaptiveCards.Rendering;

namespace AdaptiveCardViewer.ViewModels
{
    public class CardModel : ViewModelBase
    {
        private string _name;
        private string _uri;
        private AdaptiveCard _card;
        private AdaptiveHostConfig _hostConfig;

        public string Uri { get => _uri; set => SetProperty(ref _uri, value); }

        public string Name { get=>_name; set=> SetProperty(ref _name, value); }

        public AdaptiveCard Card { get => _card; set => SetProperty(ref _card, value); }

        public AdaptiveHostConfig HostConfig { get => _hostConfig; set => SetProperty(ref _hostConfig, value); }
    }
}
