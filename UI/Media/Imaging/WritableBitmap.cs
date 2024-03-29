﻿/*
Copyright (C) 2018  Prism Framework Team

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
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Prism.Native;
using UIKit;

namespace Prism.iOS.UI.Media.Imaging
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeWritableBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWritableBitmap))]
    public sealed class WritableBitmap : ImageSource, INativeWritableBitmap
    {
        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public override int PixelHeight { get; }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public override int PixelWidth { get; }
        
        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public override double Scale
        {
            get { return 1; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WritableBitmap"/> class.
        /// </summary>
        /// <param name="pixelWidth">The number of pixels along the image's X-axis.</param>
        /// <param name="pixelHeight">The number of pixels along the image's Y-axis.</param>
        public WritableBitmap(int pixelWidth, int pixelHeight)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }

        /// <summary>
        /// Gets the data of the bitmap as a byte array.
        /// </summary>
        /// <returns>The image data as an <see cref="Array"/> of bytes.</returns>
        public Task<byte[]> GetPixelsAsync()
        {
            return Task.Run(() => Source?.CGImage.DataProvider.CopyData().ToArray() ?? new byte[0]);
        }

        /// <summary>
        /// Sets the pixel data of the bitmap to the specified byte array.
        /// </summary>
        /// <param name="pixelData">The byte array containing the pixel data.</param>
        public async Task SetPixelsAsync(byte[] pixelData)
        {
            var oldSource = Source;
            await Task.Run(() =>
            {
                using (var colorSpace = CGColorSpace.CreateGenericRgb())
                {
                    using (var context = new CGBitmapContext(pixelData, PixelWidth, PixelHeight, 8, PixelWidth * 4,
                        colorSpace, CGBitmapFlags.ByteOrderDefault | CGBitmapFlags.PremultipliedFirst))
                    {
                        SetSource(UIImage.FromImage(context.ToImage(), 1, UIImageOrientation.Up), false);
                    }
                }
            });

            OnSourceChanged();
            oldSource?.Dispose();
        }
    }
}

