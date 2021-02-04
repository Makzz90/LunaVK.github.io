using LunaVK.Core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Common
{
    public class SpriteListItemData
    {
        public bool IsEmoji { get; set; }

        public bool IsRecentStickers { get; set; }

        public bool IsStore { get; set; }

        public bool IsSettings { get; set; }

        public StockItem StickerStockItemHeader { get; set; }
        public StoreProduct StickerProduct
        {
            get
            {
                if (this.StickerStockItemHeader == null)
                    return null;
                return this.StickerStockItemHeader.product ?? null;
            }
        }

        public string TabThumb
        {
            get
            {
                if (this.IsEmoji)
                    return "\xE76E";//"/Resources/Smile32px.png";
                if (this.IsRecentStickers)
                    return "\xED5A";//"/Resources/Recent32px.png";
                if (this.IsStore)
                    return "\xE7BF";//"/Resources/Shop32px.png";
                return this.IsSettings ? /*"/Resources/Settings32px.png"*/"\xE713" : null;
            }
        }

        public double TabImageOpacity
        {
            get
            {
                if (this.StickerProduct == null)
                    return 1.0;
                return this.StickerProduct.purchased ? 1.0 : 0.4;
            }
        }

        public double ImageDim
        {
            get
            {
                return this.IsStickersPack ? 60.0 : 40.0;
            }
        }

        public bool IsStickersPack
        {
            get
            {
                if (!this.IsEmoji && !this.IsStore && !this.IsSettings)
                    return !this.IsRecentStickers;
                return false;
            }
        }

        public string TabThumbSticker
        {
            get
            {
                if (this.StickerStockItemHeader == null)
                    return null;
                return this.StickerStockItemHeader.photo_70;//string.Format("{0}thumb_102.png", this.StickerProduct.base_url);
            }
        }
    }
}
