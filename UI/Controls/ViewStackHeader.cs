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
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeViewStackHeader"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    public sealed class ViewStackHeader : INativeViewStackHeader
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the background for the header.
        /// </summary>
        public Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);

                    background = value;
                    navigationBar.BarTintColor = background.GetColor(navigationBar.Frame.Width, navigationBar.Frame.Height, OnBackgroundImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the font to use for displaying the title text.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the header.
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

                    var uifont = navigationBar.TitleTextAttributes.Font;
                    var size = Title.GetStringSize(navigationBar.Frame.Size, uifont);
                    navigationBar.TitleTextAttributes = new UIStringAttributes()
                    {
                        Font = uifont,
                        ForegroundColor = foreground.GetColor(size.Width, size.Height, OnForegroundImageLoaded)
                    };

                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets the title for the header.
        /// </summary>
        public string Title
        {
            get { return navigationBar.TopItem?.Title ?? string.Empty; }
            set
            {
                var item = navigationBar.TopItem;
                if (item != null && value != item.Title)
                {
                    item.Title = value ?? string.Empty;
                    title = item.Title;
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.TitleProperty);
                }
            }
        }
        private string title;

        private readonly UINavigationBar navigationBar;

        internal ViewStackHeader(UINavigationBar navBar)
        {
            navigationBar = navBar;
        }

        internal void CheckTitle()
        {
            if (title != navigationBar.TopItem?.Title)
            {
                title = navigationBar.TopItem.Title;
                OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.TitleProperty);
            }
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            navigationBar.BarTintColor = background.GetColor(navigationBar.Frame.Width, navigationBar.Frame.Height, null);
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            SetForeground();
        }

        private void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void SetForeground()
        {
            var uifont = fontFamily.GetUIFont(fontSize, fontStyle);
            var size = Title.GetStringSize(navigationBar.Frame.Size, uifont);
            navigationBar.TitleTextAttributes = new UIStringAttributes()
            {
                Font = uifont,
                ForegroundColor = foreground.GetColor(size.Width, size.Height, null) ?? UIColor.Black
            };
        }
    }
}

