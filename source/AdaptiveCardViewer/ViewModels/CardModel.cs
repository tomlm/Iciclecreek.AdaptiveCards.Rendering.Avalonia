using AdaptiveCards;
using AdaptiveCards.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveCardViewer.ViewModels
{
    public class CardModel : ViewModelBase
    {
        private string _name;
        private AdaptiveCard _card;
        private AdaptiveHostConfig _hostConfig;

        public string Name { get=>_name; set=> SetProperty(ref _name, value); }

        public AdaptiveCard Card { get => _card; set => SetProperty(ref _card, value); }

        public AdaptiveHostConfig HostConfig { get => _hostConfig; set => SetProperty(ref _hostConfig, value); }
    }
}
