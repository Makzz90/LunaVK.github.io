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

using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Framework;
using Windows.Media.Playback;
using LunaVK.ViewModels;
using Windows.Media;
using LunaVK.Library.Events;

namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachAudioUC : UserControl, ISubscriber<AudioPlayerStateChanged>
    {
        public AttachAudioUC()
        {
            this.InitializeComponent();
            this.Loaded += AttachAudioUC_Loaded;
            this.Unloaded += AttachAudioUC_Unloaded;
        }
        
        private void AttachAudioUC_Loaded(object sender, RoutedEventArgs e)
        {
            //EventAggregator.Instance.AudioPlayerStateChangedEventHandler += this.AudioPlayerStateChanged;
            EventAggregator.Instance.DownloadProgressEventHandler += this.DownloadProgressChanged;
            //this.AudioPlayerStateChanged(AudioPlayerViewModel2.Instance.PlaybackState);


            EventAggregator1.Instance.SubsribeEvent(this);
            this.OnEventHandler(new AudioPlayerStateChanged(AudioPlayerViewModel2.Instance.PlaybackState));
        }

        private void AttachAudioUC_Unloaded(object sender, RoutedEventArgs e)
        {
            EventAggregator1.Instance.UnSubsribeEvent(this);
            //EventAggregator.Instance.AudioPlayerStateChangedEventHandler -= this.AudioPlayerStateChanged;
            EventAggregator.Instance.DownloadProgressEventHandler -= this.DownloadProgressChanged;
        }

        public AttachAudioUC(VKAudio a):this()
        {
            this.DataContext = a;
        }

        private VKAudio VM
        {
            get { return base.DataContext as VKAudio; }
        }

        public void OnEventHandler(AudioPlayerStateChanged message)
        {
            if (this.VM == null || AudioPlayerViewModel2.Instance.CurrentTrack == null)//todo: не должно быть нулом
            {
                //VisualStateManager.GoToState(this, "Default", false);
                return;
            }

            MediaPlayerState state = message.PlayState;

            if (AudioPlayerViewModel2.Instance.CurrentTrack.ToString() == this.VM.ToString())
            {
                this._playPauseIcon.Glyph = (state == MediaPlayerState.Playing || state == MediaPlayerState.Buffering) ? "\xE769" : "\xEDDA";
            }
            else
            {
                this._playPauseIcon.Glyph = "\xEDDA";
            }
            
            this.VM.UpdateUI();
        }
        /*
        private void AudioPlayerStateChanged(MediaPlaybackStatus state)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if( AudioPlayerViewModel2.Instance.CurrentTrack == this.VM)
                {
                    this._playPauseIcon.Glyph = (state == MediaPlaybackStatus.Playing || state == MediaPlaybackStatus.Changing) ? "\xE769" : "\xEDDA";
                }
                else
                {
                    this._playPauseIcon.Glyph = "\xEDDA";
                }
                this.VM.UpdateUI();
            });
        }
        */
        private void DownloadProgressChanged(string id, double progress)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if (id == this.VM.ToString())
                {
                    this._progress.Progress = progress;
                }
            });
        }

        RoutedEventHandler _SecondaryClick;
        public event RoutedEventHandler SecondaryClick
        {
            add { this._SecondaryClick += value; }
            remove { this._SecondaryClick -= value; }
        }

        RoutedEventHandler _PrimaryClick;
        public event RoutedEventHandler PrimaryClick
        {
            add { this._PrimaryClick += value; }
            remove { this._PrimaryClick -= value; }
        }

        private void Cover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._PrimaryClick != null)
            {
                e.Handled = true;
                this._PrimaryClick.Invoke(sender, e);
            }
            else
            {
                if (AudioPlayerViewModel2.Instance.CurrentTrack == this.VM)
                    AudioPlayerViewModel2.Instance.PlayPause();
                else
                {
                    List<VKAudio> list = new List<VKAudio>();

                    if(base.Parent is FrameworkElement parent)
                    {
                        if(parent.DataContext is VKWallPost post)
                        {
                            list = post.attachments.Where((a) => a.type == Core.Enums.VKAttachmentType.Audio).Select((a) => a.audio).ToList();
                        }
                        else if (parent.DataContext is VKNewsfeedPost news)
                        {
                            list = news.attachments.Where((a) => a.type == Core.Enums.VKAttachmentType.Audio).Select((a) => a.audio).ToList();
                        }
                    }
                    
                    if(list.Count==0)
                        list.Add(this.VM);
                    
                    AudioPlayerViewModel2.Instance.FillTracks(list, this.VM.ToString() );
                    AudioPlayerViewModel2.Instance.PlayTrack(this.VM);
                }
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.NewSize.Width>400)
            {
                this._sp.Orientation = Orientation.Horizontal;
                this._tb.Margin = new Thickness(10,0,0,0);
            }
            else
            {
                this._sp.Orientation = Orientation.Vertical;
                this._tb.Margin = new Thickness();
            }
        }
    }
}
