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
using Foundation;
using Prism.Native;
using UIKit;

namespace Prism.iOS.UI.Media.Imaging
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeRenderTargetBitmap"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeRenderTargetBitmap))]
    public class RenderTargetBitmap : ImageSource, INativeRenderTargetBitmap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetBitmap"/> class.
        /// </summary>
        public RenderTargetBitmap()
        {
        }

        /// <summary>
        /// Gets the data for the captured image as a byte array.
        /// </summary>
        /// <returns>The image data as an <see cref="Array"/> of bytes.</returns>
        public Task<byte[]> GetPixelsAsync()
        {
            return Task.Run(() =>
            {
                if (Source == null)
                {
                    return new byte[0];
                }

                ConvertToARGB();
                return Source.CGImage.DataProvider.CopyData().ToArray();
            });
        }

        /// <summary>
        /// Renders a snapshot of the specified visual object.
        /// </summary>
        /// <param name="target">The visual object to render.    This value can be <c>null</c> to render the entire visual tree.</param>
        /// <param name="width">The width of the snapshot.</param>
        /// <param name="height">The height of the snapshot.</param>
        public Task RenderAsync(INativeVisual target, int width, int height)
        {
            var view = target as UIView ?? (target as UIViewController)?.View ??
                UIApplication.SharedApplication.KeyWindow.RootViewController.View;

            UIGraphics.BeginImageContextWithOptions(view.Frame.Size, false, 0);
            view.DrawViewHierarchy(view.Bounds, true);

            SetSource(UIGraphics.GetImageFromCurrentImageContext().Scale(new CGSize(width, height), 0), false);
            UIGraphics.EndImageContext();
            OnSourceChanged();

            return Task.CompletedTask;
        }
    }
}

