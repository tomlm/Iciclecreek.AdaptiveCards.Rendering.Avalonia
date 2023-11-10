using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
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
        }

        public string Source
        {
            get => _source;
            set
            {
                this.SetAndRaise(SourceProperty, ref _source, value);
                this.VideoView.Source = value;
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
