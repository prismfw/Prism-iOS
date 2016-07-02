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
using Prism.UI;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativePopup"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePopup))]
    public class Popup : UIViewController, INativePopup
    {
        /// <summary>
        /// Occurs when the popup has been closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the popup has been opened.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when a property value changes.
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
        private bool areAnimationsEnabled = true;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets the object that acts as the content of the popup.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                var oldRoot = ChildViewControllers.FirstOrDefault(vc => vc == content);
                if (oldRoot != null)
                {
                    oldRoot.WillMoveToParentViewController(null);
                    oldRoot.View.RemoveFromSuperview();
                    oldRoot.RemoveFromParentViewController();
                }

                content = value;
                var controller = value as UIViewController;
                AddChildViewController(controller);
                View.AddSubview(controller.View);
                controller.DidMoveToParentViewController(this);
            }
        }
        private object content;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return View.Superview?.Frame.GetRectangle() ?? new Rectangle(new Point(), PreferredContentSize.GetSize()); }
            set
            {
                if (View.Superview != null)
                {
                    var frame = View.Subviews.FirstOrDefault()?.Frame;

                    var center = View.Superview.Center;
                    View.Superview.Frame = value.GetCGRect();
                    View.Superview.Center = center;

                    if (frame.HasValue)
                    {
                        View.Subviews[0].Frame = frame.Value;
                    }
                }
                PreferredContentSize = value.Size.GetCGSize();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return View.UserInteractionEnabled; }
            set
            {
                if (value != View.UserInteractionEnabled)
                {
                    View.UserInteractionEnabled = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the popup can be dismissed by pressing outside of its bounds.
        /// </summary>
        public bool IsLightDismissEnabled
        {
            get { return dismissalGesture.Enabled; }
            set
            {
                if (value != dismissalGesture.Enabled)
                {
                    dismissalGesture.Enabled = value;
                    OnPropertyChanged(Prism.UI.Popup.IsLightDismissEnabledProperty);
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
        
        private UITapGestureRecognizer dismissalGesture;
        private bool isKeyboardChanging;

        /// <summary>
        /// Initializes a new instance of the <see cref="Popup"/> class.
        /// </summary>
        public Popup()
        {
            dismissalGesture = new UITapGestureRecognizer((tap) =>
            {
                if (!isKeyboardChanging && !View.PointInside(tap.LocationInView(View), null))
                {
                    Close();
                }
            });
            
            dismissalGesture.CancelsTouchesInView = false;
            dismissalGesture.Delegate = new DismissalGestureDelegate();
            dismissalGesture.Enabled = false;
            dismissalGesture.NumberOfTapsRequired = 1;
        }
        
        /// <summary>
        /// Closes the popup.
        /// </summary>
        public void Close()
        {
            if (PresentingViewController != null)
            {
                PresentingViewController.DismissViewController(areAnimationsEnabled, () => Closed(this, EventArgs.Empty));
            }
        }
        
        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            View.SetNeedsLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            View.SetNeedsLayout();
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary>
        /// Opens the popup using the specified presenter and presentation style.
        /// </summary>
        /// <param name="presenter">The object responsible for presenting the popup.</param>
        /// <param name="style">The style in which to present the popup.</param>
        public void Open(object presenter, PopupPresentationStyle style)
        {
            ModalPresentationStyle = style == PopupPresentationStyle.FullScreen ?
                UIModalPresentationStyle.FullScreen : UIModalPresentationStyle.FormSheet;
            
            var viewController = (presenter as UIViewController) ?? (presenter as Window)?.Content as UIViewController;
            viewController?.PresentViewController(this, areAnimationsEnabled, () => Opened(this, EventArgs.Empty));
        }
        
        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewWillAppear(bool animated)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillChange:"), UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillChange:"), UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardDidChange:"), UIKeyboard.DidShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardDidChange:"), UIKeyboard.DidHideNotification, null);
            
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }

            base.ViewWillAppear(animated);
        }
        
        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            View.Window.AddGestureRecognizer(dismissalGesture);
        }
        
        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewWillDisappear(bool animated)
        {
            View.Window.RemoveGestureRecognizer(dismissalGesture);
            base.ViewWillDisappear(animated);
        }

        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }

        /// <summary></summary>
        public override void ViewWillLayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.ViewWillLayoutSubviews();
        }
        
        /// <summary></summary>
        /// <param name="fromInterfaceOrientation"></param>
        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            UIView.AnimationsEnabled = true;
            base.DidRotate(fromInterfaceOrientation);
        }

        /// <summary></summary>
        /// <param name="toInterfaceOrientation"></param>
        /// <param name="duration"></param>
        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            UIView.AnimationsEnabled = areAnimationsEnabled;
            base.WillRotate(toInterfaceOrientation, duration);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }
        
        [Export("onKeyboardDidChange:")]
        private void OnKeyboardDidChange(NSNotification notification)
        {
            isKeyboardChanging = false;
        }
        
        [Export("onKeyboardWillChange:")]
        private void OnKeyboardWillChange(NSNotification notification)
        {
            isKeyboardChanging = true;
        }
        
        private class DismissalGestureDelegate : UIGestureRecognizerDelegate
        {
            public override bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                return true;
            }
            
            public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
            {
                return true;
            }
            
            public override bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
            {
                return true;
            }
        }
    }
}

