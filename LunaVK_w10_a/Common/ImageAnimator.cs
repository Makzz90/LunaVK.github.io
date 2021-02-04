using LunaVK.Core.Utils;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.Common
{
    public class ImageAnimator
    {
        private int _animationDurationMs;
        private EasingFunctionBase _easingFunction;
        
        public ImageAnimator(int animationDurationMs, EasingFunctionBase easingFunction)
        {
            this._animationDurationMs = animationDurationMs;
            this._easingFunction = easingFunction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageOriginal">104 157</param>
        /// <param name="imageFit">854 480</param>
        /// <param name="completionCallback"></param>
        /// <param name="startTime"></param>
        public void AnimateIn(/*Size imageSize, */FrameworkElement imageOriginal, FrameworkElement imageFit, Action completionCallback = null, int startTime = 0)
        {
            if (imageOriginal == null)
            {
                completionCallback();
            }
            else
            {
                Size childSize = new Size(imageFit.Width, imageFit.Height);
                if(double.IsNaN( childSize.Width))
                {
                    childSize = new Size(imageFit.ActualWidth, imageFit.ActualHeight);
                }

                //Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), imageSize), childSize);//orig
                Rect fill = RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), childSize);

                Rect target = imageOriginal.TransformToVisual(imageFit).TransformBounds(fill);
                imageFit.RenderTransform = RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false);
                GeneralTransform visual = imageOriginal.TransformToVisual(imageFit);

                Rect rect1;

                if (imageOriginal.Parent is FrameworkElement parent)
                    rect1 = new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight);
                else
                    rect1 = new Rect(0.0, 0.0, imageOriginal.ActualWidth, imageOriginal.ActualHeight);//orig

                Rect source = visual.TransformBounds(rect1);

                CompositeTransform compositeTransform1 = new CompositeTransform();
                RectangleGeometry rectangleGeometry = new RectangleGeometry();
                rectangleGeometry.Rect = source;
                rectangleGeometry.Transform = compositeTransform1;
                imageFit.Clip = rectangleGeometry;

                CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, new Rect(new Point(), childSize), false);
                compositeTransform1.Animate(0.0, compositeTransform3.TranslateY, "TranslateY", this._animationDurationMs, startTime, this._easingFunction);
                compositeTransform1.Animate(0.0, compositeTransform3.TranslateX, "TranslateX", this._animationDurationMs, startTime, this._easingFunction);
                compositeTransform1.Animate(1.0, compositeTransform3.ScaleX, "ScaleX", this._animationDurationMs, startTime, this._easingFunction);
                compositeTransform1.Animate(1.0, compositeTransform3.ScaleY, "ScaleY", this._animationDurationMs, startTime, this._easingFunction);

                CompositeTransform renderTransform = imageFit.RenderTransform as CompositeTransform;
                renderTransform.Animate(renderTransform.TranslateX, 0.0, "TranslateX", this._animationDurationMs, startTime, this._easingFunction, completionCallback);
                renderTransform.Animate(renderTransform.TranslateY, 0.0, "TranslateY", this._animationDurationMs, startTime, this._easingFunction);

                //Scale new image?
                renderTransform.Animate(renderTransform.ScaleX, 1.0, "ScaleX", this._animationDurationMs, startTime, this._easingFunction);
                renderTransform.Animate(renderTransform.ScaleY, 1.0, "ScaleY", this._animationDurationMs, startTime, this._easingFunction);
            }
        }

        public void AnimateOut(/*Size imageSize,*/ FrameworkElement imageOriginal, FrameworkElement imageFit, bool? clockwiseRotation, Action completionCallback = null)
        {
            CompositeTransform renderTransform = imageFit.RenderTransform as CompositeTransform;
            if (imageOriginal == null || renderTransform.ScaleX != 1.0)
            {
                //если нет картинки, то просто сдвигаем вниз
                this.AnimateFlyout(completionCallback, renderTransform);
            }
            else
            {
                Size childSize = new Size(imageFit.ActualWidth, imageFit.ActualHeight);
                //Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), imageSize), childSize);
                Rect fill = RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), childSize);
                Rect rect = imageOriginal.TransformToVisual(imageFit).TransformBounds(fill);
                if (clockwiseRotation.HasValue)
                    rect = RectangleUtils.Rotate90(rect);
                CompositeTransform compositeTransform1 = RectangleUtils.TransformRect(new Rect(new Point(), childSize), rect, true);
                renderTransform.CenterX = imageFit.Width / 2.0;
                renderTransform.CenterY = imageFit.Height / 2.0;

                double num1 = imageFit.Width / fill.Width;

                //Rect target = new Rect(-fill.X * num1, -fill.Y * num1, imageOriginal.ActualWidth * num1, (imageOriginal.ActualHeight) * num1);
                // Rect target = new Rect(-fill.X * num1, -fill.Y * num1 * 2.15, (imageOriginal.Parent as FrameworkElement).ActualWidth * num1, ((imageOriginal.Parent as FrameworkElement).ActualHeight) * num1);//BugFix
                //Rect target = new Rect(-fill.X * num1, -fill.Y * num1, (imageOriginal.Parent as FrameworkElement).ActualWidth * num1, ((imageOriginal.Parent as FrameworkElement).ActualHeight) * num1);//BugFix
                Rect target = new Rect(-fill.X * num1, -fill.Y * num1, imageOriginal.ActualWidth * num1, imageOriginal.ActualHeight * num1);
                if (target.Width < 10.0 || target.Height < 10.0)
                {
                    this.AnimateFlyout(completionCallback, renderTransform);
                }
                else
                {
                    RectangleGeometry rectangleGeometry = new RectangleGeometry();
                    Rect source = new Rect(0.0, 0.0, imageFit.Width, imageFit.Height);
                    rectangleGeometry.Rect = source;
                    imageFit.Clip = rectangleGeometry;
                    CompositeTransform compositeTransform2 = new CompositeTransform();
                    rectangleGeometry.Transform = compositeTransform2;
                    CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, target, false);
                    compositeTransform2.Animate(0.0, compositeTransform3.TranslateY, "TranslateY", this._animationDurationMs, 0, this._easingFunction);
                    compositeTransform2.Animate(0.0, compositeTransform3.TranslateX, "TranslateX", this._animationDurationMs, 0, this._easingFunction);
                    compositeTransform2.Animate(1.0, compositeTransform3.ScaleX, "ScaleX", this._animationDurationMs, 0, this._easingFunction);
                    compositeTransform2.Animate(1.0, compositeTransform3.ScaleY, "ScaleY", this._animationDurationMs, 0, this._easingFunction);
                    if (clockwiseRotation.HasValue)
                        renderTransform.Animate(renderTransform.Rotation, clockwiseRotation.Value ? renderTransform.Rotation + 90.0 : renderTransform.Rotation - 90.0, "Rotation", this._animationDurationMs, 0, this._easingFunction);
                    renderTransform.Animate(renderTransform.TranslateX, renderTransform.TranslateX + compositeTransform1.TranslateX, "TranslateX", this._animationDurationMs, 0, this._easingFunction);
                    renderTransform.Animate(renderTransform.TranslateY, renderTransform.TranslateY + compositeTransform1.TranslateY, "TranslateY", this._animationDurationMs, 0, this._easingFunction);
                    renderTransform.Animate(renderTransform.ScaleX, compositeTransform1.ScaleX, "ScaleX", this._animationDurationMs, 0, this._easingFunction, null);
                    renderTransform.Animate(renderTransform.ScaleY, compositeTransform1.ScaleY, "ScaleY", this._animationDurationMs, 0, this._easingFunction, completionCallback);
                }
            }
        }

        private void AnimateFlyout(Action completionCallback, CompositeTransform imageFitTransform)
        {
            ExponentialEase exponentialEase = new ExponentialEase() { Exponent = 6, EasingMode = EasingMode.EaseIn };
            imageFitTransform.Animate(imageFitTransform.TranslateY, 1000, "TranslateY", this._animationDurationMs, 0, exponentialEase, completionCallback);
        }
    }
}
