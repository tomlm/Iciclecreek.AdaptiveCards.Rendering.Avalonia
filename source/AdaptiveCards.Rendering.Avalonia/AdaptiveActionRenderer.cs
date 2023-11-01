// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Text;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveActionRenderer
    {
        public static Control Render(AdaptiveAction action, AdaptiveRenderContext context)
        {
            if (context.Config.SupportsInteractivity && context.ActionHandlers.IsSupported(action.GetType()))
            {
                var uiButton = CreateActionButton(action, context);

                uiButton.Click += (sender, e) =>
                {
                    context.InvokeAction(uiButton, new AdaptiveActionEventArgs(action));

                    // Prevent nested events from triggering
                    e.Handled = true;
                };

                return uiButton;
            }
            return null;
        }

        public static Button CreateActionButton(AdaptiveAction action, AdaptiveRenderContext context)
        {
            int iRow = 0;
            int iCol = 0;
            var sb = new StringBuilder(action.Style ?? "Default".ToLower());
            sb[0] = Char.ToUpper(sb[0]);
            var actionStyle = sb.ToString();

            ContainerStyleConfig? containerConfig = context.Config.ContainerStyles.Default;
            var uiButton = new Button()
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                IsEnabled = action.IsEnabled,
                Padding = new Thickness(1),
                BorderThickness = new Thickness(1)
            };

            switch (actionStyle)
            {
                case "Positive":
                    containerConfig = context.Config.ContainerStyles.Accent;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(context.Config.ContainerStyles.Default.ForegroundColors.Accent.Default);
                    uiButton.BorderBrush = context.GetColorBrush(context.Config.ContainerStyles.Default.ForegroundColors.Accent.Default);
                    break;
                case "Destructive":
                    containerConfig = context.Config.ContainerStyles.Default;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(containerConfig.ForegroundColors.Attention.Default);
                    uiButton.BorderBrush = context.GetColorBrush(containerConfig.ForegroundColors.Attention.Default);
                    break;
                default:
                    containerConfig = context.Config.ContainerStyles.Default;
                    uiButton.Background = context.GetColorBrush(containerConfig.BackgroundColor);
                    uiButton.Foreground = context.GetColorBrush(containerConfig.ForegroundColors.Accent.Default);
                    uiButton.BorderBrush = context.GetColorBrush(containerConfig.ForegroundColors.Accent.Default);
                    break;
            }

            // Style = context.GetStyle($"Adaptive.{action.Type}"),
            uiButton.Classes.Add(actionStyle);

            var contentStackPanel = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto"),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            };

            if (!context.IsRenderingSelectAction && !context.IsRenderingOverflowAction)
            {
                // Only apply padding for normal card actions
                uiButton.Padding = new Thickness(6, 4, 6, 4);
            }
            else
            {
                // Remove any extra spacing for selectAction
                uiButton.Padding = new Thickness(0, 0, 0, 0);
                contentStackPanel.Margin = new Thickness(0, 0, 0, 0);
            }
            uiButton.Content = contentStackPanel;
            Image uiIcon = null;

            var uiTitle = new TextBlock
            {
                Text = action.Title,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = context.Config.GetFontSize(AdaptiveFontType.Default, AdaptiveTextSize.Default),
                // Style = context.GetStyle($"Adaptive.Action.Title")
            };

            var actionsConfig = context.Config.Actions;
            if (action.IconUrl != null)
            {
                uiIcon = new Image()
                {
                    Stretch = Stretch.Uniform,
                };

                if (actionsConfig.IconPlacement == IconPlacement.AboveTitle)
                {
                    uiIcon.Height = (double)actionsConfig.IconSize;
                }
                else
                {
                    //Size the image to the textblock, wait until layout is complete (loaded event)
                    uiIcon.Loaded += (sender, e) =>
                    {
                        uiIcon.Height = uiTitle.Bounds.Height;
                        uiIcon.Width = uiTitle.Bounds.Height;
                    };
                }

                // Try to resolve the image URI
                Uri finalUri = context.Config.ResolveFinalAbsoluteUri(action.IconUrl);
                uiIcon.SetSource(finalUri, context);

                // Add spacing for the icon for horizontal actions
                if (actionsConfig.IconPlacement == IconPlacement.LeftOfTitle)
                {
                    Grid.SetColumn(uiIcon, iCol++);
                    contentStackPanel.Children.Add(uiIcon);

                    int spacing = context.Config.GetSpacing(AdaptiveSpacing.Default);
                    var uiSep = new Grid
                    {
                        // Style = context.GetStyle($"Adaptive.VerticalSeparator"),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = spacing,
                    };
                    Grid.SetColumn(uiSep, iCol++);
                    contentStackPanel.Children.Add(uiSep);
                }
                else
                {
                    Grid.SetRow(uiIcon, iRow++);
                    contentStackPanel.Children.Add(uiIcon);
                }
            }

            if (!context.IsRenderingSelectAction)
            {
                if (actionsConfig.IconPlacement == IconPlacement.LeftOfTitle)
                    Grid.SetColumn(uiTitle, iCol++);
                else
                    Grid.SetRow(uiTitle, iRow++);
                contentStackPanel.Children.Add(uiTitle);
            }
            else
            {
                uiButton.SetValue(ToolTip.TipProperty, action.Tooltip ?? action.Title);
            }

            string name = context.GetType().Name.Replace("Action", String.Empty);

            // action alignment.
            switch(actionsConfig.ActionAlignment)
            {
                case AdaptiveHorizontalAlignment.Stretch:
                    uiButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                    break;
                case AdaptiveHorizontalAlignment.Left:
                    uiButton.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
                case AdaptiveHorizontalAlignment.Center:
                    uiButton.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case AdaptiveHorizontalAlignment.Right:
                    uiButton.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
            }

            uiButton.Classes.Add(typeof(AdaptiveAction).Name);
            return uiButton;
        }

    }

    public class Observer<T> : IObserver<T>
    {
        Action<T> _action;
        public Observer(Action<T> action)
        {
            _action = action;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(T value)
        {
            this._action(value);
        }

    }
}

