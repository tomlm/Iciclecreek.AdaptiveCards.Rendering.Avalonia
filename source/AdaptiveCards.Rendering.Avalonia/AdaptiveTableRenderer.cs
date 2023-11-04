// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
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
            }

            foreach (var row in table.Rows)
            {
                uiTable.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
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
            var childRenderArgs = new AdaptiveRenderArgs(parentRenderArgs);

            // uiContainer.MinHeight = table.PixelMinHeight;
            ContainerStyleConfig gridStyleConfig = context.Config.ContainerStyles.GetContainerStyleConfig(table.GridStyle);
            Brush borderBrush = context.GetColorBrush(gridStyleConfig.BackgroundColor);

            // Modify context outer parent style so padding necessity can be determined
            childRenderArgs.ParentStyle = table.GridStyle != null ? table.GridStyle.Value : default(AdaptiveContainerStyle);
            context.RenderArgs = childRenderArgs;

            var tableProps = (IDictionary<string, object>)table.AdditionalProperties;
            int iRow = 0;
            foreach (var row in table.Rows)
            {
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
                    cell.Style = cell.Style ?? row.Style;
                    AdaptiveTypedElement rendereableElement = context.GetRendereableElement(cell);

                    // if there's an element that can be rendered, then render it, otherwise, skip
                    if (rendereableElement != null)
                    {
                        var uiCell = context.Render(rendereableElement);
                        if (uiCell != null)
                        {
                            var cellProps = (IDictionary<string, object>)cell.AdditionalProperties;
                            var cellHorizontalAlignment = cellProps.TryGetValue<AdaptiveHorizontalAlignment>("horizontalAlignment");

                            var uiGrid = uiCell.GetLogicalChildren().Where(cell => cell is Grid).Cast<Grid>().FirstOrDefault();
                            if (uiGrid != null)
                            {
                                uiGrid = uiGrid.GetLogicalChildren().Where(cell => cell is Grid).Cast<Grid>().FirstOrDefault();
                                if (uiGrid != null)
                                {

                                    foreach (var uiChild in uiGrid.Children)
                                    {
                                        if (cell.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                                            RendererUtil.ApplyVerticalContentAlignment(uiChild, cell.VerticalContentAlignment);
                                        else if (row.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                                            RendererUtil.ApplyVerticalContentAlignment(uiChild, row.VerticalContentAlignment);
                                        else if (table.VerticalContentAlignment != default(AdaptiveVerticalContentAlignment))
                                            RendererUtil.ApplyVerticalContentAlignment(uiChild, table.VerticalContentAlignment);

                                        if (cellHorizontalAlignment != default(AdaptiveHorizontalAlignment))
                                        {
                                            switch (cellHorizontalAlignment)
                                            {
                                                case AdaptiveHorizontalAlignment.Center:
                                                    uiChild.HorizontalAlignment = HorizontalAlignment.Center;
                                                    break;
                                                case AdaptiveHorizontalAlignment.Left:
                                                    uiChild.HorizontalAlignment = HorizontalAlignment.Left;
                                                    break;
                                                case AdaptiveHorizontalAlignment.Right:
                                                    uiChild.HorizontalAlignment = HorizontalAlignment.Right;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else if (row.HorizontalContentAlignment != default(AdaptiveHorizontalContentAlignment))
                                            RendererUtil.ApplyHorizontalContentAlignment(uiChild, row.HorizontalContentAlignment);
                                        else if (table.HorizontalContentAlignment != default(AdaptiveHorizontalContentAlignment))
                                            RendererUtil.ApplyHorizontalContentAlignment(uiChild, table.HorizontalContentAlignment);
                                    }
                                }
                            }

                            if (table.ShowGridLines)
                            {
                                uiCell = new Border() { BorderBrush = borderBrush, BorderThickness = new Thickness(1), Child = uiCell };
                            }

                            uiCell.SetValue(Grid.ColumnProperty, iCol);
                            uiCell.SetValue(Grid.RowProperty, iRow);

                            uiTable.Children.Add(uiCell);
                        }
                    }
                    iCol++;
                }
                iRow++;
            }

            // Revert context's value to that of outside the Container
            context.RenderArgs = parentRenderArgs;

            if (table.ShowGridLines == true)
            {
                uiRoot = new Border() { BorderBrush = borderBrush, BorderThickness = new Thickness(1), Child = uiTable };
            }

            return RendererUtil.ApplySelectAction(uiRoot, table, context);
        }


    }

}
