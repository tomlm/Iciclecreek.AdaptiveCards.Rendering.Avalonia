using Avalonia.Controls;
using System;

namespace AdaptiveCards.Rendering.Avalonia
{
    /// <summary>
    /// Class for showing a card in a window
    /// </summary>
    public class AdaptiveCardWindow : Window
    {

        public AdaptiveCardWindow()
        {
            var cardView = new AdaptiveCardView();
            Content = cardView;
        }

        /// <summary>
        /// AdaptiveCard to render
        /// </summary>
        public AdaptiveCard Card { get => ((AdaptiveCardView)Content).Card; set => ((AdaptiveCardView)Content).Card = value; }

        /// <summary>
        /// HostConfig to use to for rendering
        /// </summary>
        public AdaptiveHostConfig HostConfig { get => ((AdaptiveCardView)Content).HostConfig; set => ((AdaptiveCardView)Content).HostConfig = value; }

        /// <summary>
        /// Action handler, subscribe to handle action events
        /// </summary>
        public event EventHandler<RoutedAdaptiveActionEventArgs> Action
        {
            add => AddHandler(AdaptiveCardView.ActionEvent, value);
            remove => RemoveHandler(AdaptiveCardView.ActionEvent, value);
        }

    }
}
