// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;


namespace AdaptiveCards.Rendering.Avalonia
{
    public class MissingInputEventArgs : EventArgs
    {
        public MissingInputEventArgs(AdaptiveInput input, Control frameworkElement)
        {
            this.Control = frameworkElement;
            this.AdaptiveInput = input;
        }

        public Control Control { get; private set; }

        public AdaptiveInput AdaptiveInput { get; private set; }
    }
}
