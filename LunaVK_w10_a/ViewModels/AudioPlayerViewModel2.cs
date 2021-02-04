using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    public class AudioPlayerViewModel2 : ViewModelBase
    {
        private bool _initialized;
        private MediaPlayer media;
        private DispatcherTimer _localTimer;
        private SystemMediaTransportControls systemControls;
        private DeviceWatcher watcher;
        public int CurentTrackId;
        private string _id = "";
        private int prevPos = -1;
        private List<int> ShuffledIndexes;

        /// <summary>
        /// Список треков для воспроизведения
        /// </summary>
        public ObservableCollection<VKAudio> Tracks { get; private set; }

        private static AudioPlayerViewModel2 _instance;
        public static AudioPlayerViewModel2 Instance
        {
            get
            {
                if (AudioPlayerViewModel2._instance == null)
                    AudioPlayerViewModel2._instance = new AudioPlayerViewModel2();

                return AudioPlayerViewModel2._instance;
            }
        }

        public AudioPlayerViewModel2()
        {
            this.Tracks = new ObservableCollection<VKAudio>();
        }

        public void FillTracks(IReadOnlyList<VKAudio> tracks, string id)
        {
            this.InitMedia();

            if (string.IsNullOrEmpty(this._id))
                this._id = id;
            else
            {
                //if (this._id == id)
                //    return;
                //else
                this._id = id;
            }

//            this.mediaPlaybackList.Items.Clear();

            this.CurentTrackId = -1;
            this.Tracks.Clear();
 //           this.Position = new TimeSpan();


            foreach (var track in tracks)
            {
                this.Tracks.Add(track);
            }
        }

        private void InitMedia()
        {
            if (this._initialized)
                return;

            this._localTimer = new DispatcherTimer();
            this._localTimer.Interval = TimeSpan.FromSeconds(1);
            this._localTimer.Tick += this._localTimer_Tick;
            this._localTimer.Start();



            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                this.media = new MediaPlayer();
                this.media.CommandManager.IsEnabled = false;
                this.systemControls = SystemMediaTransportControls.GetForCurrentView();
            }
            else
            {
                //Before version 10.0.14393 you had to create background task for your player app.
                this.media = BackgroundMediaPlayer.Current;
                this.systemControls = this.media.SystemMediaTransportControls;
            }

            this.media.AutoPlay = false;
            this.media.CurrentStateChanged += this.Media_CurrentStateChanged;
//            this.media.Source = this.mediaPlaybackList;
            this.media.MediaEnded += Media_MediaEnded;

            Window.Current.VisibilityChanged += this.Current_VisibilityChanged;

            // Register to handle the following system transpot control buttons.
            this.systemControls.ButtonPressed += this.SystemControls_ButtonPressed;

            this.systemControls.IsEnabled = false;
            this.systemControls.IsPauseEnabled = this.systemControls.IsPlayEnabled = false;
            this.systemControls.IsPreviousEnabled = false;
            this.systemControls.IsNextEnabled = false;


            this.watcher = DeviceInformation.CreateWatcher(DeviceClass.AudioRender);

            this.watcher.Removed += this.Watcher_Removed;

            this._initialized = true;

            base.NotifyPropertyChanged(nameof(this.Volume));//BugFix: громкость нам теперь известна
            base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));
        }

        private void _localTimer_Tick(object sender, object e)
        {
            if (prevPos != (int)this.Position.TotalSeconds)
            {
                prevPos = (int)this.Position.TotalSeconds;
                base.NotifyPropertyChanged(nameof(this.PositionStr));
                base.NotifyPropertyChanged(nameof(this.PositionSeconds));
                base.NotifyPropertyChanged(nameof(this.RemainingStr));
                base.NotifyPropertyChanged(nameof(this.RemainingSeconds));
            }
        }

        public void PlayTrack(VKAudio track)
        {
            this.CurentTrackId = this.Tracks.IndexOf(track);
//            Debug.WriteLine("PlayTrack {0} {1} {2}", this.CurentTrackId, track.artist, track.title);
            if (this.CurentTrackId == -1)//после восстановления из кеша список иной
            {
                var tt = this.Tracks.FirstOrDefault((t) => t.ToString() == track.ToString());
                this.CurentTrackId = this.Tracks.IndexOf(tt);

                if (this.CurentTrackId == -1)
                    this.CurentTrackId = 0;
            }

            //            Debug.Assert(this.CurentTrackId >= 0);
            //            mediaPlaybackList.MoveTo((uint)this.CurentTrackId);

            //            media.Play();
            base.NotifyPropertyChanged(nameof(this.PositionStr));
            base.NotifyPropertyChanged(nameof(this.TrackName));
            base.NotifyPropertyChanged(nameof(this.ArtistName));

            base.NotifyPropertyChanged(nameof(this.PrevButtonOpacity));
            base.NotifyPropertyChanged(nameof(this.NextButtonOpacity));
            base.NotifyPropertyChanged(nameof(this.RemainingStr));

            this.FetchArtwork();


            if (track.IsUrlUnavailable)
            {
                AudioService.Instance.GetAudio(track.owner_id, track.id, (result) =>
                {
                    if (result.error.error_code == Core.Enums.VKErrors.None)
                    {
                        VKAudio a = result.response;
                        track.url = a.url;
                        track.duration = a.duration;
                        //args.SetUri(new Uri(track.url));
                        this.media.Source = MediaSource.CreateFromUri(new Uri(track.url));
                        this.media.Play();
                    }
                });
            }
            else if (string.IsNullOrEmpty(track.url) && !string.IsNullOrEmpty(track.actionHash))
            {
                this.ProcessAudio(this.CurentTrackId, (result) =>
                {
                    if (result)
                    {
                        this.media.Source = MediaSource.CreateFromUri(new Uri(this.Tracks[this.CurentTrackId].url));//args.SetUri(new Uri(track.url));
                        this.media.Play();
                    }
                });
            }
            else
            {
                //args.SetUri(new Uri(track.url));
                this.media.Source = MediaSource.CreateFromUri(new Uri(this.Tracks[this.CurentTrackId].url));
                this.media.Play();
            }
        }

        private void ProcessAudio(int startIndex, Action<bool> callback)
        {
#if DEBUG
            Debug.WriteLine("ProcessAudio startIndex: {0}", startIndex);
#endif


            //            this.InProcess = true;

            AudioService.Instance.ReloadAudio(this.Tracks.ToList().GetRange(startIndex, Math.Min(3, this.Tracks.Count - startIndex)), (result) =>
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        for (int i = 0; i < result.response.Count; i++)
                        {
                            this.Tracks[i + startIndex].url = result.response[i].url;
#if DEBUG
                            Debug.WriteLine("{0}={1}", i + startIndex, this.Tracks[i + startIndex].url);
#endif
                        }

                        
//                        this.InProcess = false;
                        callback(true);

                    });
                }
                else
                {
//                    this.InProcess = false;
                    callback(false);
                }
            });
        }

        private BitmapImage _artwork;
        public BitmapImage Artwork
        {
            get
            {
                if (this.CurrentTrack == null)
                    return null;
                return this._artwork;
            }
            set
            {
                this._artwork = value;
                base.NotifyPropertyChanged(nameof(this.Artwork));
            }
        }

        private bool IsPlaying
        {
            get { return this.media.CurrentState == MediaPlayerState.Playing; }
        }

        public double Volume
        {
            get
            {
                if (this.media == null)
                    return 0;
                if(this.IsMuted)
                    return 0;
                return this.media.Volume;
            }
            set
            {
                if (this.IsMuted)
                    this.IsMuted = false;
                this.media.Volume = value;
                base.NotifyPropertyChanged(nameof(this.VolumeIcon));
            }
        }

        public string VolumeIcon
        {
            get
            {
                if (this.Volume > 0.75)
                    return "\xE995";//Volume3
                else if (this.Volume > 0.50)
                    return "\xE994";//Volume2
                else if (this.Volume > 0.25)
                    return "\xE993";//Volume1
                else if (this.Volume == 0)
                    return "\xE74F";//Mute
                else
                    return "\xE992";//Volume0
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

        public string PlayPauseIcon
        {
            get
            {
                if (this.media == null)
                    return "";

                return this.IsPlaying ? "\xE769" : "\xE768";
            }
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
                /*
#if USE_PlaybackSession
                if ( (media.PlaybackSession.PlaybackState != MediaPlaybackState.Playing && media.PlaybackSession.PlaybackState != MediaPlaybackState.Paused) || string.IsNullOrWhiteSpace(this.TrackName))
#else
                if ((media.CurrentState != MediaPlayerState.Playing && media.CurrentState != MediaPlayerState.Paused) || string.IsNullOrWhiteSpace(this.TrackName))
#endif
                    return Visibility.Collapsed;
                    */
                return Visibility.Visible;
            }
        }

        public VKAudio CurrentTrack
        {
            get
            {
                if (this.Tracks.Count == 0)
                    return null;
                return this.Tracks[this.CurentTrackId];
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

        private void Media_MediaEnded(MediaPlayer sender, object args)
        {
            if(this.Repeat==2)
            {
                this.Position = TimeSpan.FromSeconds(0);
                this.media.Play();
                return;
            }

            if (this.CurentTrackId + 1 >= this.Tracks.Count)
            {
                if(this.Repeat ==1)
                {
                    this.PlayTrack(0);
                }
                return;
            }

            this.NextTrack();
        }

        /// <summary>
        /// Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUVCOnNewTrack(VKAudio audio)
        {
            this.systemControls.IsEnabled = this.Tracks.Count > 0;//this.CurentTrackId != -1;

            this.systemControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            this.systemControls.DisplayUpdater.MusicProperties.Title = audio == null ? "" : audio.title;
            this.systemControls.DisplayUpdater.MusicProperties.Artist = audio == null ? "" : audio.artist;
            //systemControls.DisplayUpdater.MusicProperties.AlbumArtist = "album artist";

            this.systemControls.IsPauseEnabled = this.systemControls.IsPlayEnabled = this.Tracks.Count > 0;//this.CurentTrackId != -1;
            this.systemControls.IsNextEnabled = this.CurentTrackId + 1 < this.Tracks.Count;
            this.systemControls.IsPreviousEnabled = this.CurentTrackId > 0;

            if (audio != null && !string.IsNullOrEmpty(audio.cover))
            {
                // Set the album art thumbnail.
                RandomAccessStreamReference r = RandomAccessStreamReference.CreateFromUri(new Uri(audio.cover));
                systemControls.DisplayUpdater.Thumbnail = r;
            }
            else if (systemControls.DisplayUpdater.Thumbnail != null)
            {
                systemControls.DisplayUpdater.Thumbnail = null;
            }

            systemControls.DisplayUpdater.Update();
        }

        private void Media_CurrentStateChanged(MediaPlayer sender, object args)
        {
            bool flag = true;
#if USE_PlaybackSession
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
#else
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Buffering:
                case MediaPlayerState.Opening:
                    systemControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    flag = false;
                    break;
                default:
                    break;
            }
#endif
            EventAggregator1.Instance.PublishEvent(new Library.Events.AudioPlayerStateChanged(sender.CurrentState));
            base.NotifyPropertyChanged(nameof(this.PlayPauseIcon));

           if (flag)
                base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));

            this.UpdateUVCOnNewTrack(this.CurentTrackId == -1 ? null : this.Tracks[this.CurentTrackId]);
        }

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
#if USE_PlaybackSession
                    return (int)this.media.PlaybackSession.NaturalDuration.TotalSeconds;
#else
                    return (int)this.media.NaturalDuration.TotalSeconds;
#endif
                return this.CurrentTrack.duration;
            }
        }

        private TimeSpan Duration
        {
            get
            {

                if (this.CurrentTrack == null)
                    return new TimeSpan();
                if (this.CurrentTrack.duration == 0)
#if USE_PlaybackSession
                    return this.media.PlaybackSession.NaturalDuration;
#else
                    return this.media.NaturalDuration;
#endif
                return TimeSpan.FromSeconds(this.CurrentTrack.duration);
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

        private TimeSpan Position
        {
            get
            {
                try
                {
                    if (this.media == null)
                        return new TimeSpan();
#if USE_PlaybackSession
                    return this.media.PlaybackSession.Position;
#else
                    return this.media.Position;
#endif
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
#if USE_PlaybackSession
                    media.PlaybackSession.Position = value;
#else
                    media.Position = value;
#endif
                }
                catch (Exception ex)
                {
                }
            }
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
                base.NotifyPropertyChanged(nameof(this.VolumeIcon));
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

        public void PlayPause()
        {
            this.InitMedia();

            base.NotifyPropertyChanged(nameof(this.RemainingSeconds));

            if (this.IsPlaying)
                this.media.Pause();
            else
            {
                if (this.Position.TotalSeconds == 0)
                    this.PlayTrack(0);
                else
                    this.media.Play();
            }
        }

        /// <summary>
        /// Играть следующий трек. После окончания трека или после нажатия на кнопку следующего трека.
        /// </summary>
        public void NextTrack()
        {
            if (this.CurentTrackId + 1 >= this.Tracks.Count)
                return;

            this.CurentTrackId++;
            if (this.Shuffle)
                this.PlayTrack(this.ShuffledIndexes[this.CurentTrackId]);
            else
                this.PlayTrack(this.CurentTrackId);
        }

        public void PrevTrack()
        {
            if (this.CurentTrackId == 0)
                return;

            this.CurentTrackId--;
            this.PlayTrack(this.CurentTrackId);
        }

        public void Stop()
        {
            this._id = string.Empty;
            this.Tracks.Clear();
            this.CurentTrackId = -1;

            media.CurrentStateChanged -= this.Media_CurrentStateChanged;

            media.Pause();
            
            EventAggregator1.Instance.PublishEvent(new Library.Events.AudioPlayerStateChanged(MediaPlayerState.Stopped));

            this._localTimer.Tick -= this._localTimer_Tick;
            this._localTimer.Stop();
            this._localTimer = null;


            
            Window.Current.VisibilityChanged -= this.Current_VisibilityChanged;

            this.systemControls.ButtonPressed -= this.SystemControls_ButtonPressed;

            this.systemControls.IsEnabled = false;

            this.media = null;
            this.systemControls = null;

            base.NotifyPropertyChanged(nameof(this.CurrentTrackVisibility));

            this.watcher.Removed -= this.Watcher_Removed;
            this.watcher = null;

            this._initialized = false;
        }

        private void Watcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (this.IsPlaying)
                this.PlayPause();
        }

        private void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible == true)
            {
                EventAggregator1.Instance.PublishEvent(new Library.Events.AudioPlayerStateChanged(this.media.CurrentState));
                base.NotifyPropertyChanged(nameof(this.PlayPauseIcon));
            }
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

        public MediaPlayerState PlaybackState
        {
            get
            {
                if (this.media == null)
                    return MediaPlayerState.Stopped;
                return this.media.CurrentState;
            }
        }

        private byte _repeat;

        /// <summary>
        /// 0 - нет повтора
        /// 1 - все
        /// 2 - один
        /// </summary>
        public byte Repeat
        {
            get { return this._repeat; }
            set
            {
                this._repeat = value;
                base.NotifyPropertyChanged(nameof(this.RepeatBackground));
                base.NotifyPropertyChanged(nameof(this.RepeatIcon));
                base.NotifyPropertyChanged(nameof(this.RepeatText));
            }
        }

        public string RepeatText
        {
            get
            {
                if (this.Repeat == 0)
                    return LocalizedStrings.GetString("Audio_RepeatNone");
                else if (this.Repeat == 1)
                    return LocalizedStrings.GetString("Audio_RepeatAll");
                return LocalizedStrings.GetString("Audio_RepeatOne");
            }
        }

        public Symbol RepeatIcon
        {
            get
            {
                if (this._repeat == 2)
                    return Symbol.RepeatOne;
                return Symbol.RepeatAll;
            }
        }

        private bool _shuffle;
        /// <summary>
        /// Случайный порядок
        /// </summary>
        public bool Shuffle
        {
            get { return this._shuffle; }
            set
            {
                this._shuffle = value;
                base.NotifyPropertyChanged(nameof(this.ShuffleBackground));
                base.NotifyPropertyChanged(nameof(this.ShuffleText));

                if (value == true && this.Tracks.Count>0)
                {
                    this.ShuffledIndexes = new List<int>();
                    for (int index = 0; index < this.Tracks.Count; ++index)
                        this.ShuffledIndexes.Add(index);
                    this.ShuffledIndexes.Shuffle();
                }
                else
                {
                    this.ShuffledIndexes = null;
                }
            }
        }

        public string ShuffleText
        {
            get { return LocalizedStrings.GetString(this.Shuffle ?  "Audio_ShuffleOn" : "Audio_ShuffleOff"); }
        }

        public void FetchArtwork()
        {
            this.Artwork = null;

            var track1 = this.CurrentTrack;
            if (track1 == null)
                return;

            if (!string.IsNullOrEmpty(track1.cover))
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    this.Artwork = track1.Cover;
                });
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

        public Visibility ShuffleBackground
        {
            get { return this.Shuffle.ToVisiblity(); }
        }

        public Visibility RepeatBackground
        {
            get { return (this.Repeat>0).ToVisiblity(); }
        }
    }
}
