using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;

namespace LunaVK.Core.Library
{
    public class FavoritesService
    {
        private static FavoritesService _instance;
        public static FavoritesService Instance
        {
            get
            {
                if (FavoritesService._instance == null)
                    FavoritesService._instance = new FavoritesService();
                return FavoritesService._instance;
            }
        }

        public void GetFavePhotos(int offset, int count, Action<VKResponse< VKCountedItemsObject<VKPhoto>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPhoto>>("fave.getPhotos", parameters, callback);
        }

        public void GetLikedVideos(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKVideoBase>>("fave.getVideos", parameters, (result)=> {
                if(result.error.error_code == VKErrors.None)
                {
                    foreach (var item in result.response.items)
                    {
                        if (item.owner_id < 0 && result.response.groups != null)
                            item.Owner = result.response.groups.Find(ow => ow.id == (-item.owner_id));
                        else
                            item.Owner = result.response.profiles.Find(ow => ow.id == item.owner_id);
                    }
                }
                callback(result);
            });
        }

        public void GetFaveVideos(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKVideoBase>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            parameters["item_type"] = "video";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FaveObjects>>("fave.get", parameters, (result) => {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKVideoBase> o = new VKCountedItemsObject<VKVideoBase>();
                    o.count = result.response.count;
                    o.items = new List<VKVideoBase>();

                    foreach (var item in result.response.items)
                    {
                        if (item.type == "video")
                        {
                            VKVideoBase video = item.video;
                            if (video.owner_id < 0 && result.response.groups != null)
                                video.Owner = result.response.groups.Find(ow => ow.id == (-video.owner_id));
                            else
                                video.Owner = result.response.profiles.Find(ow => ow.id == video.owner_id);

                            o.items.Add(video);
                        }

                    }

                    callback(new VKResponse<VKCountedItemsObject<VKVideoBase>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKVideoBase>>() { error = result.error });
                }
            });
        }


        public void GetFavePosts(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKWallPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            parameters["fields"] = "photo_100";
            parameters["item_type"] = "post";
            
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FaveObjects>>("fave.get", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKWallPost> o = new VKCountedItemsObject<VKWallPost>();
                    o.count = result.response.count;
                    o.items = new List<VKWallPost>();

                    foreach (var item in result.response.items)
                    {
                        if(item.type == "post")
                        {
                            VKWallPost p = item.post;

                            VKBaseDataForGroupOrUser owner = null;

                            if (p.from_id < 0 && result.response.groups != null)
                                owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                            else
                                owner = result.response.profiles.Find(ow => ow.id == p.from_id);

                            p.Owner = owner;

                            if (p.copy_history != null)
                            {
                                for (int i = 0; i < p.copy_history.Count; i++)
                                {
                                    VKWallPost h = p.copy_history[i];
                                    if (h.owner_id < 0 && result.response.groups != null)
                                        h.Owner = result.response.groups.Find(ow => ow.id == (-h.owner_id));
                                    else
                                        h.Owner = result.response.profiles.Find(ow => ow.id == h.owner_id);

                                    h.IsRepost = true;
                                    h.IsFooterHiden = true;

                                    if (p.attachments == null)
                                        p.attachments = new List<VKAttachment>();
                                    p.attachments.Add(new VKAttachment() { wall = h, type = VKAttachmentType.Wall });
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

                            if(p.attachments!=null && p.attachments.Count>0)
                            {
                                foreach(var att in p.attachments)
                                {
                                    if(att.type== VKAttachmentType.Event)
                                    {
                                        att.@event.Owner = result.response.groups.Find(ow => ow.id == att.@event.id);
                                    }
                                }
                            }

                            o.items.Add(p);
                        }
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKWallPost>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKWallPost>>() { error = result.error });
                }
            });
        }

        public void GetLikedPosts(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKWallPost>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";

            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKWallPost>>("fave.getPosts", parameters, (result)=>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    foreach (var item in result.response.items)
                    {
                        VKWallPost p = item;

                        VKBaseDataForGroupOrUser owner = null;

                        if (p.from_id < 0 && result.response.groups != null)
                            owner = result.response.groups.Find(ow => ow.id == (-p.from_id));
                        else
                            owner = result.response.profiles.Find(ow => ow.id == p.from_id);

                        p.Owner = owner;

                        if (p.copy_history != null)
                        {
                            //for (int i = 0; i < p.copy_history.Count; i++)
                            foreach (var h in p.copy_history)
                            {
                                //VKWallPost h = p.copy_history[i];
                                if (h.owner_id < 0 && result.response.groups != null)
                                    h.Owner = result.response.groups.Find(ow => ow.id == (-h.owner_id));
                                else
                                    h.Owner = result.response.profiles.Find(ow => ow.id == h.owner_id);
                                h.IsRepost = true;
                                h.IsFooterHiden = true;

                                if (p.attachments == null)
                                    p.attachments = new List<VKAttachment>();
                                p.attachments.Add(new VKAttachment() { wall = h, type = VKAttachmentType.Wall });
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

        public void GetFaveLinks(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKLink>>> callback)
        {
            /*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKLink>>("fave.getLinks", parameters, callback);//в старом варианте нет изображений
            */
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["type"] = "link";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FaveObjects>>("fave.get", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKLink> o = new VKCountedItemsObject<VKLink>();
                    o.count = result.response.count;
                    o.items = new List<VKLink>();

                    foreach (var item in result.response.items)
                    {
                        if (item.type == "link")
                        {
                            o.items.Add(item.link);
                        }
                    }

                    if(offset==0 && o.items.Count < result.response.count)
                    {
                        o.count = (uint)o.items.Count;
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKLink>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKLink>>() { error = result.error });
                }
            });
        }

        public void GetFaveProducts(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            /*
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMarketItem>>("fave.getMarketItems", parameters, callback);
            */
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["item_type"] = "product";
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FavePage>>("fave.get", parameters, (result)=>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKMarketItem> o = new VKCountedItemsObject<VKMarketItem>();
                    o.count = result.response.count;
                    o.items = new List<VKMarketItem>();

                    foreach (var item in result.response.items)
                    {
                        if (item.type == "product")
                        {
                            o.items.Add(item.product);
                        }
                    }

                    if (offset == 0 && result.response.items.Count < result.response.count)
                    {
                        o.count = (uint)result.response.items.Count;
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKMarketItem>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKMarketItem>>() { error = result.error });
                }
            });
        }

        public void GetLikedProducts(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKMarketItem>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["extended"] = "1";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKMarketItem>>("fave.getMarketItems", parameters, callback);
        }

        public void GetFavePodcasts(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKPodcast>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<VKPodcast>>("fave.getPodcasts", parameters, callback);
        }

        /// <summary>
        /// Возвращает страницы пользователей и сообществ, добавленных в закладки. 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        public void GetFaveUsers(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKUser>>> callback)
        {
            //string code = "var us = API.fave.getUsers({offset:" + offset + ", count:"+ count+"});  var users = API.users.get({user_ids: us.items@.id, fields: \"online, online_mobile, photo_50\"}); if (users) {return {items:users,count:us.count};} return null;";
            //VKRequestsDispatcher.Execute<VKCountedItemsObject<VKUser>>(code,callback);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(offset>0)
                parameters["offset"] = offset.ToString();
            if(count>0)
                parameters["count"] = count.ToString();
            parameters["type"] = "users";
            parameters["fields"] = "online,online_mobile,photo_100,occupation,city,country,verified";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FavePage>>("fave.getPages", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKUser> o = new VKCountedItemsObject<VKUser>();
                    o.count = result.response.count;
                    o.items = new List<VKUser>();
                    
                    foreach (var item in result.response.items)
                    {
                        if (item.type == "user")
                        {
                            item.user.activity = item.description;
                            o.items.Add(item.user);
                        }
                    }

                    if (offset == 0 && result.response.items.Count< result.response.count)
                    {
                        o.count = (uint)result.response.items.Count;
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKUser>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKUser>>() { error = result.error });
                }
            });
        }

        public void GetFaveGroups(int offset, int count, uint tagId, Action<VKResponse<VKCountedItemsObject<VKGroup>>> callback)
        {
            //string code = "return API.fave.getPages({offset:" + offset + ", count:"+ count+ ", type:\"groups\",fields: \"photo_50,photo_100,members_count,trending\"});";
            //VKRequestsDispatcher.Execute<VKCountedItemsObject<FavePage>>(code, (result)=>
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(offset>0)
                parameters["offset"] = offset.ToString();
            if(tagId>0)
                parameters["tag_id"] = tagId.ToString();
            parameters["count"] = count.ToString();
            parameters["type"] = "groups";
            parameters["fields"] = "photo_50,photo_100,members_count,trending,verified";
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FavePage>>("fave.getPages", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKGroup> o = new VKCountedItemsObject<VKGroup>();
                    o.count = result.response.count;
                    o.items = new List<VKGroup>();

                    foreach (var item in result.response.items)
                    {
                        if (item.type == "group")
                        {
                            item.group.activity = item.description;
                            o.items.Add(item.group);
                        }
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKGroup>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKGroup>>() { error = result.error });
                }
            });
        }

        public void FaveAddRemoveGroup(uint groupId, bool add, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["group_id"] = groupId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(add ? "fave.addGroup" : "fave.removeGroup", parameters, callback);
        }

        public void FaveAddRemoveUser(uint userId, bool add, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["user_id"] = userId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>(add ? "fave.addUser" : "fave.removeUser", parameters, callback);
        }

        public void GetFaveTags(Action<VKResponse<VKCountedItemsObject<FaveTag>>> callback)
        {
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FaveTag>>("fave.getTags", new Dictionary<string, string>(), callback);
        }

        public void AddArticle(string url, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["url"] = url;
            VKRequestsDispatcher.DispatchRequestToVK<int>("fave.addArticle", parameters, callback);
        }

        public void RemoveArticle(int ownerId, uint articleId, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["article_id"] = articleId.ToString();
            VKRequestsDispatcher.DispatchRequestToVK<int>("fave.removeArticle", parameters, callback);
        }

        public void AddRemovePost(int ownerId, uint id, bool add, Action<VKResponse<int>> callback, string access_key = "", string _ref = "", string track_code = "", string source = "")
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["id"] = id.ToString();
            if(!string.IsNullOrEmpty(access_key))
                parameters["access_key"] = access_key;
            if (!string.IsNullOrEmpty(_ref))
                parameters["ref"] = _ref;
            if (!string.IsNullOrEmpty(track_code))
                parameters["track_code"] = track_code;
            if (!string.IsNullOrEmpty(source))
                parameters["source"] = source;
            VKRequestsDispatcher.DispatchRequestToVK<int>(add ? "fave.addPost" : "fave.removePost", parameters, callback);
        }

        public void FaveAddRemoveVideo(int ownerId, uint videoId, string acceseKey, bool add, Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["owner_id"] = ownerId.ToString();
            parameters["id"] = videoId.ToString();
            if(add && !string.IsNullOrEmpty(acceseKey))
                parameters["access_key"] = acceseKey;
            VKRequestsDispatcher.DispatchRequestToVK<int>(add ? "fave.addVideo" : "fave.removeVideo", parameters, callback);
        }

        public void GetFaveArticle(int offset, int count, Action<VKResponse<VKCountedItemsObject<VKArticle>>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["offset"] = offset.ToString();
            parameters["count"] = count.ToString();
            parameters["type"] = "article";
            //is_from_snackbar
            VKRequestsDispatcher.DispatchRequestToVK<VKCountedItemsObject<FaveObjects>>("fave.get", parameters, (result) =>
            {
                if (result.error.error_code == VKErrors.None)
                {
                    VKCountedItemsObject<VKArticle> o = new VKCountedItemsObject<VKArticle>();
                    o.count = result.response.count;
                    o.items = new List<VKArticle>();

                    foreach (var item in result.response.items)
                    {
                        if (item.type == "article")
                        {
                            o.items.Add(item.article);
                        }
                    }

                    callback(new VKResponse<VKCountedItemsObject<VKArticle>>() { error = result.error, response = o });
                }
                else
                {
                    callback(new VKResponse<VKCountedItemsObject<VKArticle>>() { error = result.error });
                }
            });
        }

        public void MarkSeen(Action<VKResponse<int>> callback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            VKRequestsDispatcher.DispatchRequestToVK<int>("fave.markSeen", parameters, callback);
        }

        public class FavePage
        {
            public string type { get; set; }

            public string description { get; set; }

            List<FaveTag> tags { get; set; }

            public VKGroup group { get; set; }

            public VKUser user { get; set; }

            public VKMarketItem product { get; set; }

            public int updated_date { get; set; }
        }

        public class FaveTag
        {
            /// <summary>
            /// Идентификатор метки
            /// </summary>
            public uint id { get; set; }

            /// <summary>
            /// Название метки
            /// </summary>
            public string name { get; set; }
        }

        public class FaveObjects
        {
            /// <summary>
            /// дата добавления объекта в закладки в формате Unixtime
            /// </summary>
            public int added_date { get; set; }

            /// <summary>
            /// является ли закладка просмотренной
            /// </summary>
            public bool seen { get; set; }

            /// <summary>
            /// тип объекта, добавленного в закладки. Возможные значения: 
            /// post — запись на стене; 
            /// video — видеозапись; 
            /// product — товар; 
            /// article — статья; 
            /// link — ссылки.
            /// </summary>
            public string type { get; set; }

            public VKArticle article { get; set; }

            public VKLink link { get; set; }

            public VKWallPost post { get; set; }

            public VKVideoBase video { get; set; }

            /// <summary>
            /// массив меток объекта в списке закладок
            /// </summary>
            public List<FaveTag> tags { get; set; }
        }
    }
}
