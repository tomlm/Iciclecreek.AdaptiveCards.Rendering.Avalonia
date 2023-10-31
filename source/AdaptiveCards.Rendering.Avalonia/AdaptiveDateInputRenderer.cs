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

    public class AdaptiveDateInputValue : AdaptiveTextBoxInputValue
    {
        public AdaptiveDateInputValue(AdaptiveDateInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override string GetValue()
        {
            var datePicker = this.RenderedInputElement as DatePicker;
            return datePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
        }

        public override bool Validate()
        {
            bool isValid = base.Validate();

            AdaptiveDateInput dateInput = InputElement as AdaptiveDateInput;
            // Check if our input is valid
            string currentValue = GetValue();
            DateTime inputValue;
            if (DateTime.TryParse(currentValue, out inputValue))
            {
                DateTime minDate, maxDate;

                if (!String.IsNullOrEmpty(dateInput.Min))
                {
                    // if min is a valid date, compare against it, otherwise ignore
                    if (DateTime.TryParse(dateInput.Min, out minDate))
                    {
                        isValid = isValid && (inputValue >= minDate);
                    }
                }

                if (!String.IsNullOrEmpty(dateInput.Max))
                {
                    // if max is a valid date, compare against it, otherwise ignore
                    if (DateTime.TryParse(dateInput.Max, out maxDate))
                    {
                        isValid = isValid && (inputValue <= maxDate);
                    }
                }
            }
            else
            {
                // if the input is not required and the string is empty, then proceed
                // This is a fail safe as non xceed controls are rendered with a Text
                if (!(!dateInput.IsRequired && String.IsNullOrEmpty(currentValue)))
                {
                    isValid = false;
                }
            }

            return isValid;
        }


    }
}
