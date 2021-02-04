using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// место, указанное в информации о сообществе
    /// </summary>
    public sealed class VKPlace : IBinarySerializable
    {
        /// <summary>
        /// Идентификатор места.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Название места.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Географическая широта, заданная в градусах.
        /// </summary>
        public double latitude { get; set; }

        /// <summary>
        /// Географическая долгота места, заданная в градусах.
        /// </summary>
        public double longitude { get; set; }

        /// <summary>
        /// Тип места.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Название страны.
        /// </summary>
        public string country { get; set; }//если запись на стене, то строка

        /// <summary>
        /// Название города.
        /// </summary>
        public string city { get; set; }//если запись на стене, то строка

        /// <summary>
        /// Адрес места.
        /// </summary>
        public string address { get; set; }


#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.WriteString(this.title);
            writer.Write(this.latitude);
            writer.Write(this.longitude);
            writer.WriteString(this.type);
            writer.WriteString(this.country);
            writer.WriteString(this.city);
            writer.WriteString(this.address);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.title = reader.ReadString();
            this.latitude = reader.ReadInt64();
            this.longitude = reader.ReadInt64();
            this.type = reader.ReadString();
            this.country = reader.ReadString();
            this.city = reader.ReadString();
            this.address = reader.ReadString();
        }
#endregion
    }
}
