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
    /// Represents an iOS implementation of an <see cref="INativePanel"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePanel))]
    public class Panel : UIView, INativePanel
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
        /// Gets or sets the background for the panel.
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
                    BackgroundColor = background.GetColor(base.Frame.Width, base.Frame.Height, OnBackgroundImageLoaded);
                    OnPropertyChanged(Prism.UI.Controls.Panel.BackgroundProperty);
                }
            }
        }
        private Brush background;

        /// <summary>
        /// Gets a collection of the child elements that currently belong to this instance.
        /// </summary>
        public IList Children
        {
            get { return children; }
        }
        private readonly PanelChildrenList children;

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

        private CGRect currentFrame;

        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        public Panel()
        {
            children = new PanelChildrenList(this);

            ClipsToBounds = true;
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
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();

            if (currentFrame != base.Frame)
            {
                BackgroundColor = background.GetColor(base.Frame.Width, base.Frame.Height, null);
            }
            currentFrame = base.Frame;
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

        private void OnBackgroundImageLoaded(object sender, EventArgs e)
        {
            BackgroundColor = background.GetColor(base.Frame.Width, base.Frame.Height, null);
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

        private class PanelChildrenList : IList
        {
            public int Count
            {
                get { return parent.Subviews.Count(sv => sv is INativeElement); }
            }

            public bool IsFixedSize
            {
                get { return false; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            public object SyncRoot
            {
                get { return null; }
            }

            public object this[int index]
            {
                get { return parent.Subviews.OfType<INativeElement>().ElementAt(index); }
                set
                {
                    var child = value as UIView;
                    if (child == null)
                    {
                        throw new ArgumentException("Value must be an object of type UIView.", "value");
                    }
                    
                    var oldChild = parent.Subviews.Where(sv => sv is INativeElement).ElementAt(index);
                    parent.InsertSubviewBelow(child, oldChild);
                    oldChild.RemoveFromSuperview();
                }
            }

            private readonly UIView parent;
            
            public PanelChildrenList(UIView parent)
            {
                this.parent = parent;
            }

            public int Add(object value)
            {
                int count = parent.Subviews.Length;
                var child = value as UIView;
                if (child == null)
                {
                    throw new ArgumentException("Value must be an object of type UIView.", "value");
                }

                parent.Add(child);
                return parent.Subviews.Length - count;
            }

            public void Clear()
            {
                for (int i = parent.Subviews.Length - 1; i >= 0; i--)
                {
                    var child = parent.Subviews[i];
                    if (child is INativeElement)
                    {
                        child.RemoveFromSuperview();
                    }
                }
            }

            public bool Contains(object value)
            {
                return parent.Subviews.Any(sv => sv == value);
            }

            public int IndexOf(object value)
            {
                int index = 0;
                foreach (var subview in parent.Subviews.OfType<INativeElement>())
                {
                    if (subview == value)
                    {
                        return index;
                    }

                    index++;
                }

                return -1;
            }

            public void Insert(int index, object value)
            {
                var child = value as UIView;
                if (child == null)
                {
                    throw new ArgumentException("Value must be an object of type UIView.", "value");
                }

                if (index == Count)
                {
                    parent.Add((UIView)value);
                }
                else
                {
                    int currentIndex = 0;
                    for (int i = 0; i < parent.Subviews.Length; i++)
                    {
                        if (parent.Subviews[i] is INativeElement && currentIndex++ == index)
                        {
                            parent.InsertSubview(child, i);
                            return;
                        }
                    }
                }
            }

            public void Remove(object value)
            {
                var subview = value as UIView;
                if (subview != null && subview.Superview == parent)
                {
                    subview.RemoveFromSuperview();
                }
            }

            public void RemoveAt(int index)
            {
                int currentIndex = 0;
                for (int i = 0; i < parent.Subviews.Length; i++)
                {
                    var subview = parent.Subviews[i];
                    if (subview is INativeElement && currentIndex++ == index)
                    {
                        subview.RemoveFromSuperview();
                        return;
                    }
                }
            }

            public void CopyTo(Array array, int index)
            {
                parent.Subviews.OfType<INativeElement>().ToArray().CopyTo(array, index);
            }

            public IEnumerator GetEnumerator()
            {
                return new PanelChildrenEnumerator(parent.Subviews.GetEnumerator());
            }

            private class PanelChildrenEnumerator : IEnumerator<INativeElement>, IEnumerator
            {
                public INativeElement Current
                {
                    get { return viewEnumerator.Current as INativeElement; }
                }

                object IEnumerator.Current
                {
                    get { return viewEnumerator.Current; }
                }

                private readonly IEnumerator viewEnumerator;

                public PanelChildrenEnumerator(IEnumerator viewEnumerator)
                {
                    this.viewEnumerator = viewEnumerator;
                }

                public void Dispose()
                {
                    var disposable = viewEnumerator as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }

                public bool MoveNext()
                {
                    do
                    {
                        if (!viewEnumerator.MoveNext())
                        {
                            return false;
                        }
                    }
                    while (!(viewEnumerator.Current is INativeElement));

                    return true;
                }

                public void Reset()
                {
                    viewEnumerator.Reset();
                }
            }
        }
    }
}

