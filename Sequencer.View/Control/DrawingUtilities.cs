using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Point = System.Windows.Point;

namespace Sequencer.View.Control
{
    public static class DrawingUtilities
    {
        public static BitmapImage ToBitmapImage([NotNull] this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static void SetCentreOnCanvas([NotNull] this FrameworkElement frameworkElement, Point startingPoint)
        {
            Canvas.SetLeft(frameworkElement, startingPoint.X - (frameworkElement.Width/2));
            Canvas.SetTop(frameworkElement, startingPoint.Y - (frameworkElement.Height/2));
        }

        /// <summary>
        /// Create a fade out animation.
        /// </summary>
        /// <param name="fadeOutDuration">How long the fade out animation should last, in milliseconds.</param>
        /// <returns>A fade out animation.</returns>
        [NotNull]
        [Pure]
        public static DoubleAnimation CreateFadeOutAnimation(int fadeOutDuration)
        {
            return new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromMilliseconds(fadeOutDuration),
                FillBehavior = FillBehavior.Stop
            };
        }
    }
}