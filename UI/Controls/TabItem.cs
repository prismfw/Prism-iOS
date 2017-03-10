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
using System.Linq;
using Foundation;
using UIKit;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeTabItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeTabItem))]
    public class TabItem : UITabBarItem, INativeTabItem, IVisualTreeObject
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
        /// Gets or sets the object that acts as the content of the item.
        /// This is typically an <see cref="IView"/> or <see cref="INativeViewStack"/> instance.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                content = value;

                var tabView = UIApplication.SharedApplication.KeyWindow.RootViewController as UITabBarController;
                if (tabView == null)
                {
                    var splitView = UIApplication.SharedApplication.KeyWindow.RootViewController as UISplitViewController;
                    if (splitView != null)
                    {
                        tabView = splitView.ViewControllers.FirstOrDefault() as UITabBarController;
                    }
                }

                if (tabView != null)
                {
                    var controllers = tabView.ViewControllers;
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        var controller = controllers[i];
                        if (controller.TabBarItem == this)
                        {
                            controller = content as UIViewController;
                            controller.TabBarItem = this;
                            controllers[i] = controller;
                            tabView.ViewControllers = controllers;
                            break;
                        }
                    }
                }
            }
        }
        private object content;

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
                    var attributes = GetTitleTextAttributes(UIControlState.Normal);
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(attributes.Font?.PointSize ?? 0, attributes.Font?.GetFontStyle() ?? FontStyle.Normal),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the title text.
        /// </summary>
        public double FontSize
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).Font.PointSize; }
            set
            {
                var attributes = GetTitleTextAttributes(UIControlState.Normal);
                if (value != attributes.Font.PointSize)
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(value, attributes.Font.GetFontStyle()),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontSizeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the style with which to render the title text.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return GetTitleTextAttributes(UIControlState.Normal).Font.GetFontStyle(); }
            set
            {
                var attributes = GetTitleTextAttributes(UIControlState.Normal);
                if (value != attributes.Font.GetFontStyle())
                {
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = fontFamily.GetUIFont(attributes.Font.PointSize, value),
                        TextColor = attributes.TextColor
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.FontStyleProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the title.
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

                    var attributes = GetTitleTextAttributes(UIControlState.Normal);
                    SetTitleTextAttributes(new UITextAttributes()
                    {
                        Font = attributes.Font,
                        TextColor = foreground.GetColor(View?.Frame.Width ?? 1, attributes.Font.LineHeight, OnForegroundImageLoaded) ?? UIColor.Gray,
                        TextShadowColor = attributes.TextShadowColor,
                        TextShadowOffset = attributes.TextShadowOffset
                    }, UIControlState.Normal);

                    OnPropertyChanged(Prism.UI.Controls.TabItem.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public Rectangle Frame { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="INativeImageSource"/> for an image to display with the item.
        /// </summary>
        public new INativeImageSource Image
        {
            get { return image; }
            set
            {
                if (value != image)
                {
                    image.ClearImageHandler(OnImageLoaded);

                    image = value;
                    base.Image = image.BeginLoadingImage(OnImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.TabItem.ImageProperty);
                }
            }
        }
        private INativeImageSource image;
        
        /// <summary>
        /// Gets or sets a value indicating whether the user can interact with the item.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;
                    OnPropertyChanged(Prism.UI.Controls.TabItem.IsEnabledProperty);
                }
            }
        }

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
                    if (View != null)
                    {
                        View.UserInteractionEnabled = value;
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
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets the visual parent of the object.
        /// </summary>
        public object Parent
        {
            get { return View?.GetNextResponder<INativeTabView>(); }
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
        /// Gets or sets the title for the item.
        /// </summary>
        public override string Title
        {
            get { return base.Title; }
            set
            {
                if (value != base.Title)
                {
                    base.Title = value;
                    OnPropertyChanged(Prism.UI.Controls.TabItem.TitleProperty);
                }
            }
        }
        
        private UIView View
        {
            get { return ValueForKey(new NSString("view")) as UIView; }
        }

        object[] IVisualTreeObject.Children { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabItem"/> class.
        /// </summary>
        public TabItem()
        {
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
            return View?.Frame.Size.GetSize() ?? new Size();
        }

        /// <summary></summary>
        /// <param name="keyPath"></param>
        /// <param name="ofObject"></param>
        /// <param name="change"></param>
        /// <param name="context"></param>
        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == Visual.IsLoadedProperty.Name)
            {
                var isloaded = (NSNumber)change.ObjectForKey(ChangeNewKey);
                if (isloaded.BoolValue)
                {
                    OnLoaded();
                }
                else
                {
                    OnUnloaded();
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

        private void OnForegroundImageLoaded(object sender, EventArgs e)
        {
            var font = GetTitleTextAttributes(UIControlState.Normal).Font;
            SetTitleTextAttributes(new UITextAttributes()
            {
                Font = font,
                TextColor = foreground.GetColor(View?.Frame.Width ?? 1, font.LineHeight, null) ?? UIColor.Gray
            }, UIControlState.Normal);
        }

        private void OnImageLoaded(object sender, EventArgs e)
        {
            base.Image = (sender as INativeImageSource).GetImageSource();
        }
 
        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
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
    }
}

