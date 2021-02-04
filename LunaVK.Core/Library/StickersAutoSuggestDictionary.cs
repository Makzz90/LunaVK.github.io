using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaVK.Core.Library
{
    public class StickersAutoSuggestDictionary : IBinarySerializable
    {
        private Dictionary<string, StickerKeywordItem> _lookupDictionary = new Dictionary<string, StickerKeywordItem>();

        /// <summary>
        /// 48 часов
        /// </summary>
        private static readonly int REFRESH_AFTER_HOURS = 48;//было 12

        private static readonly string AUTO_SUGGEST_FILE_NAME = "StickersAutoSuggestData";

        /// <summary>
        /// 100
        /// </summary>
        private static readonly int MAX_TEXT_LENGTH = 100;

        /// <summary>
        /// 20
        /// </summary>
        private static readonly int MAX_SUGGEST_STICKERS_COUNT = 20;
        private DateTime _lastLoadedDate = DateTime.MinValue;
        private bool _isLoading;
        private bool _isLoaded;

        /// <summary>
        /// Идёт восстановление из кеша
        /// </summary>
        private bool _isRestoringState;
        private StickersKeywordsData _keywordsData;

        private static StickersAutoSuggestDictionary _instance;
        public static StickersAutoSuggestDictionary Instance
        {
            get
            {
                if (StickersAutoSuggestDictionary._instance == null)
                    StickersAutoSuggestDictionary._instance = new StickersAutoSuggestDictionary();
                return StickersAutoSuggestDictionary._instance;
            }
        }

        public uint Count
        {
            get
            {
                if (this._keywordsData == null)
                    return 0;

                return this._keywordsData.count;
            }
        }

        public class StickersKeywordsData : IBinarySerializable
        {
            public string base_url { get; set; }//не возвращается

            public uint count { get; set; }

            public List<StickerKeywordItem> dictionary { get; set; }
            
            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteString(this.base_url);
                writer.Write(this.count);
                writer.WriteList<StickerKeywordItem>(this.dictionary);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.base_url = reader.ReadString();
                this.count = reader.ReadUInt32();
                this.dictionary = reader.ReadList<StickerKeywordItem>();
            }
        }

        public class StickerKeywordItem : IBinarySerializable
        {
            /// <summary>
            /// Ключевые слова
            /// </summary>
            public List<string> words { get; set; }

            public List<VKSticker> user_stickers { get; set; }

            public List<VKSticker> promoted_stickers { get; set; }

            public StickerKeywordItem()
            {
                this.user_stickers = new List<VKSticker>();
                this.promoted_stickers = new List<VKSticker>();
            }

#region VM
            public string Words
            {
                get { return string.Join(", ", this.words); }
            }
#endregion
            
            public void Write(BinaryWriter writer)
            {
                writer.Write(1);
                writer.WriteList(this.words);
                writer.WriteList(this.user_stickers);
                writer.WriteList(this.promoted_stickers);
            }

            public void Read(BinaryReader reader)
            {
                reader.ReadInt32();
                this.words = reader.ReadList();
                this.user_stickers = reader.ReadList<VKSticker>();
                this.promoted_stickers = reader.ReadList<VKSticker>();
            }
        }

        /*
        private List<VKSticker> SortBasedOnRecents(List<VKSticker> user_stickers)
        {
            List<VKSticker> intList1 = new List<VKSticker>();
            if (user_stickers == null)
                return intList1;
            
            List<VKSticker> intList2 = null;

            //                StoreStickers recentStickers = StickersSettings.Instance.RecentStickers;
            //                intList2 = recentStickers != null ? recentStickers.sticker_ids : null;


            if (intList2 == null)
                return user_stickers;
            var enumerator1 = intList2.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    var current = enumerator1.Current;
                    if (user_stickers.Contains(current))
                        intList1.Add(current);
                }
            }
            finally
            {
                enumerator1.Dispose();
            }
            var enumerator2 = user_stickers.GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    var current = enumerator2.Current;
                    if (!intList1.Contains(current))
                        intList1.Add(current);
                }
            }
            finally
            {
                enumerator2.Dispose();
            }
            return intList1;
        }
        */

        public string PrepareTextForLookup(string text)
        {
            if (text.EndsWith("  "))
                return "";
            text = text.ToLowerInvariant();
            text = text.Replace("ั‘", "ะต");
            text = text.Replace(" ", "");
            return text;
        }

        public IEnumerable<VKSticker> GetAutoSuggestItemsFor(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || (text.Length > StickersAutoSuggestDictionary.MAX_TEXT_LENGTH || this._isRestoringState) || !this._isLoaded)
                return Enumerable.Empty<VKSticker>();
            List<VKSticker> stickersAutoSuggestItemList = new List<VKSticker>();
            StickerKeywordItem stickerKeywordItem;
            if (this._lookupDictionary.TryGetValue(text, out stickerKeywordItem))
            {
                var enumerator1 = stickerKeywordItem.user_stickers.GetEnumerator();
                try
                {
                    while (enumerator1.MoveNext())
                    {
                        VKSticker current = enumerator1.Current;
                        stickersAutoSuggestItemList.Add(current);
                    }
                }
                finally
                {
                    enumerator1.Dispose();
                }

                var enumerator2 = stickerKeywordItem.promoted_stickers.GetEnumerator();

                try
                {
                    while (enumerator2.MoveNext())
                    {
                        VKSticker current = enumerator2.Current;
                        current.is_allowed = false;
                        stickersAutoSuggestItemList.Add(current);
                    }
                }
                finally
                {
                    enumerator2.Dispose();
                }
            }
            return Enumerable.Take<VKSticker>(stickersAutoSuggestItemList, StickersAutoSuggestDictionary.MAX_SUGGEST_STICKERS_COUNT);
        }

        public void EnsureDictIsLoadedAndUpToDate(bool forceReload = false)
        {
            if ((Settings.UserId == 0 || this._isLoading ? false : (forceReload ? true : ((DateTime.UtcNow - this._lastLoadedDate).TotalHours >= StickersAutoSuggestDictionary.REFRESH_AFTER_HOURS ? true : false))) == false)
                return;

            if (!this._isCacheChecked)
            {
                if (!this._isRestoringState)
                {
                    this.RestoreStateAsync((result) =>
                    {
                        if (!result)
                        {
                            this.EnsureDictIsLoadedAndUpToDate(true);
                        }
                    });

                    return;
                }
            }

            this._isLoading = true;
            StoreService.Instance.GetStickersKeywords((res =>
            {
                this._isLoading = false;

                if (res.error.error_code== Enums.VKErrors.None)
                {
                    this.SetKeywordsData(res.response);
                    this._lastLoadedDate = DateTime.UtcNow;
                    this._isLoaded = true;
                }
                else
                {
                    if (forceReload)
                        this._lastLoadedDate = DateTime.MinValue;
                }
            }));
        }
        
        private bool _isCacheChecked;

        /*
        public async void RestoreStateAsync()
        {
            this._isRestoringState = true;
            var temp = await CacheManager2.ReadTextFromFile<StickersAutoSuggestDictionary>(StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME);
            if (temp != null)
            {
                this._keywordsData = temp._keywordsData;
                this._lastLoadedDate = temp._lastLoadedDate;
                this._lookupDictionary = temp._lookupDictionary;
            }
            this._isRestoringState = false;
            this._isCacheChecked = true;
        }
        
        public async Task SaveState()
        {
            await CacheManager2.WriteTextToFile(StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME, this);
        }
        */

        public async void RestoreStateAsync(Action<bool> callback)
        {
            this._isRestoringState = true;
            bool result = await CacheManager.TryDeserializeAsync(this, StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME);
            this._isRestoringState = false;
            this._isCacheChecked = true;
            callback(result);
        }

        public void SaveState()
        {
            CacheManager.TrySerialize(this, StickersAutoSuggestDictionary.AUTO_SUGGEST_FILE_NAME);
        }

        public void Clear()
        {
            this._keywordsData = null;
            _instance = null;
        }

        private void SetKeywordsData(StickersKeywordsData keywordsData)
        {
            this._keywordsData = keywordsData;
            this.BuildDictionary();
        }

        private void BuildDictionary()
        {
            if (this._keywordsData == null)
                return;
            this._lookupDictionary.Clear();
            List<StickerKeywordItem>.Enumerator enumerator1 = this._keywordsData.dictionary.GetEnumerator();
            try
            {
                while (enumerator1.MoveNext())
                {
                    StickerKeywordItem current = enumerator1.Current;
                    List<string>.Enumerator enumerator2 = current.words.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            string key = this.PrepareTextForLookup(enumerator2.Current);
                            if (!this._lookupDictionary.ContainsKey(key))
                                this._lookupDictionary.Add(key, current);
                        }
                    }
                    finally
                    {
                        enumerator2.Dispose();
                    }
                }
            }
            finally
            {
                enumerator1.Dispose();
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this._isLoaded);
            BinarySerializerExtensions.Write(writer, this._lastLoadedDate);
            writer.Write<StickersKeywordsData>(this._keywordsData, false);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._isLoaded = reader.ReadBoolean();
            this._lastLoadedDate = BinarySerializerExtensions.ReadDateTime(reader);
            this.SetKeywordsData(reader.ReadGeneric<StickersKeywordsData>());
        }
    }
}
