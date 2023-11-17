using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Templates;
using System.Collections;
using Avalonia.Metadata;

namespace Iciclecreek.Avalonia.Controls
{
    public partial class ColumnsPanel : UserControl
    {
        /// <summary>
        /// The default value for the <see cref="ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<Panel?> DefaultPanel =
            new(() => new StackPanel());

        /// <summary>
        /// Defines the <see cref="ColumnSpacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ColumnSpacingProperty =
            AvaloniaProperty.Register<ColumnsPanel, double>(nameof(ColumnsPanel), 0);

        /// <summary>
        /// Defines the <see cref="ColumnDefinitions"/> property.
        /// </summary>
        public static readonly StyledProperty<ColumnDefinitions> ColumnDefinitionsProperty =
            AvaloniaProperty.Register<ColumnsPanel, ColumnDefinitions>(nameof(ColumnDefinitions), new ColumnDefinitions());

        /// <summary>
        /// Defines the <see cref="ItemsPanel"/> property.
        /// </summary>
        public static readonly StyledProperty<ITemplate<Panel?>> ItemsPanelProperty =
            AvaloniaProperty.Register<ColumnsPanel, ITemplate<Panel?>>(nameof(ItemsPanel), DefaultPanel);

        /// <summary>
        /// Defines the <see cref="ItemsSource"/> property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<ColumnsPanel, IEnumerable?>(nameof(ItemsSource));

        /// <summary>
        /// Defines the <see cref="ItemTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
            AvaloniaProperty.Register<ColumnsPanel, IDataTemplate?>(nameof(ItemTemplate));

        private Grid _grid = new Grid();

        public ColumnsPanel()
        {
            _grid = new Grid();
            this.Content = _grid;
            this.GetSubject(ItemsSourceProperty).Subscribe((value) => BindColumns());
            this.GetSubject(ColumnDefinitionsProperty).Subscribe((value) => BindColumns());
            this.GetSubject(ColumnSpacingProperty).Subscribe((value) => BindColumns());
            //this.PropertyChanged += (sender, args) =>
            //{
            //    if (args.Property.Name == nameof(ItemsSource) ||
            //        args.Property.Name == nameof(ColumnDefinitions))
            //    {
            //        BindColumns();
            //    }
            //};
        }

        /// <summary>
        /// The column definitions to use.
        /// </summary>
        public ColumnDefinitions ColumnDefinitions
        {
            get => GetValue(ColumnDefinitionsProperty);
            set => SetValue(ColumnDefinitionsProperty, value);
        }

        /// <summary>
        /// Gets or sets the panel used to display the items.
        /// </summary>
        public ITemplate<Panel?> ItemsPanel
        {
            get => GetValue(ItemsPanelProperty);
            set => SetValue(ItemsPanelProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content of the <see cref="ItemsControl"/>.
        /// </summary>
        /// <remarks>
        /// A common scenario is to use an <see cref="ItemsControl"/> such as a 
        /// <see cref="ListBox"/> to display a data collection, or to bind an
        /// <see cref="ItemsControl"/> to a collection object. To bind an <see cref="ItemsControl"/>
        /// to a collection object, use the <see cref="ItemsSource"/> property.
        /// 
        /// When the <see cref="ItemsSource"/> property is set, the <see cref="Items"/> collection
        /// is made read-only and fixed-size.
        ///
        /// When <see cref="ItemsSource"/> is in use, setting the property to null removes the
        /// collection and restores usage to <see cref="Items"/>, which will be an empty 
        /// <see cref="ItemCollection"/>.
        /// </remarks>
        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the data template used to display the items in the control.
        /// </summary>
        [InheritDataTypeFromItems(nameof(ItemsSource))]
        public IDataTemplate? ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the column spacing to use.
        /// </summary>
        public double ColumnSpacing
        {
            get => GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        private void BindColumns()
        {
            int totalColumns = this.ColumnDefinitions.Count;
            _grid.ColumnDefinitions.Clear();
            _grid.Children.Clear();

            // define grid column definitions (with extra columns for spacing)
            int iCol = 0;
            int iGridCol = 0;
            foreach (var columnDefinition in ColumnDefinitions)
            {
                _grid.ColumnDefinitions.Add(columnDefinition);
                iCol++;
                iGridCol++;
                if (ColumnSpacing != 0 && iCol < totalColumns)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition(ColumnSpacing, GridUnitType.Pixel));
                    iGridCol++;
                }
            }

            // create itemscontrol to hold items for each column
            iGridCol = 0;
            List<ItemsControl> columns = ColumnDefinitions.Select(cd =>
            {
                var column = new ItemsControl()
                {
                    ItemTemplate = this.ItemTemplate,
                    ItemsPanel = this.ItemsPanel
                };
                Grid.SetColumn(column, iGridCol++);
                if (ColumnSpacing != 0)
                    iGridCol++;
                return column;
            }).ToList();

            // assign each item to a column container based on the column index
            iCol = 0;
            foreach (var columnDefinition in ColumnDefinitions)
            {
                if (this.ItemsSource is IEnumerable<object> items)
                {
                    columns[iCol].ItemsSource = items.Where((item, index) => (index % totalColumns) == iCol);
                }
                iCol++;
            }

            // add the columns as the children to the grid (each column with Grid.Column set appropriately)
            _grid.Children.AddRange(columns);
        }


    }
}
