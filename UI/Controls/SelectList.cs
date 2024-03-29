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
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using Prism.Utilities;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeSelectList"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSelectList))]
    public class SelectList : UIControl, INativeSelectList
    {
        /// <summary>
        /// Occurs when the control receives focus.
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the control loses focus.
        /// </summary>
        public event EventHandler LostFocus;

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
        /// Occurs when the selection of the select list is changed.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

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
        /// Gets or sets the background for the control.
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
                    BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, OnBackgroundImageChanged);
                    OnPropertyChanged(Control.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the border of the control.
        /// </summary>
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != borderBrush)
                {
                    (borderBrush as ImageBrush).ClearImageHandler(OnBorderImageChanged);

                    borderBrush = value;
                    Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, OnBorderImageChanged)?.CGColor ?? UIColor.Black.CGColor;
                    OnPropertyChanged(Control.BorderBrushProperty);
                }
            }
        }
        private Brush borderBrush;

        /// <summary>
        /// Gets or sets the width of the border around the control.
        /// </summary>
        public double BorderWidth
        {
            get { return Layer.BorderWidth; }
            set
            {
                if (value != Layer.BorderWidth)
                {
                    Layer.BorderWidth = (nfloat)value;
                    OnPropertyChanged(Control.BorderWidthProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can become first responder.
        /// </summary>
        public override bool CanBecomeFirstResponder
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a display item for the select list.
        /// </summary>
        public SelectListDisplayItemRequestHandler DisplayItemRequest { get; set; }

        /// <summary>
        /// Gets or sets the font to use for displaying the text in the control.
        /// </summary>
        public object FontFamily
        {
            get { return fontFamily; }
            set
            {
                if (value != fontFamily)
                {
                    fontFamily = value as Media.FontFamily;
                    OnPropertyChanged(Control.FontFamilyProperty);
                }
            }
        }
        private Media.FontFamily fontFamily;

        /// <summary>
        /// Gets or sets the size of the text in the control.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                if (value != fontSize)
                {
                    fontSize = value;
                    OnPropertyChanged(Control.FontSizeProperty);
                }
            }
        }
        private double fontSize;

        /// <summary>
        /// Gets or sets the style with which to render the text in the control.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                if (value != fontStyle)
                {
                    fontStyle = value;
                    OnPropertyChanged(Control.FontStyleProperty);
                }
            }
        }
        private FontStyle fontStyle;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the foreground content of the control.
        /// </summary>
        public Brush Foreground
        {
            get { return foreground; }
            set
            {
                if (value != foreground)
                {
                    var displayObj = this.GetSubview<UIView>(sv => sv is INativeVisual);
                    var panel = displayObj as INativePanel;
                    if (panel != null)
                    {
                        foreach (var child in panel.Children)
                        {
                            var label = child as INativeLabel;
                            if (label != null && (label.Foreground == null || label.Foreground == foreground))
                            {
                                label.Foreground = value;
                            }
                            else
                            {
                                var control = child as INativeControl;
                                if (control != null && (control.Foreground == null || control.Foreground == foreground))
                                {
                                    control.Foreground = value;
                                }
                            }
                        }
                    }
                    else
                    {
                        var label = displayObj as INativeLabel;
                        if (label != null && (label.Foreground == null || label.Foreground == foreground))
                        {
                            label.Foreground = value;
                        }
                        else
                        {
                            var control = displayObj as INativeControl;
                            if (control != null && (control.Foreground == null || control.Foreground == foreground))
                            {
                                control.Foreground = value;
                            }
                        }
                    }

                    foreground = value;
                    OnPropertyChanged(Control.ForegroundProperty);
                }
            }
        }
        private Brush foreground;

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
        /// Gets or sets a value indicating whether the user can interact with the control.
        /// </summary>
        public bool IsEnabled
        {
            get { return base.Enabled; }
            set
            {
                if (value != base.Enabled)
                {
                    base.Enabled = value;
                    OnPropertyChanged(Control.IsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control has focus.
        /// </summary>
        public bool IsFocused { get; private set; }

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
        /// Gets or sets a value indicating whether the list is open for selection.
        /// </summary>
        public bool IsOpen
        {
            get { return selectListController != null && selectListController.View.Window != null; }
            set
            {
                if (value != IsOpen)
                {
                    if (selectListController == null)
                    {
                        selectListController = new SelectListViewController(this);
                    }

                    var navController = this.GetNextResponder<UINavigationController>();
                    if (navController == null)
                    {
                        if (selectListController.PresentingViewController == null && value)
                        {
                            var viewController = this.GetNextResponder<UIViewController>();
                            if (viewController == null)
                            {
                                Logger.Warn("Unable to open selection list.  There is no suitable UIViewController to present it.");
                                return;
                            }

                            viewController.PresentViewController(selectListController, areAnimationsEnabled, null);
                        }
                        else if (!value)
                        {
                            selectListController.PresentingViewController.DismissViewController(areAnimationsEnabled, null);
                        }
                    }
                    else if (value)
                    {
                        navController.PushViewController(selectListController, areAnimationsEnabled);
                    }
                    else if (navController.TopViewController == selectListController)
                    {
                        navController.PopViewController(areAnimationsEnabled);
                    }
                    else
                    {
                        var controllers = navController.ViewControllers.ToList();
                        controllers.RemoveAll(vc => vc == selectListController);
                        navController.SetViewControllers(controllers.ToArray(), false);
                    }

                    OnPropertyChanged(Prism.UI.Controls.SelectList.IsOpenProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a list of the items that make up the select list.
        /// </summary>
        public IList Items
        {
            get { return items; }
            set
            {
                if (value != items)
                {
                    items = value;
                    OnPropertyChanged(Prism.UI.Controls.SelectList.ItemsProperty);
                    RefreshDisplayItem();
                }
            }
        }
        private IList items;

        /// <summary>
        /// Gets or sets the background of the selection list.
        /// </summary>
        public Brush ListBackground
        {
            get { return listBackground; }
            set
            {
                if (value != listBackground)
                {
                    listBackground = value;
                    if (selectListController != null)
                    {
                        selectListController.Background = listBackground;
                    }

                    OnPropertyChanged(Prism.UI.Controls.SelectList.ListBackgroundProperty);
                }
            }
        }
        private Brush listBackground;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a list item for an object in the select list.
        /// </summary>
        public SelectListListItemRequestHandler ListItemRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the separators in the selection list, if applicable.
        /// </summary>
        public Brush ListSeparatorBrush
        {
            get { return listSeparatorBrush; }
            set
            {
                if (value != listSeparatorBrush)
                {
                    listSeparatorBrush = value;
                    if (selectListController != null)
                    {
                        selectListController.SeparatorBrush = listSeparatorBrush;
                    }

                    OnPropertyChanged(Prism.UI.Controls.SelectList.ListSeparatorBrushProperty);
                }
            }
        }
        private Brush listSeparatorBrush;

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
        /// Gets or sets the zero-based index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { OnSelected(value); }
        }
        private int selectedIndex;

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

        private CGSize currentSize;
        private SelectListViewController selectListController;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectList"/> class.
        /// </summary>
        public SelectList()
        {
            MultipleTouchEnabled = true;

            TouchDown += (sender, e) => BecomeFirstResponder();
            TouchUpInside += (sender, e) => IsOpen = true;
        }

        /// <summary>
        /// Attempts to set focus to the control.
        /// </summary>
        public void Focus()
        {
            BecomeFirstResponder();
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
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            return constraints;
        }

        /// <summary>
        /// Forces a refresh of the display item.
        /// </summary>
        public void RefreshDisplayItem()
        {
            var displayView = DisplayItemRequest() as UIView;
            for (int i = 0; i < Subviews.Length; i++)
            {
                var subview = Subviews[i];
                if (subview != displayView)
                {
                    subview.RemoveFromSuperview();
                }
            }

            if (displayView.Superview != this)
            {
                displayView.RemoveFromSuperview();
                Add(displayView);
            }
        }

        /// <summary>
        /// Forces a refresh of the items in the selection list.
        /// </summary>
        public void RefreshListItems()
        {
            if (selectListController != null)
            {
                selectListController.TableView.ReloadData();
            }
        }

        /// <summary>
        /// Attempts to remove focus from the control.
        /// </summary>
        public void Unfocus()
        {
            ResignFirstResponder();
        }

        /// <summary></summary>
        public override bool BecomeFirstResponder()
        {
            base.BecomeFirstResponder();

            if (Window != null && !IsFocused)
            {
                IsFocused = true;
                OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
                GotFocus(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();

            if (currentSize != Bounds.Size)
            {
                BackgroundColor = background.GetColor(Bounds.Width, Bounds.Height, null);
                Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, null)?.CGColor ?? UIColor.Black.CGColor;
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
        public override bool ResignFirstResponder()
        {
            base.ResignFirstResponder();

            if (IsFocused)
            {
                IsFocused = false;
                OnPropertyChanged(Prism.UI.Controls.Control.IsFocusedProperty);
                LostFocus(this, EventArgs.Empty);
            }
            return true;
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

        private void OnBorderImageChanged(object sender, EventArgs e)
        {
            Layer.BorderColor = borderBrush.GetColor(Bounds.Width, Bounds.Height, null)?.CGColor ?? UIColor.Black.CGColor;
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }

            foreach (var subview in Subviews.Where(sv => sv is INativeVisual))
            {
                try
                {
                    subview.ObserveValue(new NSString(Visual.IsLoadedProperty.Name), this,
                        NSDictionary.FromObjectAndKey(new NSNumber(true), NSObject.ChangeNewKey), IntPtr.Zero);
                }
                catch { }
            }
        }

        private void OnSelected(int index)
        {
            if (selectedIndex != index)
            {
                if (IsOpen)
                {
                    var cell = selectListController.TableView.CellAt(NSIndexPath.FromRowSection(selectedIndex, 0));
                    if (cell != null)
                    {
                        cell.Accessory = UITableViewCellAccessory.None;
                    }

                    cell = selectListController.TableView.CellAt(NSIndexPath.FromRowSection(index, 0));
                    if (cell != null)
                    {
                        cell.Accessory = UITableViewCellAccessory.Checkmark;
                    }
                }

                var args = new SelectionChangedEventArgs(items == null || items.Count <= index || index < 0 ? null : items[index],
                    items == null || items.Count <= selectedIndex || selectedIndex < 0 ? null : items[selectedIndex]);

                selectedIndex = index;
                OnPropertyChanged(Prism.UI.Controls.SelectList.SelectedIndexProperty);
                RefreshDisplayItem();
                SelectionChanged(this, args);
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

            foreach (var subview in Subviews.Where(sv => sv is INativeVisual))
            {
                try
                {
                    subview.ObserveValue(new NSString(Visual.IsLoadedProperty.Name), this,
                        NSDictionary.FromObjectAndKey(new NSNumber(false), NSObject.ChangeNewKey), IntPtr.Zero);
                }
                catch { }
            }
        }

        private class SelectListViewController : UITableViewController
        {
            public Brush Background
            {
                get { return background; }
                set
                {
                    if (value != background)
                    {
                        (background as ImageBrush).ClearImageHandler(OnBackgroundImageChanged);

                        background = value;

                        if (TableView.BackgroundView == null)
                        {
                            TableView.BackgroundView = new UIView();
                        }
                        TableView.BackgroundView.BackgroundColor = value.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, OnBackgroundImageChanged);
                    }
                }
            }
            private Brush background;

            public Brush SeparatorBrush
            {
                get { return separatorBrush; }
                set
                {
                    if (value != separatorBrush)
                    {
                        (separatorBrush as ImageBrush).ClearImageHandler(OnSeparatorImageChanged);

                        separatorBrush = value;
                        TableView.SeparatorColor = separatorBrush.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, OnSeparatorImageChanged) ??
                            new UIColor(0.78f, 0.78f, 0.8f, 1);
                    }
                }
            }
            private Brush separatorBrush;

            private readonly Dictionary<NSIndexPath, nfloat> cellHeights = new Dictionary<NSIndexPath, nfloat>();
            private readonly SelectList selectList;
            private CGSize currentSize;

            public SelectListViewController(SelectList selectList)
            {
                this.selectList = selectList;

                Background = selectList.ListBackground;
                SeparatorBrush = selectList.ListSeparatorBrush;
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                nfloat height;
                return cellHeights.TryGetValue(indexPath, out height) ? height : tableView.EstimatedRowHeight;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(string.Empty) as SelectListCell;
                if (cell == null)
                {
                    cell = new SelectListCell(UITableViewCellStyle.Default, string.Empty);
                    cell.BackgroundColor = null;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                cell.Accessory = selectList.SelectedIndex == indexPath.Row ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

                var view = selectList.ListItemRequest(selectList.items[indexPath.Row]) as UIView;
                for (int i = cell.ContentView.Subviews.Length - 1; i >= 0; i--)
                {
                    var subview = cell.ContentView.Subviews[i];
                    if (subview != view)
                    {
                        subview.RemoveFromSuperview();
                    }
                }

                if (view.Superview != cell.ContentView)
                {
                    cell.ContentView.Add(view);
                }

                cell.LayoutIfNeeded();
                cellHeights[indexPath] = cell.Bounds.Height;
                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell != null)
                {
                    cell.LayoutIfNeeded();
                    return cell.Bounds.Height;
                }

                nfloat height;
                return cellHeights.TryGetValue(indexPath, out height) ? height : tableView.EstimatedRowHeight;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                selectList.IsOpen = false;
                selectList.OnSelected(indexPath.Row);
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return selectList.Items == null ? 0 : selectList.Items.Count;
            }

            public override void ViewDidLoad()
            {
                TableView.EstimatedRowHeight = 44;
                TableView.RowHeight = UITableView.AutomaticDimension;

                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    TableView.CellLayoutMarginsFollowReadableWidth = false;
                }
            }

            public override void ViewWillAppear(bool animated)
            {
                var insets = new UIEdgeInsets();
                if (NavigationController?.NavigationBar?.Hidden ?? true)
                {
                    insets.Top = UIApplication.SharedApplication.StatusBarFrame.Height;
                }

                TableView.ContentInset = insets;
                TableView.ScrollIndicatorInsets = insets;
            }

            public override void ViewDidLayoutSubviews()
            {
                base.ViewDidLayoutSubviews();

                if (currentSize != TableView.Bounds.Size)
                {
                    currentSize = TableView.Bounds.Size;

                    if ((background is LinearGradientBrush || background is ImageBrush) && TableView.BackgroundView != null)
                    {
                        TableView.BackgroundView.BackgroundColor = background.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, null);
                    }

                    if (separatorBrush is LinearGradientBrush || separatorBrush is ImageBrush)
                    {
                        TableView.SeparatorColor = separatorBrush.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, null);
                    }
                }
            }

            private void OnBackgroundImageChanged(object sender, EventArgs e)
            {
                if (TableView.BackgroundView != null)
                {
                    TableView.BackgroundView.BackgroundColor = background.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, null);
                }
            }

            private void OnSeparatorImageChanged(object sender, EventArgs e)
            {
                TableView.SeparatorColor = separatorBrush.GetColor(TableView.Bounds.Width, TableView.Bounds.Height, null) ??
                    new UIColor(0.78f, 0.78f, 0.8f, 1);
            }
        }

        private class SelectListCell : UITableViewCell
        {
            public SelectListCell(UITableViewCellStyle style, string reuseIdentifier)
                : base(style, reuseIdentifier)
            {
            }

            /// <summary></summary>
            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var frame = Frame;

                var visual = ContentView.Subviews.OfType<INativeVisual>().FirstOrDefault();
                if (visual != null)
                {
                    visual.MeasureRequest(true, null);
                    visual.ArrangeRequest(frame.Width != Math.Ceiling(visual.Frame.Width), new Rectangle(0, 0, frame.Width, int.MaxValue));
                    frame.Height = NMath.Max((nfloat)visual.Frame.Height, 44);
                }
                else
                {
                    frame.Height = 44;
                }

                Frame = frame;
            }
        }
    }
}