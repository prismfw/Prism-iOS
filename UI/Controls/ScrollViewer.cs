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
using Prism.UI.Controls;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeScrollViewer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeScrollViewer))]
    public class ScrollViewer : UIScrollView, INativeScrollViewer
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
        /// Gets or sets a value indicating whether the content of the scroll viewer can be scrolled horizontally.
        /// </summary>
        public bool CanScrollHorizontally
        {
            get { return base.ShowsHorizontalScrollIndicator; }
            set
            {
                if (value != base.ShowsHorizontalScrollIndicator)
                {
                    base.ShowsHorizontalScrollIndicator = value;
                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.CanScrollHorizontallyProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content of the scroll viewer can be scrolled vertically.
        /// </summary>
        public bool CanScrollVertically
        {
            get { return base.ShowsVerticalScrollIndicator; }
            set
            {
                if (value != base.ShowsVerticalScrollIndicator)
                {
                    base.ShowsVerticalScrollIndicator = value;
                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.CanScrollVerticallyProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the content of the scroll viewer.
        /// </summary>
        public object Content
        {
            get { return content; }
            set
            {
                for (int i = Subviews.Length - 1; i >= 0; i--)
                {
                    Subviews[i].RemoveFromSuperview();
                }

                content = value;
                if (value != null)
                {
                    Add(value as UIView);
                }
            }
        }
        private object content;

        /// <summary>
        /// Gets the distance that the content has been scrolled.
        /// </summary>
        public new Point ContentOffset
        {
            get { return new Point(base.ContentOffset.X + ContentInset.Left, base.ContentOffset.Y + ContentInset.Top); }
        }

        /// <summary>
        /// Gets the size of the scrollable area within the scroll viewer.
        /// </summary>
        public new Size ContentSize
        {
            get { return contentSize; }
            private set
            {
                if (value != contentSize)
                {
                    contentSize = value;
                    OnPropertyChanged(Prism.UI.Controls.ScrollViewer.ContentSizeProperty);
                }
            }
        }
        private Size contentSize;

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
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollViewer"/> class.
        /// </summary>
        public ScrollViewer()
        {
            Scrolled += (sender, e) =>
            {
                OnPropertyChanged(Prism.UI.Controls.ScrollViewer.ContentOffsetProperty);
            };
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
            base.LayoutSubviews();

            constraints.Width += (ContentInset.Left + ContentInset.Right);
            constraints.Height += (ContentInset.Top + ContentInset.Bottom);
            return constraints;
        }

        /// <summary>
        /// Scrolls the content within the scroll viewer to the specified offset.
        /// </summary>
        /// <param name="offset">The position to which to scroll the content.</param>
        /// <param name="animate">Whether to animate the scrolling.</param>
        public void ScrollTo(Point offset, Animate animate)
        {
            SetContentOffset(new CGPoint((nfloat)offset.X - ContentInset.Left, (nfloat)offset.Y - ContentInset.Top),
                areAnimationsEnabled && animate != Prism.UI.Animate.Off);
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            var offset = base.ContentOffset;
            
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.ContentOffset = offset;

            var contentSize = base.ContentSize;
            var subview = Subviews.FirstOrDefault();
            var element = ObjectRetriever.GetAgnosticObject(subview) as Element;
            if (element != null)
            {
                contentSize = element.DesiredSize.GetCGSize();
            }
            else if (subview != null)
            {
                base.LayoutSubviews();
                contentSize = subview.Frame.Size;
            }

            ContentSize = contentSize.GetSize();
            contentSize = new CGSize(CanScrollHorizontally ? contentSize.Width : Math.Min(base.Frame.Width - (ContentInset.Left + ContentInset.Right), contentSize.Width),
                CanScrollVertically ? contentSize.Height : Math.Min(base.Frame.Height - (ContentInset.Top + ContentInset.Bottom), contentSize.Height));

            if (contentSize != base.ContentSize)
            {
                base.ContentSize = contentSize;
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

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
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
    }
}

