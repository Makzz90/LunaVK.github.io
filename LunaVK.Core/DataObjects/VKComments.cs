using Newtonsoft.Json;
using LunaVK.Core.Json;
using System.Collections.Generic;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKComments : IBinarySerializable
    {
        /// <summary>
        /// Количество комментариев.
        /// </summary>
        public uint count { get; set; }

        /// <summary>
        /// Может ли текущий пользователь комментировать запись.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_post { get; set; }

        public List<VKComment> list { get; set; }

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.count);
            writer.Write(this.can_post);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.count = reader.ReadUInt32();
            this.can_post = reader.ReadBoolean();
        }
#endregion
    }
}
