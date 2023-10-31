// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using System;
using System.Text.RegularExpressions;

namespace AdaptiveCards.Rendering.Avalonia
{
    public abstract class AdaptiveInputValue
    {
        public AdaptiveInputValue(AdaptiveInput input, Control renderedInput)
        {
            InputElement = input;
            RenderedInputElement = renderedInput;
            VisualElementForAccessibility = renderedInput;
        }

        public AdaptiveInputValue(AdaptiveInput input, Control renderedInput, Control visualElementForAccessibility)
        {
            InputElement = input;
            RenderedInputElement = renderedInput;
            VisualElementForAccessibility = visualElementForAccessibility;
        }

        public abstract string GetValue();

        public abstract bool Validate();

        public abstract void SetFocus();

        public virtual void ChangeVisualCueVisibility(bool inputIsValid)
        {
            // Change visibility for error message (and spacing)
            if (ErrorMessage != null)
            {
                TagContent tagContent = ErrorMessage.Tag as TagContent;
                RendererUtil.SetVisibility(ErrorMessage, !inputIsValid, tagContent);

                string helpText = "";
                if (!inputIsValid)
                {
                    helpText = ErrorMessage.Text;
                }

                // AutomationProperties.SetHelpText(VisualElementForAccessibility, helpText);
            }
        }

        public AdaptiveInput InputElement { get; set; }

        public Control RenderedInputElement { get; set; }

        public TextBlock ErrorMessage { private get; set; }

        public Control VisualElementForAccessibility { get; set; }
    }

    /// <summary>
    /// Abstract class that implements the Validate behaviour for isRequired in almost all inputValues (except for Input.Toggle)
    /// </summary>
    public abstract class AdaptiveInputValueNonEmptyValidation : AdaptiveInputValue
    {
        public AdaptiveInputValueNonEmptyValidation(AdaptiveInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public AdaptiveInputValueNonEmptyValidation(AdaptiveInput input, Control renderedInput, Control visualElementForAccessibility) :
            base(input, renderedInput, visualElementForAccessibility)
        { }

        public override bool Validate()
        {
            bool isValid = true;

            if (InputElement.IsRequired)
            {
                isValid = !(String.IsNullOrEmpty(GetValue()));
            }

            return isValid;
        }
    }

    /// <summary>
    /// Intermediate class, as most of the elements in the vanilla wpf (no xceed) renderers use a textbox,
    /// this class was created to avoid all inputValues to implement the same GetValue and Focus method
    /// </summary>
    public class AdaptiveTextBoxInputValue : AdaptiveInputValueNonEmptyValidation
    {
        public AdaptiveTextBoxInputValue(AdaptiveInput inputElement, Control renderedElement) : base(inputElement, renderedElement) { }

        public override string GetValue()
        {
            return (RenderedInputElement as TextBox).Text;
        }

        public override void SetFocus()
        {
            RenderedInputElement.Focus();
        }

        public override void ChangeVisualCueVisibility(bool isInputValid)
        {
            base.ChangeVisualCueVisibility(isInputValid);

            if (isInputValid)
            {
                VisualErrorCue.BorderThickness = new Thickness(0);
            }
            else
            {
                VisualErrorCue.BorderThickness = new Thickness(2);
            }
        }

        public Border VisualErrorCue { private get; set; }
    }

  
}
