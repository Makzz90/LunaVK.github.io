using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Library.Events
{
    public class CountersChanged
    {
        public OwnCounters Counts { get; set; }

        public CountersChanged()
        {
            this.Counts = new OwnCounters();
        }

        public CountersChanged(OwnCounters o)
        {
            this.Counts = o;
        }

        public class OwnCounters
        {
            public int friends { get; set; }

            public int messages { get; set; }

            public int photos { get; set; }

            public int videos { get; set; }

            public int groups { get; set; }

            public int notifications { get; set; }

            public int sdk { get; set; }

            public int app_requests { get; set; }
        }
    }
}
