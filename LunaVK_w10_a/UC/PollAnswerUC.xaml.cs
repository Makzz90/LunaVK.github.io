using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class PollAnswerUC : UserControl
    {
        private bool isSelected;
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(PollAnswerUC), new PropertyMetadata(0.0,Value_OnChanged));
        public double Value
        {
            get { return (double)base.GetValue(ValueProperty); }
            set { base.SetValue(ValueProperty, value); }
        }

        public PollAnswerUC()
        {
            this.InitializeComponent();
            this.Loaded += PollAnswerUC_Loaded;
            base.SizeChanged += (sender, args) =>
            {
                this.clipRectangleFill.Rect = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight);
                this.UpdateValue(false);
            };
        }

        private void PollAnswerUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.isSelected = false;
            this.UpdateIcon();
        }

        private static void Value_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PollAnswerUC)d).UpdateValue(true);
        }

        private void UpdateValue(bool animated = true)
        {
            double num1 = base.ActualWidth * this.Value / 100.0;
            double num2 = -base.ActualWidth + num1;
            if (num1 == 0.0 || !animated)
            {
                this.transformRectangleFill.X = num2;
            }
            else
            {
                this.transformRectangleFill.X = (-base.ActualWidth);
                this.AnimationUpdateValue.To = num2;
                this.StoryboardAnimationUpdateValue.Begin();
            }
        }

        private void UpdateIcon()
        {
            this.MultiSelectCheck.Glyph = this.isSelected ? "\xE73D" : "\xE739";
        }

        private void MultiSelectSquare_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.isSelected = !this.isSelected;
            this.UpdateIcon();
        }
    }
}
