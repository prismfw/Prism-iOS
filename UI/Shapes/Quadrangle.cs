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
    /// Represents an iOS implementation for an <see cref="INativeQuadrangle"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeQuadrangle))]
    public class Quadrangle : UIView, INativeQuadrangle
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
                    (fill as ImageBrush).ClearImageHandler(OnImageChanged);
                    fill = value;
                    (fill as ImageBrush).BeginLoadingImage(OnImageChanged);

                    OnPropertyChanged(Prism.UI.Shapes.Shape.FillProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private Brush fill;

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
                isDirty = true;
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
        /// Gets or sets the X-axis radius of the ellipse that is used to round the corners of the quadrangle.
        /// </summary>
        public double RadiusX
        {
            get { return radiusX; }
            set
            {
                if (value != radiusX)
                {
                    radiusX = value;
                    isDirty = true;
                    OnPropertyChanged(Prism.UI.Shapes.Quadrangle.RadiusXProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private double radiusX;

        /// <summary>
        /// Gets or sets the X-axis radius of the ellipse that is used to round the corners of the quadrangle.
        /// </summary>
        public double RadiusY
        {
            get { return radiusY; }
            set
            {
                if (value != radiusY)
                {
                    radiusY = value;
                    isDirty = true;
                    OnPropertyChanged(Prism.UI.Shapes.Quadrangle.RadiusYProperty);
                    SetNeedsDisplay();
                }
            }
        }
        private double radiusY;

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
                    (stroke as ImageBrush).ClearImageHandler(OnImageChanged);
                    stroke = value;
                    (stroke as ImageBrush).BeginLoadingImage(OnImageChanged);

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

        private bool isDirty;
        private CGPath path;
        private nfloat[] strokeDashArray;
        private nfloat strokeDashOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quadrangle"/> class.
        /// </summary>
        public Quadrangle()
        {
            ContentMode = UIViewContentMode.Redraw;
            MultipleTouchEnabled = true;
            Opaque = false;
        }

        /// <summary></summary>
        /// <param name="rect"></param>
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

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

                    rect.X += (nfloat)(strokeThickness * 0.5);
                    rect.Y += (nfloat)(strokeThickness * 0.5);
                    rect.Width -= (nfloat)strokeThickness;
                    rect.Height -= (nfloat)strokeThickness;

                    if (isDirty || path == null)
                    {
                        path = new CGPath();
                        path.AddRoundedRect(rect, (nfloat)radiusX, (nfloat)radiusY);
                        isDirty = false;
                    }

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

        private void OnImageChanged(object sender, EventArgs e)
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
