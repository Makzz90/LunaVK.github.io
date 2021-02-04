using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    public class PrivacyInfo
    {
        public PrivacyTypeClass PrivacyType { get; set; }

        public List<long> DeniedUsers { get; set; }

        public List<long> DeniedLists { get; set; }

        public List<long> AllowedUsers { get; set; }

        public List<long> AllowedLists { get; set; }
        /*
        public PrivacyInfo(List<string> inputStrings)
        {
            this.DeniedUsers = new List<long>();
            this.AllowedUsers = new List<long>();
            this.DeniedLists = new List<long>();
            this.AllowedLists = new List<long>();
            this.Parse(inputStrings);
        }
        */
        public PrivacyInfo(string privacyStr = "all")
        {
            this.DeniedUsers = new List<long>();
            this.AllowedUsers = new List<long>();
            this.DeniedLists = new List<long>();
            this.AllowedLists = new List<long>();
            this.Parse((privacyStr.Split(',')).ToList<string>());
        }

        public void CleanupAllowedDeniedArraysBasedOnPrivacyType()
        {
            switch (this.PrivacyType)
            {
                case PrivacyTypeClass.AllUsers:
                case PrivacyTypeClass.Friends:
                case PrivacyTypeClass.FriendsOfFriends:
                case PrivacyTypeClass.FriendsOfFriendsOnly:
                    this.AllowedLists.Clear();
                    this.AllowedUsers.Clear();
                    break;
                case PrivacyTypeClass.OnlyMe:
                case PrivacyTypeClass.Nobody:
                    this.AllowedUsers.Clear();
                    this.AllowedLists.Clear();
                    this.DeniedUsers.Clear();
                    this.DeniedLists.Clear();
                    break;
                case PrivacyTypeClass.CertainUsers:
                    if (this.AllowedLists.Any<long>())
                        break;
                    this.DeniedUsers.Clear();
                    this.DeniedLists.Clear();
                    break;
            }
        }

        public List<string> ToStringList()
        {
            return ((IEnumerable<string>)this.ToString().Split(',')).ToList();
        }

        public override string ToString()
        {
            string str = "";
            switch (this.PrivacyType)
            {
                case PrivacyTypeClass.AllUsers:
                    str = "all";
                    break;
                case PrivacyTypeClass.Friends:
                    str = "friends";
                    break;
                case PrivacyTypeClass.FriendsOfFriends:
                    str = "friends_of_friends";
                    break;
                case PrivacyTypeClass.OnlyMe:
                    str = "only_me";
                    break;
                case PrivacyTypeClass.FriendsOfFriendsOnly:
                    str = "friends_of_friends_only";
                    break;
                case PrivacyTypeClass.Nobody:
                    str = "nobody";
                    break;
            }
            List<string> list = this.AllowedUsers.Select((uid => uid.ToString())).Union(this.DeniedUsers.Select((duid => "-" + duid.ToString()))).Union(this.AllowedLists.Select((alid => "list" + alid))).Union(this.DeniedLists.Select((dlid => "-list" + dlid))).ToList();
            if (list.Count > 0 && str != "")
                str += ",";
            return str + list.GetCommaSeparated(",");
        }

        private void Parse(List<string> inputStrings)
        {
            if (inputStrings.Count == 0)
                inputStrings.Add("all");
            this.PrivacyType = PrivacyTypeClass.CertainUsers;
            foreach (string inputString in inputStrings)
            {
                if (inputString == "all")
                    this.PrivacyType = PrivacyTypeClass.AllUsers;
                else if (inputString == "friends")
                    this.PrivacyType = PrivacyTypeClass.Friends;
                else if (inputString == "friends_of_friends")
                    this.PrivacyType = PrivacyTypeClass.FriendsOfFriends;
                else if (inputString == "friends_of_friends_only")
                    this.PrivacyType = PrivacyTypeClass.FriendsOfFriendsOnly;
                else if (inputString == "only_me")
                {
                    this.PrivacyType = PrivacyTypeClass.OnlyMe;
                }
                else
                {
                    if (inputString == "nobody")
                        this.PrivacyType = PrivacyTypeClass.Nobody;
                    if (inputString.StartsWith("list"))
                        this.AllowedLists.Add(long.Parse(inputString.Substring(4)));
                    else if (inputString.StartsWith("-list"))
                        this.DeniedLists.Add(long.Parse(inputString.Substring(5)));
                    else if (inputString.StartsWith("-"))
                    {
                        this.DeniedUsers.Add(long.Parse(inputString.Substring(1)));
                    }
                    else
                    {
                        long result = 0;
                        if (long.TryParse(inputString, out result))
                            this.AllowedUsers.Add(result);
                    }
                }
            }
        }

        public enum PrivacyTypeClass
        {
            AllUsers,
            Friends,
            FriendsOfFriends,
            OnlyMe,
            CertainUsers,
            FriendsOfFriendsOnly,
            Nobody,
        }

        public string UserFrendlyStr
        {
            get
            {
                string str = "";
                switch (this.PrivacyType)
                {
                    case PrivacyTypeClass.AllUsers:
                        str = "Privacy_AllUsers/Content";
                        break;
                    case PrivacyTypeClass.Friends:
                        str = "Privacy_Friends/Content";
                        break;
                    case PrivacyTypeClass.FriendsOfFriends:
                        str = "Privacy_FriendsOfFriends/Content";
                        break;
                    case PrivacyTypeClass.OnlyMe:
                        str = "Privacy_OnlyMe/Content";
                        break;
                    //case PrivacyTypeClass.FriendsOfFriendsOnly:
                    //    str = "friends_of_friends_only";
                    //    break;
                    case PrivacyTypeClass.Nobody:
                        str = "Privacy_Nobody/Content";
                        break;
                }
                return LocalizedStrings.GetString(str);
            }
        }
    }
}
