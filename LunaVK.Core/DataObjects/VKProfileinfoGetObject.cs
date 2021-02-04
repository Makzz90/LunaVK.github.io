using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LunaVK.Core.DataObjects
{
    public class VKProfileinfoGetObject
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        
        [JsonProperty("maiden_name")]
        public string MaidenName { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("sex")]
        public int Sex { get; set; }

        [JsonProperty("relation")]
        public int Relation { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
