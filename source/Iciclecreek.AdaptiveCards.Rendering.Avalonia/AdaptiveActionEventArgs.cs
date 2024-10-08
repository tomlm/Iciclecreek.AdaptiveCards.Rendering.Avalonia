// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AdaptiveCards;
using System;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class AdaptiveActionEventArgs : EventArgs
    {
        public AdaptiveActionEventArgs(AdaptiveAction action)
        {
            Action = action;
        }

        /// <summary>
        /// The action that fired
        /// </summary>
        public AdaptiveAction Action { get; set; }
    }
}
