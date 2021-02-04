using Newtonsoft.Json;
using LunaVK.Core.Json;
using System.Collections.Generic;
using System.IO;
using LunaVK.Core.Framework;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKLikes : IBinarySerializable
    {
        /// <summary>
        /// Количество отметок.
        /// </summary>
        public uint count { get; set; }

        /// <summary>
        /// Есть ли отметка от ткущего пользователя.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool user_likes { get; set; }

        /// <summary>
        /// Может ли текущий пользователь поставить отметку.
        /// Возвращает НЕТ если уже ставил лайк :(
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_like { get; set; }

        /// <summary>
        /// Может ли текущий пользователь сделать репост.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_publish { get; set; }

#region VM
        public string Count
        {
            get
            {
                if (this.count == 0)
                    return "";

                return this.count.ToString();
            }
        }

        //public List<VKUser> users { get; set; }
#endregion

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.count);
            writer.Write(this.user_likes);
            writer.Write(this.can_like);
            writer.Write(this.can_publish);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.count = reader.ReadUInt32();
            this.user_likes = reader.ReadBoolean();
            this.can_like = reader.ReadBoolean();
            this.can_publish = reader.ReadBoolean();
        }
#endregion
    }
}
