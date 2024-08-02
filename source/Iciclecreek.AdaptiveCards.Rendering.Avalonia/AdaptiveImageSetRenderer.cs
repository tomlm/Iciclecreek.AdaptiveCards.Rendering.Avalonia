// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.




using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveImageSetRenderer
    {
        public static Control Render(AdaptiveImageSet imageSet, AdaptiveRenderContext context)
        {
            var uiImageSet = new ListBox();
            uiImageSet.BorderThickness = new Thickness(0);
            uiImageSet.Background = new SolidColorBrush(Colors.Transparent);
            ScrollViewer.SetHorizontalScrollBarVisibility(uiImageSet, ScrollBarVisibility.Disabled);
            uiImageSet.ItemsPanel = new FuncTemplate<Panel?>(() =>
            {
                var wrapPanel = new WrapPanel();
                wrapPanel.Orientation = Orientation.Horizontal;
                return wrapPanel;
            });
            // uiImageSet.Style = context.GetStyle("Adaptive.ImageSet");
            foreach (var image in imageSet.Images)
            {
                // Use the imageSize in imageSet for all images if present
                if (imageSet.ImageSize != AdaptiveImageSize.Auto)
                    image.Size = imageSet.ImageSize;
                else if (image.Size == AdaptiveImageSize.Auto)
                    image.Size = context.Config.ImageSet.ImageSize;

                var uiImage = context.Render(image);
                uiImageSet.Add(uiImage);
            }

            return uiImageSet;

        }
    }
}
