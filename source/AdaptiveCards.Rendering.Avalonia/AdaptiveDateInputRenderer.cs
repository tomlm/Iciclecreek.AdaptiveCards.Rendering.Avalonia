// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;



namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveDateInputRenderer
    {
        public static Control Render(AdaptiveDateInput input, AdaptiveRenderContext context)
        {
            var datePicker = new DatePicker();
            if (DateTimeOffset.TryParse(input.Value, out var date))
            {
                datePicker.SelectedDate = date;
            }
            // textBox.SetPlaceholder(input.Placeholder);
            // textBox.Style = context.GetStyle($"Adaptive.Input.Text.Date");
            datePicker.SetContext(input);

            DateTimeOffset maxDate, minDate;
            if (DateTimeOffset.TryParse(input.Min, out minDate))
            {
                datePicker.MinYear = minDate;
            }
            
            if (DateTimeOffset.TryParse(input.Max, out maxDate))
            {
                datePicker.MaxYear = maxDate;
            }


            if ((DateTimeOffset.TryParse(input.Max, out maxDate) || DateTimeOffset.TryParse(input.Min, out minDate) || input.IsRequired)
                && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }

            context.InputValues.Add(input.Id, new AdaptiveDateInputValue(input, datePicker));

            return datePicker;
        }
    }
}
