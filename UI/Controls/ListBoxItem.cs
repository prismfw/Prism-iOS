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
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeListBoxItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeListBoxItem))]
    public class ListBoxItem : UITableViewCell, INativeListBoxItem, ITableViewChild
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;

        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets the accessory for the item.
        /// </summary>
        public new ListBoxItemAccessory Accessory
        {
            get
            {
                switch (base.Accessory)
                {
                    case UITableViewCellAccessory.DisclosureIndicator:
                        return ListBoxItemAccessory.Indicator;
                    case UITableViewCellAccessory.DetailButton:
                        return ListBoxItemAccessory.InfoButton;
                    case UITableViewCellAccessory.DetailDisclosureButton:
                        return ListBoxItemAccessory.Indicator | ListBoxItemAccessory.InfoButton;
                    default:
                        return ListBoxItemAccessory.None;
                }
            }
            set
            {
                if (value != Accessory)
                {
                    if (value == ListBoxItemAccessory.None)
                    {
                        base.Accessory = UITableViewCellAccessory.None;
                    }
                    else if ((value & ListBoxItemAccessory.Indicator) != 0)
                    {
                        base.Accessory = (value & ListBoxItemAccessory.InfoButton) != 0 ?
                            UITableViewCellAccessory.DetailDisclosureButton : UITableViewCellAccessory.DisclosureIndicator;
                    }
                    else if ((value & ListBoxItemAccessory.InfoButton) != 0)
                    {
                        base.Accessory = UITableViewCellAccessory.DetailButton;
                    }

                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.AccessoryProperty);
                }
            }
        }

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
        /// Gets or sets the background of the item.
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
                    BackgroundColor = value.GetColor(Bounds.Width, Bounds.Height, OnBackgroundImageChanged);
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the panel containing the content to be displayed by the item.
        /// </summary>
        public INativePanel ContentPanel
        {
            get { return contentPanel; }
            set
            {
                if (value == contentPanel)
                {
                    return;
                }

                for (int i = ContentView.Subviews.Length - 1; i >= 0; i--)
                {
                    ContentView.Subviews[i].RemoveFromSuperview();
                }

                contentPanel = value;
                if (value != null)
                {
                    ContentView.Add((UIView)value);
                }

                OnPropertyChanged(Prism.UI.Controls.ListBoxItem.ContentPanelProperty);
            }
        }
        private INativePanel contentPanel;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public new Rectangle Frame
        {
            get { return new Rectangle(Center.X - (Bounds.Width / 2), Center.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height); }
            set
            {
                Bounds = new CGRect(Bounds.Location, value.Size.GetCGSize());
                Center = new CGPoint(value.X + (value.Width / 2), value.Y + (value.Height / 2));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return UserInteractionEnabled; }
            set
            {
                if (value != UserInteractionEnabled)
                {
                    UserInteractionEnabled = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return Selected; }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (nfloat)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }

        /// <summary>
        /// Gets the containing <see cref="UITableView"/>.
        /// </summary>
        public UITableView Parent { get; private set; }

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
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(this);

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
        /// Gets or sets the background of the item when it is selected.
        /// </summary>
        public Brush SelectedBackground
        {
            get { return selectedBackground; }
            set
            {
                if (value != selectedBackground)
                {
                    (selectedBackground as ImageBrush).ClearImageHandler(OnSelectedBackgroundImageChanged);

                    selectedBackground = value;

                    if (selectedBackground == null)
                    {
                        SelectedBackgroundView = null;
                    }
                    else
                    {
                        SelectedBackgroundView = new UIView()
                        {
                            BackgroundColor = value.GetColor(Bounds.Width, Bounds.Height, OnSelectedBackgroundImageChanged)
                        };
                    }

                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.SelectedBackgroundProperty);
                }
            }
        }
        private Brush selectedBackground;

        /// <summary>
        /// Gets or sets the amount to indent the separator.
        /// </summary>
        public Thickness SeparatorIndentation
        {
            get { return base.SeparatorInset.GetThickness(); }
            set
            {
                if (value != SeparatorIndentation)
                {
                    SeparatorInset = value.GetInsets();
                    OnPropertyChanged(Prism.UI.Controls.ListBoxItem.SeparatorIndentationProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if (value != visibility)
                {
                    visibility = value;
                    Hidden = value != Visibility.Visible;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }
        private Visibility visibility;

        private CGPoint currentCenter;
        private CGSize currentSize;
        private double? parentWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxItem"/> class.
        /// </summary>
        public ListBoxItem()
            : base(UITableViewCellStyle.Default, ListBox.CurrentItemId)
        {
            Layer.MasksToBounds = true;
            BackgroundColor = null;
            MultipleTouchEnabled = true;
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            var oldCenter = Center;
            var oldBounds = Bounds;
            var width = Parent?.Bounds.Width ?? (ObjectRetriever.GetAgnosticObject(this.GetNextResponder<INativeListBox>()) as Visual)?.RenderSize.Width;

            var desiredSize = MeasureRequest(width != parentWidth, new Size(width ?? double.PositiveInfinity, double.PositiveInfinity));
            ArrangeRequest(oldCenter.X != currentCenter.X || oldBounds.Width != width || oldBounds.Height != desiredSize.Height, new Rectangle(0, Center.Y - (Bounds.Height / 2), width ?? 0, desiredSize.Height));

            parentWidth = width;
            base.LayoutSubviews();

            currentCenter = Center;
            if (currentSize != Bounds.Size)
            {
                BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null);

                if (SelectedBackgroundView != null)
                {
                    SelectedBackgroundView.BackgroundColor = selectedBackground.GetColor(Bounds.Width, Bounds.Height, null);
                }
            }
            currentSize = Bounds.Size;
        }

        /// <summary></summary>
        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview == null && IsLoaded)
            {
                OnUnloaded();
            }
            else if (Superview != null)
            {
                var parent = this.GetNextResponder<INativeVisual>();
                if (parent == null || parent.IsLoaded)
                {
                    OnLoaded();
                }
            }
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
                var isloaded = (NSNumber)change.ObjectForKey(NSObject.ChangeNewKey);
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

        /// <summary></summary>
        /// <param name="selected"></param>
        /// <param name="animated"></param>
        public override void SetSelected(bool selected, bool animated)
        {
            if (selected != Selected)
            {
                base.SetSelected(selected, animated && areAnimationsEnabled);
                OnPropertyChanged(Prism.UI.Controls.ListBoxItem.IsSelectedProperty);
            }
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerPressed(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesBegan(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerCanceled(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesCancelled(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerReleased(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesEnded(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerMoved(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesMoved(touches, evt);
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
            BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null);
        }

        private void OnSelectedBackgroundImageChanged(object sender, EventArgs e)
        {
            if (SelectedBackgroundView != null)
            {
                SelectedBackgroundView.BackgroundColor = selectedBackground.GetColor(Bounds.Width, Bounds.Height, null);
            }
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);

                foreach (var subview in ContentView.Subviews.Where(sv => sv is INativeVisual))
                {
                    try
                    {
                        subview.ObserveValue(new NSString(Visual.IsLoadedProperty.Name), this,
                            NSDictionary.FromObjectAndKey(new NSNumber(true), NSObject.ChangeNewKey), IntPtr.Zero);
                    }
                    catch { }
                }
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);

                foreach (var subview in ContentView.Subviews.Where(sv => sv is INativeVisual))
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

        void ITableViewChild.SetParent(UITableView parent)
        {
            Parent = parent;
        }
    }
}

