// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class AdaptiveMediaEventArgs : EventArgs
    {
        public AdaptiveMediaEventArgs(AdaptiveMedia media)
        {
            Media = media;
        }

        /// <summary>
        /// The clicked media
        /// </summary>
        public AdaptiveMedia Media { get; set; }
    }
}
