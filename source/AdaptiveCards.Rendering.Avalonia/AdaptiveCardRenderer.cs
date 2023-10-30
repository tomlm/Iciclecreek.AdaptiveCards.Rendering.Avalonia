// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;




namespace AdaptiveCards.Rendering.Avalonia
{
    public class AdaptiveCardRenderer : AdaptiveCardRendererBase<Control, AdaptiveRenderContext>
    {
        protected override AdaptiveSchemaVersion GetSupportedSchemaVersion()
        {
            return new AdaptiveSchemaVersion(1, 5);
        }

        protected Action<object, AdaptiveActionEventArgs> ActionCallback;
        protected Action<object, MissingInputEventArgs> missingDataCallback;

        protected RamCachedWebImageLoader _imageLoader = new RamCachedWebImageLoader();

        public AdaptiveCardRenderer() : this(new AdaptiveHostConfig()) { }

        public AdaptiveCardRenderer(AdaptiveHostConfig hostConfig)
        {
            HostConfig = hostConfig ?? new AdaptiveHostConfig();
            SetObjectTypes();
        }

        private void SetObjectTypes()
        {
            ElementRenderers.Set<AdaptiveCard>(RenderAdaptiveCardWrapper);

            ElementRenderers.Set<AdaptiveTextBlock>(AdaptiveTextBlockRenderer.Render);
            ElementRenderers.Set<AdaptiveRichTextBlock>(AdaptiveRichTextBlockRenderer.Render);

            ElementRenderers.Set<AdaptiveImage>(AdaptiveImageRenderer.Render);
            ElementRenderers.Set<AdaptiveMedia>(AdaptiveMediaRenderer.Render);

            ElementRenderers.Set<AdaptiveContainer>(AdaptiveContainerRenderer.Render);
            ElementRenderers.Set<AdaptiveColumn>(AdaptiveColumnRenderer.Render);
            ElementRenderers.Set<AdaptiveColumnSet>(AdaptiveColumnSetRenderer.Render);
            ElementRenderers.Set<AdaptiveFactSet>(AdaptiveFactSetRenderer.Render);
            ElementRenderers.Set<AdaptiveImageSet>(AdaptiveImageSetRenderer.Render);
            ElementRenderers.Set<AdaptiveActionSet>(AdaptiveActionSetRenderer.Render);

            ElementRenderers.Set<AdaptiveChoiceSetInput>(AdaptiveChoiceSetRenderer.Render);
            ElementRenderers.Set<AdaptiveTextInput>(AdaptiveTextInputRenderer.Render);
            ElementRenderers.Set<AdaptiveNumberInput>(AdaptiveNumberInputRenderer.Render);
            ElementRenderers.Set<AdaptiveDateInput>(AdaptiveDateInputRenderer.Render);
            ElementRenderers.Set<AdaptiveTimeInput>(AdaptiveTimeInputRenderer.Render);
            ElementRenderers.Set<AdaptiveToggleInput>(AdaptiveToggleInputRenderer.Render);

            ElementRenderers.Set<AdaptiveAction>(AdaptiveActionRenderer.Render);
        }

        public AdaptiveFeatureRegistration FeatureRegistration { get; } = new AdaptiveFeatureRegistration();

        public AdaptiveActionHandlers ActionHandlers { get; } = new AdaptiveActionHandlers();

        //         public ResourceResolver ResourceResolvers { get; } = new ResourceResolver();

        public static Control RenderAdaptiveCardWrapper(AdaptiveCard card, AdaptiveRenderContext context)
        {
            var outerGrid = new Grid();
            // outerGrid.Style = context.GetStyle("Adaptive.Card");

            outerGrid.Background = context.GetColorBrush(context.Config.ContainerStyles.Default.BackgroundColor);
            outerGrid.SetBackgroundSource(card.BackgroundImage, context);

            if (context.CardRoot == null)
            {
                context.CardRoot = outerGrid;
            }

            // Reset the parent style
            context.RenderArgs.ParentStyle = AdaptiveContainerStyle.Default;

            var grid = new Grid();
            // grid.Style = context.GetStyle("Adaptive.InnerCard");
            grid.Margin = new Thickness(context.Config.Spacing.Padding);

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            switch (card.VerticalContentAlignment)
            {
                case AdaptiveVerticalContentAlignment.Center:
                    grid.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case AdaptiveVerticalContentAlignment.Bottom:
                    grid.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case AdaptiveVerticalContentAlignment.Top:
                default:
                    break;
            }

            outerGrid.MinHeight = card.PixelMinHeight;

            outerGrid.Children.Add(grid);

            AdaptiveInternalID parentCardId = context.RenderArgs.ContainerCardId;
            context.ParentCards.Add(card.InternalID, parentCardId);
            context.RenderArgs.ContainerCardId = card.InternalID;

            AdaptiveContainerRenderer.AddContainerElements(grid, card.Body, context);
            AdaptiveActionSetRenderer.AddRenderedActions(grid, card.Actions, context, card.InternalID);

            context.RenderArgs.ContainerCardId = parentCardId;

            if (card.SelectAction != null)
            {
                var outerGridWithSelectAction = context.RenderSelectAction(card.SelectAction, outerGrid);

                return outerGridWithSelectAction;
            }

            return outerGrid;
        }

        /// <summary>
        /// Renders an adaptive card.
        /// </summary>
        /// <param name="card">The card to render</param>
        public RenderedAdaptiveCard RenderCard(AdaptiveCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            RenderedAdaptiveCard renderCard = null;

            void ActionCallback(object sender, AdaptiveActionEventArgs args)
            {
                renderCard?.InvokeOnAction(args);
            }

            void MediaClickCallback(object sender, AdaptiveMediaEventArgs args)
            {
                renderCard?.InvokeOnMediaClick(args);
            }

            var context = new AdaptiveRenderContext(ActionCallback, null, MediaClickCallback)
            {
                ImageLoader = _imageLoader,
                ActionHandlers = ActionHandlers,
                Config = HostConfig ?? new AdaptiveHostConfig(),
                ElementRenderers = ElementRenderers,
                FeatureRegistration = FeatureRegistration,
                Lang = card.Lang,
                RenderArgs = new AdaptiveRenderArgs { ForegroundColors = (HostConfig != null) ? HostConfig.ContainerStyles.Default.ForegroundColors : new ContainerStylesConfig().Default.ForegroundColors }
            };

            var element = context.Render(card);
            element.Classes.Add(nameof(AdaptiveCard));

            renderCard = new RenderedAdaptiveCard(element, card, context.Warnings, ref context.InputBindings);

            return renderCard;
        }

        /// <summary>
        /// Renders an adaptive card to a PNG image. This method cannot be called from a server. Use <see cref="RenderCardToImageOnStaThreadAsync"/> instead.
        /// </summary>
        /// <param name="createStaThread">If true this method will create an STA thread allowing it to be called from a server.</param>
        public async Task<RenderedAdaptiveCardImage> RenderCardToImageAsync(AdaptiveCard card, bool createStaThread, int width = 400, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            if (createStaThread)
            {
                return await await Task.Factory.StartNewSta(async () => await RenderCardToImageInternalAsync(card, width, cancellationToken));
            }
            else
            {
                return await RenderCardToImageInternalAsync(card, width, cancellationToken);
            }
        }

        private async Task<RenderedAdaptiveCardImage> RenderCardToImageInternalAsync(AdaptiveCard card, int width, CancellationToken cancellationToken)
        {
            RenderedAdaptiveCardImage renderCard = null;

            try
            {

                var context = new AdaptiveRenderContext(null, null, null)
                {
                    ImageLoader = _imageLoader,
                    ActionHandlers = ActionHandlers,
                    Config = HostConfig ?? new AdaptiveHostConfig(),
                    ElementRenderers = ElementRenderers,
                    Lang = card.Lang,
                    RenderArgs = new AdaptiveRenderArgs { ForegroundColors = (HostConfig != null) ? HostConfig.ContainerStyles.Default.ForegroundColors : new ContainerStylesConfig().Default.ForegroundColors }
                };

                var stream = context.Render(card).RenderToImage(width);
                renderCard = new RenderedAdaptiveCardImage(stream, card, context.Warnings);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RENDER Failed. {e.Message}");
            }

            return renderCard;
        }


    }
}
