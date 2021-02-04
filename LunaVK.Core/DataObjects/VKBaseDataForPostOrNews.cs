using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;

namespace LunaVK.Core.DataObjects
{
    public class VKBaseDataForPostOrNews
    {
        /// <summary>
        /// время публикации новости в формате unixtime;
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public VKNewsfeedPostType post_type { get; set; }

        /// <summary>
        /// находится в записях со стен и содержит текст записи; 
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// содержит 1, если текущий пользователь может редактировать запись; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_edit { get; set; }

        /// <summary>
        /// возвращается, если пользователь может удалить новость, всегда содержит 1; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_delete { get; set; }

        /// <summary>
        /// Информация о комментариях к записи.
        /// </summary>
        public VKComments comments { get; set; }

        /// <summary>
        /// Информация об отметках Мне нравится.
        /// </summary>
        public VKLikes likes { get; set; }

        /// <summary>
        /// Информация о репостах.
        /// </summary> 
        public VKReposts reposts { get; set; }

        /// <summary>
        /// Список вложений.
        /// </summary>
        public List<VKAttachment> attachments { get; set; }

        /// <summary>
        /// информация о местоположении
        /// </summary>
        public VKGeo geo { get; set; }
        
        /// <summary>
        /// Количество просмотров новости
        /// (нет в документации)
        /// </summary>
        public VKViews views { get; set; }
        
        public VKBaseDataForGroupOrUser Owner { get; set; }

        public VKBaseDataForGroupOrUser Signer { get; set; }

        //public virtual string Title { get; set; }

        public virtual int OwnerId { get; set; }

        public virtual uint PostId { get; set; }

        public virtual bool IsPinned { get; set; }

        public Action IgnoreNewsfeedItemCallback;
        public Action HideSourceItemsCallback;
    }
}
