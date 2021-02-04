using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Core.ViewModels
{
    public class FriendsViewModel2 : ViewModelBase, ISupportUpDownIncrementalLoading
    {
        public int _userId;

        /// <summary>
        /// Это отображаемые люди
        /// </summary>
        public ObservableCollection<VKUserEx> AllFriendsVM { get; private set; }

        public ObservableCollection<VKUserEx> OnlineFriendsVM { get; private set; }

        public List<VKFriendsGetObject.Lists> Lists { get; private set; }

        private List<VKUserEx> _allFriendsVM;
        private List<VKUserEx> _onlineFriendsVM;

        private List<VKUserEx> _requests;
        private List<VKUserEx> _outRequests;

        public VKUserEx RequestsViewModel { get; set; }

        public Visibility RequestsBlockVisibility
        {
            get
            {
                if (this.RequestsViewModel == null || this.ItemsSource != 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string RequestsTitle
        {
            get
            {
                if (this._totalInRequest == 0)
                    return "";
                return UIStringFormatterHelper.FormatNumberOfSomething((int)this._totalInRequest, LocalizedStrings.GetString("OneFriendRequestFrm"), LocalizedStrings.GetString("TwoFourFriendRequestsFrm"), LocalizedStrings.GetString("FiveFriendRequestsFrm"), true, null, false);
            }
        }

        private string _firstHeader = "Все";
        public string FirstHeader
        {
            get
            {
                return this._firstHeader;
            }
            set
            {
                this._firstHeader = value;
                base.NotifyPropertyChanged();
            }
        }

        private string _secondaryHeader = "Онлайн";
        public string SecondaryHeader
        {
            get
            {
                return this._secondaryHeader;
            }
            set
            {
                this._secondaryHeader = value;
                base.NotifyPropertyChanged();
            }
        }

        uint Friends_maximum = 0;

        uint _totalInRequest = 0;
        uint _totalOutRequest = 0;

        public bool HasMoreUpItems
        {
            get { return false; }
        }

        public async Task LoadUpAsync()
        {

        }

        public bool HasMoreDownItems
        {
            get
            {
                switch (this.ItemsSource)
                {
                    // Друзья
                    case 0:
                        {
                            switch (this._subSource)
                            {
                                case 0://Все друзья
                                    {
                                        return this.AllFriendsVM.Count == 0 || this.AllFriendsVM.Count < this.Friends_maximum;
                                    }
                                case 1:
                                    {
                                        return false;//return this.OnlineFriendsVM.Count == 0 || this.OnlineFriendsVM.Count < this._totalOutRequest;
                                    }
                            }
                            break;
                        }
                    case 1:
                        {
                            switch (this._subSource)
                            {
                                case 0://Входящие
                                    {
                                        return this.AllFriendsVM.Count == 0 || this.AllFriendsVM.Count < this._totalInRequest;
                                    }
                                case 1://Исходящие
                                    {
                                        return this.OnlineFriendsVM.Count == 0 || this.OnlineFriendsVM.Count < this._totalOutRequest;
                                    }
                            }
                            break;
                        }
                }
                return false;                                    
            }
        }

        public Action<Enums.ProfileLoadingStatus> LoadingStatusUpdated { get; set; }

        /// <summary>
        /// 0 - друзья
        /// 2 - заявки
        /// 3 - СПИСОК
        /// </summary>
        private byte ItemsSource = 0;

        /// <summary>
        /// 0 - Все друзья/онлайн/общие
        /// 1 - Входящие/Исходящие заявки
        /// </summary>
        private byte _subSource = 0;

        public void SetSource(byte value, bool force_load)
        {
            this.ItemsSource = value;
            this._totalInRequest = 0;
            this._totalOutRequest = 0;

            if (value==0)
            {
                this.FirstHeader = "All";
                this.SecondaryHeader = "Online";
            }
            else if (value==1)
            {
                this.FirstHeader = "Входящие";
                this.SecondaryHeader = "Исходящие";

                base.NotifyPropertyChanged("RequestsBlockVisibility");
            }

            this.AllFriendsVM.Clear();
            this.OnlineFriendsVM.Clear();

            
            if(force_load)
                this.Reload();
        }

        public void SetSubSource(byte value)
        {
            this._subSource = value;
            if (value == 0 && this.AllFriendsVM.Count > 0)
                return;
            else if (value == 1 && this.OnlineFriendsVM.Count > 0)
                return;
            this.LoadDownAsync();
        }

        public FriendsViewModel2(int user_id)
        {
            this.AllFriendsVM = new ObservableCollection<VKUserEx>();
            this.OnlineFriendsVM = new ObservableCollection<VKUserEx>();

            this._allFriendsVM = new List<VKUserEx>();
            this._onlineFriendsVM = new List<VKUserEx>();
            this._requests = new List<VKUserEx>();
            this._outRequests = new List<VKUserEx>();

            this._userId = user_id;
        }

        public async Task<object> Reload()
        {
            this.LoadingStatusUpdated?.Invoke(Enums.ProfileLoadingStatus.Reloading);
            await LoadDownAsync(true);
            return null;
        }

        public async Task LoadDownAsync(bool InReload = false)
        {
            switch (this.ItemsSource)
            {
                // Друзья
                case 0:
                    {
                        switch (this._subSource)
                        {
                            case 0://Все друзья
                                {
                                    if (InReload)
                                    {
                                        this._allFriendsVM.Clear();

                                        if (this.Lists != null)
                                            this.Lists.Clear();
                                    }
                                    
                                    string code = "var lists=null;var requests=[];requests.count=0;var user=null;var online=null;";
                                    code += "var friends=API.friends.get({order:\"hints\",count:40,offset:" + this._allFriendsVM.Count + ",user_id:" + this._userId + "});";

                                    if (this._allFriendsVM.Count == 0 && this._userId == Settings.UserId)
                                    {
                                        code += "lists = API.friends.getLists({return_system:1});";
                                        code += "requests = API.friends.getRequests({need_viewed:0,count:1,extended:1});";
                                        code += "var r = API.users.get({ user_ids: requests.items,fields:\"city,country,photo_100,occupation\"});";
                                        code += "user=r[0];";
                                        code += "if(requests.count>0) { user.message=requests.items[0].message; }";

                                        code += "online=API.friends.getOnline({\"online_mobile\":1});";
                                    }
                                    
                                    code += "var users = API.users.get({user_ids: online.online + online.online_mobile + friends.items,fields: \"online,last_seen,photo_100,occupation,bdate,verified\"});";
                                    code += "return {friends:friends,lists:lists,total_request:requests.count,last_request:user,online:online,users:users};";

                                    VKResponse<VKFriendsGetObject> result = await RequestsDispatcher.Execute<VKFriendsGetObject>(code, (json) =>
                                    {//todo:Возможный баг - если никого нет в онлайне, то список [] и надо веределать в {}
                                        //json = RequestsDispatcher.FixArrayToObject(json, "online");
                                        //json = RequestsDispatcher.FixArrayToObject(json, "lists");
                                        return json;
                                    });

                                    if (result.error.error_code != Enums.VKErrors.None)
                                        return;

                                    this.Friends_maximum = result.response.friends.count;

                                    if (this._allFriendsVM.Count == 0)
                                    {
                                        this.Lists = result.response.lists.items;
                                        this.FirstHeader = "Все " + result.response.friends.count;
                                        this.SecondaryHeader = (result.response.online.online.Count + result.response.online.online_mobile.Count) + " в сети";
                                    }

                                    

                                    foreach (var item in result.response.friends.items)
                                    {
                                        var user = result.response.users.First((u)=>u.id==item);
                                        user.OptionsVisibility = Visibility.Visible;
                                        user.RequestBtnVisibility = Visibility.Collapsed;

                                        if (this.Lists != null)
                                        {
                                            if (user.lists != null && user.lists.Count > 0)
                                            {
                                                user.list_names = new List<string>();
                                                foreach (var list in user.lists)
                                                {
                                                    var new_list = this.Lists.FirstOrDefault((l) => l.id == list);
                                                    if (new_list != null)
                                                        user.list_names.Add(new_list.name);
                                                }
                                            }
                                        }

                                        this.AllFriendsVM.Add(user);
                                    }

                                    if (result.response.online != null)
                                    {
                                        foreach (var online_mobile in result.response.online.online_mobile)
                                        {
                                            var user = result.response.users.First((u) => u.id == online_mobile);
                                            user.OptionsVisibility = Visibility.Visible;
                                            user.RequestBtnVisibility = Visibility.Collapsed;

                                            this.OnlineFriendsVM.Add(user);
                                        }

                                        foreach (var online in result.response.online.online)
                                        {
                                            var user = result.response.users.First((u) => u.id == online);
                                            if (this.OnlineFriendsVM.Contains(user))
                                                continue;

                                            user.OptionsVisibility = Visibility.Visible;
                                            user.RequestBtnVisibility = Visibility.Collapsed;

                                            this.OnlineFriendsVM.Add(user);
                                        }

                                        this._onlineFriendsVM.AddRange(this.OnlineFriendsVM);
                                    }

                                    this._allFriendsVM.AddRange(this.AllFriendsVM);

                                    if (InReload)//todo: надо сохранять весь список
                                        FriendsCache.Instance.SetFriends(this._allFriendsVM.ToList<VKUser>());


                                    if (result.response.last_request != null)
                                    {
                                        this._totalInRequest = (uint)result.response.total_request;

                                        this.RequestsViewModel = result.response.last_request;
                                        base.NotifyPropertyChanged("RequestsViewModel");
                                        base.NotifyPropertyChanged("RequestsBlockVisibility");
                                        base.NotifyPropertyChanged("RequestsTitle");
                                    }

                                    break;
                                }
                            case 1://онлайн
                                {/*
                                    await Task.Delay(1000);

                                    string code = "var o=API.friends.getOnline({\"online_mobile\":1});";
                                    code += "var users = API.users.get({user_ids: o.online + o.online_mobile,fields: \"online,last_seen,photo_100,occupation\"});";
                                    code+="return users;";

                                    var result = await RequestsDispatcher.Execute<List<VKUserEx>>(code);
                                    if (result.error.error_code != Enums.VKErrors.None)
                                        return;

                                    foreach (var item in result.response)
                                    {
                                        this.OnlineFriendsVM.Add(item);
                                    }

                                    this._onlineFriendsVM.AddRange(result.response);*/
                                    break;
                                }
                        }

                        break;
                    }
                case 1://Заявки
                    {
                        switch (this._subSource)
                        {
                            case 0://Входящие
                                {
                                    string code = "var r = API.friends.getRequests({ need_viewed: 1,need_mutual:1, count: 20,extended: 1,offset:" + this._requests.Count + "});";

                                    code += "var o_r = API.friends.getRequests({out:1,count: 20});";

                                    code += "var arr = r.items@.mutual@.users; var arr2 =[];";
                                    code += "var temp = arr.length;";
                                    code += "while (temp >= 0)";
                                    code += "{";
                                    code += "temp = temp - 1;";
                                    code += "if (arr[temp] != null) { if (arr2.indexOf(arr[temp]) == -1) { arr2 = arr2 + arr[temp]; } }";
                                    code += "}";
                                    code += "var users = API.users.get({ user_ids: r.items@.user_id + o_r.items + arr2,fields: \"city,country,photo_100,occupation\"});";

                                    code += "return { requests: r, users: users, out_requests: o_r};";
                                    
                                    VKResponse<VKRequestsGetObject> result = await RequestsDispatcher.Execute<VKRequestsGetObject>(code);

                                    if (result.error.error_code != Enums.VKErrors.None)
                                        return;

                                    this.FirstHeader = "Входящие " + result.response.requests.count;
                                    this.SecondaryHeader = "Исходящие " + result.response.out_requests.count;

                                    foreach (var item in result.response.requests.items)
                                    {
                                        VKUserEx user = result.response.users.First((u)=>u.id== item.user_id);
                                        user.message = item.message;

                                        if(item.mutual!=null)
                                        {
                                            user.randomMutualFriends = new List<VKUser>();
                                            foreach (var m in item.mutual.users)
                                            {
                                                VKUserEx mutual = result.response.users.First((u) => u.id == m);
                                                user.randomMutualFriends.Add(mutual);
                                            }
                                        }

                                        user.OptionsVisibility = Visibility.Collapsed;
                                        user.RequestBtnVisibility = Visibility.Visible;
                                        this.AllFriendsVM.Add(user);
                                        this._requests.AddRange(this.AllFriendsVM);
                                    }

                                    this._totalInRequest = result.response.requests.count;
                                    this._totalOutRequest = result.response.out_requests.count;

                                    if (this.OnlineFriendsVM.Count == 0)
                                    {
                                        foreach (var item in result.response.out_requests.items)
                                        {
                                            VKUserEx user = result.response.users.First((u) => u.id == item);
                                            user.OptionsVisibility = Visibility.Collapsed;
                                            user.RequestBtnVisibility = Visibility.Visible;
                                            this.OnlineFriendsVM.Add(user);
                                        }

                                        this._outRequests.AddRange(this.AllFriendsVM);
                                    }

                                    

                                    break;
                                }
                            case 1://Исходящие
                                {
                                    break;
                                }
                        }
                        break;
                    }
             }
        }

        public class VKRequestsGetObject
        {
            public VKCountedItemsObject<RequestWithMsg> requests { get; set; }

            public List<VKUserEx> users { get; set; }

            public VKCountedItemsObject<int> out_requests { get; set; }

            public class RequestWithMsg
            {
                public int user_id { get; set; }
                public string message { get; set; }
                public Mutual mutual { get; set; }

                public class Mutual
                {
                    public int count { get; set; }
                    public List<int> users { get; set; }
                }
            }
        }

        public void LocalSearch(string text)
        {/*
            this.Items.Clear();
            foreach (var friend in this._allFriendsVM)
            {
                if (friend.first_name.StartsWith(text, StringComparison.OrdinalIgnoreCase) || friend.last_name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    this.Items.Add(friend);
            }*/
        }

        public async void ServerSearch(string text)
        {/*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = this._userId.ToString();
            parameters["q"] = text;
            parameters["count"] = "50";
            parameters["fields"] = "online,photo_100,verified,last_seen,occupation";

            VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKFriendVM>>("friends.search", parameters);

            if (result.error.error_code != Enums.VKErrors.None)
                return;

            foreach (var item in result.response.items)
            {
                var temp = this.Items.FirstOrDefault((f) => f.id == item.id);
                if (temp == null)
                {
                    //this.Friends.Add(item);
                    this.Items.Add(item);
                }
            }*/
        }

        public class DeleteFriendAnswer
        {
            /// <summary>
            /// удалось успешно удалить друга 
            /// </summary>
            public int success { get; set; }

            /// <summary>
            /// отклонена входящая заявка 
            /// </summary>
            public int in_request_deleted { get; set; }

            /// <summary>
            /// был удален друг 
            /// </summary>
            public int friend_deleted { get; set; }

            /// <summary>
            /// отменена исходящая заявка 
            /// </summary>
            public int out_request_deleted { get; set; }

            /// <summary>
            /// отклонена рекомендация друга 
            /// </summary>
            public int suggestion_deleted { get; set; }
        }

        public void AddFriend(VKUserEx user)
        {
            UsersService.Instance.FriendAccept(user.id, false, (res) =>
            {
                this.RequestsViewModel = null;
                this._allFriendsVM.Insert(0, user);
                this.AllFriendsVM.Insert(0, user);
                this._requests.Remove(user);
                base.NotifyPropertyChanged("RequestsBlockVisibility");
            });
        }

        public void DeleteFriend(VKUserEx user)
        {
            UsersService.Instance.FriendDelete(user.id, (res) =>
            {
                this.RequestsViewModel = null;
                this._requests.Remove(user);
                base.NotifyPropertyChanged("RequestsBlockVisibility");
            });
        }

        public async void EditFriend(VKUserEx user, int id)
        {
            List<int> ids = user.lists;
            if (ids == null)
                ids = new List<int>();

            if (ids.Contains(id))
                ids.Remove(id);
            else
                ids.Add(id);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = user.id.ToString();
            parameters["list_ids"] = ids.GetCommaSeparated();

            var temp = await RequestsDispatcher.GetResponse<int>("friends.edit", parameters);
            if (temp.error.error_code != Enums.VKErrors.None)
                return;

            if (temp.response == 1)
            {
                user.lists = new List<int>(ids);
                user.list_names = new List<string>();

                foreach (var list in user.lists)
                {
                    var new_list = this.Lists.FirstOrDefault((l) => l.id == list);
                    if (new_list != null)
                        user.list_names.Add(new_list.name);
                }

                user.UpdateUI();
            }
        }

        public class VKUserEx : VKUser, INotifyPropertyChanged
        {
            /// <summary>
            /// Приветственное сообщение при добавлении в друзья
            /// </summary>
            public string message { get; set; }

            /// <summary>
            /// Названия списков
            /// </summary>
            public List<string> list_names { get; set; }

            public void UpdateUI()
            {
                this.NotifyPropertyChanged("list_names");
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            {
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            public Visibility OptionsVisibility { get; set; }

            public Visibility RequestBtnVisibility { get; set; }
        }

        public class VKFriendsGetObject
        {
            //public VKCountedItemsObject<VKUserEx> friends { get; set; }

            public VKCountedItemsObject<int> friends { get; set; }

            public List<VKUserEx> users { get; set; }

            public int total_request { get; set; }

            /// <summary>
            /// Списки идов онлайн друзей
            /// </summary>
            public Online online { get; set; }

            public VKUserEx last_request { get; set; }

            public VKCountedItemsObject<Lists> lists { get; set; }

            public class Lists
            {
                public string name { get; set; }

                public int id { get; set; }
            }

            public class Online
            {
                public List<int> online { get; set; }

                public List<int> online_mobile { get; set; }
            }
        }
    }
}
