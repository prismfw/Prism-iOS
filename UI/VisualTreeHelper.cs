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
using System.Linq;
using Foundation;
using Prism.Native;
using Prism.UI;
using UIKit;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeVisualTreeHelper"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeVisualTreeHelper), IsSingleton = true)]
    public class VisualTreeHelper : INativeVisualTreeHelper
    {
        /// <summary>
        /// Returns the number of children in the specified object's child collection.
        /// </summary>
        /// <param name="reference">The parent object.</param>
        /// <returns>The number of children in the parent object's child collection.</returns>
        public int GetChildrenCount(object reference)
        {
            var window = reference as INativeWindow;
            if (window != null)
            {
                return window.Content == null ? 0 : 1;
            }

            var view = reference as UIView;
            if (view == null)
            {
                var controller = reference as UIViewController;
                if (controller != null)
                {
                    view = controller.View;
                }
            }

            return view == null ? 0 : view.Subviews.Length;
        }

        /// <summary>
        /// Returns the child that is located at the specified index in the child collection of the specified object.
        /// </summary>
        /// <param name="reference">The parent object.</param>
        /// <param name="childIndex">The zero-based index of the child to return.</param>
        /// <returns>The child at the specified index.</returns>
        public object GetChild(object reference, int childIndex)
        {
            var window = reference as INativeWindow;
            if (window != null && childIndex == 0)
            {
                return window.Content;
            }

            var view = reference as UIView;
            if (view == null)
            {
                var controller = reference as UIViewController;
                if (controller != null)
                {
                    if (controller.ChildViewControllers.Length > 0)
                    {
                        return controller.ChildViewControllers.ElementAtOrDefault(childIndex);
                    }

                    view = controller.View;
                }
            }

            return view == null ? null : view.Subviews.ElementAtOrDefault(childIndex);
        }

        /// <summary>
        /// Returns the parent of the specified object.
        /// </summary>
        /// <param name="reference">The child object.</param>
        /// <returns>The parent.</returns>
        public object GetParent(object reference)
        {
            if (reference is INativePopup)
            {
                // Popups should not have visual parents.  This keeps things consistent across platforms.
                return null;
            }
        
            var window = ObjectRetriever.GetNativeObject(Prism.UI.Window.MainWindow) as INativeWindow;
            if (window != null && window.Content == reference)
            {
                return window;
            }

            var controller = reference as UIViewController;
            if (controller != null)
            {
                return controller.ParentViewController ?? controller.NavigationController ??
                    controller.SplitViewController ?? controller.TabBarController ?? controller.NextResponder;
            }

            var view = reference as UIResponder;
            return view == null ? null : view.NextResponder;
        }
    }
}

