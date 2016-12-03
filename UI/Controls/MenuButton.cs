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
using UIKit;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeMenuButton"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMenuButton))]
    public class MenuButton : UIBarButtonItem, INativeMenuButton
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the action to perform when the button is pressed by the user.
        /// </summary>
        public new Action Action { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the menu item.
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    (foreground as ImageBrush).ClearImageHandler(OnForegroundImageLoaded);
                
                    foreground = value;
                    TintColor = foreground.GetColor(30, 30, OnForegroundImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.MenuItem.ForegroundProperty);
                }
            }
        }
        private Brush foreground;
        
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the image to display within the button.
        /// </summary>
        public Uri ImageUri
        {
            get { return imageUri; }
            set
            {
                if (value != imageUri)
                {
                    imageUri = value;
                    if (imageUri == null)
                    {
                        Image = null;
                    }
                    else
                    {
                        Image = imageUri.IsFile || !imageUri.IsAbsoluteUri ? UIImage.FromFile(imageUri.OriginalString) :
                            UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(imageUri.OriginalString)));
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.ImageUriProperty);
                }
            }
        }
        private Uri imageUri;

        /// <summary>
        /// Gets or sets a value indicating whether the button is enabled and should respond to user interaction.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != Enabled)
                {
                    Enabled = value;
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.IsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title of the button.
        /// </summary>
        public new string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;
                    OnPropertyChanged(Prism.UI.Controls.MenuButton.TitleProperty);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuButton"/> class.
        /// </summary>
        public MenuButton()
        {
            base.Clicked += (o, e) => Action();
        }
        
        /// <summary>
        /// Gets a <see cref="UIAlertAction"/> instance for the button.
        /// </summary>
        public UIAlertAction GetAlertAction()
        {
            var action = UIAlertAction.Create(Title, UIAlertActionStyle.Default, (a) => Action());
            action.Enabled = IsEnabled;
            return action;
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TintColor = foreground.GetColor(30, 30, null);
        }
    }
}

