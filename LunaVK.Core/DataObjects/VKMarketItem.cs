using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using System.IO;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    //Product

    /// <summary>
    /// Объект, описывающий товар
    /// https://vk.com/dev/objects/market_item
    /// </summary>
    public class VKMarketItem : IBinarySerializable
    {
        /// <summary>
        /// идентификатор товара. 
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// идентификатор владельца товара. 
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// название товара 
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// текст описания товара. 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// цена.
        /// </summary>
        public MarketItemPrice price { get; set; }

        /// <summary>
        /// вес в граммах
        /// </summary>
        public int weight { get; set; }

        /// <summary>
        /// категория товара
        /// </summary>
        public MarketItemCategory category { get; set; }

        /// <summary>
        /// URL изображения-обложки товара. 
        /// </summary>
        public string thumb_photo { get; set; }

        /// <summary>
        /// дата создания товара в формате Unixtime. 
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// статус доступности товара
        /// 0 — товар доступен; 
        /// 1 — товар удален; 
        /// 2 — товар недоступен. 
        /// </summary>
        public int availability { get; set; }

        //---------------- Опциональные поля, задаваемые параметром extended = 1 --------------------

        /// <summary>
        /// возможность комментировать товар
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_comment { get; set; }

        /// <summary>
        /// возможность сделать репост товара
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_repost { get; set; }

        public List<VKPhoto> photos { get; set; }

        public VKLikes likes { get; set; }

        public VKReposts reposts { get; set; }

        public int views_count { get; set; }

        

        #region VM
        public string PriceString
        {
            get
            {
                if (price != null && !string.IsNullOrEmpty(price.text))
                    return price.text;

                return "";
            }
        }

        public string UriSource
        {
            get
            {
                /*
                 * if (photos != null && photos.Count > 0)
        ImageLoader.SetUriSource(this.image, photos[0].GetAppropriateForScaleFactor(((FrameworkElement) this.image).Height, 1));
      else
        ImageLoader.SetUriSource(this.image, "");
        */
                if (!string.IsNullOrEmpty(this.thumb_photo))
                    return this.thumb_photo;

                if (photos != null && photos.Count > 0)
                    return photos[0].ThumbnailSource;
                return "";
            }
        }

        public string CategoryString
        {
            get
            {
                if (category != null && !string.IsNullOrEmpty(category.name))
                    return category.name;

                return "";
            }
        }
#endregion




        /// <summary>
        /// Price class
        /// </summary>
        public class MarketItemPrice : IBinarySerializable
        {
            /// <summary>
            /// цена товара в сотых долях единицы валюты; 
            /// </summary>
            public int amount { get; set; }

            /// <summary>
            /// валюта
            /// </summary>
            public Currency currency { get; set; }

            /// <summary>
            /// старая цена товара в сотых долях единицы валюты
            /// </summary>
            public int old_amount { get; set; }

            /// <summary>
            ///  строковое представление цены. 
            /// </summary>
            public string text { get; set; }

            public class Currency : IBinarySerializable
            {
                /// <summary>
                /// идентификатор валюты; 
                /// </summary>
                public int id { get; set; }

                /// <summary>
                /// обозначение валюты; 
                /// </summary>
                public string name { get; set; }

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

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write(this.amount);
                writer.Write<Currency>(this.currency);
                writer.WriteString(this.text);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.amount = reader.ReadInt32();
                this.currency = reader.ReadGeneric<Currency>();
                this.text = reader.ReadString();
            }
        }

        /// <summary>
        /// Category class
        /// </summary>
        public class MarketItemCategory : IBinarySerializable
        {
            /// <summary>
            /// идентификатор категории; 
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// название категории; 
            /// </summary>
            public string name { get; set; }

            public Section section { get; set; }

            public class Section
            {
                /// <summary>
                /// идентификатор секции; 
                /// </summary>
                public int id { get; set; }

                /// <summary>
                /// название секции. 
                /// </summary>
                public string name { get; set; }
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.Write(this.id);
                writer.WriteString(this.name);
//                writer.Write<Section>(this.section, false);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.id = reader.ReadInt32();
                this.name = reader.ReadString();
//                this.section = reader.ReadGeneric<Section>();
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            writer.WriteString(this.description);
            writer.Write<MarketItemPrice>(this.price);
//            writer.Write<MarketItemCategory>(this.category);
            writer.WriteString(this.thumb_photo);
            writer.Write(this.date);
            writer.Write(this.availability);
            writer.WriteList<VKPhoto>(this.photos);
            writer.Write(this.can_comment);
            writer.Write(this.can_repost);
            //writer.Write<Likes>(this.likes, false);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            this.description = reader.ReadString();
            this.price = reader.ReadGeneric<MarketItemPrice>();
//            this.category = reader.ReadGeneric<MarketItemCategory>();
            this.thumb_photo = reader.ReadString();
            this.date = reader.ReadDateTime();
            this.availability = reader.ReadInt32();
            this.photos = reader.ReadList<VKPhoto>();
            this.can_comment = reader.ReadBoolean();
            this.can_repost = reader.ReadBoolean();
            //this.likes = reader.ReadGeneric<Likes>();
        }
    }
}
