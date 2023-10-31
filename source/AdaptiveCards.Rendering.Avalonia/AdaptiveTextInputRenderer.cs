// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.



using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Text.RegularExpressions;

namespace AdaptiveCards.Rendering.Avalonia
{

    public static class AdaptiveTextInputRenderer
    {
        public static Control Render(AdaptiveTextInput input, AdaptiveRenderContext context)
        {
            TextBox textBox = null;

            switch (input.Style)
            {
                case AdaptiveTextInputStyle.Tel:
                    textBox = new MaskedTextBox() { Mask = "(000) 000-0000" };
                    break;
                case AdaptiveTextInputStyle.Password:
                    textBox = new TextBox() { PasswordChar = '•' };
                    break;
                case AdaptiveTextInputStyle.Email:
                case AdaptiveTextInputStyle.Url:
                case AdaptiveTextInputStyle.Text:
                default:
                    textBox = new TextBox();
                    break;
            }

            textBox.Text = input.Value;

            if (input.IsMultiline == true)
            {
                textBox.AcceptsReturn = true;

                textBox.TextWrapping = TextWrapping.Wrap;
                textBox.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled); //textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }

            if (input.MaxLength > 0)
            {
                textBox.MaxLength = input.MaxLength;
            }

            textBox.SetPlaceholder(input.Placeholder);
            // textBox.Style = context.GetStyle($"Adaptive.Input.Text.{input.Style}");
            textBox.SetContext(input);

            if ((input.IsRequired || input.Regex != null) && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }

            context.InputValues.Add(input.Id, new AdaptiveTextInputValue(input, textBox));

            if (input.InlineAction != null)
            {
                if (context.Config.Actions.ShowCard.ActionMode == ShowCardActionMode.Inline &&
                    input.InlineAction.GetType() == typeof(AdaptiveShowCardAction))
                {
                    context.Warnings.Add(new AdaptiveWarning(-1, "Inline ShowCard not supported for InlineAction"));
                }
                else
                {
                    if (context.Config.SupportsInteractivity && context.ActionHandlers.IsSupported(input.InlineAction.GetType()))
                    {
                        return RenderInlineAction(input, context, textBox);
                    }
                }
            }

            return textBox;
        }

        public static Control RenderInlineAction(AdaptiveTextInput input, AdaptiveRenderContext context, Control textBox)
        {
            // Set up a parent view that holds textbox, separator and button
            var parentView = new Grid();

            // grid config for textbox
            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(textBox, 0);
            parentView.Children.Add(textBox);

            // grid config for spacing
            int spacing = context.Config.GetSpacing(AdaptiveSpacing.Default);
            var uiSep = new Grid
            {
                // Style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Separator"),
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = spacing,
            };
            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(spacing, GridUnitType.Pixel) });
            Grid.SetColumn(uiSep, 1);

            // adding button
            var uiButton = new Button();
            //Style style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Button");
            //if (style != null)
            //{
            //    uiButton.Style = style;
            //}

            // this textblock becomes tooltip if icon url exists else becomes the tile for the button
            var uiTitle = new TextBlock
            {
                Text = input.InlineAction.Title,
            };

            if (input.InlineAction.IconUrl != null)
            {
                var actionsConfig = context.Config.Actions;

                var image = new AdaptiveImage(input.InlineAction.IconUrl)
                {
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                    Type = "Adaptive.Input.Text.InlineAction.Image",
                };

                Control uiIcon = null;
                uiIcon = AdaptiveImageRenderer.Render(image, context);
                uiButton.Content = uiIcon;

                // adjust height
                textBox.Loaded += (sender, e) =>
                {
                    uiIcon.Height = textBox.Bounds.Height;
                };

                uiButton.SetValue(ToolTip.TipProperty, input.InlineAction.Tooltip ?? input.InlineAction.Title);
            }
            else
            {
                uiTitle.FontSize = context.Config.GetFontSize(AdaptiveFontType.Default, AdaptiveTextSize.Default);
                // uiTitle.Style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Title");
                uiButton.Content = uiTitle;
            }

            if (input.InlineAction is AdaptiveSubmitAction ||
                input.InlineAction is AdaptiveExecuteAction)
            {
                context.SubmitActionCardId[input.InlineAction as AdaptiveSubmitAction] = context.RenderArgs.ContainerCardId;
            }

            uiButton.Click += (sender, e) =>
            {
                context.InvokeAction(uiButton, new AdaptiveActionEventArgs(input.InlineAction));

                // Prevent nested events from triggering
                e.Handled = true;
            };

            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            Grid.SetColumn(uiButton, 2);
            parentView.Children.Add(uiButton);
            uiButton.VerticalAlignment = VerticalAlignment.Bottom;

            textBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    context.InvokeAction(uiButton, new AdaptiveActionEventArgs(input.InlineAction));
                    e.Handled = true;
                }
            };
            return parentView;
        }
    }

    public class AdaptiveTextInputValue : AdaptiveTextBoxInputValue
    {
        public AdaptiveTextInputValue(AdaptiveTextInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override bool Validate()
        {
            bool isValid = base.Validate();

            AdaptiveTextInput textInput = InputElement as AdaptiveTextInput;

            if (!String.IsNullOrEmpty(textInput.Regex) && !String.IsNullOrEmpty(GetValue()))
            {
                isValid = isValid && Regex.IsMatch(GetValue(), textInput.Regex);
            }

            if (textInput.MaxLength != 0)
            {
                isValid = isValid && (GetValue().Length <= textInput.MaxLength);
            }

            return isValid;
        }
    }
}
