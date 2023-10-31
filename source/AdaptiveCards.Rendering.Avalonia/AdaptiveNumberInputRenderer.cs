// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia.Controls;
using System;



namespace AdaptiveCards.Rendering.Avalonia
{

    public static class AdaptiveNumberInputRenderer
    {
        public static Control Render(AdaptiveNumberInput input, AdaptiveRenderContext context)
        {
            var uiInput = new NumericUpDown();

            if (!Double.IsNaN(input.Value))
            {
                uiInput.Value = (decimal)input.Value;
            }

            // textBox.Style = context.GetStyle($"Adaptive.Input.Text.Number");
            uiInput.SetContext(input);

            if (input.Min != 0)
                uiInput.Minimum = (decimal)input.Min;
            if (input.Max != 0)
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
}
