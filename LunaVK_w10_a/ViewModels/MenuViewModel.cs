using System;
using Windows.UI.Xaml;
using LunaVK.Core.Enums;
using Windows.UI.Xaml.Controls;
using LunaVK.Framework;
using Windows.UI.Notifications;
using LunaVK.Core.Framework;
using LunaVK.Core;
using LunaVK.Core.Network;
using LunaVK.Core.DataObjects;
using LunaVK.Pages;

namespace LunaVK.ViewModels
{
    public class MenuViewModel : LunaVK.Core.ViewModels.ViewModelBase
    {
        private static MenuViewModel _instance;
        public static MenuViewModel Instance
        {
            get
            {
                if (MenuViewModel._instance == null)
                {
                    MenuViewModel._instance = new MenuViewModel();
                    if (string.IsNullOrEmpty(MenuViewModel._instance.UserName) && Settings.IsAuthorized)
                        MenuViewModel._instance.GetBaseData();
                }
                return MenuViewModel._instance;
            }
            set
            {
                MenuViewModel._instance = value;
            }
        }

        public BirthdaysViewModel BirthdaysVM { get; private set; }
       
        public void SetItemsMenuState(CustomFrame.MenuStates new_state)
        {
            this.NewsItem.UpdateWideState(new_state);
            this.NotificationsItem.UpdateWideState(new_state);
            this.MessagesItem.UpdateWideState(new_state);
            this.FriendsItem.UpdateWideState(new_state);
            this.CommunitiesItem.UpdateWideState(new_state);
            this.PhotosItem.UpdateWideState(new_state);
            this.VideosItem.UpdateWideState(new_state);
            this.AudiosItem.UpdateWideState(new_state);
            this.GamesItem.UpdateWideState(new_state);
            this.BookmarksItem.UpdateWideState(new_state);
            this.SettingsItem.UpdateWideState(new_state);
            this.SearchItem.UpdateWideState(new_state);
            this.MarketItem.UpdateWideState(new_state);
            this.DocumentsItem.UpdateWideState(new_state);
        }

        public void GetBaseData(Action<bool> callback = null)
        {
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters["fields"] = "online,online_mobile,photo_200";
            //parameters["extended"] = "1";
            //VKResponse<VKProfileinfoGetObject> temp = await RequestsDispatcher.GetResponse<VKProfileinfoGetObject>("account.getProfileInfo", parameters);

            string code = "var u = API.users.get({\"fields\":\"photo_100,status\"});return u[0];";
            VKRequestsDispatcher.Execute<VKUser>(code, (result)=> { 


            if (result.error.error_code == VKErrors.None)
            {
                Settings.LoggedInUserName = result.response.Title;
                Settings.LoggedInUserPhoto = result.response.photo_100;
                Settings.LoggedInUserStatus = result.response.status;
                
                Execute.ExecuteOnUIThread(()=>
                {
                    this.UpdateUser();
                });
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
            });
        }
        
        public MenuItemViewModel MarketItem { get; private set; }
        public MenuItemViewModel NewsItem { get; private set; }
        public MenuItemViewModel NotificationsItem { get; private set; }
        public MenuItemViewModel MessagesItem { get; private set; }
        public MenuItemViewModel FriendsItem { get; private set; }
        public MenuItemViewModel CommunitiesItem { get; private set; }
        public MenuItemViewModel PhotosItem { get; private set; }
        public MenuItemViewModel VideosItem { get; private set; }
        public MenuItemViewModel AudiosItem { get; private set; }
        public MenuItemViewModel GamesItem { get; private set; }
        public MenuItemViewModel BookmarksItem { get; private set; }
        public MenuItemViewModel LikesItem { get; private set; }
        public MenuItemViewModel SettingsItem { get; private set; }
        public MenuItemViewModel SearchItem { get; private set; }
        public MenuItemViewModel DocumentsItem { get; private set; }
        
        public MenuViewModel()
        {
            this.BirthdaysVM = new BirthdaysViewModel();

            this.NewsItem = new MenuItemViewModel(MenuSectionName.News);
            this.NotificationsItem = new MenuItemViewModel(MenuSectionName.Notifications);
            this.MessagesItem = new MenuItemViewModel(MenuSectionName.Messages);
            this.FriendsItem = new MenuItemViewModel(MenuSectionName.Friends);
            this.CommunitiesItem = new MenuItemViewModel(MenuSectionName.Communities);
            this.PhotosItem = new MenuItemViewModel(MenuSectionName.Photos);
            this.VideosItem = new MenuItemViewModel(MenuSectionName.Videos);
            this.AudiosItem = new MenuItemViewModel(MenuSectionName.Audios);
            this.GamesItem = new MenuItemViewModel(MenuSectionName.Games);
            this.BookmarksItem = new MenuItemViewModel(MenuSectionName.Bookmarks);
            this.LikesItem = new MenuItemViewModel(MenuSectionName.Likes);
            this.SettingsItem = new MenuItemViewModel(MenuSectionName.Settings);
            this.SearchItem = new MenuItemViewModel(MenuSectionName.Search);
            this.MarketItem = new MenuItemViewModel(MenuSectionName.Market);
            this.DocumentsItem = new MenuItemViewModel(MenuSectionName.Documents);

            EventAggregator.Instance.CountersEventHandler += this.CounersChanged;
            EventAggregator.Instance.ProfileStatusChangedEventHandler += this.ProfileStatusChanged;
            EventAggregator.Instance.ProfileAvatarChangedEventHandler += this.ProfileAvatarChanged;
            EventAggregator.Instance.ProfileNameChangedEventHandler += this.ProfileNameChanged;
        }

        private void ProfileStatusChanged(string status)
        {
            Settings.LoggedInUserStatus = status;
            base.NotifyPropertyChanged(nameof(this.Status));
            base.NotifyPropertyChanged(nameof(this.StatusVisibility));
        }

        private void ProfileAvatarChanged(string status)
        {
            Settings.LoggedInUserPhoto = status;
            base.NotifyPropertyChanged(nameof(this.UserPhoto));
        }

        private void ProfileNameChanged(string fullName)
        {
            Settings.LoggedInUserName = fullName;
            base.NotifyPropertyChanged(nameof(this.UserName));
        }

        public void UpdateUser()
        {
            base.NotifyPropertyChanged(nameof(this.UserPhoto));
            base.NotifyPropertyChanged("UserName");
            base.NotifyPropertyChanged("Status");
            base.NotifyPropertyChanged("StatusVisibility");
        }

        //private bool _isLoading;
        public bool NeedRefreshBaseData = true;

        public void RefreshBaseDataIfNeeded()
        {
            //double temp = (Settings.DataLastRefreshTime - DateTime.Now).TotalHours;
            //double temp2 = (DateTime.Now - Settings.DataLastRefreshTime).TotalHours;
            if (/*this._isLoading ||*/ !Settings.IsAuthorized )
                return;

            if (this.NeedRefreshBaseData || (DateTime.Now - Settings.DataLastRefreshTime).TotalHours > 12)
            {
                this.BirthdaysVM.UpdateData();
                Settings.DataLastRefreshTime = DateTime.Now;
                this.NeedRefreshBaseData = false;
            }
        }

        public Visibility StatusVisibility
        {
            get
            {
                return string.IsNullOrEmpty(this.Status) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string UserPhoto
        {
            get { return Settings.LoggedInUserPhoto; }
        }

        public string UserName
        {
            get { return Settings.LoggedInUserName; }
        }

        public string Status
        {
            get { return Settings.LoggedInUserStatus; }
        }
        
        private void CounersChanged(CountersArgs args)
        {
            this.FriendsItem.Count = (int)args.friends;
            this.MessagesItem.Count = (int)args.messages;
            this.CommunitiesItem.Count = (int)args.groups;
            this.NotificationsItem.Count = (int)args.notifications;
            this.BookmarksItem.Count = (int)args.faves;

            base.NotifyPropertyChanged<string>(() => this.TotalCountString);
            base.NotifyPropertyChanged<Visibility>(() => this.HaveAnyNotificationsVisibility);
            //
            var b = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            var xml = BadgeUpdateManager.GetTemplateContent( BadgeTemplateType.BadgeNumber);
            var atr = xml.GetElementsByTagName("badge");
            atr[0].Attributes.GetNamedItem("value").NodeValue = this.TotalCount == 0 ? "" : this.TotalCountString;
            var n = new BadgeNotification(xml);
            b.Update(n);
        }
        
        private int TotalCount
        {
            get { return this.NotificationsItem.Count + this.MessagesItem.Count + this.FriendsItem.Count + this.CommunitiesItem.Count/* + this.GamesItem.Count*/; }
        }

        public string TotalCountString
        {
            get { return this.TotalCount.ToString(); }
        }

        public Visibility HaveAnyNotificationsVisibility
        {
            get
            {
                if (this.TotalCount==0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public void UpdateSelectedItem()
        {
            MenuSectionName selectedSection = MenuSectionName.Unknown;

            var page = (Window.Current.Content as Frame).Content as PageBase;

            if (page is NewsPage)
                selectedSection = MenuSectionName.News;
            else if (page is DialogsConversationPage2)
                selectedSection = MenuSectionName.Messages;
            else if (page is NotificationsPage)
                selectedSection = MenuSectionName.Notifications;
            else if (page is FriendsPage)
                selectedSection = MenuSectionName.Friends;
            else if (page is GroupsPage)
                selectedSection = MenuSectionName.Communities;
            else if (page is AllPhotosPage)
                selectedSection = MenuSectionName.Photos;
            else if (page is VideoAlbumsListPage)
                selectedSection = MenuSectionName.Videos;
            else if (page is MusicPage)
                selectedSection = MenuSectionName.Audios;
            else if (page is GamesMainPage)
                selectedSection = MenuSectionName.Games;
            else if (page is FavoritesPage)
                selectedSection = MenuSectionName.Bookmarks;
            else if (page is MyLikesPage)
                selectedSection = MenuSectionName.Likes;
            else if (page is SettingsPage)
                selectedSection = MenuSectionName.Settings;
            else if (page is DocumentsPage)
                selectedSection = MenuSectionName.Documents;
            else if (page is SearchResultsPage)
                selectedSection = MenuSectionName.Search;

            this.NewsItem.UpdateSelectionState(selectedSection);
            this.NotificationsItem.UpdateSelectionState(selectedSection);
            this.MessagesItem.UpdateSelectionState(selectedSection);
            this.FriendsItem.UpdateSelectionState(selectedSection);
            this.CommunitiesItem.UpdateSelectionState(selectedSection);
            this.PhotosItem.UpdateSelectionState(selectedSection);
            this.VideosItem.UpdateSelectionState(selectedSection);
            this.AudiosItem.UpdateSelectionState(selectedSection);
            this.GamesItem.UpdateSelectionState(selectedSection);
            this.BookmarksItem.UpdateSelectionState(selectedSection);
            this.LikesItem.UpdateSelectionState(selectedSection);
            this.SettingsItem.UpdateSelectionState(selectedSection);
            this.SearchItem.UpdateSelectionState(selectedSection);
            this.MarketItem.UpdateSelectionState(selectedSection);
            this.DocumentsItem.UpdateSelectionState(selectedSection);

            base.NotifyPropertyChanged(nameof(this.SelectedIndex));
        }

        public int SelectedIndex
        {
            get
            {
                var page = (Window.Current.Content as Frame).Content as PageBase;

                if (page is SearchResultsPage)
                    return 0;
                else if (page is NewsPage)
                    return 1;
                else if (page is NotificationsPage)
                    return 2;
                else if (page is DialogsConversationPage2)
                    return 3;
                else if (page is FriendsPage)
                    return 4;
                else if (page is GroupsPage)
                    return 5;
                else if (page is AllPhotosPage)
                    return 6;
                else if (page is VideoAlbumsListPage)
                    return 7;
                else if (page is MusicPage)
                    return 8;
                else if (page is FavoritesPage)
                    return 9;
                else if (page is MyLikesPage)
                    return 10;
                else if (page is GamesMainPage)
                    return 11;
                //else if (page is SettingsPage)
                //    selectedSection = MenuSectionName.Settings;
                else if (page is DocumentsPage)
                    return 12;
                

                return -1;
            }
            set
            {
                int i = 0;
            }
        }
    }
}
