/*
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
using Prism.UI.Media.Imaging;
using UIKit;

namespace Prism.iOS.UI.Media.Imaging
{
    /// <summary>
    /// Represents the base class for image sources.  This class is abstract.
    /// </summary>
    public abstract class ImageSource
    {
        /// <summary>
        /// Occurs when the underlying image data has changed.
        /// </summary>
        public event EventHandler SourceChanged;

        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public virtual int PixelHeight
        {
            get { return Source == null ? 0 : (int)(Source.Size.Height * Source.CurrentScale); }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public virtual int PixelWidth
        {
            get { return Source == null ? 0 : (int)(Source.Size.Width * Source.CurrentScale); }
        }

        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public virtual double Scale
        {
            get { return Source == null ? 1 : Source.CurrentScale; }
        }

        /// <summary>
        /// Gets the image source instance.
        /// </summary>
        public UIImage Source { get; private set; }

        /// <summary>
        /// Saves the image data to a file at the specified path using the specified file format.
        /// </summary>
        /// <param name="filePath">The path to the file in which to save the image data.</param>
        /// <param name="fileFormat">The file format in which to save the image data.</param>
        public Task SaveAsync(string filePath, ImageFileFormat fileFormat)
        {
            return Task.Run(() =>
            {
                if (fileFormat == ImageFileFormat.Jpeg)
                {
                    Source?.AsJPEG().Save(filePath, true);
                }
                else
                {
                    Source?.AsPNG().Save(filePath, true);
                }
            });
        }

        /// <summary>
        /// Converts the image source to ARGB if it isn't already.
        /// </summary>
        protected void ConvertToARGB()
        {
            if (Source.CGImage.ColorSpace.Name != CGColorSpaceNames.GenericRgb ||
                Source.CGImage.BitmapInfo.HasFlag(CGBitmapFlags.PremultipliedLast))
            {
                var pixels = new byte[PixelWidth * PixelHeight * 4];
                using (var colorSpace = CGColorSpace.CreateGenericRgb())
                {
                    using (var context = new CGBitmapContext(pixels, PixelWidth, PixelHeight, 8, PixelWidth * 4,
                        colorSpace, CGBitmapFlags.ByteOrderDefault | CGBitmapFlags.PremultipliedFirst))
                    {
                        context.DrawImage(new CGRect(0, 0, PixelWidth, PixelHeight), Source.CGImage);
                        Source = UIImage.FromImage(context.ToImage());
                    }
                }
            }
        }

        /// <summary>
        /// Called when the image source changes significantly.
        /// </summary>
        protected void OnSourceChanged()
        {
            SourceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the image source.
        /// </summary>
        /// <param name="source">The new image source.</param>
        /// <param name="notify">A value indicating whether to trigger the SourceChanged event.</param>
        protected void SetSource(UIImage source, bool notify)
        {
            if (Source != source)
            {
                Source = source;
                if (notify)
                {
                    OnSourceChanged();
                }
            }
        }
    }
}

