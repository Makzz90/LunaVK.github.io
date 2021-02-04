using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class PhotoViewModel : ViewModelBase, ISupportUpDownIncrementalLoading
    {
        public ObservableCollection<VKComment> Items { get; set; }
        private readonly int _ownerId;
        private readonly uint _pid;
        public VKPhoto Photo { get; set; }
        private PhotoWithFullInfo _photoWithFullInfo;
        private readonly string _accessKey = "";
        private bool _adding;
        public ObservableCollection<IOutboundAttachment> Attachments { get; set; }

        private uint? _knownCommentsCount = null;
        public readonly int CountToLoad = 5;

        public uint _maximum = 0;

        public PhotoViewModel(VKPhoto photo, PhotoWithFullInfo photoWithFullInfo = null)
        {
            this.Photo = photo;
            this._ownerId = photo.owner_id;
            this._pid = photo.id;
            this._accessKey = photo.access_key;
            this._photoWithFullInfo = photoWithFullInfo;
            VKComments comments;
            if (this._photoWithFullInfo == null)
            {
                comments = null;
            }
            else
            {
                VKPhoto photo1 = this._photoWithFullInfo.Photo;
                comments = photo1 != null ? photo1.comments : null;
            }
            if (comments == null)
                return;
            this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
        }

        public PhotoViewModel(int ownerId, uint pid, string accessKey)
        {
            this._accessKey = accessKey;
            this._ownerId = ownerId;
            this._pid = pid;
            this.Items = new ObservableCollection<VKComment>();
            this.Attachments = new ObservableCollection<IOutboundAttachment>();
        }

        public Action<ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        public async Task LoadUpAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public bool HasMoreDownItems
        {
            get { return this.Items.Count == 0 || this.Items.Count < this._maximum; }
        }

        public async Task<object> Reload()
        {
            this.Items.Clear();
            this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);
            return null;
        }

        public List<VKPhotoVideoTag> PhotoTags;

        public async Task LoadDownAsync(bool InReload = false)
        {
            if (InReload)
                PhotosService.Instance.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, 0, (res) => {
                    int num1 = 0;
                    if (res!=null && res.error.error_code == VKErrors.None)
                    {
                        bool num2 = string.IsNullOrEmpty(this.ImageSrc) ? true : false;
                        this._photoWithFullInfo = res.response;
                        this.Photo = res.response.Photo;
                        if (this.Photo != null && string.IsNullOrEmpty(this.Photo.access_key) && !string.IsNullOrEmpty(this._accessKey))
                            this.Photo.access_key = this._accessKey;
                        this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;



                        this.Photo.comments.count = this._knownCommentsCount.Value;
                        this.PhotoTags = res.response.PhotoTags;


                        //PhotoWithFullInfo resultData = res.response;
                        //VKGroup group = resultData != null ? Enumerable.FirstOrDefault(resultData.Groups, (g => g.id == -this._ownerId)) : null;
                        //if (group != null)
                        //    num1 = group.admin_level;
                        if (num2)
                            this.NotifyPropertyChanged(nameof(this.ImageSrc));
                        this.NotifyPropertyChanged(nameof(this.CommentsCountStr));
                        this.NotifyPropertyChanged(nameof(this.Text));
                        //this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsLoadedFullInfo));
                        this.NotifyPropertyChanged(nameof(this.UserCountStr));
                        //this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UserVisibility));
                        this.NotifyPropertyChanged(nameof(this.LikesCountStr));
                        this.NotifyPropertyChanged(nameof(this.DateStr));
                        this.NotifyPropertyChanged(nameof(this.UserLiked));
                        //this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.LikeOpacity));
                        //this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeBackgroundBrush));
                        //this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeTextForegroundBrush));
                        //this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsFullInfoLoadedVisibility));
                        //this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.IsFullInfoLoadedOpacity));

                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);
                    }
                    else
                    {
                        this.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                    }
                });
        }

        private bool _isLoading;

        public void LoadInfoWithComments(Action<bool, int> callback)
        {
            
                if (this._isLoading)
                    return;
                this._isLoading = true;
                //this.SetInProgress(true, "");
                PhotosService.Instance.GetPhotoWithFullInfo(this._ownerId, this._pid, this._accessKey, 0, (res =>
                {
                    int num1 = 0;
                    if (res.error.error_code == VKErrors.None)
                    {
                        int num2 = string.IsNullOrEmpty(this.ImageSrc) ? 1 : 0;
                        this._photoWithFullInfo = res.response;
                        this.Photo = res.response.Photo;
                        if (this.Photo != null && string.IsNullOrEmpty(this.Photo.access_key) && !string.IsNullOrEmpty(this._accessKey))
                            this.Photo.access_key = this._accessKey;
                        this._knownCommentsCount = this._photoWithFullInfo.Photo.comments.count;
                        //PhotoWithFullInfo resultData = res.ResultData;
                        //GroupOrUser group = resultData != null ? Enumerable.FirstOrDefault<GroupOrUser>(resultData.Groups, (Func<GroupOrUser, bool>)(g => g.id == -this._ownerId)) : null;
                        //if (group != null)
                        //    num1 = group.admin_level;


                        this.PhotoTags = res.response.PhotoTags;



                        if (num2 != 0)
                            this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.ImageSrc));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.CommentsCountStr));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.Text));
                        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsLoadedFullInfo));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.UserCountStr));
                        //this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.UserVisibility));
                        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.LikesCountStr));
                        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.UserLiked));
                        this.NotifyPropertyChanged(nameof(this.DateStr));
                        //this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.LikeOpacity));
                        //this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeBackgroundBrush));
                        //this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.LikeTextForegroundBrush));
                        //this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.IsFullInfoLoadedVisibility));
                        //this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.IsFullInfoLoadedOpacity));
                    }
                    //this.SetInProgress(false, "");
                    this._isLoading = false;
                    callback?.Invoke(res.error.error_code == VKErrors.None, num1);
                }));
        }

        public string Text
        {
            get
            {
                if (this.Photo == null)
                    return "";

                return this.Photo.text;
                /*
                Photo photo = this.Photo;
                switch (photo != null ? photo.text : null)
                {
                    case null:
                        return "";
                    default:
                        return Extensions.ForUI(this.Photo.text);
                }*/
            }
        }

        public string CommentsCountStr
        {
            get
            {
                if (this.Photo == null)
                    return "";

                if (this.Photo.comments == null)
                    return "";

                if (this.Photo.comments.count == 0)
                    return "";
                /*
                Photo photo = this.Photo;
                if ((photo != null ? photo.comments : (VKClient.Common.Backend.DataObjects.Comments)null) == null || this.Photo.comments.count <= 0)
                    return "";
                return UIStringFormatterHelper.FormatForUIShort((long)this.Photo.comments.count);*/
                return this.Photo.comments.count.ToString();
            }
        }

        public string LikesCountStr
        {
            get
            {
                if (this.Photo == null)
                    return "";

                if (this.Photo.likes == null)
                    return "";

                if (this.Photo.likes.count == 0)
                    return "";

                return this.Photo.likes.count.ToString();/*
                Photo photo = this.Photo;
                if ((photo != null ? photo.likes : null) == null || this.Photo.likes.count <= 0)
                    return "";
                return UIStringFormatterHelper.FormatForUIShort((long)this.Photo.likes.count);*/
            }
        }

        public string RepostsCountStr
        {
            get
            {
                if (this.Photo == null)
                    return "";

                if (this.Photo.reposts == null)
                    return "";

                if (this.Photo.reposts.count == 0)
                    return "";

                return this.Photo.reposts.count.ToString();
            }
        }

        public string DateStr
        {
            get
            {
                if (this.Photo == null)
                    return "";

                return UIStringFormatterHelper.FormatDateTimeForUI(this.Photo.date);
            }
        }

        public string UserCountStr
        {
            get
            {
                /*
                if (this.Photo == null)
                    return "";

                if (this.Photo.tags == null)
                    return "";

                if (this.Photo.tags.count == 0)
                    return "";

                return this.Photo.tags.count.ToString();
                */
                if (this.PhotoTags == null)
                    return "";

                if (this.PhotoTags.Count == 0)
                    return "";

                return UIStringFormatterHelper.CountForUI(this.PhotoTags.Count);
            }
        }

        public bool IsLoadedFullInfo
        {
            get
            {
                return this._knownCommentsCount != null;
            }
        }

        public bool UserLiked
        {
            get
            {
                if (this.Photo == null)
                    return false;

                if (this.Photo.likes == null)
                    return false;

                return this.Photo.likes.user_likes;
            }
        }

        public string ImageSrc
        {
            get
            {
                if (this.Photo == null)
                    return null;
                return this.Photo.MaxPhoto;
            }
        }

        public string OwnerImageUri
        {
            get
            {
                string str = "";
                if (this._photoWithFullInfo != null)
                {
                    if (this._ownerId < 0L)
                    {
                        VKGroup group = this._photoWithFullInfo.Groups.FirstOrDefault(g => g.id == -this._ownerId);
                        if (group != null)
                            str = group.photo_200;
                    }
                    else
                    {
                        VKUser user = this._photoWithFullInfo.Users.FirstOrDefault(u => u.id == this._ownerId);
                        if (user != null)
                            str = user.photo_max;
                    }
                }
                return str;
            }
        }

        public string OwnerName
        {
            get
            {
                string str = "";
                if (this._photoWithFullInfo != null)
                {
                    if (this._ownerId < 0L)
                    {
                        VKGroup group = this._photoWithFullInfo.Groups.FirstOrDefault(g => g.id == -this._ownerId);
                        if (group != null)
                            str = group.name;
                    }
                    else
                    {
                        VKUser user = this._photoWithFullInfo.Users.FirstOrDefault(u => u.id == this._ownerId);
                        if (user != null)
                            str = user.Title;
                    }
                }
                return str;
            }
        }

        public int OwnerId
        {
            get
            {
                if (this.Photo == null)
                    return 0;
                return this.Photo.owner_id;
            }
        }

        public void AddComment(VKComment comment, bool fromGroup, Action<VKComment> callback, string stickerReferrer = "")
        {
            if (this._adding)
            {
                callback.Invoke( null);
            }
            else
            {
                this._adding = true;

                List<string> attachmentIds = this.Attachments.Select((a => { return a.ToString(); })).ToList();

                PhotosService.Instance.CreateComment(this.OwnerId, this.Photo.id, comment.reply_to_comment, comment.text, fromGroup, attachmentIds, (res =>
                {
                    if (res.error.error_code == VKErrors.None)
                    {
                        ++this.Photo.comments.count;
                        Execute.ExecuteOnUIThread(() =>
                        {
                            if (this._photoWithFullInfo == null)
                                return;
                            this._photoWithFullInfo.Comments.items.Add(res.response);
                        });
                        callback(res.response);
                    }
                    else
                        callback(null);
                    this._adding = false;
                }), this._accessKey, /*comment.sticker_id*/0, stickerReferrer);
            }
        }








        public Visibility BotKeyboardVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public ObservableCollection<string> BotKeyboardButtons { get; private set; }

        public string TextWatermarkText
        {
            get { return LocalizedStrings.GetString("Comment"); }
        }
    }
}
