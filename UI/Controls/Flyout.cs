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
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativeFlyout"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFlyout))]
    public class Flyout : UIViewController, INativeFlyout
    {
        /// <summary>
        /// Occurs when the flyout has been closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the flyout has been opened.
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
        /// Gets or sets the background for the flyout.
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

                    var size = PopoverPresentationController.PresentedView?.Frame.Size ?? CGSize.Empty;
                    if (size.Width == 0 && size.Height == 0)
                    {
                        size = PreferredContentSize;
                    }
                    PopoverPresentationController.BackgroundColor = background.GetColor(size.Width, size.Height, OnBackgroundImageChanged);
                    OnPropertyChanged(FlyoutBase.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the element that serves as the content of the flyout.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                if (value != content)
                {
                    for (int i = View.Subviews.Length - 1; i >= 0; i--)
                    {
                        View.Subviews[i].RemoveFromSuperview();
                    }

                    content = value;

                    var view = value as UIView;
                    if (view != null)
                    {
                        View.Add(view);
                    }

                    OnPropertyChanged(Prism.UI.Controls.Flyout.ContentProperty);
                }
            }
        }
        private object content;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return new Rectangle(new Point(), PreferredContentSize.GetSize()); }
            set
            {
                var size = value.Size.GetCGSize();
                if (size != PreferredContentSize)
                {
                    PreferredContentSize = size;

                    size = PopoverPresentationController.PresentedView?.Frame.Size ?? CGSize.Empty;
                    if (size.Width == 0 && size.Height == 0)
                    {
                        size = PreferredContentSize;
                    }
                    PopoverPresentationController.BackgroundColor = background.GetColor(size.Width, size.Height, null);
                }
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
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the placement of the flyout in relation to its placement target.
        /// </summary>
        public FlyoutPlacement Placement
        {
            get { return placement; }
            set
            {
                if (value != placement)
                {
                    placement = value;
                    OnPropertyChanged(FlyoutBase.PlacementProperty);
                }
            }
        }
        private FlyoutPlacement placement;

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
        /// Initializes a new instance of the <see cref="Flyout"/> class.
        /// </summary>
        public Flyout()
        {
            ModalPresentationStyle = UIModalPresentationStyle.Popover;
        }

        /// <summary>
        /// Dismisses the flyout.
        /// </summary>
        public void Hide()
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
        /// Presents the flyout and positions it relative to the specified placement target.
        /// </summary>
        /// <param name="placementTarget">The object to use as the flyout's placement target.</param>
        public void ShowAt(object placementTarget)
        {
            if (!(PopoverPresentationController.Delegate is FlyoutDelegate))
            {
                PopoverPresentationController.Delegate = new FlyoutDelegate(this);
            }

            var view = placementTarget as UIView;
            if (view != null)
            {
                PopoverPresentationController.SourceView = view;
                PopoverPresentationController.SourceRect = new CGRect(CGPoint.Empty, view.Frame.Size);
                view.GetNextResponder<UIViewController>()?.PresentViewController(this, areAnimationsEnabled, () => Opened(this, EventArgs.Empty));
            }
            else
            {
                var button = placementTarget as UIBarButtonItem;
                if (button != null)
                {
                    PopoverPresentationController.BarButtonItem = button;
                    UIApplication.SharedApplication.KeyWindow.RootViewController?.PresentViewController(this, areAnimationsEnabled, () => Opened(this, EventArgs.Empty));
                }
            }
        }

        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewWillAppear(bool animated)
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);

                foreach (var subview in View.Subviews.Where(sv => sv is INativeVisual))
                {
                    try
                    {
                        subview.ObserveValue(new NSString(Visual.IsLoadedProperty.Name), this,
                            NSDictionary.FromObjectAndKey(new NSNumber(true), NSObject.ChangeNewKey), IntPtr.Zero);
                    }
                    catch { }
                }
            }

            var size = PopoverPresentationController.PresentedView?.Frame.Size ?? CGSize.Empty;
            if (size.Width == 0 && size.Height == 0)
            {
                size = PreferredContentSize;
            }
            PopoverPresentationController.BackgroundColor = background.GetColor(size.Width, size.Height, null);

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

                foreach (var subview in View.Subviews.Where(sv => sv is INativeVisual))
                {
                    try
                    {
                        subview.ObserveValue(new NSString(Visual.IsLoadedProperty.Name), this,
                            NSDictionary.FromObjectAndKey(new NSNumber(false), NSObject.ChangeNewKey), IntPtr.Zero);
                    }
                    catch { }
                }
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

        private void OnBackgroundImageChanged(object sender, EventArgs e)
        {
            var size = PopoverPresentationController.PresentedView?.Frame.Size ?? CGSize.Empty;
            if (size.Width == 0 && size.Height == 0)
            {
                size = PreferredContentSize;
            }
            PopoverPresentationController.BackgroundColor = background.GetColor(size.Width, size.Height, null);
        }

        private void OnDismiss()
        {
            Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private class FlyoutDelegate : UIPopoverPresentationControllerDelegate
        {
            private readonly WeakReference flyoutRef;

            public FlyoutDelegate(Flyout flyout)
            {
                flyoutRef = new WeakReference(flyout);
            }

            public override void DidDismissPopover(UIPopoverPresentationController popoverPresentationController)
            {
                (flyoutRef.Target as Flyout)?.OnDismiss();
            }

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller, UITraitCollection traitCollection)
            {
                return UIModalPresentationStyle.None;
            }

            public override void PrepareForPopoverPresentation(UIPopoverPresentationController popoverPresentationController)
            {
                var flyout = flyoutRef.Target as Flyout;
                if (flyout != null)
                {
                    popoverPresentationController.PermittedArrowDirections = flyout.placement.GetPopoverArrowDirection();
                }
            }
        }
    }
}

