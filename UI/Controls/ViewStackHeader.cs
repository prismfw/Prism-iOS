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
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeViewStackHeader"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    public sealed class ViewStackHeader : INativeViewStackHeader, IVisualTreeObject
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
        /// Gets or sets the background for the header.
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
                    NavigationBar.BarTintColor = background.GetColor(NavigationBar.Bounds.Width, NavigationBar.Bounds.Height, OnBackgroundImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.BackgroundProperty);
                }
            }
        }
        private Brush background;
        
        /// <summary>
        /// Gets the visual children of the object.
        /// </summary>
        public object[] Children
        {
            get
            {
                ActionMenu currentMenu = null;
                for (int i = menuRefs.Count - 1; i >= 0; i--)
                {
                    var menu = menuRefs[i].Target as ActionMenu;
                    if (menu == null)
                    {
                        menuRefs.RemoveAt(i);
                        continue;
                    }
                    
                    if (menu.AttachedController?.NavigationItem == NavigationBar.TopItem)
                    {
                        currentMenu = menu;
                        break;
                    }
                }
                
                var children = new object[NavigationBar.Subviews.Length + (currentMenu == null ? 0 : 1)];
                NavigationBar.Subviews.CopyTo(children, 0);
                
                if (currentMenu != null)
                {
                    children[children.Length - 1] = currentMenu;
                }
                
                return children;
            }
        }

        /// <summary>
        /// Gets or sets the font to use for displaying the title text.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    SetForeground();
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the header.
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

                    var uifont = NavigationBar.TitleTextAttributes.Font;
                    var size = Title.GetStringSize(NavigationBar.Bounds.Size, uifont);
                    NavigationBar.TitleTextAttributes = new UIStringAttributes()
                    {
                        Font = uifont,
                        ForegroundColor = foreground.GetColor(size.Width, size.Height, OnForegroundImageLoaded)
                    };

                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(NavigationBar.Center.X - (NavigationBar.Bounds.Width / 2),
                    NavigationBar.Center.Y - (NavigationBar.Bounds.Height / 2), NavigationBar.Bounds.Width, NavigationBar.Bounds.Height + NavigationBar.Frame.Y);
            }
            set
            {
                value.Height -= NavigationBar.Frame.Y;
                NavigationBar.Bounds = new CGRect(NavigationBar.Bounds.Location, value.Size.GetCGSize());
                NavigationBar.Center = new CGPoint(value.X + (value.Width / 2), value.Y + (value.Height / 2) + NavigationBar.Frame.Y);

                if (background is LinearGradientBrush || background is ImageBrush)
                {
                    NavigationBar.BarTintColor = background.GetColor(NavigationBar.Bounds.Width, NavigationBar.Bounds.Height, null);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return NavigationBar.UserInteractionEnabled; }
            set
            {
                if (value != NavigationBar.UserInteractionEnabled)
                {
                    NavigationBar.UserInteractionEnabled = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the header is inset on top of the view stack content.
        /// A value of <c>false</c> indicates that the header offsets the view stack content.
        /// </summary>
        public bool IsInset
        {
            get { return true; }
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
        /// Gets the visual parent of the object.
        /// </summary>
        public object Parent
        {
            get { return NavigationBar.NextResponder; }
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
                    (renderTransform as Media.Transform)?.RemoveView(NavigationBar);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(NavigationBar);

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
        /// Gets or sets the title for the header.
        /// </summary>
        public string Title
        {
            get { return NavigationBar.TopItem?.Title ?? string.Empty; }
            set
            {
                var item = NavigationBar.TopItem;
                if (item != null && value != item.Title)
                {
                    item.Title = value ?? string.Empty;
                    title = item.Title;
                    OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.TitleProperty);
                }
            }
        }
        private string title;

        internal UINavigationBar NavigationBar { get; }

        // for reporting ActionMenus as children
        private readonly List<WeakReference> menuRefs = new List<WeakReference>();

        internal ViewStackHeader(UINavigationBar navBar)
        {
            NavigationBar = navBar;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            NavigationBar.SetNeedsLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            NavigationBar.SetNeedsLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return new Size(constraints.Width, NavigationBar.Frame.Bottom);
        }

        internal void SetMenu(ActionMenu menu)
        {
            if (menu == null)
            {
                menuRefs.RemoveAll(r => r.Target == null);
            }
            else if (!menuRefs.Any(r => r.Target == menu))
            {
                menuRefs.Add(new WeakReference(menu));
            }
        }

        internal void CheckTitle()
        {
            if (title != NavigationBar.TopItem?.Title)
            {
                title = NavigationBar.TopItem.Title;
                OnPropertyChanged(Prism.UI.Controls.ViewStackHeader.TitleProperty);
            }
        }

        internal void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }

            for (int i = menuRefs.Count - 1; i >= 0; i--)
            {
                var menu = menuRefs[i].Target as ActionMenu;
                if (menu == null)
                {
                    menuRefs.RemoveAt(i);
                    continue;
                }
                
                if (menu.AttachedController?.NavigationItem == NavigationBar.TopItem)
                {
                    menu.OnLoaded();
                    break;
                }
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

            for (int i = menuRefs.Count - 1; i >= 0; i--)
            {
                var menu = menuRefs[i].Target as ActionMenu;
                if (menu == null)
                {
                    menuRefs.RemoveAt(i);
                    continue;
                }
                
                if (menu.AttachedController?.NavigationItem == NavigationBar.TopItem)
                {
                    menu.OnUnloaded();
                    break;
                }
            }
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            NavigationBar.BarTintColor = background.GetColor(NavigationBar.Bounds.Width, NavigationBar.Bounds.Height, null);
        }

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            SetForeground();
        }

        private void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private void SetForeground()
        {
            var uifont = fontFamily.GetUIFont(fontSize, fontStyle);
            var size = Title.GetStringSize(NavigationBar.Bounds.Size, uifont);
            NavigationBar.TitleTextAttributes = new UIStringAttributes()
            {
                Font = uifont,
                ForegroundColor = foreground.GetColor(size.Width, size.Height, null) ?? UIColor.Black
            };
        }
    }
}

