// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using Avalonia.Layout;
using System;



namespace AdaptiveCards.Rendering.Avalonia
{

    public static class AdaptiveNumberInputRenderer
    {
        public static Control Render(AdaptiveNumberInput input, AdaptiveRenderContext context)
        {
            var uiInput = new NumericUpDown()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            if (!Double.IsNaN(input.Value))
            {
                uiInput.Value = (decimal)input.Value;
            }

            // textBox.Style = context.GetStyle($"Adaptive.Input.Text.Number");
            uiInput.SetContext(input);

            if (!Double.IsNaN(input.Min))
                uiInput.Minimum = (decimal)input.Min;
            if (!Double.IsNaN(input.Max))
                uiInput.Maximum = (decimal)input.Max;
            
            if ((!Double.IsNaN(input.Max) || !Double.IsNaN(input.Min) || input.IsRequired)
                && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }

            context.InputValues.Add(input.Id, new AdaptiveNumberInputValue(input, uiInput));

            return uiInput;
        }
    }

    public class AdaptiveNumberInputValue : AdaptiveTextBoxInputValue
    {
        public AdaptiveNumberInputValue(AdaptiveNumberInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override string GetValue()
        {
            return (RenderedInputElement as NumericUpDown).Value?.ToString();
        }

        public override bool Validate()
        {
            bool isValid = base.Validate();

            AdaptiveNumberInput numberInput = InputElement as AdaptiveNumberInput;
            double inputValue = 0.0;

            if (isValid && Double.TryParse(GetValue(), out inputValue))
            {


                bool isMinValid = true, isMaxValid = true;
                if (!Double.IsNaN(numberInput.Min))
                {
                    isMinValid = (inputValue >= numberInput.Min);
                }

                if (!Double.IsNaN(numberInput.Max))
                {
                    isMaxValid = (inputValue <= numberInput.Max);
                }

                isValid = isValid && isMinValid && isMaxValid;
            }
            else
            {
                // if the input is not required and the string is empty, then proceed
                // This is a fail safe as non xceed controls are rendered with a Text
                if (!(!numberInput.IsRequired && String.IsNullOrEmpty(GetValue())))
                {
                    isValid = false;
                }
            }

            return isValid;
        }
    }


}
