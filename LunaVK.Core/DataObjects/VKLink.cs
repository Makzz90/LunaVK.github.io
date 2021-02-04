using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LunaVK.Core.Json;
using LunaVK.Core.Enums;
using Windows.UI.Xaml;
using LunaVK.Core.ViewModels;
using Windows.UI.Xaml.Media.Imaging;
using System;
using LunaVK.Core.Utils;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Ссылка (type = link)
    /// https://vk.com/dev/attachments_w
    /// </summary>
    public class VKLink : IBinarySerializable
    {
        /// <summary>
        /// Адрес ссылки.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Заголовок ссылки.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// подпись ссылки (если имеется). 
        /// </summary>
        public string caption { get; set; }

        /// <summary>
        /// Описание ссылки.
        /// </summary>
        public string description { get; set; }
        
        public VKPhoto photo { get; set; }

        /// <summary>
        /// является ли ссылка внешней
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_external { get; set; }

        public LinkProduct product { get; set; }
        public LinkRating rating { get; set; }
        public LinkApplication application { get; set; }
        public VKPrettyCard.Button button { get; set; }


        /// <summary>
        /// идентификатор вики-страницы с контентом для предпросмотра
        /// содержимого страницы. Возвращается в формате "owner_id_page_id". 
        /// </summary>
        public string preview_page { get; set; }

        /// <summary>
        /// URL страницы с контентом для предпросмотра содержимого страницы. 
        /// </summary>
        public string preview_url { get; set; }

#region Classes
        public class LinkProduct
        {
            public Price price { get; set; }
        }

        public class LinkRating
        {
            public double stars { get; set; }

            public long reviews_count { get; set; }
        }

        public class LinkApplication
        {
            public LinkStore store { get; set; }

            public string app_id { get; set; }
        }

        public class Price
        {
            public long amount { get; set; }

            public VKGroupMarket.VKCurrency currency { get; set; }

            public string text { get; set; }
        }

        public class LinkStore
        {
            public int id { get; set; }

            public string name { get; set; }
        }
#endregion

#region VM
        /*
        public string ImageSrc
        {
            get
            {
                if (this.photo == null)
                    return "";

                if (!string.IsNullOrEmpty(this.photo.photo_130))
                    return this.photo.photo_130;

                if (!string.IsNullOrEmpty(this.photo.photo_75))
                    return this.photo.photo_75;

                return "";
            }
        }
        */
        public BitmapImage ImageSrc
        {
            get
            {
                if (this.photo == null)
                    return null;

                string temp = null;

                if (this.IsAMP)
                {
                    temp = this.photo.photo_807;
                    if (string.IsNullOrEmpty(temp))
                        temp = this.photo.photo_604;
                    if (string.IsNullOrEmpty(temp))
                        temp = this.photo.photo_130;
                }
                else
                {
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
                }

                if (string.IsNullOrEmpty(temp))
                    return null;

                return new BitmapImage(new Uri(temp));
            }
        }

        public string CaptionUI
        {
            get
            {
                if (!string.IsNullOrEmpty(this.caption))
                    return this.caption;

                //http://itun.es/i6xC69N
                string ret = this.url.Substring(this.url.IndexOf("//") + 2);
                int temp = ret.IndexOf("/");
                if (temp > 0)
                    ret = ret.Substring(0, temp);
                return ret;
            }
        }

        /// <summary>
        /// Имеет ли ссылка изображение.
        /// </summary>
        private bool HasImage
        {
            get { return this.ImageSrc != null; }
        }

        public Visibility IconLinkVisibility
        {
            get
            {
                return this.HasImage || this.button != null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility BtnVisibility
        {
            get
            {
                return this.button == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Thickness BorderThickness
        {
            get
            {
                if (this.IsAMP)
                    return new Thickness(0);
                return this.button == null ? new Thickness(0/*, 0, 0, 1*/) : new Thickness(1);
            }
        }

        public bool IsAMP
        {
            get
            {
                //https://vk.com/@-76477496-prometheus-posts
                return this.button != null && this.button.action.url.Contains("vk.com/@");
            }
        }

        public bool IsLink
        {
            get
            {
                return this.button == null;
            }

        }

        public Visibility AMPVisibility
        {
            get
            {
                return this.IsAMP ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return this.IsAMP ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            }
        }

        public int TextRowSpan
        {
            get
            {
                if (this.IsLink)
                    return 2;
                return this.IsAMP ? 2 : 0;
            }
        }

        public int TextRow
        {
            get
            {
                if (this.IsLink)
                    return 0;
                return this.IsAMP ? 0 : 1;
            }
        }


        public int ContentColumnSpan
        {
            get
            {
                return this.IsLink ? 0 : 2;
            }
        }

        public int ContentColumn
        {
            get
            {
                return this.IsLink ? 1 : 0;
            }
        }

        public int TextColumnSpan
        {
            get
            {
                return this.IsAMP ? 2 : 0;
            }
        }

        public int ImgColumnSpan
        {
            get
            {
                return this.IsLink ? 0 : 2;
            }
        }

        public int BtnColumn
        {
            get
            {
                return this.IsAMP ? 0 : 1;
            }
        }

        public int BtnRowSpan
        {
            get
            {
                return this.IsAMP ? 0 : 3;
            }
        }

        public int BtnRow
        {
            get
            {
                return this.IsAMP ? 3 : 0;
            }
        }

        public int ImgRowSpan
        {
            get
            {
                if (this.IsLink)
                    return 2;
                return this.IsAMP ? 2 : 0;
            }
        }

        public Visibility RatingVisibility
        {
            get
            {
                return (this.rating != null).ToVisiblity();
            }
        }
        #endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(4);
            writer.WriteString(this.url);
            writer.WriteString(this.title);
            writer.WriteString(this.description);
            writer.WriteString(this.caption);
            writer.Write<VKPhoto>(this.photo);
            //writer.Write<LinkProduct>(this.product);
            //writer.Write<LinkButton>(this.button);
           // writer.Write<LinkApplication>(this.application);
            //writer.Write<LinkRating>(this.rating);
            //writer.Write<MoneyTransfer>(this.money_transfer);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.url = reader.ReadString();
            this.title = reader.ReadString();
            this.description = reader.ReadString();
            this.caption = reader.ReadString();
            this.photo = reader.ReadGeneric<VKPhoto>();
            //this.product = reader.ReadGeneric<LinkProduct>();
            //this.button = reader.ReadGeneric<LinkButton>();
            //this.application = reader.ReadGeneric<LinkApplication>();
            //this.rating = reader.ReadGeneric<LinkRating>();

            //this.money_transfer = reader.ReadGeneric<MoneyTransfer>();
        }
    }
}
