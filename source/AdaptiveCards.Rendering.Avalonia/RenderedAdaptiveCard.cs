// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;
using System.Collections.Generic;


namespace AdaptiveCards.Rendering.Avalonia
{
    public class RenderedAdaptiveCard : RenderedAdaptiveCardBase
    {
        public RenderedAdaptiveCard(
            Control frameworkElement,
            AdaptiveCard originatingCard,
            IList<AdaptiveWarning> warnings,
            ref IDictionary<string, Func<string>> inputBindings)
            : base(originatingCard, warnings)
        {
            Control = frameworkElement;
            UserInputs = new RenderedAdaptiveCardInputs(ref inputBindings);
        }

        /// <summary>
        /// The rendered card
        /// </summary>
        public Control Control { get; }

        /// <summary>
        /// Event handler for when user invokes an action.
        /// </summary>
        public event TypedEventHandler<RenderedAdaptiveCard, AdaptiveActionEventArgs> OnAction;

        internal void InvokeOnAction(AdaptiveActionEventArgs args)
        {
            OnAction?.Invoke(this, args);
        }

        /// <summary>
        /// Event handler for when user clicks a media.
        /// </summary>
        public event TypedEventHandler<RenderedAdaptiveCard, AdaptiveMediaEventArgs> OnMediaClicked;

        internal void InvokeOnMediaClick(AdaptiveMediaEventArgs args)
        {
            OnMediaClicked?.Invoke(this, args);
        }
    }
}
