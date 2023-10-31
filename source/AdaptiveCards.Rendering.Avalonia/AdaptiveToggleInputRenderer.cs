// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Avalonia.Controls;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveToggleInputRenderer
    {
        public static Control Render(AdaptiveToggleInput input, AdaptiveRenderContext context)
        {
            var uiToggle = new CheckBox();
            AdaptiveChoiceSetRenderer.SetContent(uiToggle, input.Title, input.Wrap);
            uiToggle.Foreground =
                context.GetColorBrush(context.Config.ContainerStyles.Default.ForegroundColors.Default.Default);
            uiToggle.SetState(input.Value == (input.ValueOn ?? "true"));
            // uiToggle.Style = context.GetStyle($"Adaptive.Input.Toggle");
            uiToggle.SetContext(input);

            if (input.IsRequired && string.IsNullOrEmpty(input.ErrorMessage))
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.NoErrorMessageForValidatedInput,
                    "Inputs with validation should include an ErrorMessage"));
            }

            context.InputValues.Add(input.Id, new AdaptiveToggleInputValue(input, uiToggle));

            return uiToggle;
        }
    }

    public class AdaptiveToggleInputValue : AdaptiveInputValue
    {
        public AdaptiveToggleInputValue(AdaptiveToggleInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override string GetValue()
        {
            CheckBox uiToggle = RenderedInputElement as CheckBox;
            AdaptiveToggleInput toggleInput = InputElement as AdaptiveToggleInput;

            return (uiToggle.GetState() == true ? toggleInput.ValueOn ?? "true" : toggleInput.ValueOff ?? "false");
        }

        public override void SetFocus()
        {
            RenderedInputElement.Focus();
        }

        public override bool Validate()
        {
            bool isValid = true;

            if (InputElement.IsRequired)
            {
                AdaptiveToggleInput toggleInput = InputElement as AdaptiveToggleInput;
                isValid = (GetValue() == toggleInput.ValueOn);
            }

            return isValid;
        }
    }
}
