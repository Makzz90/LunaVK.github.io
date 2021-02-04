using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKViews : IBinarySerializable
    {
        public uint count { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.count);
        }

        public void Read(BinaryReader reader)
        {
            this.count = reader.ReadUInt32();
        }
    }
}
