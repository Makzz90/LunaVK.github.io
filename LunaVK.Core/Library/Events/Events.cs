using Windows.Media.Playback;

namespace LunaVK.Core.Library.Events
{
    public sealed class AudioTrackDownloadProgress
    {
        public string Id { get; private set; }
        public double Progress { get; private set; }

        public AudioTrackDownloadProgress(string id, double progress)
        {
            this.Id = id;
            this.Progress = progress;
        }
    }

    public sealed class AudioPlayerStateChanged
    {
        public MediaPlaybackState PlayState { get; private set; }

        public AudioPlayerStateChanged(MediaPlaybackState state)
        {
            this.PlayState = state;
        }
    }
}
