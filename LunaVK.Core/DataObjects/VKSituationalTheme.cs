using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class VKSituationalTheme
    {
        public uint id { get; set; }
        public int owner_id { get; set; }
        public string link { get; set; }
        public int date { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public long date_start { get; set; }
        public CoverPhoto cover_photo { get; set; }
        public CoverPhoto squared_cover_photo { get; set; }
        public bool is_anonymous { get; set; }
        public int publications_count { get; set; }
        public uint views_count { get; set; }
        public int friends_posted_count { get; set; }
        public List<int> friends_posted { get; set; }
        public bool can_edit { get; set; }
        public bool can_delete { get; set; }

        public class CoverPhoto
        {
            public int album_id { get; set; }
            public int date { get; set; }
            public uint id { get; set; }
            public int owner_id { get; set; }
            public bool has_tags { get; set; }

            [JsonConverter(typeof(SizesToDictionaryConverter))]
            public Dictionary<char, VKImageWithSize> sizes { get; set; }

            public string text { get; set; }
            public int user_id { get; set; }
        }
    }
}
