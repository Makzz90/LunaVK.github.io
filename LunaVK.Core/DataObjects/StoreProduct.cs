using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LunaVK.Core.Json;

namespace LunaVK.Core.DataObjects
{
    public class StoreProduct
    {
        public int id { get; set; }

        public string type { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool purchased { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool active { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool promoted { get; set; }

        public int purchase_date { get; set; }

        public string title { get; set; }

        public string base_url { get; set; }

        //public StoreStickers stickers { get; set; }
        public List<VKSticker> stickers { get; set; }

        //previews

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool has_animation { get; set; }
    }
}
