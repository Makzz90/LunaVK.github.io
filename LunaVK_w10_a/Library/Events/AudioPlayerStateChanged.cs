using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace LunaVK.Library.Events
{
    public class AudioPlayerStateChanged
    {
        public MediaPlayerState PlayState { get; private set; }

        public AudioPlayerStateChanged(MediaPlayerState state)
        {
            this.PlayState = state;
        }
    }
}
