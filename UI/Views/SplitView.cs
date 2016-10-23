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
using Prism.Native;
using Prism.UI;
using UIKit;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeSplitView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSplitView))]
    public class SplitView : UISplitViewController, INativeSplitView
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets the actual width of the detail pane.
        /// </summary>
        public double ActualDetailWidth { get; private set; }

        /// <summary>
        /// Gets the actual width of the master pane.
        /// </summary>
        public double ActualMasterWidth { get; private set; }

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
        /// Gets or sets the object that acts as the content for the detail pane.
        /// This is typical an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object DetailContent
        {
            get { return ViewControllers.Length > 1 ? ViewControllers[1] : null; }
            set
            {
                ViewControllers = new[] { ViewControllers.Length > 0 ? ViewControllers[0] : new UIViewController(), value as UIViewController };
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents
        /// the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return new Rectangle(View.Center.X - (View.Bounds.Width / 2), View.Center.Y - (View.Bounds.Height / 2), View.Bounds.Width, View.Bounds.Height); }
            set
            {
                View.Bounds = new CGRect(View.Bounds.Location, value.Size.GetCGSize());
                View.Center = new CGPoint(value.X + (value.Width / 2), value.Y + (value.Height / 2));
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
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets the object that acts as the content for the master pane.
        /// This is typical an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object MasterContent
        {
            get { return ViewControllers.Length > 0 ? ViewControllers[0] : null; }
            set
            {
                ViewControllers = new[] { value as UIViewController, ViewControllers.Length > 1 ? ViewControllers[1] : new UIViewController() };
            }
        }

        /// <summary>
        /// Gets or sets the maximum width of the master pane.
        /// </summary>
        public double MaxMasterWidth
        {
            get { return MaximumPrimaryColumnWidth; }
            set
            {
                if (value != MaximumPrimaryColumnWidth)
                {
                    MaximumPrimaryColumnWidth = (nfloat)value;
                    OnPropertyChanged(Prism.UI.SplitView.MaxMasterWidthProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the minimum width of the master pane.
        /// </summary>
        public double MinMasterWidth
        {
            get { return MinimumPrimaryColumnWidth; }
            set
            {
                if (value != MinimumPrimaryColumnWidth)
                {
                    MinimumPrimaryColumnWidth = (nfloat)value;
                    OnPropertyChanged(Prism.UI.SplitView.MinMasterWidthProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred width of the master pane as a percentage of the width of the split view.
        /// Valid values are between 0.0 and 1.0.
        /// </summary>
        public double PreferredMasterWidthRatio
        {
            get { return PreferredPrimaryColumnWidthFraction; }
            set
            {
                if (value != PreferredPrimaryColumnWidthFraction)
                {
                    PreferredPrimaryColumnWidthFraction = (nfloat)value;
                    OnPropertyChanged(Prism.UI.SplitView.PreferredMasterWidthRatioProperty);
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
                    (renderTransform as Media.Transform)?.RemoveView(View);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(View);

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
        /// Initializes a new instance of the <see cref="SplitView"/> class.
        /// </summary>
        public SplitView()
        {
            MinimumPrimaryColumnWidth = 320;
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
            return new Size(Math.Min(View.Bounds.Width, constraints.Width), Math.Min(View.Bounds.Height, constraints.Height));
        }

        /// <summary>
        /// The orientations supported by this <see cref="UIViewController"/>.
        /// </summary>
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return Prism.UI.Window.Current.AutorotationPreferences.GetInterfaceOrientationMask();
        }

        /// <summary>
        /// The orientation that best displays the content of this <see cref="UIViewController"/>.
        /// </summary>
        /// <returns></returns>
        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            var preferences = Prism.UI.Window.Current.AutorotationPreferences;
            if (preferences.HasFlag(DisplayOrientations.Portrait) && !preferences.HasFlag(DisplayOrientations.Landscape))
            {
                return InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown ? UIInterfaceOrientation.PortraitUpsideDown : UIInterfaceOrientation.Portrait;
            }

            if (preferences.HasFlag(DisplayOrientations.Landscape) && !preferences.HasFlag(DisplayOrientations.Portrait))
            {
                return InterfaceOrientation == UIInterfaceOrientation.LandscapeRight ? UIInterfaceOrientation.LandscapeRight : UIInterfaceOrientation.LandscapeLeft;
            }

            return base.PreferredInterfaceOrientationForPresentation();
        }

        /// <summary></summary>
        public override void ViewWillLayoutSubviews()
        {
            if (PrimaryColumnWidth != ActualMasterWidth)
            {
                ActualMasterWidth = PrimaryColumnWidth;
                OnPropertyChanged(Prism.UI.SplitView.ActualMasterWidthProperty);
            }

            if ((View.Bounds.Width - PrimaryColumnWidth) != ActualDetailWidth)
            {
                ActualDetailWidth = (View.Bounds.Width - PrimaryColumnWidth);
                OnPropertyChanged(Prism.UI.SplitView.ActualDetailWidthProperty);
            }
            
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.ViewWillLayoutSubviews();
        }

        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewWillAppear(bool animated)
        {
            PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;

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
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
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
    }
}

