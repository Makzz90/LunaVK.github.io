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
using LunaVK.Core;

namespace LunaVK.Framework
{
    public static class ImageExtensions
    {
        // Using a DependencyProperty as the backing store for WebUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CacheUriProperty = DependencyProperty.RegisterAttached("CacheUri", typeof(Uri), typeof(ImageExtensions), new PropertyMetadata(null, OnCacheUriChanged));
        public static readonly DependencyProperty CacheUriImageBrush = DependencyProperty.RegisterAttached("CacheUriImageBrush", typeof(Uri), typeof(ImageExtensions), new PropertyMetadata(null, OnImageBrushMultiResSourceChanged));

        /// <summary>
        /// Gets the CacheUri property. This dependency property 
        /// WebUri that has to be cached
        /// </summary>
        public static Uri GetCacheUri(DependencyObject d)
        {
            return (Uri)d.GetValue(CacheUriProperty);
        }

        /// <summary>
        /// Sets the CacheUri property. This dependency property 
        /// WebUri that has to be cached
        /// </summary>
        public static void SetCacheUri(DependencyObject d, Uri value)
        {
            d.SetValue(CacheUriProperty, value);
        }

        public static Uri GetCacheUriImageBrush(ImageBrush obj)
        {
            return (Uri)obj.GetValue(ImageExtensions.CacheUriImageBrush);
        }

        public static void SetCacheUriImageBrush(ImageBrush obj, Uri value)
        {
            obj.SetValue(ImageExtensions.CacheUriImageBrush, value);
        }

        private static void OnCacheUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Uri newCacheUri = (Uri)d.GetValue(CacheUriProperty);
            Image image = (Image)d;

            if (newCacheUri != null)
            {
                try
                {
                    //Get image from cache (download and set in cache if needed)
                    //string cacheUri = await ImageExtensions.ImageFromCache2(newCacheUri.AbsoluteUri);

                    //Set cache uri as source for the image
                    //image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(cacheUri));

                    ImageExtensions.ImageFromCache3(newCacheUri.AbsoluteUri, (cacheUri) => //Get image from cache (download and set in cache if needed)
                    {
                        image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(cacheUri)); //Set cache uri as source for the image
                    });
                }
                catch (Exception)
                {
                }
            }
            else
                image.Source = null;

        }

        private static void OnImageBrushMultiResSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageBrush imageBrush = (ImageBrush)d;
            string str = e.NewValue.ToString();
            
            string newCacheUri = CacheManager.NewGuid(str);
            if (string.IsNullOrEmpty( newCacheUri))
            {
                try
                {
                    ImageExtensions.ImageFromCache3(newCacheUri, (cacheUri) => //Get image from cache (download and set in cache if needed)
                    {
                        imageBrush.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(cacheUri)); //Set cache uri as source for the image
                    });
                }
                catch (Exception)
                {
                }
            }
            else
                imageBrush.ImageSource = null;
        }

        public static async void ImageFromCache3(string path, Action<string> callback)
        {
            string cacheUri = await ImageExtensions.ImageFromCache2(path);
            //todo: cacheUri == null? how?
            if (callback != null && cacheUri!=null)
            {
                callback(cacheUri);
            }
        }

        public static async Task<string> ImageFromCache2(string path)
        {
            int ru = path.IndexOf(".com") + 5;// TODO: .com .net .org
            string new_path = path.Substring(ru).Replace("/", "\\").Replace('?','_');

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                
                Stream p = await localFolder.OpenStreamForReadAsync(new_path);
                p.Dispose();
                //System.Diagnostics.Debug.WriteLine("From cache " + path);
                return localFolder.Path + "\\" + new_path;
            }
            catch (FileNotFoundException)
            {

            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("{0}", e.Message);
            }

            try
            {
                StorageFile storageFile = await localFolder.CreateFileAsync(new_path, CreationCollisionOption.OpenIfExists);

                Uri Website = new Uri(path);
                HttpClient http = new HttpClient();
                // TODO: Check connection. Return message on fail.
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Downloading started for " + path);
#endif
                byte[] image_from_web_as_bytes = await http.GetByteArrayAsync(Website);

                MakeFolders(localFolder, path.Substring(ru));

                Stream outputStream = await storageFile.OpenStreamForWriteAsync();
                outputStream.Write(image_from_web_as_bytes, 0, image_from_web_as_bytes.Length);
                outputStream.Position = 0;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Write file done {0}", outputStream.Length);
#endif
                outputStream.Dispose();
                return localFolder.Path + "\\" + new_path;
            }
            catch
            {

            }
            return null;
        }

        private static async void MakeFolders(StorageFolder localFolder, string path)
        {
            //pics/thumbnail/050/197/50197442.jpg
            int slash = path.IndexOf("/");
            if (slash <= 0) // -1 Not found
                return;

            string new_path = path.Substring(0, slash);
            StorageFolder opened_folder = await localFolder.CreateFolderAsync(new_path, CreationCollisionOption.OpenIfExists);
            string very_new_path = path.Remove(0, new_path.Length + 1);
            MakeFolders(opened_folder, very_new_path);
        }
    }
}
