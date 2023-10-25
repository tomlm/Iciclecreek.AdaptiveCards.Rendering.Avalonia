// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.MarkedNet;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveTextBlockRenderer
    {
        public static Control Render(AdaptiveTextBlock textBlock, AdaptiveRenderContext context)
        {
            if (String.IsNullOrEmpty(textBlock.Text))
            {
                return null;
            }

            var uiTextBlock = CreateControl(textBlock, context);

            uiTextBlock.SetColor(textBlock.Color, textBlock.IsSubtle, context);

            if (textBlock.MaxWidth > 0)
            {
                uiTextBlock.MaxWidth = textBlock.MaxWidth;
            }

            if (textBlock.MaxLines > 0)
            {
                var uiGrid = new Grid();
                uiGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                // create hidden textBlock with appropriate linebreaks that we can use to measure the ActualHeight
                // using same style, fontWeight settings as original textblock
                var measureBlock = new TextBlock()
                {
                    // Style = uiTextBlock.Style,
                    FontWeight = uiTextBlock.FontWeight,
                    FontSize = uiTextBlock.FontSize,
                    IsVisible = true,
                    TextWrapping = TextWrapping.NoWrap,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    DataContext = textBlock.MaxLines
                };

                measureBlock.Inlines.Add(uiTextBlock.Text);

                // bind the real textBlock's Height => MeasureBlock.ActualHeight
                //uiTextBlock.Bind(Control.MaxHeightProperty, measureBlock.Bounds.Height.ToBind new Binding()
                //{
                //    Path = new PropertyPath("ActualHeight"),
                //    Source = measureBlock,
                //    Mode = BindingMode.OneWay,
                //    Converter = new MultiplyConverter(textBlock.MaxLines)
                //});

                // Add both to a grid so they go as a unit
                uiGrid.Children.Add(measureBlock);

                uiGrid.Children.Add(uiTextBlock);
                return uiGrid;

            }

            return uiTextBlock;
        }

        private class ALink
        {
            public int Start { get; set; }

            public string Original { get; set; }
            public string Text { get; set; }
            
            public string Url { get; set; }
        }

        private static TextBlock CreateControl(AdaptiveTextBlock textBlock, AdaptiveRenderContext context)
        {
            Marked marked = new Marked();
            marked.Options.Renderer = new AdaptiveXamlMarkdownRenderer();
            marked.Options.Mangle = false;
            marked.Options.Sanitize = true;

            string text = RendererUtilities.ApplyTextFunctions(textBlock.Text, context.Lang);

            text = marked.Parse(text);
            text = RendererUtilities.HandleHtmlSpaces(text);

            string xaml = $"<TextBlock  xmlns=\"https://github.com/avaloniaui\">{text}</TextBlock>";
            // string xaml = $"<TextBlock >{text}</TextBlock>";
            var uiTextBlock = AvaloniaRuntimeXamlLoader.Parse<TextBlock>(xaml);
            // uiTextBlock.Style = context.GetStyle($"Adaptive.{textBlock.Type}");

            uiTextBlock.TextWrapping = TextWrapping.NoWrap;

            uiTextBlock.FontFamily = new FontFamily(RendererUtil.GetFontFamilyFromList(context.Config.GetFontFamily(textBlock.FontType)));
            uiTextBlock.FontWeight = (FontWeight)context.Config.GetFontWeight(textBlock.FontType, textBlock.Weight);
            uiTextBlock.FontSize = context.Config.GetFontSize(textBlock.FontType, textBlock.Size);

            uiTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;

            if (textBlock.Italic)
            {
                uiTextBlock.FontStyle = FontStyle.Italic;
            }

            if (textBlock.Strikethrough)
            {
                uiTextBlock.TextDecorations = TextDecorations.Strikethrough;
            }

            if (textBlock.HorizontalAlignment != AdaptiveHorizontalAlignment.Left)
            {
                TextAlignment alignment;
                if (Enum.TryParse<TextAlignment>(textBlock.HorizontalAlignment.ToString(), out alignment))
                    uiTextBlock.TextAlignment = alignment;
            }

            if (textBlock.Wrap)
                uiTextBlock.TextWrapping = TextWrapping.Wrap;

            return uiTextBlock;
        }

        private class MultiplyConverter : IValueConverter
        {
            private int multiplier;

            public MultiplyConverter(int multiplier)
            {
                this.multiplier = multiplier;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value * this.multiplier;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value * this.multiplier;
            }
        }
    }
}
