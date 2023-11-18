using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Input;

namespace Iciclecreek.Avalonia.Controls
{
    /// <summary>
    /// A panel which lays out its children horizontally or vertically.
    /// </summary>
    public class ColumnsPanel : Panel, INavigableContainer
    {
        /// <summary>
        /// Defines the <see cref="Gap"/> property.
        /// </summary>
        public static readonly StyledProperty<double> GapProperty = AvaloniaProperty.Register<ColumnsPanel, double>(nameof(Gap));

        /// <summary>
        /// Defines the <see cref="ColumnGap"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ColumnGapProperty = AvaloniaProperty.Register<ColumnsPanel, double>(nameof(Gap));

        /// <summary>
        /// Defines the <see cref="ColumnWidthProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ColumnWidthProperty = AvaloniaProperty.Register<ColumnsPanel, double>(nameof(ColumnWidth), 200);

        /// <summary>
        /// Initializes static members of the <see cref="ColumnsPanel"/> class.
        /// </summary>
        static ColumnsPanel()
        {
            AffectsMeasure<ColumnsPanel>(GapProperty);
            // AffectsMeasure<ColumnsPanel2>(OrientationProperty);
        }

        /// <summary>
        /// Gets or sets the size of the gap to place between child controls.
        /// </summary>
        public double Gap
        {
            get { return GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }

        /// <summary>
        /// Gets or sets the size of the gap to place between columns
        /// </summary>
        public double ColumnGap
        {
            get { return GetValue(ColumnGapProperty); }
            set { SetValue(ColumnGapProperty, value); }
        }

        /// <summary>
        /// Gets or sets the orientation in which child controls will be layed out.
        /// </summary>
        public double ColumnWidth
        {
            get { return GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        /// <summary>
        /// Gets the next control in the specified direction.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="from">The control from which movement begins.</param>
        /// <returns>The control.</returns>
        public IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            var fromControl = from as Control;
            return (fromControl != null) ? GetControlInDirection(direction, fromControl) : null;
        }

        /// <summary>
        /// Gets the next control in the specified direction.
        /// </summary>
        /// <param name="direction">The movement direction.</param>
        /// <param name="from">The control from which movement begins.</param>
        /// <returns>The control.</returns>
        protected virtual IInputElement GetControlInDirection(NavigationDirection direction, Control from)
        {
            int index = Children.IndexOf((Control)from);

            switch (direction)
            {
                case NavigationDirection.First:
                    index = 0;
                    break;
                case NavigationDirection.Last:
                    index = Children.Count - 1;
                    break;
                case NavigationDirection.Next:
                    ++index;
                    break;
                case NavigationDirection.Previous:
                    --index;
                    break;
                case NavigationDirection.Left:
                    index = -1;
                    break;
                case NavigationDirection.Right:
                    index = -1;
                    break;
                case NavigationDirection.Up:
                    index = index - 1;
                    break;
                case NavigationDirection.Down:
                    index = index + 1;
                    break;
                default:
                    index = -1;
                    break;
            }

            if (index >= 0 && index < Children.Count)
            {
                return Children[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Measures the control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size of the control.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            double childAvailableWidth = double.PositiveInfinity;
            double childAvailableHeight = double.PositiveInfinity;

            childAvailableWidth = availableSize.Width;

            if (!double.IsNaN(Width))
            {
                childAvailableWidth = Width;
            }

            childAvailableWidth = Math.Min(childAvailableWidth, MaxWidth);
            childAvailableWidth = Math.Max(childAvailableWidth, MinWidth);

            var nColumns = Math.Max(1, (int)Math.Floor(childAvailableWidth / (ColumnWidth + ColumnGap)));

            var columnHeights = new List<double>(nColumns);
            for (int i = 0; i < nColumns; i++)
                columnHeights.Add((double)0);

            double measuredWidth = nColumns * (ColumnWidth + ColumnGap);

            foreach (Control child in Children)
            {
                child.Measure(new Size(ColumnWidth, childAvailableHeight));
                columnHeights[columnHeights.IndexOfMin()] += child.DesiredSize.Height + Gap;
            }

            var measuredHeight = columnHeights.Max() - Gap;
            return new Size(measuredWidth, measuredHeight);
        }

        /// <summary>
        /// Arranges the control's children.
        /// </summary>
        /// <param name="finalSize">The size allocated to the control.</param>
        /// <returns>The space taken.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double arrangedWidth = finalSize.Width;
            double arrangedHeight = finalSize.Height;
            double gap = Gap;

            arrangedHeight = 0;
            var nColumns = Math.Max(1, (int)Math.Floor(arrangedWidth / (ColumnWidth + ColumnGap)));

            var columnHeights = new List<double>(nColumns);
            var columnLefts = new List<double>(nColumns);
            for (int i = 0; i < nColumns; i++)
            {
                columnHeights.Add((double)0);
                columnLefts.Add((double)0);
            }

            double left = 0;
            for (int iCol = 0; iCol < nColumns; iCol++)
            {
                if (iCol > 0)
                {
                    columnLefts[iCol] = left;
                }
                left += ColumnWidth + ColumnGap;
            }

            foreach (Control child in Children)
            {
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;

                //  double width = Math.Max(childWidth, arrangedWidth);
                var minCol = columnHeights.IndexOfMin();
                var childLeft = columnLefts[minCol];
                var childTop = columnHeights[minCol];
                if (arrangedWidth < ColumnWidth && nColumns == 1)
                    childHeight = arrangedWidth / childWidth * childHeight;
                Rect childFinal = new Rect(childLeft, childTop, Math.Min(arrangedWidth, ColumnWidth), childHeight);
                child.Arrange(childFinal);
                columnHeights[minCol] += childHeight + Gap;
            }

            arrangedHeight = Math.Max(arrangedHeight - gap, finalSize.Height);
            return new Size(arrangedWidth, arrangedHeight);
        }

    }

    public static class Extensions
    {
        public static int IndexOfMin(this IEnumerable<double> list)
        {
            var min = list.First();
            int minIndex = 0;

            int i = 0;
            foreach (var item in list)
            {
                if (item < min)
                {
                    min = item;
                    minIndex = i;
                }
                i++;
            }

            return minIndex;
        }

    }
}
