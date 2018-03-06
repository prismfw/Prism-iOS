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
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeToggleSwitch"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeToggleSwitch))]
    public class ToggleSwitch : UISwitch, INativeToggleSwitch
    {
        /// <summary>
        /// Occurs when the control receives focus.
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the control loses focus.
        /// </summary>
        public event EventHandler LostFocus;

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
        /// Occurs when the value of the toggle switch is changed.
        /// </summary>
        public new event EventHandler ValueChanged;

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
        /// Gets or sets the background for the control.
        /// </summary>
        public Brush Background
        {
            get { return background; }
            set
            {
                if (value != background)
                {
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageChanged);

                    background = value;
                    BackgroundColor = TintColor = background.GetColor(Bounds.Width, Bounds.Height, OnBackgroundImageChanged);
                    OnPropertyChanged(Control.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the border of the control.
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != borderBrush)
                {
                    (borderBrush as ImageBrush).ClearImageHandler(OnBorderImageChanged);

                    borderBrush = value;
                    Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, OnBorderImageChanged)?.CGColor ?? UIColor.Black.CGColor;
                    OnPropertyChanged(Control.BorderBrushProperty);
                }
            }
        }
        private Brush borderBrush;

        /// <summary>
        /// Gets or sets the width of the border around the control.
        /// </summary>
        public double BorderWidth
        {
            get { return Layer.BorderWidth; }
            set
            {
                if (value != Layer.BorderWidth)
                {
                    Layer.BorderWidth = (nfloat)value;
                    OnPropertyChanged(Control.BorderWidthProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can become first responder.
        /// </summary>
        public override bool CanBecomeFirstResponder
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the font to use for displaying the text in the control.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    OnPropertyChanged(Control.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the text in the control.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the control.
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    (foreground as ImageBrush).ClearImageHandler(OnForegroundImageChanged);

                    foreground = value;
                    OnTintColor = foreground.GetColor(Bounds.Width, Bounds.Height, OnForegroundImageChanged);
                    OnPropertyChanged(Control.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public new Rectangle Frame
        {
            get { return new Rectangle(Center.X - (Bounds.Width / 2), Center.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height); }
            set
            {
                Bounds = new CGRect(Bounds.Location, value.Size.GetCGSize());
                Center = new CGPoint(value.X + (value.Width / 2), value.Y + (value.Height / 2));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can interact with the control.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;
                    OnPropertyChanged(Control.IsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control has focus.
        /// </summary>
        public bool IsFocused { get; private set; }

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
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(this);

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the thumb of the control when the control's value is false.
        /// </summary>
        public Brush ThumbOffBrush
        {
            get { return thumbOffBrush; }
            set
            {
                if (value != thumbOffBrush)
                {
                    (thumbOffBrush as ImageBrush).ClearImageHandler(OnThumbOffImageChanged);

                    thumbOffBrush = value;
                    if (!On)
                    {
                        ThumbTintColor = value.GetColor(Bounds.Width, Bounds.Height, null);
                    }

                    OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ThumbOffBrushProperty);
                }
            }
        }
        private Brush thumbOffBrush;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the thumb of the control when the control's value is true.
        /// </summary>
        public Brush ThumbOnBrush
        {
            get { return thumbOnBrush; }
            set
            {
                if (value != thumbOnBrush)
                {
                    (thumbOnBrush as ImageBrush).ClearImageHandler(OnThumbOnImageChanged);

                    thumbOnBrush = value;
                    if (On)
                    {
                        ThumbTintColor = value.GetColor(Bounds.Width, Bounds.Height, null);
                    }

                    OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ThumbOnBrushProperty);
                }
            }
        }
        private Brush thumbOnBrush;

        /// <summary>
        /// Gets or sets the value of the toggle switch.
        /// </summary>
        public bool Value
        {
            get { return On; }
            set
            {
                if (value != On)
                {
                    SetState(value, areAnimationsEnabled);
                    OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ValueProperty);
                    ValueChanged(this, EventArgs.Empty);
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
        /// </summary>
        public ToggleSwitch()
        {
            Layer.CornerRadius = 16f;
            MultipleTouchEnabled = true;
            On = false;

            // TouchDown doesn't seem to work, so using whatever does.
            AddTarget((sender, e) => BecomeFirstResponder(), UIControlEvent.AllTouchEvents);

            base.ValueChanged += (sender, e) =>
            {
                if (On && (thumbOnBrush != null || ThumbTintColor != null))
                {
                    ThumbTintColor = thumbOnBrush.GetColor(Bounds.Width, Bounds.Height, null);
                }
                else if (!On && (thumbOffBrush != null || ThumbTintColor != null))
                {
                    ThumbTintColor = thumbOffBrush.GetColor(Bounds.Width, Bounds.Height, null);
                }

                OnPropertyChanged(Prism.UI.Controls.ToggleSwitch.ValueProperty);
                ValueChanged(this, EventArgs.Empty);
            };
        }

        /// <summary>
        /// Attempts to set focus to the control.
        /// </summary>
        public void Focus()
        {
            BecomeFirstResponder();
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
            var frame = base.Frame;
            base.Frame = new CGRect(CGPoint.Empty, new CGSize((nfloat)constraints.Width, (nfloat)constraints.Height));
            SizeToFit();

            var size = new Size(Bounds.Width, Bounds.Height);
            base.Frame = frame;
            return new Size(Math.Min(constraints.Width, size.Width + (BorderWidth * 2)),
                Math.Min(constraints.Height, size.Height + (BorderWidth * 2)));
        }

        /// <summary>
        /// Attempts to remove focus from the control.
        /// </summary>
        public void Unfocus()
        {
            ResignFirstResponder();
        }

        /// <summary></summary>
        public override bool BecomeFirstResponder()
        {
            base.BecomeFirstResponder();

            if (Window != null && !IsFocused)
            {
                IsFocused = true;
                OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
                GotFocus(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();
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
                var isloaded = (NSNumber)change.ObjectForKey(NSObject.ChangeNewKey);
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
        public override bool ResignFirstResponder()
        {
            base.ResignFirstResponder();

            if (IsFocused)
            {
                IsFocused = false;
                OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
                LostFocus(this, EventArgs.Empty);
            }
            return true;
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerPressed(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesBegan(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerCanceled(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesCancelled(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerReleased(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesEnded(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerMoved(this, evt.GetPointerEventArgs(touch, this));
                }
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

        private void OnBackgroundImageChanged(object sender, EventArgs e)
        {
            BackgroundColor = TintColor = background.GetColor(Bounds.Width, Bounds.Height, null);
        }

        private void OnBorderImageChanged(object sender, EventArgs e)
        {
            Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, null)?.CGColor ?? UIColor.Black.CGColor;
        }

        private void OnForegroundImageChanged(object sender, EventArgs e)
        {
            OnTintColor = foreground.GetColor(Bounds.Width, Bounds.Height, null);
        }

        private void OnThumbOffImageChanged(object sender, EventArgs e)
        {
            if (!On)
            {
                ThumbTintColor = thumbOffBrush.GetColor(Bounds.Width, Bounds.Height, null);
            }
        }

        private void OnThumbOnImageChanged(object sender, EventArgs e)
        {
            if (On)
            {
                ThumbTintColor = thumbOnBrush.GetColor(Bounds.Width, Bounds.Height, null);
            }
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