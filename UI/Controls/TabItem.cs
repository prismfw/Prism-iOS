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
using UIKit;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTabItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabItem))]
    public class TabItem : UITabBarItem, INativeTabItem
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the object that acts as the content of the item.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                content = value;

                var tabView = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
                if (tabView == null)
                {
                    var splitView = UIApplication.SharedApplication.KeyWindow.RootViewController as UISplitViewController;
                    if (splitView != null)
                    {
                        tabView = splitView.ViewControllers.FirstOrDefault() as UITabBarController;
                    }
                }

                if (tabView != null)
                {
                    var controllers = tabView.ViewControllers;
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        var controller = controllers[i];
                        if (controller.TabBarItem == this)
                        {
                            controller = content as UIViewController;
                            controller.TabBarItem = this;
                            controllers[i] = controller;
                            tabView.ViewControllers = controllers;
                            break;
                        }
                    }
                }
            }
        }
        private object content;

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
                    var attributes = GetTitleTextAttributes(UIControlState.Normal);
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(attributes.Font?.PointSize ?? Fonts.TabItemFontSize, attributes.Font?.GetFontStyle() ?? Fonts.TabItemFontStyle),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).Font.PointSize; }
            set
            {
                var attributes = GetTitleTextAttributes(UIControlState.Normal);
                if (value != attributes.Font.PointSize)
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(value, attributes.Font.GetFontStyle()),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).Font.GetFontStyle(); }
            set
            {
                var attributes = GetTitleTextAttributes(UIControlState.Normal);
                if (value != attributes.Font.GetFontStyle())
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(attributes.Font.PointSize, value),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the title.
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

                    var font = GetTitleTextAttributes(UIControlState.Normal).Font;
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = font,
                        TextColor = foreground.GetColor(1, font.LineHeight, OnForegroundImageLoaded) ?? UIColor.Gray
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets an <see cref="INativeImageSource"/> for an image to display with the item.
        /// </summary>
        public INativeImageSource ImageSource
        {
            get { return imageSource; }
            set
            {
                if (value != imageSource)
                {
                    imageSource.ClearImageHandler(OnImageLoaded);

                    imageSource = value;
                    Image = imageSource.BeginLoadingImage(OnImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.TabItem.ImageSourceProperty);
                }
            }
        }
        private INativeImageSource imageSource;

        /// <summary>
        /// Gets or sets the title for the item.
        /// </summary>
        public override string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;
                    OnPropertyChanged(Prism.UI.Controls.TabItem.TitleProperty);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabItem"/> class.
        /// </summary>
        public TabItem()
        {
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
            var font = GetTitleTextAttributes(UIControlState.Normal).Font;
            SetTitleTextAttributes(new UITextAttributes()
            {
                Font = font,
                TextColor = foreground.GetColor(1, font.LineHeight, null) ?? UIColor.Gray
            }, UIControlState.Normal);
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            Image = (sender as INativeImageSource).GetImageSource();
        }
    }
}

