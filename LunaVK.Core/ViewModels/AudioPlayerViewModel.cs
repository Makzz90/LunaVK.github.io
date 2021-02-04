using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.Framework;
using Windows.Media;
using Windows.Media.Core;
using System.Linq;
using Windows.Devices.Enumeration;
using LunaVK.Core.Library;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;
using Windows.Web.Http;
using System.Text.RegularExpressions;
using LunaVK.Core.Network;
using Windows.Storage.Streams;

#if WINDOWS_PHONE_APP || WINDOWS_UWP
using Windows.Media.Playback;
#endif

namespace LunaVK.Core.ViewModels
{
    /// <summary>
    /// Это для списка воспроизведения на странице плеера и миниплеера
    /// </summary>
    public class AudioPlayerViewModel : ViewModelBase
    {
        private static AudioPlayerViewModel _instance;
        public static AudioPlayerViewModel Instance
        {
            get
            {
                if (AudioPlayerViewModel._instance == null)
                    AudioPlayerViewModel._instance = new AudioPlayerViewModel();

                return AudioPlayerViewModel._instance;
            }
        }

//        public event EventHandler<int> TrackChanged;

        /// <summary>
        /// Список треков для воспроизведения
        /// </summary>
        public ObservableCollection<VKAudio> Tracks { get; private set; }
        private DispatcherTimer _localTimer = new DispatcherTimer();
        public int CurentTrackId;

        int prevPos = -1;

        public VKAudio CurrentTrack
        {
            get
            {
                if (this.Tracks.Count == 0)
                    return null;
                return this.Tracks[this.CurentTrackId];
            }
        }
        

#region VM
        /// <summary>
        /// Количество оставшихся секунд у трека
        /// </summary>
        public int RemainingSeconds
        {
            get
            {
                if (this.CurrentTrack == null)
                    return 0;
                if (this.CurrentTrack.duration == 0)
                    return (int)this.media.PlaybackSession.NaturalDuration.TotalSeconds;
                return this.CurrentTrack.duration;
            }
        }

        /// <summary>
        /// Количество оставшихся секунд у трека в виде текста
        /// </summary>
        public string RemainingStr
        {
            get
            {
                if (this.RemainingSeconds == 0)
                    return "";
                int durationSeconds = (int)Math.Round((this.Duration - this.Position).TotalSeconds);
                if (this.Position > this.Duration)
                    durationSeconds = 0;
                return string.Format("-{0}", UIStringFormatterHelper.FormatDuration(durationSeconds));
            }
        }

        public string TrackName
        {
            get
            {
                VKAudio track = this.CurrentTrack;
                if (track == null)
                    return "";
                return track.title;
            }
        }

        public string ArtistName
        {
            get
            {
                VKAudio track = this.CurrentTrack;
                if (track == null)
                    return "";
                return track.artist;
            }
        }

        public float PrevButtonOpacity
        {
            get { return this.CurentTrackId > 0 ? 1.0f : 0.4f; }
        }

        public float NextButtonOpacity
        {
            get { return this.CurentTrackId < (this.Tracks.Count - 1) ? 1.0f : 0.4f; }
        }


        /// <summary>
        /// От этого зависит видимость миниплеера (и больше ничего)
        /// </summary>
        public Visibility CurrentTrackVisibility
        {
            get
            {
                if (media == null)
                    return Visibility.Collapsed;

                if ( (media.PlaybackSession.PlaybackState != MediaPlaybackState.Playing && media.PlaybackSession.PlaybackState != MediaPlaybackState.Paused) || string.IsNullOrWhiteSpace(this.TrackName))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public double MiniPlayerProgressWidth
        {
            get
            {
                //if (CustomFrame.Instance.Menu == null)
                    return 0;
                //double totalSeconds2 = this.Duration.TotalSeconds;
                //if (totalSeconds2 == 0)
                //    return 0;
                //double totalSeconds1 = this.Position.TotalSeconds;
                
                //return Math.Round(CustomFrame.Instance.Menu.VisibleWidth * (totalSeconds1 / totalSeconds2), MidpointRounding.AwayFromZero);
            }
        }

        //private Framework.CustomFrame CFrame
        //{
        //    get { return Window.Current.Content as Framework.CustomFrame; }
        //}
        
        private SystemMediaTransportControls systemControls;

        private BitmapImage _artwork;
        public BitmapImage Artwork
        {
            get
            {
                VKAudio track = this.CurrentTrack;
                if (track == null)
                    return null;
                return this._artwork;
            }
            set
            {
                this._artwork = value;
                base.NotifyPropertyChanged("Artwork");
            }
        }

        private TimeSpan Duration
        {
            get
            {
                
                if(this.CurrentTrack == null)
                    return new TimeSpan();
                if (this.CurrentTrack.duration == 0)
                    return this.media.PlaybackSession.NaturalDuration;
                return TimeSpan.FromSeconds(this.CurrentTrack.duration);
            }
        }

        private TimeSpan Position
        {
            get
            {
                try
                {
                    if(this.media==null)
                        return new TimeSpan();
                    return this.media.PlaybackSession.Position;
                }
                catch (Exception)
                {
                    return new TimeSpan();
                }
            }
            set
            {
                try
                {
                    media.PlaybackSession.Position = value;
                }
                catch (Exception ex)
                {
                }
            }
        }
#endregion
        
        private MediaPlayer media;
        private MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
        private DeviceWatcher watcher;

        public AudioPlayerViewModel()
        {
            this.Tracks = new ObservableCollection<VKAudio>();
            
            this._localTimer.Interval = TimeSpan.FromSeconds(1);
            this._localTimer.Tick += _localTimer_Tick;
            this._localTimer.Start();
            
            this.systemControls = SystemMediaTransportControls.GetForCurrentView();

            // Register to handle the following system transpot control buttons.
            this.systemControls.ButtonPressed += SystemControls_ButtonPressed;
            
            this.mediaPlaybackList.CurrentItemChanged += L_CurrentItemChanged;


            
            //media2 = new MediaPlayerElement() { AutoPlay = false };
            
            try
            {
                media = new MediaPlayer() { AutoPlay = false };
                media.CommandManager.IsEnabled = false;

                media.Source = this.mediaPlaybackList;
                media.CurrentStateChanged += Media_CurrentStateChanged;
                Application.Current.LeavingBackground += Current_LeavingBackground;
            }
            catch
            {

            }



            this.systemControls.IsEnabled = true;
            this.systemControls.IsPlayEnabled = false;
            this.systemControls.IsPauseEnabled = true;//todo: не должно быть так
            this.systemControls.IsPreviousEnabled = false;
            this.systemControls.IsNextEnabled = false;

            base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));
            base.NotifyPropertyChanged("PlayPauseIcon");

            this.Initialize();

            

            this.watcher = DeviceInformation.CreateWatcher(DeviceClass.AudioRender);

            watcher.Removed += Watcher_Removed;
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (this.PlaybackState == MediaPlaybackState.Playing)
                this.PlayPause();
        }

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            EventAggregator.Instance.PublishAudioPlayerStateChanged(media.PlaybackSession.PlaybackState);
            base.NotifyPropertyChanged("PlayPauseIcon");
        }

        private async void Initialize()
        {
            await AudioCacheManager.Instance.Init();
        }

        private void Media_CurrentStateChanged(MediaPlayer sender, object args)
        {
            bool flag = true;
            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlaybackState.Buffering:
                case MediaPlaybackState.Opening:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    flag = false;
                    break;
                default:
                    break;
            }

            EventAggregator.Instance.PublishAudioPlayerStateChanged(sender.PlaybackSession.PlaybackState);

            base.NotifyPropertyChanged("PlayPauseIcon");

            if (flag)
                base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));
        }


        public MediaPlaybackState PlaybackState
        {
            get { return media.PlaybackSession.PlaybackState; }
        }

        string _id = "";

        public void FillTracks(IReadOnlyList<VKAudio> tracks, string id)
        {
            if (string.IsNullOrEmpty(this._id))
                this._id = id;
            else
            {
                //if (this._id == id)
                //    return;
                //else
                    this._id = id;
            }

            this.mediaPlaybackList.Items.Clear();

            //this.CurentTrackId = -1;
            this.Tracks.Clear();
            this.Position = new TimeSpan();

            int i = 0;

            //this.InProcess = true;

            foreach (var track in tracks)
            {
                this.Tracks.Add(track);
                MediaPlaybackItem item = new MediaPlaybackItem(MediaSource.CreateFromMediaBinder(this.CreateBinder(i)));
                //MediaPlaybackItem item = new MediaPlaybackItem(MediaSource.CreateFromMediaBinder(this.CreateBinder(track.ToString())));

                this.mediaPlaybackList.Items.Add(item);
                i++;
            }

            //this.InProcess = false;
        }
        
        private MediaBinder CreateBinder(int number)
        {
            MediaBinder binder = new MediaBinder();
            binder.Token = number.ToString();
            binder.Binding += this.Binder_Binding;
            return binder;
        }
        /*
        private MediaBinder CreateBinder(string token)
        {
            MediaBinder binder = new MediaBinder();
            binder.Token = token;
            binder.Binding += this.Binder_Binding;
            return binder;
        }
        */

        /// <summary>
        /// Вызывается, когда пришло время задать путь к источнику музыки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Binder_Binding(MediaBinder sender, MediaBindingEventArgs args)
        {
            var deferral = args.GetDeferral();// Get a deferral if you need to perform async operations
            /*
            if (this.InProcess && this.CurentTrackId == -1)
            {
#if DEBUG
                //Мы ещё добавляем элементы
                Debug.WriteLine(string.Format("deferral.Complete(); "));
#endif
                deferral.Complete();
                return;
            }
            */
            int number = int.Parse(args.MediaBinder.Token);
            //VKAudio track = this.Tracks.FirstOrDefault((a => a.ToString() == args.MediaBinder.Token));
            
            //int number = this.Tracks.IndexOf(track);
            /*
            if(this.CurentTrackId==-1 && number != 0)
            {
#if DEBUG
                //Такого не может быть
                Debug.WriteLine(string.Format("deferral.Complete(); this.CurentTrackId==-1"));
#endif
                deferral.Complete();
                return;
            }
            */
            if ( Math.Abs(this.CurentTrackId -number) > 4)
            {
#if DEBUG
                //Бинд очень далёкого трека
                Debug.WriteLine(string.Format("deferral.Complete(); number:{0}  Math.Abs {1}", number, Math.Abs(this.CurentTrackId - number)));
#endif
                deferral.Complete();
                return;
            }

            VKAudio track = this.Tracks[number];

            /*
                        string localPath = AudioCacheManager.Instance.GetLocalFileForUniqueId(track.ToString());
                        if (string.IsNullOrEmpty(localPath))
                        {
                            AudioCacheManager.Instance.WriteAudioToCacheCallBackString(track, (fileName) =>
                            {
                                if(fileName==null)
                                {
                                    args.SetUri(new Uri(track.url));
                                    deferral.Complete();
                                    return;
                                }

                                //https://www.freeformatter.com/mime-types-list.html
                                args.SetStream(fileName, "audio/mpeg");
                            }, (percent) =>
                            {
                                this.SendPercent(number, (int)percent);
                                if((int)percent==100)
                                    deferral.Complete();
                            });
                        }
                        else
                        {
                            //BackgroundMediaPlayer.Current.SetUriSource(new Uri(localPath));

                            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                IsolatedStorageFileStream file = storeForApplication.OpenFile(localPath, FileMode.Open);
                                args.SetStream(file.AsRandomAccessStream(), "audio/mpeg");
                            }
                                //args.SetUri(new Uri(localPath));
                            track.Progress = 100;
                            deferral.Complete();
                        }
            */
            
            if (track.IsUrlUnavailable)
            {
                AudioService.Instance.GetAudio(track.owner_id, track.id, (result) =>
                {
                    if (result.error.error_code == Enums.VKErrors.None)
                    {
                        VKAudio a = result.response;
                        track.url = a.url;
                        track.duration = a.duration;
                        args.SetUri(new Uri(track.url));
                    }
                    deferral.Complete();
                });
            }
            else if(string.IsNullOrEmpty(track.url) && !string.IsNullOrEmpty(track.actionHash))
            {
                if (this.InProcess)
                {
#if DEBUG
                    Debug.WriteLine( string.Format("deferral.Complete(); number:{0}", number));
#endif
                    deferral.Complete();
                    return;
                }

                this.ProcessAudio(number, (result) =>
                {
                    if(result)
                        args.SetUri(new Uri(track.url));
                    deferral.Complete();
                });
            }
            else
            {
                args.SetUri(new Uri(track.url));
                deferral.Complete();// Call complete after your async operations are complete
            }
        }

        private bool InProcess;
        
        private void ProcessAudio(int startIndex, Action<bool> callback)
        {
            Debug.WriteLine("ProcessAudio startIndex: {0}", startIndex);


            this.InProcess = true;
            
            AudioService.Instance.ReloadAudio(this.Tracks.ToList().GetRange(startIndex,Math.Min(3, this.Tracks.Count- startIndex)), (result) => {
                
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => {
                        for(int i = 0; i < result.response.Count;i++)
                        {
                            /*
                            if(i + startIndex >= result.response.Count)
                            {
                                break;//todo: Что это за баг? вроде исправил. ловились ссылки на картинки
                            }
                            */
                            this.Tracks[i + startIndex].url = result.response[i].url;
                            Debug.WriteLine("{0}={1}", i + startIndex, this.Tracks[i + startIndex].url);
                        }
                        this.InProcess = false;
                        callback(true);

                    });
                }
                else
                {
                    this.InProcess = false;
                    callback(false);
                }
            });
        }
        
        private void SendPercent(int number, int percent)
        {
            VKAudio track = this.Tracks[number];
            track.Progress = percent;
        }

        private void L_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            int index = sender.Items.IndexOf(args.NewItem);
            int count = sender.Items.Count;

            if (index < 0 )
                return;

            //if (this.TrackChanged != null)
            //{
            //    Execute.ExecuteOnUIThread(() => { this.TrackChanged(this, index); });
            //}
            EventAggregator.Instance.PublishAudioPlayerStateChanged(media.PlaybackSession.PlaybackState);


            this.CurentTrackId = index;

            Execute.ExecuteOnUIThread(()=> {
                base.NotifyPropertyChanged(nameof(this.PrevButtonOpacity));
                base.NotifyPropertyChanged(nameof(this.NextButtonOpacity));
                base.NotifyPropertyChanged(nameof(this.RemainingStr));

                base.NotifyPropertyChanged(nameof(this.TrackName));
                base.NotifyPropertyChanged(nameof(this.ArtistName));
                this.FetchArtwork();
            });
            
            this.UpdateUVCOnNewTrack(this.Tracks[index]);
        }
        
        private void Media_MediaEnded(MediaPlayer sender, object args)
        {
            this.NextTrack();
        }
        
        /// <summary>
        /// Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUVCOnNewTrack(VKAudio audio)
        {
            systemControls.IsEnabled = this.CurentTrackId != -1;

            systemControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemControls.DisplayUpdater.MusicProperties.Title = audio == null ? "" : audio.title;
            systemControls.DisplayUpdater.MusicProperties.Artist = audio == null ? "" : audio.artist;
            //systemControls.DisplayUpdater.MusicProperties.AlbumArtist = "album artist";

            systemControls.IsPlayEnabled = this.CurentTrackId != -1;
            systemControls.IsNextEnabled = this.CurentTrackId + 1 < this.Tracks.Count;
            systemControls.IsPreviousEnabled = this.CurentTrackId > 0;

            if(audio!= null && !string.IsNullOrEmpty( audio.cover))
            {
                // Set the album art thumbnail.
                RandomAccessStreamReference r = RandomAccessStreamReference.CreateFromUri(new Uri(audio.cover));
                systemControls.DisplayUpdater.Thumbnail = r;
            }
            else if(systemControls.DisplayUpdater.Thumbnail!=null)
            {
                systemControls.DisplayUpdater.Thumbnail = null;
            }
            
            systemControls.DisplayUpdater.Update();
        }
        
        
        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            Execute.ExecuteOnUIThread(() =>
            { 
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                    case SystemMediaTransportControlsButton.Pause:
                        this.PlayPause();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                        this.Stop();
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        this.NextTrack();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        this.PrevTrack();
                        break;
                    default:
                        break;
                }
            });
        }

        //private string _fetchingArtworkForTrackid = "";

        public void FetchArtwork()
        {
            this.Artwork = null;

            var track1 = this.CurrentTrack;
            if (track1 == null)
                return;

            if(!string.IsNullOrEmpty( track1.cover))
            {
                this.Artwork = track1.Cover;
                return;
            }

            string tag = track1.ToString();
            //if (this._fetchingArtworkForTrackid == tag)
            //    return;

            //this._fetchingArtworkForTrackid = tag;
            AudioService.Instance.GetAlbumArtwork(track1.artist + " " + track1.title, (res =>
            {
                //this._fetchingArtworkForTrackid = "";
                if (string.IsNullOrEmpty(res))
                    return;

                
                    var track2 = this.CurrentTrack;
                    if (track2 == null || track2.ToString() != tag)
                        return;
                //this._trackIdOfArtwork = tag;
                Execute.ExecuteOnUIThread(() =>
                {
                    this.Artwork = new BitmapImage(new Uri(res));
                });
            }));
        }

        private void _localTimer_Tick(object sender, object e)
        {
            if (prevPos != (int)this.Position.TotalSeconds)
            {
                prevPos = (int)this.Position.TotalSeconds;
                base.NotifyPropertyChanged("MiniPlayerProgressWidth");
                base.NotifyPropertyChanged(nameof(this.PositionStr));
                base.NotifyPropertyChanged("PositionSeconds");
                base.NotifyPropertyChanged("RemainingStr");
                base.NotifyPropertyChanged(nameof(this.RemainingSeconds));
            }
        }

        public void NextTrack()
        {
            this.CurentTrackId++;
            this.PlayTrack(this.CurentTrackId);
        }

        public void PrevTrack()
        {
            this.CurentTrackId--;
            this.PlayTrack(this.CurentTrackId);
        }

        public void PlayPause()
        {
            base.NotifyPropertyChanged(nameof(this.RemainingSeconds));
            if (media.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                media.Pause();
            else
            {

                if (this.Position.TotalSeconds == 0)
                    this.PlayTrack(0);
                else
                    media.Play();
            }
        }

        public void Stop()
        {
            this._id = string.Empty;
            this.mediaPlaybackList.Items.Clear();
            this.Tracks.Clear();
            this.CurentTrackId = -1;
            media.Pause();
            base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));

            EventAggregator.Instance.PublishAudioPlayerStateChanged(MediaPlaybackState.None);
            this.UpdateUVCOnNewTrack(null);
        }

        public string PlayPauseIcon
        {
            get
            {
                if (media == null)
                    return "";

                return media.PlaybackSession.PlaybackState == MediaPlaybackState.Playing ? "\xE769" : "\xE768";
            }
        }
        
        public void PlayTrack(int number)
        {
            if (number < 0)
                number = 0;
            else if (number >= this.Tracks.Count)
                number = this.Tracks.Count - 1;

            this.PlayTrack(this.Tracks[number]);
        }
        
        public void PlayTrack(VKAudio track)
        {
            this.CurentTrackId = this.Tracks.IndexOf(track);
            Debug.WriteLine("PlayTrack {0} {1} {2}",this.CurentTrackId, track.artist, track.title);
            if(this.CurentTrackId==-1)//после восстановления из кеша список иной
            {
                var tt = this.Tracks.FirstOrDefault((t)=>t.ToString() == track.ToString());
                this.CurentTrackId = this.Tracks.IndexOf(tt);

                if (this.CurentTrackId == -1)
                    this.CurentTrackId = 0;
            }

            Debug.Assert(this.CurentTrackId>=0);
            mediaPlaybackList.MoveTo((uint)this.CurentTrackId);
            
            media.Play();
            base.NotifyPropertyChanged(nameof(this.PositionStr));
            base.NotifyPropertyChanged(nameof(this.TrackName));
            base.NotifyPropertyChanged(nameof(this.ArtistName));

            this.FetchArtwork();
        }
        
        public string PositionStr
        {
            get
            {
                if (this.CurrentTrack == null)
                    return "";
                return UIStringFormatterHelper.FormatDuration((int)Math.Round(this.Position.TotalSeconds));
            }
        }

        public double PositionSeconds
        {
            get
            {
                if (this.CurrentTrack == null)
                    return 0.0;
                return this.Position.TotalSeconds;
            }
            set
            {
                //if (this.PreventPositionUpdates)
                //    return;
                //this._manualPositionSeconds = value;
                this.Position = TimeSpan.FromSeconds(value);
                //this._lastTimeManualPositionSet = DateTime.Now;
                //this.NotifyPropertyChanged<double>((Expression<Func<double>>)(() => this.PositionSeconds));
                base.NotifyPropertyChanged(nameof(this.PositionStr));
            }
        }

        public bool IsMuted
        {
            get
            {
                if (this.media == null)
                    return false;//emulator
                return this.media.IsMuted;
            }
            set
            {
                this.media.IsMuted = value;
                base.NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get
            {
                if (this.media == null)
                    return 0;//emulator
                return this.media.Volume;
            }
            set
            {
                this.media.Volume = value;
                base.NotifyPropertyChanged(nameof(this.VolumeIcon));
            }
        }

        public string VolumeIcon
        {
            get
            {
                if (this.Volume > 0.75)
                    return "\xE995";
                else if (this.Volume > 0.50)
                    return "\xE994";
                else if (this.Volume > 0.25)
                    return "\xE993";
                else
                    return "\xE992";
            }
        }
    }
}
