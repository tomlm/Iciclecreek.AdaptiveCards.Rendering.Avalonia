// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AdaptiveCards.Rendering.Avalonia.Video;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
//using LibVLCSharp.Avalonia;
//using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class AdaptiveMediaRenderer
    {
        private enum MediaState
        {
            NotStarted = 0,
            IsPlaying,
            IsPaused,
        }

        #region Appearance Config

        // Element height and margin
        // NOTE: Child height + child margin * 2 = panel height (50)
        private static readonly int _childHeight = 40;
        private static readonly int _childMargin = 5;
        private static readonly int _panelHeight = _childHeight + _childMargin * 2;
        private static readonly Thickness _marginThickness = new Thickness(_childMargin, _childMargin, _childMargin, _childMargin);

        // Use contrasting colors for foreground and background
        private static readonly SolidColorBrush _controlForegroundColor = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _controlBackgroundColor = new SolidColorBrush(Colors.Gray)
        {
            Opacity = 0.5,
        };

        private static readonly FontFamily _symbolFontFamily = new FontFamily("Segoe UI Symbol");

        #endregion

        public static Control Render(AdaptiveMedia media, AdaptiveRenderContext context)
        {
            // If host doesn't support interactivity or no media source is provided
            // just return the poster image if present
            if (!context.Config.SupportsInteractivity || media.Sources.Count == 0)
            {
                return GetPosterImage(media, context);
            }

            AdaptiveMediaSource mediaSource = GetMediaSource(media, context);
            if (mediaSource == null)
            {
                return null;
            }

            // Main element to return

            // Inline playback is possible only when inline playback is allowed by the host and the chosen media source is not https
            bool isInlinePlaybackPossible = context.Config.Media.AllowInlinePlayback;

            var uiMedia = new Grid();

            if (isInlinePlaybackPossible)
            {
                // Media player is only created if inline playback is allowed
                var uiMediaPlayer = new MediaPlayerControl()
                {
                    Source = mediaSource.Url,
                    Poster = media.Poster,
                };
                uiMediaPlayer.DataContext = uiMediaPlayer;

                uiMediaPlayer.IsVisible = true;

                uiMedia.Children.Add(uiMediaPlayer);
            }


            return uiMedia;
        }

        private static Control RenderThumbnailPlayButton(AdaptiveRenderContext context)
        {
            var playButtonSize = 100;

            // Wrap in a Viewbox to control width, height, and aspect ratio
            var uiPlayButton = new Viewbox()
            {
                Width = playButtonSize,
                Height = playButtonSize,
                Stretch = Stretch.Fill,
                Margin = _marginThickness,
            };

            MediaConfig mediaConfig = context.Config.Media;
            if (!string.IsNullOrEmpty(mediaConfig.PlayButton)
                 && context.Config.ResolveFinalAbsoluteUri(mediaConfig.PlayButton) != null)
            {
                // Try to use provided image from host config
                var content = new Image()
                {
                    Height = playButtonSize,
                };
                content.SetImageSource(mediaConfig.PlayButton, context);

                uiPlayButton.Child = content;
            }
            else
            {
                // Otherwise, use the default play symbol
                var content = new TextBlock()
                {
                    Text = "⏵",
                    FontFamily = _symbolFontFamily,
                    Foreground = _controlForegroundColor,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                uiPlayButton.Child = content;
            }

            return uiPlayButton;
        }


        /** Helper function to call async function from context */
        private static async void SetImageSource(this Image image, string urlString, AdaptiveRenderContext context)
        {
            // image.Source = await context.ResolveImageSource(context.Config.ResolveFinalAbsoluteUri(urlString));
            var imageUrl = context.Config.ResolveFinalAbsoluteUri(urlString);
            image.SetValue(ImageLoader.SourceProperty, imageUrl.ToString());
        }

        /** Get poster image from either card payload or host config */
        private static Image GetPosterImage(AdaptiveMedia media, AdaptiveRenderContext context)
        {
            Image uiPosterImage = null;
            if (!string.IsNullOrEmpty(media.Poster) && context.Config.ResolveFinalAbsoluteUri(media.Poster) != null)
            {
                // Use the provided poster
                uiPosterImage = new Image();
                uiPosterImage.SetImageSource(media.Poster, context);
            }
            else if (!string.IsNullOrEmpty(context.Config.Media.DefaultPoster)
                 && context.Config.ResolveFinalAbsoluteUri(context.Config.Media.DefaultPoster) != null)
            {
                // Use the default poster from host
                uiPosterImage = new Image();
                uiPosterImage.SetImageSource(context.Config.Media.DefaultPoster, context);
            }

            return uiPosterImage;
        }

        /** Simple template for playback buttons */
        private static Control RenderPlaybackButton(string text)
        {
            return new Viewbox()
            {
                Width = _childHeight,
                Height = _childHeight,
                Stretch = Stretch.Fill,
                Margin = _marginThickness,
                VerticalAlignment = VerticalAlignment.Center,
                IsVisible = false,
                Child = new TextBlock()
                {
                    Text = text,
                    FontFamily = _symbolFontFamily,
                    Foreground = _controlForegroundColor,
                }
            };
        }

        /** Handle visibility of playback buttons based on the current media state
         *  - If it's playing, user can only pause
         *  - If it's paused, user can only resume
         *  - If it's complete, user can only replay
         */
        private static void HandlePlaybackButtonVisibility(MediaState currentMediaState,
            Control pauseButton, Control resumeButton, Control replayButton)
        {
            pauseButton.IsVisible = false;
            resumeButton.IsVisible = false;
            replayButton.IsVisible = false;

            if (currentMediaState == MediaState.IsPlaying)
            {
                pauseButton.IsVisible = true;
            }
            else if (currentMediaState == MediaState.IsPaused)
            {
                resumeButton.IsVisible = true;
            }
            else
            {
                // Video is complete
                replayButton.IsVisible = true;
            }
        }

        private static List<string> _supportedMimeTypes = new List<string>
        {
            "video/mp4",
            "audio/mp4",
            "audio/mpeg"
        };

        private static List<string> _supportedAudioMimeTypes = new List<string>
        {
            "audio/mp4",
            "audio/mpeg"
        };

        /** Get the first media URI with a supported mime type */
        private static AdaptiveMediaSource GetMediaSource(AdaptiveMedia media, AdaptiveRenderContext context)
        {
            // Check if sources contain an invalid mix of MIME types (audio and video)
            bool? isLastMediaSourceAudio = null;
            foreach (var source in media.Sources)
            {
                if (!isLastMediaSourceAudio.HasValue)
                {
                    isLastMediaSourceAudio = IsAudio(source);
                }
                else
                {
                    if (IsAudio(source) != isLastMediaSourceAudio.Value)
                    {
                        // If there is one pair of sources with different MIME types,
                        // it's an invalid mix and a warning should be logged
                        context.Warnings.Add(new AdaptiveWarning(-1, "A Media element contains an invalid mix of MIME type"));
                        return null;
                    }

                    isLastMediaSourceAudio = IsAudio(source);
                }
            }

            // Return the first supported source with not-null URI
            bool isAllHttps = true;
            AdaptiveMediaSource httpsSource = null;
            foreach (var source in media.Sources)
            {
                if (_supportedMimeTypes.Contains(source.MimeType))
                {
                    Uri finalMediaUri = context.Config.ResolveFinalAbsoluteUri(source.Url);
                    if (finalMediaUri != null)
                    {
                        // Since https is not supported by WPF,
                        // try to use non-https sources first
                        if (finalMediaUri.Scheme != "https")
                        {
                            isAllHttps = false;
                            return source;
                        }
                        else if (httpsSource == null)
                        {
                            httpsSource = source;
                        }
                    }
                }
            }

            // If all sources are https, log a warning and return the first one
            if (isAllHttps)
            {
                context.Warnings.Add(new AdaptiveWarning(-1, "All sources have unsupported https scheme. The host would be responsible for playing the media."));
                return httpsSource;
            }

            // No valid source is found
            context.Warnings.Add(new AdaptiveWarning(-1, "A Media element does not have any valid source"));
            return null;
        }

        private static bool IsAudio(AdaptiveMediaSource mediaSource)
        {
            return _supportedAudioMimeTypes.Contains(mediaSource.MimeType);
        }
    }
}
