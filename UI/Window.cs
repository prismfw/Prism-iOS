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
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;
using Prism.Utilities;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeWindow"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWindow))]
    public class Window : NSObject, INativeWindow
    {
        /// <summary>
        /// Occurs when the window gains focus.
        /// </summary>
        public event EventHandler Activated;

        /// <summary>
        /// Does not fire on iOS.
        /// </summary>
        #pragma warning disable 67
        [Preserve]
        public event EventHandler<CancelEventArgs> Closing;
        #pragma warning restore 67

        /// <summary>
        /// Occurs when the window loses focus.
        /// </summary>
        public event EventHandler Deactivated;

        /// <summary>
        /// Occurs when the orientation of the rendered content has changed.
        /// </summary>
        public event EventHandler<DisplayOrientationChangedEventArgs> OrientationChanged;

        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

        /// <summary>
        /// Gets or sets the preferred orientations in which to automatically rotate the window in response to orientation changes of the physical device.
        /// </summary>
        public DisplayOrientations AutorotationPreferences { get; set; }

        /// <summary>
        /// Gets or sets the object that acts as the content of the window.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return UIApplication.SharedApplication.KeyWindow.RootViewController; }
            set
            {
                UIApplication.SharedApplication.KeyWindow.RootViewController = value as UIViewController;
                
                var indicator = UIApplication.SharedApplication.KeyWindow.Subviews.FirstOrDefault(s => s is INativeLoadIndicator);
                if (indicator != null)
                {
                    UIApplication.SharedApplication.KeyWindow.BringSubviewToFront(indicator);
                }
            }
        }

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        public double Height
        {
            get { return UIApplication.SharedApplication.KeyWindow.Frame.Height; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get { return !UIApplication.SharedApplication.KeyWindow.Hidden; }
        }

        /// <summary>
        /// Gets or sets the internal UIWindow instance acting as the key window of the application.
        /// </summary>
        public UIWindow Instance
        {
            get { return instance; }
            set
            {
                if (value != instance)
                {
                    var window = instance as CoreWindow;
                    if (window != null)
                    {
                        window.OrientationChanged -= OnOrientationChanged;
                        window.SizeChanged -= OnSizeChanged;
                    }

                    instance = value;
                    instance.MakeKeyAndVisible();
                    KeyWindowRef = null;

                    window = instance as CoreWindow;
                    if (window != null)
                    {
                        window.OrientationChanged -= OnOrientationChanged;
                        window.OrientationChanged += OnOrientationChanged;

                        window.SizeChanged -= OnSizeChanged;
                        window.SizeChanged += OnSizeChanged;
                    }
                }
            }
        }
        private UIWindow instance;

        /// <summary>
        /// Gets the current orientation of the rendered content within the window.
        /// </summary>
        public DisplayOrientations Orientation
        {
            get
            {
                return UIApplication.SharedApplication.KeyWindow.RootViewController?.InterfaceOrientation.GetDisplayOrientations() ??
                    (UIScreen.MainScreen.ApplicationFrame.Width > UIScreen.MainScreen.ApplicationFrame.Height ? DisplayOrientations.Landscape : DisplayOrientations.Portrait);
            }
        }

        /// <summary>
        /// Gets or sets the style for the window.
        /// </summary>
        public WindowStyle Style { get; set; }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public double Width
        {
            get { return UIApplication.SharedApplication.KeyWindow.Frame.Width; }
        }

        // temporary storage of key window to prevent GC
        internal static UIWindow KeyWindowRef { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public Window()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onActivated:"), UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onDeactivated:"), UIApplication.WillResignActiveNotification, null);

            Instance = UIApplication.SharedApplication.KeyWindow;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public void Close()
        {
            Logger.Warn("Closing the application window is not supported on this platform.  Ignoring.");
        }
        
        /// <summary>
        /// Sets the preferred minimum size of the window.
        /// </summary>
        /// <param name="minSize">The preferred minimum size.</param>
        public void SetPreferredMinSize(Size minSize) { }

        /// <summary>
        /// Displays the window if it is not already visible.
        /// </summary>
        public void Show()
        {
            UIApplication.SharedApplication.KeyWindow.Hidden = false;
        }
        
        /// <summary>
        /// Attempts to resize the window to the specified size.
        /// </summary>
        /// <param name="newSize">The width and height at which to size the window.</param>
        /// <returns><c>true</c> if the window was successfully resized; otherwise, <c>false</c>.</returns>
        public bool TryResize(Size newSize)
        {
            return false;
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIApplication.WillResignActiveNotification, null);

            base.Dispose(disposing);
        }

        [Export("onActivated:")]
        private void OnActivated(NSNotification notification)
        {
            Activated(this, EventArgs.Empty);
        }

        [Export("onDeactivated:")]
        private void OnDeactivated(NSNotification notification)
        {
            Deactivated(this, EventArgs.Empty);
        }

        private void OnOrientationChanged(object sender, DisplayOrientationChangedEventArgs e)
        {
            OrientationChanged(this, e);
        }

        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SizeChanged(this, e);
        }
    }
    
    /// <summary>
    /// Represents a <see cref="UIWindow"/> that performs additional core functions.
    /// </summary>
    public class CoreWindow : UIWindow
    {
        /// <summary>
        /// Occurs when the orientation of the rendered content has changed.
        /// </summary>
        public event EventHandler<DisplayOrientationChangedEventArgs> OrientationChanged;

        /// <summary>
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

        private UIInterfaceOrientation currentOrientation;
        private Size currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreWindow"/> class.
        /// </summary>
        /// <param name="frame"></param>
        public CoreWindow(CGRect frame)
            : base(frame)
        {
            currentOrientation = UIScreen.MainScreen.ApplicationFrame.Width > UIScreen.MainScreen.ApplicationFrame.Height ?
                UIInterfaceOrientation.LandscapeLeft : UIInterfaceOrientation.Portrait;

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

            var orientation = RootViewController?.InterfaceOrientation ?? (UIScreen.MainScreen.ApplicationFrame.Width > UIScreen.MainScreen.ApplicationFrame.Height ?
                UIInterfaceOrientation.LandscapeLeft : UIInterfaceOrientation.Portrait);

            if (currentOrientation != orientation)
            {
                OrientationChanged?.Invoke(this, new DisplayOrientationChangedEventArgs(orientation.GetDisplayOrientations()));
                currentOrientation = orientation;
            }
        }
    }
}