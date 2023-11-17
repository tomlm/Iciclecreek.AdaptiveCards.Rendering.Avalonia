// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AsyncImageLoader;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AdaptiveCards.Rendering.Avalonia
{
    public class MyCachedImageLoader: BaseWebImageLoader
    {
        private readonly ConcurrentDictionary<string, Task<Bitmap?>> _memoryCache = new();

        /// <inheritdoc />
        public MyCachedImageLoader() { }

        /// <inheritdoc />
        public MyCachedImageLoader(HttpClient httpClient, bool disposeHttpClient) : base(httpClient,
            disposeHttpClient)
        { }

        /// <inheritdoc />
        public override async Task<Bitmap?> ProvideImageAsync(string url)
        {
            var bitmap = await _memoryCache.GetOrAdd(url, LoadAsync).ConfigureAwait(false);
            // If load failed - remove from cache and return
            // Next load attempt will try to load image again
            if (bitmap == null) _memoryCache.TryRemove(url, out _);
            return bitmap;
        }
        
        protected override async Task<Bitmap> LoadAsync(string url)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                byte[] array = await LoadDataFromExternalAsync(url).ConfigureAwait(continueOnCapturedContext: false);
                if (array == null)
                {
                    return null;
                }

                using MemoryStream memoryStream = new MemoryStream(array);
                Bitmap bitmap = new Bitmap(memoryStream);
                await SaveToGlobalCache(url, array).ConfigureAwait(continueOnCapturedContext: false);
                
                sw.Stop();
                if (Debugger.IsAttached)
                    Debug.WriteLine($"Image {url} loaded in {sw.ElapsedMilliseconds} ms");
                return bitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public class AdaptiveCardRenderer : AdaptiveCardRendererBase<Control, AdaptiveRenderContext>
    {
        protected override AdaptiveSchemaVersion GetSupportedSchemaVersion()
        {
            return new AdaptiveSchemaVersion(1, 5);
        }

        protected Action<object, AdaptiveActionEventArgs> ActionCallback;
        protected Action<object, MissingInputEventArgs> missingDataCallback;

        static AdaptiveCardRenderer()
        {
            ImageLoader.AsyncImageLoader = new MyCachedImageLoader();
        }

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

            ElementRenderers.Set<AdaptiveTable>(AdaptiveTableRenderer.Render);
            ElementRenderers.Set<AdaptiveTableCell>(AdaptiveTableCellRenderer.Render);

            ActionHandlers.AddSupportedAction<AdaptiveOverflowAction>();
        }

        public AdaptiveFeatureRegistration FeatureRegistration { get; } = new AdaptiveFeatureRegistration();

        public AdaptiveActionHandlers ActionHandlers { get; } = new AdaptiveActionHandlers();

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

            // Missing schema
            var cardRtl = ((IDictionary<string, object>)card.AdditionalProperties).TryGetValue<bool?>("rtl"); //  previousContextRtl;
            bool updatedRtl = false;

            if (cardRtl.HasValue && cardRtl.Value == true)
            {
                context.Rtl = true;
                updatedRtl = true;
            }

            if (cardRtl.HasValue)
            {
                outerGrid.FlowDirection = cardRtl.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }

            // Reset the parent style
            context.RenderArgs.ParentStyle = AdaptiveContainerStyle.Default;

            var grid = new Grid();
            // grid.Style = context.GetStyle("Adaptive.InnerCard");
            grid.Margin = new Thickness(context.Config.Spacing.Padding);

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            RendererUtil.ApplyVerticalContentAlignment(grid, card.VerticalContentAlignment);

            outerGrid.MinHeight = card.PixelMinHeight;

            outerGrid.Children.Add(grid);

            AdaptiveInternalID parentCardId = context.RenderArgs.ContainerCardId;
            if (card.InternalID == null)
                card.InternalID = AdaptiveInternalID.Next();
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
    }
}
