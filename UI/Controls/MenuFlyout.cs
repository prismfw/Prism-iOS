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
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Represents an iOS implementation for an <see cref="INativeMenuFlyout"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMenuFlyout))]
    public class MenuFlyout : INativeMenuFlyout
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

                    if (Controller != null)
                    {
                        var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
                        if (size.Width == 0 && size.Height == 0)
                        {
                            size = Controller.PreferredContentSize;
                        }
                        SetBackground(background.GetColor(size.Width, size.Height, OnBackgroundImageChanged));
                    }

                    OnPropertyChanged(FlyoutBase.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the menu.
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

                    var imageBrush = foreground as ImageBrush;
                    if (imageBrush != null)
                    {
                        var image = imageBrush.BeginLoadingImage(OnForegroundImageChanged);
                        if (Controller != null)
                        {
                            var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
                            if (size.Width == 0 && size.Height == 0)
                            {
                                size = Controller.PreferredContentSize;
                            }

                            Controller.View.TintColor = image.GetColor(size.Width, size.Height, imageBrush.Stretch);
                        }
                    }
                    else
                    {
                        if (Controller != null)
                        {
                            var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
                            if (size.Width == 0 && size.Height == 0)
                            {
                                size = Controller.PreferredContentSize;
                            }

                            Controller.View.TintColor = foreground.GetColor(size.Width, size.Height, null);
                        }
                    }

                    OnPropertyChanged(Prism.UI.Controls.MenuFlyout.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return isHitTestVisible; }
            set
            {
                if (value != isHitTestVisible)
                {
                    isHitTestVisible = value;
                    if (Controller != null)
                    {
                        Controller.View.UserInteractionEnabled = isHitTestVisible;
                    }

                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }
        private bool isHitTestVisible = true;

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets a collection of the items within the menu.
        /// </summary>
        public IList Items { get; }

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
                    (renderTransform as Media.Transform)?.RemoveView(Controller?.View);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(Controller?.View);

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
        /// Gets the controller that is presenting the menu.
        /// </summary>
        protected UIAlertController Controller { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuFlyout"/> class.
        /// </summary>
        public MenuFlyout()
        {
            Items = new ObservableCollection<INativeMenuItem>();
            ((ObservableCollection<INativeMenuItem>)Items).CollectionChanged += (o, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add && Controller != null && e.NewStartingIndex == Controller.Actions.Length)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var item = e.NewItems[i] as MenuButton;
                        if (item != null)
                        {
                            Controller.AddAction(item.GetAlertAction());
                        }
                    }
                }
                else
                {
                    Controller = null;
                }
            };
        }

        /// <summary>
        /// Dismisses the flyout.
        /// </summary>
        public void Hide()
        {
            if (Controller?.PresentingViewController != null)
            {
                Controller.PresentingViewController.DismissViewController(areAnimationsEnabled, () => Closed(this, EventArgs.Empty));
            }
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return Controller?.View.Bounds.Size.GetSize() ?? Size.Empty;
        }

        /// <summary>
        /// Presents the flyout and positions it relative to the specified placement target.
        /// </summary>
        /// <param name="placementTarget">The object to use as the flyout's placement target.</param>
        public void ShowAt(object placementTarget)
        {
            if (Controller == null)
            {
                Controller = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i] as MenuButton;
                    if (item != null)
                    {
                        Controller.AddAction(item.GetAlertAction());
                    }
                }
            }

            if (Controller.PopoverPresentationController != null && !(Controller.PopoverPresentationController.Delegate is FlyoutDelegate))
            {
                Controller.PopoverPresentationController.Delegate = new FlyoutDelegate(this);
            }

            Controller.View.UserInteractionEnabled = isHitTestVisible;

            var view = placementTarget as UIView;
            if (view != null)
            {
                if (Controller.PopoverPresentationController != null)
                {
                    Controller.PopoverPresentationController.SourceView = view;
                    Controller.PopoverPresentationController.SourceRect = new CGRect(CGPoint.Empty, view.Frame.Size);
                }

                view.GetNextResponder<UIViewController>()?.PresentViewController(Controller, areAnimationsEnabled, () =>
                {
                    OnLoaded();
                    Opened(this, EventArgs.Empty);
                });
            }
            else
            {
                var button = placementTarget as UIBarButtonItem;
                if (button != null)
                {
                    if (Controller.PopoverPresentationController != null)
                    {
                        Controller.PopoverPresentationController.BarButtonItem = button;
                    }

                    UIApplication.SharedApplication.KeyWindow.RootViewController?.PresentViewController(Controller, areAnimationsEnabled, () =>
                    {
                        OnLoaded();
                        Opened(this, EventArgs.Empty);
                    });
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

        private void OnBackgroundImageChanged(object sender, EventArgs e)
        {
            if (Controller != null)
            {
                var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
                if (size.Width == 0 && size.Height == 0)
                {
                    size = Controller.View.Frame.Size;
                }

                SetBackground(background.GetColor(size.Width, size.Height, null));
            }
        }

        private void OnDismiss()
        {
            Closed(this, EventArgs.Empty);
            OnUnloaded();
        }

        private void OnForegroundImageChanged(object sender, EventArgs e)
        {
            if (Controller != null)
            {
                var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
                if (size.Width == 0 && size.Height == 0)
                {
                    size = Controller.View.Frame.Size;
                }

                Controller.View.TintColor = foreground.GetColor(size.Width, size.Height, null);
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

            var size = Controller.PopoverPresentationController?.PresentedView?.Frame.Size ?? CGSize.Empty;
            if (size.Width == 0 && size.Height == 0)
            {
                size = Controller.View.Frame.Size;
            }

            SetBackground(background.GetColor(size.Width, size.Height, null));
            Controller.View.TintColor = foreground.GetColor(size.Width, size.Height, null);

            MeasureRequest(false, null);
            ArrangeRequest(false, null);
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

        private void SetBackground(UIColor color)
        {
            if (Controller.PopoverPresentationController != null)
            {
                Controller.PopoverPresentationController.BackgroundColor = color;
            }
            else
            {
                Controller.View.BackgroundColor = color;
            }
        }

        private class FlyoutDelegate : UIPopoverPresentationControllerDelegate
        {
            private readonly WeakReference flyoutRef;

            public FlyoutDelegate(MenuFlyout flyout)
            {
                flyoutRef = new WeakReference(flyout);
            }

            public override void DidDismissPopover(UIPopoverPresentationController popoverPresentationController)
            {
                (flyoutRef.Target as MenuFlyout)?.OnDismiss();
            }

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller, UITraitCollection traitCollection)
            {
                return UIModalPresentationStyle.None;
            }

            public override void PrepareForPopoverPresentation(UIPopoverPresentationController popoverPresentationController)
            {
                var flyout = flyoutRef.Target as MenuFlyout;
                if (flyout != null)
                {
                    popoverPresentationController.PermittedArrowDirections = flyout.placement.GetPopoverArrowDirection();
                }
            }
        }
    }
}

