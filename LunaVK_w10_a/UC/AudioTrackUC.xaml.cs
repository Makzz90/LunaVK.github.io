using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Library.Events;
using LunaVK.ViewModels;
using System;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace LunaVK.UC
{
    public sealed partial class AudioTrackUC : UserControl, ISubscriber<AudioPlayerStateChanged>
    {
        public AudioTrackUC()
        {
            this.InitializeComponent();
            
            this.Loaded += this.AudioTrackUC_Loaded;
            this.Unloaded += this.AudioTrackUC_Unloaded;

            VisualStateManager.GoToState(this, "Default", true);
        }

        private void AudioTrackUC_Loaded(object sender, RoutedEventArgs e)
        {
            //EventAggregator.Instance.AudioPlayerStateChangedEventHandler += this.AudioPlayerStateChanged;
            //this.AudioPlayerStateChanged(AudioPlayerViewModel.Instance.PlaybackState);

            EventAggregator1.Instance.SubsribeEvent(this);
            this.OnEventHandler(new AudioPlayerStateChanged(AudioPlayerViewModel2.Instance.PlaybackState));
        }

        private void AudioTrackUC_Unloaded(object sender, RoutedEventArgs e)
        {
            //EventAggregator.Instance.AudioPlayerStateChangedEventHandler -= this.AudioPlayerStateChanged;
            EventAggregator1.Instance.UnSubsribeEvent(this);
        }

        private VKAudio VM
        {
            get { return base.DataContext as VKAudio; }
        }

        public void OnEventHandler(AudioPlayerStateChanged message)
        {
            if (this.VM == null || AudioPlayerViewModel2.Instance.CurrentTrack == null)//todo: не должно быть нулом
            {
                VisualStateManager.GoToState(this, "Default", false);
                return;
            }

            MediaPlayerState state = message.PlayState;

            if (AudioPlayerViewModel2.Instance.CurrentTrack.ToString() == this.VM.ToString())
            {
                VisualStateManager.GoToState(this, "Selected", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Default", true);
            }

            this.VM.UpdateUI();
        }
        /*
        private void AudioPlayerStateChanged(MediaPlaybackStatus state)
        {
            Execute.ExecuteOnUIThread(() =>
            {
                if (AudioPlayerViewModel.Instance.CurrentTrack == this.VM)
                {
                    this._brdPlayPause.Visibility = Visibility.Visible;
                    this._playPauseIcon.Glyph = state == MediaPlaybackStatus.Playing ? "\xE769" : "\xEDDA";
                }
                else
                {
                    this._brdPlayPause.Visibility = Visibility.Collapsed;
                    this._playPauseIcon.Glyph = "\xEDDA";
                }
                this.VM.UpdateUI();
            });
        }
        */
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
            if(this._PrimaryClick!=null)
            {
                e.Handled = true;
                this._PrimaryClick.Invoke(sender, e);
            }
            
        }

        private void Back_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._SecondaryClick != null)
            {
                e.Handled = true;
                this._SecondaryClick.Invoke(sender, e);
            }
        }
    }
}
