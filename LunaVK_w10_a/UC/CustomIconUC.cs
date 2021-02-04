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
    public class CustomIconUC : Grid
    {
        /*
        public IconUC Icon_UC
        {
            get { return this.i1; }
        }
        */
        private IconUC i1;
        private bool InAnimation;
        private readonly int Duration = 150;

#region GlyphProperty
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(CustomIconUC), new PropertyMetadata("", GlyphChanged));
        public string Glyph
        {
            get { return (string)base.GetValue(GlyphProperty); }
            set { base.SetValue(GlyphProperty, value); }
        }

        private static void GlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomIconUC lv = (CustomIconUC)d;
            lv.Do((string)e.NewValue);
        }
#endregion

#region FontSizeProperty
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(CustomIconUC), new PropertyMetadata(18.0, FontSizeChanged));
        public double FontSize
        {
            get { return (double)base.GetValue(FontSizeProperty); }
            set { base.SetValue(FontSizeProperty, value); }
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomIconUC ci = (CustomIconUC)d;
            ci.i1.FontSize = (double)e.NewValue;

            //ci.plane.CenterOfRotationX = ci.ActualHeight / 2.0;
        }
#endregion

#region ForegroundProperty
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(CustomIconUC), new PropertyMetadata(null, ForegroundChanged));
        public Brush Foreground
        {
            get { return (Brush)base.GetValue(ForegroundProperty); }
            set { base.SetValue(ForegroundProperty, value); }
        }

        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomIconUC lv = (CustomIconUC)d;
            Brush b = (Brush)e.NewValue;
            lv.i1.Foreground = b;
        }
#endregion

        private PlaneProjection plane
        {
            get
            {
                if (base.Projection==null)
                    base.Projection = new PlaneProjection();
                return base.Projection as PlaneProjection;
            }
        }

        public CustomIconUC()
        {
            this.i1 = new IconUC();
            base.Children.Add(this.i1);
        }
        
        private void Do(string text)
        {
            if (this.InAnimation || string.IsNullOrEmpty(this.i1.Glyph))
            {
                this.i1.Glyph = text;
                return;
            }

            this.InAnimation = true;
            this.plane.Animate(0, 90, "RotationY", this.Duration, 0, null, this.OnAnimationDone);

        }

        private void OnAnimationDone()
        {
            this.InAnimation = false;
            this.i1.Glyph = this.Glyph;
            this.plane.Animate(90, 0, "RotationY", this.Duration);
        }
    }
}
