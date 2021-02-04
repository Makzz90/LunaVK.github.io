using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// город, указанный в информации о сообществе
    /// </summary>
    public class VKCity : IBinarySerializable
    {
        /// <summary>
        /// идентификатор города
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// название города
        /// </summary>
        public string title { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(0);
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
