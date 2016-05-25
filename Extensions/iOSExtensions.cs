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
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.Systems;
using Prism.UI;
using Prism.UI.Controls;
using Prism.UI.Media;
using Prism.Utilities;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Provides methods for converting Prism objects to iOS objects and vice versa.
    /// </summary>
    public static class iOSExtensions
    {
        private static readonly WeakEventManager imageLoadedEventManager = new WeakEventManager("ImageLoaded", typeof(INativeBitmapImage));
        
        /// <summary>
        /// Checks the state of the image brush's image.  If the image is not loaded, the specified handler
        /// is attached to the image's load event and loading is initiated.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to attach to the ImageLoaded event of the brush's image if the image is not already loaded.</param>
        /// <returns>If the image is already loaded, the UIImage instance; otherwise, <c>null</c>.</returns>
        public static UIImage BeginLoadingImage(this ImageBrush brush, EventHandler handler)
        {
            return (ObjectRetriever.GetNativeObject(brush?.Image) as INativeImageSource).BeginLoadingImage(handler);
        }

        /// <summary>
        /// Checks the state of the image.  If the image is not loaded, the specified handler
        /// is attached to the image's load event and loading is initiated.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to attach to the ImageLoaded event of the image if the image is not already loaded.</param>
        /// <returns>If the image is already loaded, the UIImage instance; otherwise, <c>null</c>.</returns>
        public static UIImage BeginLoadingImage(this INativeImageSource source, EventHandler handler)
        {
            if (source == null)
            {
                return null;
            }
            
            var bitmapImage = source as INativeBitmapImage;
            if (bitmapImage == null)
            {
                return source.GetImageSource();
            }

            if (handler != null)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
                imageLoadedEventManager.AddHandler(bitmapImage, handler);
            }

            if (bitmapImage.IsLoaded)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
                return bitmapImage.GetImageSource();
            }
            else if (bitmapImage.IsFaulted)
            {
                imageLoadedEventManager.RemoveHandler(bitmapImage, handler);
            }
            else
            {
                (bitmapImage as ILazyLoader)?.LoadInBackground();
            }

            return null;
        }

        /// <summary>
        /// Removes the specified handler from the brush image's load event.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this ImageBrush brush, EventHandler handler)
        {
            var image = ObjectRetriever.GetNativeObject(brush?.Image) as INativeImageSource;
            if (image != null)
            {
                imageLoadedEventManager.RemoveHandler(image, handler);
            }
        }

        /// <summary>
        /// Removes the specified handler from the image's load event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this INativeImageSource source, EventHandler handler)
        {
            if (source != null)
            {
                imageLoadedEventManager.RemoveHandler(source, handler);
            }
        }

        /// <summary>
        /// Gets an <see cref="ActionKeyType"/> from a <see cref="UIReturnKeyType"/>.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        public static ActionKeyType GetActionKeyType(this UIReturnKeyType keyType)
        {
            switch (keyType)
            {
                case UIReturnKeyType.Done:
                    return ActionKeyType.Done;
                case UIReturnKeyType.Go:
                    return ActionKeyType.Go;
                case UIReturnKeyType.Next:
                    return ActionKeyType.Next;
                case UIReturnKeyType.Search:
                    return ActionKeyType.Search;
                default:
                    return ActionKeyType.Default;
            }
        }

        /// <summary>
        /// Gets a <see cref="CGColor"/> from a <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        public static CGColor GetCGColor(this Color color)
        {
            return new CGColor((nfloat)color.R / 255, (nfloat)color.G / 255,
                (nfloat)color.B / 255, (nfloat)color.A / 255);
        }

        /// <summary>
        /// Gets a <see cref="Color"/> from a <see cref="CGColor"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        public static Color GetColor(this CGColor color)
        {
            if (color == null)
            {
                return new Color();
            }

            var components = new byte[4];
            for (int i = 0; i < color.Components.Length; i++)
            {
                components[i] = (byte)(color.Components[i] * 255);
            }

            // components are in rgba order
            return new Color(components[3], components[0], components[1], components[2]);
        }

        /// <summary>
        /// Gets a <see cref="UIColor"/> from a <see cref="UIImage"/>.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="width">The width of the area to which the color will be applied.</param>
        /// <param name="height">The height of the area to which the color will be applied.</param>
        /// <param name="stretch">The manner in which to stretch the image.</param>
        public static UIColor GetColor(this UIImage image, nfloat width, nfloat height, Stretch stretch)
        {
            if (image == null)
            {
                return null;
            }

            if (stretch == Stretch.None)
            {
                return UIColor.FromPatternImage(image);
            }

            CGSize size = new CGSize(width, height);
            switch (stretch)
            {
                case Stretch.Uniform:
                    nfloat scale = NMath.Min(width / image.Size.Width, height / image.Size.Height);
                    size.Width = image.Size.Width * scale;
                    size.Height = image.Size.Height * scale;
                    break;
                case Stretch.UniformToFill:
                    scale = NMath.Max(width / image.Size.Width, height / image.Size.Height);
                    size.Width = image.Size.Width * scale;
                    size.Height = image.Size.Height * scale;
                    break;
            }

            UIGraphics.BeginImageContextWithOptions(size, false, 0);
            var context = UIGraphics.GetCurrentContext();
            if (context == null)
            {
                return null;
            }

            image.Draw(new CGRect(CGPoint.Empty, size));
            image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image == null ? null : UIColor.FromPatternImage(image);
        }

        /// <summary>
        /// Gets a <see cref="UIColor"/> representation of the brush.  If the brush is an <see cref="ImageBrush"/>
        /// and the image has not yet loaded, loading will be initiated and <c>null</c> will be returned.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="width">The width of the area to which the color will be applied.</param>
        /// <param name="height">The height of the area to which the color will be applied.</param>
        /// <param name="handler">The event handler to invoke when an image brush's image has loaded.</param>
        public static UIColor GetColor(this Brush brush, nfloat width, nfloat height, EventHandler handler)
        {
            var solid = brush as SolidColorBrush;
            if (solid != null)
            {
                return UIColor.FromCGColor(solid.Color.GetCGColor());
            }

            var image = brush as ImageBrush;
            if (image != null)
            {
                return image.BeginLoadingImage(handler).GetColor(width, height, image.Stretch);
            }

            var linear = brush as LinearGradientBrush;
            if (linear != null)
            {
                CGSize size = new CGSize(width, height);
                UIGraphics.BeginImageContextWithOptions(size, false, 0);
                var context = UIGraphics.GetCurrentContext();
                if (context == null)
                {
                    return null;
                }

                var colorspace = CGColorSpace.CreateDeviceRGB();

                var gradient = new CGGradient(colorspace, linear.Colors.Select(c => c.GetCGColor()).ToArray());
                context.DrawLinearGradient(gradient, new CGPoint((nfloat)linear.StartPoint.X * width, (nfloat)linear.StartPoint.Y * height),
                    new CGPoint((nfloat)linear.EndPoint.X * width, (nfloat)linear.EndPoint.Y * height),
                    CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);

                var uiimage = UIGraphics.GetImageFromCurrentImageContext();

                UIGraphics.EndImageContext();

                return UIColor.FromPatternImage(uiimage);
            }

            return null;
        }

        /// <summary>
        /// Gets a <see cref="UIViewContentMode"/> value from a <see cref="Stretch"/> value.
        /// </summary>
        /// <param name="stretch">The Stretch value.</param>
        public static UIViewContentMode GetContentMode(this Stretch stretch)
        {
            switch (stretch)
            {
                case Stretch.Fill:
                    return UIViewContentMode.ScaleToFill;
                case Stretch.Uniform:
                    return UIViewContentMode.ScaleAspectFit;
                case Stretch.UniformToFill:
                    return UIViewContentMode.ScaleAspectFill;
                default:
                    return UIViewContentMode.Center;
            }
        }

        /// <summary>
        /// Gets a <see cref="FontStyle"/> from a <see cref="UIFont"/>.
        /// </summary>
        /// <param name="font">The font.</param>
        public static FontStyle GetFontStyle(this UIFont font)
        {
            if (font == null)
            {
                return FontStyle.Normal;
            }

            var style = FontStyle.Normal;
            if (font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold))
            {
                style |= FontStyle.Bold;
            }
            if (font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic))
            {
                style |= FontStyle.Italic;
            }

            return style;
        }

        /// <summary>
        /// Gets a <see cref="UIFont"/> from a <see cref="FontFamily"/> with the specified size and style.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="fontStyle">The style of the font.</param>
        public static UIFont GetUIFont(this UI.Media.FontFamily fontFamily, double fontSize, FontStyle fontStyle)
        {
            if (fontFamily == null)
            {
                return UIFont.SystemFontOfSize((nfloat)fontSize);
            }

            var traits = fontFamily.Traits;
            if (fontStyle.HasFlag(FontStyle.Bold))
            {
                traits |= UIFontDescriptorSymbolicTraits.Bold;
            }
            if (fontStyle.HasFlag(FontStyle.Italic))
            {
                traits |= UIFontDescriptorSymbolicTraits.Italic;
            }

            var descriptor = new UIFontDescriptor().CreateWithFamily(fontFamily.Name).CreateWithTraits(traits);
            if (descriptor == null)
            {
                // Something went wrong.  Fall back to a more primitive option.
                string faceAttributes = fontStyle == FontStyle.BoldItalic ? "Bold Italic" :
                    fontStyle == FontStyle.Bold ? "Bold" : fontStyle == FontStyle.Italic ? "Italic" : "Regular";

                descriptor = new UIFontDescriptor(NSDictionary.FromObjectsAndKeys(
                    new object[] { fontFamily.Name, faceAttributes },
                    new object[] { "NSFontFamilyAttribute", "NSFontFaceAttribute" }
                ));
            }

            return UIFont.FromDescriptor(descriptor, (nfloat)fontSize);
        }

        /// <summary>
        /// Gets an <see cref="UIImage"/> from an <see cref="INativeImageSource"/>.
        /// </summary>
        /// <param name="source">The image.</param>
        public static UIImage GetImageSource(this INativeImageSource source)
        {
            var image = source as UI.Media.Imaging.IImageSource;
            return image == null ? source as UIImage : image.Source;
        }

        /// <summary>
        /// Gets a <see cref="UIEdgeInsets"/> from a <see cref="Thickness"/>.
        /// </summary>
        /// <param name="thickness">The thickness.</param>
        public static UIEdgeInsets GetInsets(this Thickness thickness)
        {
            return new UIEdgeInsets((nfloat)thickness.Top, (nfloat)thickness.Left, (nfloat)thickness.Bottom, (nfloat)thickness.Right);
        }

        /// <summary>
        /// Gets a <see cref="CGPoint"/> from a <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        public static CGPoint GetCGPoint(this Point point)
        {
            return new CGPoint((nfloat)point.X, (nfloat)point.Y);
        }

        /// <summary>
        /// Gets a <see cref="Point"/> from a <see cref="CGPoint"/>.
        /// </summary>
        /// <param name="point">The point.</param>
        public static Point GetPoint(this CGPoint point)
        {
            return new Point(point.X, point.Y);
        }
        
        /// <summary>
        /// Generates a <see cref="PointerEventArgs"/> from a <see cref="UIEvent"/>.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <param name="touch">The event's touch.</param>
        /// <param name="source">The source of the event.</param>
        public static PointerEventArgs GetPointerEventArgs(this UIEvent evt, UITouch touch, UIView source)
        {
            bool v9 = UIDevice.CurrentDevice.CheckSystemVersion(9, 0);
            return new PointerEventArgs(source, v9 ? touch.Type.GetPointerType() : PointerType.Unknown,
                touch.LocationInView(source).GetPoint(), v9 ? touch.Force : 1, (long)(evt.Timestamp * 1000));
        }
        
        /// <summary>
        /// Gets a <see cref="PointerType"/> from a <see cref="UITouchType"/>.
        /// </summary>
        /// <param name="type">The touch type.</param>
        public static PointerType GetPointerType(this UITouchType type)
        {
            switch (type)
            {
                case UITouchType.Direct:
                case UITouchType.Indirect:
                    return PointerType.Touch;
                case UITouchType.Stylus:
                    return PointerType.Stylus;
                default:
                    return PointerType.Unknown;
            }
        }

        /// <summary>
        /// Gets a <see cref="PowerSource"/> from a <see cref="UIDeviceBatteryState"/>.
        /// </summary>
        /// <param name="state">The battery state.</param>
        public static PowerSource GetPowerSource(this UIDeviceBatteryState state)
        {
            switch (state)
            {
                case UIDeviceBatteryState.Charging:
                case UIDeviceBatteryState.Full:
                    return PowerSource.External;
                case UIDeviceBatteryState.Unplugged:
                    return PowerSource.Battery;
                default:
                    return PowerSource.Unknown;
            }
        }

        /// <summary>
        /// Gets a <see cref="CGRect"/> from a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        public static CGRect GetCGRect(this Rectangle rect)
        {
            return new CGRect((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> from a <see cref="CGRect"/>.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        public static Rectangle GetRectangle(this CGRect rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Gets a <see cref="UIReturnKeyType"/> from an <see cref="ActionKeyType"/>.
        /// </summary>
        /// <param name="keyType">The key type.</param>
        public static UIReturnKeyType GetReturnKeyType(this ActionKeyType keyType)
        {
            switch (keyType)
            {
                case ActionKeyType.Done:
                    return UIReturnKeyType.Done;
                case ActionKeyType.Go:
                    return UIReturnKeyType.Go;
                case ActionKeyType.Next:
                    return UIReturnKeyType.Next;
                case ActionKeyType.Search:
                    return UIReturnKeyType.Search;
                default:
                    return UIReturnKeyType.Default;
            }
        }

        /// <summary>
        /// Gets a <see cref="CGSize"/> from a <see cref="Size"/>.
        /// </summary>
        /// <param name="size">The size.</param>
        public static CGSize GetCGSize(this Size size)
        {
            return new CGSize((nfloat)size.Width, (nfloat)size.Height);
        }

        /// <summary>
        /// Gets a <see cref="Size"/> from a <see cref="CGSize"/>.
        /// </summary>
        /// <param name="size">The size.</param>
        public static Size GetSize(this CGSize size)
        {
            return new Size(size.Width, size.Height);
        }

        /// <summary>
        /// Gets a <see cref="Stretch"/> value from a <see cref="UIViewContentMode"/> value.
        /// </summary>
        /// <param name="mode">The UIViewContentMode value.</param>
        public static Stretch GetStretch(this UIViewContentMode mode)
        {
            switch (mode)
            {
                case UIViewContentMode.ScaleToFill:
                    return Stretch.Fill;
                case UIViewContentMode.ScaleAspectFit:
                    return Stretch.Uniform;
                case UIViewContentMode.ScaleAspectFill:
                    return Stretch.UniformToFill;
                default:
                    return Stretch.None;
            }
        }

        /// <summary>
        /// Gets the size of the string as a <see cref="CGSize"/> instance.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="constraints">The maximum width and height.</param>
        /// <param name="font">The font with which the text is rendered.</param>
        public static CGSize GetStringSize(this string text, CGSize constraints, UIFont font)
        {
            return string.IsNullOrEmpty(text) ? CGSize.Empty : new NSString(text).GetBoundingRect(constraints,
                NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.TruncatesLastVisibleLine,
                new UIStringAttributes() { Font = font }, new NSStringDrawingContext()).Size;
        }

        /// <summary>
        /// Gets a <see cref="UITextAlignment"/> from a <see cref="TextAlignment"/>.
        /// </summary>
        /// <param name="alignment">The text alignment.</param>
        public static UITextAlignment GetTextAlignment(this TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Center:
                    return UITextAlignment.Center;
                case TextAlignment.Justified:
                    return UITextAlignment.Justified;
                case TextAlignment.Right:
                    return UITextAlignment.Right;
                default:
                    return UITextAlignment.Left;
            }
        }

        /// <summary>
        /// Gets a <see cref="TextAlignment"/> from a <see cref="UITextAlignment"/>.
        /// </summary>
        /// <param name="alignment">The text alignment.</param>
        public static TextAlignment GetTextAlignment(this UITextAlignment alignment)
        {
            switch (alignment)
            {
                case UITextAlignment.Center:
                    return TextAlignment.Center;
                case UITextAlignment.Justified:
                    return TextAlignment.Justified;
                case UITextAlignment.Right:
                    return TextAlignment.Right;
                default:
                    return TextAlignment.Left;
            }
        }

        /// <summary>
        /// Gets a <see cref="Thickness"/> from a <see cref="UIEdgeInsets"/>.
        /// </summary>
        /// <param name="insets">The insets.</param>
        public static Thickness GetThickness(this UIEdgeInsets insets)
        {
            return new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom);
        }
    }
}

