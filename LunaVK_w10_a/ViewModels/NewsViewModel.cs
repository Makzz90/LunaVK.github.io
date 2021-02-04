using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LunaVK.Core.DataObjects;
using System.Threading.Tasks;

using LunaVK.Core.Network;
using LunaVK.Core.Library;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.UserProfile;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core;
using LunaVK.Common;
using System.IO;
using LunaVK.Core.Utils;
using System.Linq;

namespace LunaVK.ViewModels
{
    public class NewsViewModel : GenericCollectionViewModel<VKNewsfeedPost>, IBinarySerializable//, ISupportCache
    {
#region DATA
        public ObservableCollection<NewStory> Stories { get; private set; }

        public int NewsSource { get; set; }
        //public bool InterestingFirst { get; set; }
        public Visibility StoryVisible
        {
            get { return this.Stories.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility StoryTitleVisible
        {
            get { return this.Stories.Count > 6 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string UserPhoto
        {
            get { return Settings.LoggedInUserPhoto; }
        }

        public double _scrollPosition;
        private DateTime _lastFreshNewsCheckDateTime = DateTime.MinValue;
        private DateTime _lastNewsFeedUpdateDateTime = DateTime.MinValue;

        #endregion


        private bool _isLoadingFreshNews;

        public NewsViewModel()
        {
            this.Stories = new ObservableCollection<NewStory>();
        }

        public void SetNewsSource(int value, bool need_reload = true)
        {
            this.NewsSource = value;
            if (need_reload)
            {
                //this.ReloadData();
                //this.LoadDownAsync(true);
                base.Reload();
            }
        }

        public Action<FreshNewsState> FreshNewsStateChangedCallback;

        private FreshNewsState _freshNewsState;
        public FreshNewsState FreshNewsState
        {
            get
            {
                return this._freshNewsState;
            }
            set
            {
                this._freshNewsState = value;
                this.FreshNewsStateChangedCallback?.Invoke(value);
            }
        }

        //
        public override void OnRefresh()
        {
            base.OnRefresh();


            this.Stories.Clear();
            base.NotifyPropertyChanged(nameof(this.StoryVisible));
            base.NotifyPropertyChanged(nameof(this.StoryTitleVisible));
        }
        //

        public void ReloadData()
        {
            base._nextFrom = "";
            this.Items.Clear();
            this.Stories.Clear();
            base.NotifyPropertyChanged(nameof(this.StoryVisible));
            base.NotifyPropertyChanged(nameof(this.StoryTitleVisible));
        }

#region AD
        private string ads_device_id
        {
            get
            {
                try
                {
                    string advertisingId = AdvertisingManager.AdvertisingId;
                    if (!string.IsNullOrWhiteSpace(advertisingId))
                        return advertisingId;
                    return "-3";
                }
                catch (Exception)
                {
                    return "-1";
                }
            }
        }

        private int ads_tracking_disabled
        {
            get
            {
                string adsDeviceId = this.ads_device_id;
                return adsDeviceId == "-3" || adsDeviceId == "-2" ? 1 : 0;
            }
        }

        private string AdvertisingDeviceInfo
        {
            get
            {
                /*
                var clientDeviceInformation = new EasClientDeviceInformation();

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["app_version"] = "0.0.0.8";//версия приложения; 
          //      dictionary["app_build"] = "";//билд приложения; 
                dictionary["manufacturer "] = clientDeviceInformation.SystemManufacturer;//производитель, кроме iOS;
                dictionary["device_model"] = clientDeviceInformation.SystemProductName;//модель устройства; 
                dictionary["system_name"] = clientDeviceInformation.OperatingSystem.Contains("Phone") ? "Windows Mobile" : "Windows";//название системы (iOS, Android); 
                dictionary["system_version"] = clientDeviceInformation.SystemHardwareVersion;//версия системы (6.2, 11.0.2); 
                dictionary["ads_device_id"] = this.ads_device_id;//ID устройства, 
          //      dictionary["ads_android_id "] = "";//Android ID, только для Android; 
                dictionary["ads_tracking_disabled "] = this.ads_tracking_disabled.ToString();//0, если отслеживание разрешено пользователем, 1 — запрещено. 
                return JsonConvert.SerializeObject(dictionary);
                */
                return "{\"app_version\":\"4.13.1\",\"system_name\":\"Android\",\"system_version\":\"6.0\",\"manufacturer\":\"Fly\",\"device_model\":\"FS407\",\"ads_device_id\":\"d30bbfeb - 0fd9 - 4e6c - 96ef - 7229b5424937\",\"ads_tracking_disabled\":1,\"ads_android_id\":\"3da1047301b670fc\"}";
            }
        }

        //com\vkontakte\android\api\newsfeed\NewsfeedGet.java
        /*
        public static String getDeviceInfo() {
        JSONObject devInfo = new JSONObject();
        try {
            devInfo.put("app_version", VKApplication.context.getPackageManager().getPackageInfo(VKApplication.context.getPackageName(), 0).versionName);
            devInfo.put("system_name", "Android");
            devInfo.put("system_version", VERSION.RELEASE);
            devInfo.put("manufacturer", Build.MANUFACTURER);
            devInfo.put("device_model", Build.MODEL);
            String adId = Analytics.getDeviceID();
            if ("-3".equals(adId)) {
                devInfo.put("ads_tracking_disabled", 1);
            } else if (!("-1".equals(adId) || "-2".equals(adId))) {
                devInfo.put("ads_device_id", adId);
                if (Analytics.isLimitAdTrackingEnabled()) {
                    devInfo.put("ads_tracking_disabled", 1);
                }
            }
            if (VKApplication.deviceID != 0) {
                devInfo.put("ads_android_id", Long.toHexString(VKApplication.deviceID));
            }
        } catch (Exception e) {
            C1045L.m230e(e, new Object[0]);
        }
        return devInfo.toString();
    }
    */
    /*
        private async void Test()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = "40";
            parameters["device_info"] = this.AdvertisingDeviceInfo;
            parameters["filters"] = "friends_recomm,ads_post";
            parameters["source_ids"] = "groups,pages";
            parameters["is_newsfeed"] = "1";

            var temp = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKNewsfeedPost>>("newsfeed.get", parameters);
            foreach (var item in temp.response.items)
            {
                if (item.ads != null)
                {
                    int j = 0;
                }
            }
            int i = 0;
        }*/
        #endregion

        //https://vk.com/dev/feed_ads
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKNewsfeedPost>> callback)
        {
            if (offset == 0)
                this._lastFreshNewsCheckDateTime = DateTime.Now;

            string code = "var stories;";
            code += "var news = API." + (this.NewsSource == 1 ? "newsfeed.getRecommended" : "newsfeed.get") + "({fields:\"verified,photo_100\",";

            if (!string.IsNullOrEmpty(base._nextFrom))
            {
                code += ("start_from:\"" + base._nextFrom + "\",");
                code += "count:20,";
            }
            else
            {
                code += "count:10,";
            }
#region SOURCE
            switch (this.NewsSource)
            {
                case 0://news
                    {
                        code += "source_ids:\"groups,pages,friends\",filters:\"post,ads_post\"";
                        //code += ",device_info:";
                        //code += "{}";
                        //code += this.AdvertisingDeviceInfo;
                        break;
                    }
                case 1://recomm
                    {
                        break;
                    }
                case 4://friends
                    {
                        code += "source_ids:\"friends,following\",filters:\"post\",return_banned:1";
                        break;
                    }
                case 2://photos
                    {
                        code += "source_ids:\"friends\",filters:\"photo,wall_photo\"";
                        break;
                    }
                case 3://video
                    {
                        code += "source_ids:\"groups,pages,friends\",filters:\"video\"";
                        break;
                    }
            }
            code += "});";

            if (this.NewsSource == 0 && string.IsNullOrEmpty(base._nextFrom))
                code += "stories = API.stories.get({fields:\"photo_50,photo_100\",extended:1});";
            else
            {

            }

            code += "return {news:news, stories:stories};";
#endregion

            VKRequestsDispatcher.Execute<VKNewsfeedRequestStories>(code, (result) =>
            {

                if (result.error.error_code != VKErrors.None)
                {
                    //                 this.UpdateLoadingStatus(ProfileLoadingStatus.LoadingFailed);
                    callback(result.error, null);
                    return;
                }

                //             this.UpdateLoadingStatus(ProfileLoadingStatus.Loaded);

                base._nextFrom = result.response.news.next_from;

                List<VKGroup> groups = result.response.news.groups;
                List<VKUser> profiles = result.response.news.profiles;

                var stories = result.response.stories;

                if (stories != null && stories.groups != null && stories.groups.Count > 0)
                    groups.AddRange(stories.groups);
                if (stories != null && stories.profiles != null && stories.profiles.Count > 0)
                    profiles.AddRange(result.response.stories.profiles);


                //var items = new List<VKBaseDataForPostOrNews>();

                foreach (VKNewsfeedPost item in result.response.news.items)
                {
                    VKBaseDataForGroupOrUser owner = null;

                    //
                    //if (this.NewsItem.ads == null || this.NewsItem.ads.Count <= 0 || !(this.NewsItem.ads[0].type == "post"))
                    if (item.ads != null && item.ads.Count > 0 || !string.IsNullOrEmpty(item.ads_title))
                    {
                        int i = 0;
                    }
                    //

                    if (item.source_id < 0 && groups != null)
                        owner = groups.Find(ow => ow.id == (-item.source_id));
                    else
                        owner = profiles.Find(ow => ow.id == item.source_id);

                    item.Owner = owner;

                    if (item.copy_history != null)
                    {
                        for (int i = 0; i < item.copy_history.Count; i++)
                        {
                            VKNewsfeedPost j = item.copy_history[i];
                            if (j.owner_id < 0 && groups != null)
                                j.Owner = groups.Find(ow => ow.id == (-j.owner_id));
                            else
                                j.Owner = profiles.Find(ow => ow.id == j.owner_id);

                            j.IsRepost = true;
                            j.IsFooterHiden = true;

                            if (item.attachments == null)
                                item.attachments = new List<VKAttachment>();
                            item.attachments.Add(new VKAttachment() { newsfeed_post = j, type = VKAttachmentType.Repost });
                        }
                    }

                    if (item.type == VKNewsfeedFilters.video)
                    {
                        if (item.video != null)
                        {
                            if (item.attachments == null)
                                item.attachments = new List<VKAttachment>();
                            foreach (var video in item.video.items)
                                item.attachments.Add(new VKAttachment() { video = video, type = VKAttachmentType.Video });
                        }
                    }

                    if (item.photos != null)
                    {
                        if (item.attachments == null)
                            item.attachments = new List<VKAttachment>();
                        foreach (var photo in item.photos.items)
                            item.attachments.Add(new VKAttachment() { photo = photo, type = VKAttachmentType.Photo });
                    }

                    // для виртуализации
                    if (item.geo != null)
                    {
                        if (item.attachments == null)
                            item.attachments = new List<VKAttachment>();
                        item.attachments.Add(new VKAttachment() { geo = item.geo, type = VKAttachmentType.Geo });
                    }

                    if (item.signer_id != 0)
                    {
                        if (item.signer_id < 0 && groups != null)
                            owner = groups.Find(ow => ow.id == (-item.signer_id));
                        else
                            owner = profiles.Find(ow => ow.id == item.signer_id);
                        item.Signer = owner;
                    }

                    //


                    item.IgnoreNewsfeedItemCallback = () => { this.IgnoreNewsFeedItem(item); };
                    item.HideSourceItemsCallback = () => { this.HideSourceItemsCallback(item); };
                    item._deletedItemCallback = () => { this.DeletedCallback(item); };
                }


                callback(new VKError(), result.response.news.items);


                if (stories != null && stories.items != null && stories.items.Count > 0)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        foreach (var item in stories.items)
                        {

                            if (item.stories.Count > 0)
                            {
                                NewStory s = new NewStory();

                                var firstStory = item.stories[0];
                                VKBaseDataForGroupOrUser owner = null;
                                if (firstStory.owner_id < 0)
                                    owner = groups.Find(ow => ow.id == (-firstStory.owner_id));
                                else
                                    owner = profiles.Find(ow => ow.id == firstStory.owner_id);
                                s.Owner = owner;
                                s.Stories = item.stories;
                                this.Stories.Add(s);
                            }
                        }
                    });
                }


                base.NotifyPropertyChanged(nameof(this.StoryVisible));
                base.NotifyPropertyChanged(nameof(this.StoryTitleVisible));


            }, (jsonStr =>
            {
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "stories", true);
                //jsonStr = jsonStr.Replace("\"type\":false", "\"type\":null");
                jsonStr = jsonStr.Replace("\"type\":false", "");

                return jsonStr;
            }
            ));


        }

        public void IgnoreNewsFeedItem(VKNewsfeedPost item)
        {
            //todo:указать правильный тип (фото, видео)
            NewsFeedService.Instance.IgnoreUnignoreItem(true, "wall", item.OwnerId, item.PostId, (result) =>
            {
                if (result == true)
                {
                    Execute.ExecuteOnUIThread(() => {
                        //for (int index = 0; index < this.Items.Count; index++)
                        //{
                        //    var virtualizable = this.Items[index];
                        //    if (virtualizable.OwnerId == item.OwnerId)
                        //        this.Items.Remove(virtualizable);
                        //}
                        this.Items.Remove(item);
                    });
                }
            });
        }

        public void DeletedCallback(VKNewsfeedPost item)
        {
            WallService.Instance.DeletePost(item.OwnerId, item.PostId, (result) =>
            {
                if(result.error.error_code == VKErrors.None && result.response == 1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.Items.Remove(item);
                        base._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.FooterText));
                    });
                }
            });
        }

            public void HideSourceItemsCallback(VKNewsfeedPost item)
        {

            //if (str1 == string.Empty)
            //    NewsFeedService.Current.AddBan(longList1, longList2, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => { }));
            //else
            //    AdsIntService.HideAd(str1, "source", (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>)(res => { }));

            List<uint> longList1 = new List<uint>();
            List<uint> longList2 = new List<uint>();

            int ownerId = item.OwnerId;

            if (ownerId > 0)
                longList1.Add((uint)ownerId);
            else
                longList2.Add((uint)(-ownerId));

            NewsFeedService.Instance.AddDeleteBan(true, longList1, longList2, (result) =>
            {
                if (result == true)
                {
                    Execute.ExecuteOnUIThread(() => {
                        for (int index = 0; index < this.Items.Count; index++)
                        {
                            var virtualizable = this.Items[index];
                            if (virtualizable.OwnerId == item.OwnerId)
                                base.Items.Remove(virtualizable);
                        }
                    });
                }
            });
        }

        /*
        private object OldItems;//public ObservableCollection<VKNewsfeedPost> SearchItems { get; private set; }
        private string searchStartFrom;
        public string q;
        private uint searchMaximum;

        private bool _inSearch;
        public bool InSearch
        {
            get { return this._inSearch; }
            set
            {
                this._inSearch = value;

                this.searchStartFrom = "";

                base.NotifyPropertyChanged("StoryVisible");
                base.NotifyPropertyChanged("StoryTitleVisible");

                if (value)
                {
                    //this.SearchItems = new ObservableCollection<VKNewsfeedPost>();
                    this.OldItems = base.Items;
                    base.Items.Clear();
                }
                else
                {
                    //this.SearchItems = null;
                    base.Items = this.OldItems;
                }
            }
        }

        public void ServerSearch(string text)
        {
            NewsViewModel.Instance.SearchItems.Clear();
            this.q = text;
            this.SearchNext();
        }

        private void SearchNext(Action<VKError, IReadOnlyList<VKBaseDataForPostOrNews>> callback)
        {
            NewsFeedService.Instance.Search(this.q, 15, 0, 0, this.searchStartFrom, (result) => {
                if(result.error.error_code == VKErrors.None)
                {
                    this.searchMaximum = result.response.count;
                    this.searchStartFrom = result.response.next_from;
                    callback(result.error, result.response.items);
                }
                else
                {
                    callback(result.error, null);
                }
            });
        }
        */
        public class NewStory
        {
            public VKBaseDataForGroupOrUser Owner { get; set; }
            public List<VKStory> Stories;

            public bool Seen
            {
                get
                {
                    return this.Stories[0].seen;
                }
            }
        }

        public class VKNewsfeedRequestStories
        {
            public VKCountedItemsObject<GetStory> stories { get; set; }
            

            public VKCountedItemsObject<VKNewsfeedPost> news { get; set; }

            public class GetStory
            {
                public string type { get; set; }//stories

                public List<VKStory> stories { get; set; }
            }
        }

        public void CheckForFreshNewsIfNeeded()
        {
            if (base.CurrentLoadingStatus != ProfileLoadingStatus.Loaded || base.Items.Count == 0)
                return;
            
            double totalSeconds = (DateTime.Now - this._lastFreshNewsCheckDateTime).TotalSeconds;//fresh_news_expiration_timeout
            if (totalSeconds < 360 )
                return;
            this._isLoadingFreshNews = true;
            
            this.CheckForFreshNews((state =>
            {
                if (state.HasValue)
                    this.FreshNewsState = state.Value;
                this._lastFreshNewsCheckDateTime = DateTime.Now;
                this._isLoadingFreshNews = false;
            }));
            
        }

        VKCountedItemsObject<VKNewsfeedPost> _freshNewsData;

        private void CheckForFreshNews( Action<FreshNewsState?> successCallback)
        {
            this._freshNewsData = null;

            string code = "var news = API." + (this.NewsSource == 1 ? "newsfeed.getRecommended" : "newsfeed.get") + "({fields:\"verified,photo_100\",";

            
            code += "count:20,";//fresh_news_load_count : 25

#region SOURCE
            switch (this.NewsSource)
            {
                case 0://news
                    {
                        code += "source_ids:\"groups,pages,friends\",filters:\"post,ads_post\"";
                        //code += ",device_info:";
                        //code += "{}";
                        //code += this.AdvertisingDeviceInfo;
                        break;
                    }
                case 1://recomm
                    {
                        break;
                    }
                case 4://friends
                    {
                        code += "source_ids:\"friends,following\",filters:\"post\",return_banned:1";
                        break;
                    }
                case 2://photos
                    {
                        code += "source_ids:\"friends\",filters:\"photo,wall_photo\"";
                        break;
                    }
                case 3://video
                    {
                        code += "source_ids:\"groups,pages,friends\",filters:\"video\"";
                        break;
                    }
            }
            code += "});";

            

            code += "return {news:news};";
#endregion

            VKRequestsDispatcher.Execute<VKNewsfeedRequestStories>(code, (result) =>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    successCallback(null);
                    return;
                }

                Execute.ExecuteOnUIThread(() =>
                {
                    foreach(var item in result.response.news.items)
                    {
                        bool exists = base.Items.FirstOrDefault((news) => news.PostId == item.PostId && news.OwnerId == item.OwnerId) != null;
                        if (exists)
                            continue;

                        
                            VKBaseDataForGroupOrUser owner = null;
                        List<VKGroup> groups = result.response.news.groups;
                        List<VKUser> profiles = result.response.news.profiles;
                        //
                        //if (this.NewsItem.ads == null || this.NewsItem.ads.Count <= 0 || !(this.NewsItem.ads[0].type == "post"))
                        if (item.ads != null && item.ads.Count > 0 || !string.IsNullOrEmpty(item.ads_title))
                            {
                                int i = 0;
                            }
                            //

                            if (item.source_id < 0 && groups != null)
                                owner = groups.Find(ow => ow.id == (-item.source_id));
                            else
                                owner = profiles.Find(ow => ow.id == item.source_id);

                            item.Owner = owner;

                            if (item.copy_history != null)
                            {
                                for (int i = 0; i < item.copy_history.Count; i++)
                                {
                                    VKNewsfeedPost j = item.copy_history[i];
                                    if (j.owner_id < 0 && groups != null)
                                        j.Owner = groups.Find(ow => ow.id == (-j.owner_id));
                                    else
                                        j.Owner = profiles.Find(ow => ow.id == j.owner_id);

                                    j.IsRepost = true;
                                    j.IsFooterHiden = true;

                                    if (item.attachments == null)
                                        item.attachments = new List<VKAttachment>();
                                    item.attachments.Add(new VKAttachment() { newsfeed_post = j, type = VKAttachmentType.Repost });
                                }
                            }

                            if (item.type == VKNewsfeedFilters.video)
                            {
                                if (item.video != null)
                                {
                                    if (item.attachments == null)
                                        item.attachments = new List<VKAttachment>();
                                    foreach (var video in item.video.items)
                                        item.attachments.Add(new VKAttachment() { video = video, type = VKAttachmentType.Video });
                                }
                            }

                            if (item.photos != null)
                            {
                                if (item.attachments == null)
                                    item.attachments = new List<VKAttachment>();
                                foreach (var photo in item.photos.items)
                                    item.attachments.Add(new VKAttachment() { photo = photo, type = VKAttachmentType.Photo });
                            }

                            // для виртуализации
                            if (item.geo != null)
                            {
                                if (item.attachments == null)
                                    item.attachments = new List<VKAttachment>();
                                item.attachments.Add(new VKAttachment() { geo = item.geo, type = VKAttachmentType.Geo });
                            }

                            if (item.signer_id != 0)
                            {
                                if (item.signer_id < 0 && groups != null)
                                    owner = groups.Find(ow => ow.id == (-item.signer_id));
                                else
                                    owner = profiles.Find(ow => ow.id == item.signer_id);
                                item.Signer = owner;
                            }

                            //


                            item.IgnoreNewsfeedItemCallback = () => { this.IgnoreNewsFeedItem(item); };
                            item.HideSourceItemsCallback = () => { this.HideSourceItemsCallback(item); };
                            item._deletedItemCallback = () => { this.DeletedCallback(item); };
                        

                        if (this._freshNewsData == null)
                        {
                            this._freshNewsData = new VKCountedItemsObject<VKNewsfeedPost>();
                            this._freshNewsData.items = new List<VKNewsfeedPost>();
                        }

                        this._freshNewsData.items.Add(item);
                    }

                    if(this._freshNewsData==null)
                    {
                        successCallback(FreshNewsState.NoNews);
                    }
                    else
                    {
                        if(this._freshNewsData.items.Count == 20)
                        {
                            this._freshNewsData.next_from = result.response.news.next_from;
                            successCallback(FreshNewsState.Reload);
                        }
                        else
                        {
                            successCallback(FreshNewsState.Insert);
                        }
                    }
                });

            });
                    /*
                    NewsFeedService.Instance.GetNewsFeed(newsFeedGetParams2, (result =>
                    {

                    });
                    */
                    
                }
        /*
        private bool IsNewsFeedExpired
        {
            get
            {
                int autoReloadInterval = 0;//this.GetNewsfeedAutoReloadInterval();
                if (autoReloadInterval <= 0)
                    return false;
                return (DateTime.Now - this._lastNewsFeedUpdateDateTime).TotalSeconds >= (double)autoReloadInterval;
            }
        }

        public bool ForceNewsFeedUpdate
        {
            get
            {
                return this.IsNewsFeedExpired;
            }
        }
        */
        public void ReplaceAllWithPendingFreshNews()
        {
            if (this._freshNewsData == null)
                return;
            base.Items.Clear();
            foreach (var item in this._freshNewsData.items)
                base.Items.Add(item);
            //this.NewsFeedVM.TotalCount = 0;
            base._nextFrom = this._freshNewsData.next_from;
            this.FreshNewsState = FreshNewsState.NoNews;
            this._lastNewsFeedUpdateDateTime = DateTime.Now;
        }

        public void InsertFreshNews()
        {
            foreach (var item in this._freshNewsData.items)
                base.Items.Insert(0,item);
            this.FreshNewsState = FreshNewsState.NoNews;
        }

#region IBinarySerializable
        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.WriteList<VKNewsfeedPost>(this.Items.ToList(), 40);
            writer.Write(this.NewsSource);
            writer.Write(this._scrollPosition);
            writer.Write(this._lastFreshNewsCheckDateTime);
            writer.Write(this._lastNewsFeedUpdateDateTime);
            writer.WriteString(base._nextFrom);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            var items = reader.ReadList<VKNewsfeedPost>();
            
            foreach (var item in items)
                base.Items.Add(item);

            this.NewsSource = reader.ReadInt32();
            this._scrollPosition = reader.ReadDouble();
            this._lastFreshNewsCheckDateTime = reader.ReadDateTime();
            this._lastNewsFeedUpdateDateTime = reader.ReadDateTime();
            base._nextFrom = reader.ReadString();
        }
#endregion
    }
}

/*
 * filters
перечисленные через запятую названия списков новостей, которые необходимо получить. В данный момент поддерживаются следующие списки новостей:
•post — новые записи со стен; 
•photo — новые фотографии; 
•photo_tag — новые отметки на фотографиях; 
•wall_photo — новые фотографии на стенах; 
•friend — новые друзья; 
•note — новые заметки; 
•audio — записи сообществ и друзей, содержащие аудиозаписи, а также новые аудиозаписи, добавленные ими; 
•video — новые видеозаписи. 

 * return_banned 1 - включить в выдачу также скрытых из новостей пользователей. 0 - не возвращать скрытых пользователей.  
 * 
 * start_time время в формате unixtime, начиная с которого следует получить новости для текущего пользователя. 

положительное число 

end_time время в формате unixtime, до которого следует получить новости для текущего пользователя. Если параметр не задан, то он считается равным текущему времени. 

положительное число 

max_photos Максимальное количество фотографий, информацию о которых необходимо вернуть. По умолчанию: 5, максимальное значение: 100. 

положительное число 


source_ids перечисленные через запятую иcточники новостей, новости от которых необходимо получить.

Идентификаторы пользователей можно указывать в форматах

<uid> или u<uid>

где <uid> — идентификатор друга пользователя.

Идентификаторы сообществ можно указывать в форматах

-<gid> или g<gid>

где <gid> — идентификатор сообщества. 

Помимо этого параметр может принимать строковые значения:
•friends - список друзей пользователя 
•groups - список групп, на которые подписан текущий пользователь 
•pages - список публичных страниц, на который подписан тeкущий пользователь 
•following - список пользователей, на которых подписан текущий пользователь 
•list{идентификатор списка новостей} - список новостей. Вы можете найти все списки новостей пользователя используя метод newsfeed.getLists 

Если параметр не задан, то считается, что он включает список всех друзей и групп пользователя, за исключением скрытых источников, которые можно получить методом newsfeed.getBanned. 

Максимальное число символов — 5000. 

строка 

start_from Идентификатор, необходимый для получения следующей страницы результатов. Значение, необходимое для передачи в этом параметре, возвращается в поле ответа next_from. 

строка, доступен начиная с версии 5.13 

count указывает, какое максимальное число новостей следует возвращать, но не более 100. По умолчанию 50. 

положительное число 

*/
