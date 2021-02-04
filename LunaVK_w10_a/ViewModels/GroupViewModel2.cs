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
using Windows.ApplicationModel.Calls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LunaVK.ViewModels
{
    public class GroupViewModel2 : GenericCollectionViewModel<VKWallPost>
    {
        public uint Id { get; private set; }
        public VKGroup Group { get; private set; }
        DelayedExecutor _de = new DelayedExecutor(100);

        public ProfileInfoFullViewModel CompactinfoViewModel { get; private set; }
        //public ProfileInfoFullViewModel fullInfoViewModel { get; private set; }

        public AllProfilePostsToggleViewModel PostsToggleViewModel { get; private set; }
        public SuggestedPostponedPostsViewModel SuggestedPostponedViewModel { get; private set; }

        public ProfileMediaViewModelFacade MediaViewModel { get; private set; }

        public GroupViewModel2(uint gid)
        {
            this.Id = gid;
            base.LoadCount = 5;
            base.ReloadCount = 20;

            this.CompactinfoViewModel = new ProfileInfoFullViewModel();
            this.MediaViewModel = new ProfileMediaViewModelFacade();
            //this.Commands = new ObservableCollection<CommandVM>();
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            this.CompactinfoViewModel.InfoSections.Clear();
            //this.MediaViewModel = new ProfileMediaViewModelFacade();
            this.Group = null;
            base.NotifyPropertyChanged(nameof(this.GroupCoverImageVisibility));
            this.PostsToggleViewModel = null;
        }

        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            if (this.Group == null)
            {
                string code = "var g = API.groups.getById({group_id:" + this.Id + ",fields:\"description,photo_100,verified,activity,cover,counters,can_message,member_status,status,members_count,site,can_post,is_subscribed,is_favorite,start_date,finish_date,city,country,action_button,main_section,live_covers,online_status,contacts,links,phone,wall,wiki_page\"})[0];";
                code += "var sd=0;var pd=0;var lc=null; var m=null;var ms=0;";
                code += "if(g.deactivated!=\"deleted\")";
                code += "{";
                code +=     "var mutualIds = API.groups.getMembers({group_id:" + this.Id + ", count:10,filter:\"friends\"});";
                code +=     "var mutualUsers = API.users.get({user_ids:mutualIds.items,fields:\"photo_50\"});";
                code +=     "g.friends=[];g.friends.items=mutualUsers;g.friends.count=mutualIds.count;";

                code +=     "g.contactsUsers = API.users.get({user_ids:g.contacts@.user_id,fields:\"photo_100\"});";

                code +=     "if(g.is_admin==1)";
                code +=     "{";
                //code +=         "var s=API.wall.get({owner_id:-" + this.Id + ",count:1,filter:\"suggests\"});sd=s.count;";//предложенные

                code +=      "if(g.type==\"page\")";
                code +=       "{";
                code +=         "ms=API.messages.getConversations({count:0,filter:\"unread\",group_id:"+ this.Id + "}).count;";
                code +=        "}";
                code +=         "var p=API.wall.get({owner_id:-" + this.Id + ",count:1,filter:\"postponed\"});pd=p.count;";//отложенные
                code +=     "}";

                //code += "if(g.is_admin==1)";
                //code += "{";
                //code += "}";

                code +=     "var s=API.wall.get({owner_id:-" + this.Id + ",count:1,filter:\"suggests\"});sd=s.count;";//предложенные
                code +=     "if(g.live_covers.is_enabled==true)";
                code +=     "{";
                code +=         "lc = API.stories.getById({stories:g.live_covers.story_ids[0]}).items[0];";
                code +=     "}";
                code +=     "if(g.market.enabled==1)";
                code +=     "{";
                code +=         "m=API.market.get({owner_id:-g.id,count:10});";
                code +=     "}";
                code += "}";
                code += "return {group:g,suggested:sd,postponed:pd,live_cover:lc,market:m,msgs:ms};";

                //RequestsDispatcher.GetResponseFromDump<GroupResponse>(1000,"club36338110.json", (result) => 
                VKRequestsDispatcher.Execute<GroupResponse>(code, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this.Group = result.response.group;

                        if (offset == 0)
                        {
                            if (result.response.live_cover != null && result.response.live_cover.type == "video")
                            {
                                var filse = result.response.live_cover.video.files;
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

                            if (this.Group.member_status == VKGroupMembershipType.Member || this.Group.is_closed == VKGroupIsClosed.Opened && this.Group.wall != 0)
                                this._de.AddToDelayedExecution(this.LoadWall);

                            base.NotifyPropertyChanged(nameof(this.Title));
                            base.NotifyPropertyChanged(nameof(this.CoverImageUrl));
                            base.NotifyPropertyChanged(nameof(this.GroupCoverImageVisibility));
                            base.NotifyPropertyChanged(nameof(this.RightOffsetVisibility));
                            base.NotifyPropertyChanged(nameof(this.AvatarBackPanelVisibility));
                            base.NotifyPropertyChanged(nameof(this.Avatar));
                            base.NotifyPropertyChanged(nameof(this.GroupText));
                            base.NotifyPropertyChanged(nameof(this.IsVerifiedVisibility));
                            base.NotifyPropertyChanged(nameof(this.IsFavorite));
                            base.NotifyPropertyChanged(nameof(this.NotificationsVisibility));

                            base.NotifyPropertyChanged(nameof(this.TextButtonPrimary));
                            base.NotifyPropertyChanged(nameof(this.TextButtonSecondary));
                            base.NotifyPropertyChanged(nameof(this.VisibilityButtonSendMessage));
                            base.NotifyPropertyChanged(nameof(this.VisibilityButtonSecondary));

                            base.NotifyPropertyChanged(nameof(this.SubscribeGlyph));
                            base.NotifyPropertyChanged(nameof(this.SubscribeText));
                            base.NotifyPropertyChanged(nameof(this.FavoriteGlyph));
                            base.NotifyPropertyChanged(nameof(this.FavoriteText));

                            base.NotifyPropertyChanged(nameof(this.IsOnlineVisibility));
                            base.NotifyPropertyChanged(nameof(this.OnlineText));
                            base.NotifyPropertyChanged(nameof(this.OnlineDescription));
                            base.NotifyPropertyChanged(nameof(this.OnlineTextColor));

                            base.NotifyPropertyChanged(nameof(this.IsSubscribed));
                            base.NotifyPropertyChanged(nameof(this.AddPostText));
                            base.NotifyPropertyChanged(nameof(this.CanPostVisibility));

                            base.NotifyPropertyChanged(nameof(this.FullInfoRight));

                            base.NotifyPropertyChanged(nameof(this.WikiPageVisibility));
                            base.NotifyPropertyChanged(nameof(this.WikiPageText));
                            base.NotifyPropertyChanged(nameof(this.ConversationsButtonVisibility));

                            this._msgsCount = (uint)result.response.msgs;
                            base.NotifyPropertyChanged(nameof(this.VisibilityMessages));
                            base.NotifyPropertyChanged(nameof(this.CountString));

                            Execute.ExecuteOnUIThread(() =>
                            {
                                this.SuggestedPostponedViewModel = new SuggestedPostponedPostsViewModel((int)-this.Id, result.response.suggested, result.response.postponed);
                                base.NotifyPropertyChanged(nameof(this.SuggestedPostponedViewModel));
                                this.CreateData();
                                this.MediaViewModel.PreInit(this.Group);

                                if (result.response.market != null)
                                {
                                    this.MediaViewModel.Init(result.response.market);
                                }
                            });
                        }
                    }

                    callback(result.error, null);

                    base._totalCount = 0;//вынуждаем не загружать данные автоматически, ведь оно же потом вручную заведётся от AddToDelayedExecution :)
                    base.NotifyPropertyChanged(nameof(base.FooterText));
                }, (jsonStr) =>
                {
                    jsonStr = jsonStr.Replace("\"action_type\":\"\"", "\"action_type\":\"none\"");
                    jsonStr = VKRequestsDispatcher.FixArrayToObject(jsonStr, "target");
                    jsonStr = VKRequestsDispatcher.FixNull(jsonStr, "market");
                    jsonStr = VKRequestsDispatcher.FixNull(jsonStr, "msgs");
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }
            else
            {
                //if (this.Group.member_status == VKGroupMembershipType.Member || this.Group.is_closed == VKGroupIsClosed.Opened && this.Group.wall != 0)
                    this.LoadWall();
            }
        }

        private void LoadWall()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loading);
            });

            string filter = "all";
            if (this.PostsToggleViewModel != null && !this.PostsToggleViewModel.IsAllPosts)
                filter = "owner";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = (-this.Id).ToString();
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

                                item.IsRepost = true;
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

                        p._deletedItemCallback = () => { this.DeleteWallItem(p); };

                        base.Items.Add(p);
                    }

                    base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                    base.CurrentLoadingStatus = ProfileLoadingStatus.Loaded;





                    if (this.PostsToggleViewModel == null)
                    {
                        AllProfilePostsToggleViewModel postsToggleViewModel = new AllProfilePostsToggleViewModel(this.Group, (int)base._totalCount, this.Group.name);

                        postsToggleViewModel.StateChangedCallback = this.StateChanged;
                        this.PostsToggleViewModel = postsToggleViewModel;
                        base.NotifyPropertyChanged(nameof(this.PostsToggleViewModel));
                        this.PostsToggleViewModel.UpdateState(base.CurrentLoadingStatus);
                    }
                });
            });
        }

        private void StateChanged(bool isAllPosts)
        {
            this.Items.Clear();
            this.LoadWall();
        }

        public Visibility IsSubscribed
        {
            get
            {
                if (this.Group != null)
                    return this.Group.is_subscribed.ToVisiblity();
                return Visibility.Collapsed;
            }
        }

        public bool IsFavorite
        {
            get
            {
                if (this.Group != null)
                    return this.Group.is_favorite;
                return false;
            }
        }

        public string TextButtonPrimary
        {
            get
            {
                if (this.Group != null)
                {
                    if (this.Group.can_message == true)
                        return LocalizedStrings.GetString("Group_SendAMessage");
                }

                return "";
            }
        }

        public Visibility VisibilityButtonSendMessage
        {
            get
            {
                if (this.Group != null)
                {
                    if (this.Group.can_message == false)
                        return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public Visibility ConversationsButtonVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Collapsed;
//#if DEBUG

                return (this.Group.IsAdminOrHigher && this.Group.type != VKGroupType.Group).ToVisiblity();
//#else
//                return Visibility.Collapsed;
//#endif
            }
        }

        private uint _msgsCount = 0;

        public Visibility VisibilityMessages
        {
            get { return (this._msgsCount > 0).ToVisiblity(); }
        }

        public string CountString
        {
            get { return this._msgsCount.ToString(); }
        }

        #region ButtonSecondary
        /// <summary>
        /// Видимость вторичной кнопки
        /// </summary>
        public Visibility VisibilityButtonSecondary
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Collapsed;

                if (this.Group.Deactivated == VKIsDeactivated.Deleted)
                    return Visibility.Collapsed;

                if (this.Group.action_button != null)
                {
                    return Visibility.Visible;
                }
                /*
                //Мы были кем-то приглашены
                if (this.Group.invited_by != 0)
                    return Visibility.Visible;

                //Мы не состоим в группе и мы забанены или группа приватная
                if (this.Group.member_status == VKGroupMembershipType.NotAMember && (this.Group.is_closed == VKGroupIsClosed.Private || this.Group.ban_info != null && this.Group.ban_info.end_date == 0))
                    return Visibility.Collapsed;
                    */
                return Visibility.Collapsed;
            }
        }

        public string TextButtonSecondary
        {
            get
            {
                if (this.Group == null)
                    return "";

                if (this.Group.action_button != null)
                {
                    return this.Group.action_button.title;
                }
                return "";
                /*
                //TextButtonSecondaryAction
                switch (this.Group.member_status)
                {
                    case VKGroupMembershipType.Member:
                        switch (this.Group.type)
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
                            if (this.Group.is_closed != VKGroupIsClosed.Opened)
                                return LocalizedStrings.GetString("GroupPage_SendRequest");
                            switch (this.Group.type)
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
                */
            }
        }
#endregion

        public string SubscribeText
        {
            get
            {
                if (this.Group != null)
                {
                    switch (this.Group.member_status)
                    {
                        case VKGroupMembershipType.Member:
                            switch (this.Group.type)
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
                                if (this.Group.is_closed != VKGroupIsClosed.Opened)
                                    return LocalizedStrings.GetString("GroupPage_SendRequest");
                                switch (this.Group.type)
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

        public string SubscribeGlyph
        {
            get
            {
                if (this.Group != null)
                {
                    switch (this.Group.member_status)
                    {
                        case VKGroupMembershipType.Member:
                            return "\xE8FB";
                        //case VKGroupMembershipType.NotSure:
                        //    return "Group_MayAttend");
                        //case VKGroupMembershipType.RequestSent:
                        //    return "Profile_RequestSent");
                        case VKGroupMembershipType.InvitationReceived:
                        case VKGroupMembershipType.NotAMember:
                            {
                                return "\xE710";
                            }
                        default:
                            return "";
                    }
                }
                return "";
            }
        }

        public string FavoriteGlyph
        {
            get
            {
                if (this.Group != null)
                {
                    return this.Group.is_favorite ? "\xE735" : "\xE734";
                }
                return "";
            }
        }

        public string FavoriteText
        {
            get
            {
                if (this.Group != null)
                {
                    return this.Group.is_favorite ? "В избранном" : "Избранное";
                }
                return "";
            }
        }

        private string _liveCoverLink;

        /// <summary>
        /// Ссылка на живую обложку
        /// </summary>
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

        /// <summary>
        /// Обложка
        /// </summary>
        public BitmapImage CoverImageUrl
        {
            get
            {
                if (this.Group == null)
                    return null;

                if (this.Group.cover != null && this.Group.cover.enabled == true)
                {
                    BitmapImage bimg = new BitmapImage(new Uri(this.Group.cover.CurrentImage));
                    return bimg;
                }

                return null;
            }
        }

        public Visibility IsVerifiedVisibility
        {
            get
            {
                if (this.Group == null || !this.Group.IsVerified)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility GroupCoverImageVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Collapsed;

                //if (this.Group.live_covers != null && this.Group.live_covers.is_enabled)
                //    return Visibility.Collapsed;
                if (!string.IsNullOrEmpty(this.LiveCoverLink))
                    return Visibility.Collapsed;

                if (this.Group.cover != null && this.Group.cover.enabled == true)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        public Visibility AvatarBackPanelVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Visible;
                return this.GroupLiveCoverImageVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility RightOffsetVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Visible;
                return this.GroupCoverImageVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility GroupLiveCoverImageVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Collapsed;

                //if (this.Group.live_covers != null && this.Group.live_covers.is_enabled)
                //    return Visibility.Visible;
                if (!string.IsNullOrEmpty(this.LiveCoverLink))
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }

        public Visibility NotificationsVisibility
        {
            get
            {
                if (this.Group == null)
                    return Visibility.Collapsed;

                if(this.Group.is_closed != VKGroupIsClosed.Opened && this.Group.member_status != VKGroupMembershipType.Member)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string Title
        {
            get
            {
                if (this.Group == null)
                    return "";
                return this.Group.name;
            }
        }
        /*
        public string GroupTypeStr
        {
            get
            {
                if (this.Group == null)
                    return "";
                switch (this.Group.type)
                {
                    case VKGroupType.Group:
                        {
                            //return GroupHeader.GetGroupTypeText(this.Group).Capitalize();
                            switch (this.Group.is_closed)
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
        */
        public string GroupText
        {
            get
            {
                if (this.Group == null)
                    return "";

                if (!string.IsNullOrEmpty(this.Group.status))
                    return this.Group.status;
                //if (this.Group.type == VKGroupType.Page || this.Group.type == VKGroupType.Group)
                //    return this.Group.status;

                if (this.Group.type == VKGroupType.Event)
                {
                    string str1 = "";
                    if (this.Group.start_date == null)
                        return "";
                    string str2 = str1 + UIStringFormatterHelper.FormateDateForEventUI(this.Group.start_date);
                    if (this.Group.finish_date != null)
                        str2 = str2 + " — " + UIStringFormatterHelper.FormateDateForEventUI(this.Group.finish_date);
                    if (this.Group.place != null)
                    {
                        if (!string.IsNullOrEmpty(this.Group.place.title))
                            str2 = str2 + ", " + this.Group.place.title;
                        if (!string.IsNullOrEmpty(this.Group.place.address))
                            str2 = str2 + ", " + this.Group.place.address;
                        if (!string.IsNullOrEmpty(this.Group.place.city))
                            str2 = str2 + ", " + this.Group.place.city;
                        if (!string.IsNullOrEmpty(this.Group.place.country))
                            str2 = str2 + ", " + this.Group.place.country;
                    }
                    return str2;
                }

                switch (this.Group.type)
                {
                    case VKGroupType.Group:
                        {
                            //return GroupHeader.GetGroupTypeText(this.Group).Capitalize();
                            switch (this.Group.is_closed)
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

        public BitmapImage Avatar
        {
            get
            {
                if (this.Group == null)
                    return null;
                return new BitmapImage(new Uri(this.Group.photo_100));
            }
        }

        public Visibility IsOnlineVisibility
        {
            get
            {
                if (this.Group == null || this.Group.online_status == null)
                    return Visibility.Collapsed;

                if (this.Group.online_status.status == "none")
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string OnlineText
        {
            get
            {
                if (this.Group == null)
                    return "";

                if (this.Group.online_status != null)
                {
                    if (this.Group.online_status.status == "online")
                        return "Сообщество онлайн";
                    else if (this.Group.online_status.status == "answer_mark")
                        return "Быстро отвечает";
                }
                return "";
            }
        }

        public string OnlineDescription
        {
            get
            {
                if (this.Group == null)
                    return "";

                if (this.Group.online_status != null)
                {
                    if (this.Group.online_status.status == "answer_mark")
                        return " · время ответа - " + this.Group.online_status.minutes + " минут";
                    else if (this.Group.online_status.status == "online")
                        return " · ответят прямо сейчас";
                }
                return "";
            }
        }

        public SolidColorBrush OnlineTextColor
        {
            get
            {
                if (this.Group == null || this.Group.online_status == null)
                    return null;

                return (SolidColorBrush)Application.Current.Resources[this.Group.online_status.status == "online" ? "VKColorBrushGreen" : "PhoneAccentColorBrush"];
            }
        }

        private void CreateData()
        {
            this.CompactinfoViewModel.InfoSections.Clear();
            
            if ((!string.IsNullOrEmpty(this.Group.status)) || (int)this.Group.admin_level > 1)
            {
                var item = new CustomProfileInfoItem("\xEC42", string.IsNullOrEmpty(this.Group.status) ? LocalizedStrings.GetString("ChangeStatusText") : this.Group.status);
                
                if ((int)this.Group.admin_level > 1)
                {
                    item.NavigationAction = () => { this.OpenSetStatusPopup(this.Group.status, this.Id, this.UpdateData); };
                }
                this.CompactinfoViewModel.InfoSections.Add(item);
            }
            
            if (!string.IsNullOrEmpty(this.Group.description))
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xEA37", this.Group.description));//"ProfilePage_Info_Description"
            
            if (this.Group.members_count > 0)
            {
                var infoListItem = new InfoListItem("\xE701", this.ComposeInlinesForMembers) { NavigationAction = () => { NavigatorImpl.Instance.NavigateToCommunitySubscribers(this.Id, this.Group.type); } };

                List<VKUser> friends = this.Group.friends.items;
                if (friends != null)
                {
                    List<string> l = new List<string>();
                    foreach (var f in friends)
                        l.Add(f.photo_50);

                    infoListItem.Previews = l;
                }

                this.CompactinfoViewModel.InfoSections.Add(infoListItem);
            }
            
            if (this.Group.start_date > DateTime.MinValue && this.Group.type == VKGroupType.Event)
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xED5A", UIStringFormatterHelper.FormatDateTimeForUI(this.Group.start_date)));//"ProfilePage_Info_StartDate"

            //if (this.Group.finish_date > DateTime.MinValue)
            //    this.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_FinishDate", UIStringFormatterHelper.FormatDateTimeForUI(this.Group.finish_date)));

            string description = "";
            if (this.Group.place != null && !string.IsNullOrEmpty(this.Group.place.address))
                description = this.Group.place.address;
            if (this.Group.city != null && !string.IsNullOrEmpty(this.Group.city.title))
            {
                if (!string.IsNullOrEmpty(description))
                    description += ", ";
                description += this.Group.city.title;
            }
            if (this.Group.country != null && !string.IsNullOrEmpty(this.Group.country.title))
            {
                if (!string.IsNullOrEmpty(description))
                    description += ", ";
                description += this.Group.country.title;
            }
            if (!string.IsNullOrEmpty(description))
            {
                //Action navigationAction = null;
                //if (this.Group.place != null && this.Group.place.latitude != 0.0 && this.Group.place.longitude != 0.0)
                //    navigationAction = (Action)(() => Navigator.Current.NavigateToMap(false, this.Group.place.latitude, this.Group.place.longitude));
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE819", description));//"ProfilePage_Info_Location"
            }
            
            if (!string.IsNullOrEmpty(this.Group.site))
            {
                Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(this.Group.site));

                if(this.Group.site.Contains(" "))
                    NavigationAction = null;
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE774", this.Group.site) { NavigationAction = NavigationAction });
            }

            if(!string.IsNullOrEmpty( this.Group.phone))
            {
                Action callAction = ( () => { PhoneCallManager.ShowPhoneCallUI(this.Group.phone, this.Title); });
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE717", this.Group.phone) { NavigationAction = callAction });
            }

            if(this.Group.is_closed != VKGroupIsClosed.Opened)
            {
                this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE72E", "Это закрытая группа") );
            }
        }

        private string ComposeInlinesForMembers
        {
            get
            {
                bool flag = this.Group.type == VKGroupType.Page;

                string ret = UIStringFormatterHelper.CountForUI(this.Group.members_count) + " " + UIStringFormatterHelper.FormatNumberOfSomething(this.Group.members_count, flag ? "OneSubscriberFrm" : "OneMemberFrm", flag ? "TwoFourSubscribersFrm" : "TwoFourMembersFrm", flag ? "FiveSubscribersFrm" : "FiveMembersFrm", false);

                if (this.Group.friends.count > 0)
                {

                    ret += " · ";
                    string str5 = UIStringFormatterHelper.FormatNumberOfSomething((int)this.Group.friends.count, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                    ret += str5;
                }

                return ret;
            }
        }

        public void GroupJoin(bool? notSure)
        {
            GroupsService.Instance.Join(this.Id, notSure, (result) =>
            {
                if (result == true)
                {
                    if (this.Group.is_closed != VKGroupIsClosed.Opened)
                    {
                        this.Group.member_status = VKGroupMembershipType.RequestSent;
                        base.NotifyPropertyChanged(nameof(this.SubscribeGlyph));
                        base.NotifyPropertyChanged(nameof(this.SubscribeText));
                        return;
                    }

                    if (this.Group.type == VKGroupType.Event)
                    {
                        if (notSure != null)
                        {
                            this.Group.member_status = notSure.Value ? VKGroupMembershipType.NotSure : VKGroupMembershipType.Member;
                        }
                        else
                        {
                            this.Group.member_status = VKGroupMembershipType.Member;
                        }
                    }
                    else
                    {
                        this.Group.member_status = VKGroupMembershipType.Member;
                    }
                    base.NotifyPropertyChanged(nameof(this.SubscribeGlyph));
                    base.NotifyPropertyChanged(nameof(this.SubscribeText));
                }
            });
        }

        public void GroupLeave()
        {
            GroupsService.Instance.Leave(this.Id, (result) =>
            {
                if (result == true)
                {
                    this.Group.member_status = VKGroupMembershipType.NotAMember;
                    base.NotifyPropertyChanged(nameof(this.SubscribeGlyph));
                    base.NotifyPropertyChanged(nameof(this.SubscribeText));
                }
            });
        }

        public void SubscribeUnsubscribe(Action<bool> callback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = (-this.Id).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(this.Group.is_subscribed ? "wall.unsubscribe" : "wall.subscribe", parameters, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.Group.is_subscribed = !this.Group.is_subscribed;
                        base.NotifyPropertyChanged(nameof(this.IsSubscribed));
                    }
                    callback?.Invoke(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });
        }

        public void FaveUnfave(Action<bool> callback = null)
        {
            FavoritesService.Instance.FaveAddRemoveGroup(this.Id, !this.Group.is_favorite, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.Group.is_favorite = !this.Group.is_favorite;
                        base.NotifyPropertyChanged(nameof(this.IsFavorite));
                        base.NotifyPropertyChanged(nameof(this.FavoriteGlyph));
                        base.NotifyPropertyChanged(nameof(this.FavoriteText));
                    }

                    callback?.Invoke(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });
        }

        public string UserPhoto
        {
            get { return Settings.LoggedInUserPhoto; }
        }

        public bool CanPost
        {
            get
            {
                if (this.Group != null)
                    return this.Group.can_post;
                return false;
            }
        }

        public bool CanSuggestAPost
        {
            get
            {
                if (this.Group != null && !this.CanPost)
                    return this.Group.type == VKGroupType.Page;

                return false;
            }
        }

        public string AddPostText
        {
            get
            {
                if (this.Group == null)
                    return "";

                if (this.CanPost)
                    return LocalizedStrings.GetString("MainPage_News_AddNews");
                else if (this.CanSuggestAPost)
                    return LocalizedStrings.GetString("SuggestedNews_SuggestAPost");

                return "";
            }
        }

        public Visibility CanPostVisibility
        {
            get
            {
                if (this.CanPost || this.CanSuggestAPost)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility WikiPageVisibility
        {
            get
            {

                if (this.Group == null || string.IsNullOrEmpty(this.Group.wiki_page))
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public string WikiPageText
        {
            get
            {
                if (this.Group == null || string.IsNullOrEmpty(this.Group.wiki_page))
                    return "";

                return this.Group.wiki_page.ToUpper();
            }
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return LocalizedStrings.GetString("NoWallPosts");
                return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneWallPostFrm", "TwoWallPostsFrm", "FiveWallPostsFrm");
            }
        }

        public ProfileInfoFullViewModel FullInfoRight
        {
            get
            {
                if (this.Group == null)
                    return null;

                return this.GetFullInfoViewModel(false);
            }
        }

        public ProfileInfoFullViewModel GetFullInfoViewModel(bool addGenericInfo)
        {
            ProfileInfoFullViewModel viewModel = new ProfileInfoFullViewModel();
            
#region Основная информация
            if (addGenericInfo)
            {
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE82F", this.Title));

                if ((!string.IsNullOrEmpty(this.Group.status)) || (int)this.Group.admin_level > 1)
                {
                    var item = new CustomProfileInfoItem("\xEC42", string.IsNullOrEmpty(this.Group.status) ? LocalizedStrings.GetString("ChangeStatusText") : this.Group.status);

                    if ((int)this.Group.admin_level > 1)
                    {
                        item.NavigationAction = () => { this.OpenSetStatusPopup(this.Group.status, this.Id, this.UpdateData); };
                    }
                    viewModel.InfoSections.Add(item);
                }

                if (!string.IsNullOrEmpty(this.Group.description))
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Description", this.Group.description));







                if (this.Group.start_date > DateTime.MinValue && this.Group.type == VKGroupType.Event)
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_StartDate", UIStringFormatterHelper.FormatDateTimeForUI(this.Group.start_date)));

                if (this.Group.finish_date > DateTime.MinValue)
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_FinishDate", UIStringFormatterHelper.FormatDateTimeForUI(this.Group.finish_date)));

                string description = "";
                if (this.Group.place != null && !string.IsNullOrEmpty(this.Group.place.address))
                    description = this.Group.place.address;
                if (this.Group.city != null && !string.IsNullOrEmpty(this.Group.city.title))
                {
                    if (!string.IsNullOrEmpty(description))
                        description += ", ";
                    description += this.Group.city.title;
                }
                if (this.Group.country != null && !string.IsNullOrEmpty(this.Group.country.title))
                {
                    if (!string.IsNullOrEmpty(description))
                        description += ", ";
                    description += this.Group.country.title;
                }
                if (!string.IsNullOrEmpty(description))
                {
                    //Action navigationAction = null;
                    //if (this.Group.place != null && this.Group.place.latitude != 0.0 && this.Group.place.longitude != 0.0)
                    //    navigationAction = (Action)(() => Navigator.Current.NavigateToMap(false, this.Group.place.latitude, this.Group.place.longitude));
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Location", description));
                }
            }
#endregion

            //ProfileInfoSectionItem contactsInfo = new ProfileInfoSectionItem("ProfilePage_Info_ContactInformation");

            viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE910", string.IsNullOrEmpty(this.Group.screen_name) ? ("club" + this.Id) : this.Group.screen_name) );

            if (!string.IsNullOrEmpty(this.Group.site))
            {
                Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(this.Group.site));
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE774", this.Group.site) { NavigationAction = NavigationAction });
            }

            if (this.Group.links != null && this.Group.links.Count > 0)
            {
                //ProfileInfoSectionItem linksInfo = new ProfileInfoSectionItem("ProfilePage_Info_Links");
                foreach (var link in this.Group.links)
                {
                    Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(link.url));
                    viewModel.InfoSections.Add(new LinkItem(link.name, link.desc, link.photo_100) { NavigationAction = NavigationAction });
                }

                //viewModel.InfoSections.Add(linksInfo);
            }


            //string _domain = string.IsNullOrEmpty(this.Group.domain) ? string.Format("club{0}", this.Group.id) : this.Group.domain;
            //viewModel.InfoSections.Add(new ProfileInfoItem("", _domain, ProfileInfoItemType.Contact) { Icon = "\xE95F" });

            if (this.Group.contacts != null && this.Group.contacts.Count > 0)
            {
                foreach (var contact in this.Group.contacts)
                {
                    if (contact.user_id > 0)
                    {
                        var user = this.Group.contactsUsers.FirstOrDefault(u => u.id == contact.user_id);

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

                        Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToProfilePage((int)user.id));

                        viewModel.InfoSections.Add(new LinkItem(user.Title, str, user.photo_100) {NavigationAction = NavigationAction });
                    }
                    else if (!string.IsNullOrEmpty(contact.email))
                    {
                        //viewModel.InfoSections.Add(new ProfileInfoItem(contact.phone, contact.email));
                        Action NavigationAction = (async () =>
                        {
                            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
                            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(contact.email));
                            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
                        });
                        viewModel.InfoSections.Add(new LinkItem(string.IsNullOrEmpty( contact.desc) ? contact.phone : contact.desc, contact.email, "https://vk.com/images/contact.png") { NavigationAction = NavigationAction });
                    }
                    else if (!string.IsNullOrEmpty(contact.phone))
                    {
                        Action call = ( () => {
                            PhoneCallManager.ShowPhoneCallUI(contact.phone, this.Title);
                            
                        });
                        viewModel.InfoSections.Add(new LinkItem(contact.desc, contact.phone, "https://vk.com/images/contact.png") { NavigationAction = call });
                    }
                    else
                    {
                        int i = 0;
                    }
                }
            }

            return viewModel;
        }

        public bool CanManageCommunity
        {
            get
            {
                if (this.Group == null)
                    return false;

                return this.Group.is_admin;
            }
        }

        public void PinToStart()
        {
            //if (this._isLoading)
            //    return;
            //this._isLoading = true;
            //string photoMax = this._profileData.PhotoMax;
            //string name = this._profileData.Name;
            this.SetInProgress(true, "");
            string link = "url=https://";
            if (CustomFrame.Instance.IsDevicePhone)
                link += "m.";
            link += "vk.com/club";
            link += this.Id;


            SecondaryTileCreator.CreateTileFor((int)-this.Id, this.Title, this.Group.photo_100, link, (res =>
            {
                this.SetInProgress(false, "");
                //this._isLoading = false;
                Execute.ExecuteOnUIThread((() =>
                {
                    //Action barPropertyUpdated = this.AppBarPropertyUpdated;
                    //if (barPropertyUpdated == null)
                    //    return;
                    //barPropertyUpdated();
                }));
                if (res)
                    return;
                //Execute.ExecuteOnUIThread((() => new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null)));
            }));
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
            this.Group.status = status;
            base.NotifyPropertyChanged(nameof(this.CompactinfoViewModel));
            //base.NotifyPropertyChanged(nameof(this.fullInfoViewModel));
        }

        private void DeleteWallItem(VKWallPost item)
        {
            WallService.Instance.DeletePost(item.OwnerId, item.PostId, (result) =>
            {
                if (result.error.error_code == VKErrors.None && result.response == 1)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.Items.Remove(item);
                        base._totalCount--;
                        base.NotifyPropertyChanged(nameof(base.FooterText));


                        if(this.PostsToggleViewModel != null)
                        {
                            this.PostsToggleViewModel.UpdateAllPostsText(base._totalCount.Value);
                        }
                    });
                }
            });
        }

        public class GroupResponse
        {
            public VKGroup group { get; set; }
            public int suggested { get; set; }
            public int postponed { get; set; }
            public int msgs { get; set; }
            public VKStory live_cover { get; set; }
            public VKCountedItemsObject<VKMarketItem> market { get; set; }
    }
    }
}
