using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    public class GroupViewModel : GenericCollectionViewModel<VKWallPost>
    {
        public uint _gid;
        public VKGroup _group;
        public ObservableCollection<ProfileInfoSectionItem> InfoSections { get; private set; }
        DelayedExecutor _de = new DelayedExecutor(100);
        public Action CoverLoaded;

        /// <summary>
        /// Видимость счётчиков
        /// </summary>
        public Visibility CountersVisibility { get; private set; }

        public ProfileMediaViewModelFacade MediaViewModel { get; private set; }

        public AllProfilePostsToggleViewModel PostsToggleViewModel { get; private set; }

        private SuggestedPostponedPostsViewModel _suggestedPostponedViewModel;
        public SuggestedPostponedPostsViewModel SuggestedPostponedViewModel
        {
            get
            {
                return this._suggestedPostponedViewModel;
            }
            private set
            {
                this._suggestedPostponedViewModel = value;
                base.NotifyPropertyChanged(nameof(this.SuggestedPostponedViewModel));
                //base.NotifyPropertyChanged(nameof(this.SuggestedPostponedVisibility));
            }
        }

        public ObservableCollection<CommandVM> Commands { get; private set; }

        public void UpdateCoverVisibility()
        {
            base.NotifyPropertyChanged(nameof(this.GroupCoverImageVisibility));
            base.NotifyPropertyChanged(nameof(this.GroupLiveCoverImageVisibility));
        }

        public BitmapImage ProfileImageUrl { get; private set; }

        private string _liveCoverLink;
        public string LiveCoverLink
        {
            get
            {
                return this._liveCoverLink;
            }
            private set
            {
                this._liveCoverLink = value;
                base.NotifyPropertyChanged(nameof(this.LiveCoverLink));
                base.NotifyPropertyChanged(nameof(this.GroupLiveCoverImageVisibility));
            }
        }

        public string Avatar
        {
            get
            {
                if (this._group == null)
                    return "";
                return this._group.photo_100;
            }
        }

        public string Title
        {
            get
            {
                if (this._group == null)
                    return "";
                return this._group.name;
            }
        }

        public string GroupTypeStr
        {
            get
            {
                if (this._group == null)
                    return "";
                switch (this._group.type)
                {
                    case VKGroupType.Group:
                        {
                            //return GroupHeader.GetGroupTypeText(this._group).Capitalize();
                            switch (this._group.is_closed)
                            {
                                case VKGroupIsClosed.Opened:
                                    return LocalizedStrings.GetString("PublicGroup");
                                case VKGroupIsClosed.Closed:
                                    return LocalizedStrings.GetString("ClosedGroup");
                                case VKGroupIsClosed.Private:
                                    return LocalizedStrings.GetString("PrivateGroup");
                                default:
                                    return "";
                            }
                        }
                    case VKGroupType.Page:
                        return LocalizedStrings.GetString("PublicPage");
                    case VKGroupType.Event:
                        return LocalizedStrings.GetString("Event");
                    default:
                        return "";
                }
            }
        }

        public string GroupText
        {
            get
            {
                if (this._group == null)
                    return "";

                if (this._group.type == VKGroupType.Page || this._group.type == VKGroupType.Group)
                    return this._group.status;//link

                string str1 = "";
                if (this._group.start_date == null)
                    return "";
                string str2 = str1 + UIStringFormatterHelper.FormateDateForEventUI(this._group.start_date);
                if (this._group.finish_date != null)
                    str2 = str2 + " — " + UIStringFormatterHelper.FormateDateForEventUI(this._group.finish_date);
                if (this._group.place != null)
                {
                    if (!string.IsNullOrEmpty(this._group.place.title))
                        str2 = str2 + ", " + this._group.place.title;
                    if (!string.IsNullOrEmpty(this._group.place.address))
                        str2 = str2 + ", " + this._group.place.address;
                    if (!string.IsNullOrEmpty(this._group.place.city))
                        str2 = str2 + ", " + this._group.place.city;
                    if (!string.IsNullOrEmpty(this._group.place.country))
                        str2 = str2 + ", " + this._group.place.country;
                }
                return str2;
            }
        }

        public string OnlineText
        {
            get
            {
                if (this._group == null)
                    return "";

                if (this._group.online_status != null)
                {
                    if (this._group.online_status.status == "online")
                        return "Online";
                    else if (this._group.online_status.status == "answer_mark")
                        return "Ответит за " + this._group.online_status.minutes + " минут";
                }
                return "";
            }
        }

        public Visibility GroupCoverImageVisibility
        {
            get
            {
                if (this._group == null)
                    return Visibility.Collapsed;

                //if (this._group.live_covers != null && this._group.live_covers.is_enabled)
                //    return Visibility.Collapsed;
                if (!string.IsNullOrEmpty(this.LiveCoverLink))
                    return Visibility.Collapsed;

                if (this._group.cover != null && this._group.cover.enabled == true)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        public Visibility GroupLiveCoverImageVisibility
        {
            get
            {
                //if (this._group == null)
                //     return Visibility.Collapsed;

                //if (this._group.live_covers != null && this._group.live_covers.is_enabled)
                //    return Visibility.Visible;
                if (!string.IsNullOrEmpty(this.LiveCoverLink))
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Обложка
        /// </summary>
        public BitmapImage CoverImageUrl
        {
            get
            {
                if (this._group == null)
                    return null;

                if (this._group.cover != null && this._group.cover.enabled == true)
                {
                    BitmapImage bimg = new BitmapImage(new Uri(this._group.cover.CurrentImage));
                    return bimg;
                }

                return null;
            }
        }

        public Visibility IsVerifiedVisibility
        {
            get
            {
                if (this._group == null || !this._group.IsVerified)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsOnlineVisibility
        {
            get
            {
                if (this._group == null || this._group.online_status == null)
                    return Visibility.Collapsed;

                if (this._group.online_status.status == "none")
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }


        public GroupViewModel(uint gid/*, string name*/)
        {
            this._gid = gid;
            base.LoadCount = 5;
            base.ReloadCount = 20;

            this.InfoSections = new ObservableCollection<ProfileInfoSectionItem>();
            this.MediaViewModel = new ProfileMediaViewModelFacade();
            this.Commands = new ObservableCollection<CommandVM>();
        }

        public class GroupResponse
        {
            public VKGroup group { get; set; }
            public int suggested { get; set; }
            public int postponed { get; set; }
            public VKStory live_cover { get; set; }
        }
        /*
        private int _postponedPostsCount;
        public int PostponedPostsCount
        {
            get
            {
                return this._postponedPostsCount;
            }
            set
            {
                this._postponedPostsCount = value;
                base.NotifyPropertyChanged(nameof(this.PostponedPostsCount));
            }
        }

        private int _suggestedPostsCount;
        public int SuggestedPostsCount
        {
            get
            {
                return this._suggestedPostsCount;
            }
            set
            {
                this._suggestedPostsCount = value;
                base.NotifyPropertyChanged(nameof(this.SuggestedPostsCount));
            }
        }
        */



        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            if (this._group == null || (offset == 0 && this._group != null))
            {
                string code = "var g = API.groups.getById({group_id:" + this._gid + ",fields:\"description,photo_100,verified,activity,cover,counters,can_message,member_status,status,members_count,site,can_post,is_subscribed,is_favorite,crop_photo,start_date,finish_date,city,country,action_button,main_section,live_covers,online_status\"})[0];";
                code += "var sd=0;var pd=0;var lc=null;";
                code += "if(g.deactivated!=\"deleted\")";
                code += "{";
                code +=     "var mutualIds = API.groups.getMembers({group_id:" + this._gid + ", count:10,filter:\"friends\"});";
                code +=     "var mutualUsers = API.users.get({user_ids:mutualIds.items,fields:\"photo_50\"});";
                code +=     "g.friends=[];g.friends.items=mutualUsers;g.friends.count=mutualIds.count;";
                code +=     "if(g.is_admin==1)";
                code +=     "{";
                code +=         "var s=API.wall.get({owner_id:-" + this._gid + ",count:1,filter:\"suggests\"});sd=s.count;";//предложенные
                code +=         "var p=API.wall.get({owner_id:-" + this._gid + ",count:1,filter:\"postponed\"});pd=p.count;";//отложенные
                code +=     "}";
                code +=     "if(g.live_covers.is_enabled==true)";
                code +=     "{";
                code +=         "lc = API.stories.getById({stories:g.live_covers.story_ids[0]}).items[0];";
                code +=     "}";
                code += "}";
                code += "return {group:g,suggested:sd,postponed:pd,live_cover:lc};";

                VKRequestsDispatcher.Execute<GroupResponse>(code, ((res) =>
                {
                    if (res.error.error_code == VKErrors.None)
                    {
                        this._group = res.response.group;
                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.ReadData();
                            this.CreateData();
                            this.MediaViewModel.PreInit(this._group);
                            //
                            base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loading);
                            base.CurrentLoadingStatus = ProfileLoadingStatus.Loading;

                            if (offset == 0)
                            {
                                this.SuggestedPostponedViewModel = new SuggestedPostponedPostsViewModel((int)-this._gid, res.response.suggested, res.response.postponed);
                                if (res.response.live_cover != null && res.response.live_cover.type == "video")
                                {
                                    var filse = res.response.live_cover.video.files;
                                    string link = filse.mp4_720;
                                    if (string.IsNullOrEmpty(link))
                                        link = filse.mp4_480;
                                    if (string.IsNullOrEmpty(link))
                                        link = filse.mp4_360;
                                    if (string.IsNullOrEmpty(link))
                                        link = filse.mp4_240;
                                    if (!string.IsNullOrEmpty(link))
                                        this.LiveCoverLink = link;
                                }

                                //Нет кроп фото, но ковер то надо показать!
                                if (this._group.crop_photo != null)
                                    this.UpdateProfilePhoto(_group.crop_photo.photo.photo_604, _group.crop_photo.crop);
                                else
                                    this.UpdateProfilePhoto(_group.photo_100, null);
                            }
                        });
                        this._de.AddToDelayedExecution(this.LoadOnlyWall);
                    }
                    callback(res.error.error_code, null);
                }), (jsonStr) =>
                {
                    jsonStr = jsonStr.Replace("\"action_type\":\"\"", "\"action_type\":\"none\"");
                    jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "target");
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }
            else
            {
                this.LoadOnlyWall();
            }
        }

        private void UpdateProfilePhoto(string uri, CropPhoto.CropRect crop)
        {
            //if (for_group)
            //{
            //    if (this.GroupData.cover != null && this.GroupData.cover.enabled == true)
            //        this.GroupCoverImageVisibility = Visibility.Visible;

            //}

            this.UpdateCoverVisibility();

            BitmapImage bimg = new BitmapImage();
            if (crop == null)
                crop = new CropPhoto.CropRect() { x = 0, y = 0, x2 = 100, y2 = 100 };//BugFix: люди не указали кроп :(


            Task.Run(async () =>
            {
                try
                {
                    var refernce = RandomAccessStreamReference.CreateFromUri(new Uri(uri));
                    using (IRandomAccessStream fileStream = await refernce.OpenReadAsync())
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                        // create a new stream and encoder for the new image
                        InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                        BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

                        uint height = decoder.PixelHeight;
                        uint width = decoder.PixelWidth;

                        BitmapBounds bounds = new BitmapBounds();
                        bounds.Height = (uint)((crop.y2 - crop.y) * height / 100);
                        bounds.Width = (uint)((crop.x2 - crop.x) * width / 100);
                        bounds.X = (uint)(crop.x * width / 100);
                        bounds.Y = (uint)(crop.y * height / 100);

                        enc.BitmapTransform.Bounds = bounds;

                        // write out to the stream
                        {
                            await enc.FlushAsync();


                            // render the stream to the screen
                            //await Task.Delay(4000);
                            Execute.ExecuteOnUIThread(() =>
                            {
                                bimg.SetSource(ras);
                                this.CoverLoaded?.Invoke();
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            });

            this.ProfileImageUrl = bimg;

            base.NotifyPropertyChanged(nameof(this.ProfileImageUrl));


        }

        private void ReadData()
        {
            base.NotifyPropertyChanged<string>(() => this.Avatar);
            base.NotifyPropertyChanged<string>(() => this.GroupTypeStr);
            base.NotifyPropertyChanged<string>(() => this.GroupText);
            base.NotifyPropertyChanged<Visibility>(() => this.IsVerifiedVisibility);
            //base.NotifyPropertyChanged<bool>(() => this.IsFavorite);
            //base.NotifyPropertyChanged<bool>(() => this.IsSubscribed);
            //base.NotifyPropertyChanged<string>(() => this.GroupName);
            //        base.NotifyPropertyChanged<Visibility>(() => this.AllVSGroupPostsVisibility);
            //        base.NotifyPropertyChanged<Visibility>(() => this.AllVSGroupPostsVisibilityInversed);
            base.NotifyPropertyChanged(nameof(this.Title));
            /*
            this.ActionButtonsSeparatorVisibility = 0;
            this.ReadActionButtons();
            this.ReadNavigateButtons();
            this.ReadInfoRows();
            this.GroupMembershipInfo = new GroupMembershipInfo(null);
            */



            base.NotifyPropertyChanged(nameof(this.CoverImageUrl));
            base.NotifyPropertyChanged(nameof(this.GroupCoverImageVisibility));
            //base.NotifyPropertyChanged(nameof(this.GroupLiveCoverImageVisibility));
            base.NotifyPropertyChanged(nameof(this.TextButtonPrimary));
            base.NotifyPropertyChanged(nameof(this.TextButtonSecondary));
            base.NotifyPropertyChanged(nameof(this.VisibilityButtonSendMessage));
            base.NotifyPropertyChanged(nameof(this.VisibilityButtonSecondary));
            base.NotifyPropertyChanged(nameof(this.VisibilityButtonThird));

            base.NotifyPropertyChanged(nameof(this.IsOnlineVisibility));
            base.NotifyPropertyChanged(nameof(this.OnlineText));






        }

        private void StateChanged(bool isAllPosts)
        {
            this.Items.Clear();
            this.LoadOnlyWall();
        }

        private void CreateData()
        {
            this.InfoSections.Clear();


#region Основная информация
            ProfileInfoSectionItem genericInfo = new ProfileInfoSectionItem("");

            if ((!string.IsNullOrEmpty(this._group.status)/* && this._group.type != VKGroupType.Page*/) || (int)this._group.admin_level > 1)
            {
                var item = new ProfileInfoItem("", string.IsNullOrEmpty(this._group.status) ? LocalizedStrings.GetString("ChangeStatusText") : this._group.status, ProfileInfoItemType.Contact);
                item.Icon = "\xEC42";
                if ((int)this._group.admin_level > 1)
                {
                    item.NavigationAction = () => { this.OpenSetStatusPopup(this._group.status, this._gid, this.UpdateData); };
                }
                genericInfo.Items.Add(item);
            }

            if (!string.IsNullOrEmpty(this._group.description))
                genericInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_Description", this._group.description));

            if (this._group.members_count > 0)
            {
                ProfileInfoItem infoListItem = new ProfileInfoItem("", this.ComposeInlinesForMembers(), ProfileInfoItemType.Previews)
                {
                    Icon = "\xE701",//E716 - People
                    NavigationAction = () => { NavigatorImpl.Instance.NavigateToCommunitySubscribers(this._gid, this._group.type); }
                };

                List<VKUser> friends = this._group.friends.items;
                if (friends != null)
                {
                    List<string> l = new List<string>();
                    foreach (var f in friends)
                        l.Add(f.photo_50);

                    infoListItem.Previews = l;
                    genericInfo.Items.Add(infoListItem);
                }
            }






            if (this._group.start_date > DateTime.MinValue && this._group.type == VKGroupType.Event)
                genericInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_StartDate", UIStringFormatterHelper.FormatDateTimeForUI(this._group.start_date)));

            if (this._group.finish_date > DateTime.MinValue)
                genericInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_FinishDate", UIStringFormatterHelper.FormatDateTimeForUI(this._group.finish_date)));

            string description = "";
            if (this._group.place != null && !string.IsNullOrEmpty(this._group.place.address))
                description = this._group.place.address;
            if (this._group.city != null && !string.IsNullOrEmpty(this._group.city.title))
            {
                if (!string.IsNullOrEmpty(description))
                    description += ", ";
                description += this._group.city.title;
            }
            if (this._group.country != null && !string.IsNullOrEmpty(this._group.country.title))
            {
                if (!string.IsNullOrEmpty(description))
                    description += ", ";
                description += this._group.country.title;
            }
            if (!string.IsNullOrEmpty(description))
            {
                //Action navigationAction = null;
                //if (this._group.place != null && this._group.place.latitude != 0.0 && this._group.place.longitude != 0.0)
                //    navigationAction = (Action)(() => Navigator.Current.NavigateToMap(false, this._group.place.latitude, this._group.place.longitude));
                genericInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_Location", description));
            }

            if (genericInfo.Items.Count > 0)
                this.InfoSections.Add(genericInfo);
            #endregion

            ProfileInfoSectionItem contactsInfo = new ProfileInfoSectionItem("ProfilePage_Info_ContactInformation");



            if (!string.IsNullOrEmpty(this._group.site))
            {
                Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(this._group.site));
                contactsInfo.Items.Add(new ProfileInfoItem("", this._group.site, ProfileInfoItemType.Contact) { Icon = "\xE774", NavigationAction = NavigationAction });
            }

            if (contactsInfo.Items.Count > 0)
                this.InfoSections.Add(contactsInfo);



            if (this._group.action_button != null)
            {
                //                    this.AppViewModel = new ProfileAppViewModel(this.Id, this._group.action_button);
            }


        }

        private string ComposeInlinesForMembers()
        {
            bool flag = this._group.type == VKGroupType.Page;

            string ret = UIStringFormatterHelper.CountForUI(this._group.members_count) + " " + UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, flag ? "OneSubscriberFrm" : "OneMemberFrm", flag ? "TwoFourSubscribersFrm" : "TwoFourMembersFrm", flag ? "FiveSubscribersFrm" : "FiveMembersFrm", false);

            if (this._group.friends.count > 0)
            {

                ret += " · ";
                string str5 = UIStringFormatterHelper.FormatNumberOfSomething((int)this._group.friends.count, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                ret += str5;
            }

            return ret;
        }

        private void LoadOnlyWall()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loading);
            });

            string filter = "all";
            if (this.PostsToggleViewModel != null && !this.PostsToggleViewModel.IsAllPosts)
                filter = "owner";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = (-this._gid).ToString();
            parameters["offset"] = base.Items.Count.ToString();
            parameters["count"] = "20";
            parameters["extended"] = "1";
            parameters["filter"] = filter;

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKWallPost>>("wall.get", parameters, (result) =>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                        base.CurrentLoadingStatus = ProfileLoadingStatus.LoadingFailed;
                    });
                    return;
                }

                base._totalCount = result.response.count;
                Execute.ExecuteOnUIThread(() =>
                {
                    foreach (VKWallPost p in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (p.from_id != 0)
                        {
                            if (p.from_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.from_id);
                        }
                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            for (int i = 0; i < p.copy_history.Count; i++)
                            {
                                VKWallPost item = p.copy_history[i];
                                if (item.owner_id < 0 && result.response.groups != null)
                                    item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                                else
                                    item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                                
                                item.IsFooterHiden = true;

                                if (p.attachments == null)
                                    p.attachments = new List<VKAttachment>();
                                p.attachments.Add(new VKAttachment() { wall = item, type = VKAttachmentType.Wall });
                            }
                        }

                        if (p.signer_id != 0)
                        {
                            if (p.signer_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.signer_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.signer_id);
                            p.Signer = owner;
                        }

                        //                        p._deletedItemCallback = () => { this.DeleteWallItem(p); };

                        base.Items.Add(p);
                    }

                    base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                    base.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;





                    if (this.PostsToggleViewModel == null)
                    {
                        AllProfilePostsToggleViewModel postsToggleViewModel = new AllProfilePostsToggleViewModel(this._group, (int)base._totalCount, this._group.name);

                        postsToggleViewModel.StateChangedCallback = this.StateChanged;
                        this.PostsToggleViewModel = postsToggleViewModel;
                        base.NotifyPropertyChanged(nameof(this.PostsToggleViewModel));
                        this.PostsToggleViewModel.UpdateState(base.CurrentLoadingStatus);
                    }
                });
            });
        }

        public string TextButtonPrimary
        {
            get
            {
                if (this._group != null)
                {
                    if (this._group.can_message == true)
                        return LocalizedStrings.GetString("Group_SendAMessage");
                }

                return "";
            }
        }

        public Visibility VisibilityButtonSendMessage
        {
            get
            {
                if (this._group != null)
                {
                    if (this._group.can_message == false)
                        return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

#region ButtonSecondary
        /// <summary>
        /// Видимость вторичной кнопки
        /// </summary>
        public Visibility VisibilityButtonSecondary
        {
            get
            {
                if (this._group != null)
                {
                    if (this._group.Deactivated == VKIsDeactivated.Deleted)
                        return Visibility.Collapsed;
                }

                if (this._group != null)
                {
                    //Мы были кем-то приглашены
                    if (this._group.invited_by != 0)
                        return Visibility.Visible;

                    //Мы не состоим в группе и мы забанены или группа приватная
                    if (this._group.member_status == VKGroupMembershipType.NotAMember && (this._group.is_closed == VKGroupIsClosed.Private || this._group.ban_info != null && this._group.ban_info.end_date == 0))
                        return Visibility.Collapsed;

                    //if (!this._group.CanJoin || this._group.member_status == VKGroupMembershipType.RequestSent)//это не нужно
                    //    return Visibility.Collapsed;


                    //if (this._group.type != VKGroupType.Event || this._group.start_date == 0)
                    //    return Visibility.Collapsed;
                    //if (!(UIStringFormatterHelper.UnixTimeStampToDateTime((double)Math.Max(this._group.start_date, this._group.finish_date), true) > DateTime.Now))
                    //    return Visibility.Visible;
                }

                return Visibility.Visible;
            }
        }

        public string TextButtonSecondary
        {
            get
            {
                if (this._group != null)
                {
                    if (this._group.action_button != null)
                    {
                        return this._group.action_button.title;
                    }

                    //TextButtonSecondaryAction
                    switch (this._group.member_status)
                    {
                        case VKGroupMembershipType.Member:
                            switch (this._group.type)
                            {
                                case VKGroupType.Group:
                                    return LocalizedStrings.GetString("Group_Joined");
                                case VKGroupType.Page:
                                    return LocalizedStrings.GetString("Group_Following");
                                case VKGroupType.Event:
                                    return LocalizedStrings.GetString("Group_Attending");//Вы пойдёте
                                default:
                                    return "";
                            }
                        case VKGroupMembershipType.NotSure:
                            return LocalizedStrings.GetString("Group_MayAttend");
                        case VKGroupMembershipType.RequestSent:
                            return LocalizedStrings.GetString("Profile_RequestSent");
                        case VKGroupMembershipType.InvitationReceived:
                        case VKGroupMembershipType.NotAMember:
                            {
                                if (this._group.is_closed != VKGroupIsClosed.Opened)
                                    return LocalizedStrings.GetString("GroupPage_SendRequest");
                                switch (this._group.type)
                                {
                                    case VKGroupType.Group:
                                        return LocalizedStrings.GetString("Group_Join");
                                    case VKGroupType.Page:
                                        return LocalizedStrings.GetString("Profile_Follow");
                                    case VKGroupType.Event:
                                        return LocalizedStrings.GetString("Event_Join");// + "...";
                                    default:
                                        return "";
                                }
                            }
                        default:
                            return "";
                    }
                }

                return "";
            }
        }
#endregion

        public Visibility VisibilityButtonThird
        {
            get
            {
                if (this._group != null)
                {
                    if (this._group.action_button != null)
                        return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public string ButtonThirdGlyph
        {
            get
            {
                if (this._group != null)
                {
                    switch (this._group.member_status)
                    {
                        case VKGroupMembershipType.Member:
                            return "\xE911";
                        //case VKGroupMembershipType.NotSure:
                        //    return "Group_MayAttend");
                        //case VKGroupMembershipType.RequestSent:
                        //    return "Profile_RequestSent");
                        case VKGroupMembershipType.InvitationReceived:
                        case VKGroupMembershipType.NotAMember:
                            {
                                return "\xE1B7";
                            }
                        default:
                            return "";
                    }
                }
                return "";
            }
        }

        public void ShowFullInfoPopup(Action<bool> callback)
        {
            GroupsService.Instance.GetGroupInfo(this._gid, false, (result) =>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        callback(false);
                    });
                    return;
                }

                VKGroup group = result.response;
                this._group.links = group.links;
                this._group.contacts = group.contacts;
                this._group.contactsUsers = group.contactsUsers;


                Execute.ExecuteOnUIThread(() =>
                {
                    callback(true);
                    this.UpdateInfoData();
                });
            });


        }

        private void UpdateInfoData()
        {
            if (this._group.links != null && this._group.links.Count > 0)
            {
                ProfileInfoSectionItem linksInfo = new ProfileInfoSectionItem("ProfilePage_Info_Links");
                foreach (var link in this._group.links)
                {
                    Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(link.url));
                    linksInfo.Items.Add(new ProfileInfoItem(link.name, link.desc, ProfileInfoItemType.Full) { photo = link.photo_100, NavigationAction = NavigationAction });
                }

                this.InfoSections.Add(linksInfo);
            }

            ProfileInfoSectionItem contactsInfo = new ProfileInfoSectionItem("ProfilePage_Info_Contacts");

            string _domain = string.IsNullOrEmpty(this._group.domain) ? string.Format("club{0}", this._group.id) : this._group.domain;
            contactsInfo.Items.Add(new ProfileInfoItem("", _domain, ProfileInfoItemType.Contact) { Icon = "\xE94B" });

            if (this._group.contacts != null && this._group.contacts.Count > 0)
            {

                //this._group.contactsUsers
                foreach (var contact in this._group.contacts)
                {
                    if (contact.user_id > 0)
                    {
                        var user = this._group.contactsUsers.FirstOrDefault(u => u.id == contact.user_id);

                        if (user == null)
                            continue;

                        string str = contact.phone ?? "";
                        if (!string.IsNullOrEmpty(contact.email))
                        {
                            if (!string.IsNullOrEmpty(str))
                                str += "\n";
                            str += contact.email;
                        }

                        if (string.IsNullOrEmpty(str))
                            str = contact.desc;

                        Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToProfilePage(user.Id));

                        contactsInfo.Items.Add(new ProfileInfoItem(user.Title, str, ProfileInfoItemType.Full) { photo = user.photo_100, NavigationAction = NavigationAction });
                    }
                    else if (!string.IsNullOrEmpty(contact.email))
                    {
                        //contactsInfo.Items.Add(new ProfileInfoItem(contact.phone, contact.email));
                        Action NavigationAction = (async () =>
                        {
                            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(contact.email));
                            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
                        });
                        contactsInfo.Items.Add(new ProfileInfoItem(contact.desc, contact.email, ProfileInfoItemType.Full) { photo = "https://vk.com/images/contact.png", NavigationAction = NavigationAction });
                    }
                    else
                    {
                        int i = 0;
                    }
                }

                this.InfoSections.Add(contactsInfo);
            }
        }

        public void GroupJoin(bool? notSure)
        {
            GroupsService.Instance.Join(this._gid, notSure, (result) =>
            {
                if (result == true)
                {
                    if (this._group.is_closed != VKGroupIsClosed.Opened)
                    {
                        this._group.member_status = VKGroupMembershipType.RequestSent;
                        base.NotifyPropertyChanged("TextButtonSecondary");
                        base.NotifyPropertyChanged("ButtonThirdGlyph");
                        return;
                    }

                    if (this._group.type == VKGroupType.Event)
                    {
                        if (notSure != null)
                        {
                            this._group.member_status = notSure.Value ? VKGroupMembershipType.NotSure : VKGroupMembershipType.Member;
                        }
                        else
                        {
                            this._group.member_status = VKGroupMembershipType.Member;
                        }
                    }
                    else
                    {
                        this._group.member_status = VKGroupMembershipType.Member;
                    }
                    base.NotifyPropertyChanged("TextButtonSecondary");
                    base.NotifyPropertyChanged("ButtonThirdGlyph");
                }

            });
        }

        public void GroupLeave()
        {
            GroupsService.Instance.Leave(this._gid, (result) =>
            {
                if (result == true)
                {
                    this._group.member_status = VKGroupMembershipType.NotAMember;
                    base.NotifyPropertyChanged("TextButtonSecondary");
                    base.NotifyPropertyChanged("ButtonThirdGlyph");
                }
            });
        }

        public bool CanPost
        {
            get
            {
                //if (this.IsServiceProfile)
                //    return false;

                if (this._group.Deactivated > 0)
                    return false;

                return this._group.can_post;
            }
        }

        public bool CanSuggestAPost
        {
            get
            {
                if (!this.CanPost)
                    return this._group.type == VKGroupType.Page;
                return false;
            }
        }

        public bool CanManageCommunity
        {
            get
            {
                if (this._group == null)
                    return false;

                return this._group.is_admin;
            }
        }

        public bool CanSubscribeUnsubscribe
        {
            get
            {
                if (this._group.Deactivated > 0)
                    return false;

                //if (this._profileData == null || this.Id == Settings.UserId || this.IsServiceProfile)
                //    return false;

                if (!this._group.is_member && this._group.is_closed != VKGroupIsClosed.Opened)
                    return false;

                if (this._group.ban_info != null && this._group.ban_info.end_date == 0)
                    return false;

                return true;
            }
        }

        public bool IsSubscribed
        {
            get
            {
                if (this._group != null)
                    return this._group.is_subscribed;
                return false;
            }
            set
            {
                if (this._group == null)
                    return;
                this._group.is_subscribed = value;
            }
        }

        public bool CanFaveUnfave
        {
            get
            {
                return true;
            }
        }

        public bool IsFavorite
        {
            get
            {
                if (this._group != null)
                    return this._group.is_favorite;
                return false;
            }
            set
            {
                if (this._group == null)
                    return;
                this._group.is_favorite = value;
                //Action barPropertyUpdated = this.AppBarPropertyUpdated;
                //if (barPropertyUpdated == null)
                //    return;
                //barPropertyUpdated();
            }
        }

        public bool CanBanUnban
        {
            get
            {
                //    if (this._group != null && this.Id > 0 && (this.Id != Settings.UserId && !this.IsServiceProfile))
                //        return true;//this._profileData.Deactivated == VKIsDeactivated.None;
                return false;
            }
        }

        public void SubscribeUnsubscribe(Action<bool> callback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = (-this._gid).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(this._group.is_subscribed ? "wall.unsubscribe" : "wall.subscribe", parameters, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.IsSubscribed = !this.IsSubscribed;
                    }
                    callback(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });
        }

        public void FaveUnfave(Action<bool> callback = null)
        {
            Action<VKResponse<int>> action = new Action<VKResponse<int>>((result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.IsFavorite = !this.IsFavorite;
                    }

                    callback(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });

            FavoritesService.Instance.FaveAddRemoveGroup(this._gid, !this._group.is_favorite, action);
        }

        public void OpenProfilePhotos()
        {
            if (base.CurrentLoadingStatus == ProfileLoadingStatus.Banned || base.CurrentLoadingStatus == ProfileLoadingStatus.Deleted)
                return;
            NavigatorImpl.Instance.NavigateToImageViewer("profile", ImageViewerViewModel.AlbumType.ProfilePhotos, -(int)this._gid, 0, 0, new List<VKPhoto>(), null);
        }

        public void NavigateToNewWallPost()
        {
            if (this.CanSuggestAPost)
                return;//BUG: надо доделать страницу с предложением новости
            //public void NavigateToNewWallPost(long userOrGroupId = 0, bool isGroup = false, int adminLevel = 0, bool isPublicPage = false, bool isNewTopicMode = false, bool isPostponed = false)
            NavigatorImpl.Instance.NavigateToNewWallPost( WallPostViewModel.Mode.NewWallPost, this._group.Id, this._group.admin_level, this._group.type == VKGroupType.Page);
        }

        public void OpenSetStatusPopup(string status, uint groupId, Action<string> callback = null)
        {
            EditStatusUC editStatusUC = new EditStatusUC();
            editStatusUC.TextBoxText.Text = status;
            PopUpService statusChangePopup = new PopUpService
            {
                Child = editStatusUC
            };

            Action updateStatusAction = delegate
            {
                string newStatus = editStatusUC.TextBoxText.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");

                AccountService.Instance.StatusSet(newStatus, "", groupId, (res) =>
                {
                    if (res.error.error_code == VKErrors.None)
                    {
                        if (callback != null)
                        {
                            callback.Invoke(newStatus);
                        }
                    }
                });


                statusChangePopup.Hide();
            };

            editStatusUC.TextBoxText.KeyUp += (delegate (object sender, KeyRoutedEventArgs args)
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    updateStatusAction.Invoke();
                }
            });
            editStatusUC.ButtonSave.Click += (delegate (object s, RoutedEventArgs e)
            {
                updateStatusAction.Invoke();
            });
            statusChangePopup.Show();
        }

        private void UpdateData(string status)
        {
            this._group.status = status;
            base.NotifyPropertyChanged(nameof(this.InfoSections));
        }

        public class CommandVM
        {
            public string Icon { get; set; }
            public string Title { get; set; }
            public Action Callback;

            public CommandVM(string icon, string title, Action callback = null)
            {
                this.Title = title;
                this.Icon = icon;
                this.Callback = callback;
            }
        }
    }
}
