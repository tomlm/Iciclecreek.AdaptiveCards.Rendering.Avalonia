// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;




namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveTableCellRenderer
    {
        public static Control Render(AdaptiveTableCell cell, AdaptiveRenderContext context)
        {
            var args = (AdaptiveTableRenderArgs)context.RenderArgs;

            var uiCell = new Grid();
            // uiContainer.Style = context.GetStyle("Adaptive.tableCell");

            bool? previousContextRtl = context.Rtl;
            bool? currentRtl = previousContextRtl;

            bool updatedRtl = false;
            if (cell.Rtl.HasValue)
            {
                currentRtl = cell.Rtl;
                context.Rtl = currentRtl;
                updatedRtl = true;
            }

            if (currentRtl.HasValue)
            {
                uiCell.FlowDirection = currentRtl.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }

            // Keep track of ContainerStyle.ForegroundColors before Container is rendered
            var parentRenderArgs = context.RenderArgs;
            // This is the renderArgs that will be passed down to the children
            var childRenderArgs = new AdaptiveRenderArgs(parentRenderArgs);

            Border border = new Border();
            if (args.Table.ShowGridLines == true)
            {
                border.BorderBrush = args.BorderBursh;
                border.BorderThickness = new Thickness(0, 0, 1, 1);
            }

            border.Child = uiCell;

            bool inheritsStyleFromParent = !cell.Style.HasValue;
            bool tableCellHasPadding = false;

            //            if (!inheritsStyleFromParent)
            {
                uiCell.Margin = new Thickness(context.Config.Spacing.Padding / 2);
                //tableCellHasPadding = AdaptiveContainerRenderer.ApplyPadding(border, uiCell, cell, parentRenderArgs, context);

                // Apply background color
                ContainerStyleConfig containerStyle = context.Config.ContainerStyles.GetContainerStyleConfig(cell.Style);
                border.Background = context.GetColorBrush(containerStyle.BackgroundColor);

                childRenderArgs.ForegroundColors = containerStyle.ForegroundColors;
            }

            childRenderArgs.ParentStyle = (inheritsStyleFromParent) ? parentRenderArgs.ParentStyle : cell.Style.Value;

            // If the tableCell has no padding or has padding and doesn't bleed, then the children can bleed
            // to the side the tableCell would have bled
            if (tableCellHasPadding)
            {
                childRenderArgs.BleedDirection = BleedDirection.BleedAll;
            }

            // If either this tableCell or an ancestor had padding, then the children will have an ancestor with padding
            childRenderArgs.HasParentWithPadding = (tableCellHasPadding || parentRenderArgs.HasParentWithPadding);
            context.RenderArgs = childRenderArgs;
            AdaptiveContainerRenderer.AddContainerElements(uiCell, cell.Items, context);

            // RendererUtil.ApplyVerticalContentAlignment(border, cell.VerticalContentAlignment);

            uiCell.MinHeight = cell.PixelMinHeight;

            // Revert context's value to that of outside the tableCell
            context.RenderArgs = parentRenderArgs;

            if (updatedRtl)
            {
                context.Rtl = previousContextRtl;
            }

            if (cell.BackgroundImage != null)
            {
                uiCell.SetBackgroundSource(cell.BackgroundImage, context);
                if (cell.Items.Count == 0)
                {
                    // if we have background source and no children we need to have at least padding to show background image 
                    uiCell.Children.Add(new Grid() { Margin = new Thickness(context.Config.Spacing.Padding) });
                }
            }
            var cellProps = (IDictionary<string, object>)cell.AdditionalProperties;
            var cellHorizontalAlignment = cellProps.TryGetValue<AdaptiveHorizontalAlignment>("horizontalAlignment");

            if (cell.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                RendererUtil.ApplyVerticalContentAlignment(uiCell, cell.VerticalContentAlignment);
            else if (args.Row.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                RendererUtil.ApplyVerticalContentAlignment(uiCell, args.Row.VerticalContentAlignment);
            else if (args.Table.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                RendererUtil.ApplyVerticalContentAlignment(uiCell, args.Table.VerticalContentAlignment);

            if (cellHorizontalAlignment != default(AdaptiveHorizontalAlignment))
            {
                uiCell.HorizontalAlignment = cellHorizontalAlignment switch
                {
                    AdaptiveHorizontalAlignment.Center => HorizontalAlignment.Center,
                    AdaptiveHorizontalAlignment.Left => HorizontalAlignment.Left,
                    AdaptiveHorizontalAlignment.Right => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Stretch,
                };
            }
            else if (args.Row.HorizontalContentAlignment != default(AdaptiveHorizontalContentAlignment))
                RendererUtil.ApplyHorizontalContentAlignment(uiCell, args.Row.HorizontalContentAlignment);
            else if (args.Table.HorizontalContentAlignment != default(AdaptiveHorizontalContentAlignment))
                RendererUtil.ApplyHorizontalContentAlignment(uiCell, args.Table.HorizontalContentAlignment);

            return RendererUtil.ApplySelectAction(border, cell, context);
        }
    }
}
