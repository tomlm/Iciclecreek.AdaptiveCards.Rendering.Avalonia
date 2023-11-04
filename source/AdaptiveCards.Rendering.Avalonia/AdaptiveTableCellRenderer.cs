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
        public static Control Render(AdaptiveTableCell tableCell, AdaptiveRenderContext context)
        {
            var uiTableCell = AdaptiveContainerRenderer.Render(tableCell, context);
            return uiTableCell;
        }
    }
}
