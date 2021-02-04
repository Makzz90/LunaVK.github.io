using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core;
using System.Linq;

namespace LunaVK.Core.Library
{
    public class FriendsCache
    {
        private const string Friends = "Friends";

        private static FriendsCache _instance;
        public static FriendsCache Instance
        {
            get
            {
                if (FriendsCache._instance == null)
                    FriendsCache._instance = new FriendsCache();
                return FriendsCache._instance;
            }
        }

        public /*async Task<IReadOnlyList<VKUser>>*/IReadOnlyList<VKUser> GetFriends()
        {
            var temp = /*await*/ CacheManager2.ReadTextFromFile<IReadOnlyList<VKUser>>(Friends);
            if (temp == null)
                return null;//нет файла в кеше

            List<VKUser> result = new List<VKUser>();
            foreach(var user in temp)
            {
                if(result.FirstOrDefault((u) => user.id == u.id)==null)
                    result.Add(user);
            }
            return result;
        }

        public void SetFriends(IReadOnlyList<VKUser> friends)
        {
            
            List<VKUser> filtered = new List<VKUser>();

            foreach(var friend in friends)
            {
                if (filtered.FirstOrDefault((f) => f.id == friend.id)==null)
                {
                    filtered.Add(friend);
                }
            }
            
            CacheManager2.WriteTextToFile<IReadOnlyList<VKUser>>(Friends, filtered);
        }
    }
}
