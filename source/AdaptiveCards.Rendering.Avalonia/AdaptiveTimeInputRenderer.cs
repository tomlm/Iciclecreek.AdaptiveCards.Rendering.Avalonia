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

    public class AdaptiveTimeInputValue : AdaptiveTextBoxInputValue
    {
        public AdaptiveTimeInputValue(AdaptiveTimeInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override string GetValue()
        {
            var timePicker = this.RenderedInputElement as TimePicker;
            return timePicker.SelectedTime.Value.ToString("hh\\:mm");
        }

        public override bool Validate()
        {
            bool isValid = base.Validate();

            AdaptiveTimeInput timeInput = InputElement as AdaptiveTimeInput;
            string currentValue = GetValue();

            // Check if our input is valid
            TimeSpan inputValue;
            if (TimeSpan.TryParse(currentValue, out inputValue))
            {
                TimeSpan minTime, maxTime;

                if (!String.IsNullOrEmpty(timeInput.Min))
                {
                    // if min is a valid date, compare against it, otherwise ignore
                    if (TimeSpan.TryParse(timeInput.Min, out minTime))
                    {
                        isValid = isValid && (inputValue >= minTime);
                    }
                }

                if (!String.IsNullOrEmpty(timeInput.Max))
                {
                    // if max is a valid date, compare against it, otherwise ignore
                    if (TimeSpan.TryParse(timeInput.Max, out maxTime))
                    {
                        isValid = isValid && (inputValue <= maxTime);
                    }
                }
            }
            else
            {
                // if the input is not required and the string is empty, then proceed
                // This is a fail safe as non xceed controls are rendered with a TextBox
                if (!(!timeInput.IsRequired && String.IsNullOrEmpty(currentValue)))
                {
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}
