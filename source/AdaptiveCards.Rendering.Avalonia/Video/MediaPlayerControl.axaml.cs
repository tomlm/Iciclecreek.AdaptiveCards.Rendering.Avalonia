using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using System;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Image = Avalonia.Controls.Image;

namespace AdaptiveCards.Rendering.Avalonia.Video
{
    public partial class MediaPlayerControl : UserControl
    {
        public static readonly DirectProperty<MediaPlayerControl, string> SourceProperty =
            AvaloniaProperty.RegisterDirect<MediaPlayerControl, string>(nameof(Source), o => o.Source, (o, v) => o.Source = v);

        public static readonly DirectProperty<MediaPlayerControl, string> PosterProperty =
            AvaloniaProperty.RegisterDirect<MediaPlayerControl, string>(nameof(Source), o => o.Source, (o, v) => o.Source = v);

        private string _source;
        private string _poster;

        public MediaPlayerControl()
        {
            InitializeComponent();
            this.VideoView.MediaPlayer.EnableHardwareDecoding = true;
            this.VideoView.MediaPlayer.MediaChanged += MediaPlayer_MediaChanged;
        }

        private void MediaPlayer_MediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e)
        {
            e.Media.ParsedChanged += Media_ParsedChanged;
        }

        private void Media_ParsedChanged(object? sender, MediaParsedChangedEventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
                {
                    // resize viewer to match layout with aspect ratio 
                    // we assume width is correct and calculate height based on aspect ratio
                    var videoTrack = this.VideoView.MediaPlayer.Media.Tracks.Where(t => t.TrackType == TrackType.Video).FirstOrDefault().Data.Video;
                    var ratio = (double)videoTrack.Height / (double)videoTrack.Width;
                    var parent = this.Parent as Grid;
                    this.Height = parent.Bounds.Width * ratio;
                });

        }

        public string Source
        {
            get => _source;
            set
            {
                this.SetAndRaise(SourceProperty, ref _source, value);
            }
        }

        public string Poster
        {
            get => _poster;
            set
            {
                this.SetAndRaise(PosterProperty, ref _poster, value);
                // PosterImage.SetValue(ImageLoader.SourceProperty, _poster);
            }
        }

        public void Play()
        {
            this.VideoView.MediaPlayer.Play();
        }

        public void Stop()
        {
            this.VideoView.MediaPlayer.Stop();
        }

    }
}
