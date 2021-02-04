using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Markup;
using Windows.UI;

namespace LunaVK.Framework
{
    public static class PointOver
    {
        private static Storyboard storyEntered;
        private static Storyboard storyExited;

        public static readonly DependencyProperty PointColorProperty = DependencyProperty.RegisterAttached("PointColor", typeof(SolidColorBrush), typeof(PointOver), new PropertyMetadata(null, new PropertyChangedCallback(PointOver.OnTPointColorChanged)));

        public static SolidColorBrush GetPointColor(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(PointOver.PointColorProperty);
        }

        public static void SetPointColor(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(PointOver.PointColorProperty, value);
        }

        private static void OnTPointColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            UIElement element = (UIElement)d;
            if (Application.Current.Resources.ContainsKey("SystemControlTransparentRevealBorderBrush"))
            {
                if (element is Grid)
                {
                    Grid grid = element as Grid;

                    object _blinkBrush = Application.Current.Resources["SystemControlTransparentRevealBorderBrush"];
                    if (_blinkBrush != null)
                    {
                        grid.BorderThickness = new Thickness(1);
                        grid.BorderBrush = _blinkBrush as Brush;
                    }
                }
            }
            else
            {
                Color brushEntered = ((SolidColorBrush)args.NewValue).Color;
                Panel panel = element as Panel;
                Color brushNormal;
                if (panel.Background == null)
                    panel.Background = new SolidColorBrush(Colors.Transparent);

                brushNormal = (panel.Background as SolidColorBrush).Color;
                string hexEntered = brushEntered.A.ToString("X2") + brushEntered.R.ToString("X2") + brushEntered.G.ToString("X2") + brushEntered.B.ToString("X2");
                string hexNormal = brushNormal.A.ToString("X2");

                if (brushNormal.A == 0 && brushNormal.R == byte.MaxValue && brushNormal.G == byte.MaxValue && brushNormal.B == byte.MaxValue) // Transp
                    hexNormal += (brushEntered.R.ToString("X2") + brushEntered.G.ToString("X2") + brushEntered.B.ToString("X2"));
                else
                    hexNormal += (brushNormal.R.ToString("X2") + brushNormal.G.ToString("X2") + brushNormal.B.ToString("X2"));

                element.PointerEntered += OnPointerEntered;
                element.PointerExited += OnPointerExited;
                element.PointerCaptureLost += OnPointerExited;

                storyEntered = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><ColorAnimation Duration=\"0:0:0.2\" To=\"#" + hexEntered + "\" Storyboard.TargetProperty=\"(Panel.Background).(SolidColorBrush.Color)\"/> </Storyboard>") as Storyboard;
                storyExited = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><ColorAnimation Duration=\"0:0:0.2\" From=\"#" + hexEntered + "\" To=\"#" + hexNormal + "\" Storyboard.TargetProperty=\"(Panel.Background).(SolidColorBrush.Color)\"/> </Storyboard>") as Storyboard;
            }

        }

        static void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            UIElement element = sender as UIElement;

            storyEntered.Stop();
            storyExited.Stop();

            Storyboard.SetTarget(storyEntered.Children[0], element);
            Storyboard.SetTarget(storyExited.Children[0], element);

            storyExited.Begin();
        }

        static void OnPointerEntered(object sender, PointerRoutedEventArgs args)
        {
            UIElement element = sender as UIElement;

            storyEntered.Stop();
            storyExited.Stop();

            Storyboard.SetTarget(storyEntered.Children[0], element);
            Storyboard.SetTarget(storyExited.Children[0], element);


            storyEntered.Begin();
        }
    }
}
