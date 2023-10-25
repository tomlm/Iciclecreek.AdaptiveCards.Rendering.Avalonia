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
            var timePicker = new TimePicker();
            if (TimeSpan.TryParse(input.Value, out var time))
            {
                timePicker.SelectedTime = time;
            }
            // timePicker.SetPlaceholder(input.Placeholder);
            // textBox.Style = context.GetStyle("Adaptive.Input.Text.Time");
            timePicker.SetContext(input);
            TimeSpan maxTime, minTime;
            if ((TimeSpan.TryParse(input.Max, out maxTime) || TimeSpan.TryParse(input.Min, out minTime) || input.IsRequired)
                && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }
            context.InputValues.Add(input.Id, new AdaptiveTimeInputValue(input, timePicker));

            return timePicker;
        }
    }
}
