using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.Linq;
using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Enums;

namespace LunaVK.ViewModels
{
    public class EditPrivacyViewModel : ViewModelBase
    {
        //private string _key;
        //private string _privacyQuestion;
//        private Action<string> _saveCallback;
//        private PrivacySetting.PrivacyTypeClass _privacyType;
        private List<string> _supportedValues = new List<string>();
        public ObservableCollection<FriendHeader> AllowedDeniedCollection { get; set; }
        public PrivacySettingItem Setting { get; private set; }
        public bool IsOKState;
        
        

        public EditPrivacyViewModel(PrivacySettingItem vm/*, Action<string> saveCallback*/)
        {
            this.AllowedDeniedCollection = new ObservableCollection<FriendHeader>();
            this.Setting = vm;

            this._supportedValues = vm.supported_categories;

//            this._privacyType = vm.PrivacyType;
 //           this._saveCallback = saveCallback;
            
            //
            //
            //
            this.SupportedCategories = this.Setting.supported_categories.Select((c) => SettingsPrivacyViewModel.supported_categories.Find((sc) => sc.value == c)).ToList();
            this.Category = SettingsPrivacyViewModel.supported_categories.Find((s) => s.value == vm.value.category);

            this.GenerateAllowedDenied();
        }

        public List<PrivacySettingsInfo.ValueTitle> SupportedCategories { get; private set; }

        private PrivacySettingsInfo.ValueTitle _category;
        public PrivacySettingsInfo.ValueTitle Category
        {
            get
            {
                return this._category;
            }
            set
            {
                this._category = value;
                base.NotifyPropertyChanged();
                base.NotifyPropertyChanged(nameof(this.CertainUsersVisibility));
                base.NotifyPropertyChanged(nameof(this.CustomTitle));
                if(this.IsCachedDataReady && value != null)
                    this.LoadAllowedDenied();
                this.Save();
            }
        }

        public void Save()
        {
            if (this.IsOKState)
            {
                string str = this.Category.value;

                List<int> AllowedUsers = new List<int>();
                List<int> DeniedUsers = new List<int>();
                List<int> AllowedLists = new List<int>();
                List<int> DeniedLists = new List<int>();

                if (this.Setting.value.owners != null )
                {
                    if(this.Setting.value.owners.allowed != null)
                    {
                        AllowedUsers = this.Setting.value.owners.allowed;
                    }
                    if (this.Setting.value.owners.excluded != null)
                    {
                        DeniedUsers = this.Setting.value.owners.excluded;
                    }
                }
                if (this.Setting.value.lists != null )
                {
                    if (this.Setting.value.lists.allowed != null)
                    {
                        AllowedLists = this.Setting.value.lists.allowed;
                    }
                    if (this.Setting.value.lists.excluded != null)
                    {
                        DeniedLists = this.Setting.value.lists.excluded;
                    }
                }
                
                
                
                List<string> list = AllowedUsers.Select((uid => uid.ToString())).Union(DeniedUsers.Select((duid => "-" + duid.ToString()))).Union(AllowedLists.Select((alid => "list" + alid))).Union(DeniedLists.Select((dlid => "-list" + dlid))).ToList();
                
                if (list.Count > 0 && str != "")
                    str += ",";
                string temp = str + list.GetCommaSeparated(",");

                //this._saveCallback(temp);
                this.UpdatePrivacy(this.Setting.key, temp);
            }
        }

        public void UpdatePrivacy(string key, string value)
        {
            base.SetInProgress(true);
            AccountService.Instance.SetPrivacy(key, value, (result) =>
            {
                
                Execute.ExecuteOnUIThread(() =>
                {
                    base.SetInProgress(false);
                    if (result.error.error_code == VKErrors.None)
                    {
                        this.Parse(value.Split(',').ToList());
                        this.Setting.RefreshUI();
                        //VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
                        return;
                    }
                    //vm.ReadFromPrivacyInfo(pi);
                });
            });
        }

        private void Parse(List<string> inputStrings)
        {
            this.Setting.value.owners = null;
            this.Setting.value.lists = null;
            if (inputStrings.Count == 0)
                inputStrings.Add("all");
            //this.PrivacyType = PrivacyType.CertainUsers;
            foreach (string inputString in inputStrings)
            {
                if (inputString == "all" || inputString == "friends" || inputString == "friends_of_friends" || inputString == "friends_of_friends_only" || inputString == "only_me")
                    this.Setting.value.category = inputString;
                else
                {
                    if (inputString == "nobody")
                        this.Setting.value.category = inputString;
                    if (inputString.StartsWith("list"))
                    {
                        if (this.Setting.value.lists == null)
                        {
                            this.Setting.value.lists = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                            this.Setting.value.lists.allowed = new List<int>();
                        }

                        this.Setting.value.lists.allowed.Add(int.Parse(inputString.Substring(4)));
                    }
                    else if (inputString.StartsWith("-list"))
                    {
                        if (this.Setting.value.lists == null)
                        {
                            this.Setting.value.lists = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                            this.Setting.value.lists.excluded = new List<int>();
                        }

                        this.Setting.value.lists.excluded.Add(int.Parse(inputString.Substring(5)));
                    }
                    else if (inputString.StartsWith("-"))
                    {
                        if (this.Setting.value.owners == null)
                        {
                            this.Setting.value.owners = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                            this.Setting.value.owners.excluded = new List<int>();
                        }

                        this.Setting.value.owners.excluded.Add(int.Parse(inputString.Substring(1)));
                    }
                    else
                    {
                        int result = 0;
                        if (int.TryParse(inputString, out result))
                        {
                            if (this.Setting.value.owners == null)
                            {
                                this.Setting.value.owners = new PrivacySetting.PrivacyTypeClass2.PrivacyTypeClassOwners();
                                this.Setting.value.owners.allowed = new List<int>();
                            }

                            this.Setting.value.owners.allowed.Add(result);
                        }
                    }
                }
            }
        }
        /*
       public PrivacyInfo GetAsPrivacyInfo()
       {
           PrivacyInfo privacyInfo = this.ReadCurrentStateIntoPrivacyInfo();
           privacyInfo.CleanupAllowedDeniedArraysBasedOnPrivacyType();
           return privacyInfo;
       }

       private PrivacyInfo ReadCurrentStateIntoPrivacyInfo()
       {
           PrivacyInfo privacyInfo = new PrivacyInfo(this._inputPrivacyInfo.ToString());
           privacyInfo.PrivacyType = this.PrivacyType;
           Group<FriendHeader> source1 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 0));
           if (source1 != null)
           {
               privacyInfo.AllowedUsers = source1.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriend)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.UserId)).ToList<long>();
               privacyInfo.AllowedLists = source1.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriendList)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.FriendListId)).ToList<long>();
           }
           Group<FriendHeader> source2 = this._allowedDeniedCollection.FirstOrDefault<Group<FriendHeader>>((Func<Group<FriendHeader>, bool>)(g => g.Id == 1));
           if (source2 != null)
           {
               privacyInfo.DeniedUsers = source2.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriend)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.UserId)).ToList<long>();
               privacyInfo.DeniedLists = source2.Where<FriendHeader>((Func<FriendHeader, bool>)(fh => fh.IsFriendList)).Select<FriendHeader, long>((Func<FriendHeader, long>)(fh => fh.FriendListId)).ToList<long>();
           }
           return privacyInfo;
       }
       */
        public Visibility CheckSupported(string privacyTypeStr)
        {
            if ((this._supportedValues.Count == 0 ? true : (this._supportedValues.Contains(privacyTypeStr) ? true : false)) == false)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public string PrivacyQuestionLowerCase
        {
            get
            {
                //if (this._privacyQuestion == null)
                //    return "";
                //return this._privacyQuestion.ToLowerInvariant();
                return this.Setting.title;
            }
        }
        
        public Visibility CertainUsersVisibility
        {
            get
            {
                if(this.Category==null)
                    return Visibility.Visible;

                string val = this.Category.value;
                if (val== "only_me")
                    return Visibility.Collapsed;

                return this.CheckSupported("some");
            }
        }

        public string CustomTitle
        {
            get
            {
                if(this.Category!=null && this.Category.value== "some")
                    return LocalizedStrings.GetString("Privacy_AllowedTo");
                return LocalizedStrings.GetString("Privacy_DeniedTo");
            }
        }


        private static bool _isLoadingCache;
        private bool IsCachedDataReady
        {
            get
            {
                return EditPrivacyViewModel._friendsAndListsCached != null;
            }
        }

        private void GenerateAllowedDenied()
        {
            if (!this.IsCachedDataReady)
                this.PrepareCache();
            else
                this.LoadAllowedDenied();
        }

        private void LoadAllowedDenied()
        {
            this.AllowedDeniedCollection.Clear();

            if (this.Setting.value.owners != null)
            {

                //PrivacyType.CertainUsers
                //all friends friends_of_friends friends_of_friends_only nobody only_me list{list_id} {user_id} -list{list_id} -{user_id} 
                if (this.Category.value == "some")
                {
                    if (this.Setting.value.owners.allowed != null)
                    {
                        foreach (var friendId in this.Setting.value.owners.allowed)
                        {
                            var bannedFriend = EditPrivacyViewModel._friendsAndListsCached.friends.FirstOrDefault((f) => f.id == friendId);

                            if (bannedFriend != null)
                            {
                                FriendHeader header = new FriendHeader();
                                header.data = bannedFriend;
                                this.AllowedDeniedCollection.Add(header);
                            }
                        }
                    }

                }
                else
                {
                    if (this.Setting.value.owners.excluded != null)
                    {
                        foreach (var friendId in this.Setting.value.owners.excluded)
                        {
                            var bannedFriend = EditPrivacyViewModel._friendsAndListsCached.friends.FirstOrDefault((f) => f.id == friendId);

                            if (bannedFriend != null)
                            {
                                FriendHeader header = new FriendHeader();
                                header.data = bannedFriend;
                                this.AllowedDeniedCollection.Add(header);
                            }
                        }
                    }
                }
            }

            if (this.Setting.value.lists != null)
            {
                if (this.Category.value == "some")
                {
                    if (this.Setting.value.lists.allowed != null)
                    {
                        foreach (var listId in this.Setting.value.lists.allowed)
                        {
                            var list = EditPrivacyViewModel._friendsAndListsCached.friendLists.FirstOrDefault((l) => l.id == listId);

                            if (list != null)
                            {
                                FriendHeader header = new FriendHeader();
                                header._friendsList = list;
                                this.AllowedDeniedCollection.Add(header);
                            }
                        }
                    }
                }
                else
                {
                    if (this.Setting.value.lists.excluded != null)
                    {
                        foreach (var listId in this.Setting.value.lists.excluded)
                        {
                            var list = EditPrivacyViewModel._friendsAndListsCached.friendLists.FirstOrDefault((l) => l.id == listId);

                            if (list != null)
                            {
                                FriendHeader header = new FriendHeader();
                                header._friendsList = list;
                                this.AllowedDeniedCollection.Add(header);
                            }
                        }
                    }
                }
                    
                
            }
        }

        private static FriendsAndLists _friendsAndListsCached;

        private void PrepareCache()
        {
            if (EditPrivacyViewModel._isLoadingCache)
                return;
            EditPrivacyViewModel._isLoadingCache = true;
            /*
            UsersService.Instance.GetFriendsAndLists((Action<BackendResult<FriendsAndLists, ResultCode>>)(res =>
            {
                bool flag = res.ResultCode == ResultCode.Succeeded;
                if (flag)
                {
                    EditPrivacyViewModel._friendsAndListsCached = res.ResultData;
                    EditPrivacyViewModel._friendsAndListsCachedOwnerId = AppGlobalStateManager.Current.LoggedInUserId;
                }
                //EventAggregator.Current.Publish(new FinishedLoadingFriendsAndListsCacheEvent()
                //{
                //    Success = flag
                //});
                EditPrivacyViewModel._isLoadingCache = false;
            }));*/

            base.SetInProgress(true);
            string code = "var friends=API.friends.get({ order:\"hints\", fields:\"first_name,last_name,online,online_mobile,photo_50,first_name_gen,last_name_gen\"}).items; var friendLists = API.friends.getLists({return_system:1}).items;  return {friends:friends, friendLists:friendLists};";

            VKRequestsDispatcher.Execute<FriendsAndLists>(code,(result)=>
            {
                base.SetInProgress(false);
                if (result.error.error_code == VKErrors.None)
                {
                    EditPrivacyViewModel._friendsAndListsCached = result.response;
                    //EditPrivacyViewModel._friendsAndListsCachedOwnerId = AppGlobalStateManager.Current.LoggedInUserId;


                    Execute.ExecuteOnUIThread(() => {
                        this.LoadAllowedDenied();
                    });
                }
                else
                {

                }

            EditPrivacyViewModel._isLoadingCache = false;
            });
        }

        public FriendsAndLists LookFor(string str)
        {
            FriendsAndLists ret = new FriendsAndLists();
            if(EditPrivacyViewModel._friendsAndListsCached.friends!=null)
            {
                var temp = EditPrivacyViewModel._friendsAndListsCached.friends.FindAll((u) => u.Title.StartsWith(str, StringComparison.CurrentCultureIgnoreCase));
                if (this.AllowedDeniedCollection.Count > 0)
                {
                    foreach (var f in temp)
                    {
                        var found = this.AllowedDeniedCollection.FirstOrDefault((a) => a.data != null && a.data.Id == f.Id);
                        if (found != null)
                            continue;

                        if (ret.friends == null)
                            ret.friends = new List<VKUser>();

                        ret.friends.Add(f);

                    }
                }
                else
                {
                    ret.friends = temp;
                }
            }

            if (EditPrivacyViewModel._friendsAndListsCached.friendLists != null)
            {
                var temp = EditPrivacyViewModel._friendsAndListsCached.friendLists.FindAll((u) => u.name.StartsWith(str, StringComparison.CurrentCultureIgnoreCase));
                if (this.AllowedDeniedCollection.Count > 0)
                {
                    foreach (var f in temp)
                    {
                        var found = this.AllowedDeniedCollection.FirstOrDefault((a) => a._friendsList != null && a._friendsList.id == f.id);
                        if (found != null)
                            continue;

                        if (ret.friendLists == null)
                            ret.friendLists = new List<FriendsList>();

                        ret.friendLists.Add(f);

                    }
                }
                else
                {
                    ret.friendLists = temp;
                }
            }

            return ret;
        }

        public class FriendsAndLists
        {
            public List<VKUser> friends { get; set; }

            public List<FriendsList> friendLists { get; set; }//public List<FriendsList> friendLists { get; set; }
        }

        public class FriendHeader
        {
            public static List<List<string>> _frListColors = new List<List<string>>() { new List<string>() { "c8e6c9", "73ba77" }, new List<string>() { "b6dedb", "50b3a9" }, new List<string>() { "fceab1", "e6c153" }, new List<string>() { "e6dad5", "ba9d93" }, new List<string>() { "ffccbc", "f09073" }, new List<string>() { "c4e5f5", "69b7db" }, new List<string>() { "c8cce6", "8b95cc" }, new List<string>() { "e3c8e8", "be8bc7" } };

            public VKBaseDataForGroupOrUser data;

            public FriendsList _friendsList;

            public Visibility IsListVisibility
            {
                get
                {
                    if (this._friendsList == null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public Visibility IsNotListVisibility
            {
                get
                {
                    if (this._friendsList != null)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            public SolidColorBrush PlaceholderFill
            {
                get
                {
                    if (this._friendsList == null)
                        return Application.Current.Resources["PhoneAccentColorBrush"] as SolidColorBrush;
                    int num = Math.Abs(this._friendsList.id);
                    return ("#FF" + FriendHeader._frListColors[(num % 8)][0]).GetColor();
                }
            }

            public string ImageUrl
            {
                get
                {
                    if (this.data != null)
                        return this.data.MinPhoto;
                    if (this._friendsList == null)
                        return "";
                    return null;//return MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/PlaceholderGroup62Light.png", true, "");
                }
            }

            public SolidColorBrush FriendListBackground
            {
                get
                {
                    if (this._friendsList == null)
                        return new SolidColorBrush(Colors.Transparent);
                    int num = Math.Abs(this._friendsList.id);
                    return ("#FF" + FriendHeader._frListColors[(num % 8)][1]).GetColor();
                }
            }

            public string FullName
            {
                get
                {
                    if (this.data != null)
                        return this.data.Title;
                    if (this._friendsList == null)
                        return "";
                    return this._friendsList.name;
                }
            }

            public Visibility DeleteVisibility { get; set; }
        }
    }
}
