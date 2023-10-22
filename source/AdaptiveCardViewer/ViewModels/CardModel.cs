using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveCardViewer.ViewModels
{
    public class CardModel
    {
        public string Name { get; set; }
        public AdaptiveCard Card { get; set; }
    }
}
