using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    public class VKGift : IBinarySerializable
    {
        public uint stickers_product_id { get; set; }

        /// <summary>
        /// Номер подарка. Бывает отрицательный :(
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// URL изображения подарка размером 256x256px.
        /// </summary>
        public string thumb_256 { get; set; }

        /// <summary>
        /// URL изображения подарка размером 96x96px.
        /// </summary>
        public string thumb_96 { get; set; }

        /// <summary>
        /// URL изображения подарка размером 48x48px.
        /// </summary>
        public string thumb_48 { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.id);
            writer.WriteString(this.thumb_256);
            writer.Write(this.stickers_product_id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadInt32();
            this.thumb_256 = reader.ReadString();
            this.stickers_product_id = reader.ReadUInt32();
        }
    }
}
