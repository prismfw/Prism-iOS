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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI.Media;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeActionMenu"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeActionMenu))]
    public class ActionMenu : INativeActionMenu
    {
        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;
        
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
                    (background as ImageBrush).ClearImageHandler(OnBackgroundImageLoaded);
                
                    background = value;
                    
                    var imageBrush = background as ImageBrush;
                    if (imageBrush != null)
                    {
                        var image = imageBrush.BeginLoadingImage(OnBackgroundImageLoaded);
                        var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.BackgroundColor = image.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, imageBrush.Stretch);
                        }
                    }
                    else
                    {
                        var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.BackgroundColor = background.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
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
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the menu.
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
                    
                    var imageBrush = foreground as ImageBrush;
                    if (imageBrush != null)
                    {
                        var image = imageBrush.BeginLoadingImage(OnForegroundImageLoaded);
                        var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.TintColor = image.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, imageBrush.Stretch);
                        }
                        
                        var color = image.GetColor(30, 30, imageBrush.Stretch);
                        if (overflowColor == null && navigationItem?.RightBarButtonItem != null && !(navigationItem.RightBarButtonItem is INativeMenuItem))
                        {
                            navigationItem.RightBarButtonItem.TintColor = color;
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
                        var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
                        if (controller != null)
                        {
                            controller.View.TintColor = foreground.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
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
        /// Gets the amount that the menu is inset on top of its parent view.
        /// </summary>
        public Thickness Insets
        {
            get { return new Thickness(); }
        }

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
                    if (navigationItem != null && (oldValue < Items.Count || maxDisplayItems < Items.Count))
                    {
                        SetButtons();
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.MaxDisplayItemsProperty);
                }
            }
        }
        private int maxDisplayItems;
        
        /// <summary>
        /// Gets or sets the color of the overflow button.
        /// </summary>
        public UIColor OverflowColor
        {
            get { return overflowColor; }
            set
            {
                overflowColor = value;
                SetOverflowColor();
            }
        }
        private UIColor overflowColor;

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
                    if (Items.Count > maxDisplayItems && navigationItem != null)
                    {
                        var buttons = navigationItem.RightBarButtonItems;
                        buttons[0] = GetOverflowButton();
                        navigationItem.SetRightBarButtonItems(buttons, false);
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.ActionMenu.OverflowImageUriProperty);
                }
            }
        }
        private Uri overflowImageUri;
        
        private UINavigationItem navigationItem;
        private INativeVisual attachedParent;

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
            
                if (navigationItem == null)
                {
                    return;
                }
            
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewStartingIndex < maxDisplayItems || Items.Count > maxDisplayItems && (Items.Count - e.NewItems.Count <= maxDisplayItems))
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (e.NewStartingIndex < maxDisplayItems || e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewStartingIndex < maxDisplayItems)
                        {
                            SetButtons();
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        navigationItem.SetRightBarButtonItems(null, false);
                        break;
                }
            };
        }
        
        /// <summary>
        /// Attaches the menu to the specified parent.
        /// </summary>
        public void Attach(INativeVisual parent)
        {
            if (attachedParent != null)
            {
                Detach();
            }
            
            attachedParent = parent;
            navigationItem = (parent as UIViewController)?.NavigationItem;
            
            if (navigationItem != null)
            {
                SetButtons();
            }
        }
        
        /// <summary>
        /// Detaches the menu from its current parent.
        /// </summary>
        public void Detach()
        {
            navigationItem = null;
            attachedParent = null;
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
            controller.View.BackgroundColor = background.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
            controller.View.TintColor = foreground.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
            if (controller.PopoverPresentationController != null)
            {
                controller.PopoverPresentationController.BarButtonItem = (UIBarButtonItem)sender;
            }
            
            await (attachedParent as UIViewController)?.PresentViewControllerAsync(controller, attachedParent.AreAnimationsEnabled);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
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
            
            button.TintColor = overflowColor ?? foreground.GetColor(30, 30, null);
            button.Clicked += OnOverflowButtonClicked;
            return button;
        }
        
        private void OnItemPropertyChanged(object sender, FrameworkPropertyChangedEventArgs e)
        {
            if (e.Property == Prism.UI.Controls.MenuItem.ForegroundProperty)
            {
                SetItemForeground(sender as INativeMenuItem);
            }
        }
        
        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
            if (controller != null)
            {
                controller.View.BackgroundColor = background.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
            }
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            var controller = (attachedParent as UIViewController)?.PresentedViewController as UIAlertController;
            if (controller != null)
            {
                controller.View.TintColor = foreground.GetColor(controller.View.Frame.Width, controller.View.Frame.Height, null);
            }
            
            SetOverflowColor();
            foreach (var item in Items.OfType<INativeMenuItem>())
            {
                SetItemForeground(item);
            }
        }
        
        private void OnOverflowImageLoaded(object sender, EventArgs e)
        {
            if (navigationItem?.RightBarButtonItem != null && !(navigationItem.RightBarButtonItem is INativeMenuItem))
            {
                navigationItem.RightBarButtonItem.TintColor = overflowColor ?? foreground.GetColor(30, 30, null);
            }
        }
        
        private void SetButtons()
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
            
            navigationItem.SetRightBarButtonItems(buttons, false);
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
            if (navigationItem?.RightBarButtonItem != null && !(navigationItem.RightBarButtonItem is INativeMenuItem))
            {
                navigationItem.RightBarButtonItem.TintColor = overflowColor ?? foreground.GetColor(30, 30, null);
            }
        }
    }
}

