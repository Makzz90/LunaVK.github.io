using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class LikesService
    {
        private static LikesService _instance = new LikesService();

        public static LikesService Instance
        {
            get { return LikesService._instance; }
        }
        
        class TempLikes
        {
            public int likes { get; set; }
        }

        public void AddRemoveLike(bool add, int owner_id, uint item_id, LikeObjectType objectType, Action<int> callback, string accessKey = "")
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = owner_id.ToString();
            parameters["item_id"] = item_id.ToString();
            parameters["type"] = objectType.ToString();
            if (!string.IsNullOrEmpty(accessKey))
                parameters["access_key"] = accessKey;
            
            VKRequestsDispatcher.DispatchRequestToVK<TempLikes>(add ? "likes.add" : "likes.delete", parameters,(result)=> {
                if (result.error.error_code == VKErrors.None)
                    callback(result.response.likes);
                else
                    callback(-1);
            });
        }

        public void GetLikesList(LikeObjectType type, int owner_id, uint item_id, int count, int offset, bool onlyCopies, bool onlyFriends, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            //string str = string.Format("var count = {0};\r\nvar offset= {1};\r\nvar owner_id={2};\r\n
            //var item_id= {3};\r\nvar type = \"{4}\";\r\n\r\n\r\n
            //var likesAll = API.likes.getList({{\"type\":type,\r\n\"owner_id\": owner_id, \"item_id\":item_id, \"count\":count, \"offset\":offset, \"filter\":\"{5}\", \"friends_only\":{6} }});
            //\r\n\r\nvar userOrGroupIds = likesAll.items;\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\n
            //var i = 0;\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n   
            //var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});\r\nvar groups = API.groups.getById({{\"group_ids\":groupIds}});\r\n\r\nreturn {{\"AllCount\": likesAll.count, \"All\":users, \"AllGroups\":groups, \"AllIds\" : userOrGroupIds}};", count, offset, owner_id, item_id, type.ToString(), (onlyCopies ? "copies" : "likes"), (onlyFriends ? 1 : 0));
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = type.ToString();
            parameters["owner_id"] = owner_id.ToString();
            parameters["item_id"] = item_id.ToString();
            parameters["count"] = count.ToString();
            parameters["offset"] = offset.ToString();
            parameters["filter"] = onlyCopies ? "copies" : "likes";
            parameters["friends_only"] = onlyFriends ? "1" : "0";
            parameters["extended"] = "1";
            parameters["fields"] = "photo_50";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("likes.getList", parameters, callback,(json)=> {
                return json;
            });
        }
    }
}
