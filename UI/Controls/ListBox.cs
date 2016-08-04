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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using Foundation;
using CoreGraphics;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeListBox"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeListBox))]
    public class ListBox : UITableView, INativeListBox
    {
        /// <summary>
        /// Gets the current item reuse identifier.  New instances of <see cref="ListBoxItem"/> are initialized with this reuse identifier.
        /// </summary>
        public static string CurrentItemId { get; private set; }

        /// <summary>
        /// Gets the current section header reuse identifier.  New instances of <see cref="ListBoxSectionHeader"/> are initialized with this reuse identifier.
        /// </summary>
        public static string CurrentSectionHeaderId { get; private set; }

        /// <summary>
        /// Occurs when an accessory in a list box item is clicked or tapped.
        /// </summary>
        public event EventHandler<AccessoryClickedEventArgs> AccessoryClicked;
        
        /// <summary>
        /// Occurs when an item in the list box is clicked or tapped.
        /// </summary>
        public event EventHandler<ItemClickedEventArgs> ItemClicked;

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
        /// Occurs when the selection of the list box is changed.
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
        /// Gets or sets the background of the list box.
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

                    if (BackgroundView == null)
                    {
                        BackgroundView = new UIView();
                    }
                    BackgroundView.BackgroundColor = value.GetColor(base.Frame.Width, base.Frame.Height, OnBackgroundImageLoaded);

                    OnPropertyChanged(Prism.UI.Controls.ListBox.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the list box can be scrolled horizontally.
        /// </summary>
        public bool CanScrollHorizontally
        {
            get { return base.ShowsHorizontalScrollIndicator; }
            set
            {
                if (value != base.ShowsHorizontalScrollIndicator)
                {
                    base.ShowsHorizontalScrollIndicator = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the contents of the list box can be scrolled vertically.
        /// </summary>
        public bool CanScrollVertically
        {
            get { return base.ShowsVerticalScrollIndicator; }
            set
            {
                if (value != base.ShowsVerticalScrollIndicator)
                {
                    base.ShowsVerticalScrollIndicator = value;
                }
            }
        }

        /// <summary>
        /// Gets the distance that the contents of the list box has been scrolled.
        /// </summary>
        public new Point ContentOffset
        {
            get { return new Point(base.ContentOffset.X + ContentInset.Left, base.ContentOffset.Y + ContentInset.Top); }
        }

        /// <summary>
        /// Gets the size of the scrollable area within the list box.
        /// </summary>
        public new Size ContentSize
        {
            get { return base.ContentSize.GetSize(); }
        }

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public new Rectangle Frame
        {
            get { return base.Frame.GetRectangle(); }
            set { base.Frame = value.GetCGRect(); }
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
        /// Gets or sets a value indicating whether each object in the <see cref="P:Items"/> collection represents a different section in the list.
        /// When <c>true</c>, objects that implement <see cref="IList"/> will have each of their items represent a different entry in the same section.
        /// </summary>
        public bool IsSectioningEnabled
        {
            get { return isSectioningEnabled; }
            set
            {
                if (value != isSectioningEnabled)
                {
                    isSectioningEnabled = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.IsSectioningEnabledProperty);

                    EstimatedSectionHeaderHeight = value ? 23 : 0;
                    if (items != null && items.Count > 0)
                    {
                        ReloadData();
                    }
                }
            }
        }
        private bool isSectioningEnabled;

        /// <summary>
        /// Gets or sets the method to be used for retrieving reuse identifiers for items in the list box.
        /// </summary>
        public ItemIdRequestHandler ItemIdRequest { get; set; }

        /// <summary>
        /// Gets or sets the method to be used for retrieving display items for items in the list box.
        /// </summary>
        public ListBoxItemRequestHandler ItemRequest { get; set; }

        /// <summary>
        /// Gets or sets the items that make up the contents of the list box.
        /// </summary>
        public IList Items
        {
            get { return items; }
            set
            {
                if (value != items)
                {
                    var notifier = items as INotifyCollectionChanged;
                    if (notifier != null)
                    {
                        notifier.CollectionChanged -= OnItemsCollectionChanged;
                        foreach (var item in items.OfType<INotifyCollectionChanged>())
                        {
                            item.CollectionChanged -= OnItemsSubcollectionChanged;
                        }
                    }

                    notifier = value as INotifyCollectionChanged;
                    if (notifier != null)
                    {
                        notifier.CollectionChanged -= OnItemsCollectionChanged;
                        notifier.CollectionChanged += OnItemsCollectionChanged;
                    }

                    if (value != null)
                    {
                        foreach (var item in value.OfType<INotifyCollectionChanged>())
                        {
                            item.CollectionChanged -= OnItemsSubcollectionChanged;
                            item.CollectionChanged += OnItemsSubcollectionChanged;
                        }
                    }

                    items = value;
                    OnPropertyChanged(Prism.UI.Controls.ListBox.ItemsProperty);
                    ReloadData();
                }
            }
        }
        private IList items;

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
        /// Gets or sets the method to be used for retrieving section headers in the list box.
        /// </summary>
        public ListBoxSectionHeaderRequestHandler SectionHeaderRequest { get; set; }

        /// <summary>
        /// Gets or sets the method to be used for retrieving reuse identifiers for section headers.
        /// </summary>
        public ItemIdRequestHandler SectionHeaderIdRequest { get; set; }

        /// <summary>
        /// Gets the currently selected items.
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                if (IndexPathsForSelectedRows == null)
                {
                    return new ReadOnlyCollection<object>(new object[0]);
                }

                return new List<object>(IndexPathsForSelectedRows.Select(i => GetItemAtIndexPath(i))).AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the selection behavior for the list box.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return AllowsMultipleSelection ? SelectionMode.Multiple : AllowsSelection ? SelectionMode.Single : SelectionMode.None; }
            set
            {
                if (value != SelectionMode)
                {
                    if (value == SelectionMode.Multiple)
                    {
                        AllowsMultipleSelection = true;
                        AllowsSelection = true;
                    }
                    else
                    {
                        var removedItems = IndexPathsForSelectedRows?.Select(i => GetItemAtIndexPath(i)).ToArray();
                        AllowsMultipleSelection = false;
                        AllowsSelection = value == SelectionMode.Single;

                        if (removedItems != null)
                        {
                            SelectionChanged(this, new SelectionChangedEventArgs(null, removedItems));
                        }
                    }

                    OnPropertyChanged(Prism.UI.Controls.ListBox.SelectionModeProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to apply to the separators between each item in the list.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return separatorBrush; }
            set
            {
                if (value != separatorBrush)
                {
                    (separatorBrush as ImageBrush).ClearImageHandler(OnSeparatorImageLoaded);

                    separatorBrush = value;
                    SeparatorColor = separatorBrush.GetColor(base.Frame.Width, base.Frame.Height, OnSeparatorImageLoaded) ?? new UIColor(0.78f, 0.78f, 0.8f, 1);
                    OnPropertyChanged(Prism.UI.Controls.ListBox.SeparatorBrushProperty);
                }
            }
        }
        private Brush separatorBrush;

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

        private CGPoint currentContentOffset;
        private CGSize currentContentSize;
        private CGRect currentFrame;
        private NSIndexPath lastCellIndex = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox(ListBoxStyle style)
            : base(CGRect.Empty, style == ListBoxStyle.Grouped ? UITableViewStyle.Grouped : UITableViewStyle.Plain)
        {
            EstimatedRowHeight = 44;
            EstimatedSectionHeaderHeight = 0;
            EstimatedSectionFooterHeight = 0;
            RowHeight = UITableView.AutomaticDimension;
            SectionHeaderHeight = UITableView.AutomaticDimension;
            SectionFooterHeight = UITableView.AutomaticDimension;

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                CellLayoutMarginsFollowReadableWidth = false;
            }

            Source = new ListBoxSource();
        }

        /// <summary>
        /// Deselects the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to deselect.</param>
        /// <param name="animate">Whether to animate the deselection.</param>
        public void DeselectItem(object item, Animate animate)
        {
            var indexPath = GetIndexPathForItem(item);
            if (indexPath != null && IndexPathsForSelectedRows != null && IndexPathsForSelectedRows.Contains(indexPath))
            {
                DeselectRow(indexPath, areAnimationsEnabled && animate == Prism.UI.Animate.On);
                OnDeselected(indexPath);
            }
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
            var frame = base.Frame;
            base.Frame = new CGRect(0, 0, 0, 0);

            SizeToFit();

            var size = base.Frame.Size;
            base.Frame = frame;
            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
        }

        /// <summary>
        /// Forces a reload of the list box's contents.
        /// </summary>
        public void Reload()
        {
            ReloadData();
        }

        /// <summary>
        /// Scrolls to the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to which the list box should scroll.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(object item, Animate animate)
        {
            var indexPath = GetIndexPathForItem(item);
            if (indexPath != null)
            {
                var nextIndex = lastCellIndex ?? NSIndexPath.FromRowSection(0, 0);
                if (nextIndex.Section < indexPath.Section || (nextIndex.Section == indexPath.Section && nextIndex.Row < indexPath.Row))
                {
                    List<NSIndexPath> reloads = new List<NSIndexPath>();
                    for (nint section = nextIndex.Section; section <= indexPath.Section; section++)
                    {
                        nint itemCount = section == indexPath.Section ? indexPath.Row + 1 : NumberOfRowsInSection(section);
                        for (nint row = (section == nextIndex.Section ? nextIndex.Row : 0); row < itemCount; row++)
                        {
                            var cellIndex = NSIndexPath.FromRowSection(row, section);
                            Source.GetCell(this, cellIndex);
                            reloads.Add(cellIndex);
                        }
                    }

                    ReloadRows(reloads.ToArray(), UITableViewRowAnimation.None);
                }
                
                ScrollToRow(indexPath, UITableViewScrollPosition.None, areAnimationsEnabled && animate != Prism.UI.Animate.Off);
            }
        }

        /// <summary>
        /// Scrolls the contents within the list box to the specified offset.
        /// </summary>
        /// <param name="offset">The position to which to scroll the contents.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(Point offset, Animate animate)
        {
            SetContentOffset(new CGPoint((nfloat)offset.X - ContentInset.Left, (nfloat)offset.Y - ContentInset.Top),
                areAnimationsEnabled && animate != Prism.UI.Animate.Off);
        }

        /// <summary>
        /// Selects the specified item.
        /// </summary>
        /// <param name="item">The item within the <see cref="P:Items"/> collection to select.</param>
        /// <param name="animate">Whether to animate the selection.</param>
        public void SelectItem(object item, Animate animate)
        {
            var indexPath = GetIndexPathForItem(item);
            if (indexPath != null && (IndexPathsForSelectedRows == null || !IndexPathsForSelectedRows.Contains(indexPath)))
            {
                var previousIndexPath = AllowsMultipleSelection ? null : IndexPathForSelectedRow;
                SelectRow(indexPath, areAnimationsEnabled && animate == Prism.UI.Animate.On, UITableViewScrollPosition.None);
                OnSelected(indexPath, previousIndexPath);
            }
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();

            if (currentFrame != base.Frame)
            {
                currentFrame = base.Frame;

                if (BackgroundView != null)
                {
                    BackgroundView.BackgroundColor = background.GetColor(base.Frame.Width, base.Frame.Height, null);
                }

                SeparatorColor = separatorBrush.GetColor(base.Frame.Width, base.Frame.Height, null) ?? new UIColor(0.78f, 0.78f, 0.8f, 1);
            }

            if (currentContentOffset != base.ContentOffset)
            {
                currentContentOffset = base.ContentOffset;
                OnPropertyChanged(Prism.UI.Controls.ListBox.ContentOffsetProperty);
            }

            if (currentContentSize != base.ContentSize)
            {
                currentContentSize = base.ContentSize;
                OnPropertyChanged(Prism.UI.Controls.ListBox.ContentSizeProperty);
            }
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
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerPressed(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesBegan(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerCanceled(this, evt.GetPointerEventArgs(touch, this));
            }
        
            base.TouchesCancelled(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerReleased(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesEnded(touches, evt);
        }
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerMoved(this, evt.GetPointerEventArgs(touch, this));
            }
            
            base.TouchesMoved(touches, evt);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && Source != null)
            {
                Source.Dispose();
                Source = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        private NSIndexPath GetIndexPathForItem(object item)
        {
            if (items == null)
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var obj = items[i];
                if (item.Equals(obj))
                {
                    return isSectioningEnabled ? NSIndexPath.FromRowSection(0, i) : NSIndexPath.FromRowSection(i, 0);
                }

                if (isSectioningEnabled)
                {
                    var list = obj as IList;
                    if (list != null)
                    {
                        int index = list.IndexOf(item);
                        if (index >= 0)
                        {
                            return NSIndexPath.FromRowSection(index, i);
                        }
                    }
                }
            }

            return null;
        }

        private NSIndexPath[] GetIndexPathsForItems(IList items, int startingIndex)
        {
            if (items == null || this.items == null)
            {
                return null;
            }

            var indexPaths = new NSIndexPath[items.Count];
            if (indexPaths.Length == 0)
            {
                return indexPaths;
            }

            var item = items[0];
            if (startingIndex < this.items.Count && item == this.items[startingIndex])
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (isSectioningEnabled)
                    {
                        indexPaths[i] = NSIndexPath.FromRowSection(0, startingIndex++);
                    }
                    else
                    {
                        indexPaths[i] = NSIndexPath.FromRowSection(startingIndex++, 0);
                    }
                }
            }
            else
            {
                
                for (int i = 0; i < this.items.Count; i++)
                {
                    var list = this.items[i] as IList;
                    if (list != null && startingIndex < list.Count && item == list[startingIndex])
                    {
                        for (int j = 0; j < items.Count; j++)
                        {
                            indexPaths[j] = NSIndexPath.FromRowSection(startingIndex++, i);
                        }

                        break;
                    }
                }
            }

            return indexPaths;
        }

        private object GetItemAtIndexPath(NSIndexPath indexPath)
        {
            if (items == null || indexPath == null)
            {
                return null;
            }

            if (isSectioningEnabled)
            {
                var item = items[indexPath.Section];
                var list = item as IList;
                return list == null ? item : list[indexPath.Row];
            }

            return items[indexPath.Row];
        }

        private void OnAccessoryClicked(NSIndexPath indexPath)
        {
            AccessoryClicked(this, new AccessoryClickedEventArgs(GetItemAtIndexPath(indexPath)));
        }

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            if (BackgroundView != null)
            {
                BackgroundView.BackgroundColor = background.GetColor(base.Frame.Width, base.Frame.Height, null);
            }
        }

        private void OnDeselected(NSIndexPath indexPath)
        {
            OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
            SelectionChanged(this, new SelectionChangedEventArgs(null, GetItemAtIndexPath(indexPath)));
        }
        
        private void OnItemClicked(NSIndexPath indexPath)
        {
            ItemClicked(this, new ItemClickedEventArgs(GetItemAtIndexPath(indexPath)));
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AnimationsEnabled = areAnimationsEnabled;

            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<INotifyCollectionChanged>())
                    {
                        item.CollectionChanged -= OnItemsSubcollectionChanged;
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<INotifyCollectionChanged>())
                    {
                        item.CollectionChanged += OnItemsSubcollectionChanged;
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (isSectioningEnabled)
                    {
                        InsertSections(NSIndexSet.FromNSRange(new NSRange(e.NewStartingIndex, e.NewItems.Count)), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        InsertRows(GetIndexPathsForItems(e.NewItems, e.NewStartingIndex), UITableViewRowAnimation.Automatic);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (isSectioningEnabled)
                    {
                        MoveSection(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else
                    {
                        MoveRow(NSIndexPath.FromRowSection(e.OldStartingIndex, 0), NSIndexPath.FromRowSection(e.NewStartingIndex, 0));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (isSectioningEnabled)
                    {
                        DeleteSections(NSIndexSet.FromNSRange(new NSRange(e.OldStartingIndex, e.OldItems.Count)), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        int index = e.OldStartingIndex;
                        var indexPaths = new NSIndexPath[e.OldItems.Count];
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            indexPaths[i] = NSIndexPath.FromRowSection(index++, 0);
                        }

                        DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (isSectioningEnabled)
                    {
                        ReloadSections(NSIndexSet.FromIndex(e.NewStartingIndex), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        ReloadRows(new[] { NSIndexPath.FromRowSection(e.NewStartingIndex, 0) }, UITableViewRowAnimation.Automatic);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReloadData();
                    break;
            }

            AnimationsEnabled = true;
        }

        private void OnItemsSubcollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsSectioningEnabled)
            {
                return;
            }

            AnimationsEnabled = areAnimationsEnabled;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InsertRows(GetIndexPathsForItems(e.NewItems, e.NewStartingIndex), UITableViewRowAnimation.Automatic);
                    break;
                case NotifyCollectionChangedAction.Move:
                    int section = items.IndexOf(sender);
                    MoveRow(NSIndexPath.FromRowSection(e.OldStartingIndex, section), NSIndexPath.FromRowSection(e.NewStartingIndex, section));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    section = items.IndexOf(sender);
                    int index = e.OldStartingIndex;
                    var indexPaths = new NSIndexPath[e.OldItems.Count];
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        indexPaths[i] = NSIndexPath.FromRowSection(index++, section);
                    }

                    DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReloadRows(new[] { NSIndexPath.FromRowSection(e.NewStartingIndex, items.IndexOf(sender)) }, UITableViewRowAnimation.Automatic);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ReloadSections(NSIndexSet.FromIndex(items.IndexOf(sender)), UITableViewRowAnimation.Automatic);
                    break;
            }

            AnimationsEnabled = true;
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);

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
        }

        private void OnSelected(NSIndexPath newIndexPath, NSIndexPath previousIndexPath)
        {
            if (newIndexPath != null && !newIndexPath.Equals(previousIndexPath))
            {
                OnPropertyChanged(Prism.UI.Controls.ListBox.SelectedItemsProperty);
                SelectionChanged(this, new SelectionChangedEventArgs(GetItemAtIndexPath(newIndexPath), GetItemAtIndexPath(previousIndexPath)));
            }
        }

        private void OnSeparatorImageLoaded(object sender, EventArgs e)
        {
            SeparatorColor = separatorBrush.GetColor(base.Frame.Width, base.Frame.Height, null) ?? new UIColor(0.78f, 0.78f, 0.8f, 1);
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);

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
        }

        private class ListBoxSource : UITableViewSource
        {
            private Dictionary<NSIndexPath, UITableViewCell> cells = new Dictionary<NSIndexPath, UITableViewCell>();
            private Dictionary<nint, UIView> headers = new Dictionary<nint, UIView>();

            private NSIndexPath previouslySelectedCellIndex;

            public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
            {
                var listBox = tableView as ListBox;
                if (listBox != null)
                {
                    listBox.OnAccessoryClicked(indexPath);
                }
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell;
                if (!cells.TryGetValue(indexPath, out cell) || cell == null)
                {
                    return tableView.EstimatedRowHeight;
                }

                cell.LayoutIfNeeded();
                return cell.Frame.Height;
            }

            public override nfloat EstimatedHeightForHeader(UITableView tableView, nint section)
            {
                UIView header = null;
                if (!headers.TryGetValue(section, out header))
                {
                    return tableView.EstimatedSectionHeaderHeight;
                }

                if (header == null)
                {
                    return 0;
                }

                header.LayoutIfNeeded();
                return header.Frame.Height;
            }
            
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var listBox = tableView as ListBox;
                if (listBox == null)
                {
                    return new UITableViewCell();
                }

                if (listBox.lastCellIndex == null || listBox.lastCellIndex.Section < indexPath.Section ||
                    (listBox.lastCellIndex.Section == indexPath.Section && listBox.lastCellIndex.Row < indexPath.Row))
                {
                    listBox.lastCellIndex = indexPath;
                }

                object item = listBox.GetItemAtIndexPath(indexPath);
                CurrentItemId = listBox.ItemIdRequest(item);

                var cell = (UITableViewCell)listBox.ItemRequest(item, tableView.DequeueReusableCell(CurrentItemId) as INativeListBoxItem);
                (cell as ITableViewChild)?.SetParent(tableView);
                cell.Frame = new CGRect(cell.Frame.Location, new CGSize(tableView.Frame.Width, tableView.EstimatedRowHeight));
                cells[indexPath] = cell;

                CurrentItemId = null;
                return cell;
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                UIView header = null;
                if (!headers.TryGetValue(section, out header))
                {
                    return UITableView.AutomaticDimension;
                }

                if (header == null)
                {
                    return 0;
                }

                header.LayoutSubviews();
                return header.Frame.Height;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell;
                if (!cells.TryGetValue(indexPath, out cell) || cell == null)
                {
                    return tableView.EstimatedRowHeight;
                }

                cell.LayoutSubviews();
                return cell.Frame.Height;
            }

            public override UIView GetViewForHeader(UITableView tableView, nint section)
            {
                var listBox = tableView as ListBox;
                if (listBox == null || !listBox.isSectioningEnabled || listBox.items == null || listBox.items.Count <= section)
                {
                    return null;
                }

                var obj = listBox.items[(int)section];
                CurrentSectionHeaderId = listBox.SectionHeaderIdRequest(obj);

                var header = listBox.SectionHeaderRequest(obj,
                    tableView.DequeueReusableHeaderFooterView(CurrentSectionHeaderId) as INativeListBoxSectionHeader) as UIView;

                if (header != null)
                {
                    (header as ITableViewChild)?.SetParent(tableView);
                    header.Frame = new CGRect(header.Frame.Location, new CGSize(tableView.Frame.Width, tableView.EstimatedSectionHeaderHeight));
                }

                headers[section] = header;

                CurrentSectionHeaderId = null;
                return header;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                var listBox = tableView as ListBox;
                if (listBox == null)
                {
                    return 0;
                }

                if (listBox.isSectioningEnabled)
                {
                    return listBox.items.Count;
                }

                return 1;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                if (previouslySelectedCellIndex != null)
                {
                    return;
                }
                
                var listBox = tableView as ListBox;
                if (listBox != null)
                {
                    listBox.OnDeselected(indexPath);
                }
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var listBox = tableView as ListBox;
                if (listBox != null)
                {
                    listBox.OnSelected(indexPath, previouslySelectedCellIndex);
                }

                previouslySelectedCellIndex = null;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                var listBox = tableview as ListBox;
                if (listBox == null)
                {
                    return 0;
                }

                if (listBox.isSectioningEnabled)
                {
                    var list = listBox.items[(int)section] as IList;
                    return list == null ? 0 : list.Count;
                }

                return section == 0 && listBox.items != null ? listBox.items.Count : 0;
            }
            
            public override NSIndexPath WillDeselectRow(UITableView tableView, NSIndexPath indexPath)
            {
                var listBox = tableView as ListBox;
                if (listBox == null)
                {
                    return indexPath;
                }
                
                var retVal = listBox.SelectionMode != SelectionMode.Multiple ? indexPath : null;
                
                if (retVal == null)
                {
                    listBox.OnItemClicked(indexPath);
                }
                return retVal;
            }

            public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
            {
                var listBox = tableView as ListBox;
                if (listBox != null)
                {
                    listBox.OnItemClicked(indexPath);
                }
                
                if (!tableView.AllowsMultipleSelection)
                {
                    previouslySelectedCellIndex = tableView.IndexPathForSelectedRow;
                }
                return indexPath;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    cells.Clear();
                    headers.Clear();
                }
            }
        }
    }
}