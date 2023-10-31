using AdaptiveCards;
using AdaptiveCards.Rendering;
using Newtonsoft.Json;

namespace AdaptiveCardViewer.ViewModels
{
    public class CardModel : ViewModelBase
    {
        private string _name;
        private string _uri;
        private AdaptiveCard _card;
        private AdaptiveHostConfig _hostConfig;

        public string Uri { get => _uri; set => SetProperty(ref _uri, value); }

        public string Name { get => _name; set => SetProperty(ref _name, value); }

        public AdaptiveCard Card { get => _card; set => SetProperty(ref _card, value); }

        public AdaptiveHostConfig HostConfig { get => _hostConfig; set => SetProperty(ref _hostConfig, value); }

        public string HtmlCard
        {
            get
            {
                var html = $@"
<!DOCTYPE html>
<html lang='en'>

<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    
    <script src='https://unpkg.com/adaptivecards@latest/dist/adaptivecards.min.js'></script>
    <script src='https://unpkg.com/markdown-it/dist/markdown-it.js'></script>
    <script type='text/javascript' src='/js/hostconfig.js'></script>
    <style>
        .cardDiv {{ width: 500px;
            padding: 10px;
            box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
            text-align: center;
        }}
    </style>
    <link rel='stylesheet' type='text/css' href='https://adaptivecards.io/node_modules/adaptivecards-designer/dist/containers/teams-container-light.css'>
    <!-- /CRAZOR -->

</head>

<body>
    
    <div class='cardDiv' id='card'></div>
<script>
var card = {JsonConvert.SerializeObject(Card)};

// Create an AdaptiveCard instance
var adaptiveCard = new AdaptiveCards.AdaptiveCard();
adaptiveCard.hostConfig = new AdaptiveCards.HostConfig({JsonConvert.SerializeObject(HostConfig)});
adaptiveCard.onExecuteAction = function(action) {{alert(JSON.stringify(action.data)); }}

AdaptiveCards.AdaptiveCard.onProcessMarkdown = function (text, result) {{
   result.outputHtml = markdownit().render(text);
   result.didProcess = true;
}};

// Parse the card payload
adaptiveCard.parse(card);

// Render the card to an HTML element:
var renderedCard = adaptiveCard.render();

// And finally insert it somewhere in your page:
document.getElementById('card').appendChild(renderedCard);
    </script>
</body>

</html>
";
                return html;
            }
}
    }
}
