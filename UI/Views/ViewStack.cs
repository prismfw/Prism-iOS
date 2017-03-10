/*
Copyright (C) 2017  Prism Framework Team

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
using ObjCRuntime;
using Prism.iOS.UI.Controls;
using Prism.Native;
using Prism.UI;
using UIKit;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeViewStack"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeViewStack))]
    public class ViewStack : UINavigationController, INativeViewStack, IVisualTreeObject
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when a view is being popped off of the view stack.
        /// </summary>
        public event EventHandler<NativeViewStackPoppingEventArgs> Popping;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Occurs when the current view of the view stack has changed.
        /// </summary>
        public event EventHandler ViewChanged;

        /// <summary>
        /// Occurs when the current view of the view stack is being replaced by a different view.
        /// </summary>
        public event EventHandler<NativeViewStackViewChangingEventArgs> ViewChanging;

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
        /// Gets the visual children of the object.
        /// </summary>
        public object[] Children
        {
            get { return new object[] { Header }; }
        }

        /// <summary>
        /// Gets the view that is currently on top of the stack.
        /// </summary>
        public object CurrentView
        {
            get { return TopViewController; }
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
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
        /// Gets the header for the view stack.
        /// </summary>
        public INativeViewStackHeader Header { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the back button is enabled.
        /// </summary>
        public bool IsBackButtonEnabled
        {
            get { return !(TopViewController?.NavigationItem?.HidesBackButton) ?? true; }
            set { TopViewController?.NavigationItem?.SetHidesBackButton(!value, false); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the header is hidden.
        /// </summary>
        public bool IsHeaderHidden
        {
            get { return NavigationBar.Hidden; }
            set
            {
                if (value != NavigationBar.Hidden)
                {
                    SetNavigationBarHidden(value, areAnimationsEnabled);
                    OnPropertyChanged(Prism.UI.ViewStack.IsHeaderHiddenProperty);
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
        /// Gets a collection of the views that are currently a part of the stack.
        /// </summary>
        public IEnumerable<object> Views
        {
            get { return ViewControllers; }
        }
        
        object IVisualTreeObject.Parent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewStack"/> class.
        /// </summary>
        public ViewStack()
        {
            Delegate = new ViewStackDelegate();
            Header = new ViewStackHeader(NavigationBar);
        }

        /// <summary>
        /// Inserts the specified view into the stack at the specified index.
        /// </summary>
        /// <param name="view">The view to be inserted.</param>
        /// <param name="index">The zero-based index of the location in the stack in which to insert the view.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void InsertView(object view, int index, Animate animate)
        {
            var vcs = ViewControllers.ToList();
            vcs.Insert(index, (UIViewController)view);
            SetViewControllers(vcs.ToArray(), areAnimationsEnabled && animate == Animate.On);
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
        /// Removes the top view from the stack.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public object PopView(Animate animate)
        {
            return PopViewController(areAnimationsEnabled && animate != Animate.Off);
        }

        /// <summary>
        /// Removes every view from the stack except for the root view.
        /// </summary>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public object[] PopToRoot(Animate animate)
        {
            var controllers = PopToRootViewController(areAnimationsEnabled && animate != Animate.Off);
            return controllers == null ? null : controllers.ToArray();
        }

        /// <summary>
        /// Removes from the stack every view on top of the specified view.
        /// </summary>
        /// <param name="view">The view to pop to.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public object[] PopToView(object view, Animate animate)
        {
            var controllers = PopToViewController((UIViewController)view, areAnimationsEnabled && animate != Animate.Off);
            return controllers == null ? null : controllers.ToArray();
        }

        /// <summary>
        /// Pushes the specified view onto the top of the stack.
        /// </summary>
        /// <param name="view">The view to push to the top of the stack.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void PushView(object view, Animate animate)
        {
            PushViewController((UIViewController)view, areAnimationsEnabled && animate != Animate.Off);
        }

        /// <summary>
        /// Replaces a view that is currently on the stack with the specified view.
        /// </summary>
        /// <param name="oldView">The view to be replaced.</param>
        /// <param name="newView">The view with which to replace the old view.</param>
        /// <param name="animate">Whether to use any system-defined transition animation.</param>
        public void ReplaceView(object oldView, object newView, Animate animate)
        {
            var vcs = ViewControllers;
            int index = Array.IndexOf(vcs, (UIViewController)oldView);
            if (index >= 0)
            {
                vcs[index] = (UIViewController)newView;
                SetViewControllers(vcs, areAnimationsEnabled && animate == Animate.On);
            }
        }

        /// <summary></summary>
        /// <param name="navigationBar"></param>
        /// <param name="item"></param>
        [Export("navigationBar:shouldPopItem:")]
        public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
        {
            var args = new NativeViewStackPoppingEventArgs(TopViewController);
            Popping(this, args);
            if (args.Cancel)
            {
                return false;
            }

            if (TopViewController.NavigationItem == item)
            {
                base.PopViewController(areAnimationsEnabled);
            }
            return true;
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
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            Header.MeasureRequest(false, new Size(View.Frame.Width, NavigationBar?.Frame.Bottom ?? 0));
            Header.ArrangeRequest(false, null);

            base.ViewWillLayoutSubviews();
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

                (Header as ViewStackHeader)?.OnLoaded();
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

                (Header as ViewStackHeader)?.OnUnloaded();
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

        private void OnViewChanged()
        {
            ViewChanged(this, EventArgs.Empty);
        }

        private void OnViewChanging(object oldView, object newView)
        {
            if (oldView != newView)
            {
                ViewChanging(this, new NativeViewStackViewChangingEventArgs(oldView, newView));
            }
        }

        private class ViewStackDelegate : UINavigationControllerDelegate
        {
            private WeakReference currentViewController;

            public override void DidShowViewController(UINavigationController navigationController, [Transient]UIViewController viewController, bool animated)
            {
                bool same = currentViewController?.Target == viewController;
                currentViewController = new WeakReference(viewController);

                var viewStack = navigationController as ViewStack;
                if (viewStack != null)
                {
                    (viewStack.Header as ViewStackHeader)?.CheckTitle();
                    if (!same)
                    {
                        viewStack.OnViewChanged();
                    }
                }
            }

            public override void WillShowViewController(UINavigationController navigationController, [Transient]UIViewController viewController, bool animated)
            {
                (navigationController as ViewStack)?.OnViewChanging(currentViewController?.Target, viewController);
            }
        }
    }
}

