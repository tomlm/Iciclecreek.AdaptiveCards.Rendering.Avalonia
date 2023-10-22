using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class AdaptiveCardView : UserControl
    {
        public static readonly DirectProperty<AdaptiveCardView, AdaptiveCard> CardProperty = AvaloniaProperty.RegisterDirect<AdaptiveCardView, AdaptiveCard>(nameof(Card), o => o.Card, (o, v) => o.Card = v);

        public AdaptiveCardView()
        {
            this.Content = new TextBlock() { Text = "inner" };
        }

        private AdaptiveCard _card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));
        public AdaptiveCard Card
        {
            get => _card;
            set
            {
                try
                {
                    value = value ?? new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
                    {
                        Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock() { Text = "Hello World" }
                        }
                    };
                    SetAndRaise(CardProperty, ref _card, value);
                    var renderer = new AdaptiveCardRenderer(/*config*/);
                    var renderedCard = renderer.RenderCard(_card);
                    this.Content = renderedCard.Control;
                }
                catch (Exception err)
                {
                    SetAndRaise(CardProperty, ref _card, new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
                    {
                        Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock() { Text = err.Message.Replace("https://","https//"), Color = AdaptiveTextColor.Attention, }
                       }
                    });
                    var renderer = new AdaptiveCardRenderer(/*config*/);
                    var renderedCard = renderer.RenderCard(_card);
                    this.Content = renderedCard.Control;
                }
            }
        }
    }
}
