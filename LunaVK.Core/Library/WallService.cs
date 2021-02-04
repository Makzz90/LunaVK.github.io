using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Json;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class WallService
    {
        private static WallService _instance;
        public static WallService Instance
        {
            get
            {
                if (WallService._instance == null)
                    WallService._instance = new WallService();
                return WallService._instance;
            }
        }

        /// <summary>
        /// Копирует объект на стену пользователя или сообщества
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="obj_id"></param>
        /// <param name="message">сопроводительный текст, который будет добавлен к записи с объектом</param>
        /// <param name="obj"></param>
        /// <param name="gid">идентификатор сообщества, на стене которого будет размещена запись с объектом. Если не указан, запись будет размещена на стене текущего пользователя</param>
        /// <param name="callback"></param>
        public void Repost(int ownerId, uint obj_id, string message, RepostObject obj, uint gid, Action<VKResponse<RepostResult>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string str = obj.ToString() + ownerId + "_" + obj_id;
            parameters["object"] = str;
            if (!string.IsNullOrEmpty(message))
                parameters["message"] = message;

            if (gid != 0)
                parameters["group_id"] = gid.ToString();
            //mark_as_ads

            VKRequestsDispatcher.DispatchRequestToVK<RepostResult>("wall.repost", parameters, callback);
        }

        public void SendStats(List<VKWallPost> viewedWallPosts)
        {
            List<string> ids1 = new List<string>();
            List<string> ids2 = new List<string>();

            foreach (VKWallPost viewedWallPost in viewedWallPosts)
            {
                ids1.Add(/*viewedWallPost.to_id +*/ "_" + viewedWallPost.id);
                if (viewedWallPost.copy_history != null )
                {
                    foreach (VKWallPost wallPost in viewedWallPost.copy_history)
                    {
                        ids2.Add(wallPost.owner_id + "_" + wallPost.id);
                    }
                }
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["post_ids"] = ids1.GetCommaSeparated(",");
            parameters["repost_ids"] = ids2.GetCommaSeparated(",");
            VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("stats.viewPosts", parameters, null, null, true, null);
        }

        
       public void GetWallPostByIdWithComments( int ownerId, uint postId, int offset, int commentId, bool needWallPost, Action<VKResponse<GetWallPostResponseData>> callback, LikeObjectType likeObjType = LikeObjectType.post)
       {
            /*
           if (knownTotalCount != -1 && offset + countToRead > knownTotalCount)
               countToRead = knownTotalCount - offset;
           string str1 = (!needWallPost ? "var likesAll = API.likes.getList({{\"item_id\":{0}, \"owner_id\":{1}, \"count\":20, type:\"{8}\"}});" : " var wallPost = API.wall.getById({{\"posts\":\"{1}_{0}\"}});\r\nvar likesAll = API.likes.getList({{\"item_id\":{0}, \"owner_id\":{1}, \"count\":20, type:wallPost[0].post_type}});" + Environment.NewLine) + "\r\n\r\n\r\n\r\nvar offset = {2};\r\n\r\n\r\nvar comments = API.wall.getComments({{\"post_id\":\"{0}\", \"owner_id\":\"{1}\", \"offset\":offset, \"count\":\"{3}\", \"need_likes\":\"1\", \"sort\":\"desc\", \"preview_length\":\"0\", \"allow_group_comments\":1}});\r\n\r\nvar datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;\r\nvar users2 = API.users.get({{\"user_ids\":datUsersNames, \"fields\":\"first_name_dat,last_name_dat\"}});\r\n\r\n\r\n\r\nvar userOrGroupIds = likesAll.items;\r\n";
           if (needWallPost)
               str1 += "userOrGroupIds = userOrGroupIds + wallPost@.from_id + wallPost@.to_id + wallPost@.signer_id + wallPost[0].copy_history@.owner_id + wallPost[0].copy_history@.from_id;\r\n";
           string str2 = str1 + "userOrGroupIds = userOrGroupIds + comments.items@.from_id;\r\n\r\n\r\nvar userIds = [];\r\nvar groupIds = [];\r\n\r\nvar i = 0;\r\n\r\nvar length = userOrGroupIds.length;\r\n\r\nwhile (i < length)\r\n{{\r\n    var id = parseInt(userOrGroupIds[i]);\r\n    \r\n    if (id > 0)\r\n    {{\r\n       if (userIds.length > 0)\r\n       {{\r\n          userIds = userIds + \",\";\r\n       }}\r\n       userIds = userIds + id;\r\n    }}\r\n    else if (id < 0)\r\n    {{\r\n        id = -id;\r\n        if (groupIds.length > 0)\r\n        {{\r\n            groupIds = groupIds + \",\";\r\n        }}\r\n        groupIds = groupIds + id;\r\n    }}\r\n     \r\n    i = i + 1;\r\n}}\r\n\r\nif ({1} < 0)\r\n{{\r\n    if (groupIds.length > 0) groupIds = groupIds + \",\";\r\n    groupIds = groupIds + ({1} * -1);\r\n}}\r\n\r\nvar users  = API.users.get({{\"user_ids\":userIds, \"fields\":\"sex,photo_max,online,online_mobile\" }});\r\nvar groups = API.groups.getById({{\"group_ids\":groupIds}});";
           string str3 = string.Format(pollId == 0L ? (!needWallPost ? str2 + "return {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"comments\": comments, \"Users2\": users2 }};" : str2 + "return {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"WallPost\":wallPost[0], \"comments\": comments, \"Users2\": users2 }};") : (!needWallPost ? str2 + "\r\nvar poll= API.polls.getById({{\"owner_id\":{7}, \"poll_id\":{6}}});   \r\nreturn {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"comments\": comments, \"Users2\": users2, \"Poll\":poll }};" : str2 + "\r\nvar poll= API.polls.getById({{\"owner_id\":{7}, \"poll_id\":{6}}});   \r\nreturn {{\"Users\": users, \"Groups\":groups, \"LikesAll\":likesAll, \"WallPost\":wallPost[0], \"comments\": comments, \"Users2\": users2, \"Poll\":poll }};"), postId, ownerId, offset, countToRead, knownTotalCount, userId, pollId, pollOwnerId, likeObjType);
           Dictionary<string, string> parameters = new Dictionary<string, string>();
           parameters["code"] = str3;
           VKRequestsDispatcher.DispatchRequestToVK<GetWallPostResponseData>("execute", parameters, callback, (jsonStr =>
           {
               VKRequestsDispatcher.GenericRoot<GetWallPostResponseData> genericRoot = JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<GetWallPostResponseData>>(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(VKRequestsDispatcher.FixFalseArray(jsonStr, "Users", false), "Users2", false), "Groups", false), "Poll", true), "LikesAll", true), "comments", true));
               if (genericRoot.response.LikesAll.items != null)
                   genericRoot.response.LikesAll.users = new List<UserLike>(genericRoot.response.LikesAll.items.Select<long, UserLike>((Func<long, UserLike>)(it => new UserLike()
                   {
                       uid = it
                   })));
               genericRoot.response.Users.Add(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
               genericRoot.response.Comments.Reverse();
               GroupsService.Instance.AddCachedGroups(genericRoot.response.Groups);
               if (countToRead == 0)
                   genericRoot.response.Comments.Clear();
               if (genericRoot.response.WallPost == null)
                   genericRoot.response.WallPost = new WallPost();
               return genericRoot.response;
           }));
           */
            string code = "if(API.wall.getById({posts:\"" + ownerId + "_" + postId + "\"}).length<1) { return null; }";
            code += ("var comments = API.wall.getComments({post_id:" + postId + ", owner_id:" + ownerId + ", offset:" + offset + ", count:20, thread_items_count:1, extended:1, need_likes:1");
            if (commentId != 0)
            {
                code += (", start_comment_id:" + commentId);
            }
            code += ", fields:\"last_name_dat,first_name_dat,photo_50\", sort:\"desc\"";
            code += "});";

            code += "var likesAll=null;var wallPost=null; var datUsersNames = comments.items@.reply_to_user + comments.items@.from_id;var users2 = API.users.get({user_ids:datUsersNames, fields:\"first_name_dat,last_name_dat\"});";
            code += "comments.profiles = comments.profiles + users2;";


            if (needWallPost)
            {
                code += "var wallPosts = API.wall.getById({posts:\"" + ownerId + "_" + postId + "\",extended:1});";
                code += "wallPost = wallPosts.items[0];";
                code += "comments.profiles = comments.profiles + wallPosts.profiles;";
                code += "comments.groups = comments.groups + wallPosts.groups;";
            }

            if (offset == 0)
            {
                code += "likesAll = API.likes.getList({item_id:" + postId + ", owner_id:" + ownerId + ", count:20, extended:1,fields:\"photo_50\", type:\"post\"});";
            }

            if (ownerId < 0)
            {
                code += "var group = API.groups.getById({group_id:" + (-ownerId) + "});";
                code += "comments.groups = comments.groups + group;";
            }


            code += "return {comments:comments,likes:likesAll,wall_post:wallPost};";

            VKRequestsDispatcher.Execute<GetWallPostResponseData>(code, (result)=> {
                if (result.error.error_code == VKErrors.None)
                {
                    if (result.response==null)//пост удалён :)
                    {
                        callback(result);
                        return;
                    }

                    if (result.response.wall_post!=null)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (result.response.wall_post.OwnerId < 0)
                            owner = result.response.comments.groups.Find((pro) => pro.id == (-result.response.wall_post.OwnerId));
                        else
                            owner = result.response.comments.profiles.Find((pro) => pro.id == (result.response.wall_post.OwnerId));
                        result.response.wall_post.Owner = owner;

                        if(result.response.wall_post.signer_id!=0)
                        {
                            if (result.response.wall_post.signer_id < 0)
                                owner = result.response.comments.groups.Find(ow => ow.id == (-result.response.wall_post.signer_id));
                            else
                                owner = result.response.comments.profiles.Find(ow => ow.id == result.response.wall_post.signer_id);
                            result.response.wall_post.Signer = owner;
                        }
                    }






                    foreach (VKComment c in result.response.comments.items)
                    {
                        if (c.reply_to_user != 0)
                        {
                            if (c.reply_to_user < 0)
                            {
                                VKGroup g = result.response.comments.groups.Find((group) => group.id == -c.reply_to_user);
                                c._replyToUserDat = g.Title;
                            }
                            else
                            {
                                VKUser r = result.response.comments.profiles.Find((user) => user.id == c.reply_to_user);
                                c._replyToUserDat = r.first_name_dat + " " + r.last_name_dat;
                            }
                        }

                        VKBaseDataForGroupOrUser owner = null;

                        if (c.from_id < 0)
                            owner = result.response.comments.groups.Find((pro) => pro.id == (-c.from_id));
                        else
                            owner = result.response.comments.profiles.Find((pro) => pro.id == (c.from_id));

                        if (owner == null)
                        {
                            owner = new VKUser() { photo_50 = "https://vk.com/images/wall/deleted_avatar_50.png", first_name = "Комментарий удалён пользователем или руководителем страницы" };
                        }

                        c.User = owner;

                        //
                        /*
                        public static bool CanDelete(this Comment comment, long ownerId)
                        {
                          if (comment == null)
                            return false;
                          GroupOrUser cachedGroup = GroupsService.Current.GetCachedGroup(-ownerId);
                          return comment.from_id == AppGlobalStateManager.Current.LoggedInUserId || ownerId == AppGlobalStateManager.Current.LoggedInUserId || ownerId < 0L && cachedGroup != null && (comment.from_id > 0L && cachedGroup.IsModeratorOrHigher==true || cachedGroup.IsEditorOrHigher);
                        }
                        */
                        var cachedGroup = result.response.comments.groups.Find((g)=>g.id == (-ownerId));
                        c.CanDelete = c.from_id == Settings.UserId || c.owner_id == Settings.UserId || c.owner_id < 0 && cachedGroup != null && (c.from_id > 0 && cachedGroup.IsModeratorOrHigher == true || cachedGroup.IsEditorOrHigher);
                        //

                        /*
                        public static bool CanEdit(this Comment comment)
    {
      if (comment == null)
        return false;
      GroupOrUser cachedGroup = GroupsService.Current.GetCachedGroup(-comment.from_id);
      if ((comment.from_id == AppGlobalStateManager.Current.LoggedInUserId || comment.from_id < 0L && cachedGroup != null && cachedGroup.IsEditorOrHigher==true) && (DateTime.Now - Extensions.UnixTimeStampToDateTime((double) comment.date, true)).TotalHours < 24.0)
      {
        if (comment.Attachments != null)
        {
          Func<Attachment, bool> func1 = (Func<Attachment, bool>) (a => a.type == "sticker");
          if (Enumerable.Any<Attachment>(comment.Attachments, func1))
            goto label_6;
        }
        return true;
      }
label_6:
      return false;
    }*/
                        c.CanEdit = (c.from_id == Settings.UserId  || c.from_id < 0 && cachedGroup != null && cachedGroup.IsEditorOrHigher) && (DateTime.Now - c.date).TotalHours < 24;
                        //

                        if (c.thread != null && c.thread.count > 0)
                        {
                            VKBaseDataForGroupOrUser buttonOwner = null;

                            var thread_c = c.thread.items[0];
                            if (thread_c.from_id < 0)
                                owner = result.response.comments.groups.Find((pro) => pro.id == (-thread_c.from_id));
                            else
                                owner = result.response.comments.profiles.Find((pro) => pro.id == (thread_c.from_id));
                            thread_c.User = owner;
                            if (buttonOwner == null)
                                buttonOwner = owner;

                            if (c.thread.count == 1)
                            {
                                
                            }
                            else
                            {
                                c.Button = new VKComment.BottomButtonData() { TotalComments = c.thread.count, User = buttonOwner };
                                c.thread.items.Clear();
                            }
                        }

                        c.Marked = commentId == c.id;


                    }

                }
                callback(result);
            });
            
       }

        public void DeleteComment(int ownerId, uint cid, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["comment_id"] = cid.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("wall.deleteComment", parameters, callback);
        }

        public void Report(int ownerId, uint id, ReportReason reportReason, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["post_id"] = id.ToString();
            parameters["reason"] = ((int)reportReason).ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("wall.reportPost", parameters, callback);
        }

        public void GetWall(int ownerId, int offset, int count, Action<VKResponse< VKCountedItemsObject<VKWallPost>>> callback, string filter = "all")
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["owner_id"] = ownerId.ToString();
            dictionary["offset"] = offset.ToString();
            dictionary["count"] = count.ToString();
            dictionary["extended"] = "1";
            dictionary["filter"] = filter;
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKWallPost>>("wall.get", dictionary, (result)=> {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (VKWallPost p in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;
                        if (p.from_id != 0)
                        {
                            if (p.from_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.from_id);
                        }
                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            for (int i = 0; i < p.copy_history.Count; i++)
                            {
                                VKWallPost item = p.copy_history[i];
                                if (item.owner_id < 0 && result.response.groups != null)
                                    item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                                else
                                    item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                            }
                        }

                        if (p.signer_id != 0)
                        {
                            if (p.signer_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.signer_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.signer_id);
                            p.Signer = owner;
                        }
                    }
                }
                callback(result);
            });
        }

        public void Post(int owner_id, string message, List<string> AttachmentIds, double? latitude, double? longitude, long? publish_date, bool PublishOnTwitter, bool PublishOnFacebook, uint post_id, bool OnBehalfOfGroup, bool Sign, bool FriendsOnly, bool CloseComments, bool MuteNotifications, Action<VKResponse<ResponseWithPostId>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //идентификатор пользователя или сообщества, на стене которого должна быть опубликована запись.
            if (owner_id != 0)
                parameters["owner_id"] = owner_id.ToString();

            //текст сообщения (является обязательным, если не задан параметр attachments)
            if (!string.IsNullOrEmpty(message))
                parameters["message"] = message;

            //список объектов, приложенных к записи и разделённых символом
            if (!AttachmentIds.IsNullOrEmpty())
                parameters["attachments"] = AttachmentIds.GetCommaSeparated();

            //географическая широта отметки, заданная в градусах (от -90 до 90).
            if (latitude.HasValue)
                parameters["lat"] = latitude.Value.ToString(CultureInfo.InvariantCulture);

            //географическая долгота отметки, заданная в градусах (от -180 до 180).
            if (longitude.HasValue)
                parameters["long"] = longitude.Value.ToString(CultureInfo.InvariantCulture);

            //дата публикации записи в формате unixtime. Если параметр указан, публикация записи будет отложена до указанного времени. 
            if (publish_date.HasValue)
                parameters["publish_date"] = publish_date.Value.ToString(CultureInfo.InvariantCulture);
            
            if (PublishOnTwitter)
                parameters["services"] = "twitter";

            //список сервисов или сайтов, на которые необходимо экспортировать запись, в случае если пользователь настроил соответствующую опцию. Например, twitter, facebook.
            if (PublishOnFacebook)
                parameters["services"] = !PublishOnTwitter ? "facebook" : "twitter,facebook";

            //идентификатор записи, которую необходимо опубликовать. Данный параметр используется для публикации отложенных записей и предложенных новостей.
            if (post_id != 0)
                parameters["post_id"] = post_id.ToString();

            //данный параметр учитывается, если owner_id < 0 (запись публикуется на стене группы). 1 — запись будет опубликована от имени группы, 0 — запись будет опубликована от имени пользователя
            parameters["from_group"] = OnBehalfOfGroup || Sign ? "1" : "0";

            //1 — у записи, размещенной от имени сообщества, будет добавлена подпись (имя пользователя, разместившего запись), 0 — подписи добавлено не будет. Параметр учитывается только при публикации на стене сообщества и указании параметра from_group. По умолчанию подпись не добавляется. 
            parameters["signed"] = Sign ? "1" : "0";

            //1 — запись будет доступна только друзьям, 0 — всем пользователям.
            if (FriendsOnly)
                parameters["friends_only"] = "1";

            if(CloseComments)
                parameters["close_comments"] = "1";

            if(MuteNotifications)
                parameters["mute_notifications"] = "1";
            //place_id идентификатор места, в котором отмечен пользователь
            //guid уникальный идентификатор, предназначенный для предотвращения повторной отправки одинаковой записи. Действует в течение одного часа. 
            //mark_as_ads 1 — у записи, размещенной от имени сообщества, будет добавлена метка "это реклама", 0 — метки добавлено не будет.
            VKRequestsDispatcher.DispatchRequestToVK<ResponseWithPostId>("wall.post", parameters, callback);
        }

        public void DeletePost(int ownerId, uint post_id, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["owner_id"] = ownerId.ToString();
            dictionary["post_id"] = post_id.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("wall.delete", dictionary, callback);
        }
        public class ResponseWithPostId
        {
            public int post_id { get; set; }
        }


        public void Edit(int owner_id, string message, List<string> AttachmentIds, double? latitude, double? longitude, long? publish_date, uint post_id, bool Sign, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (owner_id != 0)
                parameters["owner_id"] = owner_id.ToString();

            if (post_id != 0)
                parameters["post_id"] = post_id.ToString();

            if (!string.IsNullOrEmpty(message))
                parameters["message"] = message ?? "";

            parameters["signed"] = Sign ? "1" : "0";

            if (!AttachmentIds.IsNullOrEmpty())
                parameters["attachments"] = AttachmentIds.GetCommaSeparated();

            if (latitude.HasValue)
                parameters["lat"] = latitude.Value.ToString(CultureInfo.InvariantCulture);

            if (longitude.HasValue)
                parameters["long"] = longitude.Value.ToString(CultureInfo.InvariantCulture);

            if (publish_date.HasValue)
                parameters["publish_date"] = publish_date.Value.ToString(CultureInfo.InvariantCulture);

            VKRequestsDispatcher.DispatchRequestToVK<int>("wall.edit", parameters, callback);
        }

        public void EditComment(int owner_id, string message, List<string> AttachmentIds, uint comment_id, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = owner_id.ToString();
            parameters["comment_id"] = comment_id.ToString();
            parameters["message"] = message;
            if (!AttachmentIds.IsNullOrEmpty())
                parameters["attachments"] = AttachmentIds.GetCommaSeparated();
            VKRequestsDispatcher.DispatchRequestToVK<int>("wall.editComment", parameters, callback);
        }

        public void Search(string query, string domain, int ownerId, int count, int offset, Action<VKResponse<VKCountedItemsObject<VKWallPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            
            parameters["query"] = query;
            parameters["count"] = count.ToString();
            parameters["offset"] = offset.ToString();
            parameters["extended"] = "1";
            if (ownerId != 0)
                parameters["owner_id"] = ownerId.ToString();
            else if (!string.IsNullOrEmpty(domain))
                parameters["domain"] = domain;
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKWallPost>>("wall.search", parameters, (result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach (var p in result.response.items)
                    {
                        VKBaseDataForGroupOrUser owner = null;

                        if (p.from_id != 0)
                        {
                            if (p.from_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.from_id);
                        }
                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            for (int i = 0; i < p.copy_history.Count; i++)
                            {
                                VKWallPost item = p.copy_history[i];
                                if (item.owner_id < 0 && result.response.groups != null)
                                    item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                                else
                                    item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                            }
                        }

                        if (p.signer_id != 0)
                        {
                            if (p.signer_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.signer_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.signer_id);
                            p.Signer = owner;
                        }
                    }
                }

                callback(result);

            });
        }

        public void GetWallById(int ownerId, uint postId, Action<VKResponse<VKCountedItemsObject<VKWallPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["posts"] = ownerId.ToString() + "_" + postId.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKWallPost>>("wall.getById", parameters, callback);
        }

        public void GetWallSubscriptionsProfiles(int offset, int count, Action<VKResponse< VKCountedItemsObject<VKUser>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = "1";
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKUser>>("wall.getSubscriptions", parameters, callback);
            //VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUser>>(string.Format("var subscriptions = API.wall.getSubscriptions({{\"offset\":{0}, \"count\":{1}, \"type\":1}});\r\nvar users = [];\r\nif (subscriptions.items.length > 0)\r\n{{\r\n     users = API.users.get({{\"user_ids\":subscriptions.items, \"fields\":\"photo_max, online, online_mobile\"}});\r\n}}\r\nreturn {{\"items\": users, \"count\":subscriptions.count}};", offset, count), callback);
        }

        public void GetWallSubscriptionsGroups(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKGroup>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["type"] = "2";
            parameters["extended"] = "1";
            parameters["fields"] = "photo_100,activity,verified,members_count";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKGroup>>("wall.getSubscriptions", parameters, callback);
            //VKRequestsDispatcher.Execute<VKList<Group>>(string.Format("var subscriptions = API.wall.getSubscriptions({{\"offset\":{0}, \"count\":{1}, \"type\":2}});\r\nvar groups = [];\r\nif (subscriptions.items.length > 0)\r\n{{\r\n   var groupIds = [];\r\n   var i= 0;\r\n\r\n   while (i < subscriptions.items.length)\r\n   {{\r\n       groupIds.push(-subscriptions.items[i]);\r\n       i = i + 1;\r\n   }}\r\n\r\n   groups = API.groups.getById({{\"group_ids\":groupIds}});\r\n}}\r\nreturn {{\"items\": groups, \"count\":subscriptions.count}};", offset, count), callback, null, false, true, new CancellationToken?());
        }

        public void WallSubscriptionsUnsubscribe(List<int> ownerIds, Action<VKResponse<ResponseWithId>> callback)
        {
            string format = "API.wall.unsubscribe({{owner_id:{0} }});";
            string code = "";
            foreach (int ownerId in ownerIds)
                code = code + string.Format(format, ownerId) + Environment.NewLine;
            VKRequestsDispatcher.Execute<ResponseWithId>(code, callback);
        }

        public class GetWallPostResponseData
        {
            public Comments comments { get; set; }

            public VKWallPost wall_post { get; set; }

            public VKCountedItemsObject<VKUser> likes { get; set; }



            public class Comments : VKCountedItemsObject<VKComment>
            {
                public int real_offset { get; set; }

                /// <summary>
                /// количество комментариев в ветке. 
                /// </summary>
                public int current_level_count { get; set; }

                /// <summary>
                /// может ли текущий пользователь оставлять комментарии в этой ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool can_post { get; set; }

                /// <summary>
                /// нужно ли отображать кнопку «ответить» в ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool show_reply_button { get; set; }

                /// <summary>
                /// могут ли сообщества оставлять комментарии в ветке. 
                /// </summary>
                [JsonConverter(typeof(VKBooleanConverter))]
                public bool groups_can_post { get; set; }
            }
        }


        public class RepostResult
        {
            public int success { get; set; }

            /// <summary>
            /// ИД созданной записи
            /// </summary>
            public int post_id { get; set; }
            public int reposts_count { get; set; }
            public int likes_count { get; set; }

            //public int views_count { get; set; }//mod
        }

        public enum RepostObject
        {
            wall,
            photo,
            video,
            wall_ads,
            doc,
        }
    }
}
