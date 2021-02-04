using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LunaVK.Core.Framework;
using System.Linq;
using LunaVK.Core.Network;
using LunaVK.Core.Library;
using Windows.UI.Xaml;

namespace LunaVK.Core.ViewModels
{
    public class FriendsViewModel : ViewModelBase, ISupportLoadMore
    {
        int _userId;

        /// <summary>
        /// Это отображаемые люди
        /// </summary>
        public ObservableCollection<FriendRequest> AllFriendsVM { get; private set; }

        public ObservableCollection<FriendRequest> OnlineFriendsVM { get; private set; }

        public List<VKFriendsGetObject.Lists> Lists { get; private set; }

        private List<FriendRequest> _allFriendsVM { get; set; }
        private List<FriendRequest> _onlineFriendsVM { get; set; }

        private List<FriendRequest> _requests { get; set; }
        private List<FriendRequest> _outRequests { get; set; }

        public Visibility RequestsBlockVisibility
        {
            get
            {
                if (this.RequestsViewModel == null || this.RequestsViewModel.count <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public FriendRequests RequestsViewModel
        {
            get
            {
                return this._requestsViewModel;
            }
            set
            {
                this._requestsViewModel = value;
                this.NotifyPropertyChanged<FriendRequests>(() => this.RequestsViewModel);
                this.NotifyPropertyChanged<Visibility>(() => this.RequestsBlockVisibility);
                FriendsCache.Instance.SetFriends(null, value);
            }
        }

        /// <summary>
        /// 0 - друзья
        /// 1 - онлайн
        /// 2 - заявки
        /// 3 - исходящие
        /// </summary>
        private int ItemsSource = 0;

        uint Friends_maximum = 0;
        uint Requests_maximum = 0;
        uint OutRequests_maximum = 0;
        uint Suggestions_maximum = 0;

        public bool HasMoreItems
        {
            get
            {
                switch (this.ItemsSource)
                {
                    case 0:
                        {
                            return this.Friends.Count == 0 || this.Friends.Count < this.Friends_maximum;
                        }
                    case 1:
                        {
                            return this.Requests.Count == 0 || this.Requests.Count < this.Requests_maximum;
                        }
                    case 2:
                        {
                            return this.OutRequests.Count == 0 || this.OutRequests.Count < this.OutRequests_maximum;
                        }
                    case 3:
                        {
                            return this.Suggestions.Count == 0 || this.Suggestions.Count < this.Suggestions_maximum;
                        }
                }
                return false;
            }
        }

        public FriendsViewModel(long user_id)
        {
            this.Items = new ObservableCollection<VKFriendVM>();
            this.Lists = new List<VKFriendsGetObject.Lists>();

            this.Friends = new List<VKFriendVM>();
            this.Requests = new List<VKFriendVM>();
            this.OutRequests = new List<VKFriendVM>();
            this.Suggestions = new List<VKFriendVM>();

            this._userId = user_id;
        }

        public void SetSource(int value)
        {
            this.ItemsSource = value;
            this.Items.Clear();
            this.LoadData();
        }

        public async Task LoadData(bool reload = false)
        {
            if (reload)
                this.Items.Clear();

            //Dictionary<string, string> parameters = new Dictionary<string, string>();

            switch (this.ItemsSource)
            {
                /*
            0 - друзья
            1 - заявки
            2 - исходящие
            3 - предполагаемые
            */
                case 0:
                    {
                        if (reload)
                        {
                            this.Friends.Clear();

                            if (this.Lists != null)
                                this.Lists.Clear();
                        }

                        if (this.Friends.Count > 0 && this.Items.Count == 0)
                        {
                            foreach (VKFriendVM item in this.Friends)
                            {
                                item.Mode = 0;
                                this.Items.Add(item);
                            }
                            return;
                        }
                        /*
                         * порядок, в котором нужно вернуть список друзей. Допустимые значения:

    hints — сортировать по рейтингу, аналогично тому, как друзья сортируются в разделе Мои друзья (Это значение доступно только для Standalone-приложений с ключом доступа, полученным по схеме Implicit Flow.).
    random — возвращает друзей в случайном порядке.
    mobile — возвращает выше тех друзей, у которых установлены мобильные приложения.
    name — сортировать по имени. Данный тип сортировки работает медленно, так как сервер будет получать всех друзей а не только указанное количество count. (работает только при переданном параметре fields).

                         * */
                        string code = "var lists=[]; var friends=API.friends.get({fields:\"online,bdate,photo_100,verified,last_seen,occupation\",order:\"hints\",count:40,offset:" + this.Friends.Count + ",user_id:" + this._userId + "});";

                        if (this.Items.Count == 0 && this._userId == Settings.UserId)
                        {
                            code += "lists = API.friends.getLists({return_system:1});";
                        }

                        code += "return {friends:friends,lists:lists};";


                        VKResponse<VKFriendsGetObject> result = await RequestsDispatcher.Execute<VKFriendsGetObject>(code, (json) => {
                            json = RequestsDispatcher.FixArrayToObject(json, "lists");
                            return json;
                        });

                        if (result.error.error_code != Enums.VKErrors.None)
                            return;

                        this.Friends_maximum = result.response.friends.count;
                        this.Friends.AddRange(result.response.friends.items);

                        if (reload)//todo: надо сохранять весь список
                            FriendsCache.Instance.SetFriends(result.response.friends.items.ToList<VKUser>());

                        if (this.Items.Count == 0)
                        {
                            this.Lists = result.response.lists.items;
                        }

                        foreach (VKFriendVM item in result.response.friends.items)
                        {
                            item.Mode = 0;
                            this.Items.Add(item);
                        }


                        break;
                    }
                case 1:
                    {
                        //Dictionary<string, string> parameters = new Dictionary<string, string>();
                        //parameters["offset"] = this.Requests.Count.ToString();
                        //parameters["fields"] = "online,photo_100,verified,last_seen,occupation";
                        //parameters["count"] = "30";
                        //parameters["need_viewed"] = "1";
                        //VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKFriendVM>>("friends.getRequests", parameters);
                        //this.Requests_maximum = result.response.count;
                        //this.Requests.AddRange(result.response.items);

                        //foreach (VKFriendVM item in result.response.items)
                        //{
                        //    this.Items.Add(item);
                        //}

                        if (reload)
                        {
                            this.Requests.Clear();
                        }

                        if (this.Requests.Count > 0 && this.Items.Count == 0)
                        {
                            foreach (VKFriendVM item in this.Requests)
                            {
                                item.Mode = 1;
                                this.Items.Add(item);
                            }
                            return;
                        }

                        string code = "var requests=API.friends.getRequests({need_viewed:0,count:20,offset:" + this.Requests.Count + "});";
                        code += "var requests_extended=API.friends.getRequests({need_viewed:0,extended:1,count:20,offset:" + this.Requests.Count + "});";

                        code += "var users=API.users.get({user_ids:requests.items,fields:\"city,country,photo_100,occupation\"});";
                        code += "return {items:users,count:requests.count,requests_extended:requests_extended};";

                        VKResponse<VKRequestsGetObject> result = await RequestsDispatcher.Execute<VKRequestsGetObject>(code);

                        if (result.error.error_code != Enums.VKErrors.None)
                            return;

                        this.Requests_maximum = result.response.count;
                        this.Requests.AddRange(result.response.items);

                        for (int i = 0; i < result.response.items.Count; i++)
                        {
                            VKFriendVM item = result.response.items[i];
                            item.Mode = 1;
                            if (!string.IsNullOrEmpty(result.response.requests_extended.items[i].message))
                                item.RequsetMsg = "\"" + result.response.requests_extended.items[i].message + "\"";

                            this.Items.Add(item);
                        }
                        break;
                    }
                case 2:
                    {
                        if (reload)
                        {
                            this.OutRequests.Clear();
                        }

                        if (this.OutRequests.Count > 0 && this.Items.Count == 0)
                        {
                            foreach (VKFriendVM item in this.OutRequests)
                            {
                                item.Mode = 2;
                                this.Items.Add(item);
                            }
                            return;
                        }

                        //Dictionary<string, string> parameters = new Dictionary<string, string>();
                        //parameters["offset"] = this.Requests.Count.ToString();
                        //parameters["fields"] = "online,photo_100,verified,last_seen,occupation";
                        //parameters["count"] = "30";
                        //parameters["out"] = "1";
                        //VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKFriendVM>>("friends.getRequests", parameters);

                        string code = "var requests=API.friends.getRequests({need_viewed:1,out:1,count:20,offset:" + this.Requests.Count + "});";
                        code += "var users=API.users.get({user_ids:requests.items,fields:\"city,country,photo_100,occupation\"});";
                        code += "return {items:users,count:requests.count};";

                        VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.Execute<VKCountedItemsObject<VKFriendVM>>(code);

                        if (result.error.error_code != Enums.VKErrors.None)
                            return;

                        this.OutRequests_maximum = result.response.count;
                        this.OutRequests.AddRange(result.response.items);

                        foreach (VKFriendVM item in result.response.items)
                        {
                            item.Mode = 2;
                            this.Items.Add(item);
                        }
                        break;
                    }
                case 3:
                    {
                        if (reload)
                        {
                            this.Suggestions.Clear();
                        }

                        if (this.Suggestions.Count > 0 && this.Items.Count == 0)
                        {
                            foreach (VKFriendVM item in this.Suggestions)
                            {
                                item.Mode = 3;
                                this.Items.Add(item);
                            }
                            return;
                        }

                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters["offset"] = this.Suggestions.Count.ToString();
                        parameters["fields"] = "online,photo_100,verified,last_seen,occupation";
                        parameters["count"] = "30";
                        //parameters["out"] = "1";
                        VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKFriendVM>>("friends.getSuggestions", parameters);

                        if (result.error.error_code != Enums.VKErrors.None)
                            return;

                        this.Suggestions_maximum = result.response.count;
                        this.Suggestions.AddRange(result.response.items);

                        foreach (VKFriendVM item in result.response.items)
                        {
                            item.Mode = 3;
                            this.Items.Add(item);
                        }
                        break;
                    }
                default:
                    {
                        int listId = this.ItemsSource - 99;

                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        parameters["offset"] = this.Items.Count.ToString();
                        parameters["fields"] = "online,photo_100,verified,last_seen,occupation";
                        parameters["count"] = "30";
                        parameters["list_id"] = listId.ToString();
                        VKResponse<VKCountedItemsObject<VKFriendVM>> result = await RequestsDispatcher.GetResponse<VKCountedItemsObject<VKFriendVM>>("friends.get", parameters);

                        if (result.error.error_code != Enums.VKErrors.None)
                            return;


                        foreach (VKFriendVM item in result.response.items)
                        {
                            item.Mode = 0;
                            this.Items.Add(item);
                        }
                        break;
                    }
            }
        }
        /*
        public class VKRequestsGetObject : VKCountedItemsObject<VKFriendVM>
        {
            public VKCountedItemsObject<RequestWithMsg> requests_extended { get; set; }

            public class RequestWithMsg
            {
                public int user_id { get; set; }
                public string message { get; set; }
            }
        }

        public class VKFriendsGetObject
        {
            public VKCountedItemsObject<VKFriendVM> friends { get; set; }

            public VKCountedItemsObject<Lists> lists { get; set; }

            public class Lists
            {
                public string name { get; set; }

                public int id { get; set; }
            }
        }
        */

        public class VKFriendsGetObject
        {
            public int count { get; set; }

            public VKCountedItemsObject<VKUserEx> friends { get; set; }

            public VKUser last_request { get; set; }

            public VKCountedItemsObject<Lists> lists { get; set; }

            public class Lists
            {
                public string name { get; set; }

                public int id { get; set; }
            }
        }

        public class VKUserEx : VKUser
        {
            public string message { get; set; }

            public string list_name { get; set; }
        }

        public async Task AddFriend(VKFriendVM user)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = user.id.ToString();
            VKResponse<int> result = await RequestsDispatcher.GetResponse<int>("friends.add", parameters);

            if (result.error.error_code != Enums.VKErrors.None)
                return;

            if (result != null && result.response == 1)
            {
                this.Requests.Remove(user);
                this.Items.Remove(user);
            }
        }

        public async Task DeleteFriend(VKFriendVM user)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = user.id.ToString();
            VKResponse<DeleteFriendAnswer> result = await RequestsDispatcher.GetResponse<DeleteFriendAnswer>("friends.delete", parameters);

            if (result.error.error_code != Enums.VKErrors.None)
                return;

            if (result != null && result.response.success == 1)
            {
                this.Requests.Remove(user);
                this.Items.Remove(user);
            }
        }

        public void LocalSearch(string text)
        {
            this.Items.Clear();
            foreach (var friend in this.Friends)
            {
                if (friend.first_name.StartsWith(text, StringComparison.OrdinalIgnoreCase) || friend.last_name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    this.Items.Add(friend);
            }
        }

        public async void ServerSearch(string text)
        {
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
                var temp = this.Items.FirstOrDefault((f)=>f.id == item.id);
                if (temp == null)
                {
                    //this.Friends.Add(item);
                    this.Items.Add(item);
                }
            }
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
    }
}
