// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;


namespace AdaptiveCards.Rendering.Avalonia
{
    public class MissingInputException : Exception
    {
        public MissingInputException(string message, AdaptiveInput input, Control frameworkElement)
            : base(message)
        {
            this.Control = frameworkElement;
            this.AdaptiveInput = input;
        }

        public Control Control { get; set; }

        public AdaptiveInput AdaptiveInput { get; set; }
    }
}
