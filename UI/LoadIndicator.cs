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
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativeLoadIndicator"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeLoadIndicator))]
    public class LoadIndicator : UIView, INativeLoadIndicator
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;
        
        /// <summary>
        /// Gets or sets the background of the indicator.
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
                    BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, OnBackgroundImageLoaded) ?? UIColor.White;
                    OnPropertyChanged(Prism.UI.LoadIndicator.BackgroundProperty);
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
                    TextLabel.Font = fontFamily.GetUIFont(FontSize, FontStyle);
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return TextLabel.Font.PointSize; }
            set
            {
                if (value != TextLabel.Font.PointSize)
                {
                    TextLabel.Font = fontFamily.GetUIFont(value, FontStyle);
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return TextLabel.Font.GetFontStyle(); }
            set
            {
                if (value != TextLabel.Font.GetFontStyle())
                {
                    TextLabel.Font = fontFamily.GetUIFont(FontSize, value);
                    OnPropertyChanged(Prism.UI.LoadIndicator.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the title text.
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

                    ActivityView.Color = value.GetColor(ActivityView.Bounds.Width, ActivityView.Bounds.Height, OnForegroundImageLoaded) ?? UIColor.Black;
                    TextLabel.TextColor = value.GetColor(TextLabel.Bounds.Width, TextLabel.Bounds.Height, OnForegroundImageLoaded) ?? UIColor.Black;

                    OnPropertyChanged(Prism.UI.LoadIndicator.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets a value indicating whether this instance is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get { return Superview != null; }
        }

        /// <summary>
        /// Gets or sets the title text of the indicator.
        /// </summary>
        public string Title
        {
            get { return TextLabel.Text; }
            set
            {
                if (value != TextLabel.Text)
                {
                    TextLabel.Text = value;
                    TextLabel.Frame = new CGRect(TextLabel.Frame.Location, CGSize.Empty);
                    TextLabel.SizeToFit();

                    TextLabel.TextColor = foreground.GetColor(TextLabel.Bounds.Width, TextLabel.Bounds.Height, null) ?? UIColor.Black;

                    OnPropertyChanged(Prism.UI.LoadIndicator.TitleProperty);
                }
            }
        }

        /// <summary>
        /// Gets the UI element that is displaying the activity indicator.
        /// </summary>
        protected UIActivityIndicatorView ActivityView { get; }

        /// <summary>
        /// Gets the UI element that is displaying the text.
        /// </summary>
        protected UILabel TextLabel { get; }

        private readonly nfloat margin = 8;
        private readonly nfloat space = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadIndicator"/> class.
        /// </summary>
        public LoadIndicator()
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            BackgroundColor = UIColor.White;
            Layer.BorderColor = new CGColor(0.85f, 0.85f, 0.85f);
            Layer.BorderWidth = 1;
            Layer.CornerRadius = 10;

            ActivityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            ActivityView.Color = UIColor.Black;
            Add(ActivityView);

            TextLabel = new UILabel();
            Add(TextLabel);
        }

        /// <summary>
        /// Removes the indicator from view.
        /// </summary>
        public void Hide()
        {
            RemoveFromSuperview();
            ActivityView.StopAnimating();
        }

        /// <summary>
        /// Displays the indicator.
        /// </summary>
        public void Show()
        {
            var window = UIApplication.SharedApplication.KeyWindow;

            if (string.IsNullOrEmpty(TextLabel.Text))
            {
                Frame = new CGRect(0, 0, ActivityView.Bounds.Width + margin * 2, ActivityView.Bounds.Height + margin * 2);
            }
            else
            {
                Frame = new CGRect(0, 0, ActivityView.Bounds.Width + TextLabel.Bounds.Width + space + margin * 2, ActivityView.Bounds.Height + margin * 2);
            }

            BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null) ?? UIColor.White;

            Center = window.Center;

            ActivityView.Center = new CGPoint(margin + ActivityView.Bounds.Width / 2, Bounds.Height / 2);
            TextLabel.Frame = new CGRect(new CGPoint(ActivityView.Bounds.Right + space, Bounds.Height / 2 - TextLabel.Font.LineHeight / 2), TextLabel.Bounds.Size);

            ActivityView.StartAnimating();
            window.Add(this);
            window.BringSubviewToFront(this);
        }

        /// <summary></summary>
        public override void MovedToWindow()
        {
            OnPropertyChanged(Prism.UI.LoadIndicator.IsVisibleProperty);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null) ?? UIColor.White;
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            ActivityView.Color = foreground.GetColor(ActivityView.Bounds.Width, ActivityView.Bounds.Height, null) ?? UIColor.Black;
            TextLabel.TextColor = foreground.GetColor(TextLabel.Bounds.Width, TextLabel.Bounds.Height, null) ?? UIColor.Black;
        }
    }
}

