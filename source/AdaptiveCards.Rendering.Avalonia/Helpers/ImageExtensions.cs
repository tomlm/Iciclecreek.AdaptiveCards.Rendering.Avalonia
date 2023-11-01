// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
using SkiaSharp.Extended.Svg;
using System;
using System.Globalization;
using System.IO;

namespace AdaptiveCards.Rendering.Avalonia
{
    public static class ImageExtensions
    {

        public class AdaptiveConverterParameters
        {
            public AdaptiveConverterParameters(Image image, AdaptiveImage adaptiveImage, AdaptiveRenderContext context)
            {
                Image = image;
                AdaptiveImage = adaptiveImage;
                AdaptiveContext = context;
            }
            public Image Image { get; set; }
            public AdaptiveImage AdaptiveImage { get; set; }
            public AdaptiveRenderContext AdaptiveContext { get; set; }
        }
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

        public static async void SetSource(this Image image, Uri url, AdaptiveRenderContext context)
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
                    image.SetValue(ImageLoader.SourceProperty, url.ToString());
                }
            }
        }

        public static async void SetSource(this Image image, AdaptiveImage adaptiveImage, Uri url, AdaptiveRenderContext context)
        {
            image.SetSource(url, context);

            var parameters = new AdaptiveConverterParameters(image, adaptiveImage, context);
            image.Bind(Image.StretchProperty, new Binding($"Source")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Mode = BindingMode.OneWay,
                Converter = new StretchConverter(),
                ConverterParameter = parameters
            });
        }

        public class StretchConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Bitmap bitmap)
                {
                    var adaptiveParameters = (AdaptiveConverterParameters)parameter;
                    var image = adaptiveParameters.Image;
                    var adaptiveImage = adaptiveParameters.AdaptiveImage;
                    var imageWidth = bitmap.Size.Width; // ((BitmapImage)image.Source)?.PixelWidth;
                    var imageHeight = bitmap.Size.Height; // ((BitmapImage)image.Source)?.PixelHeight;

                    switch (adaptiveImage.Size)
                    {
                        case AdaptiveImageSize.Large:
                        case AdaptiveImageSize.Medium:
                        case AdaptiveImageSize.Small:
                            return Stretch.Uniform;
                        case AdaptiveImageSize.Stretch: //Image with both scale down and up to fit as needed.
                            return Stretch.Fill;

                        case AdaptiveImageSize.Auto: // Images will scale down to fit if needed, but will not scale up to fill the area.
                        default:
                            if (adaptiveImage.PixelWidth != 0 || adaptiveImage.PixelHeight != 0)
                            {
                                if (adaptiveImage.PixelWidth == 0)
                                {
                                    image.Width = (uint)((imageWidth / (float)imageHeight) * adaptiveImage.PixelHeight);
                                }

                                if (adaptiveImage.PixelHeight == 0)
                                {
                                    image.Height = (uint)((imageHeight / (float)imageWidth) * adaptiveImage.PixelWidth);
                                }

                                return Stretch.Uniform;
                            }
                            else
                            {
                                var panel = FindParentControlOfType<Panel>(image);
                                if (panel != null)
                                {
                                    if (panel.Bounds.Width > 0 || panel.Bounds.Height > 0)
                                    {
                                        if (imageWidth > panel.Bounds.Width || imageHeight > panel.Bounds.Height)
                                        {
                                            double widthScale = 0, heightScale = 0;
                                            if (imageWidth != 0)
                                                widthScale = (double)panel.Bounds.Width / (double)imageWidth;
                                            if (imageHeight != 0)
                                                heightScale = (double)panel.Bounds.Height / (double)imageHeight;
                                            double scale = Math.Max(widthScale, heightScale);
                                            image.Width = (int)(imageWidth * scale);
                                            image.Height = (int)(imageHeight * scale);
                                            return Stretch.Uniform;
                                        }
                                    }
                                }
                                else
                                {
                                    image.Width = imageWidth;
                                    image.Height = imageHeight;
                                }

                                return Stretch.None;
                            }
                            break;
                    }
                }
                return Stretch.None;
            }

            private static T FindParentControlOfType<T>(Control control) where T : Control
            {
                Control parent = control.Parent as Control;

                while (parent != null)
                {
                    if (parent is T)
                    {
                        return (T)parent;
                    }

                    parent = parent.Parent as Control;
                }

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
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
                bitmap = await context.ImageLoader.ProvideImageAsync(finalUri.ToString());
            }

            if (bitmap != null)
            {
                // bi.Pixel{Width, Height}: dimensions of image
                // grid.Actual{Width, Height}: dimensions of grid containing background image
                switch (adaptiveBackgroundImage.FillMode)
                {
                    case AdaptiveImageFillMode.Repeat:
                        grid.Background = new ImageBrush(bitmap)
                        {
                            TileMode = TileMode.Tile,
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Top,
                            DestinationRect = new RelativeRect(0, 0, bitmap.Size.Width, bitmap.Size.Height, RelativeUnit.Absolute),
                            //Viewport = new Rect(0, 0, bi.PixelWidth, bi.PixelHeight),
                            //ViewportUnits = BrushMappingMode.Absolute
                        };
                        break;
                    case AdaptiveImageFillMode.RepeatHorizontally:
                        grid.Background = new ImageBrush(bitmap)
                        {
                            TileMode = TileMode.FlipY,
                            Stretch = Stretch.Uniform,
                            AlignmentY = (AlignmentY)adaptiveBackgroundImage.VerticalAlignment,
                            DestinationRect = new RelativeRect(0, 0, bitmap.Size.Width, grid.Bounds.Height + 1, RelativeUnit.Absolute),
                            //Viewport = new Rect(0, 0, bi.PixelWidth, grid.ActualHeight + 1),
                            //ViewportUnits = BrushMappingMode.Absolute
                        };
                        break;
                    case AdaptiveImageFillMode.RepeatVertically:
                        grid.Background = new ImageBrush(bitmap)
                        {
                            TileMode = TileMode.FlipX,
                            Stretch = Stretch.Uniform,
                            AlignmentX = (AlignmentX)adaptiveBackgroundImage.HorizontalAlignment,
                            DestinationRect = new RelativeRect(0, 0, grid.Bounds.Width + 1, bitmap.Size.Height, RelativeUnit.Absolute),
                            //Viewport = new Rect(0, 0, grid.ActualWidth + 1, bi.PixelWidth),
                            //ViewportUnits = BrushMappingMode.Absolute
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

        public static void SetImageProperties(this Image imageview, AdaptiveImage image, AdaptiveRenderContext context)
        {
            switch (image.Size)
            {
                case AdaptiveImageSize.Auto:
                    imageview.Stretch = Stretch.Uniform;
                    break;
                case AdaptiveImageSize.Stretch:
                    imageview.Stretch = Stretch.Uniform;
                    break;
                case AdaptiveImageSize.Small:
                    imageview.Width = context.Config.ImageSizes.Small;
                    imageview.Height = context.Config.ImageSizes.Small;
                    break;
                case AdaptiveImageSize.Medium:
                    imageview.Width = context.Config.ImageSizes.Medium;
                    imageview.Height = context.Config.ImageSizes.Medium;
                    break;
                case AdaptiveImageSize.Large:
                    imageview.Width = context.Config.ImageSizes.Large;
                    imageview.Height = context.Config.ImageSizes.Large;
                    break;
            }
        }
    }
}
