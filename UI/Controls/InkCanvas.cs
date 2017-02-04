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
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media.Inking;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativeInkCanvas"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeInkCanvas))]
    public class InkCanvas : UIView, INativeInkCanvas
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
        /// Occurs when a property value changes.
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
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the object relative to its parent container.
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
        /// Gets or sets how the ink canvas handles input.
        /// </summary>
        public InkInputMode InputMode
        {
            get { return inputMode; }
            set
            {
                if (value != inputMode)
                {
                    inputMode = value;
                    
                    if (inputMode == InkInputMode.Erasing)
                    {
                        currentStroke = null;
                        hitPaths = new List<CGPath>(strokes.Count);
                        for (int i = 0; i < strokes.Count; i++)
                        {
                            var stroke = strokes[i];
                            hitPaths.Add(stroke.CGPath.CopyByStrokingPath(stroke.LineWidth,
                                stroke.LineCapStyle, stroke.LineJoinStyle, stroke.MiterLimit));
                        }
                    }
                    else
                    {
                        hitPaths = null;
                    }
                    
                    OnPropertyChanged(Prism.UI.Controls.InkCanvas.InputModeProperty);
                }
            }
        }
        private InkInputMode inputMode;

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
        /// Gets the ink strokes that are on the canvas.
        /// </summary>
        public IEnumerable<INativeInkStroke> Strokes
        {
            get { return strokes.OfType<INativeInkStroke>(); }
        }
        private List<Media.Inking.InkStroke> strokes = new List<Media.Inking.InkStroke>();

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
        
        private UIImage canvasImage;
        private Media.Inking.InkStroke currentStroke;
        private InkDrawingAttributes defaultAttributes;
        private bool dryInk;
        private bool forceDraw;
        private List<CGPath> hitPaths;
        private int pointIndex;
        private CGPoint[] points;

        /// <summary>
        /// Initializes a new instance of the <see cref="InkCanvas"/> class.
        /// </summary>
        public InkCanvas()
        {
            defaultAttributes = new InkDrawingAttributes
            {
                Color = Colors.Black,
                PenTip = PenTipShape.Circle
            };
            
            points = new CGPoint[5];
            
            BackgroundColor = UIColor.Clear;
            MultipleTouchEnabled = false;
        }

        /// <summary>
        /// Adds the specified ink stroke to the canvas.
        /// </summary>
        /// <param name="stroke">The ink stroke to add.</param>
        public void AddStroke(INativeInkStroke stroke)
        {
            var inkStroke = stroke as Media.Inking.InkStroke;
            if (inkStroke != null)
            {
                strokes.Add(inkStroke);
                inkStroke.NeedsDrawing = true;
                inkStroke.Parent = this;
                SetNeedsDisplay();
                
                if (inputMode == InkInputMode.Erasing)
                {
                    hitPaths.Add(inkStroke.CGPath.CopyByStrokingPath(inkStroke.LineWidth,
                        inkStroke.LineCapStyle, inkStroke.LineJoinStyle, inkStroke.MiterLimit));
                }
            }
        }

        /// <summary>
        /// Adds the specified ink strokes to the canvas.
        /// </summary>
        /// <param name="strokes">The ink strokes to add.</param>
        public void AddStrokes(IEnumerable<INativeInkStroke> strokes)
        {
            foreach (var stroke in strokes)
            {
                var inkStroke = stroke as Media.Inking.InkStroke;
                if (inkStroke != null)
                {
                    this.strokes.Add(inkStroke);
                    inkStroke.NeedsDrawing = true;
                    inkStroke.Parent = this;
                    
                    if (inputMode == InkInputMode.Erasing)
                    {
                        hitPaths.Add(inkStroke.CGPath.CopyByStrokingPath(inkStroke.LineWidth,
                            inkStroke.LineCapStyle, inkStroke.LineJoinStyle, inkStroke.MiterLimit));
                    }
                }
            }
            
            SetNeedsDisplay();
        }

        /// <summary>
        /// Removes all ink strokes from the canvas.
        /// </summary>
        public void ClearStrokes()
        {
            foreach (var stroke in strokes)
            {
                if (stroke.Parent == this)
                {
                    stroke.Parent = null;
                }
            }
            strokes.Clear();
            hitPaths?.Clear();
            
            canvasImage = null;
            SetNeedsDisplay();
        }
        
        /// <summary></summary>
        /// <param name="rect"></param>
        public override void Draw(CGRect rect)
        {
            if (dryInk || forceDraw)
            {
                UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0);
            }
        
            canvasImage?.Draw(rect);
        
            for (int i = 0; i < strokes.Count; i++)
            {
                var stroke = strokes[i];
                if (stroke.NeedsDrawing || forceDraw)
                {
                    UIColor.FromCGColor(stroke.Color).SetStroke();
                    stroke.Stroke();
                }
            }
            
            if (dryInk || forceDraw)
            {
                canvasImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                
                for (int i = 0; i < strokes.Count; i++)
                {
                    strokes[i].NeedsDrawing = false;
                }
                
                dryInk = false;
                forceDraw = false;
                
                canvasImage.Draw(rect);
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

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();
        }

        /// <summary>
        /// Measures the object and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the object is not allowed to exceed.</param>
        /// <returns>The desired size as a <see cref="Size"/> instance.</returns>
        public Size Measure(Size constraints)
        {
            return constraints;
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
        
        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null && touch.View == this)
            {
                PointerPressed(this, evt.GetPointerEventArgs(touch, this));
                
                if (inputMode == InkInputMode.Erasing)
                {
                    var point = touch.LocationInView(this);
                    for (int i = hitPaths.Count - 1; i >= 0; i--)
                    {
                        if (hitPaths[i].ContainsPoint(point, false))
                        {
                            hitPaths.RemoveAt(i);
                            strokes.RemoveAt(i);
                            
                            canvasImage = null;
                            forceDraw = true;
                            SetNeedsDisplay();
                        }
                    }
                }
                else
                {
                    pointIndex = 0;
                    points[0] = touch.LocationInView(this);
                    
                    currentStroke = new Media.Inking.InkStroke();
                    currentStroke.Parent = this;
                    currentStroke.UpdateDrawingAttributes(defaultAttributes);
                    strokes.Add(currentStroke);
                    SetNeedsDisplay();
                }
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
            
            pointIndex = 0;
            currentStroke = null;
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
            
            dryInk = true;
            pointIndex = 0;
            currentStroke = null;
            
            SetNeedsDisplay();
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
                
                if (inputMode == InkInputMode.Erasing)
                {
                    var point = touch.LocationInView(this);
                    for (int i = hitPaths.Count - 1; i >= 0; i--)
                    {
                        if (hitPaths[i].ContainsPoint(point, false))
                        {
                            hitPaths.RemoveAt(i);
                            strokes.RemoveAt(i);
                            
                            canvasImage = null;
                            forceDraw = true;
                            SetNeedsDisplay();
                        }
                    }
                }
                else
                {
                    points[++pointIndex] = touch.LocationInView(this);
                    if (pointIndex == 4)
                    {
                        var point1 = points[2];
                        var point2 = points[4];
                        var endPoint = new CGPoint((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
                        
                        currentStroke.MoveTo(points[0]);
                        currentStroke.AddCurveToPoint(endPoint, points[1], point1);
                        
                        points[0] = endPoint;
                        points[1] = point2;
                        pointIndex = 1;
                        
                        SetNeedsDisplay();
                    }
                }
            }
            
            base.TouchesMoved(touches, evt);
        }
        
        /// <summary>
        /// Updates the drawing attributes to apply to new ink strokes on the canvas.
        /// </summary>
        /// <param name="attributes">The drawing attributes to apply to new ink strokes.</param>
        public void UpdateDrawingAttributes(InkDrawingAttributes attributes)
        {
            defaultAttributes = attributes;
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
                            NSDictionary.FromObjectAndKey(new NSNumber(true), ChangeNewKey), IntPtr.Zero);
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
                            NSDictionary.FromObjectAndKey(new NSNumber(false), ChangeNewKey), IntPtr.Zero);
                    }
                    catch { }
                }
            }
        }
    }
}
