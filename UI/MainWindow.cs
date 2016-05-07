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
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;
using Prism.Utilities;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of a main <see cref="INativeWindow"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWindow), Name = "main")]
    public class MainWindow : NSObject, INativeWindow
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
        /// Occurs when the size of the window has changed.
        /// </summary>
        public event EventHandler<WindowSizeChangedEventArgs> SizeChanged;

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
            set { Logger.Warn("Setting window height is not supported on this platform.  Ignoring."); }
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
                    var window = instance as Window;
                    if (window != null)
                    {
                        window.SizeChanged -= OnSizeChanged;
                    }

                    instance = value;
                    instance.MakeKeyAndVisible();

                    window = instance as Window;
                    if (window != null)
                    {
                        window.SizeChanged -= OnSizeChanged;
                        window.SizeChanged += OnSizeChanged;
                    }
                }
            }
        }
        private UIWindow instance;

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public double Width
        {
            get { return UIApplication.SharedApplication.KeyWindow.Frame.Width; }
            set { Logger.Warn("Setting window width is not supported on this platform.  Ignoring."); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onActivated:"), UIApplication.DidBecomeActiveNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onDeactivated:"), UIApplication.WillResignActiveNotification, null);

            Instance = UIApplication.SharedApplication.KeyWindow;
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public void Close(Animate animate)
        {
            Logger.Warn("Closing the application window is not supported on this platform.  Ignoring.");
        }

        /// <summary>
        /// Displays the window if it is not already visible.
        /// </summary>
        /// <param name="animate">Does nothing on iOS.</param>
        public void Show(Animate animate)
        {
            UIApplication.SharedApplication.KeyWindow.Hidden = false;
        }

        /// <summary>
        /// Captures the contents of the window in an image and returns the result.
        /// </summary>
        public Task<Prism.UI.Media.Imaging.ImageSource> TakeScreenshotAsync()
        {
            return Task.FromResult(new Prism.UI.Media.Imaging.ImageSource(UIScreen.MainScreen.Capture().AsPNG().ToArray()));
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

        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SizeChanged(this, e);
        }
    }
}