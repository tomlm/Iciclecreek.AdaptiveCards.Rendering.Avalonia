// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveTableRenderer
    {
        public static Control Render(AdaptiveTable table, AdaptiveRenderContext context)
        {
            var uiTable = new Grid();

            foreach (var column in table.Columns)
            {
                if (column.PixelWidth != 0)
                    uiTable.ColumnDefinitions.Add(new ColumnDefinition(column.PixelWidth, GridUnitType.Pixel));
                else if (column.Width != null)
                    uiTable.ColumnDefinitions.Add(new ColumnDefinition(column.Width, GridUnitType.Star));
                else
                    uiTable.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                if (!table.ShowGridLines)
                {
                    uiTable.ColumnDefinitions.Add(new ColumnDefinition(context.Config.Spacing.Padding / 2, GridUnitType.Pixel));
                }
            }

            foreach (var row in table.Rows)
            {
                uiTable.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                if (!table.ShowGridLines == true)
                {
                    uiTable.RowDefinitions.Add(new RowDefinition(context.Config.Spacing.Padding / 2, GridUnitType.Pixel));
                }
            }


            Control uiRoot = uiTable;
            // uiContainer.Style = context.GetStyle("Adaptive.Container");

            bool? previousContextRtl = context.Rtl;
            bool? currentRtl = previousContextRtl;

            if (currentRtl.HasValue)
            {
                uiTable.FlowDirection = currentRtl.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }

            // Keep track of ContainerStyle.ForegroundColors before Container is rendered
            var parentRenderArgs = context.RenderArgs;

            // This is the renderArgs that will be passed down to the children
            var borderColor = table.GridStyle switch
            {
                AdaptiveContainerStyle.Emphasis => context.RenderArgs.ForegroundColors.Default.Default,
                AdaptiveContainerStyle.Good => context.RenderArgs.ForegroundColors.Good.Default,
                AdaptiveContainerStyle.Attention => context.RenderArgs.ForegroundColors.Attention.Default,
                AdaptiveContainerStyle.Warning => context.RenderArgs.ForegroundColors.Warning.Default,
                AdaptiveContainerStyle.Accent => context.RenderArgs.ForegroundColors.Accent.Default,
                _ => context.RenderArgs.ForegroundColors.Default.Default,
            };

            // Modify context outer parent style so padding necessity can be determined
            var tableRenderArgs = new AdaptiveTableRenderArgs(parentRenderArgs)
            {
                Table = table,
                BorderBursh = context.GetColorBrush(borderColor),
                HasParentWithPadding = true
            };
            context.RenderArgs = tableRenderArgs;

            // uiContainer.MinHeight = table.PixelMinHeight;

            int iRow = 0;
            foreach (var row in table.Rows)
            {
                // Modify context outer parent style so padding necessity can be determined
                var rowRenderArgs = new AdaptiveTableRenderArgs(tableRenderArgs)
                {
                    Table = table,
                    Row = row,
                    ParentStyle = row.Style ?? parentRenderArgs.ParentStyle,
                    BorderBursh = tableRenderArgs.BorderBursh,
                    HasParentWithPadding = true
                };
                context.RenderArgs = rowRenderArgs;

                if (iRow != 0)
                {
                    // Only the first element can bleed to the top
                    context.RenderArgs.BleedDirection &= ~BleedDirection.BleedUp;
                }

                if (iRow != table.Rows.Count - 1)
                {
                    // Only the last element can bleed to the bottom
                    context.RenderArgs.BleedDirection &= ~BleedDirection.BleedDown;
                }

                int iCol = 0;
                foreach (var cell in row.Cells)
                {
                    cell.Style = cell.Style ?? rowRenderArgs.ParentStyle;

                    AdaptiveTypedElement rendereableElement = context.GetRendereableElement(cell);

                    // if there's an element that can be rendered, then render it, otherwise, skip
                    if (rendereableElement != null)
                    {
                        var uiCell = context.Render(rendereableElement);

                        if (uiCell != null)
                        {
                            uiCell.SetValue(Grid.ColumnProperty, iCol);
                            uiCell.SetValue(Grid.RowProperty, iRow);

                            uiTable.Children.Add(uiCell);
                        }
                    }
                    iCol++;
                    if (!table.ShowGridLines)
                        iCol++;
                }
                iRow++;

                if (!table.ShowGridLines)
                    iRow++;
            }

            // Revert context's value to that of outside the Container
            context.RenderArgs = parentRenderArgs;

            if (table.ShowGridLines == true)
            {
                uiRoot = new Border() { BorderBrush = tableRenderArgs.BorderBursh, BorderThickness = new Thickness(1, 1, 0, 0), Child = uiTable };
            }

            return RendererUtil.ApplySelectAction(uiRoot, table, context);
        }


    }

    public class AdaptiveTableRenderArgs : AdaptiveRenderArgs
    {
        public AdaptiveTableRenderArgs(AdaptiveRenderArgs parent) : base(parent)
        {
        }

        public AdaptiveTable Table { get; set; }

        public AdaptiveTableRow Row { get; set; }

        public SolidColorBrush BorderBursh { get; set; }
    }
}
