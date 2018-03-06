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
using System.Linq;
using AVFoundation;
using CoreGraphics;
using Foundation;
using Prism.Input;
using Prism.iOS.UI.Media.Imaging;
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
        private static readonly WeakEventManager imageChangedEventManager = new WeakEventManager("SourceChanged", typeof(IImageSource));

        /// <summary>
        /// Checks the state of the image brush's image.  If the image is not loaded, loading is initiated.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to attach to the SourceChanged event of the brush's image.</param>
        /// <returns>If the image is already loaded, the UIImage instance; otherwise, <c>null</c>.</returns>
        public static UIImage BeginLoadingImage(this ImageBrush brush, EventHandler handler)
        {
            return (ObjectRetriever.GetNativeObject(brush?.Image) as INativeImageSource).BeginLoadingImage(handler);
        }

        /// <summary>
        /// Checks the state of the image.  If the image is not loaded, loading is initiated.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to attach to the SourceChanged event of the image.</param>
        /// <returns>If the image is already loaded, the UIImage instance; otherwise, <c>null</c>.</returns>
        public static UIImage BeginLoadingImage(this INativeImageSource source, EventHandler handler)
        {
            if (source == null)
            {
                return null;
            }

            if (handler != null && source is IImageSource)
            {
                imageChangedEventManager.RemoveHandler(source, handler);
                imageChangedEventManager.AddHandler(source, handler);
            }

            var bitmapImage = source as INativeBitmapImage;
            if (bitmapImage == null || bitmapImage.IsLoaded)
            {
                return source.GetImageSource();
            }
            else if (bitmapImage.IsFaulted)
            {
                imageChangedEventManager.RemoveHandler(bitmapImage, handler);
            }
            else
            {
                (bitmapImage as ILazyLoader)?.LoadInBackground();
            }

            return null;
        }

        /// <summary>
        /// Removes the specified handler from the brush image's SourceChanged event.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this ImageBrush brush, EventHandler handler)
        {
            var image = ObjectRetriever.GetNativeObject(brush?.Image) as IImageSource;
            if (image != null)
            {
                imageChangedEventManager.RemoveHandler(image, handler);
            }
        }

        /// <summary>
        /// Removes the specified handler from the image's SourceChanged event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler to be removed.</param>
        public static void ClearImageHandler(this INativeImageSource source, EventHandler handler)
        {
            if (source is IImageSource)
            {
                imageChangedEventManager.RemoveHandler(source, handler);
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
        /// Gets a <see cref="CGAffineTransform"/> from a <see cref="Matrix"/>.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public static CGAffineTransform GetCGAffineTransform(this Matrix matrix)
        {
            return new CGAffineTransform((nfloat)matrix.M11, (nfloat)matrix.M12, (nfloat)matrix.M21,
                (nfloat)matrix.M22, (nfloat)matrix.OffsetX, (nfloat)matrix.OffsetY);
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
            for (int i = 0; i < 3; i++)
            {
                components[i] = (byte)(color.Components[Math.Min(i, color.Components.Length - 2)] * 255);
            }
            components[3] = (byte)(color.Components.Last() * 255);

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
            var data = brush as DataBrush;
            if (data != null)
            {
                return data.Data as UIColor;
            }

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
        /// Gets a <see cref="DisplayOrientations"/> from a <see cref="UIInterfaceOrientation"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public static DisplayOrientations GetDisplayOrientations(this UIInterfaceOrientation orientation)
        {
            var retval = DisplayOrientations.None;
            if (orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown)
            {
                retval |= DisplayOrientations.Portrait;
            }

            if (orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight)
            {
                retval |= DisplayOrientations.Landscape;
            }

            return retval;
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
        /// Gets a <see cref="AVLayerVideoGravity"/> value from a <see cref="Stretch"/> value.
        /// </summary>
        /// <param name="stretch">The Stretch value.</param>
        public static AVLayerVideoGravity GetLayerVideoGravity(this Stretch stretch)
        {
            switch (stretch)
            {
                case Stretch.Fill:
                    return AVLayerVideoGravity.Resize;
                case Stretch.UniformToFill:
                    return AVLayerVideoGravity.ResizeAspectFill;
                default:
                    return AVLayerVideoGravity.ResizeAspect;
            }
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
        /// Gets a <see cref="UIKeyboardType"/> from an <see cref="InputType"/>.
        /// </summary>
        /// <param name="inputType">The input type.</param>
        public static UIKeyboardType GetKeyboardType(this InputType inputType)
        {
            switch (inputType)
            {
                case InputType.Alphanumeric:
                    return UIKeyboardType.Default;
                case InputType.Number:
                    return UIKeyboardType.DecimalPad;
                case InputType.NumberAndSymbol:
                    return UIKeyboardType.NumbersAndPunctuation;
                case InputType.Phone:
                    return UIKeyboardType.PhonePad;
                case InputType.Url:
                    return UIKeyboardType.Url;
                case InputType.EmailAddress:
                    return UIKeyboardType.EmailAddress;
                default:
                    return (UIKeyboardType)((int)inputType - 6);
            }
        }

        /// <summary>
        /// Gets an <see cref="InputType"/> from a <see cref="UIKeyboardType"/>.
        /// </summary>
        /// <param name="keyboardType">The keyboard type.</param>
        public static InputType GetInputType(this UIKeyboardType keyboardType)
        {
            switch (keyboardType)
            {
                case UIKeyboardType.Default:
                    return InputType.Alphanumeric;
                case UIKeyboardType.DecimalPad:
                    return InputType.Number;
                case UIKeyboardType.NumbersAndPunctuation:
                    return InputType.NumberAndSymbol;
                case UIKeyboardType.PhonePad:
                    return InputType.Phone;
                case UIKeyboardType.Url:
                    return InputType.Url;
                case UIKeyboardType.EmailAddress:
                    return InputType.EmailAddress;
                default:
                    return (InputType)((int)keyboardType + 6);
            }
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
        /// Gets a <see cref="UIInterfaceOrientationMask"/> from a <see cref="DisplayOrientations"/>.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        public static UIInterfaceOrientationMask GetInterfaceOrientationMask(this DisplayOrientations orientation)
        {
            if (orientation == DisplayOrientations.None)
            {
                return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? UIInterfaceOrientationMask.AllButUpsideDown : UIInterfaceOrientationMask.All;
            }

            UIInterfaceOrientationMask retval = 0;
            if (orientation.HasFlag(DisplayOrientations.Portrait))
            {
                retval |= (UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown);
            }

            if (orientation.HasFlag(DisplayOrientations.Landscape))
            {
                retval |= UIInterfaceOrientationMask.Landscape;
            }

            return retval;
        }

        /// <summary>
        /// Gets a <see cref="CGLineCap"/> from a <see cref="LineCap"/>.
        /// </summary>
        /// <param name="lineCap">The line cap.</param>
        public static CGLineCap GetCGLineCap(this LineCap lineCap)
        {
            switch (lineCap)
            {
                case LineCap.Square:
                    return CGLineCap.Square;
                case LineCap.Round:
                    return CGLineCap.Round;
                default:
                    return CGLineCap.Butt;
            }
        }

        /// <summary>
        /// Gets a <see cref="CGLineJoin"/> from a <see cref="LineJoin"/>.
        /// </summary>
        /// <param name="lineJoin">The line join.</param>
        public static CGLineJoin GetCGLineJoin(this LineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case LineJoin.Bevel:
                    return CGLineJoin.Bevel;
                case LineJoin.Round:
                    return CGLineJoin.Round;
                default:
                    return CGLineJoin.Miter;
            }
        }

        /// <summary>
        /// Gets a <see cref="Matrix"/> from a <see cref="CGAffineTransform"/>.
        /// </summary>
        /// <param name="transform">The transform.</param>
        public static Matrix GetMatrix(this CGAffineTransform transform)
        {
            return new Matrix(transform.xx, transform.yx, transform.xy, transform.yy, transform.x0, transform.y0);
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
            return new PointerEventArgs(source, (uint)touch.Handle, v9 ? touch.Type.GetPointerType() : PointerType.Unknown,
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
        /// Gets a <see cref="UIPopoverArrowDirection"/> from a <see cref="FlyoutPlacement"/>.
        /// </summary>
        /// <param name="placement">The flyout placement.</param>
        public static UIPopoverArrowDirection GetPopoverArrowDirection(this FlyoutPlacement placement)
        {
            switch (placement)
            {
                case FlyoutPlacement.Auto:
                    return UIPopoverArrowDirection.Any;
                case FlyoutPlacement.Bottom:
                    return UIPopoverArrowDirection.Up;
                case FlyoutPlacement.Left:
                    return UIPopoverArrowDirection.Right;
                case FlyoutPlacement.Right:
                    return UIPopoverArrowDirection.Left;
                case FlyoutPlacement.Top:
                    return UIPopoverArrowDirection.Down;
                default:
                    return (UIPopoverArrowDirection)placement;
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

