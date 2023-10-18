// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;



namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveTimeInputRenderer
    {
        public static Control Render(AdaptiveTimeInput input, AdaptiveRenderContext context)
        {
            var textBox = new TextBox() { Text = input.Value };
            textBox.SetPlaceholder(input.Placeholder);
            // textBox.Style = context.GetStyle("Adaptive.Input.Text.Time");
            textBox.SetContext(input);

            TimeSpan maxTime, minTime;
            if ((TimeSpan.TryParse(input.Max, out maxTime) || TimeSpan.TryParse(input.Min, out minTime) || input.IsRequired)
                && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }

            context.InputValues.Add(input.Id, new AdaptiveTimeInputValue(input, textBox));

            return textBox;
        }
    }
}
