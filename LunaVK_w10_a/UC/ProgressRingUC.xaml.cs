using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace LunaVK.UC
{
    public sealed partial class ProgressRingUC : UserControl
    {
#region Progress
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ProgressRingUC), new PropertyMetadata(0.0, new PropertyChangedCallback(ProgressRingUC.Progress_OnChanged)));
        public double Progress
        {
            get { return (double)base.GetValue(ProgressRingUC.ProgressProperty); }
            set { base.SetValue(ProgressRingUC.ProgressProperty, value); }
        }

        private static void Progress_OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;
            ProgressRingUC progressRingUc = (ProgressRingUC)obj;

            if (value < 0.0)
                value = 0.0;
            else if (value > 100.0)
                value = 100.0;

            double num2 = value / 100.0;
            progressRingUc.progressRing.IsActive = value > 0.1 && value != 100;
            progressRingUc.canvas.Visibility = value > 0.5 ? Visibility.Visible : Visibility.Collapsed;
            progressRingUc.arcProgress.Point = new Point(progressRingUc.arcProgress.Size.Width * Math.Sin(2 * Math.PI * num2), progressRingUc.arcProgress.Size.Height * (1 - Math.Cos(2 * Math.PI * num2)));
            progressRingUc.arcProgress.IsLargeArc = value >= 50;
        }
#endregion

#region Size
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(nameof(Size), typeof(double), typeof(ProgressRingUC), new PropertyMetadata(56.0, new PropertyChangedCallback(ProgressRingUC.Size_OnChanged)));
        public double Size
        {
            get { return (double)base.GetValue(ProgressRingUC.SizeProperty); }
            set { base.SetValue(ProgressRingUC.SizeProperty, value); }
        }

        private static void Size_OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;
            ProgressRingUC progressRingUc = (ProgressRingUC)obj;

            if (value < 30.0)
                value = 30.0;
            else if (value > 100.0)
                value = 100.0;

            progressRingUc.Width = progressRingUc.Height = value;
            progressRingUc.ellipseBack.Width = progressRingUc.ellipseBack.Height = value;
            progressRingUc.progressRing.Width = progressRingUc.progressRing.Height = value;
            progressRingUc.canvas.Margin = new Thickness(value / 2.0, progressRingUc.path.StrokeThickness / 2.0, value / 2.0, progressRingUc.path.StrokeThickness / 2.0);
            progressRingUc.arcProgress.Size = new Size((value - progressRingUc.path.StrokeThickness) / 2.0, (value - progressRingUc.path.StrokeThickness) / 2.0);

            //double num2 = value / 100.0;
            //progressRingUc.arcProgress.Point = new Point(progressRingUc.arcProgress.Size.Width * Math.Sin(2 * Math.PI * num2), progressRingUc.arcProgress.Size.Height * (1 - Math.Cos(2 * Math.PI * num2)));
        }
#endregion

#region ForegroundProperty
        public static readonly DependencyProperty UCForegroundProperty = DependencyProperty.Register("UCForeground", typeof(Brush), typeof(ProgressRingUC), new PropertyMetadata(null, ForegroundChanged));
        public Brush UCForeground
        {
            get { return (Brush)base.GetValue(UCForegroundProperty); }
            set { base.SetValue(UCForegroundProperty, value); }
        }

        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressRingUC lv = (ProgressRingUC)d;
            Brush b = (Brush)e.NewValue;
            lv.progressRing.Foreground = b;
            lv.path.Stroke = b;
        }
#endregion

#region BackgroundProperty
        public static readonly DependencyProperty UCBackgroundProperty = DependencyProperty.Register("UCBackground", typeof(SolidColorBrush), typeof(ProgressRingUC), new PropertyMetadata(null, BackgroundChanged));
        public SolidColorBrush UCBackground
        {
            get { return (SolidColorBrush)base.GetValue(UCBackgroundProperty); }
            set { base.SetValue(UCBackgroundProperty, value); }
        }

        private static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressRingUC lv = (ProgressRingUC)d;
            SolidColorBrush b = (SolidColorBrush)e.NewValue;
            lv.ellipseFill.Color = b.Color;
        }
#endregion

        public ProgressRingUC()
        {
            this.InitializeComponent();
        }
    }
}
