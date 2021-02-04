using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;
using System.Text;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Товар
    /// </summary>
    public class VKMarket : IBinarySerializable
    {
        /// <summary>
        /// идентификатор подборки
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// идентификатор владельца подборки
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// название подборки
        /// </summary>
        public string title { get; set; }

        public VKPhoto photo { get; set; }

        /// <summary>
        /// число товаров в подборке
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// дата обновления подборки в формате unixtime
        /// </summary>
        public int updated_time { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            //writer.WriteString(this.description);
            //writer.Write<Price>(this.price, false);
            //writer.Write<Category>(this.category, false);
            //writer.WriteString(this.thumb_photo);
            //writer.Write(this.date);
            //writer.Write(this.availability);
            //writer.WriteList<Photo>((IList<Photo>)this.photos, 10000);
            //writer.Write(this.can_comment);
            //writer.Write(this.can_repost);
            //writer.Write<Likes>(this.likes, false);

            writer.Write(this.photo);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            //this.description = reader.ReadString();
            //this.price = reader.ReadGeneric<Price>();
            //this.category = reader.ReadGeneric<Category>();
            //this.thumb_photo = reader.ReadString();
            //this.date = reader.ReadInt64();
            //this.availability = reader.ReadInt32();
            //this.photos = reader.ReadList<Photo>();
            //this.can_comment = reader.ReadInt32();
            //this.can_repost = reader.ReadInt32();
            //this.likes = reader.ReadGeneric<Likes>();

            this.photo = reader.ReadGeneric<VKPhoto>();
        }
    }
}
