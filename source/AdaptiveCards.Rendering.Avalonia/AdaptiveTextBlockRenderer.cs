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
                uiTextBlock.MaxLines = textBlock.MaxLines;
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
