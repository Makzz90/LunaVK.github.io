#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace LunaVK.Framework.JuniperPhotonAcrylicBrush
{
    public abstract class AcrylicBrushBase : XamlCompositionBrushBase
    {
        protected Compositor _compositor;
        protected CompositionEffectBrush _brush;

#region TintColor
        /// <summary>
        /// слой наложения цвета/оттенка. Можно задать RGB-значение цвета и прозрачность альфа-канала.
        /// </summary>
        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        public static readonly DependencyProperty TintColorProperty = DependencyProperty.Register("TintColor", typeof(Color), typeof(AcrylicBrushBase), new PropertyMetadata(null));
#endregion

        #region BackdropFactor
        /// <summary>
        /// резервный сплошной цвет, заменяющий акрил в режиме экономии заряда.
        /// Акрил фона также сменяется резервным цветом, если окно приложения на рабочем столе неактивно или приложение запущено на телефоне или Xbox.
        /// </summary>
        public float BackdropFactor
        {
            get { return (float)GetValue(BackdropFactorProperty); }
            set { SetValue(BackdropFactorProperty, value); }
        }

        public static readonly DependencyProperty BackdropFactorProperty = DependencyProperty.Register("BackdropFactor", typeof(float), typeof(AcrylicBrushBase), new PropertyMetadata(0.5f, OnBackdropFactorChanged));
#endregion

#region TintColorFactor
        /// <summary>
        /// слой наложения цвета/оттенка. Можно задать RGB-значение цвета и прозрачность альфа-канала.
        /// 0 - 1
        /// </summary>
        public float TintColorFactor
        {
            get { return (float)GetValue(TintColorFactorProperty); }
            set { SetValue(TintColorFactorProperty, value); }
        }

        public static readonly DependencyProperty TintColorFactorProperty = DependencyProperty.Register("TintColorFactor", typeof(float), typeof(AcrylicBrushBase), new PropertyMetadata(1.0f, OnTintColorFactorChanged));
#endregion

        public float BlurAmount
        {
            get { return (float)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }

        public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register("BlurAmount", typeof(float), typeof(AcrylicBrushBase), new PropertyMetadata(2f, OnBlurAmountChanged));

        //изначально этой функции не было
        private static void OnBackdropFactorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (AcrylicBrushBase)obj;

            if (c._compositor == null)
                return;

            c._brush = new CompositionBrushBuilder(c.GetBrushType()).SetTintColor(c.TintColor)
                .SetBackdropFactor((float)e.NewValue)
                .SetTintColorFactor(c.TintColorFactor)
                .SetBlurAmount(c.BlurAmount)
                .Build(c._compositor);
            c.CompositionBrush = c._brush;
        }
        private static void OnTintColorFactorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (AcrylicBrushBase)obj;

            if (c._compositor == null)
                return;
            
            c._brush = new CompositionBrushBuilder(c.GetBrushType()).SetTintColor(c.TintColor)
                .SetBackdropFactor(c.BackdropFactor)
                .SetTintColorFactor((float)e.NewValue)
                .SetBlurAmount(c.BlurAmount)
                .Build(c._compositor);
            c.CompositionBrush = c._brush;
        }
        private static void OnBlurAmountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var c = (AcrylicBrushBase)obj;

            if (c._compositor == null)
                return;

            c._brush = new CompositionBrushBuilder(c.GetBrushType()).SetTintColor(c.TintColor)
                .SetBackdropFactor(c.BackdropFactor)
                .SetTintColorFactor(c.TintColorFactor)
                .SetBlurAmount((float)e.NewValue)
                .Build(c._compositor);
            c.CompositionBrush = c._brush;
        }


        public AcrylicBrushBase()
        {
        }

        protected abstract BackdropBrushType GetBrushType();

        protected override void OnConnected()
        {
            base.OnConnected();
            _compositor = ElementCompositionPreview.GetElementVisual(Window.Current.Content).Compositor;
            _brush = new CompositionBrushBuilder(GetBrushType()).SetTintColor(TintColor)
                .SetBackdropFactor(BackdropFactor)
                .SetTintColorFactor(TintColorFactor)
                .SetBlurAmount(BlurAmount)
                .Build(_compositor);
            base.CompositionBrush = _brush;

            Windows.System.Power.PowerManager.EnergySaverStatusChanged += PowerManager_EnergySaverStatusChanged;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            _brush = new CompositionBrushBuilder(GetBrushType()).SetTintColor(TintColor)
                .SetBackdropFactor(e.Visible ? BackdropFactor : 0)
                .SetTintColorFactor(e.Visible ? TintColorFactor : 1)
                .SetBlurAmount(BlurAmount)
                .Build(_compositor);
            base.CompositionBrush = _brush;
        }

        private void PowerManager_EnergySaverStatusChanged(object sender, object e)
        {
            bool IsSave = Windows.System.Power.PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On;

            Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                _brush = new CompositionBrushBuilder(GetBrushType()).SetTintColor(TintColor)
                .SetBackdropFactor(IsSave == false ? BackdropFactor : 0)
                .SetTintColorFactor(IsSave == false ? TintColorFactor : 1)
                .SetBlurAmount(BlurAmount)
                .Build(_compositor);
                base.CompositionBrush = _brush;
            });
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            _brush?.Dispose();
            Windows.System.Power.PowerManager.EnergySaverStatusChanged -= PowerManager_EnergySaverStatusChanged;
            Window.Current.VisibilityChanged -= Current_VisibilityChanged;
        }
    }
}
#endif