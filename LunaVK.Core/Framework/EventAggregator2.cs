using System;
using LunaVK.Core.DataObjects;
using Windows.Media;
using Windows.Media.Playback;

namespace LunaVK.Core.Framework
{
    public class EventAggregator
    {
        private static EventAggregator _instance;
        public static EventAggregator Instance
        {
            get
            {
                if (EventAggregator._instance == null)
                    EventAggregator._instance = new EventAggregator();
                return EventAggregator._instance;
            }
        }

        public Action<CountersArgs> CountersEventHandler;
        public void PublishCounters(CountersArgs args)
        {
            this.CountersEventHandler?.Invoke(args);
        }
        
        public Action<string, double> DownloadProgressEventHandler;
        public void PublishDownloadProgress(string id, double progress)
        {
            this.DownloadProgressEventHandler?.Invoke(id, progress);
        }

        /*
        /// <summary>
        /// Указывает состояние воспроизведения объекта MediaPlaybackSession
        /// </summary>
        public Action<MediaPlaybackState> AudioPlayerStateChangedEventHandler;
        public void PublishAudioPlayerStateChanged(MediaPlaybackState state)
        {
            this.AudioPlayerStateChangedEventHandler?.Invoke(state);
        }
        */
/*
        public Action<MediaPlaybackStatus> AudioPlayerStateChangedEventHandler;
        public void PublishAudioPlayerStateChanged(MediaPlaybackStatus state)
        {
            this.AudioPlayerStateChangedEventHandler?.Invoke(state);
        }
*/
        public Action<string> ProfileStatusChangedEventHandler;
        public void PublishProfileStatusChangedEvent(string status)
        {
            this.ProfileStatusChangedEventHandler?.Invoke(status);
        }

        public Action<string> ProfileNameChangedEventHandler;
        public void PublishProfileNameChangedEvent(string fullName)
        {
            this.ProfileNameChangedEventHandler?.Invoke(fullName);
        }

        public Action<string> ProfileAvatarChangedEventHandler;
        public void PublishProfileAvatarChangedEvent(string link)
        {
            this.ProfileAvatarChangedEventHandler?.Invoke(link);
        }
    }
}
