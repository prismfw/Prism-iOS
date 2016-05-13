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
using CoreGraphics;
using Foundation;
using UIKit;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Controls;
using Prism.Utilities;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeWebBrowser"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeWebBrowser))]
    public class WebBrowser : UIWebView, INativeWebBrowser
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when the web browser has finished loading the document.
        /// </summary>
        public event EventHandler<WebNavigationCompletedEventArgs> NavigationCompleted;

        /// <summary>
        /// Occurs when the web browser has begun navigating to a document.
        /// </summary>
        public event EventHandler<WebNavigationStartingEventArgs> NavigationStarting;
        
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
        /// Occurs when a script invocation has completed.
        /// </summary>
        public event EventHandler<WebScriptCompletedEventArgs> ScriptCompleted;

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
        /// Gets a value indicating whether the web browser has at least one document in its back navigation history.
        /// </summary>
        public new bool CanGoBack { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the web browser has at least one document in its forward navigation history.
        /// </summary>
        public new bool CanGoForward { get; private set; }

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
        /// Gets the title of the current document.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the URI of the current document.
        /// </summary>
        public Uri Uri { get; private set; }

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
        /// Initializes a new instance of the <see cref="WebBrowser"/> class.
        /// </summary>
        public WebBrowser()
        {
            ScalesPageToFit = true;
            
            ShouldStartLoad = (webView, request, navigationType) =>
            {
                var args = new WebNavigationStartingEventArgs(request.Url);
                NavigationStarting(this, args);
                return !args.Cancel;
            };

            LoadError += (sender, e) =>
            {
                Logger.Error("Error loading web request: {0}", e.Error.LocalizedDescription);
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
            };

            LoadFinished += (sender, e) => OnNavigationCompleted();
            LoadStarted += (sender, e) => { UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true; };
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
        /// Executes a script function that is implemented by the current document.
        /// </summary>
        /// <param name="scriptName">The name of the script function to execute.</param>
        public void InvokeScript(string scriptName)
        {
            var result = EvaluateJavascript(scriptName);
            ScriptCompleted(this, new WebScriptCompletedEventArgs(result));
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            var parent = this.GetNextResponder<UIViewController>();
            if (parent != null && parent.View != null)
            {
                constraints.Width = Math.Min(constraints.Width, parent.View.Frame.Width);
                constraints.Height = Math.Min(constraints.Height, parent.View.Frame.Height);
            }

            return constraints;
        }

        /// <summary>
        /// Navigates to the specified <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI to navigate to.</param>
        public void Navigate(Uri uri)
        {
            LoadRequest(NSUrlRequest.FromUrl(uri));
        }

        /// <summary>
        /// Navigates to the specified <see cref="String"/> containing the content for a document.
        /// </summary>
        /// <param name="html">The string containing the content for a document.</param>
        public void NavigateToString(string html)
        {
            LoadHtmlString(html, null);
        }

        /// <summary>
        /// Reloads the current document.
        /// </summary>
        public void Refresh()
        {
            Reload();
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();
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
            }
        }

        private void OnNavigationCompleted()
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

            if (CanGoBack != base.CanGoBack)
            {
                CanGoBack = base.CanGoBack;
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.CanGoBackProperty);
            }

            if (CanGoForward != base.CanGoForward)
            {
                CanGoForward = base.CanGoForward;
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.CanGoForwardProperty);
            }

            try
            {
                if (Uri != Request.Url)
                {
                    Uri = Request.Url;
                    OnPropertyChanged(Prism.UI.Controls.WebBrowser.UriProperty);
                }
            }
            catch { }

            string title = EvaluateJavascript("document.title");
            if (Title != title)
            {
                Title = title;
                OnPropertyChanged(Prism.UI.Controls.WebBrowser.TitleProperty);
            }
            
            NavigationCompleted(this, new WebNavigationCompletedEventArgs(Uri));
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

