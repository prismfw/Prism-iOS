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
using CoreGraphics;
using Prism.UI;
using UIKit;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents a <see cref="UIWindow"/> that monitors itself for size changes.
    /// </summary>
    public class Window : UIWindow
    {
        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

        private Size currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="frame"></param>
        public Window(CGRect frame)
            : base(frame)
        {
            currentSize = frame.Size.GetSize();
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var newSize = Frame.Size.GetSize();
            if (currentSize != newSize)
            {
                SizeChanged?.Invoke(this, new WindowSizeChangedEventArgs(currentSize, newSize));
                currentSize = newSize;
            }
        }
    }
}

