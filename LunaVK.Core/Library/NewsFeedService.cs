using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class NewsFeedService
    {
        private static NewsFeedService _instance;
        public static NewsFeedService Instance
        {
            get { return NewsFeedService._instance ?? (NewsFeedService._instance = new NewsFeedService()); }
        }

        /// <summary>
        /// Позволяет скрыть объект из ленты новостей. 
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="type"></param>
        /// <param name="ownerId"></param>
        /// <param name="itemId">wall,tag,profilephoto ,video,photo,audio</param>
        /// <param name="callback"></param>
        public void IgnoreUnignoreItem(bool ignore, string type, int ownerId, uint itemId, Action<bool> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = type;
            parameters["owner_id"] = ownerId.ToString();
            parameters["item_id"] = itemId.ToString();

            VKRequestsDispatcher.DispatchRequestToVK<int>(ignore ? "newsfeed.ignoreItem" : "newsfeed.unignoreItem", parameters, (result)=> {
                if (result.error.error_code != Enums.VKErrors.None)
                    callback(false);
                else
                    callback(result.response == 1);
            });
        }

        /// <summary>
        /// Запрещает/разрешает показывать новости от заданных пользователей и групп в ленте новостей текущего пользователя.
        /// </summary>
        /// <param name="addBan"></param>
        /// <param name="uids"></param>
        /// <param name="gids"></param>
        /// <param name="callback"></param>
        public void AddDeleteBan(bool addBan, List<uint> uids, List<uint> gids, Action<bool> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (uids != null && uids.Count > 0)
                parameters["user_ids"] = uids.GetCommaSeparated();
            if (gids != null && gids.Count > 0)
                parameters["group_ids"] = gids.GetCommaSeparated();

            VKRequestsDispatcher.DispatchRequestToVK<int>(addBan ? "newsfeed.addBan" : "newsfeed.deleteBan", parameters, (result)=> {
                if (result.error.error_code != Enums.VKErrors.None)
                    callback(false);
                else
                    callback(result.response == 1);
            });
        }

        public void GetNotifications(/*int startTime, int endTime,*/ int offset, string fromStr, int count, Action<VKResponse<NotificationData>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //if (startTime > 0)
            //    parameters["start_time"] = startTime.ToString();
            //if (endTime > 0)
            //    parameters["end_time"] = endTime.ToString();
            if (count > 0)
                parameters["count"] = count.ToString();
            //if (offset > 0)
            //    parameters["offset"] = offset.ToString();
            if (!string.IsNullOrWhiteSpace(fromStr))
                parameters["start_from"] = fromStr;
            parameters["fields"] = "sex,photo_50,photo_100,online,screen_name,first_name_dat,last_name_dat,first_name_gen,last_name_gen";
            Dictionary<string, string> dictionary = parameters;
            dictionary["fields"] = dictionary["fields"] + ",is_closed,type,is_admin,is_member,photo_200";
            VKRequestsDispatcher.DispatchRequestToVK<NotificationData>("notifications.get", parameters, (result)=> {
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    foreach (VKNotification item in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (item.ParsedFeedback is List<FeedbackUser> list)
                        {
                            if (list[0].from_id > 0)
                            {
                                owner = result.response.profiles.Find((u) => u.id == list[0].from_id);
                            }
                            else
                            {
                                owner = result.response.groups.Find((g) => g.id == (-list[0].from_id));
                            }
                        }
                        else if (item.ParsedFeedback is VKComment comment)
                        {
                            if (comment.from_id > 0)
                            {
                                owner = result.response.profiles.Find((u) => u.id == comment.from_id);
                            }
                            else
                            {
                                owner = result.response.groups.Find((g) => g.id == (-comment.from_id));
                            }
                        }
                        else if (item.ParsedFeedback is VKWallPost post)
                        {
                            if (post.from_id > 0)
                            {
                                owner = result.response.profiles.Find((u) => u.id == post.from_id);
                            }
                            else
                            {
                                owner = result.response.groups.Find((g) => g.id == (-post.from_id));
                            }
                        }
                        else if (item.ParsedFeedback is List<FeedbackCopyInfo> info)
                        {
                            var post2 = info[0];
                            if (post2.from_id > 0)
                            {
                                owner = result.response.profiles.Find((u) => u.id == post2.from_id);
                            }
                            else
                            {
                                owner = result.response.groups.Find((g) => g.id == (-post2.from_id));
                            }
                        }
                        
                        item.Owner = owner;
                    }

                    
                }

                callback(result);
            }, (jsonStr =>
            {
                int resultCount = 0;
                jsonStr = VKRequestsDispatcher.GetArrayCountAndRemove(jsonStr, "items", out resultCount);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "profiles", false);
                jsonStr = VKRequestsDispatcher.FixFalseArray(jsonStr, "groups", false);
                //NotificationData response = JsonConvert.DeserializeObject<GenericRoot<NotificationData>>(jsonStr).response;
                //response.TotalCount = resultCount;
                //List<Notification> notificationList = new List<Notification>();
                //foreach (var notification in response.items)
                //{
                //    notification.UpdateNotificationType();
                //    object parsedFeedback = notification.ParsedFeedback;
                //    object parsedParent = notification.ParsedParent;
                //    if (notification.NotType == NotificationType.unknown)
                //        notificationList.Add(notification);
                //}
                //foreach (Notification notification in notificationList)
                //    response.items.Remove(notification);
                return jsonStr;
            }));
        }

        public void MarkAsViewed()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            VKRequestsDispatcher.DispatchRequestToVK<int>("notifications.markAsViewed", parameters, null);
        }

        /// <summary>
        /// Возвращает результаты поиска по статусам. Новости возвращаются в порядке от более новых к более старым. 
        /// </summary>
        /// <param name="searchStr">поисковой запрос</param>
        /// <param name="count">какое максимальное число записей следует возвращать</param>
        /// <param name="startTime">время в формате unixtime, начиная с которого следует получить новости для текущего пользователя</param>
        /// <param name="endTime">время в формате unixtime, до которого следует получить новости для текущего пользователя</param>
        /// <param name="startFrom">Идентификатор, необходимый для получения следующей страницы результатов</param>
        /// <param name="callback"></param>
        public void Search(string searchStr, int count, int startTime, int endTime, string startFrom, Action<VKResponse< VKCountedItemsObject<VKNewsfeedPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["q"] = searchStr;
            parameters["count"] = count.ToString();
            if (startTime > 0)
                parameters["start_time"] = startTime.ToString();
            if (endTime > 0)
                parameters["end_time"] = endTime.ToString();
            if (!string.IsNullOrWhiteSpace(startFrom))
                parameters["start_from"] = startFrom.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKNewsfeedPost>>("newsfeed.search", parameters, (result)=>
            {
                if(result.error.error_code == Enums.VKErrors.None)
                {
                    List<VKGroup> groups = result.response.groups;
                    List<VKUser> profiles = result.response.profiles;

                    foreach (var p in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (p.source_id == 0)
                        {
                            if (p.owner_id < 0)
                                owner = groups.Find(ow => ow.id == (-p.owner_id));
                            else
                                owner = profiles.Find(ow => ow.id == p.owner_id);
                        }
                        else
                        {
                            if (p.source_id < 0 && groups != null)
                                owner = groups.Find(ow => ow.id == (-p.source_id));
                            else
                                owner = profiles.Find(ow => ow.id == p.source_id);
                        }

                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            for (int i = 0; i < p.copy_history.Count; i++)
                            {
                                VKNewsfeedPost j = p.copy_history[i];
                                if (j.owner_id < 0 && groups != null)
                                    j.Owner = groups.Find(ow => ow.id == (-j.owner_id));
                                else
                                    j.Owner = profiles.Find(ow => ow.id == j.owner_id);
                            }
                        }
                    }
                }

                callback(result);
            });
        }

        public void GetNewsComments(int startTime, int endTime, int count, string fromStr, Action<VKResponse<VKCountedItemsObject<VKNewsfeedPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["last_comments_count"] = "3";
            parameters["allow_group_comments"] = "1";
            //parameters["filters"] = "video";
            if (startTime > 0 )
                parameters["start_time"] = startTime.ToString();
            if (endTime > 0)
                parameters["end_time"] = endTime.ToString();
            if (count > 0)
                parameters["count"] = count.ToString();
            if (!string.IsNullOrWhiteSpace(fromStr))
                parameters["start_from"] = fromStr;
            parameters["fields"] = "sex,photo_50,photo_100";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKNewsfeedPost>>("newsfeed.getComments", parameters, (result) =>
            {
                if (result.error.error_code == Enums.VKErrors.None)
                {
                    List<VKGroup> groups = result.response.groups;
                    List<VKUser> profiles = result.response.profiles;

                    foreach (var p in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (p.source_id == 0)
                        {
                            if (p.owner_id < 0)
                                owner = groups.Find(ow => ow.id == (-p.owner_id));
                            else
                                owner = profiles.Find(ow => ow.id == p.owner_id);
                        }
                        else
                        {
                            if (p.source_id < 0 && groups != null)
                                owner = groups.Find(ow => ow.id == (-p.source_id));
                            else
                                owner = profiles.Find(ow => ow.id == p.source_id);
                        }

                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            for (int i = 0; i < p.copy_history.Count; i++)
                            {
                                VKNewsfeedPost j = p.copy_history[i];
                                if (j.owner_id < 0 && groups != null)
                                    j.Owner = groups.Find(ow => ow.id == (-j.owner_id));
                                else
                                    j.Owner = profiles.Find(ow => ow.id == j.owner_id);
                            }
                        }

                        if(p.comments!=null)
                        {
                            foreach(var comment in p.comments.list)
                            {
                                VKBaseDataForGroupOrUser ownerC = null;
                                if (comment.from_id != 0)
                                {
                                    if (comment.from_id < 0)
                                        ownerC = groups.Find((pro) => pro.id == (-comment.from_id));
                                    else
                                        ownerC = profiles.Find((pro) => pro.id == (comment.from_id));
                                }
                                else
                                {
                                    if (comment.owner_id < 0 && groups != null)
                                        ownerC = groups.Find(ow => ow.id == (-comment.owner_id));
                                    else
                                        ownerC = profiles.Find(ow => ow.id == comment.owner_id);
                                }
                                comment.User = ownerC;
                            }
                        }

                        p.IsFooterHiden = true;
                    }
                }

                callback(result);
            }, (jsonString) => {
                jsonString = jsonString.Replace("\"views\":", "\"views_\":");//тупо, у если пост это видео, то ломаются просмотры
                return jsonString;
            });
        }

        public void GetBanned(Action<VKResponse< ProfilesAndGroups>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["extended"] = "1";
            parameters["fields"] = "online,online_mobile,photo_100,activity,verified,members_count";
            VKRequestsDispatcher.DispatchRequestToVK<ProfilesAndGroups>("newsfeed.getBanned", parameters, callback);
        }

        public void DeleteBan(List<uint> uids, List<uint> gids, Action<bool> callback)
        {
            this.AddDeleteBan(false, uids, gids, callback);
        }

        public void GetSuggestedSources(int offset, int count, bool shuffle, Action<VKResponse<VKCountedItemsObject<VKUserOrGroupSource>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string index1 = "offset";
            string str1 = offset.ToString();
            parameters[index1] = str1;
            string index2 = "count";
            string str2 = count.ToString();
            parameters[index2] = str2;
            string index3 = "shuffle";
            string str3 = shuffle ? "1" : "0";
            parameters[index3] = str3;
            string index4 = "fields";
            string str4 = "is_member,activity,is_closed,photo_200,photo_max,verified,friends_status,occupation,city,country";
            parameters[index4] = str4;
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUserOrGroupSource>>("newsfeed.getSuggestedSources", parameters, callback);
        }
        /*
        public void GetNewsFeed(NewsFeedGetParams parameters, Action<BackendResult<NewsFeedData, ResultCode>> callback)
        {
            string methodName = "execute.getNewsfeed";
            Dictionary<string, string> paramDict = new Dictionary<string, string>()
      {
        {
          "count",
          parameters.count.ToString()
        },
        {
          "device_info",
          new AdvertisingDeviceInfo().ToJsonString()
        }
      };
            paramDict["fields"] = "sex,online,photo_50,photo_100,photo_200,is_friend";
            if (!string.IsNullOrEmpty(parameters.from))
                paramDict["start_from"] = parameters.from;
            bool? nullable;
            if (parameters.NewsListId == -100L)
            {
                paramDict["filters"] = "video";
                paramDict["grouping"] = "100";
                paramDict["extended"] = "1";
            }
            else if (parameters.photoFeed || parameters.NewsListId == -101L)
            {
                paramDict["filters"] = "photo,photo_tag,wall_photo";
                paramDict["max_photos"] = "10";
                if (parameters.source_ids.NotNullAndHasAtLeastOneNonNullElement())
                {
                    paramDict["source_ids"] = parameters.source_ids.GetCommaSeparated();
                }
                else
                {
                    paramDict["source_ids"] = "friends,following";
                    paramDict["return_banned"] = "1";
                }
            }
            else
            {
                paramDict["filters"] = "post,photo,photo_tag";
                long num1 = parameters.NewsListId - -10L;
                long num2 = 3;
                if ((ulong)num1 <= (ulong)num2)
                {
                    switch ((uint)num1)
                    {
                        case 0:
                            paramDict["is_newsfeed"] = "1";
                            Dictionary<string, string> dictionary1 = paramDict;
                            dictionary1["filters"] = dictionary1["filters"] + ",friends_recomm,ads_post";
                            nullable = AppGlobalStateManager.Current.GlobalState.AdsDemoManualSetting;
                            bool flag = true;
                            if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                            {
                                Dictionary<string, string> dictionary2 = paramDict;
                                dictionary2["filters"] = dictionary2["filters"] + ",ads_demo";
                                goto label_16;
                            }
                            else
                                goto label_16;
                        case 1:
                            paramDict["recommended"] = "1";
                            goto label_16;
                        case 2:
                            paramDict["source_ids"] = "friends,following";
                            goto label_16;
                        case 3:
                            paramDict["source_ids"] = "groups,pages";
                            goto label_16;
                    }
                }
                paramDict["source_ids"] = "list" + parameters.NewsListId;
            }
        label_16:
            NewsFeedType? feedType = parameters.FeedType;
            if (feedType.HasValue)
            {
                paramDict["feed_type"] = feedType.Value.ToString();
                if (parameters.UpdateFeedType)
                    paramDict["set_feed_type"] = "1";
            }
            if (parameters.SyncNotifications)
                paramDict["sync_notifications"] = "1";
            nullable = parameters.TopFeedPromoAnswer;
            if (nullable.HasValue)
            {
                Dictionary<string, string> dictionary = paramDict;
                string index = "top_feed_promo_accepted";
                nullable = parameters.TopFeedPromoAnswer;
                string str = nullable.Value ? "1" : "0";
                dictionary[index] = str;
                paramDict["top_feed_promo_id"] = parameters.TopFeedPromoId.ToString();
            }
            ConnectionType connectionType = NetworkStatusInfo.Instance.RetrieveNetworkConnectionType();
            if (connectionType != null)
            {
                paramDict["connection_type"] = connectionType.Type;
                paramDict["connection_subtype"] = connectionType.Subtype;
            }
            if (parameters.UpdateAwayTime > 0)
                paramDict["update_away_time"] = parameters.UpdateAwayTime.ToString();
            if (parameters.UpdatePosition > -1)
                paramDict["update_position"] = parameters.UpdatePosition.ToString();
            if (!string.IsNullOrEmpty(parameters.UpdatePost))
                paramDict["update_post"] = parameters.UpdatePost;
            NewsFeedService.DoLoadNewsFeed(methodName, paramDict, callback);
        }
        */
        public class NotificationData
        {
            /// <summary>
            /// массив оповещений для текущего пользователя. 
            /// </summary>
            public List<VKNotification> items { get; set; }

            /// <summary>
            /// информация о пользователях, которые находятся в списке оповещений. 
            /// </summary>
            public List<VKUser> profiles { get; set; }

            /// <summary>
            /// информация о сообществах, которые находятся в списке оповещений. 
            /// </summary>
            public List<VKGroup> groups { get; set; }

            /// <summary>
            /// время последнего просмотра пользователем раздела оповещений в формате Unixtime. 
            /// </summary>
            //public int last_viewed { get; set; }

            public string next_from { get; set; }
        }

        public class ProfilesAndGroups
        {
            public List<VKGroup> groups { get; set; }

            public List<VKUser> profiles { get; set; }
        }
    }
}
