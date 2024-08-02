// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class ImageExtensions
    {


        /// <summary>
        /// Renders the element to a bitmap
        /// </summary>
        public static MemoryStream RenderToImage(this Control element, int width)
        {
            throw new NotImplementedException();
            //element.Measure(new Size(width, int.MaxValue));
            //// Add 100 to the height to give it some buffer. This addressed some bugs with maxlines getting clipped
            //element.Arrange(new Rect(new Size(width, element.DesiredSize.Height + 100)));
            //element.UpdateLayout();

            //var bitmapImage = new RenderTargetBitmap((int)width, (int)element.DesiredSize.Height, 96, 96, PixelFormats.Bgra8888);// PixelFormats.Default);
            //bitmapImage.Render(element);

            //var encoder = new PngBitmapEncoder();
            //var metadata = new BitmapMetadata("png");
            //// TODO: Should we set the image metadata?
            ////metadata.SetQuery("/tEXt/{str=Description}", JsonConvert.SerializeObject(OriginatingCard));
            //var pngFrame = BitmapFrame.Create(bitmapImage, null, metadata, null);
            //encoder.Frames.Add(pngFrame);

            //var stream = new MemoryStream();
            //encoder.Save(stream);
            //stream.Seek(0, SeekOrigin.Begin);
            //return stream;
        }

        public static async void SetImageSource(this Image image, Uri url, AdaptiveRenderContext context)
        {
            if (url == null)
                return;

            if (url.Scheme == "data")
            {
                var encodedData = url.AbsoluteUri.Substring(url.AbsoluteUri.LastIndexOf(',') + 1);
                var decodedDataUri = Convert.FromBase64String(encodedData);
                if (url.LocalPath.StartsWith("image/svg+xml;"))
                {
                    // ugh, not great, but it works
                    var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".svg");
                    File.WriteAllBytes(path, decodedDataUri);
                    image.SetValue(Image.SourceProperty, new SvgImage() { Source = SvgSource.Load(path, null) });
                    File.Delete(path);
                }
                else
                {
                    image.SetValue(Image.SourceProperty, new Bitmap(new MemoryStream(decodedDataUri)));
                }
            }
            else
            {
                if (url.AbsoluteUri.EndsWith(".svg"))
                {
                    image.SetValue(Image.SourceProperty, new SvgImage() { Source = SvgSource.Load(url.AbsoluteUri, null) });
                }
                else
                {
                    var bitmap = await ImageLoader.AsyncImageLoader.ProvideImageAsync(url.ToString());
                    image.Source = bitmap;
                    // image.SetValue(ImageLoader.SourceProperty, url.ToString());
                }
            }
        }

        public static async void SetBackgroundSource(this Grid grid, AdaptiveBackgroundImage adaptiveBackgroundImage, AdaptiveRenderContext context)
        {
            // Try to resolve the image URI
            Uri finalUri = context.Config.ResolveFinalAbsoluteUri(adaptiveBackgroundImage?.Url);
            if (finalUri == null)
            {
                return;
            }

            // var bi = await context.ResolveImageSource(finalUri);
            Bitmap bitmap = null;
            if (finalUri.Scheme == "data")
            {
                var encodedData = finalUri.AbsoluteUri.Substring(finalUri.AbsoluteUri.LastIndexOf(',') + 1);
                var decodedDataUri = Convert.FromBase64String(encodedData);
                bitmap = new Bitmap(new MemoryStream(decodedDataUri));
            }
            else
            {
                bitmap = await ImageLoader.AsyncImageLoader.ProvideImageAsync(finalUri.ToString());
            }


            if (bitmap != null)
            {
                switch (adaptiveBackgroundImage.FillMode)
                {
                    case AdaptiveImageFillMode.RepeatVertically:
                        grid.GetObservable(Grid.BoundsProperty).Subscribe((bounds) =>
                        {
                            if (grid.Background is ImageBrush brush)
                            {
                                if (bounds.Width > 0 && brush.DestinationRect.Rect.Width != bounds.Width)
                                    brush.DestinationRect = new RelativeRect(0, 0, bounds.Width, bitmap.Size.Height, RelativeUnit.Absolute);
                            }
                            else
                            {
                                grid.Background = new ImageBrush(bitmap)
                                {
                                    TileMode = TileMode.Tile,
                                    Stretch = Stretch.Uniform,
                                    AlignmentX = (AlignmentX)adaptiveBackgroundImage.HorizontalAlignment,
                                    AlignmentY = AlignmentY.Top,
                                    DestinationRect = new RelativeRect(0, 0, bounds.Width, bitmap.Size.Height, RelativeUnit.Absolute),
                                };
                            }
                        });
                        break;
                    case AdaptiveImageFillMode.RepeatHorizontally:
                        grid.GetObservable(Grid.BoundsProperty).Subscribe((bounds) =>
                        {
                            if (grid.Background is ImageBrush brush)
                            {
                                if (bounds.Height > 0 && brush.DestinationRect.Rect.Height != bounds.Height)
                                    brush.DestinationRect = new RelativeRect(0, 0, bitmap.Size.Width, bounds.Height, RelativeUnit.Absolute);
                            }
                            else
                            {
                                grid.Background = new ImageBrush(bitmap)
                                {
                                    TileMode = TileMode.Tile,
                                    Stretch = Stretch.Uniform,
                                    AlignmentX = AlignmentX.Left,
                                    AlignmentY = (AlignmentY)adaptiveBackgroundImage.VerticalAlignment,
                                    DestinationRect = new RelativeRect(0, 0, bitmap.Size.Width, bounds.Height, RelativeUnit.Absolute),
                                };
                            }
                        });
                        break;
                    case AdaptiveImageFillMode.Repeat:
                        grid.Background = new ImageBrush(bitmap)
                        {
                            TileMode = TileMode.Tile,
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Top,
                            Stretch = Stretch.None,
                            DestinationRect = new RelativeRect(0, 0, bitmap.Size.Width, bitmap.Size.Height, RelativeUnit.Absolute),
                        };
                        break;
                    case AdaptiveImageFillMode.Cover:
                    default:
                        grid.Background = new ImageBrush(bitmap)
                        {
                            Stretch = Stretch.UniformToFill,
                            AlignmentY = (AlignmentY)adaptiveBackgroundImage.VerticalAlignment,
                            AlignmentX = (AlignmentX)adaptiveBackgroundImage.HorizontalAlignment
                        };
                        break;
                }
            }
        }
    }
}