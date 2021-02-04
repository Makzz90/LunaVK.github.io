using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пользовательского элемента управления задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class ProgressArrowUC : UserControl
    {
#region Progress
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(ProgressArrowUC), new PropertyMetadata(0.0, new PropertyChangedCallback(ProgressArrowUC.Progress_OnChanged)));
        public double Progress
        {
            get { return (double)base.GetValue(ProgressArrowUC.ProgressProperty); }
            set { base.SetValue(ProgressArrowUC.ProgressProperty, value); }
        }
        //private double x = 0;
        //private double y = 0;
        //private double w = 200;
        //private double h = 200;

        private static void Progress_OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;
            ProgressArrowUC progressRingUc = (ProgressArrowUC)obj;

            if (value < 0.0)
                value = 0.0;
            else if (value > 100.0)
                value = 100.0;

            progressRingUc.iconBack.Visibility = value == 0 ? Visibility.Collapsed : Visibility.Visible;
            value = 100 - value;
            double num2 = value / 100.0;
            progressRingUc.transformRectangleFill.Y = progressRingUc.ActualHeight * num2;
            //progressRingUc.rectG.Rect = new Rect(0.0, -value, progressRingUc.ActualWidth, progressRingUc.ActualHeight);
        }
#endregion

#region ForegroundProperty
        public static readonly DependencyProperty UCForegroundProperty = DependencyProperty.Register("UCForeground", typeof(Brush), typeof(ProgressArrowUC), new PropertyMetadata(null, ForegroundChanged));
        public Brush UCForeground
        {
            get { return (Brush)base.GetValue(UCForegroundProperty); }
            set { base.SetValue(UCForegroundProperty, value); }
        }

        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressArrowUC lv = (ProgressArrowUC)d;
            Brush b = (Brush)e.NewValue;
            lv.iconFront.Foreground = b;
            lv.iconBack.Foreground = b;
        }
#endregion

#region Size
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(ProgressArrowUC), new PropertyMetadata(56.0, new PropertyChangedCallback(ProgressArrowUC.Size_OnChanged)));
        public double Size
        {
            get { return (double)base.GetValue(ProgressArrowUC.SizeProperty); }
            set { base.SetValue(ProgressArrowUC.SizeProperty, value); }
        }

        private static void Size_OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;
            ProgressArrowUC progressRingUc = (ProgressArrowUC)obj;

            if (value < 10.0)
                value = 10.0;
            else if (value > 100.0)
                value = 100.0;

            progressRingUc.iconFront.FontSize = progressRingUc.iconBack.FontSize = value;

            //double num2 = value / 100.0;
            //progressRingUc.arcProgress.Point = new Point(progressRingUc.arcProgress.Size.Width * Math.Sin(2 * Math.PI * num2), progressRingUc.arcProgress.Size.Height * (1 - Math.Cos(2 * Math.PI * num2)));
        }
#endregion

        public ProgressArrowUC()
        {
            this.InitializeComponent();
            
            base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)
            {
                this.rectG.Rect = new Rect(0.0, 0.0, args.NewSize.Width, args.NewSize.Height);
                //this.transformRectangleFill.CenterX = args.NewSize.Width / 2.0;
                //this.transformRectangleFill.CenterY = args.NewSize.Height / 2.0;
            });
        }
    }
}
