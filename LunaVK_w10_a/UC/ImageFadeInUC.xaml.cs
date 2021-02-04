using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class ImageFadeInUC : UserControl
    {
        public ImageFadeInUC()
        {
            this.InitializeComponent();
        }

#region Source
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageFadeInUC), new PropertyMetadata(default(ImageSource), SourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
#endregion

        public Border Brd
        {
            get { return this._brd; }
        }

        private static void SourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ImageFadeInUC control = (ImageFadeInUC)dependencyObject;
            ImageSource newSource = (ImageSource)dependencyPropertyChangedEventArgs.NewValue;
            
            if (newSource != null)
            {
                var image = (BitmapImage)newSource;
                /*
                // If the image is not a local resource or it was not cached
                if (image.UriSource.Scheme != "ms-appx" && image.UriSource.Scheme != "ms-resource" && (image.PixelHeight * image.PixelWidth == 0))
                {
                    // TODO:
                    control.DownloadImageAsync(newSource);
                }
                else
                {*/
                    control.LoadImage(newSource);
                //}
            }
        }

        private void LoadImage(ImageSource source)
        {
//            this.ImageFadeOut.Begin();
            this._img.Source = source;

            if (Windows.System.Power.PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On)
                this._img_ImageFailed(this._img, null);
        }

        private void _img_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (Windows.System.Power.PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On)
            {
                this._img_ImageFailed(this._img, null);
                return;
            }

           this.ImageFadeIn.Begin();
        }

        private void _img_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            (sender as UIElement).Opacity = 1;
        }

        private void ImageFadeOut_Completed(object sender, object e)
        {
            this.ImageFadeIn.Begin();
        }
    }
}
