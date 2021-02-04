using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKCountry : IBinarySerializable
    {
        /// <summary>
        /// идентификатор страны
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// название страны
        /// </summary>
        public string title { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.WriteString(this.title);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.title = reader.ReadString();
        }
    }
}
