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


using System.Threading.Tasks;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeStatusBar"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeStatusBar))]
    public class StatusBar : INativeStatusBar
    {
        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public Color BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a rectangle describing the area that the status bar is consuming.
        /// </summary>
        public Rectangle Frame
        {
            get { return UIApplication.SharedApplication.StatusBarFrame.GetRectangle(); }
        }

        /// <summary>
        /// Gets a value indicating whether the status bar is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return !UIApplication.SharedApplication.StatusBarHidden; }
        }

        /// <summary>
        /// Gets or sets the style of the status bar.
        /// </summary>
        public StatusBarStyle Style
        {
            get { return style; }
            set
            {
                style = value;
                UIApplication.SharedApplication.StatusBarStyle = style == StatusBarStyle.Dark ?
                    UIStatusBarStyle.LightContent : UIStatusBarStyle.Default;
            }
        }
        private StatusBarStyle style;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusBar"/> class.
        /// </summary>
        public StatusBar()
        {
            var value = NSBundle.MainBundle.ObjectForInfoDictionary("UIViewControllerBasedStatusBarAppearance");
            if (value == null || ((value as NSNumber)?.BoolValue ?? true))
            {
                Prism.Utilities.Logger.Warn("StatusBar requires an entry in Info.plist with a key of UIViewControllerBasedStatusBarAppearance and a value of false.  Calls into StatusBar will not have an effect without this entry!");
            }
        }
        
        /// <summary>
        /// Hides the status bar from view.
        /// </summary>
        public Task HideAsync()
        {
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Slide);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Shows the status bar if it is not visible.
        /// </summary>
        public Task ShowAsync()
        {
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Slide);
            return Task.CompletedTask;
        }
    }
}