// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;



namespace AdaptiveCards.Rendering.Avalonia
{
    /// <summary>
    /// Class to add to all rendered items so ToggleVisibility can hide the separators or deduct the spacing from the margin
    /// </summary>
    public class TagContent
    {

        public TagContent(Grid separator, Grid elementContainer)
        {
            Separator = separator;
            ParentContainerElement = elementContainer;
        }

        public Grid Separator { get; set; } = null;

        /// <summary>
        /// Grid that contains the rendered element
        /// </summary>
        public Grid ParentContainerElement { get; set; }

        /// <summary>
        /// Column Definition for the rendered column in the columnset
        /// </summary>
        public ColumnDefinition ColumnDefinition { get; set; } = null;

        /// <summary>
        /// Row Definition for the rendered element in the container
        /// </summary>
        public RowDefinition RowDefinition { get; set; } = null;

        public int ViewIndex { get; set; }

        /// <summary>
        /// Row Definition for the rendered element in the container
        /// </summary>
        public Panel EnclosingElement { get; set; }

        /// <summary>
        /// Row Definition for the rendered element in the container
        /// </summary>
        public Control ErrorMessage { get; set; }
    }
}
