using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation.Collections;
using LunaVK.Audio;
using LunaVK.Core.DataObjects;
using System.Linq;
using LunaVK.Core.Framework;

#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
using BackgroundAudio;
#endif
#if WINDOWS_UWP
using Windows.Media.Playback;
#endif
using Windows.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace LunaVK.Audio
{
    public class ForegroundPlaylistHelper
    {
        public event EventHandler<int> TrackChanged;
        public event EventHandler<MediaPlayerState> StateChanged;
//        private SystemMediaTransportControls systemMediaControls;
        public Action<AudioTrackDownloadProgress> AudioDownloadProgressEventHandler;

        public MediaPlayerState PlayerState
        {
            get
            {
                return BackgroundMediaPlayer.Current.CurrentState;
            }
        }

        public ForegroundPlaylistHelper()
        {
#if WINDOWS_PHONE_APP
            if (BackgroundMediaPlayer.Current != null)
            {

            }

            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
#endif


/*
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            systemMediaControls.ButtonPressed += SystemMediaControls_ButtonPressed;
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;

            systemMediaControls.IsNextEnabled = true;


            systemMediaControls.DisplayUpdater.Type = MediaPlaybackType.Music;
            systemMediaControls.DisplayUpdater.MusicProperties.Title = "title";
            systemMediaControls.DisplayUpdater.MusicProperties.Artist = "artist";
            //systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;

            Scenario1MediaElement.CurrentStateChanged += Scenario1MediaElement_CurrentStateChanged;*/
        }
        /*
        ~ForegroundPlaylistHelper()
        {
            Scenario1MediaElement.CurrentStateChanged -= Scenario1MediaElement_CurrentStateChanged;
        }

        void Scenario1MediaElement_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var element = sender as MediaElement;
            System.Diagnostics.Debug.WriteLine(element.CurrentState.ToString());
            switch (element.CurrentState)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    {
                        systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        break;
                    }
                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                    {
                        systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        break;
                    }
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                    {
                        systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                    }
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    {
                        systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                    }
            }
        }

        public async void Play(byte number)
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\Mp3");
            Windows.Storage.StorageFile file = await folder.GetFileAsync("Clubbed.mp3");//("bb"+number+".mp3");
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            Scenario1MediaElement.SetSource(stream, file.ContentType);
            Scenario1MediaElement.Play();
        }
        */
        private Framework.CustomFrame CFrame
        {
            get
            {
                return Window.Current.Content as Framework.CustomFrame;
            }
        }

#if !WINDOWS_PHONE_APP
        private MediaElement Scenario1MediaElement
        {
            get { return this.CFrame.MusicPlayer; }
        }
#endif
        /*
        /// <summary>
        /// Handler for the system transport controls button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    //{
                    //    Scenario2MediaElement.Play();
                    //});
                    Execute.ExecuteOnUIThread(() => { Scenario1MediaElement.Play(); });

                    break;

                case SystemMediaTransportControlsButton.Pause:
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    //{
                    //    Scenario2MediaElement.Pause();
                    //});
                    Execute.ExecuteOnUIThread(() => { Scenario1MediaElement.Pause(); });
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    //{
                    //    Scenario2MediaElement.Stop();
                    //});
                    break;
                case SystemMediaTransportControlsButton.Next:
                    {
                        Execute.ExecuteOnUIThread(() => { Scenario1MediaElement.Play(); });
                        break;
                    }
                default:
                    break;
            }
        }
        */
#if WINDOWS_PHONE_APP
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (this.StateChanged != null)
                this.StateChanged(sender, sender.CurrentState);
        }

        
        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            for (int i = 0; i < e.Data.Count; i++)
            {
                string key = e.Data.Keys.ElementAt(i);
                if (key == null)
                    continue;
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
                            PlaylistManager.CurentTrackId = index;
                            
                            if (this.TrackChanged != null)
                            {
                                Execute.ExecuteOnUIThread(() => { this.TrackChanged(this, index); });
                            }
                            
                            break;
                        }
                    case Constants.Progress:
                        {
                            if(this.AudioDownloadProgressEventHandler!=null)
                            {
                                string temp = e.Data.Values.ElementAt(i) as string;
                                string[] splitted = temp.Split('_');
                                double p = double.Parse(splitted[2]);
                                this.AudioDownloadProgressEventHandler(new AudioTrackDownloadProgress(splitted[0] + '_' + splitted[1], p));
                            }
                            break;
                        }
                }
            }
        }
#endif
        public void PlayNext()
        {
            this.PlayTrack(PlaylistManager.CurentTrackId + 1);
        }

        public void PlayPrev()
        {
            this.PlayTrack(PlaylistManager.CurentTrackId - 1);
        }
        
        public void Pause()
        {
#if WINDOWS_PHONE_APP
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.Pause.ToString(), "");
            BackgroundMediaPlayer.SendMessageToBackground(message);
#endif
        }

        public void Continue()
        {
#if WINDOWS_PHONE_APP
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.Continue.ToString(), "");
            BackgroundMediaPlayer.SendMessageToBackground(message);
#endif
        }

        public void PlayTrack(int number)
        {
            if (number >= PlaylistManager.GetCurrentTracksCount())
            {
                return;
            }

            PlaylistManager.CurentTrackId = number;


#if WINDOWS_PHONE_APP
            ValueSet message = new ValueSet();
            message.Add(BackgroundAudio.Constants.StartPlayback.ToString(), number.ToString());
            BackgroundMediaPlayer.SendMessageToBackground(message);
#else
            ForegroundPlaylistManager.Instance.PlayTrack(number);
#endif
        }

        public VKAudio CurrentTrack
        {
            get
            {
                return PlaylistManager.GetCurrentTrack(PlaylistManager.CurentTrackId);
            }
        }
    }
}
