// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;



namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveActionSetRenderer
    {
        public static Control Render(AdaptiveActionSet actionSet, AdaptiveRenderContext context)
        {
            var outerActionSet = new Grid();

            if (!context.Config.SupportsInteractivity)
                return outerActionSet;

            // outerActionSet.Style = context.GetStyle("Adaptive.Container");

            // Keep track of ContainerStyle.ForegroundColors before Container is rendered
            AdaptiveRenderArgs parentRenderArgs = context.RenderArgs;
            AdaptiveRenderArgs elementRenderArgs = new AdaptiveRenderArgs(parentRenderArgs);

            AddRenderedActions(outerActionSet, actionSet.Actions, context, actionSet.InternalID);

            return outerActionSet;
        }

        public static void AddRenderedActions(Grid uiContainer, IList<AdaptiveAction> actions, AdaptiveRenderContext context, AdaptiveInternalID actionSetId)
        {
            if (!context.Config.SupportsInteractivity)
                return;

            ActionsConfig actionsConfig = context.Config.Actions;
            int maxActions = actionsConfig.MaxActions;

            var primaryActions = actions.Where(action => action.Mode == AdaptiveActionMode.Primary).ToList();
            var secondaryActions = actions.Where(action => action.Mode == AdaptiveActionMode.Secondary).ToList();
            if (primaryActions.Count > context.Config.Actions.MaxActions)
            {
                secondaryActions = primaryActions.Skip(context.Config.Actions.MaxActions - 1).ToList();
                secondaryActions.AddRange(secondaryActions);
                primaryActions = primaryActions.Take(context.Config.Actions.MaxActions - 1).ToList();
            }

            var uiActionBar = new UniformGrid();
            var primaryCount = primaryActions.Count() + ((secondaryActions.Count > 0) ? 1 : 0);
            if (actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal)
                uiActionBar.Columns = primaryCount;
            else
                uiActionBar.Rows = primaryCount;

            uiActionBar.HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), actionsConfig.ActionAlignment.ToString());
            uiActionBar.VerticalAlignment = VerticalAlignment.Bottom;
            // uiActionBar.Style = context.GetStyle("Adaptive.Actions");

            // For vertical, we want to subtract the top margin of the first button
            int topMargin = actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal
                ? context.Config.GetSpacing(actionsConfig.Spacing)
                : context.Config.GetSpacing(actionsConfig.Spacing) - actionsConfig.ButtonSpacing;

            uiActionBar.Margin = new Thickness(0, topMargin, 0, 0);

            uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetRow(uiActionBar, uiContainer.RowDefinitions.Count - 1);
            uiContainer.Children.Add(uiActionBar);

            bool isInline = (actionsConfig.ShowCard.ActionMode == ShowCardActionMode.Inline);

            int iPos = 0;

            if (primaryActions.Any())
            {
                iPos = RenderPrimaryActions(uiContainer, context, actionSetId, actionsConfig, primaryActions, uiActionBar, isInline, iPos);
            }

            if (secondaryActions.Any())
            {
                iPos = RenderOverflowActions(uiContainer, context, actionSetId, actionsConfig, secondaryActions, uiActionBar, isInline, iPos);
            }

        }

        private static int RenderPrimaryActions(Grid uiContainer, AdaptiveRenderContext context, AdaptiveInternalID actionSetId, ActionsConfig actionsConfig, List<AdaptiveAction> actions, UniformGrid uiActionBar, bool isInline, int iPos)
        {
            // See if all actions have icons, otherwise force the icon placement to the left
            IconPlacement oldConfigIconPlacement = actionsConfig.IconPlacement;
            bool allActionsHaveIcons = true;
            foreach (AdaptiveAction action in actions)
            {
                if (string.IsNullOrEmpty(action.IconUrl))
                {
                    allActionsHaveIcons = false;
                    break;
                }
            }

            if (!allActionsHaveIcons)
            {
                actionsConfig.IconPlacement = IconPlacement.LeftOfTitle;
            }

            // indicates showcard has not been seen if it's set false; meaningful only if it's used
            // when inline is supported
            bool hasSeenInlineShowCard = false;

            foreach (AdaptiveAction action in actions)
            {
                var rendereableAction = context.GetRendereableElement(action);

                // if there is a renderable action then render it, else drop it.
                if (rendereableAction != null)
                {
                    if ((rendereableAction is AdaptiveSubmitAction) ||
                        (rendereableAction is AdaptiveExecuteAction))
                    {
                        context.SubmitActionCardId[rendereableAction as AdaptiveAction] = context.RenderArgs.ContainerCardId;
                    }

                    // add actions
                    var uiAction = context.Render(rendereableAction) as Button;

                    if (uiAction == null)
                    {
                        context.Warnings.Add(new AdaptiveWarning(-1, $"action failed to render" +
                            $"and valid fallback wasn't present"));
                        continue;
                    }

                    if (actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal)
                    {
                        if (uiActionBar.Children.Count > 0) // don't apply left margin to the first item
                            uiAction.Margin = new Thickness(actionsConfig.ButtonSpacing, 0, 0, 0);
                    }
                    else
                    {
                        uiAction.Margin = new Thickness(0, actionsConfig.ButtonSpacing, 0, 0);
                    }

                    if (actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal)
                        Grid.SetColumn(uiAction, iPos++);

                    uiActionBar.Children.Add(uiAction);

                    if (action is AdaptiveShowCardAction showCardAction && isInline)
                    {
                        CreateShowCardAction(uiContainer, context, actionSetId, actionsConfig, hasSeenInlineShowCard, uiAction, showCardAction);
                    }
                }
            }
            // Restore the iconPlacement for the context.
            actionsConfig.IconPlacement = oldConfigIconPlacement;
            return iPos;
        }

        private static int RenderOverflowActions(Grid uiContainer, AdaptiveRenderContext context, AdaptiveInternalID actionSetId, ActionsConfig actionsConfig, List<AdaptiveAction> actions, UniformGrid uiActionBar, bool isInline, int iPos)
        {
            var uiFlyout = new Flyout()
            {
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                ShowMode = FlyoutShowMode.Standard,
            };
            var uiMenu = new UniformGrid()
            {
                Rows = actions.Count(),
                Margin = new Thickness(0, 0, 0, 0),
                Background = context.GetColorBrush("Transparent"),
            };
            uiFlyout.Content = uiMenu;

            // See if all actions have icons, otherwise force the icon placement to the left
            IconPlacement oldConfigIconPlacement = actionsConfig.IconPlacement;
            bool allActionsHaveIcons = true;
            foreach (AdaptiveAction action in actions)
            {
                if (string.IsNullOrEmpty(action.IconUrl))
                {
                    allActionsHaveIcons = false;
                    break;
                }
            }

            if (!allActionsHaveIcons)
            {
                actionsConfig.IconPlacement = IconPlacement.LeftOfTitle;
            }

            // indicates showcard has not been seen if it's set false; meaningful only if it's used
            // when inline is supported
            bool hasSeenInlineShowCard = false;
            context.IsRenderingOverflowAction = true;
            var iRow = 0;
            foreach (AdaptiveAction action in actions)
            {
                var rendereableAction = context.GetRendereableElement(action);

                // if there is a renderable action then render it, else drop it.
                if (rendereableAction != null)
                {
                    if ((rendereableAction is AdaptiveSubmitAction) ||
                        (rendereableAction is AdaptiveExecuteAction))
                    {
                        context.SubmitActionCardId[rendereableAction as AdaptiveAction] = context.RenderArgs.ContainerCardId;
                    }

                    // add actions
                    var uiAction = context.Render(rendereableAction) as Button;

                    if (uiAction == null)
                    {
                        context.Warnings.Add(new AdaptiveWarning(-1, $"action failed to render" +
                            $"and valid fallback wasn't present"));
                        continue;
                    }

                    // vertical
                    uiAction.BorderThickness = new Thickness(0);
                    uiAction.HorizontalAlignment = HorizontalAlignment.Stretch;
                    uiAction.Background = context.GetColorBrush("Transparent");
                    uiAction.HorizontalContentAlignment = HorizontalAlignment.Left;
                    uiAction.Margin = new Thickness(0, actionsConfig.ButtonSpacing, 0, 0);

                    Grid.SetRow(uiAction, iRow++);
                    uiMenu.Children.Add(uiAction);

                    if (action is AdaptiveShowCardAction showCardAction && isInline)
                    {
                        if (actionSetId != null)
                        {
                            hasSeenInlineShowCard = CreateShowCardAction(uiContainer, context, actionSetId, actionsConfig, hasSeenInlineShowCard, uiAction, showCardAction);
                        }
                        else
                        {
                            context.Warnings.Add(new AdaptiveWarning(-1, $"button's corresponding showCard" +
                                $" couldn't be added since the action set the button belongs to has null as internal id"));
                        }
                    }
                }
            }

            context.IsRenderingOverflowAction = false;

            // Restore the iconPlacement for the context.
            actionsConfig.IconPlacement = oldConfigIconPlacement;

            var uiOverflow = context.Render(new AdaptiveOverflowAction());
            uiOverflow[FlyoutBase.AttachedFlyoutProperty] = uiFlyout;

            if (actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal)
            {
                if (uiActionBar.Children.Count > 0) // don't apply left margin to the first item
                    uiOverflow.Margin = new Thickness(actionsConfig.ButtonSpacing, 0, 0, 0);
            }
            else
            {
                uiOverflow.Margin = new Thickness(0, actionsConfig.ButtonSpacing, 0, 0);
            }

            if (actionsConfig.ActionsOrientation == ActionsOrientation.Horizontal)
                Grid.SetColumn(uiOverflow, iPos++);

            uiActionBar.Children.Add(uiOverflow);
            return iPos;
        }

        private static bool CreateShowCardAction(Grid uiContainer, AdaptiveRenderContext context, AdaptiveInternalID actionSetId, ActionsConfig actionsConfig, bool hasSeenInlineShowCard, Button? uiAction, AdaptiveShowCardAction showCardAction)
        {
            // the button's context is used as key for retrieving the corresponding showcard
            uiAction.SetContext(actionSetId);

            if (!hasSeenInlineShowCard)
            {
                // Define a new row to contain all the show cards
                uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                // it's first showcard of the peers, create a new list
                context.PeerShowCardsInActionSet[actionSetId] = new List<Control>();
            }

            hasSeenInlineShowCard = true;

            Grid uiShowCardContainer = new Grid();
            // uiShowCardContainer.Style = context.GetStyle("Adaptive.Actions.ShowCard");
            uiShowCardContainer.DataContext = showCardAction;
            uiShowCardContainer.IsVisible = false;
            var padding = context.Config.Spacing.Padding;
            // set negative margin to expand the wrapper to the edge of outer most card
            uiShowCardContainer.Margin = new Thickness(-padding, actionsConfig.ShowCard.InlineTopMargin, -padding, -padding);
            var showCardStyleConfig = context.Config.ContainerStyles.GetContainerStyleConfig(actionsConfig.ShowCard.Style);
            uiShowCardContainer.Background = context.GetColorBrush(showCardStyleConfig.BackgroundColor);

            // before rendering the card, we save the current parent card id
            AdaptiveInternalID currentParentCardId = context.RenderArgs.ContainerCardId;

            // render the card
            var uiShowCardWrapper = (Grid)context.Render(showCardAction.Card);
            uiShowCardWrapper.Background = context.GetColorBrush("Transparent");
            uiShowCardWrapper.DataContext = showCardAction;

            // after rendering we re-establish the ContainerCardId as it may have been modified
            // while rendering other cards
            context.RenderArgs.ContainerCardId = currentParentCardId;

            uiShowCardContainer.Children.Add(uiShowCardWrapper);
            context.ActionShowCards.Add(uiAction, uiShowCardContainer);
            // added the rendered show card as a peer
            context.PeerShowCardsInActionSet[actionSetId].Add(uiShowCardContainer);
            // define where in the rows of the parent Grid the show card will occupy
            // and add it to the parent
            Grid.SetRow(uiShowCardContainer, uiContainer.RowDefinitions.Count - 1);
            uiContainer.Children.Add(uiShowCardContainer);
            return hasSeenInlineShowCard;
        }

        private static List<AdaptiveAction> GetActionsToProcess(IList<AdaptiveAction> actions, int maxActions, AdaptiveRenderContext context, AdaptiveActionMode mode)
        {
            // If the number of actions is bigger than maxActions, then log warning for it
            if (actions.Count > maxActions)
            {
                context.Warnings.Add(new AdaptiveWarning((int)AdaptiveWarning.WarningStatusCode.MaxActionsExceeded, "Some actions were not rendered due to exceeding the maximum number of actions allowed"));
            }

            // just take the first maxActions actions
            return actions.Take(maxActions).ToList();
        }
    }

    public class AdaptiveOverflowAction : AdaptiveAction
    {
        public AdaptiveOverflowAction()
        {
            Title = "...";
        }

        public override string Type { get => "Action.Ellipsis"; set => throw new NotImplementedException(); }
    }


}
