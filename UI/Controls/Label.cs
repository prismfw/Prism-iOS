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
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeLabel"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeLabel))]
    public class Label : UILabel, INativeLabel
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;
        
        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;
        
        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets the font to use for displaying the text in the label.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    Font = fontFamily.GetUIFont(FontSize, FontStyle);
                    OnPropertyChanged(Prism.UI.Controls.Label.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the text in the label.
        /// </summary>
        public double FontSize
        {
            get { return Font?.PointSize ?? Fonts.StandardLabelFontSize; }
            set
            {
                if (value != (Font?.PointSize ?? Fonts.StandardLabelFontSize))
                {
                    Font = fontFamily.GetUIFont(value, FontStyle);
                    OnPropertyChanged(Prism.UI.Controls.Label.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the text in the label.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return Font?.GetFontStyle() ?? Fonts.StandardLabelFontStyle; }
            set
            {
                if (value != (Font?.GetFontStyle() ?? Fonts.StandardLabelFontStyle))
                {
                    Font = fontFamily.GetUIFont(FontSize, value);
                    OnPropertyChanged(Prism.UI.Controls.Label.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the text contents of the label.
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
                    TextColor = foreground.GetColor(base.Frame.Width, foreground is ImageBrush ? base.Frame.Height : base.Font.LineHeight, OnForegroundImageLoaded) ?? UIColor.Black;
                    OnPropertyChanged(Prism.UI.Controls.Label.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public new Rectangle Frame
        {
            get { return base.Frame.GetRectangle(); }
            set { base.Frame = value.GetCGRect(); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the text contents when the label resides within a highlighted element.
        /// </summary>
        public Brush HighlightBrush
        {
            get { return highlightBrush; }
            set
            {
                if (value != highlightBrush)
                {
                    (highlightBrush as ImageBrush).ClearImageHandler(OnHighlightBrushImageLoaded);

                    highlightBrush = value;
                    HighlightedTextColor = highlightBrush.GetColor(base.Frame.Width, highlightBrush is ImageBrush ? base.Frame.Height : base.Font.LineHeight, OnHighlightBrushImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.Label.HighlightBrushProperty);
                }
            }
        }
        private Brush highlightBrush;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return UserInteractionEnabled; }
            set
            {
                if (value != UserInteractionEnabled)
                {
                    UserInteractionEnabled = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the maximum number of lines of text that the label can show.
        /// A value of 0 means there is no limit.
        /// </summary>
        public new int Lines
        {
            get { return (int)base.Lines; }
            set
            {
                if (value != Lines)
                {
                    base.Lines = value;
                    OnPropertyChanged(Prism.UI.Controls.Label.LinesProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (nfloat)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the label.
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (value != base.Text)
                {
                    base.Text = value;
                    OnPropertyChanged(Prism.UI.Controls.Label.TextProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the text within the label.
        /// </summary>
        public new TextAlignment TextAlignment
        {
            get { return base.TextAlignment.GetTextAlignment(); }
            set
            {
                if (value != base.TextAlignment.GetTextAlignment())
                {
                    base.TextAlignment = value.GetTextAlignment();
                    OnPropertyChanged(Prism.UI.Controls.Label.TextAlignmentProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if (value != visibility)
                {
                    visibility = value;
                    Hidden = value != Visibility.Visible;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }
        private Visibility visibility;
        
        private CGRect currentFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        public Label()
        {
            base.Lines = 0;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            if (string.IsNullOrEmpty(base.Text))
            {
                return new Size();
            }

            constraints.Width = Math.Min(constraints.Width, nfloat.MaxValue);
            constraints.Height = Math.Min(constraints.Height, nfloat.MaxValue);

            var lineHeight = Lines > 0 ? base.Font.LineHeight * Lines : nfloat.MaxValue;
            var size = base.Text.GetStringSize(new CGSize((nfloat)constraints.Width, lineHeight), base.Font);
            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();

            if (currentFrame != base.Frame)
            {
                TextColor = foreground.GetColor(base.Frame.Width, foreground is ImageBrush ? base.Frame.Height : base.Font.LineHeight, null) ?? UIColor.Black;
                HighlightedTextColor = highlightBrush.GetColor(base.Frame.Width, highlightBrush is ImageBrush ? base.Frame.Height : base.Font.LineHeight, null);
            }
            currentFrame = base.Frame;
        }

        /// <summary></summary>
        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview == null && IsLoaded)
            {
                OnUnloaded();
            }
            else if (Superview != null)
            {
                var parent = this.GetNextResponder<INativeVisual>();
                if (parent == null || parent.IsLoaded)
                {
                    OnLoaded();
                }
            }
        }

        /// <summary></summary>
        /// <param name="keyPath"></param>
        /// <param name="ofObject"></param>
        /// <param name="change"></param>
        /// <param name="context"></param>
        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == Visual.IsLoadedProperty.Name)
            {
                var isloaded = (NSNumber)change.ObjectForKey(ChangeNewKey);
                if (isloaded.BoolValue)
                {
                    OnLoaded();
                }
                else
                {
                    OnUnloaded();
                }
            }
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerPressed(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesBegan(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerCanceled(this, evt.GetPointerEventArgs(touch, this));
            }
        
            base.TouchesCancelled(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerReleased(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesEnded(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerMoved(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesMoved(touches, evt);
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
            TextColor = foreground.GetColor(base.Frame.Width, foreground is ImageBrush ? base.Frame.Height : base.Font.LineHeight, null) ?? UIColor.Black;
        }

        private void OnHighlightBrushImageLoaded(object sender, EventArgs e)
        {
            HighlightedTextColor = highlightBrush.GetColor(base.Frame.Width, highlightBrush is ImageBrush ? base.Frame.Height : base.Font.LineHeight, null);
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }
    }
}

