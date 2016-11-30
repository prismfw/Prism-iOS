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
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeContentView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeContentView))]
    public class ContentView : UIViewController, INativeContentView
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
        /// Gets or sets the background for the view.
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
                    View.BackgroundColor = background.GetColor(View.Bounds.Width, View.Bounds.Height, OnBackgroundImageLoaded) ?? UIColor.White;
                    OnPropertyChanged(Prism.UI.ContentView.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the content to be displayed by the view.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
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
            }
        }
        private object content;

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
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }
        
        /// <summary>
        /// Gets or sets the action menu for the view.
        /// </summary>
        public INativeActionMenu Menu
        {
            get { return menu; }
            set
            {
                if (value != menu)
                {
                    (menu as Controls.ActionMenu)?.Detach();
                    
                    menu = value;
                    if (NavigationItem != null)
                    {
                        (menu as Controls.ActionMenu)?.Attach(this);
                    }

                    OnPropertyChanged(Prism.UI.ContentView.MenuProperty);
                }
            }
        }
        private INativeActionMenu menu;

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
        /// Gets or sets the title of the view.
        /// </summary>
        public new string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;

                    var header = (NavigationController as ViewStack)?.Header;
                    if (header != null)
                    {
                        header.Title = title ?? string.Empty;
                    }
                    else if (NavigationItem != null)
                    {
                        NavigationItem.Title = title ?? string.Empty;
                    }

                    OnPropertyChanged(Prism.UI.ContentView.TitleProperty);
                }
            }
        }
        private string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentView"/> class.
        /// </summary>
        public ContentView()
        {
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

            if (NavigationItem != null)
            {
                NavigationItem.Title = title ?? string.Empty;
                (menu as Controls.ActionMenu)?.Attach(this);
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

            (menu as Controls.ActionMenu)?.OnUnloaded();
        }

        /// <summary></summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = background.GetColor(View.Bounds.Width, View.Bounds.Height, null) ?? UIColor.White;
        }

        /// <summary></summary>
        public override void ViewWillLayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.ViewWillLayoutSubviews();
        }

        /// <summary></summary>
        public override void ViewDidLayoutSubviews()
        {
            if (background != null)
            {
                View.BackgroundColor = background.GetColor(View.Bounds.Width, View.Bounds.Height, null);
            }

            base.ViewDidLayoutSubviews();
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

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            View.BackgroundColor = background.GetColor(View.Bounds.Width, View.Bounds.Height, null) ?? UIColor.White;
        }
    }
}

