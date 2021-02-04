using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace LunaVK.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SplashPage : Page
    {
        //private readonly Frame _rootFrame;
        private readonly SplashScreen _splash;
        private Rect _splashImageRect;

        public SplashPage(SplashScreen splashscreen)
        {
            InitializeComponent();

            _splash = splashscreen;

            Window.Current.SizeChanged += ExtendedSplash_OnResize;

            if (_splash != null)
            {
                _splash.Dismissed += DismissedEventHandler;
                _splashImageRect = _splash.ImageLocation;

                PositionImage();
            }

            base.Loaded += SplashPage_Loaded;
        }

        private void SplashPage_Loaded(object sender, RoutedEventArgs e)
        {
            Framework.CustomFrame.Instance.SuppressMenu = true;
            Framework.CustomFrame.Instance.Header.Visibility = Visibility.Collapsed;
        }

        private void PositionImage()
        {
            Canvas.SetLeft(ExtendedSplashImage, _splashImageRect.X);
            Canvas.SetTop(ExtendedSplashImage, _splashImageRect.Y);

            ExtendedSplashImage.Height = _splashImageRect.Height;
            ExtendedSplashImage.Width = _splashImageRect.Width;
        }
        
        private void ExtendedSplash_OnResize(Object sender, WindowSizeChangedEventArgs e)
        {
            if (_splash != null)
            {
                _splashImageRect = _splash.ImageLocation;
                PositionImage();
            }
        }

        private async void DismissedEventHandler(SplashScreen sender, object e)
        {
           // await SetupApp();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DismissExtendedSplash);
        }

        private async Task SetupApp()
        {
            await Task.Delay(2000);
        }

        private void DismissExtendedSplash()
        {
            //if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            //{
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("image", ExtendedSplashImage);
            //}
            Framework.CustomFrame.Instance.Navigate(typeof(LoginPage));
        }
    }
}
