using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    public class ProfileInfo
    {
        public string photo_max { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string screen_name { get; set; }

        public int sex { get; set; }

        public int relation { get; set; }

        public VKUser relation_partner { get; set; }

        public int relation_pending { get; set; }

        public List<VKUser> relation_requests { get; set; }

        public string bdate { get; set; }

        public int bdate_visibility { get; set; }

        public VKCity city { get; set; }

        public VKCountry country { get; set; }

        public string home_town { get; set; }

        public NameChangeRequest name_request { get; set; }

        public string phone { get; set; }

        public class NameChangeRequest
        {
            public long id { get; set; }

            public string status { get; set; }

            public string first_name { get; set; }

            public string last_name { get; set; }

            public string lang { get; set; }
        }
    }
}
