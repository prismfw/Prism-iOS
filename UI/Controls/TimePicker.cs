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
using System.Linq;
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using Prism.Utilities;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTimePicker"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTimePicker))]
    public class TimePicker : UIButton, INativeTimePicker
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
        /// Occurs when the selected time has changed.
        /// </summary>
        public event EventHandler<TimeChangedEventArgs> TimeChanged;

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
                    BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, OnBackgroundImageChanged);
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
                    Font = fontFamily.GetUIFont(FontSize, FontStyle);
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
            get { return Font?.PointSize ?? 0; }
            set
            {
                if (value != (Font?.PointSize ?? 0))
                {
                    Font = fontFamily.GetUIFont(value, FontStyle);
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return Font?.GetFontStyle() ?? FontStyle.Normal; }
            set
            {
                if (value != (Font?.GetFontStyle() ?? FontStyle.Normal))
                {
                    Font = fontFamily.GetUIFont(FontSize, value);
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }

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
                    SetTitleColor(foreground.GetColor(Bounds.Width, Bounds.Height, OnForegroundImageChanged) ?? UIColor.Black, UIControlState.Normal);
                    SetNeedsDisplay();
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
        /// Gets or sets a value indicating whether the picker is open.
        /// </summary>
        public bool IsOpen
        {
            get { return timePickerController != null && timePickerController.View.Window != null; }
            set
            {
                if (value != IsOpen)
                {
                    if (timePickerController == null)
                    {
                        timePickerController = new TimePickerViewController(this);
                    }

                    var navController = this.GetNextResponder<UINavigationController>();
                    if (navController == null)
                    {
                        if (timePickerController.PresentingViewController == null && value)
                        {
                            var viewController = this.GetNextResponder<UIViewController>();
                            if (viewController == null)
                            {
                                Logger.Warn("Unable to open time picker.  There is no suitable UIViewController to present it.");
                                return;
                            }

                            viewController.PresentViewController(timePickerController, areAnimationsEnabled, null);
                        }
                        else if (!value)
                        {
                            timePickerController.PresentingViewController.DismissViewController(areAnimationsEnabled, null);
                        }
                    }
                    else if (value)
                    {
                        navController.PushViewController(timePickerController, areAnimationsEnabled);
                    }
                    else if (navController.TopViewController == timePickerController)
                    {
                        navController.PopViewController(areAnimationsEnabled);
                    }
                    else
                    {
                        var controllers = navController.ViewControllers.ToList();
                        controllers.RemoveAll(vc => vc == timePickerController);
                        navController.SetViewControllers(controllers.ToArray(), false);
                    }

                    OnPropertyChanged(Prism.UI.Controls.TimePicker.IsOpenProperty);
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
        /// Gets or sets the selected time.
        /// </summary>
        public TimeSpan? SelectedTime
        {
            get { return selectedTime; }
            set
            {
                if (value != selectedTime)
                {
                    var oldValue = selectedTime;
                    selectedTime = value;
                    OnPropertyChanged(Prism.UI.Controls.TimePicker.SelectedTimeProperty);
                    TimeChanged(this, new TimeChangedEventArgs(oldValue, selectedTime));

                    SetTitle();
                    if (IsOpen)
                    {
                        timePickerController.SetValue(areAnimationsEnabled);
                    }
                }
            }
        }
        private TimeSpan? selectedTime;

        /// <summary>
        /// Gets or sets the format in which to display the string value of the selected time.
        /// </summary>
        public string TimeStringFormat
        {
            get { return timeStringFormat; }
            set
            {
                if (value != timeStringFormat)
                {
                    timeStringFormat = value;
                    OnPropertyChanged(Prism.UI.Controls.TimePicker.TimeStringFormatProperty);

                    SetTitle();
                }
            }
        }
        private string timeStringFormat;

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

        private CGSize currentSize;
        private TimePickerViewController timePickerController;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatePicker"/> class.
        /// </summary>
        public TimePicker()
        {
            MultipleTouchEnabled = true;

            SetTitle();
            SetTitleColor(UIColor.Black, UIControlState.Normal);

            TouchDown += (sender, e) => BecomeFirstResponder();
            TouchUpInside += (sender, e) => IsOpen = true;
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
            base.LayoutSubviews();

            TitleLabel.SizeToFit();
            return new Size(Math.Min(constraints.Width, TitleLabel.Bounds.Width + (BorderWidth * 2)),
                Math.Min(constraints.Height, TitleLabel.Bounds.Height + (BorderWidth * 2)));
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

            if (currentSize != Bounds.Size)
            {
                BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null);
                Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, null)?.CGColor ?? UIColor.Black.CGColor;
                SetTitleColor(foreground.GetColor(Bounds.Width, Bounds.Height, null) ?? UIColor.Black, UIControlState.Normal);
            }
            currentSize = Bounds.Size;
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
        /// <returns></returns>
        /// <param name="color"></param>
        /// <param name="forState"></param>
        public override void SetTitleColor(UIColor color, UIControlState forState)
        {
            base.SetTitleColor(color, forState);

            // gets around an odd issue with the foreground not being honored
            if (TitleLabel != null)
            {
                TitleLabel.TextColor = color;
            }
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
            BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null);
        }

        private void OnBorderImageChanged(object sender, EventArgs e)
        {
            Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, null)?.CGColor ?? UIColor.Black.CGColor;
        }

        private void OnForegroundImageChanged(object sender, EventArgs e)
        {
            SetTitleColor(foreground.GetColor(Bounds.Width, Bounds.Height, null) ?? UIColor.Black, UIControlState.Normal);
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

        private void SetTitle()
        {
            if (selectedTime.HasValue)
            {
                base.SetTitle((DateTime.Today + selectedTime.Value).ToString(timeStringFormat ?? "t"), UIControlState.Normal);
            }
            else
            {
                char[] array = DateTime.MinValue.ToString(timeStringFormat ?? "t").ToCharArray();
                for (int i = 0; i < array.Length; i++)
                {
                    if (char.IsLetterOrDigit(array[i]))
                    {
                        array[i] = '_';
                    }
                }

                base.SetTitle(new string(array), UIControlState.Normal);
            }
        }

        private class TimePickerViewController : UIViewController
        {
            private readonly TimePicker timePicker;

            public TimePickerViewController(TimePicker timePicker)
            {
                this.timePicker = timePicker;
            }

            public void SetValue(bool animated)
            {
                var picker = (UIDatePicker)View.Subviews.FirstOrDefault(sv => sv is UIDatePicker);
                if (picker != null)
                {
                    picker.SetDate((NSDate)(timePicker.selectedTime.HasValue ?
                        (DateTime.Today + timePicker.selectedTime.Value) : DateTime.Now), animated);
                }
            }

            public override void ViewDidLoad()
            {
                View.BackgroundColor = UIColor.White;

                var picker = new UIDatePicker()
                {
                    AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
                    Mode = UIDatePickerMode.Time
                };
                View.Add(picker);

                picker.ValueChanged += (sender, e) =>
                {
                    timePicker.SelectedTime = ((DateTime)picker.Date).ToLocalTime().TimeOfDay;
                };
            }

            public override void ViewWillAppear(bool animated)
            {
                SetValue(false);

                if (NavigationController == null)
                {
                    var weak = new WeakReference(this);
                    var bar = new UINavigationBar(new CGRect(0, 0, View.Bounds.Width, 64))
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                        Items = new UINavigationItem[]
                        {
                            new UINavigationItem()
                            {
                                LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, e) =>
                                {
                                    var controller = weak.Target as UIViewController;
                                    if (controller != null && controller.PresentingViewController != null)
                                    {
                                        controller.PresentingViewController.DismissViewController(timePicker.areAnimationsEnabled, null);
                                    }
                                })
                            }
                        }
                    };
                    View.Add(bar);
                }
                else
                {
                    for (int i = 0; i < View.Subviews.Length; i++)
                    {
                        var subview = View.Subviews[i];
                        if (subview is UINavigationBar && subview != NavigationController.NavigationBar)
                        {
                            subview.RemoveFromSuperview();
                        }
                    }
                }
            }

            public override void ViewWillLayoutSubviews()
            {
                base.ViewWillLayoutSubviews();

                var picker = (UIDatePicker)View.Subviews.FirstOrDefault(sv => sv is UIDatePicker);
                if (picker != null)
                {
                    picker.Center = View.Center;
                }
            }
        }
    }
}

