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


#pragma warning disable 1998

using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;
using Prism.Utilities;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation for a modal <see cref="INativeWindow"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWindow), Name = "modal")]
    public class ModalWindow : UIViewController, INativeWindow
    {
        /// <summary>
        /// Occurs when the window gains focus.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// Occurs when the window is about to be closed.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing;

        /// <summary>
        /// Occurs when the window loses focus.
        /// </summary>
        public event EventHandler Deactivated;

        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

        /// <summary>
        /// Gets or sets the object that acts as the content of the window.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                var oldRoot = ChildViewControllers.FirstOrDefault(vc => vc == content);
                if (oldRoot != null)
                {
                    oldRoot.WillMoveToParentViewController(null);
                    oldRoot.View.RemoveFromSuperview();
                    oldRoot.RemoveFromParentViewController();
                }

                content = value;
                var controller = value as UIViewController;
                AddChildViewController(controller);
                View.AddSubview(controller.View);
                controller.DidMoveToParentViewController(this);
            }
        }
        private object content;

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        public double Height
        {
            get { return View.Frame.Height; }
            set { Logger.Warn("Setting window height is not supported on this platform.  Ignoring."); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get { return PresentingViewController != null; }
        }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public double Width
        {
            get { return View.Frame.Width; }
            set { Logger.Warn("Setting window width is not supported on this platform.  Ignoring."); }
        }

        private Size currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModalWindow"/> class.
        /// </summary>
        public ModalWindow() { }

        /// <summary>
        /// Attempts to close the window.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void Close(Animate animate)
        {
            if (PresentingViewController == null)
            {
                return;
            }

            var args = new CancelEventArgs();
            Closing(this, args);

            if (args.Cancel)
            {
                return;
            }

            PresentingViewController.DismissViewController(animate != Animate.Off, () =>
            {
                Deactivated(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Displays the window if it is not already visible.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void Show(Animate animate)
        {
            if (PresentingViewController != null)
            {
                return;
            }

            var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (topController.PresentedViewController != null)
            {
                topController = topController.PresentedViewController;
            }

            topController.PresentViewController(this, animate != Animate.Off, () =>
            {
                Activated(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Captures the contents of the window in an image and returns the result.
        /// </summary>
        public async Task<Prism.UI.Media.Imaging.ImageSource> TakeScreenshotAsync()
        {
            UIImage image;
            UIGraphics.BeginImageContext(View.Frame.Size);
            View .DrawViewHierarchy(View.Frame, true);
            image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return new Prism.UI.Media.Imaging.ImageSource(image.AsPNG().ToArray());
        }

        /// <summary></summary>
        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            var newSize = View.Frame.Size.GetSize();
            if (currentSize != newSize)
            {
                SizeChanged(this, new WindowSizeChangedEventArgs(currentSize, newSize));
                currentSize = newSize;
            }
        }
    }
}

