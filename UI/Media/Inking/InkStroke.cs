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
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Prism.Native;
using Prism.UI.Media.Inking;
using UIKit;

namespace Prism.iOS.UI.Media.Inking
{
    /// <summary>
    /// Represents an iOS implementation for an <see cref="INativeInkStroke"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeInkStroke))]
    public class InkStroke : UIBezierPath, INativeInkStroke
    {
        /// <summary>
        /// Gets a rectangle that encompasses all of the points in the ink stroke.
        /// </summary>
        public Rectangle BoundingBox
        {
            get { return Bounds.GetRectangle(); }
        }

        /// <summary>
        /// Gets a collection of points that make up the ink stroke.
        /// </summary>
        public IEnumerable<Point> Points
        {
            get { return points; }
        }
        private List<Point> points = new List<Point>();
        
        internal CGColor Color = UIColor.Black.CGColor;
        internal bool NeedsDrawing = true;
        internal UIView Parent;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InkStroke"/> class.
        /// </summary>
        public InkStroke()
        {
            LineJoinStyle = CGLineJoin.Round;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InkStroke"/> class.
        /// </summary>
        /// <param name="points">A collection of <see cref="Point"/> objects that defines the shape of the stroke.</param>
        public InkStroke(IEnumerable<Point> points)
            : this()
        {
            var array = points.ToArray();
            if (array.Length > 0)
            {
                base.MoveTo(array[0].GetCGPoint());
                this.points.Add(array[0]);
                
                for (int i = 1; i < array.Length - 2;)
                {
                    var point1 = array[i++];
                    var point2 = array[i++];
                    var point3 = array[i++];
                    
                    this.points.Add(point1);
                    this.points.Add(point2);
                    this.points.Add(point3);
                    
                    base.AddCurveToPoint(point3.GetCGPoint(), point1.GetCGPoint(), point2.GetCGPoint());
                    base.MoveTo(point3.GetCGPoint());
                }
            }
        }
        
        /// <summary></summary>
        /// <param name="endPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        public override void AddCurveToPoint(CGPoint endPoint, CGPoint controlPoint1, CGPoint controlPoint2)
        {
            base.AddCurveToPoint(endPoint, controlPoint1, controlPoint2);
            points.Add(controlPoint1.GetPoint());
            points.Add(controlPoint2.GetPoint());
            points.Add(endPoint.GetPoint());
        }

        /// <summary>
        /// Returns a deep-copy clone of this instance.
        /// </summary>
        public INativeInkStroke Clone()
        {
            return new InkStroke(points)
            {
                Color = Color,
                LineWidth = LineWidth,
                LineCapStyle = LineCapStyle
            };
        }

        /// <summary>
        /// Returns a copy of the ink stroke's drawing attributes.
        /// </summary>
        public InkDrawingAttributes CopyDrawingAttributes()
        {
            return new InkDrawingAttributes
            {
                Color = Color.GetColor(),
                Size = LineWidth,
                PenTip = LineCapStyle == CGLineCap.Round ? PenTipShape.Circle : PenTipShape.Square
            };
        }
        
        /// <summary></summary>
        /// <param name="point"></param>
        public override void MoveTo(CGPoint point)
        {
            base.MoveTo(point);
            
            var newPoint = point.GetPoint();
            if (points.Count == 0 || points.Last() != newPoint)
            {
                points.Add(newPoint);
            }
        }

        /// <summary>
        /// Updates the drawing attributes of the ink stroke.
        /// </summary>
        /// <param name="attributes">The drawing attributes to apply to the ink stroke.</param>
        public void UpdateDrawingAttributes(InkDrawingAttributes attributes)
        {
            Color = attributes.Color.GetCGColor();
            LineWidth = (nfloat)attributes.Size;
            LineCapStyle = attributes.PenTip == PenTipShape.Circle ? CGLineCap.Round : CGLineCap.Butt;
            
            NeedsDrawing = true;
            Parent?.SetNeedsDisplay();
        }
    }
}
