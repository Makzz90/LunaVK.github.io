using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKGraffiti : IBinarySerializable
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public string photo_200 { get; set; }
        public string photo_586 { get; set; }

        /// <summary>
        /// ширина изображения в px; 
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// высота изображения в px. 
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// URL документа с граффити; 
        /// </summary>
        public string url { get; set; }

        public string access_key { get; set; }



        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteString(this.url);
            writer.Write(this.width);
            writer.Write(this.height);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.url = reader.ReadString();
            this.width = reader.ReadInt32();
            this.height = reader.ReadInt32();
        }
    }
}
