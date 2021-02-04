using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class WallPostViewModel : Core.ViewModels.ViewModelBase, IBinarySerializable
    {
        private VKComment _comment;

        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }

        public WallPostViewModel.Mode WMMode
        {
            get
            {
                return this._mode;
            }
            set
            {
                this._mode = value;
                /*
                if (this.CanAddMoreAttachments)
                    return;
                IOutboundAttachment outboundAttachment = (IOutboundAttachment)Enumerable.FirstOrDefault<IOutboundAttachment>(this._outboundAttachmentsWithAdd, (Func<IOutboundAttachment, bool>)(a => a is OutboundAddAttachment));
                if (outboundAttachment == null)
                    return;
                ((Collection<IOutboundAttachment>)this._outboundAttachmentsWithAdd).Remove(outboundAttachment);
            */
                }
        }
        private WallPostViewModel.Mode _mode;

        private bool _editWallRepost;
        public bool FriendsOnly { get; set; }
        private int _userOrGroupId;

        public bool CloseComments { get; set; }
        public bool MuteNotifications { get; set; }
        public bool IsPublishSuggestedSuppressed { get; set; }

        private VKAdminLevel _adminLevel;

        /// <summary>
        /// Базовый конструктор класса WallPostViewModel
        /// </summary>
        public WallPostViewModel()
        {
            this.Attachments = new ObservableCollection<IOutboundAttachment>();
            this.Attachments.CollectionChanged += this._outboundAttachments_CollectionChanged;
            this.Authors = new ObservableCollection<VKBaseDataForGroupOrUser>();
            this.Authors.Add(new VKUser() { id = Settings.UserId, photo_100 = Settings.LoggedInUserPhoto, first_name = Settings.LoggedInUserName });
            /*
#if DEBUG
            if (Debugger.IsAttached)
            {
                this.Authors.Add(new VKGroup() { id = 100, photo_100 = Settings.LoggedInUserPhoto, name = "group", admin_level = VKAdminLevel.Editor, type = VKGroupType.Page });
                this.Authors.Add(new VKGroup() { id = 200, photo_100 = Settings.LoggedInUserPhoto, name = "Test group Editor", admin_level = VKAdminLevel.Admin });
            }
#endif
*/
            this.Author = this.Authors[0];
        }
        

        /// <summary>
        /// Редактирование поста
        /// </summary>
        /// <param name="wallPost"></param>
        /// <param name="adminLevel"></param>
        public WallPostViewModel(VKWallPost wallPost, VKAdminLevel adminLevel) : this()
        {
            this._mode = Mode.EditWallPost;
            this.InitializeWithWallPost(wallPost, adminLevel);
        }

        /// <summary>
        /// Создание поста/обсуждения
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="userOrGroupId"></param>
        public WallPostViewModel(WallPostViewModel.Mode mode, int userOrGroupId, VKAdminLevel adminLevel, bool isPublicPage) : this()
        {
            this._mode = mode;
            this._adminLevel = adminLevel;
            this._isPublicPage = isPublicPage;
            this._userOrGroupId = userOrGroupId;

            if (mode == Mode.NewWallPost && userOrGroupId == 0)
                this.InitGroups();
        }
        
        private bool _isDirty;

        private string _topicTitle;
        public string TopicTitle
        {
            get
            {
                return this._topicTitle;
            }
            set
            {
                bool canPublish = this.CanPublish;
                this._topicTitle = value;
                this._isDirty = true;
                this.NotifyPropertyChanged(nameof(this.TopicTitle));
                if (this.CanPublish == canPublish)
                    return;
                this.NotifyPropertyChanged(nameof(this.CanPublish));
            }
        }

        private string _text = string.Empty;
        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                bool canPublish = this.CanPublish;
                this._text = value;
                this._isDirty = true;
                //this.NotifyPropertyChanged("Text");
                if (this.CanPublish == canPublish)
                    return;
                this.NotifyPropertyChanged(nameof(this.CanPublish));
            }
        }

        public string Title
        {
            get
            {
                switch (this._mode)
                {
                    case WallPostViewModel.Mode.NewWallPost:
                        if (this._isPublicPage && (byte)this._adminLevel < 2)
                            return LocalizedStrings.GetString("SuggestedNews_SuggestAPost");
                        return LocalizedStrings.GetString("NewPost_NewPost");
                    case WallPostViewModel.Mode.EditWallPost:
                        return LocalizedStrings.GetString("EditPost");
                    case WallPostViewModel.Mode.NewWallComment:
                    case WallPostViewModel.Mode.NewPhotoComment:
                    case WallPostViewModel.Mode.NewVideoComment:
                    case WallPostViewModel.Mode.NewDiscussionComment:
                    case WallPostViewModel.Mode.NewProductComment:
                        return UIStringFormatterHelper.FormatNumberOfSomething(this.Attachments.Count, "OneAttachmentFrm", "ManageAttachments_TwoFourAttachmentsFrm", "ManageAttachments_FiveMoreAttachmentsFrm");
                    case WallPostViewModel.Mode.EditWallComment:
                    case WallPostViewModel.Mode.EditPhotoComment:
                    case WallPostViewModel.Mode.EditVideoComment:
                    case WallPostViewModel.Mode.EditDiscussionComment:
                    case WallPostViewModel.Mode.EditProductComment:
                        return LocalizedStrings.GetString("EditComment");
                    case WallPostViewModel.Mode.NewTopic:
                        return LocalizedStrings.GetString("NewTopic");
                    case WallPostViewModel.Mode.PublishWallPost:
                        return LocalizedStrings.GetString("SuggestedNews_Publish");
                    default:
                        return "";
                }
            }
        }

        public void ParseCoordinates(string str, out double latitude, out double longitude)
        {
            latitude = 0.0;
            longitude = 0.0;
            string[] strArray = str.Split(' ');
            if (strArray.Length <= 1)
                return;
            double.TryParse(strArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out latitude);
            double.TryParse(strArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out longitude);
        }

        private void InitializeWithWallPost(VKWallPost wallPost, VKAdminLevel adminLevel)
        {
            //this._editWallRepost = wallPost.IsRepost();
            this._adminLevel = adminLevel;
            //this._wallRepostInfo = wallRepostInfo;
            this.Text = wallPost.text;
            this._userOrGroupId = wallPost.owner_id;
            //this._postId = wallPost.id;
            this.Signature = wallPost.signer_id > 0;
            this.InitializeAttachments(wallPost.attachments);
            if (wallPost.geo != null)
            {
                double latitude;
                double longitude;
                this.ParseCoordinates(wallPost.geo.coordinates,out latitude, out longitude);
                this.Attachments.Add(new OutboundGeoAttachment(latitude, longitude));
            }
            if (wallPost.IsPostponed)
            {
                this.IsPostponed = true;
            }
            if (!wallPost.IsSuggested)
                return;
            //this.IsSuggested = true;
        }

        private void InitializeAttachments(List<VKAttachment> attachments)
        {
            //this._uneditableAttachments.Clear();
            this.Attachments.Clear();
            if (attachments == null)
                return;
            List<VKAttachment>.Enumerator enumerator = attachments.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    VKAttachment current = enumerator.Current;
                    IOutboundAttachment outboundAttachment = null;
                    if (current.photo != null)
                        outboundAttachment = (IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(current.photo, 0, true);
                    if (current.video != null)
                        outboundAttachment = new OutboundVideoAttachment(current.video);
                    //if (current.audio != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundAudioAttachment(current.audio);
                    //if (current.doc != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundDocumentAttachment(current.doc);
                    //if (current.poll != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundPollAttachment(current.poll);
                    //if (current.link != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundLinkAttachment(current.link);
                    //if (current.note != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundNoteAttachment(current.note);
                    //if (current.market_album != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundMarketAlbumAttachment(current.market_album);
                    //if (current.album != null)
                    //    outboundAttachment = (IOutboundAttachment)new OutboundAlbumAttachment(current.album);
                    if (outboundAttachment != null)
                        ((Collection<IOutboundAttachment>)this.Attachments).Add(outboundAttachment);
                    //else
                    //    this._uneditableAttachments.Add(current);
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private void _outboundAttachments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            /*
            if (e.Action == NotifyCollectionChangedAction.Add && !e.NewItems.IsNullOrEmpty())
            {
                IOutboundAttachment outboundAttachment = e.NewItems[0] as IOutboundAttachment;
                outboundAttachment.IsOnPostPage = this.IsOnPostPage;
                this._outboundAttachmentsWithAdd.Insert(e.NewStartingIndex, outboundAttachment);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && !e.OldItems.IsNullOrEmpty())
            {
                this._outboundAttachmentsWithAdd.Remove(e.OldItems[0] as IOutboundAttachment);
                if (e.OldItems[0] is OutboundTimerAttachment)
                    this.FromGroup = false;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
                this._outboundAttachmentsWithAdd.Clear();
            IOutboundAttachment outboundAttachment1 = this._outboundAttachmentsWithAdd.FirstOrDefault<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(o => o is OutboundAddAttachment));
            if (!this.CanAddMoreAttachments && !this.CannAddTimerAttachment)
            {
                if (outboundAttachment1 == null)
                    return;
                this._outboundAttachmentsWithAdd.Remove(outboundAttachment1);
            }
            else
            {
                if (outboundAttachment1 != null)
                    return;
                this._outboundAttachmentsWithAdd.Add((IOutboundAttachment)new OutboundAddAttachment());
            }
            */
            //base.NotifyPropertyChanged(nameof(this.CanPublish));
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                this.UploadAttachments();
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove )
                base.NotifyPropertyChanged(nameof(this.CanPublish));
        }



        public Visibility IsInNewTopicModeVisibility
        {
            get
            {
                if (!this.IsInNewTopicMode)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool IsInNewTopicMode
        {
            get { return this._mode == WallPostViewModel.Mode.NewTopic; }
        }

        public bool IsInNewWallPostMode
        {
            get { return this._mode == WallPostViewModel.Mode.NewWallPost; }
        }

        public Visibility OwnPostVisibility
        {
            get
            {
                if (this._userOrGroupId != 0 || !this.IsInNewWallPostMode)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string TextWatermarkText
        {
            get
            {
                if (this.IsInNewTopicMode)
                    return LocalizedStrings.GetString("NewTopicTextLbl");
                if (this._editWallRepost)
                    return LocalizedStrings.GetString("NewsPage_EnterComment/PlaceholderText");
                return LocalizedStrings.GetString("NewsPage_WhatsNew/Text");
            }
        }
        /*
        public bool FromGroupIsEnabled
        {
            get
            {
                return Enumerable.FirstOrDefault<IOutboundAttachment>(this.Attachments, (a => a.ToString() == "timestamp")) == null;
            }
        }
        */
        public bool Signature { get; set; }
        public bool FromGroup { get; set; }

        /*
        private bool _fromGroup;
        public bool FromGroup
        {
            get
            {
                return this._fromGroup;
            }
            set
            {
                if (this._fromGroup == value)
                    return;
                this._fromGroup = value;
                if (!this._fromGroup)
                    this.Signature = false;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FromGroup));
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.FromGroupIsEnabled));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.SignatureVisibility));
            }
        }
        */
        private bool _isPublicPage;

        public bool IsPostponed { get; set; }

        public Visibility SignatureVisibility
        {
            get
            {
                if ((this._adminLevel <= VKAdminLevel.Moderator || this._userOrGroupId > 0 || !this.IsInNewWallPostMode || /*!this.FromGroup &&*/ !this._isPublicPage) && ((!this.IsPostponed || this._userOrGroupId > 0) && this._mode != WallPostViewModel.Mode.PublishWallPost))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility FromGroupVisibility
        {
            get
            {
                if (((byte)this._adminLevel <= 1 || this._userOrGroupId > 0 || (/*this._isPublicPage ||*/ !this.IsInNewWallPostMode)) && ((byte)this._adminLevel <= 1 || !this.IsInNewTopicMode))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private bool _isPublishing;
        private uint _postId;

        public void Publish(Action<VKError> callback)
        {
            if (!this.CanPublish || this._isPublishing)
                return;
            this._isPublishing = true;
            base.SetInProgress(true);

            int owner_id = this._userOrGroupId;
            double? latitude = new double?();
            double? longitude = new double?();
            long? publish_date = new long?();
            bool PublishOnTwitter = false;
            bool PublishOnFacebook = false;
            uint post_id = 0;
            bool OnBehalfOfGroup = false;
            bool Sign = false;
            //bool FriendsOnly = false;

            List<string> AttachmentIds = this.Attachments.Select(a => a.ToString()).ToList();

            string message = this.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");

            //postRequestData.post_id = this._postId;
            //if (this._comment != null)
            //    postRequestData.comment_id = this._comment.id;
            OutboundGeoAttachment outboundGeoAttachment = this.Attachments.FirstOrDefault((a => a is OutboundGeoAttachment)) as OutboundGeoAttachment;
            //if (outboundGeoAttachment != null)
            //{
            //    postRequestData.latitude = new double?(outboundGeoAttachment.Latitude);
            //    postRequestData.longitude = new double?(outboundGeoAttachment.Longitude);
            //}
            //OutboundTimerAttachment timerAttachment = this.Attachments.FirstOrDefault((a => a is OutboundTimerAttachment)) as OutboundTimerAttachment;
            //if (timerAttachment != null)
            if(this._time != DateTime.MinValue)
                publish_date = Extensions.DateTimeToUnixTimestamp(this._time, true);
            else if (this.IsPostponed)
                this._mode = WallPostViewModel.Mode.PublishWallPost;
            //postRequestData.FriendsOnly = this.FriendsOnly.Value;
            //postRequestData.PublishOnTwitter = this.PublishOnTwitter.Value;
            //postRequestData.PublishOnFacebook = this.PublishOnFacebook.Value;
            if ((byte)this._adminLevel > 1 && this._userOrGroupId < 0 || this._postId != 0)
            {
                Sign = this.Signature;
                //OnBehalfOfGroup = this.FromGroup;
            }

            switch (this._mode)
            {
                case WallPostViewModel.Mode.NewWallPost:
                case WallPostViewModel.Mode.PublishWallPost:
                    WallService.Instance.Post(owner_id, message, AttachmentIds, latitude, longitude, publish_date, PublishOnTwitter, PublishOnFacebook, post_id, OnBehalfOfGroup, Sign, this.FriendsOnly, this.CloseComments, this.MuteNotifications, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        if (res.error.error_code == VKErrors.None)
                        {
                            /*
                            if (!this.IsNewPostPostponedOrSuggested)
                                this.FireWallPostAddedOrEditedEvent(res.ResultData.response, postRequestData.owner_id, true, (() => callback(res.ResultCode)));
                            else if (this.IsNewPostPostponed)
                                EventAggregator.Current.Publish((object)new WallPostPostponed(postRequestData.owner_id));
                            else if (this.IsNewPostSuggested)
                                EventAggregator.Current.Publish((object)new WallPostSuggested()
                                {
                                    id = res.ResultData.response,
                                    to_id = postRequestData.owner_id
                                });
                                
                            if (this._mode == WallPostViewModel.Mode.PublishWallPost)
                            {
                                WallPost wallPost = new WallPost()
                                {
                                    to_id = this._isGroup ? -this._userOrGroupId : this._userOrGroupId,
                                    id = this._postId
                                };
                                EventAggregator current = EventAggregator.Current;
                                WallPostPublished wallPostPublished = new WallPostPublished();
                                wallPostPublished.WallPost = wallPost;
                                int num1 = timerAttachment != null ? 1 : 0;
                                wallPostPublished.IsPostponed = num1 != 0;
                                int num2 = !this.IsPublishSuggestedSuppressed ? 1 : 0;
                                wallPostPublished.IsSuggested = num2 != 0;
                                current.Publish((object)wallPostPublished);
                                if (this.IsPostponed)
                                    EventAggregator.Current.Publish((object)new WallPostPostponedPublished()
                                    {
                                        WallPost = wallPost
                                    });
                            }
                            if (!this.IsNewPostPostponedOrSuggested)
                                return;
                                */
                        }
                        
                        callback?.Invoke(res.error);
                    }));
                    break;
                case WallPostViewModel.Mode.EditWallPost:
                    WallService.Instance.Edit(owner_id, message, AttachmentIds, latitude, longitude, publish_date, post_id, Sign, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        //if (res.error.error_code == VKErrors.None)
                        //    this.FireWallPostAddedOrEditedEvent(this._postId, postRequestData.owner_id, false, null);
                        callback?.Invoke(res.error);
                    }));
                    break;
                case WallPostViewModel.Mode.EditWallComment:
                    WallService.Instance.EditComment(owner_id, message, AttachmentIds, _comment.id, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        //if (res.error.error_code == VKErrors.None)
                        //    this.UpdateCommentAndFireTheCommentEditedEvent();
                        callback?.Invoke(res.error);
                    }));
                    break;
                case WallPostViewModel.Mode.EditPhotoComment:
                    PhotosService.Instance.EditComment(this._comment.id, message, owner_id, AttachmentIds, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        //if (res.error.error_code == VKErrors.None)
                        //    this.UpdateCommentAndFireTheCommentEditedEvent();
                        callback?.Invoke(res.error);
                    }));
                    break;
                case WallPostViewModel.Mode.EditVideoComment:
                    VideoService.Instance.EditComment(this._comment.id, message, owner_id, AttachmentIds, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        //if (res.error.error_code == VKErrors.None)
                        //    this.UpdateCommentAndFireTheCommentEditedEvent();
                        callback?.Invoke(res.error);
                    }));
                    break;
                /*
            case WallPostViewModel.Mode.EditDiscussionComment:
                GroupsService.Instance.EditComment(this._comment.GroupId, this._comment.TopicId, this._comment.id, message, AttachmentIds, (res =>
                {
                    base.SetInProgress(false);
                    this._isPublishing = false;
                    //if (res.error.error_code == VKErrors.None)
                    //    this.UpdateCommentAndFireTheCommentEditedEvent();
                    callback(res.error.error_code);
                }));
                break;*/
                case WallPostViewModel.Mode.NewTopic:
                    GroupsService.Instance.CreateTopic((uint)this._userOrGroupId, this._topicTitle, this.Text, AttachmentIds, this.FromGroup, (res =>
                    {
                        base.SetInProgress(false);
                        this._isPublishing = false;
                        //if (res.error.error_code == VKErrors.None)
                        //    this.FireTopicCreatedEvent(res.ResultData.response);
                        callback?.Invoke(res.error);
                    }));
                    break;
                    /*
                case WallPostViewModel.Mode.EditProductComment:
                    MarketService.Instance.EditComment(postRequestData.owner_id, this._comment.cid, postRequestData.message, postRequestData.AttachmentIds, (res =>
                    {
                        this.SetInProgress(false, "");
                        this._isPublishing = false;
                        if (res.ResultCode == ResultCode.Succeeded)
                            this.UpdateCommentAndFireTheCommentEditedEvent();
                        callback(res.ResultCode);
                    }));
                    break;*/
            }
        }

        public bool CanPublish
        {
            get
            {
                if (this.Attachments.Any(a => !(a is OutboundTimerAttachment)) || !string.IsNullOrWhiteSpace(this.Text))
                {
                    if (this.Attachments.All(a => a.UploadState == OutboundAttachmentUploadState.Completed) && this._mode != WallPostViewModel.Mode.NewTopic)
                        goto label_6;
                }
                if (this._mode != WallPostViewModel.Mode.EditWallPost || !this._editWallRepost)
                {
                    if (this._mode == WallPostViewModel.Mode.NewTopic && !string.IsNullOrWhiteSpace(this._topicTitle) && !string.IsNullOrWhiteSpace(this.Text))
                        return this.Attachments.All(a => a.UploadState == OutboundAttachmentUploadState.Completed);
                    return false;
                }
            label_6:
                return true;
            }
        }

        public bool IsNewPostPostponedOrSuggested
        {
            get
            {
                if (!this.IsNewPostPostponed)
                    return this.IsNewPostSuggested;
                return true;
            }
        }

        public bool IsNewPostSuggested
        {
            get
            {
                if (this._mode == WallPostViewModel.Mode.NewWallPost && this._isPublicPage)
                    return (byte)this._adminLevel < 2;
                return false;
            }
        }

        public bool IsNewPostPostponed
        {
            get
            {
                if (this._mode == WallPostViewModel.Mode.NewWallPost && this.Attachments != null)
                    return this.Attachments.FirstOrDefault((a => a is OutboundTimerAttachment)) != null;
                return false;
            }
        }

        private VKBaseDataForGroupOrUser _author;
        public VKBaseDataForGroupOrUser Author
        {
            get
            {
                return this._author;
            }
            set
            {
                this._author = value;
                this._userOrGroupId = value.Id == Settings.UserId ? 0 : value.Id;
                base.NotifyPropertyChanged(nameof(this.Author));
                base.NotifyPropertyChanged(nameof(this.OwnPostVisibility));

                if (value is VKGroup group)
                {
                    this._adminLevel = group.admin_level;
                    this._userOrGroupId = group.Id;
                    this._isPublicPage = group.type == VKGroupType.Page;
                }
                else
                {
                    this._adminLevel = VKAdminLevel.None;
                }


                base.NotifyPropertyChanged(nameof(this.SignatureVisibility));

                
            }
        }

        public ObservableCollection<VKBaseDataForGroupOrUser> Authors { get; private set; }
        

        private DateTime _time;
        public DateTime Time
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
                if (value < DateTime.Now)
                    this._time = DateTime.MinValue;
                base.NotifyPropertyChanged(nameof(this.TimeScheduleStr));
                base.NotifyPropertyChanged(nameof(this.Time));
            }
        }

        public string TimeScheduleStr
        {
            get
            {
                if (this._time == DateTime.MinValue)
                    return "Сейчас";
                return UIStringFormatterHelper.FormateDateForEventUI(this.Time);
            }
        }

        private void InitGroups()
        {
            GroupsService.Instance.GetUserGroups(0, 0, 30, "editor", (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        foreach (var item in result.response.items)
                        {
                            this.Authors.Add(item);
                            if (this._userOrGroupId == -item.id)
                                this.Author = item;
                        }

                        base.NotifyPropertyChanged(nameof(this.IsAuthorsArrowVisibility));
                    }
                });
            });
        }

        public void UploadAttachments()
        {
            Execute.ExecuteOnUIThread(() =>
            {
                IOutboundAttachment attachment = this.Attachments.FirstOrDefault<IOutboundAttachment>((a => a.UploadState == OutboundAttachmentUploadState.NotStarted));
                if (attachment == null)
                    return;
                this.UploadAttachment(attachment, this.UploadAttachments);
            });
        }

        public void UploadAttachment(IOutboundAttachment attachment, Action callback = null)
        {
            if (attachment.UploadState == OutboundAttachmentUploadState.Completed)
                return;
            //this.UpdateUploadingStatus(true);
            attachment.Upload(() => Execute.ExecuteOnUIThread(() =>
            {
                //this.UpdateUploadingStatus(false);
                this.NotifyPropertyChanged(nameof(this.CanPublish));
                if (callback == null)
                    return;
                callback();
            }), null);
        }

        public string UniqueId
        {
            get
            {
                string str = string.Concat(this._mode, "_", this.IsNewPostSuggested.ToString());
                switch (this._mode)
                {
                    case WallPostViewModel.Mode.EditWallPost:
                    case WallPostViewModel.Mode.EditWallComment:
                    case WallPostViewModel.Mode.EditPhotoComment:
                    case WallPostViewModel.Mode.EditVideoComment:
                    case WallPostViewModel.Mode.EditDiscussionComment:
                    case WallPostViewModel.Mode.PublishWallPost:
                    case WallPostViewModel.Mode.EditProductComment:
                        return str;
                    default:
                        return string.Concat( str, "_", this._userOrGroupId );
                }
            }
        }

        public Visibility IsAuthorsArrowVisibility
        {
            get
            {
                return (this.Authors.Count > 1).ToVisiblity();
            }
        }

        public Visibility IsExtendedAuthorsVisibility
        {
            get
            {
                return (this._userOrGroupId == 0).ToVisiblity();
            }
        }


        public void Write(BinaryWriter writer)
        {
            writer.Write(4);
            writer.Write(this.FriendsOnly);
            //writer.Write(this._publishOnTwitter);
            writer.WriteString(this._text);
            writer.WriteList<OutboundAttachmentContainer>(this.Attachments.Select((a => new OutboundAttachmentContainer(a))).ToList());
            writer.Write(this._userOrGroupId);
            //writer.Write(this._isGroup);
           // writer.Write(this._publishOnFacebook);
            //writer.Write(this._isAdmin);
            writer.Write(this._isPublicPage);
            writer.Write(this.FromGroup);
            writer.Write(this.Signature);
            writer.Write(this._postId);
            //writer.WriteList<Attachment>((IList<Attachment>)this._uneditableAttachments, 10000);
            writer.Write((byte)this._mode);
            writer.Write(this._editWallRepost);
            writer.Write(this._isDirty);
            writer.WriteString(this._topicTitle);
            //BinarySerializerExtensions.WriteDictionary(writer, this._cidToAuthorIdDict);
            //writer.Write<Comment>(this._comment, false);
            //writer.Write<WallRepostInfo>(this._wallRepostInfo, false);
            writer.Write((byte)this._adminLevel);
        }

        public void Read(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            
            this.FriendsOnly = reader.ReadBoolean();
            //this._publishOnTwitter = reader.ReadBoolean();
            this._text = reader.ReadString();
            List<OutboundAttachmentContainer> source = reader.ReadList<OutboundAttachmentContainer>();
            this.Attachments.Clear();
            foreach (IOutboundAttachment outboundAttachment in source.Select<OutboundAttachmentContainer, IOutboundAttachment>((Func<OutboundAttachmentContainer, IOutboundAttachment>)(c => c.OutboundAttachment)))
                this.Attachments.Add(outboundAttachment);
            this._userOrGroupId = reader.ReadInt32();
            //this._isGroup = reader.ReadBoolean();
            //this._publishOnFacebook = reader.ReadBoolean();
            //this._isAdmin = reader.ReadBoolean();
            this._isPublicPage = reader.ReadBoolean();
            this.FromGroup = reader.ReadBoolean();
            this.Signature = reader.ReadBoolean();
            this.UploadAttachments();
            this._postId = reader.ReadUInt32();
            //this._uneditableAttachments = reader.ReadList<Attachment>();
            this._mode = (WallPostViewModel.Mode)reader.ReadByte();
            this._editWallRepost = reader.ReadBoolean();
            this._isDirty = reader.ReadBoolean();
            
            this._topicTitle = reader.ReadString();
            
            //this._cidToAuthorIdDict = reader.ReadDictionaryLong();
            //this._comment = reader.ReadGeneric<Comment>();
            
            //this._wallRepostInfo = reader.ReadGeneric<WallRepostInfo>();
            this._adminLevel = (VKAdminLevel)reader.ReadByte();
        }

        public enum Mode
        {
            /// <summary>
            /// Новый Пост
            /// </summary>
            NewWallPost,

            /// <summary>
            /// Редактирование поста
            /// </summary>
            EditWallPost,
            NewWallComment,
            EditWallComment,
            EditPhotoComment,
            EditVideoComment,
            EditDiscussionComment,
            NewPhotoComment,
            NewVideoComment,
            NewDiscussionComment,
            NewTopic,
            PublishWallPost,
            NewProductComment,
            EditProductComment,
        }
    }
}
