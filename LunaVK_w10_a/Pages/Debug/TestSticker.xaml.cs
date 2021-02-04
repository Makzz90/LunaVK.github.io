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

using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Framework;
using LottieUWP;
using LunaVK.Core;
using LunaVK.Core.Framework;

namespace LunaVK.Pages.Debug
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestSticker : Page
    {
        public TestSticker()
        {
            this.InitializeComponent();

            //            this.LottieAnimationView.DefaultCacheStrategy = LottieAnimationView.CacheStrategy.None;

            double scale = 0;
            double tr_scale = 0;
            double rate = 24.0;
            double m = 5;

            

            
            this.LottieAnimationView.Margin = new Thickness(0, -m, 0, 0);

            this.Loaded += TestSticker_Loaded;
        }

        async void TestSticker_Loaded(object sender, RoutedEventArgs e)
        {
            Stream s = await CacheManager2.GetStreamOfCachedFile("Cache/Animated Stickers/" + 13281 + ".json");

            //this.LottieAnimationView.FrameRate = 24;
            var composition = LottieComposition.Factory.FromInputStreamSync(s);
            this.LottieAnimationView.SetComposition(composition);
            this.LottieAnimationView.PlayAnimation();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.LottieAnimationView.ResumeAnimation();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.LottieAnimationView.PauseAnimation();
        }

        private void Slider_ValueChanged2(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.LottieAnimationView.Margin = new Thickness(0, -e.NewValue, 0, 0);
        }
    }
}
