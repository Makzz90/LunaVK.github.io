using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using LunaVK.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.ViewModels
{
    public class FriendsViewModel3
    {
        public uint _userId;
        public string _userName;

        public GenericCollectionAll AllFriendsVM { get; private set; }

        public GenericCollectionOnline OnlineFriendsVM { get; private set; }

        public GenericCollectionMutual MutualFriendsVM { get; private set; }

        public GenericCollectionSuggestions SuggestionsFriendsVM { get; private set; }

        public GenericCollectionRequests RequestsFriendsVM { get; private set; }

        public GenericCollectionRequestsOut RequestsOutFriendsVM { get; private set; }

        public FriendsViewModel3(uint userId)
        {
            this._userId = userId;
            this.AllFriendsVM = new GenericCollectionAll(userId);
            this.OnlineFriendsVM = new GenericCollectionOnline(userId);
            this.MutualFriendsVM = new GenericCollectionMutual(userId);
            this.SuggestionsFriendsVM = new GenericCollectionSuggestions();
            this.RequestsFriendsVM = new GenericCollectionRequests();
            this.RequestsOutFriendsVM = new GenericCollectionRequestsOut();
        }



        public class GenericCollectionAll : GenericCollectionViewModel<VKUserEx>
        {
            public VKUserEx RequestsViewModel { get; private set; }
            public List<FriendsList> Lists { get; private set; }
            public uint _userId;
            uint _totalInRequest = 0;

            public GenericCollectionAll(uint userId)
            {
                this._userId = userId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUserEx>> callback)
            {
                if (offset == 0)
                {
                    if (this.Lists != null)
                        this.Lists.Clear();
                }

                string code = "var friends=API.friends.get({order:\"hints\",fields: \"online,last_seen,photo_100,occupation,bdate,verified,first_name_gen\",count:" + count + ", offset:" + offset + ",user_id:" + this._userId + "});";
                code += "var lists=null;var last_request=null;var total_request=0;var total_online=0;";

                if (offset == 0 && this._userId == Settings.UserId)
                {
                    code += "lists = API.friends.getLists({return_system:1});";
                    code += "var requests = API.friends.getRequests({need_viewed:0,count:1,extended:1,fields:\"photo_100\"});";//при extended появляется поле с сообщением
                    code += "if(requests.count>0) { last_request=requests.items[0]; total_request = requests.count; }";

                }

                code += "var online=API.friends.getOnline({\"online_mobile\":1,user_id:" + this._userId + "});";
                code += "total_online = online.online.length + online.online_mobile.length;";

                code += "return {friends:friends,lists:lists,last_request:last_request,total_request:total_request,last_request:last_request,total_online:total_online};";

                VKRequestsDispatcher.Execute<VKFriendsGetObject>(code, (result =>
                {

                    if (result.error.error_code == VKErrors.None)
                    {
                        if (result.response.lists != null)
                            this.Lists = result.response.lists.items;

                        foreach (var user in result.response.friends.items)
                        {
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
                        }

                        if (offset == 0)
                        {
                            if (result.response.last_request != null)
                            {
                                this._totalInRequest = (uint)result.response.total_request;

                                this.RequestsViewModel = result.response.last_request;
                                base.NotifyPropertyChanged("RequestsViewModel");
                                base.NotifyPropertyChanged("RequestsBlockVisibility");
                                base.NotifyPropertyChanged("RequestsTitle");
                            }
                        }

                        base._totalCount = result.response.friends.count;
                        callback(result.error, result.response.friends.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }),(json)=> {
                    return json.Replace("user_id", "id");
                });



            }

            public Visibility RequestsBlockVisibility
            {
                get
                {
                    if (this.RequestsViewModel == null)
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
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)this._totalInRequest, "OneFriendRequestFrm", "TwoFourFriendRequestsFrm", "FiveFriendRequestsFrm");
                }
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoFriends");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                }
            }

            public void AddFriend(VKUserEx user)
            {
                UsersService.Instance.FriendAccept(user.id, false, (res) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.RequestsViewModel = null;
                        base.NotifyPropertyChanged(nameof(this.RequestsBlockVisibility));

                        if(res == true)
                        {
                            user.OptionsVisibility = Visibility.Visible;
                            user.RequestBtnVisibility = Visibility.Collapsed;

                            base.Items.Insert(0, user);//todo:sort insert

                            var c = LongPollServerService.Instance._counters;
                            c.friends -= 1;
                            EventAggregator.Instance.PublishCounters(c);
                        }
                    });
                });
            }

            public void DeleteFriend(VKUserEx user)
            {
                UsersService.Instance.FriendDelete(user.id, (res) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        this.RequestsViewModel = null;
                        base.NotifyPropertyChanged(nameof(this.RequestsBlockVisibility));
                        //
                        //todo: удаллять друзей из списка
                        //
                        var c = LongPollServerService.Instance._counters;
                        c.friends -= 1;
                        EventAggregator.Instance.PublishCounters(c);
                    });
                });
            }
            

            public class VKFriendsGetObject
            {
                public VKCountedItemsObject<VKUserEx> friends { get; set; }

                public VKCountedItemsObject<FriendsList> lists { get; set; }

                public VKUserEx last_request { get; set; }

                public int total_request { get; set; }

                public int total_online { get; set; }
            }
        }

        public class GenericCollectionOnline : GenericCollectionViewModel<VKUserEx>
        {
            public uint _userId;

            public GenericCollectionOnline(uint userId)
            {
                this._userId = userId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUserEx>> callback)
            {
                string code = "var o=API.friends.getOnline({online_mobile:1,user_id:"+ this._userId + "});";
                code += "var users = API.users.get({ user_ids: o.online + o.online_mobile,fields: \"online,last_seen,photo_100,occupation\"});";
                code += "return { count: o.online.length + o.online_mobile.length, items: users };";

                VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUserEx>>(code, (result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        foreach (var item in result.response.items)
                        {
                            item.OptionsVisibility = Visibility.Visible;
                            item.RequestBtnVisibility = Visibility.Collapsed;
                        }
                        
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoFriends");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                }
            }
        }

        public class GenericCollectionMutual : GenericCollectionViewModel<VKUser>
        {
            public uint _userId;

            public GenericCollectionMutual(uint userId)
            {
                this._userId = userId;
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                string code = "var mutualFriends = API.friends.getMutual({\"target_uid\": " + this._userId + "});";
                code += "var users = API.users.get({ user_ids: mutualFriends,fields: \"city,country,photo_100,occupation,online\"});";
                code += "return { count: mutualFriends.length, items: users };";
                /*
                string code = "var i=0;";

                code += "var r0 = API.friends.getRequests({ offset: 0,count: 40,need_viewed: 1});";
                code += "var count=r0.count;";
                code += "var rUsers = API.users.get({ user_ids: r0.items, fields: \"photo_100\"});";
                code += "i = rUsers.length - 1;";
                code += "r0 =[];r0.items=[];";

                code += "while (i >= 0)";
                code += "{";
                code +=     "var user = rUsers[i];";
                code +=     "if (user.deactivated.length == null)";//getMutual выдаёт ошибки если юзер не активный
                code +=     "{";
                code +=         "r0.items.push(user);";
                code +=     "}";
                code +=     "i=i-1;";
                code += "}";

                code += "var m=API.friends.getMutual({ target_uids: r0.items@.id});";
                code += "var users=[];";

                code += "var i=m@.common_friends.length;";
                code += "while(i>=0)";
                code += "{";
                code +=     "var arr = m@.common_friends[i];";
                code +=     "if (arr.length > 0)";
                code +=     "{";
                code +=         "var users2 = API.users.get({user_ids:arr, fields:\"photo_100\"});";
                code +=         "users.push(users2[0]);";
                code +=     "}";
                code +=     "i=i-1;";
                code += "}";

                code += "r0.profiles = users;";
                code += "r0.count=count;";

                code += "return { m: m,r: r0};";
                */

                //VKRequestsDispatcher.Execute<MutualResponse>(code, (result =>
                VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUserEx>>(code, (result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        /*
                        foreach(var mut in result.response.m)
                        {
                            if(mut.common_count>0)
                            {
                                VKUser friend = result.response.r.items.Find((u) => u.id == mut.id);
                                friend.randomMutualFriends = new List<VKUser>();

                                foreach (var mutualId in mut.common_friends)
                                {
                                    
                                    VKUser mutualUser = result.response.r.profiles.Find((u)=>u.id == mutualId);
                                    friend.randomMutualFriends.Add(mutualUser);
                                }
                            }
                        }
                        */
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoFriends");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                }
            }
        }

        

        public class GenericCollectionSuggestions : GenericCollectionViewModel<VKUser>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                //Dictionary<string, string> parameters = new Dictionary<string, string>();
                //parameters["fields"] = "online,photo_100,verified,last_seen,occupation";
                //parameters["offset"] = offset.ToString();
                //parameters["count"] = count.ToString();

                //VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("friends.getSuggestions", parameters,(result =>


                string code = "var r = API.friends.getSuggestions({offset:"+ offset+ ",count:"+ count+",fields:\"online,photo_100,verified,last_seen,occupation\"});";
                code += "var m = API.friends.getMutual({ target_uids: r.items@.id});";

                code += "var ids=[];";
                code += "var i=m@.common_friends.length-1;";

                code += "while (i >= 0)";
                code += "{";
                code +=     "var arr=m@.common_friends[i];";
                code +=     "if (arr.length>0)";
                code +=     "{";
                code +=         "var j = arr.length;";
                code +=         "while (j>=0)";
                code +=         "{";
                code +=             "var id = arr[j];";
                code +=             "if (id > 0 && ids.indexOf(id) == -1)";
                code +=             "{";
                code +=                 "ids.push(arr[j]);";
                code +=             "}";
                code +=             "j=j-1;";
                code +=         "}";
                code +=     "}";
                code +=     "i=i-1;";
                code += "}";
                code += "var users = API.users.get({user_ids:ids,fields:\"photo_50\"});";
                code += "r.profiles = users;";
                code += "return {m:m,r:r};";

                VKRequestsDispatcher.Execute<MutualResponse>(code, (result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.r.count;

                        foreach (var mut in result.response.m)
                        {
                            if (mut.common_count > 0)
                            {
                                VKUserEx friend = result.response.r.items.Find((u) => u.id == mut.id);
                                friend.OptionsVisibility = Visibility.Collapsed;

                                friend.randomMutualFriends = new List<VKUser>();

                                foreach (var mutualId in mut.common_friends)
                                {

                                    VKUser mutualUser = result.response.r.profiles.Find((u) => u.id == mutualId);
                                    friend.randomMutualFriends.Add(mutualUser);
                                }
                            }
                        }

                        callback(result.error, result.response.r.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }));
            }

            public override string GetFooterTextForCount
            {
                get
                {
                    if (base._totalCount <= 0)
                        return LocalizedStrings.GetString("NoFriends");
                    return UIStringFormatterHelper.FormatNumberOfSomething((int)base._totalCount, "OneFriendFrm", "TwoFourFriendsFrm", "FiveFriendsFrm");
                }
            }
        }

        public class GenericCollectionRequests : GenericCollectionViewModel<VKUserEx>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUserEx>> callback)
            {
                string code = "var r = API.friends.getRequests({ need_viewed:1, need_mutual:1,fields:\"city,country,photo_100,occupation,online\", count: " + count + ", extended:1, offset:" + offset + "});";//при extended появляется поле с сообщением

                code += "var arr = r.items@.mutual@.users; var arr2 =[];";
                code += "var temp = arr.length;";
                code += "while (temp >= 0)";
                code += "{";
                code +=     "temp = temp - 1;";
                code +=     "if (arr[temp] != null) { if (arr2.indexOf(arr[temp]) == -1) { arr2 = arr2 + arr[temp]; } }";
                code += "}";
                code += "var users = API.users.get({ user_ids: arr2, fields: \"city,country,photo_100,occupation\"});";
                code += "r.profiles = r.profiles + users;";
                code += "return r;";

                VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUserEx>>(code, (result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;


                        foreach (var item in result.response.items)
                        {
                            //VKUser user = result.response.profiles.First((u) => u.id == item.id);
                            //VKUserEx userEx = new VKUserEx();
                            //user.CopyTo(userEx);

                            //user.message = item.message;

                            if (item.mutual != null)
                            {
                                item.randomMutualFriends = new List<VKUser>();
                                foreach (var m in item.mutual.users)
                                {
                                    VKUser mutual = result.response.profiles.First((u) => u.id == m);
                                    item.randomMutualFriends.Add(mutual);
                                }
                            }

                            item.OptionsVisibility = Visibility.Collapsed;
                            item.RequestBtnVisibility = Visibility.Visible;
                        }



                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }), (json)=> {

                    return json.Replace("user_id", "id");
                });
            }

            public void AddFriend(VKUserEx user)
            {
                UsersService.Instance.FriendAccept(user.id, false, (res) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (res == true)
                        {
                            base._totalCount--;
                            base.Items.Remove(user);
                            base.NotifyPropertyChanged(nameof(base.FooterText));
                        }
                    });
                });
            }

            public void DeleteFriend(VKUserEx user)
            {
                UsersService.Instance.FriendDelete(user.id, (res) =>
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        if (res == true)
                        {
                            base._totalCount--;
                            base.Items.Remove(user);
                            base.NotifyPropertyChanged(nameof(base.FooterText));
                        }
                    });
                });
            }
        }

        public class GenericCollectionRequestsOut : GenericCollectionViewModel<VKUser>
        {
            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUser>> callback)
            {
                string code = "var r = API.friends.getRequests({ need_viewed: 1,need_mutual:1,out:1, fields:\"city,country,photo_100,occupation,online\", count: " + count + ", extended: 1,offset:" + offset + "});";//при extended появляется поле с сообщением
                //code += "var users = API.users.get({ user_ids: mutualFriends,fields: \"city,country,photo_100,occupation,online\"});";
                code += "return r;";

                VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUser>>(code, (result =>
                {
                    if (result.error.error_code == VKErrors.None)
                    {
                        base._totalCount = result.response.count;
                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                }), (json) => {
                    return json.Replace("user_id", "id");
                });
            }
        }
        
        public void EditFriend(VKUserEx user, int id)
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

            VKRequestsDispatcher.DispatchRequestToVK<int>("friends.edit", parameters,(result)=> {
                if(result.error.error_code== VKErrors.None && result.response==1)
                {
                    user.lists = new List<int>(ids);
                    user.list_names = new List<string>();

                    foreach (var list in user.lists)
                    {
                        var new_list = this.AllFriendsVM.Lists.FirstOrDefault((l) => l.id == list);
                        if (new_list != null)
                            user.list_names.Add(new_list.name);
                    }

                    Execute.ExecuteOnUIThread(() =>
                    { 
                        user.UpdateUI();
                    });
                }

            });
        }











        public class FriendsSearchViewModel : GenericCollectionViewModel<VKUserEx>
        {
            public string q = string.Empty;
            private uint _userId;

            public FriendsSearchViewModel(uint userId)
            {
                this._userId = userId;
            }

            public Visibility RequestsBlockVisibility
            {
                get
                {
                    return Visibility.Collapsed;
                }
            }

            public override void GetData(int offset, int count, Action<VKError, IReadOnlyList<VKUserEx>> callback)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["user_id"] = this._userId.ToString();
                parameters["q"] = this.q;
                parameters["count"] = "30";
                parameters["fields"] = "online,photo_100,verified,last_seen,occupation";

                VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUserEx>>("friends.search", parameters,(result)=>
                {

                    if (result.error.error_code == VKErrors.None)
                    {

                        base._totalCount = result.response.count;

                        foreach (var user in result.response.items)
                        {
                            user.OptionsVisibility = Visibility.Visible;
                            user.RequestBtnVisibility = Visibility.Collapsed;
                            /*
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

                            }*/
                        }

                        callback(result.error, result.response.items);
                    }
                    else
                    {
                        callback(result.error, null);
                    }
                });
            }
        }

        public class VKUserEx : VKUser, INotifyPropertyChanged, ISupportGroup
        {
            /// <summary>
            /// Приветственное сообщение при добавлении в друзья
            /// </summary>
            public string message { get; set; }

            /// <summary>
            /// Названия списков
            /// </summary>
            public List<string> list_names { get; set; }

            public Mutual mutual { get; set; }

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


            public string Key
            {
                get { return base.first_name[0].ToString(); }
            }

            public class Mutual
            {
                public int count { get; set; }
                public List<int> users { get; set; }
            }
        }

        public class MutualResponse
        {
            public List<Mut> m { get; set; }
            public VKCountedItemsObject<VKUserEx> r { get; set; }
        }

        public class Mut
        {
            /// <summary>
            /// ИД Друга
            /// </summary>
            public uint id { get; set; }

            /// <summary>
            /// ИДы общих друзей
            /// </summary>
            public List<uint> common_friends { get; set; }


            public uint common_count { get; set; }
        }
    }
}
