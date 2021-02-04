using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Library.Events
{
    public sealed class AudioTrackDownloadProgress
    {
        public string Id { get; set; }

        public float Progress { get; set; }
    }
}
