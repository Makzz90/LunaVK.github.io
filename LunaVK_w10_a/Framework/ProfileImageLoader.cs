using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Storage;
using System.Net.Http;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Library;
using System.Threading;
using Windows.Foundation;
using LunaVK.Core.Framework;
using System.Text.RegularExpressions;

namespace LunaVK.Framework
{
    public static class ProfileImageLoader
    {
        public static Dictionary<string, string> _downloadedDictionary = new Dictionary<string, string>();
        private static readonly Task _task = new Task(ProfileImageLoader.WorkerThreadProc);
        private static readonly object _syncBlock = new object();
        private static ProfileImageLoader.PendingRequest _currentRequest;

        /// <summary>
        /// Ожидающие запросф
        /// </summary>
        private static readonly Queue<ProfileImageLoader.PendingRequest> _pendingRequests = new Queue<ProfileImageLoader.PendingRequest>();

        static ProfileImageLoader()
        {
            ProfileImageLoader._task.Start();
        }

        private static void WorkerThreadProc()
        {
            while (true)
            {
                lock (ProfileImageLoader._syncBlock)
                {
                    if (ProfileImageLoader._pendingRequests.Count == 0 || ProfileImageLoader._currentRequest != null)
                    {
                        Monitor.Wait(ProfileImageLoader._syncBlock, 300);
                    }
                    else
                    {
                        ProfileImageLoader._currentRequest = ProfileImageLoader._pendingRequests.Dequeue();
                        ProfileImageLoader.DownloadFileAndCache(ProfileImageLoader._currentRequest.Uri);
                    }
                }
            }
        }

        private static async void DownloadFileAndCache(string uri)
        {
            string ret = null;
            //Windows.Storage.Streams.IRandomAccessStream rs = null;

            if (ProfileImageLoader._downloadedDictionary.ContainsKey(uri))
            {
                ret = ProfileImageLoader._downloadedDictionary[uri];
            }
            else
            {
                HttpClient http = new HttpClient();
                byte[] image_from_web_as_bytes = await http.GetByteArrayAsync(uri);
                //ret = await CacheManager.TrySaveRawCachedData(image_from_web_as_bytes, uri.GetHashCode().ToString() + ".jpg");

                //https://vk.com/sticker/1-12969-512b
                //Regex r = new Regex("-(\\d*)-");
                //Match m = r.Match(uri);
                //string temp = m.Groups[1].Value;
                var sf = await CacheManager2.WriteToCache(new Uri(uri), "Cache/Stickers/" + uri.GetHashCode().ToString() + ".jpg");
                ret = sf.Path;
                //rs = await sf.OpenReadAsync();
            }
            if (ret == null )
            {
                return;
            }
            Execute.ExecuteOnUIThread(()=>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    
                    //if(rs != null)
                    //    bitmapImage.SetSource(rs);
                   // else
                        bitmapImage.UriSource = new Uri(ret);
                    ProfileImageLoader._currentRequest.Image.Source = bitmapImage;
                    ProfileImageLoader._currentRequest = null;

                    if (!ProfileImageLoader._downloadedDictionary.ContainsKey(uri))
                        ProfileImageLoader._downloadedDictionary.Add(uri, ret);
                });
            
        }

        public static void SetUriSource(Image image, string value)
        {
            if (image == null)
                throw new ArgumentNullException("obj");

            // Картинка ложится в кеш
            if (value == null)
                return;

            if(ProfileImageLoader._downloadedDictionary.ContainsKey(value))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.UriSource = new Uri(ProfileImageLoader._downloadedDictionary[value]);
                image.Source = bitmapImage;
            }
            else
            {
                ProfileImageLoader.AddPendingRequest(image, value);
            }
        }
        /*
        public static void SaveState()
        {

            ProfileImageLoader.SerializedData serializedData = new ProfileImageLoader.SerializedData() { DownloadedUris = (List<string>)Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Skip<string>(VeryLowProfileImageLoader._downloadedList, Math.Max(0, Enumerable.Count<string>(VeryLowProfileImageLoader._downloadedList) - 1000)), 1000)) };
                CacheManager.TrySerialize(serializedData, "VeryLowProfileImageLoaderData", false, CacheManager.DataType.CachedData);
            
        }

        public static void RestoreState()
        {
            ProfileImageLoader.SerializedData serializedData = new ProfileImageLoader.SerializedData();
                CacheManager.TryDeserialize(serializedData, "VeryLowProfileImageLoaderData", CacheManager.DataType.CachedData);
                VeryLowProfileImageLoader._downloadedList = serializedData.DownloadedUris;
                foreach (string downloaded in VeryLowProfileImageLoader._downloadedList)
                    VeryLowProfileImageLoader._downloadedDictionary[downloaded] = "";
           
        }
        */
        private static void AddPendingRequest(Image image, string uri)
        {
            lock (ProfileImageLoader._syncBlock)
            {
                ProfileImageLoader._pendingRequests.Enqueue(new ProfileImageLoader.PendingRequest(image, uri/*, currentAttempt*/));
                Monitor.Pulse(ProfileImageLoader._syncBlock);
            }
        }

        private class PendingRequest
        {
            public Image Image { get; private set; }

            public string Uri { get; private set; }

            //public DateTime CreatedTimstamp { get; private set; }

            //public DateTime DownloadStaredTimestamp { get; set; }

            //public Guid UniqueId { get; private set; }

            //public int CurrentAttempt { get; set; }

            public PendingRequest(Image image, string uri)
            {
                this.Image = image;
                this.Uri = uri;
                //this.CreatedTimstamp = DateTime.Now;
                //this.UniqueId = Guid.NewGuid();
                //this.CurrentAttempt = currentAttempt;
            }
        }
        /*
        public class SerializedData : IBinarySerializable
        {
            public List<string> DownloadedUris { get; set; }

            public SerializedData()
            {
                this.DownloadedUris = new List<string>();
            }

            public void Write(BinaryWriter writer)
            {
                writer.WriteList(this.DownloadedUris);
            }

            public void Read(BinaryReader reader)
            {
                this.DownloadedUris = reader.ReadList();
            }
        }*/
    }
}
