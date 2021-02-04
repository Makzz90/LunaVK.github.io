using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace LunaVK.Core.Utils
{
    public static class RectangleUtils
    {
        public static Rect ResizeToFit(Rect parentRect, Size childSize)
        {
            Rect fit = RectangleUtils.ResizeToFit(RectangleUtils.GetSize(parentRect), childSize);
            fit.X = fit.X + parentRect.X;
            fit.Y = fit.Y + parentRect.Y;
            return fit;
        }

        public static Rect ResizeToFit(Size parentSize, Size childSize)
        {
            if (parentSize.Height == 0.0 || parentSize.Width == 0.0 || ( childSize.Width == 0.0 || childSize.Height == 0.0 ) )
                return new Rect();
            double num1 = parentSize.Width / parentSize.Height;
            double num2 = childSize.Width / childSize.Height;
            Rect rect = new Rect();
            double num3 = num2;
            if (num1 < num3)
            {
                double num4 = parentSize.Width / childSize.Width;
                rect.Width = (parentSize.Width);
                rect.Height = (childSize.Height * num4);
                rect.Y = ((parentSize.Height - rect.Height) / 2.0);
            }
            else
            {
                double num4 = parentSize.Height / childSize.Height;
                rect.Height = (parentSize.Height);
                rect.Width = (childSize.Width * num4);
                rect.X = ((parentSize.Width - rect.Width) / 2.0);
            }
            return rect;
        }

        public static Size GetSize(Rect rect)
        {
            return new Size(rect.Width, rect.Height);
        }

        public static Rect AlignRects(Rect parentRect, Rect childRect, bool fill)
        {
            bool flag1 = Math.Abs(childRect.Bottom - parentRect.Bottom) < Math.Abs(childRect.Top - parentRect.Top);
            bool flag2 = Math.Abs(childRect.Left - parentRect.Left) < Math.Abs(childRect.Right - parentRect.Right);
            double num1 = Math.Max(parentRect.Width / childRect.Width, parentRect.Height / childRect.Height);
            if (num1 > 1.0 & fill)
            {
                childRect.Width = childRect.Width * num1;
                childRect.Height = childRect.Height * num1;
            }
            double num4 = 0.0;
            double num5 = 0.0;

            if (childRect.Top > parentRect.Top ^ childRect.Bottom < parentRect.Bottom)
            {
                num5 = !flag1 ? parentRect.Top - childRect.Top : parentRect.Bottom - childRect.Bottom;
            }
            
            if (childRect.Left > parentRect.Left ^ childRect.Right < parentRect.Right)
            {
                num4 = !flag2 ? parentRect.Right - childRect.Right : parentRect.Left - childRect.Left;
            }

            childRect.X = childRect.X + num4;
            childRect.Y = childRect.Y + num5;
            if (!fill)
            {
                if (childRect.Height < parentRect.Height && (childRect.Top > parentRect.Top || childRect.Bottom < parentRect.Bottom))
                {
                    childRect.Y = (parentRect.Y + (parentRect.Height - childRect.Height) / 2.0);
                }
                
                if (childRect.Width < parentRect.Width && (childRect.Left > parentRect.Left || childRect.Right < parentRect.Right))
                {
                    childRect.X = (parentRect.Y + (parentRect.Width - childRect.Width) / 2.0);
                }
            }
            return childRect;
        }

        public static CompositeTransform TransformRect(Rect source, Rect target, bool inSourceCenterCoord = false)
        {
            if (source.Width == 0.0 || source.Height == 0.0 || target.Width == 0.0 || target.Height == 0.0)
                return new CompositeTransform();
            if (inSourceCenterCoord)
            {
                return RectangleUtils.TransformRect(new Rect(source.X - source.Width / 2.0, source.Y - source.Height / 2.0, source.Width, source.Height), new Rect(target.X - source.Width / 2.0, target.Y - source.Height / 2.0, target.Width, target.Height), false);
            }
            CompositeTransform compositeTransform = new CompositeTransform();
            compositeTransform.ScaleX = target.Width / source.Width;
            compositeTransform.ScaleY = target.Height / source.Height;
            compositeTransform.TranslateX = target.X - source.X * compositeTransform.ScaleX;
            compositeTransform.TranslateY = target.Y - source.Y * compositeTransform.ScaleY;
            return compositeTransform;
        }

        public static Rect ResizeToFill(Rect parentRect, Size childSize)
        {
            Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.GetSize(parentRect), childSize);
            fill.X += parentRect.X;
            fill.Y += parentRect.Y;
            return fill;
        }

        public static Rect ResizeToFill(Size parentSize, Size childSize)
        {            
            if (parentSize.Height == 0.0 || parentSize.Width == 0.0 || (childSize.Width == 0.0 || childSize.Height == 0.0))
                return new Rect();
            
            double num1 = parentSize.Width / parentSize.Height;
            double num2 = childSize.Width / childSize.Height;
            Rect rect = new Rect();
            double num3 = num2;
            if (num1 < num3)
            {
                double num4 = parentSize.Height / childSize.Height;
                rect.Width = (childSize.Width * num4);
                rect.Height = (parentSize.Height);
                rect.X = ((parentSize.Width - rect.Width) / 2.0);
            }
            else
            {
                double num4 = parentSize.Width / childSize.Width;
                rect.Height = (childSize.Height * num4);
                rect.Width = (parentSize.Width);
                rect.Y = ((parentSize.Height - rect.Height) / 2.0);
            }
            return rect;
        }

        public static Rect Rotate90(Rect source)
        {
            return new Rect(source.X + (source.Width - source.Height) / 2.0, source.Y + (source.Height - source.Width) / 2.0, source.Height, source.Width);
        }

        public static Rect CalculateRelative(Size parentSize, Rect relativeRect)
        {
            Rect rect = new Rect();
            rect.X = parentSize.Width * relativeRect.X;
            rect.Y = parentSize.Height * relativeRect.Y;
            rect.Width = parentSize.Width * relativeRect.Width;
            rect.Height = parentSize.Height * relativeRect.Height;
            return rect;
        }

        public static Rect ResizeToFitIfNotContained(Size parentSize, Size childSize)
        {
            if (childSize.Width > parentSize.Width || childSize.Height > parentSize.Height)
                return RectangleUtils.ResizeToFit(parentSize, childSize);
            return new Rect(new Point(), childSize);
        }
    }
}
