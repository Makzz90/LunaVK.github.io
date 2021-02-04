using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.Photo.ViewModels
{
    public class SelectStickerViewModel
    {
        public Image img { get; set; }

        public SelectStickerViewModel(Image image)
        {
            this.img = image;
            this.Items = new ObservableCollection<StickerPack>();
            this.InitPanel();            
        }
        
        public void UpdateSource(string path)
        {
            var bimg = new BitmapImage(new Uri(path));
            this.img.Source = bimg;
        }

        public ObservableCollection<StickerPack> Items { get; set; }

        public class StickerPack
        {
            public string preview { get; set; }

            public Action<string> SelectionCallBack;

            private VKSticker _selectedItem;
            public VKSticker SelectedItem
            {
                get
                {
                    return this._selectedItem;
                }
                set
                {
                    this._selectedItem = value;
                    if (SelectionCallBack != null)
                        SelectionCallBack(this._selectedItem.photo_256);
                }
            }

            public List<VKSticker> stickers { get; set; }
        }

        private int[] ids = new int[] { 3230, 4025, 6106,3760,4023,3229,
        6105,6389,3231,6107,3998,3206,3996,5383,3997,5638,3218,3234,
        4260,6382,3200,4630,10695,6378,10696,10697,10698,4634,6384,
        5635,3759,4627,3767,5634,4633,4628,5379,4130,4262,5982,4065,
        4063,5979,3949,4264,3743,5981,3749,4027,3922,4632,4638,3241,
        5381,3746,6379,3756,3185,8438,4122,4131,8439,4701,6380,4124,
        3919,6387,4629,5645,4263,5386,6381,4637,6377,6390,4064,3758,
        3750,4265,3771,3768,5378,3177,3770,6105,4636,5380,5637,5633,
        5640,5646,5630,3217,5382,4631,5632,5451,3994,5385,4635,
        3222,3742,4356,4357,4358,4359,4266,4267,4268,3235,4269,4270,
        4271,4272,4273,4128,4123,4061,4062,6108,4066,4024,3239,4026,
        4028,3991,3992,3993,3995,5377,3947,3948,3950,3920,3921,3751,
        3753,3755,3757,3763,3765,3769,3772,3541,3175,3176,3178,3179,
        3180,3181,3182,3183,3184,3186,3188,3189,3190,3191,3192,3193,
        3194,3195,3196,3197,3198,3199,3201,3202,3203,3204,3205,3207,
        3208,3209,3210,3211,3212,3213,3214,3215,3216,3219,3220,3221,
        3223,3224,3225,3226,3227,3228,3232,3233,3236,3237,3238,3240,
        3442,3243,3244};

        private void InitPanel()
        {
            var vkPack = new StickerPack();
            vkPack.preview = "https://vk.com/sticker/1-3200-64-9";
            List<VKSticker> vkStickers = new List<VKSticker>();
            //<img src="https://vk.com/sticker/1-3230-128-9">
            foreach(int id in ids)
            {
                var s = new VKSticker();
                s.images_with_background = new List<VKImageWithSize>();
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-64-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-128-9" });
                s.images_with_background.Add(new VKImageWithSize() { url = "https://vk.com/sticker/1-" + id + "-256-9" });
                vkStickers.Add(s);
            }

            vkPack.stickers = vkStickers;
            vkPack.SelectionCallBack = SelectionCallBack;
            this.Items.Add(vkPack);


            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };
            //l.Add(StoreProductFilter.Active);
            //List<StockItem> temp = StoreService.Instance.Stickers;

            //if (temp.Count == 0)
            //    temp = await StoreService.Instance.GetStickers(l);

            //if (temp == null)
            //    return;

            StoreService.Instance.GetStockItems(l,(result)=> {
                if (result != null && result.error.error_code == VKErrors.None)
                {
                    foreach (var pack in result.response.items)
                    {
                        var item = new StickerPack();
                        item.preview = pack.photo_35;
                        item.stickers = pack.product.stickers;
                        item.SelectionCallBack = SelectionCallBack;
                        this.Items.Add(item);
                    }
                }
            });
            

        }

        private void SelectionCallBack(string selected)
        {
            this.UpdateSource(selected);
        }
    }
}
