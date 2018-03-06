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
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI.Media;
using Prism.UI;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeActionMenu"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeActionMenu))]
    public class ActionMenu : INativeActionMenu, IVisualTreeObject
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
        /// Gets or sets the background for the menu.
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
                    
                    var imageBrush = background as ImageBrush;
                    if (imageBrush != null)
                    {
                        var image = imageBrush.BeginLoadingImage(OnBackgroundImageChanged);
                        var controller = AttachedController?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.BackgroundColor = image.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, imageBrush.Stretch);
                        }
                    }
                    else
                    {
                        var controller = AttachedController?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.BackgroundColor = background.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, null);
                        }
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.BackgroundProperty);
                }
            }
        }
        private Brush background;
        
        /// <summary>
        /// Gets or sets the title of the menu's Cancel button, if one exists.
        /// </summary>
        public string CancelButtonTitle
        {
            get { return cancelButtonTitle; }
            set
            {
                if (value != cancelButtonTitle)
                {
                    cancelButtonTitle = value;
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.CancelButtonTitleProperty);
                }
            }
        }
        private string cancelButtonTitle;
        
        /// <summary>
        /// Gets the visual children of the object.
        /// </summary>
        public object[] Children
        {
            get
            {
                var children = new object[AttachedController?.NavigationItem?.RightBarButtonItems?.Length ?? 0];
                if (children.Length > 0)
                {
                    AttachedController.NavigationItem.RightBarButtonItems.CopyTo(children, 0);
                }
                
                return children;
            }
        }

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
                        var controller = AttachedController?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.TintColor = image.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, imageBrush.Stretch);
                        }
                        
                        var color = image.GetColor(30, 30, imageBrush.Stretch);
                        if (overflowBrush == null && AttachedController?.NavigationItem?.RightBarButtonItem != null &&
                            !(AttachedController.NavigationItem.RightBarButtonItem is INativeMenuItem))
                        {
                            AttachedController.NavigationItem.RightBarButtonItem.TintColor = color;
                        }
                        
                        foreach (var item in Items.OfType<INativeMenuItem>().Where(i => i.Foreground == null))
                        {
                            var button = item as UIBarButtonItem;
                            if (button != null)
                            {
                                button.TintColor = color;
                            }
                        }
                    }
                    else
                    {
                        var controller = AttachedController?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.TintColor = foreground.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, null);
                        }
                        
                        SetOverflowColor();
                        foreach (var item in Items.OfType<INativeMenuItem>())
                        {
                            SetItemForeground(item);
                        }
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
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

                    var controller = AttachedController?.PresentedViewController as UIAlertController;
                    if (controller != null)
                    {
                        controller.View.UserInteractionEnabled = isHitTestVisible;
                    }

                    if (AttachedController?.NavigationItem?.RightBarButtonItems != null)
                    {
                        foreach (var button in AttachedController.NavigationItem.RightBarButtonItems)
                        {
                            var view = GetViewForItem(button);
                            if (view != null)
                            {
                                view.UserInteractionEnabled = isHitTestVisible;
                            }
                        }
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
        /// Gets or sets the maximum number of menu items that can be displayed before they are placed into an overflow menu.
        /// </summary>
        public int MaxDisplayItems
        {
            get { return maxDisplayItems; }
            set
            {
                if (value != maxDisplayItems)
                {
                    int oldValue = maxDisplayItems;
                    maxDisplayItems = value;
                    if (AttachedController?.NavigationItem != null && (oldValue < Items.Count || maxDisplayItems < Items.Count))
                    {
                        SetButtons(areAnimationsEnabled);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.MaxDisplayItemsProperty);
                }
            }
        }
        private int maxDisplayItems;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the color of the overflow button.
        /// </summary>
        public Brush OverflowBrush
        {
            get { return overflowBrush; }
            set
            {
                overflowBrush = value;
                SetOverflowColor();
            }
        }
        private Brush overflowBrush;

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> of the image to use for representing the overflow menu when one is present.
        /// </summary>
        public Uri OverflowImageUri
        {
            get { return overflowImageUri; }
            set
            {
                if (value != overflowImageUri)
                {
                    overflowImageUri = value;
                    if (Items.Count > maxDisplayItems && AttachedController?.NavigationItem != null)
                    {
                        SetButtons(false);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.OverflowImageUriProperty);
                }
            }
        }
        private Uri overflowImageUri;
        
        /// <summary>
        /// Gets the visual parent of the object.
        /// </summary>
        public object Parent
        {
            get
            {
                if (AttachedController?.NavigationController == null)
                {
                    return null;
                }
                
                var viewStack = AttachedController.NavigationController as INativeViewStack;
                if (viewStack == null)
                {
                    return AttachedController.NavigationController.NavigationBar;
                }
                
                return viewStack.Header;
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
                    if (AttachedController?.NavigationItem?.RightBarButtonItems != null)
                    {
                        foreach (var button in AttachedController.NavigationItem.RightBarButtonItems)
                        {
                            var view = GetViewForItem(button);
                            if (view != null)
                            {
                                (renderTransform as Media.Transform)?.RemoveView(view);
                            }
                        }
                    }
                    
                    renderTransform = value;
                    
                    if (AttachedController?.NavigationItem?.RightBarButtonItems != null)
                    {
                        foreach (var button in AttachedController.NavigationItem.RightBarButtonItems)
                        {
                            var view = GetViewForItem(button);
                            if (view != null)
                            {
                                (renderTransform as Media.Transform)?.AddView(view);
                            }
                        }
                    }

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }
        
        internal UIViewController AttachedController { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMenu"/> class.
        /// </summary>
        public ActionMenu()
        {
            Items = new ObservableCollection<INativeMenuItem>();
            ((ObservableCollection<INativeMenuItem>)Items).CollectionChanged += (o, e) =>
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<INativeMenuItem>())
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;

                        var view = GetViewForItem(item as UIBarButtonItem);
                        if (view != null)
                        {
                            view.UserInteractionEnabled = true;
                            (renderTransform as Media.Transform)?.RemoveView(view);
                        }
                    }
                }
            
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<INativeMenuItem>())
                    {
                        item.PropertyChanged -= OnItemPropertyChanged;
                        item.PropertyChanged += OnItemPropertyChanged;
                        
                        SetItemForeground(item);
                    }
                }
            
                if (AttachedController?.NavigationItem == null)
                {
                    return;
                }
            
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewStartingIndex < maxDisplayItems || Items.Count > maxDisplayItems && (Items.Count - e.NewItems.Count <= maxDisplayItems))
                        {
                            SetButtons(areAnimationsEnabled);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (e.NewStartingIndex < maxDisplayItems || e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons(areAnimationsEnabled);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons(areAnimationsEnabled);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewStartingIndex < maxDisplayItems)
                        {
                            SetButtons(areAnimationsEnabled);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        AttachedController.NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[0], areAnimationsEnabled);
                        break;
                }
            };
        }
        
        /// <summary>
        /// Attaches the menu to the specified parent.
        /// </summary>
        public void Attach(UIViewController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (AttachedController != controller)
            {
                if (AttachedController != null)
                {
                    throw new InvalidOperationException("Menu instance is already assigned to another object.");
                }
                
                AttachedController = controller;
                SetButtons(false);
            }

            (Parent as ViewStackHeader)?.SetMenu(this);

            var visual = Parent as INativeVisual;
            if (visual != null && visual.IsLoaded)
            {
                OnLoaded();
            }
        }
        
        /// <summary>
        /// Detaches the menu from its current parent.
        /// </summary>
        public void Detach()
        {
            if (AttachedController != null)
            {
                (Parent as ViewStackHeader)?.SetMenu(null);

                AttachedController.NavigationItem?.SetRightBarButtonItems(new UIBarButtonItem[0], false);
                AttachedController = null;
                
                OnUnloaded();
            }
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            ArrangeRequest(false, null);
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            MeasureRequest(false, null);
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            var size = new Size();
            if (AttachedController?.NavigationItem?.RightBarButtonItems != null)
            {
                foreach (var button in AttachedController.NavigationItem.RightBarButtonItems)
                {
                    var view = GetViewForItem(button);
                    if (view != null)
                    {
                        size.Width += view.Frame.Width;
                        size.Height = Math.Max(size.Height, view.Frame.Height);
                    }
                }
            }

            return size;
        }

        private async void OnOverflowButtonClicked(object sender, EventArgs e)
        {
            var controller = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            for (int i = maxDisplayItems; i < Items.Count; i++)
            {
                var item = Items[i] as MenuButton;
                if (item != null)
                {
                    controller.AddAction(item.GetAlertAction());
                }
            }
            
            controller.AddAction(UIAlertAction.Create(cancelButtonTitle, UIAlertActionStyle.Cancel, null));
            controller.PopoverPresentationController.BackgroundColor = background.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, null);
            controller.View.TintColor = foreground.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, null);
            controller.View.UserInteractionEnabled = isHitTestVisible;

            if (controller.PopoverPresentationController != null)
            {
                controller.PopoverPresentationController.BarButtonItem = (UIBarButtonItem)sender;
            }
            
            await AttachedController?.PresentViewControllerAsync(controller, areAnimationsEnabled);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        internal void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        internal void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }

        private UIBarButtonItem GetOverflowButton()
        {
            UIBarButtonItem button;
            if (overflowImageUri != null)
            {
                button = new UIBarButtonItem(overflowImageUri.IsFile || !overflowImageUri.IsAbsoluteUri ?
                    UIImage.FromFile(overflowImageUri.OriginalString) :
                    UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(overflowImageUri.OriginalString))),
                    UIBarButtonItemStyle.Plain, null);
            }
            else
            {
                button = new UIBarButtonItem(UIBarButtonSystemItem.Action);
            }

            button.TintColor = (overflowBrush ?? foreground).GetColor(30, 30, null);
            button.Clicked += OnOverflowButtonClicked;
            return button;
        }

        private UIView GetViewForItem(UIBarButtonItem item)
        {
            return item?.ValueForKey(new NSString("view")) as UIView;
        }

        private void OnBackgroundImageChanged(object sender, EventArgs e)
        {
            var controller = AttachedController?.PresentedViewController as UIAlertController;
            if (controller != null)
            {
                var size = controller.PopoverPresentationController.PresentedView?.Frame.Size ?? CGSize.Empty;
                if (size.Width == 0 && size.Height == 0)
                {
                    size = controller.View.Frame.Size;
                }

                controller.PopoverPresentationController.BackgroundColor = background.GetColor(size.Width, size.Height, null);
            }
        }

        private void OnForegroundImageChanged(object sender, EventArgs e)
        {
            var controller = AttachedController?.PresentedViewController as UIAlertController;
            if (controller != null)
            {
                controller.View.TintColor = foreground.GetColor(controller.View.Bounds.Width, controller.View.Bounds.Height, null);
            }
            
            SetOverflowColor();
            foreach (var item in Items.OfType<INativeMenuItem>())
            {
                SetItemForeground(item);
            }
        }

        private void OnItemPropertyChanged(object sender, FrameworkPropertyChangedEventArgs e)
        {
            if (e.Property == Prism.UI.Controls.MenuItem.ForegroundProperty)
            {
                SetItemForeground(sender as INativeMenuItem);
            }
        }

        private void OnOverflowImageChanged(object sender, EventArgs e)
        {
            if (AttachedController?.NavigationItem?.RightBarButtonItem != null && !(AttachedController.NavigationItem.RightBarButtonItem is INativeMenuItem))
            {
                AttachedController.NavigationItem.RightBarButtonItem.TintColor = (overflowBrush ?? foreground).GetColor(30, 30, null);
            }
        }

        private void SetButtons(bool animate)
        {
            bool hasOverflow = maxDisplayItems < Items.Count;
            var items = ((ObservableCollection<INativeMenuItem>)Items).Take(hasOverflow ? maxDisplayItems : Items.Count).OfType<UIBarButtonItem>();
            var itemsEnumerator = items.GetEnumerator();
            
            var buttons = new UIBarButtonItem[items.Count() + (hasOverflow ? 1 : 0)];
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == buttons.Length - 1 && hasOverflow)
                {
                    buttons[0] = GetOverflowButton();
                }
                else if (itemsEnumerator.MoveNext())
                {
                    buttons[(buttons.Length - 1) - i] = itemsEnumerator.Current;
                }
            }

            AttachedController?.NavigationItem?.SetRightBarButtonItems(buttons, animate);

            for (int i = 0; i < buttons.Length; i++)
            {
                var view = GetViewForItem(buttons[i]);
                if (view != null)
                {
                    view.UserInteractionEnabled = isHitTestVisible;
                    (renderTransform as Media.Transform)?.AddView(view);
                }
            }

            MeasureRequest(false, null);
            ArrangeRequest(false, null);
        }
        
        private void SetItemForeground(INativeMenuItem item)
        {
            if (item != null && item.Foreground == null)
            {
                var button = item as UIBarButtonItem;
                if (button != null)
                {
                    button.TintColor = foreground.GetColor(30, 30, null);
                }
            }
        }
        
        private void SetOverflowColor()
        {
            if (AttachedController?.NavigationItem?.RightBarButtonItem != null && !(AttachedController.NavigationItem.RightBarButtonItem is INativeMenuItem))
            {
                AttachedController.NavigationItem.RightBarButtonItem.TintColor = (overflowBrush ?? foreground).GetColor(30, 30, null);
            }
        }
    }
}

