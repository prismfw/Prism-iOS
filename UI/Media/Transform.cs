/*
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
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Prism.Native;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Media
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTransform"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTransform))]
    public class Transform : INativeTransform
    {
        /// <summary>
        /// Gets or sets the affine transformation matrix.
        /// </summary>
        public Matrix Value
        {
            get { return matrix; }
            set
            {
                matrix = value;

                var transform = matrix.GetCGAffineTransform();
                for (int i = 0; i < views.Count; i++)
                {
                    var view = views[i].Target as UIView;
                    if (view == null)
                    {
                        views.RemoveAt(i--);
                    }
                    else
                    {
                        view.Layer.AffineTransform = transform;
                        view.SetNeedsDisplay();
                    }
                }
            }
        }
        private Matrix matrix;

        private readonly List<WeakReference> views = new List<WeakReference>();

        /// <summary>
        /// Adds a view to the transformation listener.
        /// </summary>
        /// <param name="view">The view to be added.</param>
        public void AddView(UIView view)
        {
            views.Add(new WeakReference(view));
            view.Layer.AffineTransform = matrix.GetCGAffineTransform();
            view.SetNeedsDisplay();
        }

        /// <summary>
        /// Removes a view from the transformation listener.
        /// </summary>
        /// <param name="view">The view to be removed.</param>
        public void RemoveView(UIView view)
        {
            views.RemoveAll(r => r.Target == view);
            view.Layer.AffineTransform = CGAffineTransform.MakeIdentity();
            view.SetNeedsDisplay();
        }
    }
}

