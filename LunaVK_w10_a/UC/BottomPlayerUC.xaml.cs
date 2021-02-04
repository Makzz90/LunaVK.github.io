using LunaVK.Core.Framework;
using LunaVK.Core.ViewModels;
using LunaVK.Library.Events;
using LunaVK.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
    public sealed partial class BottomPlayerUC : UserControl, ISubscriber<AudioPlayerStateChanged>
    {
        public BottomPlayerUC()
        {
            this.InitializeComponent();

            base.DataContext = AudioPlayerViewModel2.Instance;

            base.Loaded += BottomPlayerUC_Loaded;
            base.Unloaded += BottomPlayerUC_Unloaded;
        }

        private void BottomPlayerUC_Unloaded(object sender, RoutedEventArgs e)
        {
            EventAggregator1.Instance.UnSubsribeEvent(this);
        }

        private void BottomPlayerUC_Loaded(object sender, RoutedEventArgs e)
        {
            EventAggregator1.Instance.SubsribeEvent(this);
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.PlayPause();
        }

        private void PreviousTrackButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.PrevTrack();
        }

        private void NextTrackButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.NextTrack();
        }

        private void AudioMuteButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.IsMuted = !AudioPlayerViewModel2.Instance.IsMuted;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.Stop();
            this.MoreFlyout.Hide();
        }

        private void CloseButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.Stop();
            this.MoreFlyout.Hide();
        }

        public void OnEventHandler(AudioPlayerStateChanged message)
        {
            this._bufferingRing.IsActive = message.PlayState == Windows.Media.Playback.MediaPlayerState.Buffering || message.PlayState == Windows.Media.Playback.MediaPlayerState.Opening;
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayerViewModel2.Instance.Shuffle = !AudioPlayerViewModel2.Instance.Shuffle;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            byte repeat = AudioPlayerViewModel2.Instance.Repeat;
            if(repeat==0)
                AudioPlayerViewModel2.Instance.Repeat = 1;
            else if(repeat==1)
                AudioPlayerViewModel2.Instance.Repeat = 2;
            else if (repeat == 2)
                AudioPlayerViewModel2.Instance.Repeat = 0;
        }
    }
}
