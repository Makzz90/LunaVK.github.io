using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;

namespace LunaVK.Framework
{
    public static class PointOverScale
    {
        public static readonly DependencyProperty PointColorProperty = DependencyProperty.RegisterAttached("OverScale", typeof(double), typeof(PointOverScale), new PropertyMetadata(1.0, new PropertyChangedCallback(OnOverScaleChanged)));

        public static double GetOverScale(DependencyObject obj)
        {
            return (double)obj.GetValue(PointColorProperty);
        }

        public static void SetOverScale(DependencyObject obj, double value)
        {
            obj.SetValue(PointColorProperty, value);
        }

        private static void OnOverScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            UIElement element = (UIElement)d;
            FrameworkElement fr = element as FrameworkElement;
            fr.SizeChanged += fr_SizeChanged;
            //if(element.RenderTransform==null)
            //{
                ScaleTransform sc = new ScaleTransform();
                sc.CenterX = 0.5;
                sc.CenterY = 0.5;
                element.RenderTransform = sc;
            //}

            element.PointerEntered += OnPointerEntered;
            element.PointerExited += OnPointerExited;
            element.PointerCaptureLost += OnPointerExited;
        }

        static void fr_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fr = sender as FrameworkElement;
            ScaleTransform sc = fr.RenderTransform as ScaleTransform;
            sc.CenterX = e.NewSize.Width/2.0;
            sc.CenterY = e.NewSize.Height/2.0;
        }

        static void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            UIElement element = sender as UIElement;
            ScaleTransform sc = element.RenderTransform as ScaleTransform;
            ExponentialEase ea = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 6 };
            sc.Animate(sc.ScaleX, 1.0, "ScaleX", 500, 0, ea);
            sc.Animate(sc.ScaleY, 1.0, "ScaleY", 500, 0, ea);
        }

        static void OnPointerEntered(object sender, PointerRoutedEventArgs args)
        {
            UIElement element = sender as UIElement;
            ScaleTransform sc = element.RenderTransform as ScaleTransform;
            ExponentialEase ea = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 6 };
            double val = (double)element.GetValue(PointOverScale.PointColorProperty);
            sc.Animate(sc.ScaleX, val, "ScaleX", 2000, 0, ea);
            sc.Animate(sc.ScaleY, val, "ScaleY", 2000, 0, ea);
        }
    }
}
