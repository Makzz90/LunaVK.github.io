using LunaVK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Common
{
    public class EmojiSpritesCreator
    {
        private SpritesPack _spritesPack = new SpritesPack();
        //private List<string> _localPaths = new List<string>();
        private int _id;
        private bool _isInitialized;

        public EmojiSpritesCreator(int id)
        {
            this._id = id;
        }

        private string SpritesPackKey
        {
            get
            {
                return "emojiSpritePack11_"/* + this.IsDarkTheme.ToString()*/;
            }
        }

        public bool TryDeserializeSpritePack()
        {
            if (this._isInitialized)
                return true;
            bool num = CacheManager.TryDeserialize(this._spritesPack, this.SpritesPackKey);
            if (!num)
                return false;
            this._isInitialized = true;
            return true;
        }

        public void CreateSprites()
        {
            if (!this.CreateEmojiSprites(this._spritesPack) || !CacheManager.TrySerialize(this._spritesPack, this.SpritesPackKey))
                return;
            this._isInitialized = true;
        }

        private bool CreateEmojiSprites(SpritesPack pack)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<EmojiDataItem> allDataItems = EmojiDataItem.GetAllDataItems();
            int num1 = EmojiConstants.ColumnsCountVerticalOrientation * EmojiConstants.VerticalSpriteRowsCount;
            int num2 = 0;
            string str1 = "emojiSpriteVertical" + this.IsDarkTheme.ToString() + ".jpg";
            SolidColorBrush backgroundColor1 = Application.Current.Resources["PhoneMenuBackgroundBrush"] as SolidColorBrush;
            IEnumerator<IEnumerable<EmojiDataItem>> enumerator1 = allDataItems.Partition<EmojiDataItem>(num1).GetEnumerator();
            try
            {
                while (((IEnumerator)enumerator1).MoveNext())
                {
                    IEnumerable<EmojiDataItem> current = enumerator1.Current;
                    SpriteDescription sprite = SpriteCreatorHelper.TryCreateSprite(num1, (List<string>)Enumerable.ToList<string>(Enumerable.Select<EmojiDataItem, string>(current, (Func<EmojiDataItem, string>)(d => d.Uri.OriginalString))), (List<string>)Enumerable.ToList<string>(Enumerable.Select<EmojiDataItem, string>(current, (Func<EmojiDataItem, string>)(d => d.String))), num2.ToString() + str1, EmojiConstants.ColumnsCountVerticalOrientation, EmojiConstants.EmojiWidthInPixels, EmojiConstants.EmojiHeightInPixels, EmojiConstants.VerticalSpriteWidthInPixels, EmojiConstants.VerticalSpriteHeightInPixels, backgroundColor1);
                    ++num2;
                    if (sprite == null)
                        return false;
                    pack.SpritesVertical.Add(sprite);
                }
            }
            finally
            {
                if (enumerator1 != null)
                    ((IDisposable)enumerator1).Dispose();
            }
            int num3 = EmojiConstants.ColumnsCountHorizontalOrientation * EmojiConstants.HorizontalSpriteRowsCount;
            int num4 = 0;
            string str2 = "emojiSprteHorizontal " + this.IsDarkTheme.ToString() + ".jpg";
            IEnumerator<IEnumerable<EmojiDataItem>> enumerator2 = allDataItems.Partition<EmojiDataItem>(num3).GetEnumerator();
            try
            {
                while (((IEnumerator)enumerator2).MoveNext())
                {
                    IEnumerable<EmojiDataItem> current = enumerator2.Current;
                    SpriteDescription sprite = SpriteCreatorHelper.TryCreateSprite(num3, (List<string>)Enumerable.ToList<string>(Enumerable.Select<EmojiDataItem, string>(current, (Func<EmojiDataItem, string>)(d => d.Uri.OriginalString))), (List<string>)Enumerable.ToList<string>(Enumerable.Select<EmojiDataItem, string>(current, (Func<EmojiDataItem, string>)(d => d.String))), num4.ToString() + str2, EmojiConstants.ColumnsCountHorizontalOrientation, EmojiConstants.EmojiWidthInPixels, EmojiConstants.EmojiHeightInPixels, EmojiConstants.HorizontalSpriteWidthInPixels, EmojiConstants.HorizontalSpriteHeightInPixels, backgroundColor1);
                    ++num4;
                    if (sprite == null)
                        return false;
                    pack.SpritesHorizontal.Add(sprite);
                }
            }
            finally
            {
                if (enumerator2 != null)
                    ((IDisposable)enumerator2).Dispose();
            }
            stopwatch.Stop();
            return true;
        }
    }
}
