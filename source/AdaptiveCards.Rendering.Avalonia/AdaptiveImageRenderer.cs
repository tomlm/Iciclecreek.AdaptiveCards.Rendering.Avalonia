// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveImageRenderer
    {
        public static Control Render(AdaptiveImage image, AdaptiveRenderContext context)
        {
            Control uiBorder = null;
            var uiImage = new Image();

            // Try to resolve the image URI
            Uri finalUri = context.Config.ResolveFinalAbsoluteUri(image.Url);
            if (finalUri == null)
            {
                return uiImage;
            }

            uiImage.SetSource(image, finalUri, context);

            uiImage.SetHorizontalAlignment(image.HorizontalAlignment);

            string style = $"Adaptive.{image.Type}";
            if (image.Style == AdaptiveImageStyle.Person)
            {
                style += $".{image.Style}";

                var mask = new RadialGradientBrush()
                {
                    GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                    Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                    Radius = 0.5,
                    //RadiusX = 0.5,
                    //RadiusY = 0.5,
                    GradientStops = new GradientStops()
                };
                mask.GradientStops.Add(new GradientStop(Color.Parse("#ffffffff"), 1.0));
                mask.GradientStops.Add(new GradientStop(Color.Parse("#00ffffff"), 1.0));
                uiImage.OpacityMask = mask;
            }
            // uiImage.Style = context.GetStyle(style);

            if (image.PixelHeight == 0 && image.PixelWidth == 0)
            {
                uiImage.SetImageProperties(image, context);
            }

            if (image.PixelHeight > 0)
            {
                uiImage.Height = image.PixelHeight;
            }
            if (image.PixelWidth > 0)
            {
                uiImage.Width = image.PixelWidth;
            }


            // If we have a background color, we'll create a border for the background and put the image on top
            if (!string.IsNullOrEmpty(image.BackgroundColor))
            {
                Color color = Color.Parse(image.BackgroundColor);
                if (color.A != 0)
                {
                    uiBorder = new Border()
                    {
                        Background = new SolidColorBrush(color),
                        Child = uiImage,
                        Width = uiImage.Width,
                        Height = uiImage.Height,
                        HorizontalAlignment = uiImage.HorizontalAlignment,
                        VerticalAlignment = uiImage.VerticalAlignment,
                        OpacityMask = uiImage.OpacityMask
                    };
                }
            }

            return RendererUtil.ApplySelectAction(uiBorder ?? uiImage, image, context);
        }

    }
}
