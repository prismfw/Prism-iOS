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
    /// Represents an iOS implementation of an <see cref="INativeHoldingGestureRecognizer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeHoldingGestureRecognizer))]
    public class HoldingGestureRecognizer : UILongPressGestureRecognizer, INativeHoldingGestureRecognizer
    {
        /// <summary>
        /// Occurs when a holding gesture is started, completed, or canceled.
        /// </summary>
        public event EventHandler<HoldingEventArgs> Holding;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets the maximum distance a touch can move before the gesture is canceled.
        /// </summary>
        public nfloat MaxMovementTolerance { get; set; } = 10;

        private CGPoint anchorPoint;
        private UITouch currentTouch;
        private bool isActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="HoldingGestureRecognizer"/> class.
        /// </summary>
        public HoldingGestureRecognizer()
        {
            CancelsTouchesInView = false;
            AddTarget(OnHoldingGestureRecognized);
        }

        /// <summary>
        /// Removes the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to clear as the target.</param>
        public void ClearTarget(object target)
        {
            (target as UIView)?.RemoveGestureRecognizer(this);
        }

        /// <summary>
        /// Sets the specified object as the target of the gesture recognizer.
        /// </summary>
        /// <param name="target">The object to set as the target.</param>
        public void SetTarget(object target)
        {
            (target as UIView)?.AddGestureRecognizer(this);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            currentTouch = touches.AnyObject as UITouch;
            base.TouchesBegan(touches, evt);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void OnHoldingGestureRecognized()
        {
            var location = LocationInView(View);

            HoldingState holdingState;
            switch (State)
            {
                case UIGestureRecognizerState.Began:
                    holdingState = HoldingState.Started;
                    anchorPoint = location;
                    isActive = true;
                    break;
                case UIGestureRecognizerState.Ended:
                    holdingState = HoldingState.Completed;
                    isActive = false;
                    break;
                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    holdingState = HoldingState.Canceled;
                    location = anchorPoint; // coincides with UWP behavior for touch input
                    isActive = false;
                    break;
                case UIGestureRecognizerState.Changed:
                    if (isActive)
                    {
                        nfloat x = NMath.Abs(anchorPoint.X - location.X);
                        nfloat y = NMath.Abs(anchorPoint.Y - location.Y);
                        if (NMath.Sqrt(x * x + y * y) > MaxMovementTolerance)
                        {
                            State = UIGestureRecognizerState.Cancelled;
                        }
                    }
                    return;
                default:
                    return;
            }

            Holding(this, new HoldingEventArgs(currentTouch?.Type.GetPointerType() ?? 0, location.GetPoint(), holdingState));

            if (!isActive)
            {
                currentTouch = null;
            }
        }
    }
}

