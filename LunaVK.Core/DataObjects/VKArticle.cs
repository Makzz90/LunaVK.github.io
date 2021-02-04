using LunaVK.Core.Json;
using Newtonsoft.Json;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Core.DataObjects
{
    public class VKArticle
    {
        public uint id { get; set; }

        public int owner_id { get; set; }

        public string owner_name { get; set; }

        public string owner_photo { get; set; }

        public string state { get; set; }

        //[JsonConverter(typeof(VKBooleanConverter))]

        public bool can_report { get; set; }

        public string title { get; set; }

        public string subtitle { get; set; }

        public int views { get; set; }

        public int shares { get; set; }

        //[JsonConverter(typeof(VKBooleanConverter))]

        public bool is_favorite { get; set; }

        public string url { get; set; }

        public string view_url { get; set; }

        //[JsonConverter(typeof(VKBooleanConverter))]

        public bool no_footer { get; set; }

        public string access_key { get; set; }

        public int published_date { get; set; }

        public VKPhoto photo { get; set; }

#region VM
        public BitmapImage ImageSrc
        {
            get
            {
                if (this.photo == null)
                    return null;

                string temp = null;

                
                temp = this.photo.photo_130;
                if (string.IsNullOrEmpty(temp))
                    temp = this.photo.photo_75;
                //
                if (string.IsNullOrEmpty(temp))
                {
                    if (this.photo.sizes.ContainsKey('b'))
                        temp = this.photo.sizes['b'].url;
                }
                //
                

                if (string.IsNullOrEmpty(temp))
                    return null;

                return new BitmapImage(new Uri(temp));
            }
        }
#endregion
    }
}
