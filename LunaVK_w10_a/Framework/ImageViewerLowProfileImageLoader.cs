using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using System.IO;
using LunaVK.Core.Network;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Framework;

namespace LunaVK.Framework
{
    public class ImageViewerLowProfileImageLoader
    {
        private static readonly Task _thread = new Task(ImageViewerLowProfileImageLoader.WorkerThreadProc);


        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(Uri), typeof(ImageViewerLowProfileImageLoader), new PropertyMetadata(null, ImageViewerLowProfileImageLoader.OnUriSourceChanged));

        private static bool _enableLog = false;
        private static readonly object _syncBlock = new object();
        private static readonly Queue<ImageViewerLowProfileImageLoader.PendingRequest> _pendingRequests = new Queue<ImageViewerLowProfileImageLoader.PendingRequest>();
        private static bool InProcess;

        /// <summary>
        /// Мы делаем подгрузку поочереди? Т.е. медленно.
        /// </summary>
        public static bool IsEnabled { get; set; }

        public static event EventHandler ImageDownloaded;

        public static event EventHandler<double> ImageDownloadingProgress;

        static ImageViewerLowProfileImageLoader()
        {
            ImageViewerLowProfileImageLoader._thread.Start();
            ImageViewerLowProfileImageLoader.IsEnabled = true;
        }

        public static Uri GetUriSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (Uri)((DependencyObject)obj).GetValue(ImageViewerLowProfileImageLoader.UriSourceProperty);
        }

        public static void SetUriSource(Image obj, Uri value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            ((DependencyObject)obj).SetValue(ImageViewerLowProfileImageLoader.UriSourceProperty, value);
        }

        private static void WorkerThreadProc()
        {
            while (true)
            {
                lock (_syncBlock)
                {
                    if (_pendingRequests.Count == 0)//если нет заданий
                    {
                        ImageViewerLowProfileImageLoader.Log("Monitor.Wait _pendingRequests.Count == 0");
                        Monitor.Wait(_syncBlock);//то ждём, когда они появятся
                        continue;
                    }

                    if (!InProcess)
                    {
                        ImageViewerLowProfileImageLoader.Log("_pendingRequests.Dequeue (Удаляет и возвращает)");
                        var temp = _pendingRequests.Dequeue();
                        InProcess = true;
                        JsonWebRequest.DownloadToStream(temp.Uri, temp.Stream, (s, res) =>
                        {
                            if (res == true)
                            {
                                ImageViewerLowProfileImageLoader.ImageDownloaded?.Invoke(null, new EventArgs());
                                //Execute.ExecuteOnUIThread(async () =>
                                //{
                                //    BitmapImage bitmapImage = new BitmapImage();
                                //    await bitmapImage.SetSourceAsync(temp.Stream);
                                //    temp.Image.Source = bitmapImage;
                                //});
                                Execute.ExecuteOnUIThread(() =>
                                {
                                    BitmapImage bitmapImage = new BitmapImage();
                                    bitmapImage.SetSource(temp.Stream);
                                    temp.Image.Source = bitmapImage;
                                });
                            }

                            InProcess = false;

                            lock (_syncBlock)
                            {
                                ImageViewerLowProfileImageLoader.Log("Monitor.Pulse InProcess = false;");
                                Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);//освобождаем поток
                            }


                        }, (s2, progress) => {
                            ImageViewerLowProfileImageLoader.ImageDownloadingProgress?.Invoke(temp.Image, progress);
                        });

                        Task.Delay(30);
                    }
                }
            }
        }
        /*
        private static void WorkerThreadProc0()
        {
            List<ImageViewerLowProfileImageLoader.PendingRequest> pendingRequestList = new List<ImageViewerLowProfileImageLoader.PendingRequest>();
            Queue<IAsyncResult> asyncResultQueue = new Queue<IAsyncResult>();
            while (true)
            {
                lock (ImageViewerLowProfileImageLoader._syncBlock)
                {
                    if (ImageViewerLowProfileImageLoader._pendingRequests.Count == 0 && ImageViewerLowProfileImageLoader._pendingResponses.Count == 0 && (pendingRequestList.Count == 0 && asyncResultQueue.Count == 0))
                    {
                        Monitor.Wait(ImageViewerLowProfileImageLoader._syncBlock, 500);
                    }
                    while (0 < ImageViewerLowProfileImageLoader._pendingRequests.Count)
                    {
                        ImageViewerLowProfileImageLoader.PendingRequest local_7 = ImageViewerLowProfileImageLoader._pendingRequests.Dequeue();
                        for (int local_8 = 0; local_8 < pendingRequestList.Count; ++local_8)
                        {
                            if (pendingRequestList[local_8].Image == local_7.Image)
                            {
                                pendingRequestList[local_8] = local_7;
                                local_7 = null;
                                break;
                            }
                        }
                        if (local_7 != null)
                            pendingRequestList.Add(local_7);
                    }
                    while (0 < ImageViewerLowProfileImageLoader._pendingResponses.Count)
                        asyncResultQueue.Enqueue(ImageViewerLowProfileImageLoader._pendingResponses.Dequeue());
                }
                Queue<ImageViewerLowProfileImageLoader.PendingCompletion> pendingCompletions = new Queue<ImageViewerLowProfileImageLoader.PendingCompletion>();
                int count = pendingRequestList.Count;
                for (int index1 = 0; 0 < count && index1 < 5; ++index1)
                {
                    int index2 = random.Next(count);
                    ImageViewerLowProfileImageLoader.PendingRequest pendingRequest = pendingRequestList[index2];
                    pendingRequestList[index2] = pendingRequestList[count - 1];
                    pendingRequestList.RemoveAt(count - 1);
                    --count;



                    HttpWebRequest http = WebRequest.CreateHttp(pendingRequest.Uri);
                    //http.AllowReadStreamBuffering = true;
                    http.BeginGetResponse(new AsyncCallback(ImageViewerLowProfileImageLoader.HandleGetResponseResult), new ImageViewerLowProfileImageLoader.ResponseState(http, pendingRequest.Image, pendingRequest.Uri, pendingRequest.Timestamp));

                    UC.AudioRecorderUC.AudioAmplitudeStream stream = new UC.AudioRecorderUC.AudioAmplitudeStream();
                    LunaVK.Network.JsonWebRequest.DownloadToStream(pendingRequest.Uri.AbsoluteUri, stream, (s, res) =>
                    {
                        lock (ImageViewerLowProfileImageLoader._syncBlock)
                        {
                            ImageViewerLowProfileImageLoader._pendingResponses.Enqueue((IAsyncResult)new ImageViewerLowProfileImageLoader.ResponseState(null, pendingRequest.Image, pendingRequest.Uri, pendingRequest.Timestamp));
                            Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);
                        }

                    }, (s2, progress) => { ImageViewerLowProfileImageLoader.ImageDownloadingProgress?.Invoke(pendingRequest.Image, progress); });

                    Task.Delay(1); //Thread.Sleep(1);
                }

                for (int index = 0; 0 < asyncResultQueue.Count && index < 5; ++index)
                {
                    IAsyncResult asyncResult = asyncResultQueue.Dequeue();
                    ImageViewerLowProfileImageLoader.ResponseState responseState = (ImageViewerLowProfileImageLoader.ResponseState)asyncResult.AsyncState;
                    try
                    {
                        WebResponse response = responseState.WebRequest.EndGetResponse(asyncResult);
                        pendingCompletions.Enqueue(new ImageViewerLowProfileImageLoader.PendingCompletion(responseState.Image, responseState.Uri, response.GetResponseStream(), responseState.Timestamp));
                    }
                    catch (WebException ex)
                    {
                        ImageViewerLowProfileImageLoader.Log(string.Format("LowProfileImageLoader exception when fetching {0}", (object)responseState.Uri.OriginalString));
                    }
                    Task.Delay(1);//Thread.Sleep(1);
                }

                if (0 < pendingCompletions.Count)
                {
                    Execute.ExecuteOnUIThread( async () =>//Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        while (0 < pendingCompletions.Count)
                        {
                            ImageViewerLowProfileImageLoader.PendingCompletion pendingCompletion = pendingCompletions.Dequeue();
                            if (ImageViewerLowProfileImageLoader.GetUriSource(pendingCompletion.Image) == pendingCompletion.Uri)
                            {
                                ImageViewerLowProfileImageLoader.Log(string.Format("Downloaded image {0} in {1} ms.", pendingCompletion.Uri.OriginalString, (DateTime.Now - pendingCompletion.Timestamp).TotalMilliseconds));

                                BitmapImage bitmapImage = new BitmapImage();
                                
                                var stream = pendingCompletion.Stream;
                                var rstream = stream.AsRandomAccessStream();
                                await bitmapImage.SetSourceAsync(rstream);//не было AsRandomAccessStream
                                  

                                pendingCompletion.Image.Source = bitmapImage;
                                
                                ImageViewerLowProfileImageLoader.ImageDownloaded?.Invoke(null, new EventArgs());
                                
                            }
                            pendingCompletion.Stream.Dispose();
                        }
                    });
                }
            }
        }
    */

        private static void AddToCache(Uri uri, Stream stream)
        {
            /*
            VeryLowProfileImageLoader.EnsureLRUCache();
            VeryLowProfileImageLoader._lruCache.Add(uri, (Stream)StreamUtils.ReadFully(stream), true);
            stream.Position = 0L;
            if (UriExtensions.ParseQueryString(uri).ContainsKey(VeryLowProfileImageLoader.REQUIRE_CACHING_KEY))
                ImageCache.Current.TrySetImageForUri(uri.OriginalString, (Stream)StreamUtils.ReadFully(stream));
            stream.Position = 0L;*/
        }

        private async static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            _canceled = false;

            Image image = (Image)o;
            Uri newValue = (Uri)e.NewValue;
            if (newValue == null)
            {
                image.Source = null;
                ImageViewerLowProfileImageLoader.Log("OnUriSourceChanged uri = NULL");
            }
            else
            {
                //ImageViewerLowProfileImageLoader.Log(string.Format("OnUriSourceChanged uri = {0}", newValue));
                var cachedImageStream = await ImageCache.Current.GetCachedImageStream(newValue.OriginalString);
                if (cachedImageStream != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    //bitmapImage.CreateOptions = ((BitmapCreateOptions)18);
                    bitmapImage.SetSource(cachedImageStream);//await bitmapImage.SetSourceAsync(cachedImageStream);
                    image.Source = bitmapImage;

                    ImageViewerLowProfileImageLoader.ImageDownloaded?.Invoke(null, new EventArgs());
                }
                else if (!newValue.IsAbsoluteUri || !ImageViewerLowProfileImageLoader.IsEnabled /*|| DesignerProperties.IsInDesignTool*/)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    //bitmapImage.UriSource = newValue;
                    //image.Source = bitmapImage;


                    ImageViewerLowProfileImageLoader.ImageDownloaded?.Invoke(image, new EventArgs());

                    UC.AudioRecorderUC.AudioAmplitudeStream stream = new UC.AudioRecorderUC.AudioAmplitudeStream();
                    JsonWebRequest.DownloadToStream(newValue.AbsoluteUri, stream, (s, res) =>
                    {
                        if (_canceled == true)
                            return;

                        if (res == true)
                        {
                            //Execute.ExecuteOnUIThread(async () =>
                            //{
                            //    await bitmapImage.SetSourceAsync(stream);
                            //    image.Source = bitmapImage;
                            //});
                            Execute.ExecuteOnUIThread(() =>
                            {
                                bitmapImage.SetSource(stream);
                                image.Source = bitmapImage;
                            });
                        }
                        ImageViewerLowProfileImageLoader.Log(string.Format("Downloaded image {0}", newValue.AbsoluteUri));

                    }, (s2, progress) => { ImageViewerLowProfileImageLoader.ImageDownloadingProgress?.Invoke(image, progress); });

                }
                else
                {
                    lock (ImageViewerLowProfileImageLoader._syncBlock)
                    {
                        ImageViewerLowProfileImageLoader.Log("_pendingRequests.Enqueue (Добавляет задание)");
                        ImageViewerLowProfileImageLoader._pendingRequests.Enqueue(new ImageViewerLowProfileImageLoader.PendingRequest(image, newValue.AbsoluteUri));
                        Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);
                    }
                }
            }
        }

        private static bool _canceled;
        public static void Cancel()
        {
            _canceled = true;
            lock (ImageViewerLowProfileImageLoader._syncBlock)
            {
                ImageViewerLowProfileImageLoader._pendingRequests.Clear();
            }
            }

        private static void Log(string info)
        {
            if (!ImageViewerLowProfileImageLoader._enableLog)
                return;
            System.Diagnostics.Debug.WriteLine(info);
        }

        private class PendingRequest
        {
            public Image Image { get; private set; }

            public string Uri { get; private set; }

            public DateTime Timestamp { get; private set; }

            public UC.AudioRecorderUC.AudioAmplitudeStream Stream { get; private set; }

            public PendingRequest(Image image, string uri)
            {
                this.Image = image;
                this.Uri = uri;
                this.Timestamp = DateTime.Now;
                this.Stream = new UC.AudioRecorderUC.AudioAmplitudeStream();
            }
        }

    }
}
