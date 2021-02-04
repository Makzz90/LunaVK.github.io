using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Core.DataObjects
{
    public class VKVideoAlbum
    {
        /// <summary>
        /// идентификатор альбома; есть и отрицательные
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// идентификатор владельца альбома; 
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// название альбома; 
        /// </summary>
        public string title { get; set; }

        /// <summary>
        ///  число видеозаписей в альбоме; 
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// url обложки альбома с шириной 320px; 
        /// </summary>
        public string photo_320 { get; set; }

        /// <summary>
        /// url обложки альбома с шириной 160px; 
        /// </summary>
        public string photo_160 { get; set; }

        public List<VKImageWithSize> image { get; set; }

        /// <summary>
        /// время последнего обновления в формате unixtime. 
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime updated_time { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_system { get; set; }//no in documentation

        public VKAlbumPrivacy privacy { get; set; }

        /// <summary>
        /// Добавленные
        /// </summary>
        public static readonly int ADDED_ALBUM_ID = -2;

        /// <summary>
        /// Загруженные
        /// </summary>
        public static readonly int UPLOADED_ALBUM_ID = -1;


#region VM
        public string Title
        {
            get { return this.title; }
        }

        public string Subtitle1
        {
            get { return UIStringFormatterHelper.FormatNumberOfSomething(this.count,"OneVideoFrm", "TwoFourVideosFrm", "FiveOrMoreVideosFrm"); }
        }

        public string Subtitle2
        {
            get
            {
                string str = LocalizedStrings.GetString("VideoCatalog_AlbumUpdated") + " " + UIStringFormatterHelper.FormatDateTimeForUI(this.updated_time).ToLower();

                return str;
            }
        }

        public string UIDuration
        {
            get { return ""; }
        }

        public string ImageUri
        {
            get
            {
                if (this.image.IsNullOrEmpty())
                    return null;
                var temp = this.image.FirstOrDefault((i) => i.width == 320);
                if (temp != null)
                    return temp.url;

                return this.image[0].url;
                //return this.photo_320;
            }
        }

        public Visibility AlreadyViewedVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public Visibility IsVideoVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public Visibility PrivacyVisibility
        {
            get
            {
                if (this.privacy == null)
                    return Visibility.Collapsed;
                return (this.privacy.category != "all").ToVisiblity();
            }
        }
#endregion
    }
}
