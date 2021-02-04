using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Объект photo, описывающий фотографию
    /// https://vk.com/dev/photo
    /// https://vk.com/dev/objects/photo
    /// </summary>
    public class VKPhoto : ThumbnailsLayoutHelper.IThumbnailSupport, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор фотографии.
        /// </summary>
        [JsonConverter(typeof(FixVKLink))]
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор альбома с фотографией.
        /// </summary>
        public int album_id { get; set; }

        /// <summary>
        /// Идентификатор владельца фотографии.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Идентификатор пользователя, загрузившего фотографию.
        /// Равен 100, если загружена от имени сообщества.
        /// (если фотография размещена в сообществе)
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// Описание фотографии.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Дата добавлени фотографии.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// массив с копиями изображения в разных размерах
        /// </summary>
        [JsonConverter(typeof(SizesToDictionaryConverter))]
        public Dictionary<char, VKImageWithSize> sizes { get; set; }

        /// <summary>
        /// Ширина оригинала фотографии в пикселах.
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// Высота оригинала фотографии в пикселах.
        /// </summary>
        public int height { get; set; }



#region Undocumented
        public string access_key { get; set; }

        public VKLikes likes { get; set; }//nullble

        public int post_id { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_comment { get; set; }

        public VKReposts reposts { get; set; }

        public VKComments comments { get; set; }

        public TagClass tags { get; set; }
#endregion

        public class TagClass
        {
            public int count { get; set; }
        }

#region VM
        public string MaxPhoto
        {
            get
            {
                string ret = this.photo_2560;
                if (string.IsNullOrEmpty(ret))
                    ret = this.photo_1280;
                if (string.IsNullOrEmpty(ret))
                    ret = this.photo_807;
                if (string.IsNullOrEmpty(ret))
                    ret = this.photo_604;
                if (string.IsNullOrEmpty(ret))
                    ret = this.photo_130;
                return ret;
            }
        }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 75x75px.
        /// </summary>
        public string photo_75 { get { return this.sizes.ContainsKey('s') ? this.sizes['s'].url : string.Empty; } }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 130x130px. 
        /// </summary>
        public string photo_130 { get { return this.sizes.ContainsKey('m') ? this.sizes['m'].url : string.Empty; } }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 604x604px. 
        /// </summary>
        public string photo_604 { get { return this.sizes.ContainsKey('x') ? this.sizes['x'].url : string.Empty; } }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 807x807px. 
        /// </summary>
        public string photo_807 { get { return this.sizes.ContainsKey('y') ? this.sizes['y'].url : string.Empty; } }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 1080x1024px.
        /// </summary>
        public string photo_1280 { get { return this.sizes.ContainsKey('z') ? this.sizes['z'].url : string.Empty; } }

        /// <summary>
        /// Ссылка на копию фотографии с максимальным размером 2560x2048px. 
        /// </summary>
        public string photo_2560
        {
            get
            {
                return this.sizes.ContainsKey('w') ? this.sizes['w'].url : string.Empty;
            }
            set
            {
                if (this.sizes == null)
                    this.sizes = new Dictionary<char, VKImageWithSize>();
                if (this.sizes.ContainsKey('w'))
                    this.sizes['w'].url = value;
                else
                {
                    this.sizes.Add('w', new VKImageWithSize() { type = 'w', url = value });
                }
            }
        }


        public string photo_200 { get { return this.sizes.ContainsKey('p') ? this.sizes['p'].url : string.Empty; } }
        public string photo_320 { get { return this.sizes.ContainsKey('q') ? this.sizes['q'].url : string.Empty; } }

        public int TrueWidth
        {
            get
            {
                //BugFix: width не всегда идёт с сервера
                if (this.width > 0)
                    return this.width;
                if (this.sizes.ContainsKey('w') && this.sizes['w'].width > 0)
                    return this.sizes['w'].width;
                if (this.sizes.ContainsKey('z') && this.sizes['z'].width > 0)
                    return this.sizes['z'].width;
                if (this.sizes.ContainsKey('y') && this.sizes['y'].width > 0)
                    return this.sizes['y'].width;
                if (this.sizes.ContainsKey('x') && this.sizes['x'].width > 0)
                    return this.sizes['x'].width;
                if (this.sizes.ContainsKey('m') && this.sizes['m'].width > 0)
                    return this.sizes['m'].width;

                return this.sizes['s'].width;
            }
        }

        public int TrueHeight
        {
            get
            {
                if (this.height > 0)
                    return this.height;

                if (this.sizes.ContainsKey('w') && this.sizes['w'].height > 0)
                    return this.sizes['w'].height;
                if (this.sizes.ContainsKey('z') && this.sizes['z'].height > 0)
                    return this.sizes['z'].height;
                if (this.sizes.ContainsKey('y') && this.sizes['y'].height > 0)
                    return this.sizes['y'].height;
                if (this.sizes.ContainsKey('x') && this.sizes['x'].height > 0)
                    return this.sizes['x'].height;
                if (this.sizes.ContainsKey('m') && this.sizes['m'].height > 0)
                    return this.sizes['m'].height;

                return this.sizes['s'].height > 0 ? this.sizes['s'].height : 300;//bug: бывает сервер возвращает высоту 0 :(
            }
        }
#endregion

#region IThumbnailSupport
        /// <summary>
        /// Данные для визуализации миниатюры.
        /// </summary>
        [JsonIgnore]
        public ThumbnailsLayoutHelper.ThumbnailSize ThumbnailSize { get; set; }

        /// <summary>
        /// Ширнина исходного изображения.
        /// </summary>
        [JsonIgnore]
        double ThumbnailsLayoutHelper.IThumbnailSupport.Width { get { return this.TrueWidth; } }

        /// <summary>
        /// Высота исходного изображения.
        /// </summary>
        [JsonIgnore]
        double ThumbnailsLayoutHelper.IThumbnailSupport.Height { get { return this.TrueHeight; } }

        /// <summary>
        /// Источник изображения миниатюры.
        /// </summary>
        [JsonIgnore]
        public string ThumbnailSource
        {
            get
            {
                double num = Math.Max(this.ThumbnailSize.Width, this.ThumbnailSize.Height);
                if (num <= 75.0 && !string.IsNullOrEmpty(this.photo_75))
                    return this.photo_75;
                if (num <= 130.0 && !string.IsNullOrEmpty(this.photo_130) /*|| !AppGlobalStateManager.Current.GlobalState.LoadBigPhotosOverMobile && NetworkStatusInfo.Instance.NetworkStatus != NetworkStatus.WiFi*/)
                    return this.photo_130;
                if (num <= 200.0 && !string.IsNullOrEmpty(this.photo_200))
                    return this.photo_200;
                if (num <= 320.0 && !string.IsNullOrEmpty(this.photo_320))
                    return this.photo_320;
                if (!string.IsNullOrEmpty(this.photo_604))
                    return this.photo_604;
                if (!string.IsNullOrEmpty(this.photo_2560))
                    return this.photo_2560;
                throw new Exception("ThumbnailSource empty");
            }
        }

        /// <summary>
        /// Возвращает соотношение ширины к высоте исходного изображения.
        /// </summary>
        public double GetRatio() { return (double)this.TrueWidth / (double)this.TrueHeight; }
#endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.album_id);
            writer.Write(this.owner_id);
            writer.WriteDictionary(this.sizes);
            writer.Write(this.width);
            writer.Write(this.height);
            writer.WriteString(this.text);
            //writer.Write(this.created);
            writer.WriteString(this.access_key);
            writer.Write(this.user_id);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.album_id = reader.ReadInt32();
            this.owner_id = reader.ReadInt32();
            this.sizes = reader.ReadDictionary<VKImageWithSize>();
            this.width = reader.ReadInt32();
            this.height = reader.ReadInt32();
            this.text = reader.ReadString();
            //this.created = reader.ReadInt32();
            this.access_key = reader.ReadString();
            this.user_id = reader.ReadInt32();
        }
    }
}
