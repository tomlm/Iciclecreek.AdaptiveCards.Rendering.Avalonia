using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using LibVLCSharp.Shared;
using System.Diagnostics;

namespace AdaptiveCards.Rendering.Avalonia.Video
{
    public partial class MediaPlayerControls : UserControl
    {
        public MediaPlayerControls()
        {
            InitializeComponent();
        }

        public MediaPlayerControl MediaPlayerControl => this.DataContext as MediaPlayerControl;

        private void StartPlay(object? sender, RoutedEventArgs e)
        {
            MediaPlayerControl.Play();
        }

        private void StopPlay(object? sender, RoutedEventArgs e)
        {
            MediaPlayerControl.Stop();
        }

        public void PointerEntered(object? sender, PointerEventArgs e)
        {
            this.Opacity = 0.8;
        }

        public void PointerExited(object? sender, PointerEventArgs e)
        {
            this.Opacity = 0.0;
        }
    }
}
