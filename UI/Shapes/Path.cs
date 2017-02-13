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
using Prism.Input;
using Prism.Native;
using Prism.UI;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Shapes
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativePath"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativePath))]
    public class Path : UIView, INativePath
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
        /// Gets or sets the <see cref="Brush"/> to apply to the interior of the shape.
        /// </summary>
        public Brush Fill
        {
            get { return fill; }
            set
            {
                if (value != fill)
                {
                    (fill as ImageBrush).ClearImageHandler(OnImageLoaded);
                    fill = value;
                    (fill as ImageBrush).BeginLoadingImage(OnImageLoaded);
                    
                    OnPropertyChanged(Prism.UI.Shapes.Shape.FillProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private Brush fill;
        
        /// <summary>
        /// Gets or sets the rule to use for determining the interior fill of the shape.
        /// </summary>
        public FillRule FillRule
        {
            get { return fillRule; }
            set
            {
                if (value != fillRule)
                {
                    fillRule = value;
                    OnPropertyChanged(Prism.UI.Shapes.Path.FillRuleProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private FillRule fillRule;

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
        /// Gets or sets the method to invoke when this instance requests information for the path being drawn.
        /// </summary>
        public PathInfoRequestHandler PathInfoRequest { get; set; }

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
        /// Gets or sets the <see cref="Brush"/> to apply to the outline of the shape.
        /// </summary>
        public Brush Stroke
        {
            get { return stroke; }
            set
            {
                if (value != stroke)
                {
                    (stroke as ImageBrush).ClearImageHandler(OnImageLoaded);
                    stroke = value;
                    (stroke as ImageBrush).BeginLoadingImage(OnImageLoaded);
                    
                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private Brush stroke;

        /// <summary>
        /// Gets or sets the manner in which the ends of a line are drawn.
        /// </summary>
        public LineCap StrokeLineCap
        {
            get { return strokeLineCap; }
            set
            {
                if (value != strokeLineCap)
                {
                    strokeLineCap = value;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeLineCapProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private LineCap strokeLineCap;

        /// <summary>
        /// Gets or sets the manner in which the connections between different lines are drawn.
        /// </summary>
        public LineJoin StrokeLineJoin
        {
            get { return strokeLineJoin; }
            set
            {
                if (value != strokeLineJoin)
                {
                    strokeLineJoin = value;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeLineJoinProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private LineJoin strokeLineJoin;

        /// <summary>
        /// Gets or sets the miter limit for connecting lines.
        /// </summary>
        public double StrokeMiterLimit
        {
            get { return strokeMiterLimit; }
            set
            {
                if (value != strokeMiterLimit)
                {
                    strokeMiterLimit = value;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeMiterLimitProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private double strokeMiterLimit;

        /// <summary>
        /// Gets or sets the width of the shape's outline.
        /// </summary>
        public double StrokeThickness
        {
            get { return strokeThickness; }
            set
            {
                if (value != strokeThickness)
                {
                    strokeThickness = value;

                    OnPropertyChanged(Prism.UI.Shapes.Shape.StrokeThicknessProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private double strokeThickness;

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
        
        private CGPath path;
        private nfloat[] strokeDashArray;
        private nfloat strokeDashOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path()
        {
            ContentMode = UIViewContentMode.Redraw;
            Opaque = false;
        }

        /// <summary></summary>
        /// <param name="rect"></param>
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            
            if (path == null)
            {
                BuildPath();
            }
            
            using (var context = UIGraphics.GetCurrentContext())
            {
                if (context != null)
                {
                    context.SaveState();

                    context.SetFillColor(fill.GetColor(rect.Width, rect.Height, null)?.CGColor);
                    context.SetStrokeColor((stroke.GetColor(rect.Width, rect.Height, null) ?? UIColor.Black).CGColor);
                    context.SetLineWidth((nfloat)strokeThickness);
                    context.SetLineCap(strokeLineCap.GetCGLineCap());
                    context.SetLineJoin(strokeLineJoin.GetCGLineJoin());
                    context.SetMiterLimit((nfloat)strokeMiterLimit);
                    context.SetLineDash(strokeDashOffset, strokeDashArray);
                    
                    context.AddPath(path);
                    context.DrawPath(CGPathDrawingMode.FillStroke);

                    context.RestoreState();
                }
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
        /// Signals that the path needs to be rebuilt before it is drawn again.
        /// </summary>
        public void InvalidatePathInfo()
        {
            path = null;
            SetNeedsDisplay();
        }

        /// <summary>
        /// Lays out subviews.
        /// </summary>
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

        /// <summary>
        /// Indicates the UIView has had its Superview property changed.
        /// </summary>
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
        /// Sets the dash pattern to be used when drawing the outline of the shape.
        /// </summary>
        /// <param name="pattern">An array of values that defines the dash pattern.  Each value represents the length of a dash, alternating between "on" and "off".</param>
        /// <param name="offset">The distance within the dash pattern where dashes begin.</param>
        public void SetStrokeDashPattern(double[] pattern, double offset)
        {
            if (pattern == null)
            {
                strokeDashArray = null;
            }
            else
            {
                strokeDashArray = new nfloat[pattern.Length];
                for (int i = 0; i < pattern.Length; i++)
                {
                    strokeDashArray[i] = (nfloat)pattern[i];
                }
            }
            
            strokeDashOffset = (nfloat)offset;
            SetNeedsDisplay();
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
        
        private void BuildPath()
        {
            path = new CGPath();
            var pathInfos = PathInfoRequest();

            for (int i = 0; i < pathInfos.Count; i++)
            {
                var figure = pathInfos[i];
                var subpath = new CGPath();

                subpath.MoveToPoint(figure.StartPoint.GetCGPoint());

                for (int j = 0; j < figure.Segments.Count; j++)
                {
                    var segment = figure.Segments[j];

                    var line = segment as LineSegment;
                    if (line != null)
                    {
                        subpath.AddLineToPoint(line.EndPoint.GetCGPoint());
                        continue;
                    }
                    
                    var arc = segment as ArcSegment;
                    if (arc != null)
                    {
                        var startPoint = subpath.CurrentPoint;
                        var endPoint = arc.EndPoint.GetCGPoint();
                        var trueSize = arc.Size.GetCGSize();

                        if (trueSize.Width == 0 || trueSize.Height == 0)
                        {
                            subpath.AddLineToPoint(endPoint);
                            continue;
                        }
            
                        nfloat rise = NMath.Round(NMath.Abs(endPoint.Y - startPoint.Y), 1);
                        nfloat run = NMath.Round(NMath.Abs(endPoint.X - startPoint.X), 1);
                        if (rise == 0 && run == 0)
                        {
                            continue;
                        }

                        CGPoint center = new CGPoint(nfloat.NaN, nfloat.NaN);
                        
                        nfloat scale = NMath.Max(run / (trueSize.Width * 2), rise / (trueSize.Height * 2));
                        if (scale > 1)
                        {
                            center.X = (startPoint.X + endPoint.X) / 2;
                            center.Y = (startPoint.Y + endPoint.Y) / 2;
            
                            nfloat diffX = run / 2;
                            nfloat diffY = rise / 2;
            
                            var angle = NMath.Atan2(diffY / trueSize.Height, diffX / trueSize.Width);
                            var cos = NMath.Cos(angle) * trueSize.Width;
                            var sin = NMath.Sin(angle) * trueSize.Height;
            
                            scale = NMath.Sqrt(diffX * diffX + diffY * diffY) / NMath.Sqrt(cos * cos + sin * sin);
                            trueSize.Width *= scale;
                            trueSize.Height *= scale;
                        }

                        startPoint.X /= trueSize.Width;
                        startPoint.Y /= trueSize.Height;
                        endPoint.X /= trueSize.Width;
                        endPoint.Y /= trueSize.Height;
                        center.X /= trueSize.Width;
                        center.Y /= trueSize.Height;

                        if (nfloat.IsNaN(center.X) || nfloat.IsNaN(center.Y))
                        {
                            var midPoint = new CGPoint((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
                            var perpAngle = NMath.Atan2(startPoint.Y - endPoint.Y, endPoint.X - startPoint.X);
                            
                            nfloat diffX = startPoint.X - midPoint.X;
                            nfloat diffY = startPoint.Y - midPoint.Y;
                            nfloat distance = NMath.Sqrt(diffX * diffX + diffY * diffY);
    
                            distance = NMath.Sqrt(1 - distance * distance);
    
                            if ((arc.IsLargeArc && arc.SweepDirection == SweepDirection.Counterclockwise) ||
                                (!arc.IsLargeArc && arc.SweepDirection == SweepDirection.Clockwise))
                            {
                                center = new CGPoint(midPoint.X + NMath.Sin(perpAngle) * distance, midPoint.Y + NMath.Cos(perpAngle) * distance);
                            }
                            else
                            {
                                center = new CGPoint(midPoint.X - NMath.Sin(perpAngle) * distance, midPoint.Y - NMath.Cos(perpAngle) * distance);
                            }
                        }
                        
                        nfloat startAngle = NMath.Atan2(startPoint.Y - center.Y, startPoint.X - center.X);
                        nfloat endAngle = NMath.Atan2(endPoint.Y - center.Y, endPoint.X - center.X);
                        
                        center.X *= trueSize.Width;
                        center.Y *= trueSize.Height;
                        
                        if (NMath.Abs(trueSize.Width - trueSize.Height) < 0.1f)
                        {
                            subpath.AddArc(center.X, center.Y, trueSize.Width, startAngle, endAngle, arc.SweepDirection != SweepDirection.Clockwise);
                        }
                        else
                        {
                            var transform = CGAffineTransform.MakeTranslation(center.X, center.Y);
                            transform = CGAffineTransform.MakeScale(1, trueSize.Height / trueSize.Width) * transform;
                            subpath.AddArc(transform, 0, 0, trueSize.Width, startAngle, endAngle, arc.SweepDirection != SweepDirection.Clockwise);
                        }
                        
                        continue;
                    }

                    var bezier = segment as BezierSegment;
                    if (bezier != null)
                    {
                        subpath.AddCurveToPoint(bezier.ControlPoint1.GetCGPoint(),
                            bezier.ControlPoint2.GetCGPoint(), bezier.EndPoint.GetCGPoint());

                        continue;
                    }

                    var quad = segment as QuadraticBezierSegment;
                    if (quad != null)
                    {
                        subpath.AddQuadCurveToPoint((nfloat)quad.ControlPoint.X, (nfloat)quad.ControlPoint.Y,
                            (nfloat)quad.EndPoint.X, (nfloat)quad.EndPoint.Y);

                        continue;
                    }
                }
                var p = subpath.CurrentPoint;
                var b = subpath.PathBoundingBox;
                if (figure.IsClosed)
                {
                    subpath.CloseSubpath();
                }
                
                path.AddPath(subpath);
            }
        }
        
        private void OnImageLoaded(object sender, EventArgs e)
        {
            SetNeedsDisplay();
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
