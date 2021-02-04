using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestAcrylic : Page
    {
        LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush lightHeader2 = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush() { TintColor = Windows.UI.Colors.Red, /*TintOpacity = 0.9f,*/ FallbackColor = Windows.UI.Colors.Blue };

        public TestAcrylic()
        {
            this.InitializeComponent();
            this.Loaded += TestAcrylic_Loaded;
        }

        private void TestAcrylic_Loaded(object sender, RoutedEventArgs e)
        {
            this.apl(_brd);
        }

        private void apl(FrameworkElement e)
        {
            lightHeader2.BlurAmount = 10;
            lightHeader2.BackdropFactor = 0.1f;

            _brd.Background = lightHeader2;
        }

        private void applyAcrylicAccent(FrameworkElement e)
        {
            var temp = ElementCompositionPreview.GetElementChildVisual(e);

            var compositor = ElementCompositionPreview.GetElementVisual(e).Compositor;
            var hostVisual = compositor.CreateSpriteVisual();
            hostVisual.Size = new System.Numerics.Vector2((float)e.ActualWidth, (float)e.ActualHeight);

            // You can choose which effect you want, it is indifferent 
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 10.0f,
                BorderMode = EffectBorderMode.Soft,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("Source"),
            };

            var factory = compositor.CreateEffectFactory(blurEffect, null);
            var effectBrush = factory.CreateBrush();

            effectBrush.SetSourceParameter("Source", compositor.CreateBackdropBrush());

            hostVisual.Brush = effectBrush;
            ElementCompositionPreview.SetElementChildVisual(e, hostVisual);
            ElementCompositionPreview.SetElementChildVisual(e, temp);
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider sl = sender as Slider;
            lightHeader2.BlurAmount = (float)sl.Value;
        }

        private void Slider_ValueChanged_1(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider sl = sender as Slider;
            lightHeader2.BackdropFactor = (float)sl.Value;
        }

        private void Slider_ValueChanged_2(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider sl = sender as Slider;
            lightHeader2.TintColorFactor = (float)sl.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lightHeader2.TintColorFactor = 1;
            lightHeader2.BackdropFactor = 0;
        }
        /*
* private void applyAcrylicAccent(Panel panel)
{
_compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
_hostSprite = _compositor.CreateSpriteVisual();
_hostSprite.Size = new Vector2((float) panel.ActualWidth, (float) panel.ActualHeight);

ElementCompositionPreview.SetElementChildVisual(panel, _hostSprite);
_hostSprite.Brush = _compositor.CreateHostBackdropBrush();
}
Compositor _compositor;
SpriteVisual _hostSprite;
*/
    }
}
