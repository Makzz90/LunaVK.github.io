using LunaVK.Core.Framework;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;

namespace LunaVK.Core.DataObjects
{
    public class VKAlbumPhoto : IBinarySerializable
    {
        /// <summary>
        /// идентификатор альбома
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// идентификатор фотографии, которая является обложкой (0, если обложка отсутствует);
        /// </summary>
        public int thumb_id { get; set; }

        /// <summary>
        /// идентификатор владельца альбома; 
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// название альбома; 
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// описание альбома; (не приходит для системных альбомов) 
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// дата создания альбома в формате unixtime; (не приходит для системных альбомов); 
        /// </summary>
        public int created { get; set; }

        /// <summary>
        /// дата последнего обновления альбома в формате unixtime; (не приходит для системных альбомов); 
        /// </summary>
        public int updated { get; set; }
        
        /// <summary>
        /// количество фотографий в альбоме; 
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 1, если текущий пользователь может загружать фотографии в альбом (при запросе информации об альбомах сообщества); 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_upload { get; set; }

        /// <summary>
        /// настройки приватности для альбома в формате настроек приватности (только для альбома пользователя, не приходит для системных альбомов); 
        /// </summary>
        public VKAlbumPrivacy privacy_view { get; set; }

        /// <summary>
        ///  настройки приватности для альбома в формате настроек приватности (только для альбома пользователя, не приходит для системных альбомов); 
        /// </summary>
        public VKAlbumPrivacy privacy_comment { get; set; }

        //upload_by_admins_only
        //comments_disabled

        /// <summary>
        /// ссылка на изображение обложки альбома (если был указан параметр need_covers). 
        /// </summary>
        public string thumb_src { get; set; }

        public List<DocPreviewPhotoSize> sizes { get; set; }

        public VKPhoto thumb { get; set; }

        public string Optimalthumb
        {
            get
            {
                //Rect size = Window.Current.Bounds;

                //var width = Window.Current.Bounds.Width * DisplayProperties.LogicalDpi;
                //var height = Window.Current.Bounds.Height * DisplayProperties.LogicalDpi;
                /*
                DocPreviewPhotoSize z = sizes.Find((element) => { return element.type == "z" || element.type == "y" || element.type == "r"; });
                if(z==null)
                {
                    return this.sizes[0].src;
                }
                return z.src;
                */

                this.sizes.Sort((x, y)=>
                {
                    if(x.width==0)
                    {
                        int valX = x.type[0];
                        int valY = y.type[0];
                        return valX < valY ? 1 : -1;

                    }
                    return x.width < y.width ? 1 : -1;
                });

                foreach (var size in this.sizes)
                {
                    if (size.type == "x")
                        return size.src;
                    else if (size.type == "y")
                        return size.src;
                    else if (size.type == "z")
                        return size.src;
                    else if (size.type == "m")
                        return size.src;
                }
                return null;
            }
        }

        public Visibility PrivacyVisibility
        {
            get
            {
                if (this.privacy_view == null)
                    return Visibility.Collapsed;
                return (this.privacy_view.category != "all").ToVisiblity();
            }
        }

        /// <summary>
        /// Альбом с сохраненными фотографиями
        /// </summary>
        public static readonly int SavedPhotos = -15;
        
        public static readonly int WallPhotos = -7;

        public static readonly int ProfilePhotos = -6;

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.id);
            writer.Write(this.thumb_id);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            writer.Write(this.description);
            writer.Write(this.created);
            writer.Write(this.updated);
            writer.Write(this.size);
            writer.WriteString(this.thumb_src);
            //writer.Write(this.thumb_src_small);
            writer.Write(this.thumb);
            //writer.WriteList(this.privacy_view);
            //writer.Write(this.comment_privacy);
            writer.Write(this.can_upload);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadInt32();
            this.thumb_id = reader.ReadInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            this.description = reader.ReadString();
            this.created = reader.ReadInt32();
            this.updated = reader.ReadInt32();
            this.size = reader.ReadInt32();
            this.thumb_src = reader.ReadString();
            //this.thumb_src_small = reader.ReadString();
            this.thumb = reader.ReadGeneric<VKPhoto>();
            //this.privacy_view = reader.ReadList();
            //this.comment_privacy = reader.ReadInt32();
            this.can_upload = reader.ReadBoolean();
        }
    }
}
