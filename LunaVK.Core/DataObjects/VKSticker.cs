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
    /// Объект, описывающий стикер, содержит следующие поля(с версии 5.74)
    /// https://vk.com/dev/objects/sticker
    /// </summary>
    public sealed class VKSticker : IBinarySerializable
    {
        /// <summary>
        /// Идентификатор продукта.
        /// </summary>
        public uint product_id { get; set; }

        /// <summary>
        /// Идентификатор стикера.
        /// </summary>
        public uint sticker_id { get; set; }

        /// <summary>
        /// изображения для стикера с прозрачным фоном
        /// </summary>
        public List<VKImageWithSize> images { get; set; }

        /// <summary>
        /// изображения для стикера с непрозрачным фоном
        /// обычно оканчивается на b.png
        /// </summary>
        public List<VKImageWithSize> images_with_background;

        /// <summary>
        /// Ссылка на JSON файл с анимацией
        /// </summary>
        public string animation_url { get; set; }


        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_allowed { get; set; }//для магазина

#region VM
        public string photo_64 { get { return this.images_with_background[0].url; } }

        public string photo_128 { get { return this.images_with_background[1].url; } }

        public string photo_256 { get { return this.images_with_background[2].url; } }

        public string photo_352 { get { return this.images_with_background[3].url; } }

        public string photo_512 { get { return this.images_with_background[4].url; } }

        public void SetImage(string source)
        {
            this.images = new List<VKImageWithSize>();
            for (int i = 0; i < 5; i++)
                this.images.Add(new VKImageWithSize() { url = source });
        }

        public double ImageOpacity
        {
            get
            {
                return this.is_allowed ? 1.0 : 0.4;
            }
        }
        #endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.WriteList(this.images_with_background);
            writer.WriteString(this.animation_url);
            writer.Write(this.sticker_id);
            writer.Write(this.product_id);
            writer.Write(this.is_allowed);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.images_with_background = reader.ReadList<VKImageWithSize>();
            this.animation_url = reader.ReadString();
            this.sticker_id = reader.ReadUInt32();
            this.product_id = reader.ReadUInt32();
            this.is_allowed = reader.ReadBoolean();
        }
    }
}
