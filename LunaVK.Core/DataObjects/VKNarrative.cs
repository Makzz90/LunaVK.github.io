using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Сюжет
    /// </summary>
    public class VKNarrative
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public int views { get; set; }
        public bool can_see { get; set; }
        public bool is_delete { get; set; }
        public bool is_favorite { get; set; }
        public bool can_delete { get; set; }
        public CoverStory cover_story { get; set; }
        public List<VKStory> stories { get; set; }

        public class CoverStory
        {
            public int id { get; set; }
            public int owner_id { get; set; }
            public int date { get; set; }
            public int expires_at { get; set; }
            public int narrative_id { get; set; }
            public int narrative_owner_id { get; set; }
            public NarrativeInfo narrative_info { get; set; }
            public int can_see { get; set; }
            public string type { get; set; }
            public VKPhoto photo { get; set; }
            public int can_share { get; set; }
            public int can_comment { get; set; }
            public int can_reply { get; set; }
            public int can_hide { get; set; }
            public Link link { get; set; }
            public Replies replies { get; set; }
            public string access_key { get; set; }
        }

        public class NarrativeInfo
        {
            public string author { get; set; }
            public string title { get; set; }
            public int views { get; set; }
        }

        public class Link
        {
            public string text { get; set; }
            public string url { get; set; }
        }

        public class Replies
        {
            public int count { get; set; }
        }
    }
}
