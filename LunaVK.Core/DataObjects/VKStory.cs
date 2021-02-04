using System;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Объект, описывающий историю
    /// https://vk.com/dev/objects/story
    /// </summary>
    public class VKStory : IBinarySerializable
    {
        /// <summary>
        /// идентификатор истории. 
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// идентификатор владельца истории. 
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// дата добавления в Unixtime. 
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// true, если срок хранения истории истёк.
        ///В этом случае объект истории содержит только поля id, owner_id, date, is_expired. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_expired { get; set; }

        /// <summary>
        /// rue, если история удалена или не существует.
        ///В этом случае объект истории содержит только поля id, owner_id, is_deleted. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_deleted { get; set; }

        /// <summary>
        /// информация о том, может ли пользователь просмотреть историю (0 — нет, 1 — да). 
        /// Если can_see = 0, объект истории содержит только поля id, owner_id, date, can_see, type. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_see { get; set; }

        /// <summary>
        /// 1, если история просмотрена текущим пользователем. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool seen { get; set; }

        /// <summary>
        /// тип истории. Возможные значения:
        /// photo — фотография;
        /// video — видеозапись. 
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// (для type = photo). Фотография из истории
        /// </summary>
        public VKPhoto photo { get; set; }

        /// <summary>
        /// для type = video). Видео из истории
        /// 
        /// Содержит дополнительные поля first_frame_800, first_frame_320, first_frame_160,
        /// first_frame_130 (string), содержащие URL изображения-первого кадра видео
        /// с соответствующей шириной, и поле is_private (integer, [0,1]) —
        /// информация о том, приватная ли история (0 — нет, 1 — да). 
        /// </summary>
        public VKVideoBase video { get; set; }

        public StoryLink link { get; set; }

        /// <summary>
        /// идентификатор пользователя, загрузившего историю, ответом на которую является текущая. 
        /// </summary>
        public int parent_story_owner_id { get; set; }

        /// <summary>
        /// идентификатор истории, ответом на которую является текущая. 
        /// </summary>
        public int parent_story_id { get; set; }

        /// <summary>
        /// объект родительской истории
        /// </summary>
        public VKStory parent_story { get; set; }
        //replies 

        /// <summary>
        /// может ли пользователь ответить на историю 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_reply { get; set; }

        /// <summary>
        /// может ли пользователь расшарить историю
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_share { get; set; }

        /// <summary>
        /// может ли пользователь комментировать историю
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_comment { get; set; }

        //clickable_stickers
        //narrative_id

        /// <summary>
        /// число просмотров
        /// </summary>
        public int views { get; set; }

        /// <summary>
        /// ключ доступа для приватного объекта
        /// </summary>
        public string access_key { get; set; }




        public bool is_one_time { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_hide { get; set; }

        //clickable_stickers
        /*
         * "clickable_stickers": {
"original_height": 2208,
"original_width": 1242,
"clickable_stickers": [{
"id": 1,
"type": "link",
"clickable_area": [{
"x": 361,
"y": 1379
}, {
"x": 921,
"y": 1379
}, {
"x": 921,
"y": 1507
}, {
"x": 361,
"y": 1507
}],
"link_object": {
"url": "https://vk.com/memories",
"title": "https://vk.com/memories",
"caption": "vk.com",
"description": ""
},
"tooltip_text": "Перейти"
}]
},
         * */

        public bool is_restricted { get; set; }
        public bool no_sound { get; set; }
        public bool need_mute { get; set; }
        public bool mute_reply { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_ask { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_ask_anonymous { get; set; }

        public bool is_owner_pinned { get; set; }


        public class StoryLink
        {
            /// <summary>
            /// текст ссылки
            /// </summary>
            public string text { get; set; }

            /// <summary>
            /// URL для перехода. 
            /// </summary>
            public string url { get; set; }
        }

#region VM
        public VKBaseDataForGroupOrUser Owner { get; set; }
#endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.Write(this.date);
            writer.Write(this.is_expired);
            
            if (this.owner_id > 0)
                writer.Write(this.Owner as VKUser);
            else
                writer.Write(this.Owner as VKGroup);

            writer.Write(this.photo);
            writer.Write(this.video);

        }

        public void Read(BinaryReader reader)
        {
            this.id = reader.ReadInt32();
            this.owner_id = reader.ReadInt32();
            this.date = reader.ReadDateTime();
            this.is_expired = reader.ReadBoolean();

            if (this.owner_id > 0)
                this.Owner = reader.ReadGeneric<VKUser>();
            else
                this.Owner = reader.ReadGeneric<VKGroup>();

            this.photo = reader.ReadGeneric<VKPhoto>();
            this.video = reader.ReadGeneric<VKVideoBase>();
        }
#endregion
    }
}
