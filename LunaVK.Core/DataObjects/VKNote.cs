using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /*
     * "id": 11714188,
"owner_id": 371592790,
"comments": 0,
"read_comments": 0,
"date": 1513962420,
"title": "РЕКЛАМА НА КАНАЛЕ Nyamik / Нямик :3",
"view_url": "https://m.vk.com/...d5f3d0910e3aa50c642"
     * */
    public class VKNote : IBinarySerializable
    {
        public uint id { get; set; }
        public int owner_id { get; set; }
        public int comments { get; set; }
        public int read_comments { get; set; }
        public int date { get; set; }
        public string title { get; set; }
        public string view_url { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            writer.Write(this.comments);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            this.comments = reader.ReadInt32();
        }
    }
}
