using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Library.Events
{
    public class DownloadProgressedEvent
    {
        public double Progress;

        public string Id;

        public DownloadProgressedEvent(double progress, string id)
        {
            this.Progress = progress;
            this.Id = id;
        }
    }
}
