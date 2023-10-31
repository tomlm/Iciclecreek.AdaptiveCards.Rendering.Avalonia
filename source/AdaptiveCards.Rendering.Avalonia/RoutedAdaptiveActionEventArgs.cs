using Avalonia.Interactivity;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class RoutedAdaptiveActionEventArgs : RoutedEventArgs
    {
        public RoutedAdaptiveActionEventArgs(RenderedAdaptiveCard source, AdaptiveAction action)
            : base(AdaptiveCardView.ActionEvent, source)
        {
            Action = action;
            UserInputs = source.UserInputs;
            Card = source.OriginatingCard;
        }

        /// <summary>
        /// The action that fired
        /// </summary>
        public AdaptiveAction Action { get; }

        public RenderedAdaptiveCardInputs UserInputs { get; }

        public AdaptiveCard Card { get; }
    }
}
