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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.iOS.UI.Controls;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.iOS.UI
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTabView"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabView))]
    public class TabView : UITabBarController, INativeTabView
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
        /// Occurs when a tab item is selected.
        /// </summary>
        public event EventHandler<NativeItemSelectedEventArgs> TabItemSelected;

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
                    TabBar.BarTintColor = background.GetColor(TabBar.Frame.Width, TabBar.Frame.Height, OnBackgroundImageLoaded);
                    OnPropertyChanged(Prism.UI.TabView.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the selected tab item.
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
                    TabBar.TintColor = foreground.GetColor(TabBar.Frame.Width, TabBar.Frame.Height, OnForegroundImageLoaded);
                    OnPropertyChanged(Prism.UI.TabView.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents
        /// the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get { return View.Frame.GetRectangle(); }
            set { View.Frame = value.GetCGRect(); }
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
        /// Gets or sets the zero-based index of the selected tab item.
        /// </summary>
        public new int SelectedIndex
        {
            get { return (int)base.SelectedIndex; }
            set
            {
                if (value != base.SelectedIndex)
                {
                    base.SelectedIndex = value;
                    OnPropertyChanged(Prism.UI.TabView.SelectedIndexProperty);
                }
            }
        }

        /// <summary>
        /// Gets a list of the tab items that are a part of the view.
        /// </summary>
        public IList TabItems
        {
            get { return tabItems; }
        }
        private readonly TabItemCollection tabItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabView"/> class.
        /// </summary>
        public TabView()
        {
            tabItems = new TabItemCollection(this);

            Delegate = new TabViewDelegate();
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
            return new Size(Math.Min(View.Frame.Width, constraints.Width), Math.Min(View.Frame.Height, constraints.Height));
        }

        /// <summary></summary>
        public override void ViewWillLayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.ViewWillLayoutSubviews();
        }

        /// <summary></summary>
        /// <param name="animated"></param>
        public override void ViewWillAppear(bool animated)
        {
            if (ViewControllers != null && ViewControllers.Length > 0)
            {
                var controllers = new UIViewController[ViewControllers.Length];
                for (int i = 0; i < ViewControllers.Length; i++)
                {
                    var controller = ViewControllers[i];
                    var tabItem = controller.TabBarItem as INativeTabItem;
                    if (tabItem?.Content != null && tabItem.Content != controller)
                    {
                        controllers[i] = tabItem.Content as UIViewController;
                    }
                    else
                    {
                        controllers[i] = controller;
                    }
                }
                
                SetViewControllers(controllers, false);
            }
        
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

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            TabBar.BarTintColor = background.GetColor(TabBar.Frame.Width, TabBar.Frame.Height, null);
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            TabBar.TintColor = foreground.GetColor(TabBar.Frame.Width, TabBar.Frame.Height, null);
        }

        private void OnTabItemSelected(NativeItemSelectedEventArgs e)
        {
            TabItemSelected(this, e);
        }

        private class TabViewDelegate : UITabBarControllerDelegate
        {
            private UIViewController previousController;
            
            public override bool ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
            {
                previousController = tabBarController.SelectedViewController;
                if (viewController == tabBarController.MoreNavigationController && tabBarController.MoreNavigationController.Delegate == null)
                {
                    tabBarController.MoreNavigationController.Delegate = new MoreViewDelegate();
                }

                return true;
            }

            public override void ViewControllerSelected(UITabBarController tabBarController, UIViewController viewController)
            {
                var tabView = tabBarController as TabView;
                if (tabView != null)
                {
                    if (previousController != viewController)
                    {
                        tabView.OnPropertyChanged(Prism.UI.TabView.SelectedIndexProperty);
                    }
                    
                    tabView.OnTabItemSelected(new NativeItemSelectedEventArgs(previousController == null ? null :
                        previousController.TabBarItem, viewController.TabBarItem));
                }
            }
        }

        private class MoreViewDelegate : UINavigationControllerDelegate
        {
            public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
            {
                if (viewController == navigationController.ViewControllers.FirstOrDefault())
                {
                    navigationController.NavigationBar.TopItem.RightBarButtonItem = null;
                }
            }
        }
    }
}
