﻿/*
Copyright (C) 2016  Prism Framework Team

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
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Prism.Native;
using Prism.UI.Media.Imaging;
using UIKit;

namespace Prism.iOS.UI.Media.Imaging
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeBitmapImage"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeBitmapImage))]
    public class BitmapImage : INativeBitmapImage, IImageSource, ILazyLoader
    {
        /// <summary>
        /// Occurs when the image fails to load.
        /// </summary>
        public event EventHandler<ErrorEventArgs> ImageFailed;

        /// <summary>
        /// Occurs when the image has been loaded into memory.
        /// </summary>
        public event EventHandler ImageLoaded;

        /// <summary>
        /// Gets a value indicating whether the image has encountered an error during loading.
        /// </summary>
        public bool IsFaulted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the image has been loaded into memory.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets the number of pixels along the image's Y-axis.
        /// </summary>
        public int PixelHeight
        {
            get { return Source == null ? 0 : (int)(Source.Size.Height * Source.CurrentScale); }
        }

        /// <summary>
        /// Gets the number of pixels along the image's X-axis.
        /// </summary>
        public int PixelWidth
        {
            get { return Source == null ? 0 : (int)(Source.Size.Width * Source.CurrentScale); }
        }
        
        /// <summary>
        /// Gets the scaling factor of the image.
        /// </summary>
        public double Scale
        {
            get { return Source == null ? 1 : Source.CurrentScale; }
        }
        
        /// <summary>
        /// Gets the image source instance.
        /// </summary>
        public UIImage Source { get; private set; }

        /// <summary>
        /// Gets the URI of the source file containing the image data.
        /// </summary>
        public Uri SourceUri { get; private set; }

        private byte[] imageBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImage"/> class.
        /// </summary>
        /// <param name="sourceUri">The URI of the source file containing the image data.</param>
        /// <param name="cachedImage">The image that was pulled from the image cache, or <c>null</c> if nothing was pulled from the cache.</param>
        public BitmapImage(Uri sourceUri, INativeImageSource cachedImage)
        {
            SourceUri = sourceUri;

            var cached = cachedImage as BitmapImage;
            if (cached != null)
            {
                Source = cached.Source;
                IsLoaded = cached.IsLoaded;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImage"/> class.
        /// </summary>
        /// <param name="imageData">The byte array containing the data for the image.</param>
        public BitmapImage(byte[] imageData)
        {
            imageBytes = imageData;
        }

        /// <summary>
        /// Loads the contents of the object in a background thread.
        /// </summary>
        public void LoadInBackground()
        {
            var context = SynchronizationContext.Current ?? new SynchronizationContext();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                lock (this)
                {
                    if (Source == null && !IsFaulted && (imageBytes != null || SourceUri != null))
                    {
                        try
                        {
                            if (imageBytes != null)
                            {
                                Source = UIImage.LoadFromData(NSData.FromArray(imageBytes));
                                imageBytes = null;
                            }
                            else if (!SourceUri.IsAbsoluteUri || SourceUri.IsFile)
                            {
                                Source = UIImage.FromFile(SourceUri.OriginalString);
                            }
                            else
                            {
                                Source = UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(SourceUri.OriginalString)));
                            }
                        }
                        catch (Exception e)
                        {
                            context.Post((obj) => OnImageFailed(e), null);
                            return;
                        }

                        if (Source == null)
                        {
                            context.Post((obj) => OnImageFailed(null), null);
                            return;
                        }
                    }

                    if (!IsLoaded)
                    {
                        context.Post((obj) => OnImageLoaded(), null);
                    }
                }
            }, this);
        }

        /// <summary>
        /// Saves the image data to a file at the specified path using the specified file format.
        /// </summary>
        /// <param name="filePath">The path to the file in which to save the image data.</param>
        /// <param name="fileFormat">The file format to with which to save the image data.</param>
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
        /// Raises the image failed event.
        /// </summary>
        /// <param name="e">The exception that describes the failure.</param>
        protected void OnImageFailed(Exception e)
        {
            IsFaulted = true;
            ImageFailed(this, new ErrorEventArgs(e));
        }

        /// <summary>
        /// Raises the image loaded event.
        /// </summary>
        protected void OnImageLoaded()
        {
            IsLoaded = true;
            ImageLoaded(this, EventArgs.Empty);
        }
    }
}

