/*
Copyright (C) 2017  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Prism.Native;
using UIKit;

namespace Prism.iOS.UI.Media.Imaging
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeImageCompositor"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeImageCompositor), IsSingleton = true)]
    public class ImageCompositor : INativeImageCompositor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCompositor"/> class.
        /// </summary>
        public ImageCompositor()
        {
        }

        /// <summary>
        /// Composites the provided images into one image with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the composited image.</param>
        /// <param name="height">The height of the composited image.</param>
        /// <param name="images">The images that are to be composited. The first image will be drawn first and each subsequent image will be drawn on top.</param>
        public Task<Prism.UI.Media.Imaging.ImageSource> CompositeAsync(int width, int height, params INativeImageSource[] images)
        {
            return Task.Run<Prism.UI.Media.Imaging.ImageSource>(() =>
            {
                if (images.Length == 0)
                {
                    return new Prism.UI.Media.Imaging.BitmapImage(new byte[0]);
                }

                Task.WaitAll(images.OfType<INativeBitmapImage>().Select(img => Task.Run(() =>
                {
                    if (!img.IsLoaded)
                    {
                        img.BeginLoadingImage(null);
                        while (!img.IsLoaded && !img.IsFaulted) { new System.Threading.ManualResetEvent(false).WaitOne(10); }
                    }
                })).ToArray());

                var uiimages = images.Select(img => img.GetImageSource()).Where(img => img != null);

                var firstImage = uiimages.FirstOrDefault()?.CGImage;
                if (firstImage == null)
                {
                    return new Prism.UI.Media.Imaging.BitmapImage(new byte[0]);
                }

                using (var context = new CGBitmapContext(IntPtr.Zero, width, height, firstImage.BitsPerComponent,
                    firstImage.BytesPerRow, CGColorSpace.CreateDeviceRGB(), CGBitmapFlags.PremultipliedLast))
                {
                    foreach (var image in uiimages)
                    {
                        context.DrawImage(new CGRect(0, 0, width, height), image.CGImage);
                    }

                    return new Prism.UI.Media.Imaging.BitmapImage(new UIImage(context.ToImage()).AsPNG().ToArray());
                }
            });
        }
    }
}

