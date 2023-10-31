using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;

namespace AdaptiveCards.Rendering.Avalonia
{

    public class AdaptiveCardView : UserControl
    {
        public static readonly DirectProperty<AdaptiveCardView, AdaptiveCard> CardProperty = AvaloniaProperty.RegisterDirect<AdaptiveCardView, AdaptiveCard>(nameof(Card), o => o.Card, (o, v) => o.Card = v);
        public static readonly DirectProperty<AdaptiveCardView, AdaptiveHostConfig> HostConfigProperty = AvaloniaProperty.RegisterDirect<AdaptiveCardView, AdaptiveHostConfig>(nameof(HostConfig), o => o.HostConfig, (o, v) => o.HostConfig = v);
        public static readonly RoutedEvent<RoutedAdaptiveActionEventArgs> ActionEvent = RoutedEvent.Register<AdaptiveCardView, RoutedAdaptiveActionEventArgs>(nameof(Action), RoutingStrategies.Bubble);

        public AdaptiveCardView()
        {
            this.Content = new TextBlock() { Text = "inner" };
        }

        private AdaptiveHostConfig _hostConfig;
        /// <summary>
        /// HostConfig to use to for rendendering
        /// </summary>
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
        /// <summary>
        /// AdaptiveCard to render
        /// </summary>
        public AdaptiveCard Card
        {
            get => _card;
            set
            {
                SetAndRaise(CardProperty, ref _card, value);
                RenderCard();
            }
        }

        /// <summary>
        /// Action handler, subscribe to handle action events
        /// </summary>
        public event EventHandler<RoutedAdaptiveActionEventArgs> Action
        {
            add => AddHandler(ActionEvent, value);
            remove => RemoveHandler(ActionEvent, value);
        }

        private void RenderCard()
        {
            /*
                var e = new RoutedEventArgs(ClickEvent);
                RaiseEvent(e);
            */
            try
            {
                if (_card != null)
                {

                    var renderer = new AdaptiveCardRenderer(_hostConfig);
                    var renderedCard = renderer.RenderCard(_card);
                    renderedCard.OnAction += (sender, e) =>
                    {
                        RaiseEvent(new RoutedAdaptiveActionEventArgs(renderedCard, e.Action));
                    };

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
