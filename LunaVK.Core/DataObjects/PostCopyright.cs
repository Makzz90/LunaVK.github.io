using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class PostCopyright : IBinarySerializable
    {
        /// <summary>
        /// -163547593
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// https://vk.com/wall-163547593_654
        /// </summary>
        public string link { get; set; }

        /// <summary>
        /// owner
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// konhis
        /// </summary>
        public string name { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(3);
            writer.Write(this.id);
            writer.WriteString(this.link);
            writer.WriteString(this.type);
            writer.WriteString(this.name);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.id = reader.ReadInt32();
            this.link = reader.ReadString();
            this.type = reader.ReadString();
            this.name = reader.ReadString();
        }
    }
}
