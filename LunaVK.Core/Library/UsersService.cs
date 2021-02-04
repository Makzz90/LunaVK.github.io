using System;
using System.Collections.Generic;
using System.Text;
using LunaVK.Core.DataObjects;
using System.Threading.Tasks;
using LunaVK.Core.Network;
using LunaVK.Core.Enums;
using LunaVK.Core.Utils;
using System.Linq;

namespace LunaVK.Core.Library
{
    public class UsersService
    {
        private readonly object _lockObj = new object();

        private Dictionary<uint, VKUser> _usersCache = new Dictionary<uint, VKUser>();
        public List<VKUser> CachedUsers
        {
            get { return this._usersCache.Values.ToList(); }
        }
        
        private static UsersService _instance;
        public static UsersService Instance
        {
            get
            {
                return UsersService._instance ?? (UsersService._instance = new UsersService());
            }
        }

        public void SetCachedUsers(IEnumerable<VKUser> users)
        {
            if (users == null)
                return;

            lock (this._lockObj)
            {
                foreach (var user in users)
                {
                    //user.CachedDateTime = DateTime.Now;
                    this._usersCache[(uint)user.id] = user;
                }
            }
        }

        public VKUser GetCachedUser(uint userId)
        {
            lock (this._lockObj)
            {
                if (this._usersCache.ContainsKey(userId))
                {
                    VKUser user = this._usersCache[userId];
                    //if ((DateTime.Now - user.CachedDateTime).TotalMinutes <= (double)this.numberOfMinutesUserIsValidInCache)
                        return user;
                }
            }
            return null;
        }

        /// <summary>
        /// Получает информацию о пользователях.
        /// Добавляем в кеш
        /// </summary>
        /// <param name="userIds"></param>
        public void GetUsers(List<uint> userIds, Action<List<VKUser>> callback)
        {
            List<VKUser> cached = new List<VKUser>();
            List<uint> to_get = new List<uint>();
            foreach (var userId in userIds)
            {
                if (this._usersCache.ContainsKey(userId))
                    cached.Add(this._usersCache[userId]);
                else
                    to_get.Add(userId);
                    
            }

            if (to_get.Count == 0 && cached.Count > 0)
            {
                callback(cached);
                return;
            }

            VKRequestsDispatcher.Execute<List<VKUser>>("return API.users.get({user_ids:\""+ to_get.GetCommaSeparated() + "\", fields:\"first_name,last_name,photo_50,photo_100,online\" });",(result)=>
            {
                if(result!=null && result.error.error_code == VKErrors.None)
                {
                    this.SetCachedUsers(result.response);
                    cached.AddRange(result.response);
                    callback(cached);
                }
                else
                {
                    //BUG: если нет интернета, то зацикливается от RefreshHeader
                    callback(null);
                }
            });
        }
        
        public void GetProfileInfo(uint userId,bool justBasicData, Action<VKResponse< VKUser>> callback)
        {
            if (justBasicData)
            {
                string code = "var users = API.users.get({user_ids:" + userId + ",fields:\"photo_100,verified,activity,status,counters,can_write_private_message,friend_status,crop_photo,bdate,occupation,last_seen,sex,online,blacklisted,city,relation,can_send_friend_request,first_name_gen,is_favorite,first_name_acc\"});";
                code += "var u = users[0];";
                if (userId != Settings.UserId)
                {
                    code += "var mutualIds = API.friends.getMutual({target_uid:" + userId + ", count:8});";
                    code += "var mutualUsers = API.users.get({user_ids:mutualIds,fields:\"photo_50\"});";
                    code += "u.randomMutualFriends=mutualUsers;";
                }
                code += "if (u.occupation != null) { if(u.occupation.type==\"work\"){ var groups = API.groups.getById({ group_id: u.occupation.id}); u.occupationGroup = groups[0]; }}";

                code += "if (u.relation_partner != null)";
                code += "{";
                code += "var relationusers = API.users.get({ user_ids: u.relation_partner.id,fields: \"first_name_ins,last_name_ins,first_name_abl,last_name_abl,first_name_acc,last_name_acc\"});";

                code += "var relationuser = relationusers[0];";

                code += "u.relation_partner.acc = relationuser.first_name_acc + \" \" + relationuser.last_name_acc;";
                code += "u.relation_partner.ins = relationuser.first_name_ins + \" \" + relationuser.last_name_ins;";
                code += "u.relation_partner.abl = relationuser.first_name_abl + \" \" + relationuser.last_name_abl;";
                code += "}";

                code += "return u;";

                VKRequestsDispatcher.Execute<VKUser>(code, callback, (jsonStr) =>
                {
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });

                
            }
            else
            {
                string code = "var users = API.users.get({user_ids:" + userId + ",fields:\"home_town,personal,relatives,contacts,country,domain,site,activities,interests,music,movies,tv,books,games,quotes,about,universities,schools\"});";
                code += "var user = users[0];var relatives = API.users.get({user_ids:user.relatives@.id});";//todo:надо var id = parseInt(userOrGroupIds[i]); чтобы не было ошибок
                code += "user.relativesUsers=relatives;";
                code += "return user;";
                VKRequestsDispatcher.Execute<VKUser>(code, callback, (jsonStr) =>
                {
                    jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "relativesUsers");
                    return VKRequestsDispatcher.FixArrayToObject(jsonStr, "counters");
                });
            }            
        }

        /// <summary>
        /// Одобряет или создает заявку на добавление в друзья.
        /// </summary>
        /// <param name="userId">	идентификатор пользователя, которому необходимо отправить заявку, либо заявку от которого необходимо одобрить.</param>
        /// <param name="text">текст сопроводительного сообщения для заявки на добавление в друзья. </param>
        /// <param name="follow">1, если необходимо отклонить входящую заявку (оставить пользователя в подписчиках). </param>
        /// <param name="callback"></param>
        public void FriendAdd(uint userId, string text = "", Action<bool> callback = null)
        {
//#if DEBUG
//            await Task.Delay(1000);
//            callback(true);
//#else
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = userId.ToString();
            if (!string.IsNullOrEmpty(text))
                parameters["text"] = text;

            VKRequestsDispatcher.DispatchRequestToVK<int>("friends.add", parameters, (result)=> {
                callback(result.error.error_code == VKErrors.None ? (result.response > 0) : false);
            });
            
            /*
            * После успешного выполнения возвращает одно из следующих значений:
            1 — заявка на добавление данного пользователя в друзья отправлена;
            2 — заявка на добавление в друзья от данного пользователя одобрена;
            4 — повторная отправка заявки.
            */
//#endif
        }

        

        public void FriendAccept(uint userId, bool follow, Action<bool> callback = null)
        {
//#if DEBUG
//            await Task.Delay(1000);
//            callback(true);
//#else
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = userId.ToString();
            parameters["follow"] = follow ? "1" : "0";

            VKRequestsDispatcher.DispatchRequestToVK<int>("friends.add", parameters,(result)=> {
                callback(result.error.error_code == VKErrors.None ? (result.response > 0) : false);
            });
            /*
            * После успешного выполнения возвращает одно из следующих значений:
            1 — заявка на добавление данного пользователя в друзья отправлена;
            2 — заявка на добавление в друзья от данного пользователя одобрена;
            4 — повторная отправка заявки.
            */
            //#endif
        }

        public void FriendDelete(uint userId, Action<bool> callback = null)
        {
//#if DEBUG
//            await Task.Delay(1000);
//            callback(true);
//#else
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = userId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<FriendDeleteResult>("friends.delete", parameters, (result) =>
            {
                callback(result.error.error_code == VKErrors.None ? (result.response.success == 1) : false);
            });
//#endif
            
        }

        public void GetFriends(uint uid, uint lid, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["fields"]= "first_name,last_name,online, online_mobile,photo_max,bdate,site,contacts,domain,occupation";
            parameters["order"]= "hints";
            if (uid != 0)
                parameters["user_id"] = uid.ToString();
            if (lid != 0)
                parameters["list_id"] = lid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("friends.get", parameters, callback);
        }
        /*
        public void GetFriendsAndLists(Action<FriendsAndLists> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string str = "var friends=API.friends.get({ order:\"hints\", fields:\"first_name,last_name,online,online_mobile,photo_max,first_name_gen,last_name_gen\"}).items; var friendLists = API.friends.getLists({\"return_system\":1}).items;  return {friends:friends, friendLists:friendLists};";
            parameters["code"] = str;
            RequestsDispatcher.GetResponse<FriendsAndLists>("execute", parameters, callback);
        }

        public class FriendsAndLists
        {
            public List<GroupOrUser> friends { get; set; }

            
            public VKCountedItemsObject<VKFriendsGetObject.Lists> friendLists { get; set; }//public List<FriendsList> friendLists { get; set; }
        }
        */

        public void GetUsersForTile(uint uid, Action<VKUser> callback)
        {
            if (uid < 0)
            {
                List<uint> userIds = new List<uint>() { uid };
                VKUser user = null;
                this.GetUsers(userIds,(result)=> {
                    if(result!=null)
                    {
                        user = this.GetCachedUser(uid);
                    }
                    
                    callback(user);
                });
                
                
            }
            else
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["user_ids"] = uid.ToString();
                parameters["fields"] = "photo_200";
                parameters["name_case"] = "ins";
                VKRequestsDispatcher.DispatchRequestToVK<List<VKUser>>("users.get", parameters,(result)=> {
                    if (result != null && result.error.error_code == VKErrors.None)
                        callback(result.response[0]);
                    else
                        callback(null);
                });
            }
        }

        /// <summary>
        /// Возвращает список пользователей и страниц, которые входят в список подписок пользователя
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="callback"></param>
        public void GetSubscriptions(uint uid, Action<VKResponse<VKCountedItemsObject<VKGroup>>> callback)//VKBaseDataForGroupOrUser
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = uid.ToString();
            parameters["count"] = "30";
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGroup>>("users.getSubscriptions", parameters, callback);
        }

        public void Search(Dictionary<string, object> searchParams, string query, int offset, int count, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            if (searchParams == null)
                return;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["count"] = count.ToString();

            if (offset > 0)
                parameters["offset"] = offset.ToString();
            
            if (!string.IsNullOrWhiteSpace(query))
                parameters["q"]= query;

            if(searchParams.ContainsKey("country"))
                parameters["country"] = ((uint)searchParams["country"]).ToString();

            if (searchParams.ContainsKey("city"))
                parameters["city"] = ((uint)searchParams["city"]).ToString();

            if (searchParams.ContainsKey("age_from"))
                parameters["age_from"] = ((int)searchParams["age_from"]).ToString();

            if (searchParams.ContainsKey("age_to"))
                parameters["age_to"] = ((int)searchParams["age_to"]).ToString();

            if (searchParams.ContainsKey("sex"))
                parameters["sex"] = ((int)searchParams["sex"]).ToString();

            if (searchParams.ContainsKey("status"))
                parameters["status"] = ((int)searchParams["status"]).ToString();

            if (searchParams.ContainsKey("has_photo"))
                parameters["has_photo"] = "1";
            if (searchParams.ContainsKey("online"))
                parameters["online"] = "1";
            
            parameters.Add("fields", "photo_100,verified,occupation,city,country,friend_status");
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("users.search", parameters, callback);
        }

        public void Report(uint uid, string type, string comment, Action<bool> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = uid.ToString();
            parameters["type"] = type;//porn,spam,insult,advertisement
            parameters["comment"] = comment;
            VKRequestsDispatcher.DispatchRequestToVK<FriendDeleteResult>("users.report", parameters, (result) =>
            {
                callback(result.error.error_code == VKErrors.None ? (result.response.success == 1) : false);
            });
        }

        public void SaveProfilePhoto(UploadPhotoResponseData responseData, Action<VKResponse<VKProfilePhoto>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["server"] = responseData.server;
            parameters["photo"] = responseData.photo;
            parameters["hash"] = responseData.hash;

            VKRequestsDispatcher.DispatchRequestToVK<VKProfilePhoto>("photos.saveOwnerPhoto", parameters, callback);
        }














        public class FriendDeleteResult
        {
            /// <summary>
            /// удалось успешно удалить друга 
            /// </summary>
            public int success { get; set; }

            /// <summary>
            /// был удален друг 
            /// </summary>
            public int friend_deleted { get; set; }

            /// <summary>
            /// отменена исходящая заявка 
            /// </summary>
            public int out_request_deleted { get; set; }

            /// <summary>
            ///  отклонена входящая заявка 
            /// </summary>
            public int in_request_deleted { get; set; }

            /// <summary>
            /// отклонена рекомендация друга 
            /// </summary>
            public int suggestion_deleted { get; set; }
        }
    }
}
