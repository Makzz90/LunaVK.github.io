using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public sealed class VKReposts : IBinarySerializable
    {
        /// <summary>
        /// Количество репостов.
        /// </summary>
        public uint count { get; set; }

        /// <summary>
        /// Скопировал ли текущий пользователь.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool user_reposted { get; set; }

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.count);
            writer.Write(this.user_reposted);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.count = reader.ReadUInt32();
            this.user_reposted = reader.ReadBoolean();
        }
#endregion
    }
}
