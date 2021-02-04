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

using LunaVK.Framework;
using Windows.Storage;
using LunaVK.Network;
using LunaVK.Core.DataObjects;

#if FFMpeg
using FFmpegInterop;
#endif
using Windows.Media.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using LunaVK.Core;
using Windows.Networking.Connectivity;
using XamlAnimatedGif;
using LunaVK.Core.Network;

namespace LunaVK.UC
{//namespace VKClient.Common.UC.InplaceGifViewer
    public sealed partial class GifViewerUC : UserControl//public class FFmpegGifPlayerUC : UserControl
    {
        private string _localFile;
        //private DocPreview.DocPreviewVideo _data;
        private VKDocument _doc;
        #if FFMpeg
        private FFmpegInteropMSS FFmpegMSS;
#endif
        bool fileCreated;
        bool isDownloading;
        DispatcherTimer timerOffset = new DispatcherTimer();

        public GifViewerUC()
        {
            this.InitializeComponent();
            this.timerOffset.Interval = TimeSpan.FromSeconds(2);
            this.timerOffset.Tick += timerOffset_Tick;

            this.Loaded += GifViewerUC_Loaded;
            this.Unloaded += GifViewerUC_Unloaded;
        }

        private void GifViewerUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this.timerOffset.Stop();
        }

        private void GifViewerUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.ForcePause = false;
            this.timerOffset.Start();
        }

        public GifViewerUC(string preview, VKDocument doc)
            :this()
        {
            this._doc = doc;
            this.size.Text = UIStringFormatterHelper.BytesForUI(this._doc.size);

            //this._localFile = Guid.NewGuid() + ".mp4";
            //string uri = this._doc.preview.video == null ? this._doc.url : this._doc.preview.video.src;



            this._localFile = Guid.NewGuid() + (this._doc.preview.video == null ? ".gif" : ".mp4");

        }

        void timerOffset_Tick(object sender, object e)
        {
            var ttv = this.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));


            double ScreenH = (Window.Current.Content as Frame).ActualHeight;
            double y = screenCoords.Y + (base.ActualHeight / 2.0);
            //Позиция считается от верхнего левого угла
             
            double top = ScreenH * 0.2;
            double bottom = ScreenH * 0.9;

            bool isOnScreen = y > top && y < bottom;
#if DEBUG
            //System.Diagnostics.Debug.WriteLine("coord:{0} y:{1} top:{2} bottom:{3} {4}", (int)screenCoords.Y,(int)y, top, bottom, isOnScreen.ToString());
#endif

            if (isOnScreen)
            {
                //Ещё ничего не открывали
                
                    this.HandleOnScreen();
            }
            else
            {
                this.PauseInAutoPlay = true;
                this.Pause();
            }
            //Rect bounds1 = new Rect(0.0, y, 0.0, 0);
            //this.TrackPositionChanged(bounds1);
        }

        private bool PauseInAutoPlay;

        private void Stop()
        {

        }

        /// <summary>
        /// Это старый тип гиф? Сейчас этож видео...
        /// В старом preview.video отсутствовало
        /// </summary>
        public bool UseOldGifPlayer
        {
            get
            {
                DocPreview preview = this._doc.preview;
                string str;
                if (preview == null)
                {
                    str = null;
                }
                else
                {
                    DocPreview.DocPreviewVideo video = preview.video;
                    str = video != null ? video.src : null;
                }
                return string.IsNullOrWhiteSpace(str);
            }
        }
                
        public async void Play()
        {
            if (this.isDownloading == true)
                return;

            this.isDownloading = true;

            string uri = this._doc.preview.video == null ? this._doc.url : this._doc.preview.video.src;

            //https://vk.com/doc306689421_437338190?hash=7f527b270cbcdf60da&dl=GM3TKOJYHAZTCMQ:1524836049:418ade0494a114bac5&api=1&mp4=1
            var f = await CacheManager.GetStorageFileInCurrentUserCacheFolder(this._localFile);
            
            if (this.fileCreated)
            {
                var stream = await f.OpenAsync(FileAccessMode.Read);
#if FFMpeg
                this.FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(stream, forceDecodeAudio, forceDecodeVideo);
                if (this.FFmpegMSS == null)
                    return;

                MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                if (mss != null)
                {
                    // Pass MediaStreamSource to Media Element
                    Execute.ExecuteOnUIThread(() => { mediaElement.SetMediaStreamSource(mss); });
                }
                else
                {
                    //DisplayErrorMessage("Cannot open media");
                    int i = 0;
                }
#endif

                /*
                Execute.ExecuteOnUIThread(() => {
                    this.Progress(null, 100);
                    this.mediaElement.SetSource(stream, f.ContentType);
                    this.mediaElement.AutoPlay = true;
                    this.mediaElement.Play();
                });*/
                //
                Execute.ExecuteOnUIThread(() => {
                    this.Progress(null, 100);
                    if (this.UseOldGifPlayer)
                    {
                        var newStream = stream.AsStreamForRead();
                        AnimationBehavior.SetSourceStream(this.imageGif, newStream);
                    }
                    else
                    {
                        this.mediaElement.SetSource(stream, f.ContentType);
                        this.mediaElement.AutoPlay = true;
                        this.mediaElement.Play();
                    }
                });

                //
                isDownloading = false;
            }
            else
            {
                
                var stream = await f.OpenAsync(FileAccessMode.ReadWrite);

                JsonWebRequest.DownloadToStream(uri, stream, (s, res) =>
                {
                    
                    if (res == true)
                    {

                        fileCreated = true;
#if FFMpeg
                        this.FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(stream, forceDecodeAudio, forceDecodeVideo);
                        if (this.FFmpegMSS == null)
                            return;

                        MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();
                        
                        if (mss != null)
                        {
                            // Pass MediaStreamSource to Media Element
                            Execute.ExecuteOnUIThread(() => { mediaElement.SetMediaStreamSource(mss); });
                        }
                        else
                        {
                            //DisplayErrorMessage("Cannot open media");
                            int i = 0;
                        }
#endif
                        /*
                        Execute.ExecuteOnUIThread(() => {
                            this.mediaElement.SetSource(stream, f.ContentType);
                            this.mediaElement.AutoPlay = true;
                            this.mediaElement.Play();
                        });
                        */
                        //
                        Execute.ExecuteOnUIThread(() => {
                            if (this.UseOldGifPlayer)
                            {
                                var newStream = stream.AsStreamForRead();
                                AnimationBehavior.SetSourceStream(this.imageGif, newStream);
                            }
                            else
                            {
                                this.mediaElement.SetSource(stream, f.ContentType);
                                this.mediaElement.AutoPlay = true;
                                this.mediaElement.Play();
                            }
                        });

                        //
                    }

                    isDownloading = false;
                }, this.Progress);
            }

            
        }

        private void Progress(object sender, double percent)
        {
            Execute.ExecuteOnUIThread(() => { this.ring.Progress = percent; });
        }
        
        private void mediaElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            MediaElement element = sender as MediaElement;
            if(element.CurrentState == MediaElementState.Playing)
            {
                this.overlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                
            }

            if (element.CurrentState == MediaElementState.Paused)
            {
                if(!this.ForcePause && !this.PauseInAutoPlay)
                    element.Play();
            }
        }

        private void Pause()
        {
            if (this.UseOldGifPlayer)
            {
                AnimationBehavior.SetSourceUri(this.imageGif, null);
            }
            else
            {
                this.overlay.Visibility = Visibility.Visible;
                this.mediaElement.Pause();
            }
        }

        private bool ForcePause;

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(this.UseOldGifPlayer)
            {
                if(this.imageGif.Source==null)
                {
                    this.Play();
                }
                else
                {
                    this.overlay.Visibility = Visibility.Visible;
                    AnimationBehavior.SetSourceUri(this.imageGif, null);
                }
            }
            else
            {
                //Изначально Closed

                if (this.mediaElement.CurrentState == MediaElementState.Playing)
                {
                    this.ForcePause = true;
                    this.Pause();
                }
                else if (this.mediaElement.CurrentState == MediaElementState.Paused)//есть картинка, но мы в паузе
                    this.mediaElement.Play();
                else if (this.mediaElement.CurrentState != MediaElementState.Playing)
                {
                    //if (this.UseOldGifPlayer)
                    //    this.PlayPauseOld(this._doc.url);
                    //else
                    this.Play();
                }
                else
                {

                }
            }
            

        }








        public void TrackPositionChanged(Rect bounds, double offset = 0)
        {
            double height = base.Height;
            double num2 = offset + height;
            double y = bounds.Y;
            
            double num3 = y + bounds.Height;
            if (num2 < y || offset > num3)
            {
                this.Pause();
            }
            else
            {
                double num4 = offset >= y ? (num2 <= num3 ? 100.0 : (num3 - offset) * 100.0 / height) : (num2 - y) * 100.0 / height;
                if (num4 > 60.0)
                {
                    this.HandleOnScreen();
                }
                else
                {
                    if (num4 >= 20.0)
                        return;
                    this.Pause();
                }
            }
        }
        
        private bool AllowAutoplay
        {
            get
            {
                var gifAutoplayType = Settings.GifAutoplayType;
                
                if (gifAutoplayType == Core.Library.Threelen.On)
                    return true;
                if (gifAutoplayType == Core.Library.Threelen.Third)
                {
                    ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                    bool isWiFi = (InternetConnectionProfile == null) ? false : InternetConnectionProfile.IsWlanConnectionProfile;
                    //bool isGsm = (InternetConnectionProfile == null) ? false : InternetConnectionProfile.IsWwanConnectionProfile;
                    bool isEthernet = (InternetConnectionProfile == null) ? false : InternetConnectionProfile.ProfileName == "Ethernet";
                    return isWiFi || isEthernet;
                }
                return false;
            }
        }

        internal void HandleOnScreen()
        {
            if (this.ForcePause)
                return;

            if (this.UseOldGifPlayer)
            {
                if (this.imageGif.Source == null)
                {
                    //this.Play();
                }
            }
            else
            {
                if (this.mediaElement.CurrentState == MediaElementState.Paused)
                {
                    this.mediaElement.Play();
                }
                else if (this.mediaElement.CurrentState == MediaElementState.Closed)
                {
                    if (this.AllowAutoplay)
                        this.Play();
                }
            }
        }
    }
}
