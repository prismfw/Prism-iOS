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
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeAlert"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeAlert))]
    public class Alert : INativeAlert
    {
        /// <summary>
        /// Gets the number of buttons that have been added to the alert.
        /// </summary>
        public int ButtonCount
        {
            get { return alert.Actions.Length; }
        }

        /// <summary>
        /// Does nothing on iOS.
        /// </summary>
        public int CancelButtonIndex { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the button that is mapped to the Enter key on desktop platforms.
        /// </summary>
        public int DefaultButtonIndex
        {
            get { return defaultButtonIndex; }
            set
            {
                defaultButtonIndex = value;
                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    if (alert.Actions.Length > defaultButtonIndex && defaultButtonIndex >= 0)
                    {
                        alert.PreferredAction = alert.Actions[defaultButtonIndex];
                    }
                    else
                    {
                        alert.PreferredAction = null;
                    }
                }
            }
        }
        private int defaultButtonIndex;

        /// <summary>
        /// Gets the message text for the alert.
        /// </summary>
        public string Message
        {
            get { return alert.Message; }
        }

        /// <summary>
        /// Gets the title text for the alert.
        /// </summary>
        public string Title
        {
            get { return alert.Title; }
        }

        private readonly UIAlertController alert;
        private readonly List<AlertButton> buttons;

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        /// <param name="message">The message text for the alert.</param>
        /// <param name="title">The title text for the alert.</param>
        public Alert(string message, string title)
        {
            alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            buttons = new List<AlertButton>();
        }

        /// <summary>
        /// Adds the specified <see cref="AlertButton"/> to the alert.
        /// </summary>
        /// <param name="button">The button to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="button"/> is <c>null</c>.</exception>
        public void AddButton(AlertButton button)
        {
            buttons.Add(button);
            alert.AddAction(UIAlertAction.Create(button.Title, UIAlertActionStyle.Default, (o) =>
            {
                int index = Array.IndexOf(alert.Actions, o);
                var b = buttons[index];
                if (b.Action != null)
                {
                    b.Action.Invoke(b);
                }
            }));
            
            if (alert.Actions.Length - 1 == defaultButtonIndex && UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                alert.PreferredAction = alert.Actions[defaultButtonIndex];
            }
        }

        /// <summary>
        /// Gets the button at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the button to retrieve.</param>
        /// <returns>The <see cref="AlertButton"/> at the specified index -or- <c>null</c> if there is no button.</returns>
        public AlertButton GetButton(int index)
        {
            return buttons.ElementAtOrDefault(index);
        }

        /// <summary>
        /// Modally presents the alert.
        /// </summary>
        public void Show()
        {
            var top = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (top.PresentedViewController != null)
            {
                top = top.PresentedViewController;
            }

            top.PresentViewController(alert, true, null);
        }
    }
}

