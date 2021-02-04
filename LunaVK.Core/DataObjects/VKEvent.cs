using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKEvent : IBinarySerializable
    {
        /// <summary>
        /// идентификатор встречи
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// время начала встречи в Unixtime
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime time { get; set; }

        /// <summary>
        /// идёт ли текущий пользователь на встречу 
        /// 1 - точно идёт; 
        /// 2 - возможно пойдёт; 
        /// 3 - не идёт. 
        /// </summary>
        public uint member_status { get; set; }

        /// <summary>
        /// добавлена ли встреча в закладки
        /// </summary>
        public bool is_favorite { get; set; }

        /// <summary>
        /// место проведения встречи
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// текст для отображения сниппета
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// текст на кнопке сниппета
        /// </summary>
        public string button_text { get; set; }

        /// <summary>
        /// список идентификаторов друзей, которые также идут на мероприятие
        /// </summary>
        public List<int> friends { get; set; }

#region VM
        public VKBaseDataForGroupOrUser Owner { get; set; }

        public List<VKUser> Friends { get; set; }
#endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.time);
            writer.Write(this.member_status);
            writer.Write(this.is_favorite);
            writer.WriteString(this.address);
            writer.WriteString(this.text);
            writer.WriteString(this.button_text);

            writer.WriteList(this.friends);
            writer.WriteList(this.Friends);

            writer.Write(this.Owner as VKGroup);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.time = reader.ReadDateTime();
            this.member_status = reader.ReadUInt32();
            this.is_favorite = reader.ReadBoolean();
            this.address = reader.ReadString();
            this.text = reader.ReadString();
            this.button_text = reader.ReadString();
            this.friends = reader.ReadListInt();
            this.Friends = reader.ReadList<VKUser>();

            this.Owner = reader.ReadGeneric<VKGroup>();
        }
        #endregion
    }
}
