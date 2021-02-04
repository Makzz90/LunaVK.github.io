using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKProfilePhoto : IBinarySerializable
    {
        public string photo_hash { get; set; }

        public string photo_src { get; set; }

        public string photo_200 { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.WriteString(this.photo_hash);
            writer.WriteString(this.photo_src);
        }

        public void Read(BinaryReader reader)
        {
            this.photo_hash = reader.ReadString();
            this.photo_src = reader.ReadString();
        }
    }
}
