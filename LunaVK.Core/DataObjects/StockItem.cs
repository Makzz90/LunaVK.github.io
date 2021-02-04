using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI;
using LunaVK.Core.ViewModels;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    public class StockItem : ViewModelBase
    {
        public StoreProduct product { get; set; }

        public string description { get; set; }
        public string author { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_purchase { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool free { get; set; }

        public string payment_type { get; set; }
        public int price { get; set; }

        public string price_str { get; set; }

        public string photo_35 { get; set; }//миниатюрный логотип стикеров
        public string photo_70 { get; set; }
        public string photo_140 { get; set; }
        public string photo_296 { get; set; }
        public string photo_592 { get; set; }
        public string background { get; set; }
        public List<string> demo_photos_560 { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool @new { get; set; }


        #region VM

        public SolidColorBrush TabBackground
        {
            get
            {
                if (!this.IsSelected)
                    return new SolidColorBrush(Colors.Transparent);
                return Application.Current.Resources["AccentBrushHigh"] as SolidColorBrush;
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                //base.NotifyPropertyChanged<bool>(() => this.IsSelected);
                //base.NotifyPropertyChanged<string>(() => this.TabThumb);
                //base.NotifyPropertyChanged<string>(() => this.TabThumbSticker);
                base.NotifyPropertyChanged<SolidColorBrush>(() => this.TabBackground);
            }
        }

        //SpriteListItemData
        public double TabImageOpacity
        {
            get
            {
                return this.product.purchased ? 1.0 : 0.4;
            }
        }

        public string DemoPhotosBackground
        {
            get
            {
                return this.background;
            }
        }

        public string DemoPhotosBackgroundThemed
        {
            get
            {
                //if (this._themeHelper.PhoneDarkThemeVisibility != Visibility.Visible)
                    return this.DemoPhotosBackground;
                //return "";
            }
        }

        public List<string> DemoPhotos
        {
            get
            {
                return this.demo_photos_560;
            }
        }

        public bool IsDemoPhotosSlideViewCycled
        {
            get
            {
                if (this.demo_photos_560 != null)
                    return this.demo_photos_560.Count > 1;
                return false;
            }
        }

        public Visibility NavDotsVisibility
        {
            get
            {
                return (this.demo_photos_560 != null && this.demo_photos_560.Count > 1).ToVisiblity();
            }
        }

        public Visibility PurchaseVisibility
        {
            get
            {
                return (!this.product.purchased /*&& !this.IsRecentStickersPack*/).ToVisiblity();
            }
        }

        public string PriceStr
        {
            get
            {
                return this.price_str ?? LocalizedStrings.GetString("Unavailable");
            }
        }
        #endregion
    }
}
