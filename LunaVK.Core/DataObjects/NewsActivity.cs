using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.DataObjects
{
    public class NewsActivity
    {
        public string type { get; set; }
        
        public NewsActivityLikes likes { get; set; }

        public NewsActivityComment comment { get; set; }

        public class NewsActivityLikes
        {
            public List<int> user_ids { get; set; }

            public string text { get; set; }
        }

        public class NewsActivityComment
        {
            public int id { get; set; }

            public int from_id { get; set; }

            public int date { get; set; }

            public string text { get; set; }
        }
    }
}
