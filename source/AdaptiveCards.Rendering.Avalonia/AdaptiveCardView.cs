using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class AdaptiveCardView : UserControl
    {
        public static readonly DirectProperty<AdaptiveCardView, AdaptiveCard> CardProperty = AvaloniaProperty.RegisterDirect<AdaptiveCardView, AdaptiveCard>(nameof(Card), o => o.Card, (o, v) => o.Card = v);
        public static readonly DirectProperty<AdaptiveCardView, AdaptiveHostConfig> HostConfigProperty = AvaloniaProperty.RegisterDirect<AdaptiveCardView, AdaptiveHostConfig>(nameof(HostConfig), o => o.HostConfig, (o, v) => o.HostConfig = v);

        public AdaptiveCardView()
        {
            this.Content = new TextBlock() { Text = "inner" };
        }

        private AdaptiveHostConfig _hostConfig;
        public AdaptiveHostConfig HostConfig
        {
            get => _hostConfig;
            set
            {
                SetAndRaise(HostConfigProperty, ref _hostConfig, value);
                RenderCard();
            }
        }

        private AdaptiveCard _card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 5));
        public AdaptiveCard Card
        {
            get => _card;
            set
            {
                SetAndRaise(CardProperty, ref _card, value);
                RenderCard();
            }
        }

        private void RenderCard()
        {
            try
            {
                if (_card != null)
                {

                    var renderer = new AdaptiveCardRenderer(_hostConfig);
                    var renderedCard = renderer.RenderCard(_card);
                    this.Content = renderedCard.Control;
                }
                else
                    this.Content = null;
            }
            catch (Exception err)
            {
                SetAndRaise(CardProperty, ref _card, new AdaptiveCard(new AdaptiveSchemaVersion(1, 5))
                {
                    Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock() { Text = $"```\n{err.Message}\n```", Color = AdaptiveTextColor.Attention, }
                       }
                });
                var renderer = new AdaptiveCardRenderer(/*config*/);
                var renderedCard = renderer.RenderCard(_card);
                this.Content = renderedCard.Control;
            }
        }
    }
}
