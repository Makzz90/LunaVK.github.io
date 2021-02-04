using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// информация о местоположении
    /// </summary>
    public sealed class VKGeo : IBinarySerializable
    {
        /// <summary>
        /// Тип места.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// координаты места
        /// Класс, если в сообщении
        /// Строка, если в посте
        /// </summary>
        public string coordinates { get; set; }

        /// <summary>
        /// Описание места (если добавлено).
        /// </summary>
        public VKPlace place { get; set; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteString(this.type);
            writer.WriteString(this.coordinates);
            writer.Write<VKPlace>(this.place);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.type = reader.ReadString();
            this.coordinates = reader.ReadString();
            this.place = reader.ReadGeneric<VKPlace>();
        }
    }
}
