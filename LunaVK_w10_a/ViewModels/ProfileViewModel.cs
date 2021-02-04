using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using LunaVK.Core.DataObjects;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using LunaVK.Core.Utils;
using LunaVK.UC;
using LunaVK.Core.Framework;
using Windows.UI.Xaml.Input;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using LunaVK.Core.Enums;
using LunaVK.Core.ViewModels;
using LunaVK.Core;
using LunaVK.Framework;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using LunaVK.Library;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Calls;

namespace LunaVK.ViewModels
{
    //ProfileViewModel
    //GroupProfileHeaderViewModel
    public class ProfileViewModel : GenericCollectionViewModel<VKWallPost>
    {
        private readonly List<uint> _serviceUserIds = new List<uint>() { 100, 101, 333 };

        public uint Id { get; private set; }
        public VKUser User { get; private set; }
        DelayedExecutor _de = new DelayedExecutor(100);

        public ObservableCollection<CommandVM> Commands { get; private set; }

        //public ObservableCollection<ProfileInfoItem> InfoSections { get; private set; }
        public ProfileInfoFullViewModel CompactinfoViewModel { get; private set; }

        public AllProfilePostsToggleViewModel PostsToggleViewModel { get; private set; }

        public ProfileMediaViewModelFacade MediaViewModel { get; private set; }

        



        public ProfileViewModel(uint id)
        {
            this.Commands = new ObservableCollection<CommandVM>();
            this.CompactinfoViewModel = new ProfileInfoFullViewModel();
            this.MediaViewModel = new ProfileMediaViewModelFacade();//this.MediaItems = new ObservableCollection<MediaListSectionViewModel>();

            this.Id = id;
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            this.CompactinfoViewModel.InfoSections.Clear();
            this.User = null;

            this.PostsToggleViewModel = null;
        }

        #region VM
        /// <summary>
        /// Видимость иконки проверенного профиля
        /// </summary>
        public Visibility IsVerifiedVisibility
        {
            get
            {
                if (this.User == null || !this.User.IsVerified)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        /// <summary>
        /// Изображение профиля после обрезки
        /// </summary>
        public BitmapImage ProfileImageUrl { get; private set; }

        /// <summary>
        /// Аватарка в кружочке
        /// </summary>
        public BitmapImage Avatar
        {
            get
            {
                if (this.User == null)
                    return null;
                return new BitmapImage(new Uri(this.User.photo_100));
            }
        }

        /// <summary>
        /// Небольшой текст ниже названия профиля - был в сети или открытая группа
        /// </summary>
        public string Activity
        {
            get
            {
                if (this.User == null)
                    return "";
                return this.User.GetUserStatusString();
            }
        }

        public Visibility User100DescriptionVisibility
        {
            get
            {
                if (base.CurrentLoadingStatus != ProfileLoadingStatus.Loading && base.CurrentLoadingStatus != ProfileLoadingStatus.LoadingFailed && this.IsServiceProfile)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string Name
        {
            get
            {
                if (this.User == null)
                    return "";
                return this.User.Title;
            }
        }

        public string Status
        {
            get
            {
                if (this.User == null)
                    return "";
                return this.User.status;
            }
        }
        #endregion


        /*
        private void LoadSecondPart()
        {
            string filter = "all";
            if (this.PostsToggleViewModel != null && !this.PostsToggleViewModel.IsAllPosts)
                filter = "owner";

            string code = "";
            code += "var wall = API.wall.get({owner_id:" + this.Id + ",count:15, offset:" + this.Items.Count + ",extended:1,fields:\"photo_100,verified,sex\",filter:\"" + filter + "\"});";
            code += "var profile_photos;";
            if (this.Photos.Count == 0)
                code += "profile_photos = API.photos.getAll({owner_id:" + this.Id + ",skip_hidden:1" + (this.Id > 0 ? "" : ",no_service_albums:1") + "});";
            code += "return {profiles:wall.profiles,groups:wall.groups,items:wall.items,count:wall.count,profile_photos:profile_photos};";


            VKRequestsDispatcher.Execute<VKUserRequest>(code, (result) =>
            {
                if (result.error.error_code != VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.LoadingFailed);
                    });
                    return;
                }

                base._totalCount = result.response.count;

                Execute.ExecuteOnUIThread(() =>
                {
                    this.MediaViewModel.Init(result.response.profile_photos.count>0 ? result.response.profile_photos : null);

                    if (this.PostsToggleViewModel == null)
                    {
                        AllProfilePostsToggleViewModel postsToggleViewModel = new AllProfilePostsToggleViewModel(this.User, (int)base._totalCount, this.User.first_name_gen);

                        //Action<bool> action = (Action<bool>)(isAllPosts => DelayedExecutorUtil.Execute((() => this.LoadWall(false, null)), 50));
                        postsToggleViewModel.StateChangedCallback = this.StateChanged;
                        this.PostsToggleViewModel = postsToggleViewModel;
                        base.NotifyPropertyChanged(nameof(this.PostsToggleViewModel));
                        this.PostsToggleViewModel.UpdateState(base.CurrentLoadingStatus);
                    }

                    if (result.response.profile_photos != null)
                    {
                        foreach (VKPhoto photo in result.response.profile_photos.items)
                        {
                            this.Photos.Add(photo);
                        }
                    }






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

                        if (p.geo != null)
                        {
                            p.attachments.Add(new VKAttachment() { geo = p.geo, type = VKAttachmentType.Geo });
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

                    base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Loaded);//не допустим цикла!
                });
            });
        }
        */
        public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKWallPost>> callback)
        {
            if (this.User == null)
            {
                string code = "var users = API.users.get({user_ids:" + this.Id + ",fields:\"photo_100,verified,activity,status,counters,friend_status,crop_photo,bdate,occupation,last_seen,sex,online,blacklisted,city,relation,first_name_gen,is_favorite,first_name_acc,domain,can_write_private_message,blacklisted_by_me\"});";
                code += "var u = users[0]; var profile_photos=null;";
                if (this.Id != Settings.UserId)
                {
                    code += "var mutualIds = API.friends.getMutual({target_uid:" + this.Id + ", count:8});";
                    code += "var mutualUsers = API.users.get({user_ids:mutualIds,fields:\"photo_50\"});";
                    code += "u.randomMutualFriends=mutualUsers;";
                }
                code += "if (u.occupation != null) { if(u.occupation.type==\"work\" && u.occupation.id>0){ var groups = API.groups.getById({ group_id: u.occupation.id}); u.occupationGroup = groups[0]; }}";

                code += "if (u.relation_partner != null)";
                code += "{";
                code +=     "var relationuser = API.users.get({ user_ids: u.relation_partner.id,fields: \"first_name_ins,last_name_ins,first_name_abl,last_name_abl,first_name_acc,last_name_acc\"})[0];";
                code +=     "u.relation_partner.acc = relationuser.first_name_acc + \" \" + relationuser.last_name_acc;";
                code +=     "u.relation_partner.ins = relationuser.first_name_ins + \" \" + relationuser.last_name_ins;";
                code +=     "u.relation_partner.abl = relationuser.first_name_abl + \" \" + relationuser.last_name_abl;";
                code += "}";

                if (this.Id == Settings.UserId)
                    code += "profile_photos = API.photos.getAll({owner_id:" + this.Id + ",skip_hidden:1});";
                else
                    code += "if(u.deactivated==null && u.blacklisted!=1 && (u.is_closed && u.friend_status<2 ) == false  ){   profile_photos = API.photos.getAll({owner_id:" + this.Id + ",skip_hidden:1});   }";

                code += "return {owner:u,profile_photos:profile_photos};";

                VKRequestsDispatcher.Execute<VKUserRequest>(code, (result) =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        this.User = result.response.owner;

                        base.NotifyPropertyChanged(nameof(this.Name));
                        base.NotifyPropertyChanged(nameof(this.Avatar));
                        base.NotifyPropertyChanged(nameof(this.Activity));

                        base.NotifyPropertyChanged(nameof(this.TextButtonSecondary));
                        base.NotifyPropertyChanged(nameof(this.SubscribeGlyph));

                        base.NotifyPropertyChanged(nameof(this.VisibilityButtonSendMessage));
                        base.NotifyPropertyChanged(nameof(this.VisibilityButtonSecondary));
                        base.NotifyPropertyChanged(nameof(this.Status));
                        base.NotifyPropertyChanged(nameof(this.IsVerifiedVisibility));

                        base.NotifyPropertyChanged(nameof(this.FavoriteGlyph));
                        base.NotifyPropertyChanged(nameof(this.FavoriteText));

                        base.NotifyPropertyChanged(nameof(this.BlockText));
                        base.NotifyPropertyChanged(nameof(this.CanBanUnban));

                        base.NotifyPropertyChanged(nameof(this.FullInfoRight));

                        base.NotifyPropertyChanged(nameof(this.CanPostVisibility));

                        Execute.ExecuteOnUIThread(() =>
                        {
                            this.CreateData();
                            this.MediaViewModel.PreInit(this.User);
                            if (result.response.profile_photos != null)
                            {
                                this.MediaViewModel.Init(result.response.profile_photos);
                            }

                            if (this.User.crop_photo != null)
                                this.UpdateProfilePhoto(User.crop_photo.photo.photo_604, User.crop_photo.crop, false);
                            else if (!string.IsNullOrEmpty(this.User.MinPhoto))
                                this.UpdateProfilePhoto(User.MinPhoto, null, false);

                            if ((this.User.is_closed && this.Id != Settings.UserId && (byte)this.User.friend_status < 2) || this.User.blacklisted || this.User.deactivated != VKIsDeactivated.None || this.IsServiceProfile)
                                base.NotifyPropertyChanged(nameof(this.LoadingStatusText));
                            else
                                this._de.AddToDelayedExecution(this.LoadWall);


                            
                        });
                    }

                    callback(result.error, null);
                    base._totalCount = 0;
                }, (jsonStr) =>
                {
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }
            else
            {
                //if ((this.User.is_closed && this.Id != Settings.UserId && (byte)this.User.friend_status < 2) || this.User.blacklisted || this.User.deactivated != VKIsDeactivated.None || this.IsServiceProfile)
                //    return;

                this.LoadWall();
            }
        }

        public override string GetFooterTextForCount
        {
            get
            {
                if (base._totalCount <= 0)
                    return "";//LocalizedStrings.GetString("NoFriends");
                return "Записей: " + base._totalCount;//UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
            }
        }
        /*
        /// <summary>
        /// Происходит только при перезагрузке
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="profileData"></param>
        private void HandleProfileDataLoaded(VKErrors resultCode, VKBaseDataForGroupOrUser profileData)
        {
            if (resultCode != VKErrors.None)
                return;

            this._profileData = profileData;


            Execute.ExecuteOnUIThread(() =>
            {



                this.ParseLoadingStatus();//переместить выше

                //
                this.Name = this._profileData.Title;
                this.Avatar = this._profileData.MinPhoto;
                this.VerifiedVisibility = this._profileData.IsVerified ? Visibility.Visible : Visibility.Collapsed;
                //this.Description = this.User.description;

                if (this.User != null)
                {
                    if (this.User.last_seen != null)//null если аккаунт удалён
                    {
                        //this.User.last_seen.Online = this.User.online;
                        //this.User.last_seen.OnlineApp = this.User.online_app;
                        this.Activity = this.User.GetUserStatusString();
                    }
                }
                else
                {
                    this.Activity = this.GroupData.activity;
                }

                this.MediaViewModel.PreInit(this._profileData);
               
                if (this.User != null)
                {
                    if (this.User.crop_photo != null)
                        this.UpdateProfilePhoto(User.crop_photo.photo.photo_604, User.crop_photo.crop, false);
                    else if (!string.IsNullOrEmpty(this.User.MinPhoto))
                        this.UpdateProfilePhoto(User.MinPhoto, null, false);
                }
                else
                {
                    //Нет кроп фото, но ковер то надо показать!
                    if (this.GroupData.crop_photo != null)
                        this.UpdateProfilePhoto(GroupData.crop_photo.photo.photo_604, GroupData.crop_photo.crop, true);
                    else
                        this.UpdateProfilePhoto(GroupData.photo_100, null, true);

                    if (this.GroupData.cover != null && this.GroupData.cover.enabled == true)
                    {
                        BitmapImage bimg = new BitmapImage(new Uri(GroupData.cover.CurrentImage));
                        this.CoverImageUrl = bimg;
                        base.NotifyPropertyChanged(nameof(this.CoverImageUrl));
                    }
                }

                this.CreateData();

                base.NotifyPropertyChanged("Name");
                base.NotifyPropertyChanged("Avatar");
                base.NotifyPropertyChanged("VerifiedVisibility");
                base.NotifyPropertyChanged("Activity");
                base.NotifyPropertyChanged("CountersVisibility");

                base.NotifyPropertyChanged(nameof(this.TextButtonPrimary));
                base.NotifyPropertyChanged(nameof(this.TextButtonSecondary));
                base.NotifyPropertyChanged(nameof(this.VisibilityButtonSendMessage));
                base.NotifyPropertyChanged(nameof(this.VisibilityButtonSecondary));
                base.NotifyPropertyChanged(nameof(this.VisibilityButtonThird));
                base.NotifyPropertyChanged(nameof(this.MediaVisibility));
                //               base.NotifyPropertyChanged("InfoItems");
            });

            if (resultCode == VKErrors.None)
            {
                this._firstPartLoaded = true;
                if (this._profileData.Deactivated == VKIsDeactivated.None)
                {
                    if (this.User != null)
                    {
                        if (this.User.is_closed && (byte)this.User.friend_status < 2 && this.Id != Settings.UserId)//BUG!!
                        {
                            Execute.ExecuteOnUIThread(() =>
                            {
                                base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Private);
                            });
                            return;
                        }
                    }

                    if (this.IsServiceProfile)
                    {
                        Execute.ExecuteOnUIThread(() =>
                        {
                            base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Service);
                        });
                        return;
                    }

                    this.LoadSecondPart();
                }
                else if(this._profileData.Deactivated == VKIsDeactivated.Banned)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Banned);
                    });
                }
                else if (this._profileData.Deactivated == VKIsDeactivated.Deleted)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        base.LoadingStatusUpdated?.Invoke(ProfileLoadingStatus.Deleted);
                    });
                }
            }
        }

        public Visibility MediaVisibility
        {
            get
            {
                if (this._firstPartLoaded)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
        }
        */
        private void StateChanged(bool isAllPosts)
        {
            this.Items.Clear();
            //this.LoadDownAsync();
            this.LoadWall();
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
            parameters["owner_id"] = this.Id.ToString();
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
                         AllProfilePostsToggleViewModel postsToggleViewModel = new AllProfilePostsToggleViewModel(this.User, (int)base._totalCount, this.User.first_name_gen);

                         postsToggleViewModel.StateChangedCallback = this.StateChanged;
                         this.PostsToggleViewModel = postsToggleViewModel;
                         base.NotifyPropertyChanged(nameof(this.PostsToggleViewModel));
                         this.PostsToggleViewModel.UpdateState(base.CurrentLoadingStatus);
                     }
                 });
             });
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

                        if (this.PostsToggleViewModel != null)
                        {
                            this.PostsToggleViewModel.UpdateAllPostsText(base._totalCount.Value);
                        }
                    });
                }
            });
        }
        /*

        /// <summary>
        /// Происходит во время первой загрузки данных (Reload)
        /// </summary>
        private void ParseLoadingStatus()
        {
            if (this.User != null)
            {
                if (this._serviceUserIds.Contains(this._profileData.Id))
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Service);
                    return;
                }

                if (this._profileData.Deactivated != VKIsDeactivated.None)
                {
                    this.UpdateProfileLoadingStatus(this._profileData.Deactivated == VKIsDeactivated.Banned ? ProfileLoadingStatus.Banned : ProfileLoadingStatus.Deleted);
                    return;
                }

                if (this.User.blacklisted)
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Blacklisted);
                    return;
                }

                if (this.User.is_closed && this.User.id != Settings.UserId && this.User.friend_status != VKUsetMembershipType.Friends && this.User.friend_status != VKUsetMembershipType.RequestReceived)
                {
                    this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Private);
                    return;
                }
            }
            }

            this.UpdateProfileLoadingStatus(ProfileLoadingStatus.Loaded);

        }
*/
        public Action CoverLoaded;

        private void UpdateProfilePhoto(string uri, CropPhoto.CropRect crop, bool for_group)
        {
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

            if (!for_group)
            {
 //               this.CoverImageUrl = bimg;
                base.NotifyPropertyChanged("CoverImageUrl");
            }
        }

        public class VKUserRequest
        {
            public VKUser owner { get; set; }

            public VKCountedItemsObject<VKPhoto> profile_photos { get; set; }
        }

        public class CounterVM
        {
            public string Title { get; set; }
            public int Count { get; set; }
            public Action Callback;

            public CounterVM(string title, int count, Action callback = null)
            {
                this.Title = title;
                this.Count = count;
                this.Callback = callback;
            }
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



        /*
        public List<InfoListItem> InfoItems
        {
            get
            {
                List<InfoListItem> infoListItemList1 = new List<InfoListItem>();
                if (this.User != null)
                {

                    VKCounters counters = this._profileData.Counters;
                    if (!string.IsNullOrEmpty(this.User.activity))
                    {
                        //StatusInfoListItem statusInfoListItem1 = new StatusInfoListItem(this.User);
                        //string str = "/Resources/Profile/ProfileStatus.png";
                        //statusInfoListItem1.IconUrl = str;
                        //StatusInfoListItem statusInfoListItem2 = statusInfoListItem1;
                        //infoListItemList1.Add((InfoListItem)statusInfoListItem2);

                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xEC42";
                        infoListItem.Text = this.User.activity;
                        if (this.User.id == Settings.UserId)
                        {
                            infoListItem.TapAction = () =>
                            {
                                this.OpenSetStatusPopup(this.User.activity, this.UpdateStatus);
                            };
                        }
                        infoListItemList1.Add(infoListItem);

                    }

                    if (counters.friends > 0)
                    {
                        InfoListItem infoListItem1 = new InfoListItem();
                        infoListItem1.IconUrl = "\xE716";
                        //InlinesCollection inlinesCollection = this.ComposeInlinesForFriends(counters);
                        //infoListItem1.Inlines = inlinesCollection;
                        //int num = 1;
                        //infoListItem1.IsTiltEnabled = num != 0;
                        //Action action = (Action)(() =>
                        //{
                        //    this.PublishProfileBlockClickEvent(ProfileBlockType.friends);
                        //    Navigator.Current.NavigateToFriends(this.User.id, "", false, FriendsPageMode.Default);
                        //});
                        //infoListItem1.TapAction = action;
                        //InfoListItem infoListItem2 = infoListItem1;
                        List<VKUser> randomMutualFriends = this.User.randomMutualFriends;
                        if (randomMutualFriends != null && this.User.id != Settings.UserId)
                        {
                            if (randomMutualFriends.Count > 0)
                                infoListItem1.Preview1 = randomMutualFriends[0].photo_50;
                            if (randomMutualFriends.Count > 1)
                                infoListItem1.Preview2 = randomMutualFriends[1].photo_50;
                            if (randomMutualFriends.Count > 2)
                                infoListItem1.Preview3 = randomMutualFriends[2].photo_50;
                        }
                        infoListItem1.TapAction = () => { Library.NavigatorImpl.Instance.NavigateToFriends(this.User.id); };

                        infoListItem1.Text = this.ComposeInlinesForFriends();

                        infoListItemList1.Add(infoListItem1);
                    }

                    if (counters.followers > 0)
                    {
                        //List<InfoListItem> infoListItemList2 = infoListItemList1;
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xF081";
                        infoListItem.Text = this.ComposeInlinesForFollowers();
                        //infoListItem.IconUrl = "/Resources/Profile/ProfileFollowers.png";
                        //InlinesCollection inlinesCollection = this.ComposeInlinesForFollowers();
                        //infoListItem.Inlines = inlinesCollection;
                        //int num = 1;
                        //infoListItem.IsTiltEnabled = num != 0;
                        //Action action = (Action)(() =>
                        //{
                        //    this.PublishProfileBlockClickEvent(ProfileBlockType.followers);
                        //    Navigator.Current.NavigateToFollowers(this.User.id, false, "");
                        //});
                        //infoListItem.TapAction = action;
                        infoListItemList1.Add(infoListItem);
                    }

                    if (!string.IsNullOrEmpty(this.User.bdate) && this.User.id != Settings.UserId)
                    {
                        InfoListItem infoListItem1 = new InfoListItem();
                        infoListItem1.IconUrl = "\xE1DC";
                        string str1 = this.ComposeTextForBirthday(this.User.bdate);
                        infoListItem1.Text = str1;
                        DateTime birthday = this.GetBirthday(this.User.bdate);
                        if (birthday > DateTime.MinValue)
                        {
                            //infoListItem2.IsTiltEnabled = true;
                            //infoListItem2.TapAction = (Action)(() =>
                            //{
                            //    SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();
                            //    DateTime? nullable = new DateTime?(birthday);
                            //    saveAppointmentTask.StartTime = nullable;
                            //    string str = string.Format("{0}: {1}", CommonResources.ProfilePage_Info_Birthday, this.User.Name);
                            //    saveAppointmentTask.Subject = str;
                            //    int num = 0;
                            //    saveAppointmentTask.IsAllDayEvent = (num != 0);
                            //    saveAppointmentTask.Show();
                            //});
                        }
                        infoListItemList1.Add(infoListItem1);
                    }

                    if (this.User.city != null && this.User.city.id > 0L && this.User.id != Settings.UserId)
                    {
                        List<InfoListItem> infoListItemList2 = infoListItemList1;
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xE80F";
                        string str = string.Format("{0}: {1}", "City", this.User.city.title);
                        infoListItem.Text = str;
                        infoListItemList2.Add(infoListItem);
                    }

                    if (this.User.occupation != null)
                    {
                        InfoListItem infoListItem = new InfoListItem() { IconUrl = this.User.occupation.type == VKOccupation.OccupationType.work ? "\xE821" : "\xE7BE", Text = string.Format("{0}: {1}", this.User.occupation.type == VKOccupation.OccupationType.work ? "OccupationType_Work") : "ProfilePage_Info_Education", this.User.occupation.name) };
                        if (this.User.occupation.type == VKOccupation.OccupationType.work)
                        {
                            infoListItem.Text = string.Format("{0}: {1}", "OccupationType_Work", this.User.occupation.name);
                        }
                        else
                        {
                            if (this.User.education != null)
                            {
                                int graduation = this.User.education.graduation;
                                string str = graduation > 0 ? string.Format("{0} '{1:00}", this.User.occupation.name, (graduation % 100)) : this.User.occupation.name;
                                infoListItem.Text = string.Format("{0}: {1}", "ProfilePage_Info_Education", str);
                            }

                        }
                        //Group occupationGroup = this.User.occupationGroup;
                        //if (occupationGroup != null)
                        //{
                        //    infoListItem.Preview1 = occupationGroup.photo_200;
                        //infoListItem.IsTiltEnabled = true;
                        //infoListItem.TapAction = (Action)(() => Navigator.Current.NavigateToGroup(occupationGroup.id, occupationGroup.name, false));
                        // }
                        infoListItemList1.Add(infoListItem);
                    }
                }
                else if (this.GroupData != null)
                {
                    if (!string.IsNullOrEmpty(this.GroupData.status))
                    {
                        //StatusInfoListItem statusInfoListItem1 = new StatusInfoListItem((IProfileData)this._groupData);
                        //string str = "/Resources/Profile/ProfileStatus.png";
                        //statusInfoListItem1.IconUrl = str;
                        //StatusInfoListItem statusInfoListItem2 = statusInfoListItem1;
                        //infoListItemList1.Add((InfoListItem)statusInfoListItem2);
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xEC42";
                        infoListItem.Text = this.GroupData.status;
                        if ((byte)this.GroupData.admin_level > 1)
                        {
                            infoListItem.TapAction = () => { this.OpenSetStatusPopup(this.GroupData.status, this.GroupData.id, this.UpdateData); };
                        }
                        infoListItemList1.Add(infoListItem);
                    }
                    if (this.GroupData.type == VKGroupType.Event)
                    {
                        DateTime eventStartDate = this.GroupData.start_date;
                        DateTime eventFinishDate = this.GroupData.finish_date;
                        if (eventStartDate > DateTime.MinValue)
                        {
                            InfoListItem infoListItem = new InfoListItem()
                            {
                                IconUrl = "\xED5A"
                            };

                            if (eventFinishDate < DateTime.Now || eventStartDate < DateTime.Now && eventStartDate >= eventFinishDate)
                            {
                                infoListItem.Text = string.Format("{0} {1}", "Event_Happened", eventStartDate.ToString("dd MMMM yyyy"));
                            }
                            else
                            {
                                infoListItem.Text = UIStringFormatterHelper.FormatDateTimeForUI(eventStartDate);
                                infoListItem.IsTiltEnabled = true;
                                infoListItem.TapAction = (Action)(() =>
                                {
                                    
                                    //SaveAppointmentTask saveAppointmentTask1 = new SaveAppointmentTask();
                                    //saveAppointmentTask1.StartTime = new DateTime?(eventStartDateTime);
                                    //saveAppointmentTask1.Subject = group.name;
                                    //int num = 0;
                                    //saveAppointmentTask1.IsAllDayEvent = num != 0;
                                    //SaveAppointmentTask saveAppointmentTask2 = saveAppointmentTask1;
                                    //Place place = group.place;
                                    //if (!string.IsNullOrEmpty(place != null ? place.address : null))
                                    //    saveAppointmentTask2.Location = group.place.address;
                                    //if (eventFinishDate > eventStartDate)
                                    //    saveAppointmentTask2.EndTime = new DateTime?(Extensions.UnixTimeStampToDateTime((double)eventFinishDate, true));
                                    //saveAppointmentTask2.Show();
                                });
                            }
                            infoListItemList1.Add(infoListItem);
                        }
                        string str = "";
                        if (!string.IsNullOrEmpty(this.GroupData.place != null ? this.GroupData.place.address : null))
                            str = this.GroupData.place.address;

                        if (!string.IsNullOrEmpty(this.GroupData.city != null ? this.GroupData.city.title : null))
                        {
                            if (!string.IsNullOrEmpty(str))
                                str += ", ";
                            str += this.GroupData.city.title;
                        }

                        if (!string.IsNullOrEmpty(this.GroupData.country != null ? this.GroupData.country.title : null))
                        {
                            if (!string.IsNullOrEmpty(str))
                                str += ", ";
                            str += this.GroupData.country.title;
                        }
                        if (!string.IsNullOrEmpty(str))
                        {
                            Action action = null;
                            //if (this.GroupData.place != null && this.GroupData.place.latitude != 0.0 && this.GroupData.place.longitude != 0.0)
                            //    action = (Action)(() => Navigator.Current.NavigateToMap(false, this.GroupData.place.latitude, this.GroupData.place.longitude));
                            InfoListItem infoListItem1 = new InfoListItem();
                            infoListItem1.IconUrl = "\xEB49";
                            infoListItem1.Text = str;
                            infoListItem1.TapAction = action;
                            int num = action != null ? 1 : 0;
                            infoListItem1.IsTiltEnabled = num != 0;
                            InfoListItem infoListItem2 = infoListItem1;
                            infoListItemList1.Add(infoListItem2);
                        }
                    }
                    if (this.GroupData.members_count > 0)
                    {
                        InfoListItem infoListItem1 = new InfoListItem();
                        infoListItem1.IconUrl = "\xE77B";
                        infoListItem1.Text = this.ComposeInlinesForMembers();
                        infoListItem1.TapAction = () => { NavigatorImpl.Instance.NavigateToCommunitySubscribers(this.GroupData.id); };
                        // int num = 1;
                        //  infoListItem1.IsTiltEnabled = num != 0;
                        //  Action action = (Action)(() => Navigator.Current.NavigateToCommunitySubscribers(group.id, group.GroupType, false, false, false));
                        //   infoListItem1.TapAction = action;
                        //   InfoListItem infoListItem2 = infoListItem1;
                        List<VKUser> friends = this.GroupData.friends.items;
                        if (friends != null)
                        {
                            if (friends.Count > 0)
                                infoListItem1.Preview1 = friends[0].photo_50;
                            if (friends.Count > 1)
                                infoListItem1.Preview2 = friends[1].photo_50;
                            if (friends.Count > 2)
                                infoListItem1.Preview3 = friends[2].photo_50;
                        }
                        infoListItemList1.Add(infoListItem1);
                    }

                    if (!string.IsNullOrEmpty(this.GroupData.description))
                    {
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xE71D";
                        infoListItem.Text = this.GroupData.description;
                        infoListItemList1.Add(infoListItem);
                    }

                    if (!string.IsNullOrEmpty(this.GroupData.screen_name))
                    {
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xE71B";
                        infoListItem.Text = this.GroupData.screen_name;
                        infoListItemList1.Add(infoListItem);
                    }

                    if (!string.IsNullOrEmpty(this.GroupData.site))
                    {
                        InfoListItem infoListItem = new InfoListItem();
                        infoListItem.IconUrl = "\xE774";
                        infoListItem.Text = this.GroupData.site;
                        infoListItemList1.Add(infoListItem);
                    }
                }

                return infoListItemList1;
            }
        }
        */
        private void UpdateData(string status)
        {
            this.User.status = status;
            base.NotifyPropertyChanged(nameof(this.CompactinfoViewModel));

            if (this.Id == Settings.UserId)
                EventAggregator.Instance.PublishProfileStatusChangedEvent(status);
        }

        public void OpenSetStatusPopup()
        {
            string status = this.User.activity;

            EditStatusUC editStatusUC = new EditStatusUC();
            editStatusUC.TextBoxText.Text = status;
            PopUpService statusChangePopup = new PopUpService
            {
                Child = editStatusUC
            };

            Action updateStatusAction = delegate
            {
                string newStatus = editStatusUC.TextBoxText.Text.Replace("\r\n", "\r").Replace("\r", "\r\n");

                AccountService.Instance.StatusSet(newStatus, "", 0, (res) =>
                {
                    if (res.error.error_code == VKErrors.None)
                    {
                        
                            //AppGlobalStateManager.Current.UpdateCachedUserStatus(newStatus);
                            //EventAggregator.Current.Publish(new BaseDataChangedEvent());
                            //bool flag = groupId > 0;
                            //long id = flag ? groupId : AppGlobalStateManager.Current.LoggedInUserId;
                            //EventAggregator.Current.Publish(new ProfileStatusChangedEvent(id, flag, newStatus));
                        

                        Execute.ExecuteOnUIThread(() => {
                            this.UpdateData(newStatus);
                        });
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

        

        private string ComposeInlinesForFriends
        {
            get
            {
                string ret = UIStringFormatterHelper.FormatNumberOfSomething(this.User.Counters.friends, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");

                if (this.User.Counters.mutual_friends > 0)
                {
                    ret += " · ";
                    string str5 = UIStringFormatterHelper.FormatNumberOfSomething(this.User.Counters.mutual_friends, "OneMutualFrm", "TwoFourMutualFrm", "FiveMutualFrm");
                    ret += str5;
                }
                return ret;
            }
        }

        private string ComposeInlinesForFollowers()
        {
            string ret = UIStringFormatterHelper.FormatNumberOfSomething(this.User.Counters.followers, "OneFollowerFrm", "TwoFourFollowersFrm", "FiveFollowersFrm");

            return ret;
        }
        /*
         * private void UpdateAppBar()
        {
            if (base.ImageViewerDecorator != null && base.ImageViewerDecorator.IsShown || (base.IsMenuOpen || base.Flyouts.Count > 0))
                return;
            this._defaultAppBar.MenuItems.Clear();
            if (this._viewModel.CanPost || this._viewModel.CanSuggestAPost)
            {
                if (!this._defaultAppBar.Buttons.Contains(this._appBarButtonAddNews))
                {
                    this._appBarButtonAddNews.Text = (this._viewModel.CanPost ? CommonResources.MainPage_News_AddNews : CommonResources.SuggestedNews_SuggestAPost);
                    this._defaultAppBar.Buttons.Add(this._appBarButtonAddNews);
                }
            }
            else if (this._defaultAppBar.Buttons.Contains(this._appBarButtonAddNews))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonAddNews);
            if (this._viewModel.CanSendGift)
            {
                if (!this._defaultAppBar.Buttons.Contains(this._appBarButtonSendGift))
                    this._defaultAppBar.Buttons.Add(this._appBarButtonSendGift);
            }
            else if (this._defaultAppBar.Buttons.Contains(this._appBarButtonSendGift))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonSendGift);
            if (this._viewModel.CanManageCommunity && !this._defaultAppBar.Buttons.Contains(this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Add(this._appBarButtonManagement);
            if (!this._viewModel.CanManageCommunity && this._defaultAppBar.Buttons.Contains(this._appBarButtonManagement))
                this._defaultAppBar.Buttons.Remove(this._appBarButtonManagement);
            if (this._viewModel.CanEditProfile)
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemEditProfile);
            if (this._viewModel.CanSubscribeUnsubscribe)
            {
                this._appBarMenuItemSubscribeUnsubscribe.Text = (this._viewModel.IsSubscribed ? CommonResources.UnsubscribeFromNews : CommonResources.SubscribeToNews);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemSubscribeUnsubscribe);
            }
            if (this._viewModel.CanPinToStart)
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemPinToStart);
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemCopyLink);
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemOpenInBrowser);
            if (this._viewModel.CanFaveUnfave)
            {
                this._appBarMenuItemFaveUnfave.Text = (this._viewModel.IsFavorite ? CommonResources.RemoveFromBookmarks : CommonResources.AddToBookmarks);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemFaveUnfave);
            }
            if (this._viewModel.CanBanUnban)
            {
                this._appBarMenuItemBanUnban.Text = (this._viewModel.IsBlacklistedByMe ? CommonResources.BannedUsers_UnbanUser : CommonResources.BannedUsers_BanUser);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemBanUnban);
            }
            if (this._defaultAppBar.MenuItems.Count > 0 || this._defaultAppBar.Buttons.Count > 0)
            {
                base.ApplicationBar = ((IApplicationBar)this._defaultAppBar);
                base.ApplicationBar.Mode = (this._defaultAppBar.Buttons.Count == 0 ? (ApplicationBarMode)1 : (ApplicationBarMode)0);
            }
            else
                base.ApplicationBar = (null);
        }
         * */
        private string ComposeTextForBirthday(string bdate)
        {
            string[] strArray = bdate.Split('.');
            if (strArray.Length < 2)
                return "";
            int num1 = int.Parse(strArray[0]);
            int month = int.Parse(strArray[1]);
            int num2 = 0;
            if (strArray.Length > 2)
                num2 = int.Parse(strArray[2]);
            string str = string.Format("{0} {1}", num1, UIStringFormatterHelper.GetOfMonthStr(month));
            if (num2 != 0)
                str += string.Format(" {0}", num2);
            return string.Format("{0}: {1}", LocalizedStrings.GetString( "ProfilePage_Info_Birthday"), str);
            //return str;
        }

        private DateTime GetBirthday(string bdate)
        {
            string[] strArray = bdate.Split('.');
            if (strArray.Length < 2)
                return DateTime.MinValue;
            int day = int.Parse(strArray[0]);
            DateTime dateTime = new DateTime(DateTime.Now.Year, int.Parse(strArray[1]), day);
            if (DateTime.Now > dateTime)
                dateTime = dateTime.AddYears(1);
            return dateTime;
        }

        public bool CanSendGift
        {
            get
            {
                return this.User != null && this.Id != Settings.UserId && (this.User.blacklisted != true && this.User.deactivated == VKIsDeactivated.None);
            }
        }

        public bool CanFaveUnfave
        {
            get
            {
                return this.Id != Settings.UserId && this.User.deactivated == VKIsDeactivated.None;
            }
        }

        public string OnlineStatus
        {
            get
            {
                if (this.User == null)
                    return string.Empty;

                return this.User.GetUserStatusString();
            }
        }

        

        public Visibility VisibilityButtonSendMessage
        {
            get
            {
                if (this.User != null)
                {
                    if (this.User.id != Settings.UserId && this.User.can_write_private_message != false && this.User.Deactivated == VKIsDeactivated.None)
                        return Visibility.Visible;

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
                if (this.User != null)
                {
                    if (this.User.Deactivated != VKIsDeactivated.None)
                        return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public string SubscribeGlyph
        {
            get
            {
                if (this.User != null)
                {
                    if (this.Id == Settings.UserId)
                        return "\xE70F";

                    switch (this.User.friend_status)
                    {
                        case VKUsetMembershipType.Friends:
                            return "\xE8FB";//галочка
                        //case VKUsetMembershipType.RequestReceived:
                        //case VKUsetMembershipType.No:
                        //    {
                        //        return "\xE710";
                        //    }
                        default:
                            return "\xE710";
                    }
                }
                return "";
            }
        }

        public string TextButtonSecondary
        {
            get
            {
                if (this.User != null)
                {
                    if (this.User.id == Settings.UserId)
                        return LocalizedStrings.GetString("Profile_Edit");

                    //TextButtonSecondaryAction
                    switch (this.User.friend_status)
                    {
                        case VKUsetMembershipType.No:
                            return LocalizedStrings.GetString(this.User.can_send_friend_request == true ? "Profile_AddToFriends" : "Profile_Follow");
                        case VKUsetMembershipType.RequestReceived:
                            return LocalizedStrings.GetString(this.User.IsFemale ? "Profile_FollowingYou_Female" : "Profile_ReplyToRequest");
                        case VKUsetMembershipType.Friends:
                            return LocalizedStrings.GetString("Profile_YouAreFriends");
                        case VKUsetMembershipType.RequestSent:
                            return LocalizedStrings.GetString("Profile_YouAreFollowing");
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
                if (this.User != null)
                {
                    return this.User.is_favorite ? "\xE735" : "\xE734";
                }
                return "";
            }
        }

        public string FavoriteText
        {
            get
            {
                if (this.User != null)
                {
                    return this.User.is_favorite ? "В избранном" : "Избранное";
                }
                return "";
            }
        }


        public string BlockText
        {
            get
            {
                if (this.User != null)
                {
                    return LocalizedStrings.GetString(this.IsBlacklistedByMe ? "BannedUsers_UnbanUser" : "BannedUsers_BanUser");
                }
                return "";
            }
        }
        #endregion



        public bool CanPost
        {
            get
            {
                if (this.IsServiceProfile)
                    return false;

                

                if (this.User != null)
                {
                    if (this.User.Deactivated > 0)
                        return false;

                    if (this.User.id == Settings.UserId)
                        return false;
                    return this.User.can_post;
                }
                
                return false;
            }
        }

        /*
         * public bool CanSuggestAPost
        {
            get
            {
                if (this._profileData != null && this._profileData.CanSuggestAPost && !this.IsServiceProfile)
                    return !this._profileData.IsDeactivated;
                return false;
            }
        }
        */

        public Visibility CanPostVisibility
        {
            get
            {
                if (this.CanPost /*|| this.CanSuggestAPost*/)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public bool IsServiceProfile
        {
            get
            {
                return this._serviceUserIds.Contains(this.Id);
            }
        }

        /*
        public bool CanSuggestAPost
        {
            get
            {
                if (this._profileData != null && this._profileData.CanSuggestAPost && !this.IsServiceProfile)
                    return !this._profileData.IsDeactivated;
                return false;
            }
        }
        */
        public bool CanSubscribeUnsubscribe
        {
            get
            {
                if (this.User == null || this.Id == Settings.UserId || this.IsServiceProfile)
                    return false;
                
                if (this.Id > 0 && (this.User == null || this.User.blacklisted == true))
                    return false;
                return this.User.Deactivated == VKIsDeactivated.None;
            }
        }

        public bool IsSubscribed
        {
            get
            {
                if (this.User != null)
                    return this.User.is_subscribed;
                return false;
            }
            set
            {
                if (this.User == null)
                    return;
                this.User.is_subscribed = value;
                //Action barPropertyUpdated = this.AppBarPropertyUpdated;
                //if (barPropertyUpdated == null)
                //    return;
                //barPropertyUpdated();
            }
        }

        public Visibility NotificationsVisibility
        {
            get
            {
                if (this.User == null)
                    return Visibility.Collapsed;

                if (this.User.blacklisted || this.User.deactivated != VKIsDeactivated.None)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public void SubscribeUnsubscribe(Action<bool> callback = null)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = this.Id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(this.IsSubscribed ? "wall.unsubscribe" : "wall.subscribe", parameters, (result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.User.is_subscribed = !this.User.is_subscribed;
                        base.NotifyPropertyChanged(nameof(this.IsSubscribed));
                    }
                    callback?.Invoke(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });
        }

        public bool IsFavorite
        {
            get
            {
                if (this.User != null)
                    return this.User.is_favorite;
                return false;
            }
            set
            {
                if (this.User == null)
                    return;
                this.User.is_favorite = value;
                //Action barPropertyUpdated = this.AppBarPropertyUpdated;
                //if (barPropertyUpdated == null)
                //    return;
                //barPropertyUpdated();
            }
        }

        public void FaveUnfave(Action<bool> callback = null)
        {
            Action<VKResponse<int>> action = new Action<VKResponse<int>>((result) =>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (result.error.error_code == VKErrors.None && result.response == 1)
                    {
                        this.User.is_favorite = !this.User.is_favorite;
                        base.NotifyPropertyChanged(nameof(this.IsFavorite));
                        base.NotifyPropertyChanged(nameof(this.FavoriteGlyph));
                        base.NotifyPropertyChanged(nameof(this.FavoriteText));
                    }

                    callback?.Invoke(result.error.error_code == VKErrors.None ? result.response == 1 : false);
                });
            });

                FavoritesService.Instance.FaveAddRemoveUser((uint)this.Id, !this.IsFavorite, action);
            
        }

        private void UpdateInfoData()
        {
            if (this.User != null)
            {
                /*
                if (!string.IsNullOrEmpty(this.User.home_town))
                    this.InfoSections.Add(new ProfileInfoItem("GroupPage_Info", "\xE80F", LocalizedStrings.GetString( "ProfilePage_Info_Hometown") + " "+ this.User.home_town));

                if (this.User.personal != null && this.User.personal.langs != null && this.User.personal.langs.Count > 0)
                    this.InfoSections.Add(new ProfileInfoItem("GroupPage_Info", "\xF2B7", LocalizedStrings.GetString("ProfilePage_Info_Languages") + " "+ this.User.personal.langs.GetCommaSeparated()));
                
                if (this.User.relatives != null && this.User.relatives.Count > 0)
                {
                    foreach (var relative in this.User.relatives)
                    {

                        string str = "";

                        switch (relative.type)
                        {
                            case "grandparent":
                                {
                                    str = "ProfilePage_Info_Grandparents";
                                    break;
                                }
                            case "parent":
                                {
                                    str = "ProfilePage_Info_Parents";
                                    break;
                                }
                            case "sibling":
                                {
                                    str = "ProfilePage_Info_Siblings";
                                    break;
                                }
                            case "child":
                                {
                                    str = "ProfilePage_Info_Children";
                                    break;
                                }
                            case "grandchild":
                                {
                                    str = "ProfilePage_Info_Grandchildren";
                                    break;
                                }
                        }

                        string localized = LocalizedStrings.GetString(str); ;

                        if (this.User.relativesUsers != null && relative.id > 0)
                        {

                            VKUser user = this.User.relativesUsers.FirstOrDefault(u => u.id == relative.id);
                            if (user != null)
                            {
                                if (user.deactivated == VKIsDeactivated.Deleted)
                                    continue;

                                this.InfoSections.Add(new ProfileInfoItem(localized, user.Title));
                            }
                            else
                            {
                                this.InfoSections.Add(new ProfileInfoItem(localized, relative.name));
                            }
                        }
                        else
                        {
                            this.InfoSections.Add(new ProfileInfoItem(localized, relative.name));
                        }
                    }
                }

 //               if (this.InfoSections.Count > 0)
 //                   this.InfoSections.Add(genericInfo);




                #region Контакты
                ProfileInfoSectionItem contactsInfo = new ProfileInfoSectionItem("ProfilePage_Info_ContactInformation");

                //profileInfoItemList2.AddRange((IEnumerable<ProfileInfoItem>)PhoneItem.GetPhones(this.User));
                if (!string.IsNullOrEmpty(this.User.mobile_phone))
                    contactsInfo.Items.Add(new ProfileInfoItem("\xE717", this.User.mobile_phone, ProfileInfoItemType.Contact) );
                if (!string.IsNullOrEmpty(this.User.home_phone))
                    contactsInfo.Items.Add(new ProfileInfoItem("\xE717", this.User.home_phone, ProfileInfoItemType.Contact) );



                string _domain = string.IsNullOrEmpty(this.User.domain) ? string.Format("id{0}", this.User.id) : this.User.domain;
                contactsInfo.Items.Add(new ProfileInfoItem("\xE94B", _domain, ProfileInfoItemType.Contact));
                */
                /*
                if (!string.IsNullOrEmpty(this.User.skype))
                    profileInfoItemList2.Add(new SkypeSocialNetworkItem(this.User.skype));
                if (!string.IsNullOrEmpty(this.User.facebook))
                    profileInfoItemList2.Add(new FacebookSocialNetworkItem(this.User.facebook, this.User.facebook_name));
                if (!string.IsNullOrEmpty(this.User.twitter))
                    profileInfoItemList2.Add(new TwitterSocialNetworkItem(this.User.twitter));
                if (!string.IsNullOrEmpty(this.User.instagram))
                    profileInfoItemList2.Add(new InstagramSocialNetworkItem(this.User.instagram));*/

                /*
                if (!string.IsNullOrEmpty(this.User.site))
                {
                    Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(this.User.site));
                    contactsInfo.Items.Add(new ProfileInfoItem("\xE774", this.User.site, ProfileInfoItemType.Contact) { NavigationAction = NavigationAction });
                }
//                if (contactsInfo.Items.Count > 0)
//                    this.InfoSections.Add(contactsInfo);
                #endregion







                ProfileInfoSectionItem educationInfo = new ProfileInfoSectionItem("ProfilePage_Info_Education");
                //                profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>)UniversityItem.GetUniversities(this.User.universities, this.User.universityGroups));
                //                profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>)SchoolItem.GetSchools(this.User.schools, this.User.schoolGroups));
   //             if (educationInfo.Items.Count > 0)
  //                  this.InfoSections.Add(educationInfo);

                #region Жизненная позиция
                ProfileInfoSectionItem beliefslInfo = new ProfileInfoSectionItem("ProfilePage_Info_Beliefs");
                var personal = this.User.personal;
                if (personal != null)
                {
                    if (personal.political > 0)
                    {
                        string str = "";
                        switch (personal.political)
                        {
                            case 1:
                                str = "PoliticalViews_Communist";
                                break;
                            case 2:
                                str = "PoliticalViews_Socialist";
                                break;
                            case 3:
                                str = "PoliticalViews_Moderate";
                                break;
                            case 4:
                                str = "PoliticalViews_Liberal";
                                break;
                            case 5:
                                str = "PoliticalViews_Conservative";
                                break;
                            case 6:
                                str = "PoliticalViews_Monarchist";
                                break;
                            case 7:
                                str = "PoliticalViews_Ultraconservative";
                                break;
                            case 8:
                                str = "PoliticalViews_Apathetic";
                                break;
                            case 9:
                                str = "PoliticalViews_Libertarian";
                                break;
                        }

                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_PoliticalViews", LocalizedStrings.GetString(str)));
                    }

                    if (!string.IsNullOrEmpty(personal.religion))
                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_WorldView", personal.religion));

                    if (personal.life_main > 0)
                    {
                        string str = "";
                        switch (personal.life_main)
                        {
                            case 1:
                                str = "PersonalPriority_FamilyAndChildren";
                                break;
                            case 2:
                                str = "PersonalPriority_CareerAndMoney";
                                break;
                            case 3:
                                str = "PersonalPriority_EntertainmentAndLeisure";
                                break;
                            case 4:
                                str = "PersonalPriority_ScienceAndResearch";
                                break;
                            case 5:
                                str = "PersonalPriority_ImprovingTheWorld";
                                break;
                            case 6:
                                str = "PersonalPriority_PersonalDevelopment";
                                break;
                            case 7:
                                str = "PersonalPriority_BeautyAndArt";
                                break;
                            case 8:
                                str = "PersonalPriority_FameAndInfluence";
                                break;
                        }

                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_PersonalPriority", LocalizedStrings.GetString(str)));
                    }

                    if (personal.people_main > 0)
                    {
                        string str = "";
                        switch (personal.people_main)
                        {
                            case 1:
                                str = "ImportantInOthers_IntellectAndCreativity";
                                break;
                            case 2:
                                str = "ImportantInOthers_KindnessAndHonesty";
                                break;
                            case 3:
                                str = "ImportantInOthers_HealthAndBeauty";
                                break;
                            case 4:
                                str = "ImportantInOthers_WealthAndPower";
                                break;
                            case 5:
                                str = "ImportantInOthers_CourageAndPersistance";
                                break;
                            case 6:
                                str = "ImportantInOthers_HumorAndLoveForLife";
                                break;
                        }

                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_ImportantInOthers", LocalizedStrings.GetString(str)));
                    }
                    if (personal.smoking > 0)
                    {
                        string str = "";
                        switch (personal.smoking)
                        {
                            case 1:
                                str = "BadHabitsViews_VeryNegative";
                                break;
                            case 2:
                                str = "BadHabitsViews_Negative";
                                break;
                            case 3:
                                str = "BadHabitsViews_Compromisable";
                                break;
                            case 4:
                                str = "BadHabitsViews_Neutral";
                                break;
                            case 5:
                                str = "BadHabitsViews_Positive";
                                break;
                        }

                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_ViewsOnSmoking", LocalizedStrings.GetString(str)));
                    }
                    if (personal.alcohol > 0)
                    {
                        string str = "";
                        switch (personal.alcohol)
                        {
                            case 1:
                                str = "BadHabitsViews_VeryNegative";
                                break;
                            case 2:
                                str = "BadHabitsViews_Negative";
                                break;
                            case 3:
                                str = "BadHabitsViews_Compromisable";
                                break;
                            case 4:
                                str = "BadHabitsViews_Neutral";
                                break;
                            case 5:
                                str = "BadHabitsViews_Positive";
                                break;
                        }

                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_ViewsOnAlcohol", LocalizedStrings.GetString(str)));
                    }
                    if (!string.IsNullOrEmpty(personal.inspired_by))
                        beliefslInfo.Items.Add(new ProfileInfoItem("ProfilePage_Info_InspiredBy", personal.inspired_by));
                }

//                if (beliefslInfo.Items.Count > 0)
 //                   this.InfoSections.Add(beliefslInfo);
                #endregion

                #region Персональная информация
                ProfileInfoSectionItem personalInfo = new ProfileInfoSectionItem("ProfilePage_Info_PersonalInfomation");
                if (!string.IsNullOrEmpty(this.User.activities))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Activities", this.User.activities));
                if (!string.IsNullOrEmpty(this.User.interests))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Interests", this.User.interests));
                if (!string.IsNullOrEmpty(this.User.music))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Music", this.User.music));
                if (!string.IsNullOrEmpty(this.User.movies))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Movies", this.User.movies));
                if (!string.IsNullOrEmpty(this.User.tv))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_TV", this.User.tv));
                if (!string.IsNullOrEmpty(this.User.books))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Books", this.User.books));
                if (!string.IsNullOrEmpty(this.User.games))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Games", this.User.games));
                if (!string.IsNullOrEmpty(this.User.quotes))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_Quotes", this.User.quotes));
                if (!string.IsNullOrEmpty(this.User.about))
                    viewModel.InfoSections.Add(new ProfileInfoItem("ProfilePage_Info_About", this.User.about));
                
 //               if (viewModel.InfoSections.Count > 0)
 //                   this.InfoSections.Add(personalInfo);
                #endregion
                */
                /*
                List<ProfileInfoItem> profileInfoItemList6 = new List<ProfileInfoItem>();
                if (!((IList)this.User.military).IsNullOrEmpty())
                {
                    profileInfoItemList6.AddRange((IEnumerable<ProfileInfoItem>)MilitaryItem.GetMilitaryItems(this.User.military, this.User.militaryCountries));
                    if (profileInfoItemList6.Count > 0)
                        this.InfoSections.Add(new ProfileInfoSectionItem("ProfilePage_Info_MilitaryService")
                        {
                            Items = profileInfoItemList6
                        });
                }
                List<ProfileInfoItem> profileInfoItemList7 = new List<ProfileInfoItem>();
                CareerData careerData = this.User.careerData;
                if (careerData != null && !((IList)careerData.Items).IsNullOrEmpty())
                {
                    profileInfoItemList7.AddRange((IEnumerable<ProfileInfoItem>)CareerItem.GetCareerItems(careerData.Items, careerData.Cities, careerData.Groups));
                    if (profileInfoItemList7.Count > 0)
                        this.InfoSections.Add(new ProfileInfoSectionItem("ProfilePage_Info_Career")
                        {
                            Items = profileInfoItemList7
                        });
                }
                if (this.InfoSections.Count <= 0)
                    return;
                ((ProfileInfoSectionItem)Enumerable.Last<ProfileInfoSectionItem>(this.InfoSections)).DividerVisibility = Visibility.Collapsed;*/
            }

        }

        private void CreateData()
        {
            this.CompactinfoViewModel.InfoSections.Clear();

            if (this.User != null)
            {
                //ProfileInfoSectionItem genericInfo = new ProfileInfoSectionItem("");
                /*
                // 1
                if (this.User.id == Settings.UserId || !string.IsNullOrEmpty(this.User.activity))
                {
                    var item = new CustomProfileInfoItem("\xEC42", string.IsNullOrEmpty(this.User.activity) ? LocalizedStrings.GetString("ChangeStatusText") : this.User.activity);
                    if (this.User.id == Settings.UserId)
                    {
                        item.NavigationAction = () => { this.OpenSetStatusPopup(this.User.activity, 0, this.UpdateData); };
                    }
                    this.FullInfoViewModel.InfoSections.Add(item);
                }
                */
                
                // 2
                if (this.User.counters != null && this.User.counters.friends > 0)
                {
                    var infoListItem = new InfoListItem("\xE77B", this.ComposeInlinesForFriends);

                    bool flag = true;
                    if (this.User.is_closed && (byte)this.User.friend_status < 2)
                        flag = false;

                    if (flag)
                    {
                        infoListItem.NavigationAction = () => { NavigatorImpl.Instance.NavigateToFriends((int)this.User.id, this.User.first_name_gen); };
                    }

                    List<VKUser> friends = this.User.randomMutualFriends;
                    if (friends != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var f in friends)
                            l.Add(f.photo_50);

                        infoListItem.Previews = l;
                    }

                    this.CompactinfoViewModel.InfoSections.Add(infoListItem);
                }

                // 3
                if (!string.IsNullOrEmpty(this.User.bdate))
                    this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xEC92", this.ComposeTextForBirthday(this.User.bdate)) );

                // 4
                if (this.User.city != null)
                    this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE80F", this.User.city.title));


                //образование, поле education

                #region Семейное положение
                if (this.User.relation != 0)
                {
                    string str1 = "";
                    string str2 = "";
                    bool flag = this.User.sex != VKUserSex.Female;
                    int num = this.User.relation_partner == null ? 0 : User.relation_partner.id;
                    var partner = this.User.relation_partner;

                    switch (this.User.relation)
                    {
                        case RelationshipStatus.NotMarried:
                            str1 = flag ? "ProfilePage_Info_NotMarriedMale" : "ProfilePage_Info_NotMarriedFemale";
                            break;
                        case RelationshipStatus.InARelationship:
                            if (num > 0)
                            {
                                str1 = "ProfilePage_Info_InARelationshipWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "ProfilePage_Info_InARelationship";
                            break;
                        case RelationshipStatus.Engaged:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_EngagedMaleWith" : "ProfilePage_Info_EngagedFemaleWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_EngagedMale" : "ProfilePage_Info_EngagedFemale";
                            break;
                        case RelationshipStatus.Married:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_MarriedMaleWith" : "ProfilePage_Info_MarriedFemaleWith";
                                str2 = flag ? partner.abl : partner.ins;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_MarriedMale" : "ProfilePage_Info_MarriedFemale";
                            break;
                        case RelationshipStatus.ItIsComplicated:
                            if (num > 0)
                            {
                                str1 = "ProfilePage_Info_ItIsComplicatedWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "ProfilePage_Info_ItIsComplicated";
                            break;
                        case RelationshipStatus.ActivelySearching:
                            str1 = "ProfilePage_Info_ActivelySearching";
                            break;
                        case RelationshipStatus.InLove:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_InLoveMaleWith" : "ProfilePage_Info_InLoveFemaleWith";
                                str2 = partner.acc;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_InLoveMale" : "ProfilePage_Info_InLoveFemale";
                            break;
                        case RelationshipStatus.InCivilUnion:
                            if (num > 0)
                            {
                                str1 = "InCivilUnionWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "InCivilUnion";
                            break;
                    }

                    string data = LocalizedStrings.GetString(str1);

                    if (num > 0)
                        data += string.Format(" [id{0}|{1}]", num, str2);

                    this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xEB51", data));//ProfilePage_Info_RelStatus
                }
                #endregion

                #region Род занятия
                // 5
                var occupation = this.User.occupation;
                if (occupation != null)
                {
                    if (occupation.type == VKOccupation.OccupationType.university && this.User.universities != null)
                    {
                        var university = this.User.universities.FirstOrDefault(u => u.id == occupation.id);
                        if (university != null && university.graduation > 0)
                            occupation.name = string.Format("{0} '{1:00}", university.name, (university.graduation % 100));
                    }
                    else if (occupation.type == VKOccupation.OccupationType.school && this.User.schools != null)
                    {
                        var enumerator = this.User.schools.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                var current = enumerator.Current;

                                if (current.id == occupation.id && current.year_graduated > 0)
                                    occupation.name = string.Format("{0} '{1:00}", current.name, (current.year_graduated % 100));
                            }
                        }
                        finally
                        {
                            enumerator.Dispose();
                        }
                    }

                    string Preview = "";
                    string data = ""; ;
                    string icon = "\xE7BE";
                    Action action = null;

                    if (occupation.type == VKOccupation.OccupationType.work)
                    {
                        if (this.User.occupationGroup != null)
                        {
                            Preview = this.User.occupationGroup.photo_100;
                            action = () => NavigatorImpl.Instance.NavigateToProfilePage((int)(-occupation.id));
                        }

                        icon = "\xE821";
                        data = LocalizedStrings.GetString("OccupationType_Work") + ": " + occupation.name;

                    }
                    else
                    {
                        icon = "\xE7BE";
                        data = LocalizedStrings.GetString("OccupationType_SchoolUniversity") + ": " + occupation.name;
                    }

                    this.CompactinfoViewModel.InfoSections.Add(new InfoListItem(icon, data) { NavigationAction = action });
                }
                #endregion



                if (this.User.is_closed)
                {
                    this.CompactinfoViewModel.InfoSections.Add(new CustomProfileInfoItem("\xE72E", "Это закрытый профиль") );
                }
            }
            
        }

        

        public ProfileInfoFullViewModel FullInfoRight
        {
            get
            {
                if (this.User == null)
                    return null;

                return this.GetFullInfoViewModel(false);
            }
        }

        public void ShowFullInfoPopup(Action<bool> callback)
        {
            
                UsersService.Instance.GetProfileInfo(this.Id, false, (result) =>
                 {

                     if (result.error.error_code != VKErrors.None)
                     {
                         Execute.ExecuteOnUIThread(() =>
                         {
                             callback(false);
                         });
                         return;
                     }

                     VKUser user = result.response;


                     this.User.home_town = user.home_town;
                     this.User.personal = user.personal;
                     this.User.relatives = user.relatives;
                     this.User.mobile_phone = user.mobile_phone;
                     this.User.home_phone = user.home_phone;
                     this.User.city = user.city;
                     this.User.country = user.country;
                     this.User.domain = user.domain;
                     this.User.site = user.site;

                     this.User.activities = user.activities;
                     this.User.interests = user.interests;
                     this.User.music = user.music;
                     this.User.movies = user.movies;
                     this.User.tv = user.tv;
                     this.User.books = user.books;
                     this.User.games = user.games;
                     this.User.quotes = user.quotes;
                     this.User.about = user.about;
                     this.User.relativesUsers = user.relativesUsers;


                     Execute.ExecuteOnUIThread(() =>
                     {
                         callback(true);
                         //this.UpdateInfoData();
                         base.NotifyPropertyChanged(nameof(this.FullInfoRight));
                     });
                 });
            
        }

        public string LoadingStatusText
        {
            get
            {
                switch (base.CurrentLoadingStatus)
                {
                    case ProfileLoadingStatus.Reloading:
                    case ProfileLoadingStatus.Loading:
                        return LocalizedStrings.GetString("Loading/Text");
                    case ProfileLoadingStatus.LoadingFailed:
                        return LocalizedStrings.GetString("FailedToConnectError");
                    default:
                        if (this.User != null)
                        {
                            if(this.IsServiceProfile)
                            {
                                switch (this.Id)
                                {
                                    case 100:
                                        return LocalizedStrings.GetString("User100_Description");
                                    case 101:
                                        return LocalizedStrings.GetString("User101_Description");
                                    case 333:
                                        return LocalizedStrings.GetString("User333_Description");
                                    default:
                                        return "";
                                }
                            }
                            else if(this.User.deactivated == VKIsDeactivated.Deleted)
                            {
                                return string.Format("{0}\n{1}", LocalizedStrings.GetString("UserDeleted"), LocalizedStrings.GetString("InformationUnavailable"));
                            }
                            else if (this.User.deactivated == VKIsDeactivated.Banned)
                            {
                                //К сожалению, нам пришлось заблокировать страницу Лики за нарушение правил сайта. 
                                return string.Format("{0}\n{1}", LocalizedStrings.GetString("UserBanned"), LocalizedStrings.GetString("InformationUnavailable"));
                            }
                            else if (this.User.blacklisted)
                            {
                                return string.Format("{0}\n{1}", string.Format(LocalizedStrings.GetString("UserBlacklisted"), this.User.first_name), LocalizedStrings.GetString("InformationUnavailable"));
                            }
                            else if (this.User.is_closed && (byte)this.User.friend_status < 2)
                            {
                                return string.Format("{0}\n{1}", LocalizedStrings.GetString("PrivateProfile"), string.Format(LocalizedStrings.GetString(this.User.IsFemale ? "UserPrivateFamale" : "UserPrivateMale"), this.User.first_name_acc));
                            }
                        }
                        
                        return "";
                }
            }
        }

        private string ComposeGroupBlacklistedText()
        {
            /*
            BlockInformation banInfo = this._profileData.ban_info;
            if (banInfo == null)
                return CommonResources.Group_Blacklisted;
            string str = "";
            if (banInfo.reason > 0)
            {
                string reasonById = ProfileViewModel.GetReasonById(banInfo.reason);
                if (!string.IsNullOrEmpty(reasonById))
                    str += string.Format("{0}: {1}", CommonResources.Group_BanReason, reasonById);
            }
            if (banInfo.end_date > 0L)
            {
                DateTime dateTime = Extensions.UnixTimeStampToDateTime((double)banInfo.end_date, true);
                if (!string.IsNullOrEmpty(str))
                    str += "\n";
                str += string.Format("{0}: {1}", CommonResources.Group_Blacklisted_BlockedTill, UIStringFormatterHelper.FormateDateForEventUI(dateTime));
            }
            if (!string.IsNullOrEmpty(banInfo.comment))
            {
                if (!string.IsNullOrEmpty(str))
                    str += "\n";
                str += string.Format("{0}: {1}", CommonResources.Group_Blacklisted_Comment, banInfo.comment);
            }
            return str.Insert(0, CommonResources.Group_Blacklisted + (!string.IsNullOrEmpty(str) ? "\n\n" : ""));*/
            return "";
        }
        
        public bool CanBanUnban
        {
            get
            {
                if (this.Id != Settings.UserId && !this.IsServiceProfile)
                    return true;//this._profileData.Deactivated == VKIsDeactivated.None;
                return false;
            }
        }

        public void BanUnban(Action<bool> callback = null)
        {
            //if (this.Id < 0)
            //    return;
            //base.SetInProgress(true, "");
            if (this.IsBlacklistedByMe)
            {
                List<int> longList = new List<int>() { (int)this.Id };
                // Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>> action = (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>)(res => Execute.ExecuteOnUIThread((Action)(() =>
                //{
                //base.SetInProgress(false, "");
                //GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.BannedUsers_UserIsUnbanned, null);
                //if (res.ResultCode != ResultCode.Succeeded)
                //    return;
                //this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                //EventAggregator current = EventAggregator.Current;
                //UserIsBannedOrUnbannedEvent bannedOrUnbannedEvent = new UserIsBannedOrUnbannedEvent();
                //bannedOrUnbannedEvent.IsBanned = false;//todo:bug
                //User user = this._userData.user;
                //bannedOrUnbannedEvent.user = user;
                //current.Publish(bannedOrUnbannedEvent);
                //this.LoadInfo(true);
                //})));
                AccountService.Instance.UnbanUsers(longList, (res =>
                {
                    

                    Execute.ExecuteOnUIThread(() =>
                    {
                        GenericInfoUC.ShowBasedOnResult(LocalizedStrings.GetString("BannedUsers_UserIsUnbanned"), res.error);

                        callback?.Invoke(res.error.error_code == VKErrors.None);

                        if(res.error.error_code == VKErrors.None)
                            this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                    });
                }));
            }
            else
            {
                AccountService.Instance.BanUser((int)this.Id, (res =>
                {
                    //this.SetInProgress(false, "");
                    //GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, CommonResources.BannedUsers_UserIsBanned, null);
                    //if (res.ResultCode != ResultCode.Succeeded)
                    //    return;
                    //this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                    //EventAggregator current = EventAggregator.Current;
                    //UserIsBannedOrUnbannedEvent bannedOrUnbannedEvent = new UserIsBannedOrUnbannedEvent();
                    //bannedOrUnbannedEvent.IsBanned = true;
                    //User user = this._userData.user;
                    //bannedOrUnbannedEvent.user = user;
                    //current.Publish(bannedOrUnbannedEvent);
                    //this.LoadInfo(true);

                    

                    Execute.ExecuteOnUIThread(() =>
                    {
                        GenericInfoUC.ShowBasedOnResult(LocalizedStrings.GetString("BannedUsers_UserIsBanned"), res.error);

                        callback?.Invoke(res.error.error_code == VKErrors.None);
                        if(res.error.error_code == VKErrors.None)
                            this.IsBlacklistedByMe = !this.IsBlacklistedByMe;
                    });
                }));
            }
        }

        public bool IsBlacklistedByMe
        {
            get
            {
                if (this.User != null)
                    return this.User.blacklisted_by_me;
                return false;
            }
            set
            {
                if (this.User == null)
                    return;
                this.User.blacklisted_by_me = value;
                base.NotifyPropertyChanged(nameof(this.BlockText));
            }
        }



        public async void FriendAdd(bool KeepAsFollower)
        {
            if (KeepAsFollower)
            {
                //Оставить в подписчиках
                UsersService.Instance.FriendAccept(this.Id, true, (result) =>
                {
                    if (result == true)
                    {
                        this.User.friend_status = VKUsetMembershipType.RequestReceived;
                        base.NotifyPropertyChanged("TextButtonSecondary");
                    }
                });
            }
            else
            {
                string text = "";

                if (this.User.friend_status == VKUsetMembershipType.No && this.User.can_send_friend_request)
                {
                    // Мы не друзья и нам надо показать окошко
                    // если конечно пользователь не против друзей
                    text = await InputTextDialogAsync("Сопроводительное сообщение");
                    if (text == null)//отменил окошко?
                        return;
                }

                UsersService.Instance.FriendAdd(this.Id, text, (result) =>
                {
                    if (result == true)
                    {
                        if (this.User.friend_status == VKUsetMembershipType.No)
                            this.User.friend_status = VKUsetMembershipType.RequestSent;
                        else if (this.User.friend_status == VKUsetMembershipType.RequestReceived)
                            this.User.friend_status = VKUsetMembershipType.Friends;
                        base.NotifyPropertyChanged("TextButtonSecondary");
                    }
                });
            }
        }

        public void FriendRemove()
        {
            UsersService.Instance.FriendDelete(this.Id, (result) =>
            {
                if (result == true)
                {
                    if (this.User.friend_status == VKUsetMembershipType.Friends)
                        this.User.friend_status = VKUsetMembershipType.RequestReceived;
                    else if (this.User.friend_status == VKUsetMembershipType.RequestSent)
                        this.User.friend_status = VKUsetMembershipType.No;
                    base.NotifyPropertyChanged("TextButtonSecondary");
                }
            });
        }

        private static async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            //dialog.SecondaryButtonText = "Cancel";
            var temp = await dialog.ShowAsync();

            if (temp == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else if (temp == ContentDialogResult.None)
                return null;

            return "";
        }

        public void OpenProfilePhotos()
        {
            if (this.User.blacklisted || this.User.deactivated != VKIsDeactivated.None)
                return;
            NavigatorImpl.Instance.NavigateToImageViewer("profile", ImageViewerViewModel.AlbumType.ProfilePhotos, (int)this.Id, 0, 0, new List<VKPhoto>(), null);
        }

        public void NavigateToNewWallPost()
        {
            NavigatorImpl.Instance.NavigateToNewWallPost();
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
            link += "vk.com/id";
            link += this.Id;


            SecondaryTileCreator.CreateTileFor((int)-this.Id, this.Name, this.User.photo_100, link, (res =>
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

        public ProfileInfoFullViewModel GetFullInfoViewModel(bool addGenericInfo)
        {
            ProfileInfoFullViewModel viewModel = new ProfileInfoFullViewModel();
            
            #region Основная информация
            if (addGenericInfo)
            {
                //ProfileInfoSectionItem genericInfo = new ProfileInfoSectionItem("");
                /*
                // 1
                if (this.User.id == Settings.UserId || !string.IsNullOrEmpty(this.User.activity))
                {
                    var item = new CustomProfileInfoItem("\xEC42", string.IsNullOrEmpty(this.User.activity) ? LocalizedStrings.GetString("ChangeStatusText") : this.User.activity);
                    if (this.User.id == Settings.UserId)
                    {
                        item.NavigationAction = () => { this.OpenSetStatusPopup(this.User.activity, 0, this.UpdateData); };
                    }
                    this.FullInfoViewModel.InfoSections.Add(item);
                }
                */

                if (!string.IsNullOrEmpty(this.User.activity))
                    viewModel.InfoSections.Add(new CustomProfileInfoItem("\xEC42", this.User.activity));

                if (!string.IsNullOrEmpty(this.User.bdate))
                    viewModel.InfoSections.Add(new CustomProfileInfoItem("\xEC92", this.ComposeTextForBirthday(this.User.bdate)));

                if (this.User.city != null)
                    viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE80F", this.User.city.title));

                // 2
                if (this.User.counters != null )
                {
                    if (this.User.counters.friends > 0)
                    {
                        var infoListItem = new InfoListItem("\xE77B", this.ComposeInlinesForFriends);

                        if (!this.User.is_closed)
                        {
                            infoListItem.NavigationAction = () => { NavigatorImpl.Instance.NavigateToFriends((int)this.User.id, this.User.first_name_gen); };
                        }

                        List<VKUser> friends = this.User.randomMutualFriends;
                        if (friends != null)
                        {
                            List<string> l = new List<string>();
                            foreach (var f in friends)
                                l.Add(f.photo_50);

                            infoListItem.Previews = l;
                        }

                        viewModel.InfoSections.Add(infoListItem);
                    }

                    if (this.User.counters.followers>0)
                    {
                        string temp_str = UIStringFormatterHelper.FormatNumberOfSomething(this.User.counters.followers, "OneFollowerFrm", "TwoFourFollowersFrm", "FiveFollowersFrm");
                        viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE701", temp_str));
                    }
                }

                
                var occupation = this.User.occupation;
                if (occupation != null)
                {
                    string str = occupation.name;
                    if (occupation.type == VKOccupation.OccupationType.university && this.User.universities != null)
                    {
                        var university = this.User.universities.FirstOrDefault(u => u.id == occupation.id);
                        if (university != null && university.graduation > 0)
                            str = string.Format("{0} '{1:00}", university.name, (university.graduation % 100));
                    }
                    else if (occupation.type == VKOccupation.OccupationType.school && this.User.schools != null)
                    {
                        var enumerator = this.User.schools.GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                var current = enumerator.Current;

                                if (current.id == occupation.id && current.year_graduated > 0)
                                    str = string.Format("{0} '{1:00}", current.name, (current.year_graduated % 100));
                            }
                        }
                        finally
                        {
                            enumerator.Dispose();
                        }
                    }
                    occupation.name = str;

                    string title = "";
                    string Preview = "";
                    string icon = "";
                    //ProfileInfoItemType type = ProfileInfoItemType.Full;
                    Action NavigationAction = null;

                    if (occupation.type == VKOccupation.OccupationType.work)
                    {
                        title = "OccupationType_Work";
                        icon = "\xE821";
                        if (this.User.occupationGroup != null)
                        {
                            Preview = this.User.occupationGroup.photo_100;
                            NavigationAction = () => NavigatorImpl.Instance.NavigateToProfilePage((int)(-this.User.occupationGroup.id));
                        }
                        var item = new InfoListItem("\xE821", "\xE821");


                    }
                    else
                    {
                        title = "OccupationType_SchoolUniversity";
                        icon = "\xE7BE";
                        //         type = ProfileInfoItemType.Contact;
                    }


                    //     this.FullInfoViewModel.InfoSections.Add(new ProfileInfoItem(icon, LocalizedStrings.GetString(title)+" "+ str, type) { photo = Preview, NavigationAction = NavigationAction });
                }
                /*
                #region Семейное положение
                if (this.User.relation != 0)
                {
                    string str1 = "";
                    string str2 = "";
                    bool flag = this.User.sex != VKUserSex.Female;
                    int num = this.User.relation_partner == null ? 0 : User.relation_partner.id;
                    var partner = this.User.relation_partner;

                    switch (this.User.relation)
                    {
                        case RelationshipStatus.NotMarried:
                            str1 = flag ? "ProfilePage_Info_NotMarriedMale" : "ProfilePage_Info_NotMarriedFemale";
                            break;
                        case RelationshipStatus.InARelationship:
                            if (num > 0)
                            {
                                str1 = "ProfilePage_Info_InARelationshipWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "ProfilePage_Info_InARelationship";
                            break;
                        case RelationshipStatus.Engaged:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_EngagedMaleWith" : "ProfilePage_Info_EngagedFemaleWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_EngagedMale" : "ProfilePage_Info_EngagedFemale";
                            break;
                        case RelationshipStatus.Married:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_MarriedMaleWith" : "ProfilePage_Info_MarriedFemaleWith";
                                str2 = flag ? partner.abl : partner.ins;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_MarriedMale" : "ProfilePage_Info_MarriedFemale";
                            break;
                        case RelationshipStatus.ItIsComplicated:
                            if (num > 0)
                            {
                                str1 = "ProfilePage_Info_ItIsComplicatedWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "ProfilePage_Info_ItIsComplicated";
                            break;
                        case RelationshipStatus.ActivelySearching:
                            str1 = "ProfilePage_Info_ActivelySearching";
                            break;
                        case RelationshipStatus.InLove:
                            if (num > 0)
                            {
                                str1 = flag ? "ProfilePage_Info_InLoveMaleWith" : "ProfilePage_Info_InLoveFemaleWith";
                                str2 = partner.acc;
                                break;
                            }
                            str1 = flag ? "ProfilePage_Info_InLoveMale" : "ProfilePage_Info_InLoveFemale";
                            break;
                        case RelationshipStatus.InCivilUnion:
                            if (num > 0)
                            {
                                str1 = "InCivilUnionWith";
                                str2 = partner.ins;
                                break;
                            }
                            str1 = "InCivilUnion";
                            break;
                    }

                    string data = LocalizedStrings.GetString(str1);

                    if (num > 0)
                        data += string.Format(" [id{0}|{1}]", num, str2);

                    this.InfoSections.Add(new ProfileInfoItem("\xEB51", data) );//ProfilePage_Info_RelStatus
                }
                #endregion

                //                if (this.InfoSections.Count > 0)
                //                   this.InfoSections.Add(genericInfo);
                */

                string _domain = string.IsNullOrEmpty(this.User.domain) ? string.Format("id{0}", this.User.id) : this.User.domain;
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE94B", _domain));

                if (this.User.is_closed)
                {
                    viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE72E", "Это закрытый профиль") );
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(this.User.home_town))
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE80F", LocalizedStrings.GetString("ProfilePage_Info_Hometown") + ": " + this.User.home_town));//"GroupPage_Info"

            if (this.User.personal != null && this.User.personal.langs != null && this.User.personal.langs.Count > 0)
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Languages", this.User.personal.langs.GetCommaSeparated(", ")));//"GroupPage_Info"

            if (this.User.relatives != null && this.User.relatives.Count > 0)
            {
                foreach (var relative in this.User.relatives)
                {
                    string str = "";

                    switch (relative.type)
                    {
                        case "grandparent":
                            {
                                str = "ProfilePage_Info_Grandparents";
                                break;
                            }
                        case "parent":
                            {
                                str = "ProfilePage_Info_Parents";
                                break;
                            }
                        case "sibling":
                            {
                                str = "ProfilePage_Info_Siblings";
                                break;
                            }
                        case "child":
                            {
                                str = "ProfilePage_Info_Children";
                                break;
                            }
                        case "grandchild":
                            {
                                str = "ProfilePage_Info_Grandchildren";
                                break;
                            }
                    }

                    if (this.User.relativesUsers != null && relative.id > 0)
                    {
                        VKUser user = this.User.relativesUsers.FirstOrDefault(u => u.id == relative.id);
                        if (user != null)
                        {
                            if (user.deactivated == VKIsDeactivated.Deleted)
                                continue;

                            viewModel.InfoSections.Add(new CustomProfileInfoItemTitled(str, user.Title));
                        }
                        else
                        {
                            viewModel.InfoSections.Add(new CustomProfileInfoItemTitled(str, relative.name));
                        }
                    }
                    else
                    {
                        viewModel.InfoSections.Add(new CustomProfileInfoItemTitled(str, relative.name));
                    }
                }
            }


            if (!string.IsNullOrEmpty(this.User.mobile_phone))
            {
                Action callAction = (() => { PhoneCallManager.ShowPhoneCallUI(this.User.mobile_phone, this.Name); });
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE717", this.User.mobile_phone) { NavigationAction = callAction });
            }

            if (!string.IsNullOrEmpty(this.User.home_phone))
            {
                Action callAction = (() => { PhoneCallManager.ShowPhoneCallUI(this.User.home_phone, this.Name); });
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE717", this.User.home_phone) { NavigationAction = callAction });
            }



            //string _domain = string.IsNullOrEmpty(this.User.domain) ? string.Format("id{0}", this.User.id) : this.User.domain;
            //viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE94B", _domain));

            if (!string.IsNullOrEmpty(this.User.site))
            {
                Action NavigationAction = (() => NavigatorImpl.Instance.NavigateToWebUri(this.User.site));
                viewModel.InfoSections.Add(new CustomProfileInfoItem("\xE774", this.User.site) { NavigationAction = NavigationAction });
            }

            //profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>)UniversityItem.GetUniversities(this.User.universities, this.User.universityGroups));
            //profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>)SchoolItem.GetSchools(this.User.schools, this.User.schoolGroups));



            var personal = this.User.personal;
            if (personal != null)
            {
                if (personal.political > 0)
                {
                    string str = "";
                    switch (personal.political)
                    {
                        case 1:
                            str = "PoliticalViews_Communist";
                            break;
                        case 2:
                            str = "PoliticalViews_Socialist";
                            break;
                        case 3:
                            str = "PoliticalViews_Moderate";
                            break;
                        case 4:
                            str = "PoliticalViews_Liberal";
                            break;
                        case 5:
                            str = "PoliticalViews_Conservative";
                            break;
                        case 6:
                            str = "PoliticalViews_Monarchist";
                            break;
                        case 7:
                            str = "PoliticalViews_Ultraconservative";
                            break;
                        case 8:
                            str = "PoliticalViews_Apathetic";
                            break;
                        case 9:
                            str = "PoliticalViews_Libertarian";
                            break;
                    }

                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_PoliticalViews", LocalizedStrings.GetString(str)));
                }

                if (!string.IsNullOrEmpty(personal.religion))
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_WorldView", personal.religion));

                if (personal.life_main > 0)
                {
                    string str = "";
                    switch (personal.life_main)
                    {
                        case 1:
                            str = "PersonalPriority_FamilyAndChildren";
                            break;
                        case 2:
                            str = "PersonalPriority_CareerAndMoney";
                            break;
                        case 3:
                            str = "PersonalPriority_EntertainmentAndLeisure";
                            break;
                        case 4:
                            str = "PersonalPriority_ScienceAndResearch";
                            break;
                        case 5:
                            str = "PersonalPriority_ImprovingTheWorld";
                            break;
                        case 6:
                            str = "PersonalPriority_PersonalDevelopment";
                            break;
                        case 7:
                            str = "PersonalPriority_BeautyAndArt";
                            break;
                        case 8:
                            str = "PersonalPriority_FameAndInfluence";
                            break;
                    }

                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_PersonalPriority", LocalizedStrings.GetString(str)));
                }

                if (personal.people_main > 0)
                {
                    string str = "";
                    switch (personal.people_main)
                    {
                        case 1:
                            str = "ImportantInOthers_IntellectAndCreativity";
                            break;
                        case 2:
                            str = "ImportantInOthers_KindnessAndHonesty";
                            break;
                        case 3:
                            str = "ImportantInOthers_HealthAndBeauty";
                            break;
                        case 4:
                            str = "ImportantInOthers_WealthAndPower";
                            break;
                        case 5:
                            str = "ImportantInOthers_CourageAndPersistance";
                            break;
                        case 6:
                            str = "ImportantInOthers_HumorAndLoveForLife";
                            break;
                    }

                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_ImportantInOthers", LocalizedStrings.GetString(str)));
                }
                if (personal.smoking > 0)
                {
                    string str = "";
                    switch (personal.smoking)
                    {
                        case 1:
                            str = "BadHabitsViews_VeryNegative";
                            break;
                        case 2:
                            str = "BadHabitsViews_Negative";
                            break;
                        case 3:
                            str = "BadHabitsViews_Compromisable";
                            break;
                        case 4:
                            str = "BadHabitsViews_Neutral";
                            break;
                        case 5:
                            str = "BadHabitsViews_Positive";
                            break;
                    }

                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_ViewsOnSmoking", LocalizedStrings.GetString(str)));
                }
                if (personal.alcohol > 0)
                {
                    string str = "";
                    switch (personal.alcohol)
                    {
                        case 1:
                            str = "BadHabitsViews_VeryNegative";
                            break;
                        case 2:
                            str = "BadHabitsViews_Negative";
                            break;
                        case 3:
                            str = "BadHabitsViews_Compromisable";
                            break;
                        case 4:
                            str = "BadHabitsViews_Neutral";
                            break;
                        case 5:
                            str = "BadHabitsViews_Positive";
                            break;
                    }

                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_ViewsOnAlcohol", LocalizedStrings.GetString(str)));
                }
                if (!string.IsNullOrEmpty(personal.inspired_by))
                    viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_InspiredBy", personal.inspired_by));
            }



            #region Персональная информация
            //ProfileInfoSectionItem personalInfo = new ProfileInfoSectionItem("ProfilePage_Info_PersonalInfomation");
            if (!string.IsNullOrEmpty(this.User.activities))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Activities", this.User.activities));
            if (!string.IsNullOrEmpty(this.User.interests))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Interests", this.User.interests));
            if (!string.IsNullOrEmpty(this.User.music))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Music", this.User.music));
            if (!string.IsNullOrEmpty(this.User.movies))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Movies", this.User.movies));
            if (!string.IsNullOrEmpty(this.User.tv))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_TV", this.User.tv));
            if (!string.IsNullOrEmpty(this.User.books))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Books", this.User.books));
            if (!string.IsNullOrEmpty(this.User.games))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Games", this.User.games));
            if (!string.IsNullOrEmpty(this.User.quotes))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_Quotes", this.User.quotes));
            if (!string.IsNullOrEmpty(this.User.about))
                viewModel.InfoSections.Add(new CustomProfileInfoItemTitled("ProfilePage_Info_About", this.User.about));

            #endregion

            /*
                List<ProfileInfoItem> profileInfoItemList6 = new List<ProfileInfoItem>();
                if (!((IList)this.User.military).IsNullOrEmpty())
                {
                    profileInfoItemList6.AddRange((IEnumerable<ProfileInfoItem>)MilitaryItem.GetMilitaryItems(this.User.military, this.User.militaryCountries));
                    if (profileInfoItemList6.Count > 0)
                        this.InfoSections.Add(new ProfileInfoSectionItem("ProfilePage_Info_MilitaryService")
                        {
                            Items = profileInfoItemList6
                        });
                }
                List<ProfileInfoItem> profileInfoItemList7 = new List<ProfileInfoItem>();
                CareerData careerData = this.User.careerData;
                if (careerData != null && !((IList)careerData.Items).IsNullOrEmpty())
                {
                    profileInfoItemList7.AddRange((IEnumerable<ProfileInfoItem>)CareerItem.GetCareerItems(careerData.Items, careerData.Cities, careerData.Groups));
                    if (profileInfoItemList7.Count > 0)
                        this.InfoSections.Add(new ProfileInfoSectionItem("ProfilePage_Info_Career")
                        {
                            Items = profileInfoItemList7
                        });
                }
                if (this.InfoSections.Count <= 0)
                    return;
                ((ProfileInfoSectionItem)Enumerable.Last<ProfileInfoSectionItem>(this.InfoSections)).DividerVisibility = Visibility.Collapsed;*/

            return viewModel;
        }


        public void PickNewPhoto()
        {
            NavigatorImpl.Instance.NavigateToPhotoPickerPhotos(1, null);
        }

        public void DeletePhoto()
        {
            base.SetInProgress(true);
            /*
            ProfilesService.Instance.DeleteProfilePhoto(this.Id, (res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.SetInProgress(false, "");
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                if (this._profileData != null)
                {
                    this._profileData.PhotoMax = res.ResultData;
                    if (AppGlobalStateManager.Current.GlobalState.LoggedInUser != null)
                        AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = res.ResultData;
                    if (this._profileData.IsGroup)//if (this._groupData != null)//todo:check for group
                        EventAggregator.Current.Publish(new CommunityPhotoChanged()
                        {
                            CommunityId = this._profileData.id,
                            PhotoMax = res.ResultData
                        });
                    EventAggregator.Current.Publish(new BaseDataChangedEvent()
                    {
                        IsProfileUpdateRequired = true
                    });
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
            }))));
            */
        }

        public bool CanChangePhoto
        {
            get
            {
                /*
                if (this._profileData != null)
                    return this._profileData.admin_level > 1;
                return false;
                */
                return this.Id == Settings.UserId;
            }
        }

        public bool HasAvatar
        {
            get
            {
                if (this.User == null)
                    return true;
                return this.IsValidAvatarUrl(this.User.photo_100);
            }
        }

        private bool IsValidAvatarUrl(string avatarUrl)
        {
            if (!string.IsNullOrWhiteSpace(avatarUrl) && !avatarUrl.Contains("vk.com/images/camera") && !avatarUrl.Contains("vk.com/images/deactivated"))
                return !avatarUrl.Contains("vk.com/images/community");
            return false;
        }


        public class ProfileAppViewModel : ViewModelBase
        {
            private readonly int _ownerId;
            private readonly AppButton _appButton;

            public string Title
            {
                get
                {
                    return this._appButton.title;
                }
            }

            public string Image
            {
                get
                {
                    //if (this._appButton.images == null)
                    //    return null;
                    //return this._appButton.images[0].url;
                    return null;
                }
            }

            public Visibility ImageVisibility
            {
                get
                {
                    //return this._appButton.images == null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                    return Visibility.Collapsed;
                }

            }

            public ProfileAppViewModel(int groupId, AppButton appButton)
            {
                this._ownerId = groupId;
                this._appButton = appButton;
            }

            public void NavigateToApp()
            {
                //NavigatorImpl.Instance.NavigateToProfileAppPage(/*this._appButton.app_id*/0, this._ownerId, "");
            }
        }
    }
}
