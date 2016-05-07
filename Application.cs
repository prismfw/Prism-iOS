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
using Foundation;
using Prism.Native;
using Prism.Utilities;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeApplication"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeApplication), IsSingleton = true)]
    public class Application : INativeApplication
    {
        /// <summary>
        /// Occurs when the application is shutting down.
        /// </summary>
        public event EventHandler Exiting;

        /// <summary>
        /// Occurs when the application is resuming from suspension.
        /// </summary>
        public event EventHandler Resuming;

        /// <summary>
        /// Occurs when the application is suspending.
        /// </summary>
        public event EventHandler Suspending;

        /// <summary>
        /// Occurs when an unhandled exception is encountered.
        /// </summary>
        public event EventHandler<ErrorEventArgs> UnhandledException;

        /// <summary>
        /// Gets the platform on which the application is running.
        /// </summary>
        public Platform Platform
        {
            get { return Platform.iOS; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidEnterBackgroundNotification, (n) => Suspending(this, EventArgs.Empty));
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillEnterForegroundNotification, (n) => Resuming(this, EventArgs.Empty));
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillTerminateNotification, (n) => Exiting(this, EventArgs.Empty));
            AppDomain.CurrentDomain.UnhandledException += (o, e) => UnhandledException(this, new ErrorEventArgs(e.ExceptionObject as Exception));
        }

        /// <summary>
        /// Signals the system to begin ignoring any user interactions within the application.
        /// </summary>
        public void BeginIgnoringUserInput()
        {
            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
        }

        /// <summary>
        /// Asynchronously invokes the specified delegate on the platform's main thread.
        /// </summary>
        /// <param name="action">The action to invoke on the main thread.</param>
        public void BeginInvokeOnMainThread(Action action)
        {
            UIApplication.SharedApplication.BeginInvokeOnMainThread(action);
        }

        /// <summary>
        /// Asynchronously invokes the specified delegate on the platform's main thread.
        /// </summary>
        /// <param name="del">A delegate to a method that takes multiple parameters.</param>
        /// <param name="parameters">The parameters for the delegate method.</param>
        public void BeginInvokeOnMainThreadWithParameters(Delegate del, params object[] parameters)
        {
            UIApplication.SharedApplication.BeginInvokeOnMainThread(() => del.DynamicInvoke(parameters));
        }

        /// <summary>
        /// Signals the system to stop ignoring user interactions within the application.
        /// </summary>
        public void EndIgnoringUserInput()
        {
            UIApplication.SharedApplication.EndIgnoringInteractionEvents();
        }

        /// <summary>
        /// Launches the specified URL in an external application, most commonly a web browser.
        /// </summary>
        /// <param name="url">The URL to launch to.</param>
        public void LaunchUrl(Uri url)
        {
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}

