using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class LongPollServerResponseExtended
    {
        public LongPollServerResponse s { get; set; }
        public CountersArgs c { get; set; }
        public int time { get; set; }

        public class FriendsOnline
        {
            public List<int> online_mobile { get; set; }
            public List<int> online { get; set; }
        }
    }
}
