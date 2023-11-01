using Avalonia.Interactivity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Common;
using Avalonia.Controls;

namespace AdaptiveCards.Rendering.Avalonia
{
    /// <summary>
    /// Event args for the Action event on AdaptiveCardView control
    /// </summary>
    public class RoutedAdaptiveActionEventArgs : RoutedEventArgs
    {
        public RoutedAdaptiveActionEventArgs(RenderedAdaptiveCard source, AdaptiveAction action, AdaptiveHostConfig hostConfig)
            : base(AdaptiveCardView.ActionEvent, source)
        {
            Action = action;
            UserInputs = source.UserInputs;
            Card = source.OriginatingCard;
            HostConfig = hostConfig;
        }

        /// <summary>
        /// The action that fired
        /// </summary>
        public AdaptiveAction Action { get; }

        public RenderedAdaptiveCardInputs UserInputs { get; }

        public AdaptiveCard Card { get; }

        public AdaptiveHostConfig HostConfig { get; }


        /// <summary>
        /// Helper method to determine if the action is an Action.Submit or Action.Execute
        /// </summary>
        /// <returns></returns>
        public bool IsDataAction()
        {
            return Action is AdaptiveSubmitAction || Action is AdaptiveExecuteAction;
        }

        /// <summary>
        /// The payload of an action if it is Action.Submit or Action.Execute
        /// </summary>
        /// <returns>Inputs merged with action.data</returns>
        public object GetActionPayload()
        {
            object data = null;

            if (Action is AdaptiveSubmitAction submitAction)
            {
                data = submitAction.Data;
            }
            else if (Action is AdaptiveExecuteAction executeAction)
            {
                data = executeAction.Data;
            }

            var inputs = UserInputs.AsJson();
            if (data is JObject jobj)
            {
                inputs.Merge(jobj);
                return inputs;
            }
            else if (data is string)
                return data;
            else
                return inputs;
        }

    }
}
