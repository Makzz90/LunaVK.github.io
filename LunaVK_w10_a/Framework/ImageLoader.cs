using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using System.Net.Http;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Framework
{
    public class ImageLoader
    {
#region UriSource
        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(null,ImageLoader.OnUriSourceChanged));

        public static string GetUriSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(ImageLoader.UriSourceProperty);
        }

        public static void SetUriSource(Image obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageLoader.UriSourceProperty, value);
        }

        private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Image arg_14_0 = (Image)o;
            string uriStr = (string)e.NewValue;
            ImageLoader.HandleUriChangeLowProfile(arg_14_0, uriStr);
        }

        private static void HandleUriChangeLowProfile(Image image, string uriStr)
        {
            Uri uri = uriStr.ConvertToUri();
            //VeryLowProfileImageLoader.SetUriSource(image, uri);

            ProfileImageLoader.SetUriSource(image, uriStr);
            //
            //
            //image.Source = new BitmapImage(new Uri(uriStr));
        }
#endregion

#region StreamSource
        public static readonly DependencyProperty StreamSourceProperty = DependencyProperty.RegisterAttached("StreamSource", typeof(Stream), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnStreamSourceChanged)));

        public static string GetStreamSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(ImageLoader.StreamSourceProperty);
        }

        public static void SetStreamSource(Image obj, Stream value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageLoader.StreamSourceProperty, value);
        }

        private static void OnStreamSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image image = (Image)d;
            Stream stream = e.NewValue as Stream;
            if (stream == null)
            {
                image.Source = null;
                return;
            }

            //           expr_24.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(stream.AsRandomAccessStream());//bitmapImage.SetSource(stream);
            image.Source = bitmapImage;
        }
#endregion

#region ImageBrushSource
        public static readonly DependencyProperty ImageBrushSourceProperty = DependencyProperty.RegisterAttached("ImageBrushSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnImageBrushSourceChanged)));

        public static string GetImageBrushSource(ImageBrush obj)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            return (string)obj.GetValue(ImageLoader.ImageBrushSourceProperty);
        }

        public static void SetImageBrushSource(ImageBrush obj, string value)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            obj.SetValue(ImageLoader.ImageBrushSourceProperty, value);
        }

        private static void OnImageBrushSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageLoader.ProcessImageBrush((ImageBrush)d, e.NewValue);
        }
        #endregion

#region ImageBrushMultiResSource
        public static readonly DependencyProperty ImageBrushMultiResSourceProperty = DependencyProperty.RegisterAttached("ImageBrushMultiResSource", typeof(string), typeof(ImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageLoader.OnImageBrushMultiResSourceChanged)));

        public static string GetImageBrushMultiResSource(ImageBrush obj)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            return (string)obj.GetValue(ImageLoader.ImageBrushMultiResSourceProperty);
        }

        public static void SetImageBrushMultiResSource(ImageBrush obj, string value)
        {
            if (obj == null)
                throw new ArgumentException("obj");
            obj.SetValue(ImageLoader.ImageBrushMultiResSourceProperty, value);
        }

        private static void OnImageBrushMultiResSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageBrush imageBrush = (ImageBrush)d;
            if (e.NewValue == null)
            {
                ImageLoader.ProcessImageBrush(imageBrush, null);
                return;
            }
            string text = e.NewValue.ToString();
            //if (DesignerProperties.GetIsInDesignMode(imageBrush))
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                //                text = MultiResolutionHelper.Instance.AppendResolutionSuffix(text, true, "-WVGA");
            }
            else
            {
                //                text = MultiResolutionHelper.Instance.AppendResolutionSuffix(text, true, "");
            }
            ImageLoader.ProcessImageBrush(imageBrush, text);
        }
#endregion













        private static void ProcessImageBrush(ImageBrush ib, object newSource)
        {
            if (newSource == null)
            {
                ib.ImageSource = (null);
            }
            else
            {
                Uri uri = new Uri((string)newSource, UriKind.RelativeOrAbsolute);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = ((BitmapCreateOptions)18);
                bitmapImage.UriSource = uri;
                ib.ImageSource = ((ImageSource)bitmapImage);
            }
        }

        


        


        public static void SetSourceForImage(Image image, string uriStr, bool animateOpacity = false)
        {
            ImageLoader.SetUriSource(image, uriStr);
        }

        
    }
}
