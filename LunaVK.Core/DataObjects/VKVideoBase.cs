using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Windows.UI.Xaml;
using System.IO;
using LunaVK.Core.Framework;
using System.Linq;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// https://vk.com/dev/objects/video
    /// </summary>
    public class VKVideoBase : IBinarySerializable
    {
        /// <summary>
        /// Идентификатор видеозаписи.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор владельца видеозаписи.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// название видеозаписи.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Описание видеозаписи.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Длительность видеозаписи.
        /// </summary>
        [JsonConverter(typeof(SecondsToTimeSpanConverter))]
        public TimeSpan duration { get; set; }

        /// <summary>
        /// URL изображения-обложки ролика с размером 130x98 px.
        /// </summary>
        public string photo_130 { get; set; }

        /// <summary>
        /// URL изображения-обложки ролика с размером 320x240 px.
        /// </summary>
        public string photo_320 { get; set; }

        /// <summary>
        /// URL изображения-обложки ролика с размером 640x480 px (если размер есть).
        /// </summary>
        public string photo_640 { get; set; }

        /// <summary>
        /// URL изображения-обложки ролика с размером 800x450 px (если размер есть).
        /// </summary>
        public string photo_800 { get; set; }

        /// <summary>
        /// Дата создания видеозаписи.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime date { get; set; }

        /// <summary>
        /// Дата добавления видеозаписи.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime adding_date { get; set; }

        /// <summary>
        /// Количество просмотров.
        /// </summary>
        public int views { get; set; }

        /// <summary>
        /// Количество комментариев.
        /// </summary>
        public int comments { get; set; }

        /// <summary>
        /// Ссылка на HTML5-проигрыватель.
        /// </summary>
        public string player { get; set; }

        /// <summary>
        /// название платформы (для видеозаписей, добавленных с внешних сайтов).
        /// YouTube
        /// </summary>
        public string platform { get; set; }

        /// <summary>
        /// содержит 1, если текущий пользователь может редактировать запись; 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_edit { get; set; }

        /// <summary>
        /// 1, если пользователь может добавить видеозапись к себе. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_add { get; set; }

        /// <summary>
        /// поле возвращается, если видеозапись приватная
        /// (например, была загружена в личное сообщение), всегда содержит 1.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_private { get; set; }

        /// <summary>
        /// Ключ прямого доступа к закрытым объектам.
        /// </summary>
        public string access_key { get; set; }

        /// <summary>
        /// поле возвращается в том случае, если видеоролик находится в процессе обработки, всегда содержит 1. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool processing { get; set; }

        /// <summary>
        /// поле возвращается в том случае, если видеозапись
        /// является прямой трансляцией, всегда содержит 1.
        /// Обратите внимание, в этом случае в поле duration содержится значение 0. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool live { get; set; }

        /// <summary>
        /// для live = 1). Поле свидетельствует о том, что трансляция скоро начнётся. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool upcoming { get; set; }

#region Undocumented
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool repeat { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_attach_link { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_add_to_faves { get; set; }
        /*
        /// <summary>
        /// содержащие URL изображения-первого кадра видео с соответствующей шириной, и поле
        /// </summary>
        [Obsolete]
        public string first_frame_320 { get; set; }

        [Obsolete]
        public string first_frame_160 { get; set; }

        [Obsolete]
        public string first_frame_130 { get; set; }

        [Obsolete]
        public string first_frame_800 { get; set; }
        */
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_comment { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_repost { get; set; }

        public VKLikes likes { get; set; }

        public VKReposts reposts { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_like { get; set; }

        public bool is_favorite { get; set; }

        /// <summary>
        /// short_video
        /// </summary>
        public string type { get; set; }

        //float volume_multiplier
        //string ov_id
#endregion




        public VideoFiles files { get; set; }//no in documentation

        public List<VKImageWithSize> image { get; set; }

        public List<VKImageWithSize> first_frame { get; set; }



        public class VideoFiles
        {
            public string mp4_240 { get; set; }

            public string mp4_360 { get; set; }

            public string mp4_480 { get; set; }

            public string mp4_720 { get; set; }

            public string mp4_1080 { get; set; }
        }

#region VM
        public VKBaseDataForGroupOrUser Owner { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool watched { get; set; }

        public Visibility AlreadyViewedVisibility
        {
            get { return this.watched == false ? Visibility.Collapsed : Visibility.Visible; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Subtitle1
        {
            get
            {
                if (this.Owner == null)
                    return "";

                return this.Owner.Title;
            }
        }

        public string Subtitle2
        {
            get { return UIStringFormatterHelper.FormatNumberOfSomething(this.views, "OneViewFrm", "TwoFourViewsFrm", "FiveViewsFrm"); }
        }

        public string ImageUri
        {
            get
            {
                if (this.image.IsNullOrEmpty())
                    return null;
                var temp = this.image.FirstOrDefault((i)=>i.width == 320);
                if (temp != null)
                    return temp.url;

                return this.image[0].url;
                //return this.photo_320;
            }
        }

        public string UIDuration
        {
            get
            {
                if (this.live == true)
                    return LocalizedStrings.GetString("VideoCatalog_LIVE");
                //if (this.duration <= 0)
                //    return "";
                string pl = "";
                if(this.platform == "YouTube")
                    pl = " · YouTube";
                else if (this.platform == "Vimeo")
                    pl = " · Vimeo";
                return UIStringFormatterHelper.FormatDuration(this.duration) + pl;
            }
        }

        public Visibility IsVideoVisibility
        {
            get { return Visibility.Visible; }
        }

        public Visibility PrivacyVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override string ToString()
        {
            //int ownerId = this._pickedDocument.owner_id;
            //uint id = this._pickedDocument.id;
            //string accessKey = this._pickedDocument.access_key;
            string str = string.Format("video{0}_{1}", this.owner_id, this.id);
            //if (ownerId != Settings.UserId && !string.IsNullOrEmpty(accessKey))
            //    str += string.Format("_{0}", accessKey);
            return str;
        }
#endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.Write(this.owner_id);
            writer.WriteString(this.title);
            writer.Write(this.duration);
            writer.WriteString(this.description);
            writer.Write(this.date);
            writer.Write(this.views);
            writer.WriteString(this.photo_130);
            writer.WriteString(this.photo_320);
            writer.WriteString(this.photo_640);
            writer.WriteString(this.access_key);
            //writer.WriteString(this.link);
            //writer.WriteDictionary(this.files);
            writer.Write(this.comments);
            writer.WriteString(this.player);
            writer.Write(this.can_edit);
            writer.Write(this.can_repost);
            writer.Write(this.can_comment);
            writer.Write(this.live);
            writer.Write(this.watched);
            writer.Write<VKLikes>(this.likes);
            writer.Write(this.can_add);

            writer.Write(this.width);
            writer.Write(this.height);

            writer.WriteList(this.image);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.owner_id = reader.ReadInt32();
            this.title = reader.ReadString();
            this.duration = reader.ReadTimeSpan();
            this.description = reader.ReadString();
            this.date = reader.ReadDateTime();
            this.views = reader.ReadInt32();
            this.photo_130 = reader.ReadString();
            this.photo_320 = reader.ReadString();
            this.photo_640 = reader.ReadString();
            this.access_key = reader.ReadString();
            //this.link = reader.ReadString();
            //this.files = reader.ReadDictionary();
            this.comments = reader.ReadInt32();
            this.player = reader.ReadString();
            this.can_edit = reader.ReadBoolean();
            this.can_repost = reader.ReadBoolean();
            this.can_comment = reader.ReadBoolean();
            this.live = reader.ReadBoolean();
            this.watched = reader.ReadBoolean();
            this.likes = reader.ReadGeneric<VKLikes>();
            this.can_add = reader.ReadBoolean();

            this.width = reader.ReadInt32();
            this.height = reader.ReadInt32();

            this.image = reader.ReadList<VKImageWithSize>();
        }
    }
}
