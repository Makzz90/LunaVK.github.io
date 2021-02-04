using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.DataObjects
{
    public class VKPinnedMessage
    {
        /// <summary>
        /// идентификатор сообщения.
        /// Содержит 0, если у текущего пользователя нет этого сообщения
        /// в истории (например, оно было отправлено в мультичат
        /// до того, как пользователя пригласили). 
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// время отправки сообщения в Unixtime.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// идентификатор отправителя.
        /// </summary>
        public int from_id { get; set; }

        /// <summary>
        /// текст сообщения.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// медиавложения сообщения (фотографии, ссылки и т.п.).
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// информация о местоположении
        /// </summary>
        public VKGeoInMsg geo { get; set; }

        /// <summary>
        /// массив пересланных сообщений (если есть)
        /// </summary>
        public List<VKMessage> fwd_messages { get; set; }
    }
}
