using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.Framework
{
    //https://github.com/RSuter/MyToolkit/blob/master/src/MyToolkit.Extended/UI/TiltEffect.cs
    public static class MetroInMotion
    {
#region IsTiltEnabled
        /// <summary>Provides an attached property to enable the tilt effect (push down/up animation) for <see cref="UIElement"/>s. </summary>
        public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.RegisterAttached("IsTiltEnabled", typeof(bool), typeof(MetroInMotion), new PropertyMetadata(default(bool), OnIsTiltEnabledChanged));

        /// <summary>Sets a value indicating whether to enable tilt effect for the <see cref="UIElement"/>. </summary>
        /// <param name="element">The element. </param>
        /// <param name="value">The value. </param>
        public static void SetIsTiltEnabled(UIElement element, bool value)
        {
            element.SetValue(IsTiltEnabledProperty, value);
        }

        /// <summary>Gets a value indicating whether the tilt effect for the <see cref="UIElement"/> is enabled. </summary>
        /// <param name="element">The element. </param>
        public static bool GetIsTiltEnabled(UIElement element)
        {
            return (bool)element.GetValue(IsTiltEnabledProperty);
        }
        #endregion

        public static readonly DependencyProperty TiltProperty = DependencyProperty.RegisterAttached("Tilt", typeof(double), typeof(MetroInMotion), new PropertyMetadata(2.0));
        public static double GetTilt(DependencyObject obj)
        {
            return (double)obj.GetValue(MetroInMotion.TiltProperty);
        }

        public static void SetTilt(DependencyObject obj, double value)
        {
            obj.SetValue(MetroInMotion.TiltProperty, value);
        }

        private static double TiltAngleFactor = 4.0;
        private static double ScaleFactor = 100.0;

        private static void OnIsTiltEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = (FrameworkElement)obj;
            if ((bool)args.NewValue)
            {
                element.PointerPressed += OnPointerPressed;
                element.PointerReleased += OnPointerReleased;
                element.PointerExited += OnPointerExited;
                element.PointerEntered += OnPointerEntered;

                element.PointerMoved += OnPointerMoved;
            }
            else
            {
                element.PointerPressed -= OnPointerPressed;
                element.PointerReleased -= OnPointerReleased;
                element.PointerExited -= OnPointerExited;
                element.PointerEntered -= OnPointerEntered;

                element.PointerMoved -= OnPointerMoved;
            }

            element.Unloaded += Element_Unloaded;
        }

        private static void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.PointerPressed -= OnPointerPressed;
            element.PointerReleased -= OnPointerReleased;
            element.PointerExited -= OnPointerExited;
            element.PointerEntered -= OnPointerEntered;

            element.PointerMoved -= OnPointerMoved;
        }

        private static void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            /*
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var temp = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton);
                if (!temp.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    return;
            }
            */
            var temp2 = e.GetCurrentPoint((UIElement)sender);
            ShowDownAnimation((UIElement)sender, temp2.Position);
        }

        private static void OnPointerExited(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            ShowUpAnimation((UIElement)sender);
        }

        private static void OnPointerEntered(object sender, PointerRoutedEventArgs args)
        {
            //if (args.Pointer.IsInContact)
            //{
            var temp = args.GetCurrentPoint((UIElement)sender);
            ShowDownAnimation((UIElement)sender, temp.Position);
            //}
        }

        private static void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ShowUpAnimation((UIElement)sender);
        }

        private static void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var temp = e.GetCurrentPoint((UIElement)sender);
            ShowDownAnimation((UIElement)sender, temp.Position);
        }

        private static void ShowDownAnimation(UIElement element, Point pos)
        {
            FrameworkElement targetElement = element as FrameworkElement;

            PlaneProjection projection = targetElement.Projection as PlaneProjection;
            if (projection == null)
            {
                projection = new PlaneProjection();
                targetElement.Projection = projection;
            }

            TransformGroup transformGroup = targetElement.RenderTransform as TransformGroup;
            if(transformGroup==null)
            {
                transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform());
                transformGroup.Children.Add(new TranslateTransform());
                targetElement.RenderTransform = transformGroup;
                targetElement.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var scale = transformGroup.Children[0] as ScaleTransform;
            var translate = transformGroup.Children[1] as TranslateTransform;

            double tiltFactor = MetroInMotion.GetTilt(element);

            double num1 = Math.Max(targetElement.ActualWidth, targetElement.ActualHeight);
            if (num1 == 0.0)
                return;

            double num2 = 2.0 * (targetElement.ActualWidth / 2.0 - pos.X) / num1;
            double num3 = 2.0 * (targetElement.ActualHeight / 2.0 - pos.Y) / num1;

            projection.RotationY = num2 * MetroInMotion.TiltAngleFactor * tiltFactor;
            projection.RotationX = -num3 * MetroInMotion.TiltAngleFactor * tiltFactor;

            double num4 = tiltFactor * (1.0 - Math.Sqrt(num2 * num2 + num3 * num3)) / MetroInMotion.ScaleFactor;
            scale.ScaleX = scale.ScaleY = 1.0 - num4;
            FrameworkElement frameworkElement = Window.Current.Content as FrameworkElement;
            double num5 = (targetElement.TransformToVisual(frameworkElement).TransformPoint(new Point()).Y - frameworkElement.ActualHeight / 2.0) / 2.0;
            translate.Y = -num5;
            projection.LocalOffsetY = num5;
        }
        
        private static void ShowUpAnimation(UIElement element)
        {
            FrameworkElement targetElement = element as FrameworkElement;

            var projection = targetElement.Projection as PlaneProjection;
            var transformGroup = targetElement.RenderTransform as TransformGroup;
            if (transformGroup == null)//в эмуляторе не было поинтер овер
                return;
            ScaleTransform scale = transformGroup.Children[0] as ScaleTransform;
            TranslateTransform translate = transformGroup.Children[1] as TranslateTransform;

            int duration = 1;
            
            new Storyboard()
            {
                Children = {
                    MetroInMotion.CreateAnimation(projection.RotationY, 0.0, duration, "RotationY", projection),
                    MetroInMotion.CreateAnimation(projection.RotationX, 0.0, duration, "RotationX", projection),
                    MetroInMotion.CreateAnimation(scale.ScaleX, 1.0, duration, "ScaleX", scale),
                    MetroInMotion.CreateAnimation(scale.ScaleY, 1.0, duration, "ScaleY", scale)
                  }
            }.Begin();
            


            /*
            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = scale.ScaleX,
                to = 1.0,
                propertyPath = "ScaleX",
                duration = duration,
                target = scale,
                //easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = scale.ScaleY,
                to = 1.0,
                propertyPath = "ScaleY",
                duration = duration,
                target = scale,
                //easing = this.ANIMATION_EASING
            });


            
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = projection.RotationX,
                to = 0.0,
                propertyPath = "RotationY",
                duration = duration,
                target = projection,
                //easing = this.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                from = projection.RotationY,
                to = 0.0,
                propertyPath = "RotationY",
                duration = duration,
                target = projection,
                //easing = this.ANIMATION_EASING
            });
            

            AnimationUtils.AnimateSeveral(animInfoList);
            */












            translate.Y = 0.0;
            projection.LocalOffsetY = 0.0;
        }

        private static DoubleAnimation CreateAnimation(double? from, double? to, double duration, string targetProperty, DependencyObject target)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = to;
            doubleAnimation.From = from;
            doubleAnimation.EasingFunction = new SineEase();
            doubleAnimation.Duration = TimeSpan.FromSeconds(duration);
            Storyboard.SetTarget(doubleAnimation, target);
            Storyboard.SetTargetProperty(doubleAnimation, targetProperty);
            return doubleAnimation;
        }

    }
}
