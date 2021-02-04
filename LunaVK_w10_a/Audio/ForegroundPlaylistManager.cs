using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media;
using Windows.Foundation.Collections;
using System.Linq;
using Windows.UI.Xaml.Controls;
using BackgroundAudio;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif

using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using LunaVK.Core.Framework;

namespace LunaVK.Audio
{
    public class ForegroundPlaylistManager
    {
        public int CurrentTrack = -1;
        public EventHandler<int> TrackChanged;
        public List<AudioHeader> Tracks;
#if !WINDOWS_PHONE_APP
        private MediaElement Scenario1MediaElement;
        private SystemMediaTransportControls systemMediaControls;
#endif


        public AudioHeader CurrentAudioTrack
        {
            get
            {
                if (this.CurrentTrack < 0)
                    return null;
                return this.Tracks[this.CurrentTrack];
            }
        }

#if WINDOWS_PHONE_APP
        
        public EventHandler<MediaPlayerState> CurrentStateChanged;
        
        
#endif
        private static ForegroundPlaylistManager _instance;
        public static ForegroundPlaylistManager Instance
        {
            get
            {
                if (ForegroundPlaylistManager._instance == null)
                    ForegroundPlaylistManager._instance = new ForegroundPlaylistManager();

                return ForegroundPlaylistManager._instance;
            }
        }

        public ForegroundPlaylistManager()
        {
            this.Tracks = new List<AudioHeader>();
            
#if WINDOWS_PHONE_APP
            
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
#else
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            systemMediaControls.IsEnabled = false;
            systemMediaControls.ButtonPressed += SystemMediaControls_ButtonPressed;
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;

            

            ForegroundPlaylistManager_Loaded();
            
#endif

        }
#if !WINDOWS_PHONE_APP
        async void ForegroundPlaylistManager_Loaded()
        {
            this.Scenario1MediaElement = new MediaElement() { AreTransportControlsEnabled = true, AudioCategory = Windows.UI.Xaml.Media.AudioCategory.BackgroundCapableMedia };
            Scenario1MediaElement.Visibility = Visibility.Collapsed;
            (Window.Current.Content as Framework.CustomFrame).GridBack./*OverlayGrid.*/Children.Add(this.Scenario1MediaElement);
            this.Scenario1MediaElement.CurrentStateChanged += Scenario1MediaElement_CurrentStateChanged;
            this.Scenario1MediaElement.MediaEnded += Scenario1MediaElement_MediaEnded;
            this.Scenario1MediaElement.MediaFailed += Scenario1MediaElement_MediaFailed;            
        }
#endif





#if !WINDOWS_PHONE_APP


        void Scenario1MediaElement_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Scenario1MediaElement_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MediaElement element = sender as MediaElement;
            if (element.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (element.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Stopped)
            {
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            }
            else if (element.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Paused)
            {
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }

        void Scenario1MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.PlayNext();
        }
#endif
#if !WINDOWS_PHONE_APP
        /// <summary>
        /// Handler for the system transport controls button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Scenario1MediaElement.Play();
                    });
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Scenario1MediaElement.Pause();
                    });
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Scenario1MediaElement.Stop();
                    });
                    break;
                case SystemMediaTransportControlsButton.Next:
                    {
                        this.PlayNext();
                        break;
                    }
                case SystemMediaTransportControlsButton.Previous:
                    {
                        this.PlayPrev();
                        break;
                    }
            }
        }
#endif
#if WINDOWS_PHONE_APP
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (this.CurrentStateChanged != null)
            {
                Execute.ExecuteOnUIThread(() => { this.CurrentStateChanged(this, sender.CurrentState); });
            }
        }
        
        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            for (int i = 0; i < e.Data.Count; i++)
            {
                string key = e.Data.Keys.ElementAt(i);
                BackgroundAudio.Constants c = (BackgroundAudio.Constants)Enum.Parse(typeof(BackgroundAudio.Constants), key);
                switch (c)
                {
                    case BackgroundAudio.Constants.SkipNext:
                        {
                            
                            break;
                        }
                    case BackgroundAudio.Constants.SkipPrevious:
                        {
                            
                            break;
                        }
                    case BackgroundAudio.Constants.StartPlayback:
                        {
                            int index = int.Parse(e.Data.Values.ElementAt(i) as string);
                            this.CurrentTrack = index;

                            if (this.TrackChanged != null)
                            {
                                Execute.ExecuteOnUIThread(() => { this.TrackChanged(this, index); });
                            }

                            
                            break;
                        }
                }
            }
        }

        public void ClearPlayList()
        {
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.ClearPlayList.ToString(), "");
            BackgroundMediaPlayer.SendMessageToBackground(message);

            this.Tracks.Clear();
        }
#endif
        public void AddTrack(AudioHeader audio)
        {
#if WINDOWS_PHONE_APP
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.AddTrack.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(audio));
            BackgroundMediaPlayer.SendMessageToBackground(message);
#endif
            this.Tracks.Add(audio);
#if !WINDOWS_PHONE_APP
            systemMediaControls.IsEnabled = true;
#endif
        }

        public void PlayNext()
        {
            this.PlayTrack(this.CurrentTrack + 1);
        }

        public void PlayPrev()
        {
            this.PlayTrack(this.CurrentTrack - 1);
        }

        public async void PlayTrack(int number)
        {

            if (number >= this.Tracks.Count || number < 0)
            {
                return;
            }

            this.CurrentTrack = number;



#if WINDOWS_PHONE_APP
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.StartPlayback.ToString(), number.ToString());
            BackgroundMediaPlayer.SendMessageToBackground(message);
#else



            AudioHeader audio = this.Tracks[number];
            /*
            //@"Assets\Countries.xml";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file2 = await InstallationFolder.GetFileAsync(audio.url);

            IRandomAccessStream stream = await file2.OpenAsync(FileAccessMode.Read);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Scenario1MediaElement.SetSource(stream, file2.ContentType);
            });
            */

            //StorageFile file2 = await StorageFile.GetFileFromPathAsync(audio.url);
            //IRandomAccessStream stream = await file2.OpenAsync(FileAccessMode.Read);
            //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    Scenario1MediaElement.SetSource(stream, file2.ContentType);
            //});

            System.Uri manifestUri = new Uri(audio.url);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Scenario1MediaElement.Source = manifestUri;
                Scenario1MediaElement.Play();
            });


            systemMediaControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemMediaControls.DisplayUpdater.MusicProperties.Title = audio.title;
            systemMediaControls.DisplayUpdater.MusicProperties.Artist = audio.artist;
            systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            //systemMediaControls.DisplayUpdater.MusicProperties.AlbumArtist = "album artist";

            systemMediaControls.IsNextEnabled = number + 1 < this.Tracks.Count;
            systemMediaControls.IsPreviousEnabled = number > 0;
#endif
        }
    }
}



#if !WINDOWS_PHONE_APP
namespace BackgroundAudio
{
    public sealed class AudioHeader
    {
        /// <summary>
        /// Исполнитель
        /// </summary>
        public string artist { get; set; }

        /// <summary>
        /// название композиции
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// длительность аудиозаписи в секундах
        /// </summary>
        public int duration { get; set; }

        /// <summary>
        /// Ссылка на MP3-файл
        /// </summary>
        public string url { get; set; }

        public string cover { get; set; }
    }
}
#endif