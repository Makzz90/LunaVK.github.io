using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// информация из блока ссылок сообщества
    /// </summary>
    public class VKGroupLink
    {
        /// <summary>
        /// идентификатор ссылки
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// название ссылки
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// описание ссылки
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// URL изображения-превью шириной 50px
        /// </summary>
        public string photo_50 { get; set; }

        /// <summary>
        /// URL изображения-превью шириной 100px
        /// </summary>
        public string photo_100 { get; set; }



        [JsonConverter(typeof(VKBooleanConverter))]
        public bool edit_title { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool image_processing { get; set; }
    }
}
