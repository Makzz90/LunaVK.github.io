using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class FriendsList : IBinarySerializable
    {
        public string name { get; set; }

        public int id { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.WriteString(this.name);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadInt32();
            this.name = reader.ReadString();
        }
    }
}
