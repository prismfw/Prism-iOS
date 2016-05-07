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
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Provides methods for traversing the iOS view hierarchy.
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Walks the view hierarchy and returns the subview that satisfies the specified condition.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="predicate">The condition to check each subview for.</param>
        public static UIView GetSubview(this UIView view, Func<UIView, bool> predicate = null)
        {
            if (view != null)
            {
                for (int i = 0; i < view.Subviews.Length; i++)
                {
                    var subview = view.Subviews[i];
                    if (predicate == null || predicate.Invoke(subview))
                    {
                        return subview;
                    }

                    subview = subview.GetSubview(predicate);
                    if (subview != null)
                    {
                        return subview;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the view hierarchy and returns the subview that is of type T and satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of subview to search for.</typeparam>
        /// <param name="view">The view.</param>
        /// <param name="predicate">The condition to check each subview for.</param>
        public static T GetSubview<T>(this UIView view, Func<T, bool> predicate = null)
            where T : UIView
        {
            if (view != null)
            {
                for (int i = 0; i < view.Subviews.Length; i++)
                {
                    var subview = view.Subviews[i];
                    var tView = subview as T;
                    if (tView != null && (predicate == null || predicate.Invoke(tView)))
                    {
                        return tView;
                    }

                    tView = subview.GetSubview<T>(predicate);
                    if (tView != null)
                    {
                        return tView;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the view hierarchy and returns the parent that satisfies the specified condition.
        /// </summary>
        /// <param name="responder">The responder.</param>
        /// <param name="predicate">The condition to check each parent for.</param>
        public static UIResponder GetNextResponder(this UIResponder responder, Func<UIResponder, bool> predicate = null)
        {
            if (responder != null)
            {
                var next = responder.NextResponder;
                if (next != null && (predicate == null || predicate.Invoke(next)))
                {
                    return next;
                }

                next = next.GetNextResponder(predicate);
                if (next != null)
                {
                    return next;
                }

                var controller = responder as UIViewController;
                if (controller != null)
                {
                    next = controller.NavigationController;
                    if (next != null && (predicate == null || predicate.Invoke(next)))
                    {
                        return next;
                    }

                    next = controller.NavigationController.GetNextResponder(predicate);
                    if (next != null)
                    {
                        return next;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the view hierarchy and returns the parent that is of type T and satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of parent to search for.</typeparam>
        /// <param name="responder">The responder.</param>
        /// <param name="predicate">The condition to check each parent for.</param>
        public static T GetNextResponder<T>(this UIResponder responder, Func<T, bool> predicate = null)
            where T : class
        {
            if (responder != null)
            {
                var next = responder.NextResponder;
                var tNext = next as T;
                if (tNext != null && (predicate == null || predicate.Invoke(tNext)))
                {
                    return tNext;
                }

                tNext = next.GetNextResponder<T>(predicate);
                if (tNext != null)
                {
                    return tNext;
                }

                var controller = responder as UIViewController;
                if (controller != null && controller.NavigationController != null)
                {
                    tNext = controller.NavigationController as T;
                    if (tNext != null && (predicate == null || predicate.Invoke(tNext)))
                    {
                        return tNext;
                    }

                    tNext = controller.NavigationController.GetNextResponder<T>(predicate);
                    if (tNext != null)
                    {
                        return tNext;
                    }
                }
            }

            return null;
        }
    }
}

