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
using Foundation;
using UIKit;
using Prism.Input;
using Prism.Native;
using CoreGraphics;

namespace Prism.iOS.Input
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTapGestureRecognizer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTapGestureRecognizer))]
    public class TapGestureRecognizer : INativeTapGestureRecognizer
    {
        /// <summary>
        /// Occurs when a double tap gesture is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTapped;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when a right tap gesture (or long press gesture for touch input) is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> RightTapped;

        /// <summary>
        /// Occurs when a single tap gesture is recognized.
        /// </summary>
        public event EventHandler<TappedEventArgs> Tapped;

        /// <summary>
        /// Gets or sets a value indicating whether double tap gestures should be recognized.
        /// </summary>
        public bool IsDoubleTapEnabled
        {
            get { return isDoubleTapEnabled; }
            set
            {
                if (value != isDoubleTapEnabled)
                {
                    isDoubleTapEnabled = value;
                    if (DoubleTapRecognizer != null)
                    {
                        DoubleTapRecognizer.Enabled = isDoubleTapEnabled;
                    }
                    else if (Target != null)
                    {
                        DoubleTapRecognizer = CreateDoubleTapGesture();
                        Target.AddGestureRecognizer(DoubleTapRecognizer);
                    }

                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsDoubleTapEnabledProperty);
                }
            }
        }
        private bool isDoubleTapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether right tap gestures (long press gestures for touch input) should be recognized.
        /// </summary>
        public bool IsRightTapEnabled
        {
            get { return isRightTapEnabled; }
            set
            {
                if (value != isRightTapEnabled)
                {
                    isRightTapEnabled = value;
                    if (RightTapRecognizer != null)
                    {
                        RightTapRecognizer.Enabled = isRightTapEnabled;
                    }
                    else if (Target != null)
                    {
                        RightTapRecognizer = CreateRightTapGesture();
                        Target.AddGestureRecognizer(RightTapRecognizer);
                    }

                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsRightTapEnabledProperty);
                }
            }
        }
        private bool isRightTapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether single tap gestures should be recognized.
        /// </summary>
        public bool IsTapEnabled
        {
            get { return isTapEnabled; }
            set
            {
                if (value != isTapEnabled)
                {
                    isTapEnabled = value;
                    if (TapRecognizer != null)
                    {
                        TapRecognizer.Enabled = isTapEnabled;
                    }
                    else if (Target != null)
                    {
                        TapRecognizer = CreateTapGesture();
                        Target.AddGestureRecognizer(TapRecognizer);
                    }

                    OnPropertyChanged(Prism.Input.TapGestureRecognizer.IsTapEnabledProperty);
                }
            }
        }
        private bool isTapEnabled;

        /// <summary>
        /// Gets or sets the maximum distance a touch can move before an active right tap gesture is aborted.
        /// </summary>
        public nfloat MaxMovementTolerance { get; set; } = 10;

        /// <summary>
        /// Gets the gesture recognizer for double taps.
        /// </summary>
        protected UITapGestureRecognizer DoubleTapRecognizer { get; private set; }

        /// <summary>
        /// Gets the gesture recognizer for right taps.
        /// </summary>
        protected UILongPressGestureRecognizer RightTapRecognizer { get; private set; }

        /// <summary>
        /// Gets the gesture recognizer for single taps.
        /// </summary>
        protected UITapGestureRecognizer TapRecognizer { get; private set; }

        /// <summary>
        /// Gets the target view of the gesture recognizer.
        /// </summary>
        /// <value>The target.</value>
        protected UIView Target { get; private set; }

        private UITouch currentTouch;
        private bool isLongPressActive;
        private CGPoint longPressAnchor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TapGestureRecognizer"/> class.
        /// </summary>
        public TapGestureRecognizer()
        {
        }

        /// <summary>
        /// Removes the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to clear as the target.</param>
        public void ClearTarget(object target)
        {
            if (Target == target && Target != null)
            {
                if (DoubleTapRecognizer != null)
                {
                    Target.RemoveGestureRecognizer(DoubleTapRecognizer);
                }

                if (RightTapRecognizer != null)
                {
                    Target.RemoveGestureRecognizer(RightTapRecognizer);
                }

                if (TapRecognizer != null)
                {
                    Target.RemoveGestureRecognizer(TapRecognizer);
                }

                Target = null;
            }
        }

        /// <summary>
        /// Sets the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to set as the target.</param>
        public void SetTarget(object target)
        {
            Target = target as UIView;
            if (Target != null)
            {
                if (DoubleTapRecognizer != null)
                {
                    Target.AddGestureRecognizer(DoubleTapRecognizer);
                }
                else if (isDoubleTapEnabled)
                {
                    Target.AddGestureRecognizer(DoubleTapRecognizer = CreateDoubleTapGesture());
                }

                if (RightTapRecognizer != null)
                {
                    Target.AddGestureRecognizer(RightTapRecognizer);
                }
                else if (isRightTapEnabled)
                {
                    Target.AddGestureRecognizer(RightTapRecognizer = CreateRightTapGesture());
                }

                if (TapRecognizer != null)
                {
                    Target.AddGestureRecognizer(TapRecognizer);
                }
                else if (isTapEnabled)
                {
                    Target.AddGestureRecognizer(TapRecognizer = CreateTapGesture());
                }
            }
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private UITapGestureRecognizer CreateDoubleTapGesture()
        {
            return new UITapGestureRecognizer(OnDoubleTapGestureRecognized)
            {
                CancelsTouchesInView = false,
                Delegate = new GestureRecognizerDelegate(this),
                NumberOfTapsRequired = 2
            };
        }

        private UILongPressGestureRecognizer CreateRightTapGesture()
        {
            return new UILongPressGestureRecognizer(OnRightTapGestureRecognized)
            {
                CancelsTouchesInView = false,
                Delegate = new GestureRecognizerDelegate(this)
            };
        }

        private UITapGestureRecognizer CreateTapGesture()
        {
            return new UITapGestureRecognizer(OnTapGestureRecognized)
            {
                CancelsTouchesInView = false,
                Delegate = new GestureRecognizerDelegate(this)
            };
        }

        private void OnDoubleTapGestureRecognized(UITapGestureRecognizer recognizer)
        {
            DoubleTapped(this, new TappedEventArgs(currentTouch?.Type.GetPointerType() ?? 0,
                recognizer.LocationInView(recognizer.View).GetPoint(), 2));

            currentTouch = null;
        }

        private void OnRightTapGestureRecognized(UILongPressGestureRecognizer recognizer)
        {
            if (recognizer.State == UIGestureRecognizerState.Began)
            {
                longPressAnchor = RightTapRecognizer.LocationInView(RightTapRecognizer.View);
                isLongPressActive = true;
            }
            else if (recognizer.State == UIGestureRecognizerState.Changed)
            {
                var location = RightTapRecognizer.LocationInView(RightTapRecognizer.View);
                nfloat x = NMath.Abs(longPressAnchor.X - location.X);
                nfloat y = NMath.Abs(longPressAnchor.Y - location.Y);
                if (NMath.Sqrt(x * x + y * y) > MaxMovementTolerance)
                {
                    isLongPressActive = false;
                }
            }
            else if (recognizer.State == UIGestureRecognizerState.Ended && isLongPressActive)
            {
                RightTapped(this, new TappedEventArgs(currentTouch?.Type.GetPointerType() ?? 0,
                    recognizer.LocationInView(recognizer.View).GetPoint(), 1));

                currentTouch = null;
                isLongPressActive = false;
            }
        }

        private void OnTapGestureRecognized(UITapGestureRecognizer recognizer)
        {
            Tapped(this, new TappedEventArgs(currentTouch?.Type.GetPointerType() ?? 0,
                recognizer.LocationInView(recognizer.View).GetPoint(), 1));

            currentTouch = null;
        }

        private class GestureRecognizerDelegate : UIGestureRecognizerDelegate
        {
            private readonly WeakReference gestureRecognizer;

            public GestureRecognizerDelegate(TapGestureRecognizer recognizer)
            {
                gestureRecognizer = new WeakReference(recognizer);
            }

            public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
            {
                var tapRecognizer = gestureRecognizer.Target as TapGestureRecognizer;
                if (tapRecognizer != null)
                {
                    tapRecognizer.currentTouch = touch;
                }

                return true;
            }
        }
    }
}

