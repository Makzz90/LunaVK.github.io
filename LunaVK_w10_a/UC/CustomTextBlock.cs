using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Xaml;
using LunaVK.Core.Utils;

namespace LunaVK.UC
{
    public class CustomTextBlock : Grid
    {
        TextBlock tb1, tb2;
        TranslateTransform tt1, tt2;
        private bool InAnimation;
        private readonly int Duration = 200;

#region TextProperty
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CustomTextBlock), new PropertyMetadata(null, TextChanged));
        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTextBlock lv = (CustomTextBlock)d;
            lv.Do((string)e.NewValue);
        }
#endregion

#region FontSizeProperty
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(CustomTextBlock), new PropertyMetadata(18.0, FontSizeChanged));
        public double FontSize
        {
            get { return (double)base.GetValue(FontSizeProperty); }
            set { base.SetValue(FontSizeProperty, value); }
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTextBlock ctb = (CustomTextBlock)d;
            ctb.tb1.FontSize = ctb.tb2.FontSize = Math.Max(10, (double)e.NewValue);//(double)e.NewValue;//todo:clear?
        }
#endregion


        public static readonly DependencyProperty UseWhiteForegroundProperty = DependencyProperty.Register("UseWhiteForeground", typeof(bool), typeof(CustomTextBlock), new PropertyMetadata(false, UseWhiteForegroundChanged));
        public bool UseWhiteForeground
        {
            get { return (bool)base.GetValue(UseWhiteForegroundProperty); }
            set { base.SetValue(UseWhiteForegroundProperty, value); }
        }
        private static void UseWhiteForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTextBlock lv = (CustomTextBlock)d;
            if ((bool)e.NewValue == true)
            {
                lv.tb1.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                lv.tb2.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                lv.tb1.Style = null;
                lv.tb2.Style = null;
            }
            else
            {
                lv.tb1.Foreground = null;
                lv.tb2.Foreground = null;
                lv.tb1.Style = (Style)Application.Current.Resources["TextBlockThemeHigh"];
                lv.tb2.Style = (Style)Application.Current.Resources["TextBlockThemeHigh"];
            }
        }

        public CustomTextBlock()
        {
            base.SizeChanged += CustomTextBlock_SizeChanged;
            base.VerticalAlignment = VerticalAlignment.Center;

            this.tb1 = new TextBlock() { FontSize = this.FontSize, Visibility = Windows.UI.Xaml.Visibility.Collapsed };
            this.tt1 = new TranslateTransform();
            this.tb1.RenderTransform = this.tt1;

            this.tb2 = new TextBlock() { FontSize = this.FontSize };
            this.tt2 = new TranslateTransform();
            this.tb2.RenderTransform = this.tt2;

            base.Children.Add(this.tb1);
            base.Children.Add(this.tb2);
        }

        void CustomTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = sender as Grid;
            grid.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        private void Do(string text)
        {
            if (text == null)
                text = String.Empty;

            if (this.InAnimation)
            {
                this.tb1.Text = text;
                return;
            }

            this.InAnimation = true;
            this.tb1.Text = text;
            this.tt1.Y = -30;
            this.tb1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.tt1.Animate(this.tt1.Y, 0, "Y", this.Duration, 0, null, this.OnAnimationDone);

            this.tt2.Animate(0, 30, "Y", Duration);
        }

        private void OnAnimationDone()
        {
            this.InAnimation = false;
            this.tb2.Text = this.tb1.Text;
            this.tt2.Y = 0;
            this.tb1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}