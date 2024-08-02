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
        }

        public AdaptiveCardView CardView { get => Content as AdaptiveCardView; set => Content = value; }
    }
}
